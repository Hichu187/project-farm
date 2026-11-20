using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Luật chơi: gieo, tưới, thu hoạch, mua hạt, bán nông sản, mở ô. Mọi thay đổi FarmState đi qua đây.
    // Thời gian chỉ lấy từ IClock; không đếm giờ theo frame.
    public sealed partial class Farm
    {
        private readonly IClock _clock;

        // Báo sau mỗi lệnh thành công. Cây lớn theo thời gian không phát sự kiện, View tự làm mới định kỳ.
        public event Action Changed;
        // Báo cấp mới, sau khi đã trao thưởng; phát trước Changed của lệnh đó.
        public event Action<int> LeveledUp;
        // Báo id cây được tặng 1 hạt vì người chơi bị kẹt; phát trước Changed của lệnh đó.
        public event Action<string> Rescued;
        // Báo giao đơn xong (người đặt, câu cảm ơn) để View hiện bong bóng; phát trước Changed của lệnh đó.
        public event Action<OrderDelivery> OrderDelivered;

        public FarmDef Definition { get; }
        public FarmState State { get; }
        // Ngày / khung giờ / âm lịch theo giờ VN (UTC+7 cố định), không lùi ngày (Đợt 2 Phần 2).
        public GameCalendar Calendar { get; }

        public Farm(FarmDef definition, FarmState state, IClock clock)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Calendar = new GameCalendar(clock, definition.Lunar, state);

            ValidateState();
            State.GrantAvailableStartPlots(Definition);
            GrantFishingStarter();
            TryRescue();
            FillOrders();
            // Thư theo build + dọn thư lần đầu trong ngày (không báo Changed lúc nạp).
            RefreshMailInternal();
        }

        public int OrderSlotCount => State.Orders.Length;

        public bool OrdersUnlocked => Definition.Orders.SlotCount > 0 && State.Level >= Definition.Orders.UnlockLevel;

        // Chỉ số ngày (số ngày từ 1970) theo giờ VN, đổi lúc 0 giờ VN, không lùi (GameCalendar).
        public long CurrentDay => Calendar.AbsoluteDay;

        // Số giây tới 0 giờ hôm sau (giờ VN).
        public long SecondsUntilNextDay => Calendar.SecondsUntilNextDay;

        public bool HasDailyOrderLimit => Definition.Orders.DailyLimit > 0;

        // Số đơn còn giao được hôm nay (không giới hạn thì int.MaxValue). Đọc theo ngày hiện tại kể cả khi chưa làm mới.
        public int OrdersLeftToday
        {
            get
            {
                int limit = Definition.Orders.DailyLimit;
                if (limit <= 0)
                    return int.MaxValue;
                int delivered = CurrentDay > State.OrderDay ? 0 : State.DeliveredToday;
                return Math.Max(0, limit - delivered);
            }
        }

        // Còn chỗ cho đơn mới hôm nay: số đơn đã giao + đơn đang có trên bảng chưa đủ giới hạn.
        // Nhờ vậy trên bảng không bao giờ có đơn không giao được vì hết lượt.
        public bool CanReceiveOrderToday => OrdersLeftToday > ActiveOrderCount;

        // Đơn khẩn cấp đang có: số giây còn lại trước khi hết hạn; false nếu đơn không hết hạn.
        public bool TryGetOrderTimeLeft(int slot, out long seconds)
        {
            seconds = 0;
            if (!TryGetOrder(slot, out OrderState order) || !order.HasOrder || order.ExpiresAt <= 0)
                return false;
            seconds = Math.Max(0L, order.ExpiresAt - _clock.UtcNowSeconds);
            return true;
        }

        public bool TryGetOrder(int slot, out OrderState order)
        {
            if (slot < 0 || slot >= State.Orders.Length)
            {
                order = null;
                return false;
            }
            order = State.Orders[slot];
            return true;
        }

        // Đủ nông sản trong Kho (đơn cá: đủ cá thường trong giỏ) để giao đơn này.
        public bool CanDeliver(int slot)
        {
            if (!OrdersUnlocked || !TryGetOrder(slot, out OrderState order) || !order.HasOrder
                || order.IsExpired(_clock.UtcNowSeconds))
                return false;
            foreach (OrderItem item in order.Items)
            {
                if (GetOrderItemHave(order, item) < item.Count)
                    return false;
            }
            return true;
        }

        // Số đang có cho 1 món của đơn: nông sản trong Kho, hoặc cá thường (không shiny) cùng loài trong giỏ.
        public int GetOrderItemHave(OrderState order, OrderItem item)
        {
            if (order == null || item == null)
                return 0;
            return order.IsFish ? CountOrderFish(item.Id) : State.GetItems(item.Id);
        }

        public FarmResult DeliverOrder(int slot)
        {
            if (!OrdersUnlocked)
                return FarmResult.OrdersLocked;
            if (!TryGetOrder(slot, out OrderState order))
                return FarmResult.UnknownOrder;
            if (!order.HasOrder)
                return FarmResult.OrderNotReady;
            long now = _clock.UtcNowSeconds;
            if (order.IsExpired(now))
            {
                // Hết hạn từ sau lần làm mới trước: dọn ô rồi báo cho UI.
                NotifyChanged();
                return FarmResult.OrderExpired;
            }
            if (!CanDeliver(slot))
                return FarmResult.NotEnoughItems;

            foreach (OrderItem item in order.Items)
            {
                if (order.IsFish)
                    TakeOrderFish(item.Id, item.Count);
                else
                    FarmState.AddCount(State.Inventory, item.Id, -item.Count);
            }
            State.Coins += order.RewardCoins;
            long xp = order.RewardXp;
            var delivery = new OrderDelivery(slot, order.NpcId, PickThanksLine(order), order.RewardCoins, xp);
            RollOrderDay();
            State.DeliveredToday++;
            ScheduleNextOrder(order, now, false);
            AddXp(xp);
            OrderDelivered?.Invoke(delivery);
            NotifyChanged();
            return FarmResult.Ok;
        }

        public FarmResult DiscardOrder(int slot)
        {
            if (!OrdersUnlocked)
                return FarmResult.OrdersLocked;
            if (!TryGetOrder(slot, out OrderState order))
                return FarmResult.UnknownOrder;
            if (!order.HasOrder)
                return FarmResult.OrderNotReady;

            ScheduleNextOrder(order, _clock.UtcNowSeconds, true);
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Sang ngày mới, đơn khẩn cấp hết hạn, điền đơn vào các ô đã hết thời gian chờ. Lệnh nào cũng tự gọi;
        // View gọi thêm định kỳ vì thời gian có thể trôi khi người chơi không làm gì.
        // Trả về true (và báo Changed) nếu có gì đổi.
        public bool RefreshOrders()
        {
            if (!FillOrders())
                return false;
            Changed?.Invoke();
            return true;
        }

        // Màn tạo nhân vật: đặt tên + ngoại hình. Kiểu / màu tóc là id của View, rỗng thì về mặc định.
        public FarmResult SetCharacter(string name, string hairStyle, string hairColor)
        {
            if (!CharacterLook.TryNormalizeName(name, out string clean))
                return FarmResult.InvalidName;

            CharacterLook look = State.Character;
            look.Name = clean;
            look.HairStyle = string.IsNullOrEmpty(hairStyle) ? CharacterLook.DefaultHairStyle : hairStyle;
            look.HairColor = string.IsNullOrEmpty(hairColor) ? CharacterLook.DefaultHairColor : hairColor;
            look.Created = true;
            Changed?.Invoke();
            return FarmResult.Ok;
        }

        public FarmResult Plant(PlotId id, string cropId)
        {
            if (!Definition.TryGetCrop(cropId, out CropDef crop))
                return FarmResult.UnknownCrop;
            if (crop.UnlockLevel > State.Level)
                return FarmResult.CropLocked;
            if (!TryGetPlot(id, out PlotState plot))
                return FarmResult.UnknownPlot;
            if (plot.Status == PlotStatus.Locked)
                return FarmResult.PlotLocked;
            if (plot.Status != PlotStatus.Empty)
                return FarmResult.PlotNotEmpty;
            if (State.GetSeeds(crop.Id) < 1)
                return FarmResult.NotEnoughSeeds;

            FarmState.AddCount(State.Seeds, crop.Id, -1);
            plot.Status = PlotStatus.Growing;
            plot.CropId = crop.Id;
            plot.PlantedAt = _clock.UtcNowSeconds;
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Tùy chọn, mỗi lần trồng tưới được 1 lần. Lùi PlantedAt nên không cần bộ đếm giờ riêng.
        public FarmResult Water(PlotId id)
        {
            if (!TryGetPlot(id, out PlotState plot))
                return FarmResult.UnknownPlot;
            if (plot.Status == PlotStatus.Locked)
                return FarmResult.PlotLocked;
            if (plot.Status != PlotStatus.Growing)
                return FarmResult.NothingToWater;
            if (!Definition.TryGetCrop(plot.CropId, out CropDef crop))
                return FarmResult.UnknownCrop;
            if (plot.Watered)
                return FarmResult.AlreadyWatered;
            if (GetElapsed(plot) >= crop.GrowSeconds)
                return FarmResult.AlreadyRipe;

            plot.PlantedAt -= crop.GrowSeconds * Definition.WaterReducePercent / 100;
            plot.Watered = true;
            NotifyChanged();
            return FarmResult.Ok;
        }

        public FarmResult Harvest(PlotId id)
        {
            if (!TryGetPlot(id, out PlotState plot))
                return FarmResult.UnknownPlot;
            if (plot.Status == PlotStatus.Locked)
                return FarmResult.PlotLocked;
            if (plot.Status != PlotStatus.Growing)
                return FarmResult.NothingToHarvest;
            if (!Definition.TryGetCrop(plot.CropId, out CropDef crop))
                return FarmResult.UnknownCrop;
            if (GetElapsed(plot) < crop.GrowSeconds)
                return FarmResult.NotRipe;

            FarmState.AddCount(State.Inventory, crop.Id, crop.HarvestYield);
            plot.Clear();
            AddXp(crop.Xp);
            TryFindBait();
            NotifyChanged();
            return FarmResult.Ok;
        }

        public FarmResult BuySeeds(string cropId, int amount)
        {
            if (!Definition.TryGetCrop(cropId, out CropDef crop))
                return FarmResult.UnknownCrop;
            if (crop.UnlockLevel > State.Level)
                return FarmResult.CropLocked;
            if (amount <= 0)
                return FarmResult.InvalidAmount;

            long cost = (long)crop.BuyPrice * amount;
            if (State.Coins < cost)
                return FarmResult.NotEnoughCoins;

            State.Coins -= cost;
            FarmState.AddCount(State.Seeds, crop.Id, amount);
            NotifyChanged();
            return FarmResult.Ok;
        }

        public FarmResult Sell(string cropId, int amount)
        {
            if (!Definition.TryGetCrop(cropId, out CropDef crop))
                return FarmResult.UnknownCrop;
            if (amount <= 0)
                return FarmResult.InvalidAmount;
            if (State.GetItems(crop.Id) < amount)
                return FarmResult.NotEnoughItems;

            FarmState.AddCount(State.Inventory, crop.Id, -amount);
            State.Coins += (long)crop.SellPrice * amount;
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Mở ô kế tiếp của khu (hết luống này mới sang luống sau). Mở xong ô cuối thì khu sau được mở khóa.
        public FarmResult UnlockNextPlot(string zoneId)
        {
            if (!Definition.TryGetZoneIndex(zoneId, out int index))
                return FarmResult.UnknownZone;

            ZoneState zone = State.Zones[index];
            if (zone.IsFull)
                return FarmResult.ZoneFull;
            if (index > 0 && !State.Zones[index - 1].IsFull)
                return FarmResult.ZoneLocked;
            if (State.Level < Definition.Zones[index].RequiredLevel)
                return FarmResult.LevelTooLow;

            int price = GetUnlockPrice(index);
            if (State.Coins < price)
                return FarmResult.NotEnoughCoins;

            State.Coins -= price;
            zone.OpenNext();
            if (zone.IsFull)
                State.GrantAvailableStartPlots(Definition);
            NotifyChanged();
            return FarmResult.Ok;
        }

        public bool TryGetNextUnlock(string zoneId, out PlotId plot, out int price)
        {
            plot = default;
            price = 0;
            if (!Definition.TryGetZoneIndex(zoneId, out int index))
                return false;

            ZoneState zone = State.Zones[index];
            if (zone.IsFull || !IsZoneAvailable(index))
                return false;

            plot = Definition.Zones[index].PlotIdAt(zone.OpenCount);
            price = GetUnlockPrice(index);
            return true;
        }

        public bool IsZoneAvailable(string zoneId)
        {
            return Definition.TryGetZoneIndex(zoneId, out int index) && IsZoneAvailable(index);
        }

        public bool TryGetPlotState(PlotId id, out PlotState plot) => TryGetPlot(id, out plot);

        public bool IsCropUnlocked(string cropId)
        {
            return Definition.TryGetCrop(cropId, out CropDef crop) && crop.UnlockLevel <= State.Level;
        }

        // XP cần để lên cấp kế tiếp; 0 nếu đã ở cấp tối đa.
        public int GetXpToNext() => Definition.Levels.GetXpToNext(State.Level);

        // Thưởng và mở khóa khi lên tới cấp này (cấp 1 không có thưởng).
        public LevelUnlocks GetUnlocksAtLevel(int level)
        {
            var crops = new List<string>();
            var zones = new List<int>();
            var rods = new List<string>();
            var baits = new List<string>();
            bool orders = false;
            long coins = 0;
            int seeds = 0;
            if (level > 1)
            {
                coins = (long)Definition.Levels.CoinsPerLevel * level;
                seeds = Definition.Levels.UnlockSeedCount;
                foreach (CropDef crop in Definition.Crops)
                {
                    if (crop.UnlockLevel == level)
                        crops.Add(crop.Id);
                }
                for (int i = 0; i < Definition.Zones.Count; i++)
                {
                    if (Definition.Zones[i].RequiredLevel == level)
                        zones.Add(i);
                }
                orders = Definition.Orders.SlotCount > 0 && Definition.Orders.UnlockLevel == level;
                if (Definition.Fishing.IsEnabled)
                {
                    foreach (RodDef rod in Definition.Fishing.Rods)
                    {
                        if (rod.UnlockLevel == level)
                            rods.Add(rod.Id);
                    }
                    foreach (BaitDef bait in Definition.Fishing.Baits)
                    {
                        if (bait.Buyable && bait.UnlockLevel == level)
                            baits.Add(bait.Id);
                    }
                }
            }
            return new LevelUnlocks(level, coins, crops, seeds, zones, orders, rods, baits);
        }

        // Giai đoạn 0..StageCount-1 của cây đang lớn; -1 nếu ô không có cây.
        // Giai đoạn cuối (sprite cây đã ra quả) chỉ hiện khi chín; các giai đoạn trước chia đều thời gian lớn.
        public int GetStage(PlotId id)
        {
            if (!TryGetGrowing(id, out PlotState plot, out CropDef crop))
                return -1;

            long elapsed = GetElapsed(plot);
            if (elapsed >= crop.GrowSeconds)
                return crop.StageCount - 1;
            if (crop.StageCount <= 1)
                return 0;
            return (int)(elapsed * (crop.StageCount - 1) / crop.GrowSeconds);
        }

        // Số giây còn lại tới khi chín, 0 nếu đã chín; -1 nếu ô không có cây.
        public long GetRemainingSeconds(PlotId id)
        {
            if (!TryGetGrowing(id, out PlotState plot, out CropDef crop))
                return -1;
            return Math.Max(0L, crop.GrowSeconds - GetElapsed(plot));
        }

        public bool IsRipe(PlotId id)
        {
            return TryGetGrowing(id, out PlotState plot, out CropDef crop) && GetElapsed(plot) >= crop.GrowSeconds;
        }

        private bool IsZoneAvailable(int index)
        {
            return (index == 0 || State.Zones[index - 1].IsFull)
                && State.Level >= Definition.Zones[index].RequiredLevel;
        }

        private void NotifyChanged()
        {
            TryRescue();
            FillOrders();
            Changed?.Invoke();
        }

        private bool FillOrders()
        {
            if (!OrdersUnlocked)
                return false;

            bool changed = RollOrderDay();
            long now = _clock.UtcNowSeconds;
            foreach (OrderState order in State.Orders)
            {
                if (!order.IsExpired(now))
                    continue;
                ScheduleNextOrder(order, now, false);
                changed = true;
            }

            foreach (OrderState order in State.Orders)
            {
                if (order.HasOrder || order.AvailableAt > now)
                    continue;
                if (!CanReceiveOrderToday)
                    break;
                // Ô chưa từng hẹn đơn (mới mở đơn hàng, save cũ, đơn hỏng bị bỏ): hạng tung lúc sinh.
                OrderRank rank = order.AvailableAt == 0 ? RollOrderRank() : order.Rank;
                changed |= GenerateOrder(order, rank, now);
            }
            return changed;
        }

        // Ô trống sau khi giao / bỏ / hết hạn: tung hạng đơn kế tiếp, chờ theo hạng.
        private void ScheduleNextOrder(OrderState order, long now, bool discarded)
        {
            OrderDef def = Definition.Orders;
            OrderRank rank = RollOrderRank();
            long wait = discarded ? def.GetDiscardSeconds(rank) : def.GetRefillSeconds(rank);
            // Luôn > 0 để phân biệt với ô chưa từng hẹn đơn (AvailableAt = 0).
            order.Clear(Math.Max(1L, now + wait), rank);
        }

        // Không có đơn khẩn cấp thì không dùng số ngẫu nhiên (giữ chuỗi đơn như trước).
        private OrderRank RollOrderRank()
        {
            int chance = Definition.Orders.UrgentChancePercent;
            if (chance <= 0)
                return OrderRank.Normal;
            if (chance >= 100)
                return OrderRank.Urgent;
            return SeededRandom.Range(ref State.RandomState, 0, 100) < chance ? OrderRank.Urgent : OrderRank.Normal;
        }

        // Sang ngày mới (giờ địa phương) thì đếm lại số đơn đã giao. Ngày lùi (đổi múi giờ) thì giữ nguyên.
        private bool RollOrderDay()
        {
            long day = CurrentDay;
            if (State.OrderDay != 0 && day <= State.OrderDay)
                return false;
            bool changed = State.DeliveredToday != 0;
            State.OrderDay = day;
            State.DeliveredToday = 0;
            return changed;
        }

        private int ActiveOrderCount
        {
            get
            {
                int count = 0;
                foreach (OrderState order in State.Orders)
                {
                    if (order.HasOrder)
                        count++;
                }
                return count;
            }
        }

        private bool GenerateOrder(OrderState order, OrderRank rank, long now)
        {
            // Đơn cá: chỉ hạng Thường (không có đơn cá khẩn), chỉ khi đã mở câu cá.
            if (rank == OrderRank.Normal && RollFishOrder(out List<FishDef> fishPool))
            {
                GenerateFishOrder(order, fishPool, now);
                return true;
            }

            OrderDef def = Definition.Orders;
            var unlocked = new List<CropDef>();
            foreach (CropDef crop in Definition.Crops)
            {
                if (crop.UnlockLevel <= State.Level)
                    unlocked.Add(crop);
            }
            if (unlocked.Count == 0)
                return false;

            int level = State.Level;
            int typeCount = SeededRandom.Range(ref State.RandomState, 1,
                Math.Min(unlocked.Count, def.GetMaxItemTypes(level)) + 1);
            var items = new OrderItem[typeCount];
            long coins = 0;
            long xp = 0;
            for (int i = 0; i < typeCount; i++)
            {
                int pick = SeededRandom.Range(ref State.RandomState, 0, unlocked.Count);
                CropDef crop = unlocked[pick];
                unlocked.RemoveAt(pick);

                int harvests = SeededRandom.Range(ref State.RandomState, 1, def.GetMaxHarvests(level) + 1);
                int count = harvests * crop.HarvestYield;
                items[i] = new OrderItem { Id = crop.Id, Count = count };
                coins += (long)count * crop.SellPrice;
                xp += (long)harvests * crop.Xp;
            }

            order.Kind = OrderKind.Crops;
            order.Items = items;
            FinishOrder(order, rank, now, coins, xp);
            return true;
        }

        // Đơn cá: 1 loài trong pool, 1..GetFishOrderMaxCount con. Thưởng theo giá / XP gốc của cá, cùng hệ số đơn nông sản.
        private void GenerateFishOrder(OrderState order, List<FishDef> pool, long now)
        {
            int levelsAbove = State.Level - Definition.Fishing.UnlockLevel;
            FishDef fish = pool[SeededRandom.Range(ref State.RandomState, 0, pool.Count)];
            int count = SeededRandom.Range(ref State.RandomState, 1, Definition.Orders.GetFishOrderMaxCount(levelsAbove) + 1);
            order.Kind = OrderKind.Fish;
            order.Items = new[] { new OrderItem { Id = fish.Id, Count = count } };
            FinishOrder(order, OrderRank.Normal, now, (long)count * fish.BasePrice, (long)count * fish.BaseXp);
        }

        // Có ra đơn cá không: đã mở câu cá, tỉ lệ theo cấp > 0, có loài hợp lệ. Không đủ điều kiện thì không tung số
        // (giữ chuỗi đơn như trước).
        private bool RollFishOrder(out List<FishDef> pool)
        {
            pool = null;
            OrderDef def = Definition.Orders;
            if (!FishingUnlocked)
                return false;
            int percent = def.GetFishOrderPercent(State.Level - Definition.Fishing.UnlockLevel);
            if (percent <= 0)
                return false;
            pool = GetFishOrderPool();
            if (pool.Count == 0)
                return false;
            return SeededRandom.Range(ref State.RandomState, 0, 100) < percent;
        }

        // Loài đặt được trong đơn cá: độ hiếm thấp, câu được ở đủ nhiều khung giờ (không kẹt đơn).
        // Phần 5 (album) sẽ lọc thêm: chỉ loài đã câu được.
        public List<FishDef> GetFishOrderPool()
        {
            OrderDef def = Definition.Orders;
            var pool = new List<FishDef>();
            foreach (FishDef fish in Definition.Fishing.Fish)
            {
                if (fish.Rarity > def.FishOrderMaxRarity)
                    continue;
                int slots = 0;
                foreach (float factor in fish.SlotFactors)
                {
                    if (factor > 0f)
                        slots++;
                }
                if (slots >= def.FishOrderMinSlots)
                    pool.Add(fish);
            }
            return pool;
        }

        // Thưởng, hạng, hạn; rồi gắn người đặt + câu thoại.
        private void FinishOrder(OrderState order, OrderRank rank, long now, long coins, long xp)
        {
            OrderDef def = Definition.Orders;
            long rankPercent = rank == OrderRank.Urgent ? def.UrgentRewardPercent : 100;
            // Làm tròn tới 5 xu gần nhất, ít nhất 5. Đơn thường: (coins × CoinPercent + 250) / 500 × 5.
            order.RewardCoins = Math.Max(5L, (coins * def.CoinPercent * rankPercent + 25000) / 50000 * 5);
            order.RewardXp = Math.Max(1L, (xp * def.XpPercent * rankPercent + 5000) / 10000);
            order.AvailableAt = 0;
            order.Rank = rank;
            order.ExpiresAt = rank == OrderRank.Urgent && def.UrgentExpireSeconds > 0 ? now + def.UrgentExpireSeconds : 0;
            // 1 lần tung từ chuỗi chính (thay cho lần chọn tên khách cũ); người + câu chọn bằng random riêng của đơn.
            order.Seed = SeededRandom.Next(ref State.RandomState);
            AssignDialogue(order);
        }

        // Người đặt: theo trọng số (đơn cá dùng FishWeight); người hay đặt cây có trong đơn ×3.
        // Câu: đúng loại (khẩn / cá / thường), không lặp câu người đó vừa dùng.
        private void AssignDialogue(OrderState order)
        {
            OrderDialogue dialogue = Definition.Orders.Dialogue;
            uint random = SeededRandom.Mix(order.Seed);
            NpcDef npc = PickNpc(dialogue, order, ref random);
            order.NpcId = npc?.Id;

            string itemId = order.Items.Length > 0 ? order.Items[0].Id : null;
            bool hasDish = !order.IsFish && dialogue.GetDishes(itemId).Count > 0;
            OrderLineKind kind = order.IsFish ? OrderLineKind.Fish
                : order.IsUrgent ? OrderLineKind.Urgent : OrderLineKind.Request;
            int exclude = npc != null && State.LastOrderLines.TryGetValue(npc.Id, out int last) ? last : -1;
            int line = dialogue.PickLine(kind, npc?.Id, itemId, hasDish, exclude, ref random);
            if (line < 0 && kind != OrderLineKind.Request)
                line = dialogue.PickLine(OrderLineKind.Request, npc?.Id, itemId, hasDish, exclude, ref random);
            order.Line = line;
            if (npc != null && line >= 0)
                State.LastOrderLines[npc.Id] = line;
        }

        private static NpcDef PickNpc(OrderDialogue dialogue, OrderState order, ref uint random)
        {
            IReadOnlyList<NpcDef> npcs = dialogue.Npcs;
            if (npcs.Count == 0)
                return null;
            bool fish = order.IsFish;
            long total = 0;
            for (int pass = 0; pass < 2 && total == 0; pass++)
            {
                // Lượt 2: không ai đặt đơn cá thì chọn theo trọng số thường.
                bool useFish = fish && pass == 0;
                total = 0;
                foreach (NpcDef npc in npcs)
                    total += NpcWeight(npc, order, useFish);
                if (total == 0)
                    continue;
                long pick = SeededRandom.Next(ref random) % total;
                foreach (NpcDef npc in npcs)
                {
                    long weight = NpcWeight(npc, order, useFish);
                    if (pick < weight)
                        return npc;
                    pick -= weight;
                }
            }
            return null;
        }

        private static long NpcWeight(NpcDef npc, OrderState order, bool fish)
        {
            if (fish)
                return npc.FishWeight;
            if (order.IsFish)
                return npc.Weight;
            foreach (OrderItem item in order.Items)
            {
                if (npc.Prefers(item.Id))
                    return (long)npc.Weight * PreferredNpcFactor;
            }
            return npc.Weight;
        }

        private const int PreferredNpcFactor = 3;

        // Câu cảm ơn khi giao: của người đặt hoặc dùng chung, không lặp câu cảm ơn vừa dùng.
        private int PickThanksLine(OrderState order)
        {
            OrderDialogue dialogue = Definition.Orders.Dialogue;
            uint random = SeededRandom.Mix(order.Seed ^ 0x7A4E4B53u);
            string itemId = order.Items.Length > 0 ? order.Items[0].Id : null;
            int line = dialogue.PickLine(OrderLineKind.Thanks, order.NpcId, itemId, false, State.LastThanksLine, ref random);
            if (line >= 0)
                State.LastThanksLine = line;
            return line;
        }

        // Cộng XP, lên liền nhiều cấp nếu đủ. Tới cấp tối đa thì không giữ XP nữa.
        private void AddXp(long amount)
        {
            LevelDef levels = Definition.Levels;
            if (amount <= 0 || State.Level >= levels.MaxLevel)
                return;

            State.Xp += amount;
            while (State.Level < levels.MaxLevel)
            {
                int need = levels.GetXpToNext(State.Level);
                if (State.Xp < need)
                    break;

                State.Xp -= need;
                State.Level++;
                if (State.Level >= levels.MaxLevel)
                    State.Xp = 0;
                GrantLevelReward(State.Level);
                State.GrantAvailableStartPlots(Definition);
                GrantFishingStarter();
                LeveledUp?.Invoke(State.Level);
            }
        }

        private void GrantLevelReward(int level)
        {
            LevelDef levels = Definition.Levels;
            State.Coins += (long)levels.CoinsPerLevel * level;
            if (levels.UnlockSeedCount <= 0)
                return;

            foreach (CropDef crop in Definition.Crops)
            {
                if (crop.UnlockLevel == level)
                    FarmState.AddCount(State.Seeds, crop.Id, levels.UnlockSeedCount);
            }
        }

        // Kẹt = không đủ xu mua hạt rẻ nhất đã mở, không còn hạt gieo được, không còn nông sản bán được, không có cây.
        private void TryRescue()
        {
            string rescueId = Definition.RescueCropId;
            if (rescueId == null || !IsStuck())
                return;

            FarmState.AddCount(State.Seeds, rescueId, 1);
            Rescued?.Invoke(rescueId);
        }

        private bool IsStuck()
        {
            long cheapestSeed = long.MaxValue;
            foreach (CropDef crop in Definition.Crops)
            {
                if (State.GetItems(crop.Id) > 0)
                    return false;
                if (crop.UnlockLevel > State.Level)
                    continue;
                if (State.GetSeeds(crop.Id) > 0)
                    return false;
                if (crop.BuyPrice < cheapestSeed)
                    cheapestSeed = crop.BuyPrice;
            }

            if (State.Coins >= cheapestSeed)
                return false;
            // Còn cá trong giỏ thì bán được lấy xu (giai đoạn 5, 13/11/2026). Mồi / cần không tính.
            if (State.Fishing != null && State.Fishing.Basket.Count > 0)
                return false;

            foreach (ZoneState zone in State.Zones)
            {
                foreach (PlotState plot in zone.Plots)
                {
                    if (plot.Status == PlotStatus.Growing)
                        return false;
                }
            }
            return true;
        }

        private int GetUnlockPrice(int index)
        {
            ZoneDef def = Definition.Zones[index];
            return def.UnlockPrices[State.Zones[index].OpenCount - def.StartOpenCount];
        }

        // Đồng hồ lùi (chỉnh giờ) thì coi như chưa trôi qua giây nào.
        private long GetElapsed(PlotState plot) => Math.Max(0L, _clock.UtcNowSeconds - plot.PlantedAt);

        private bool TryGetGrowing(PlotId id, out PlotState plot, out CropDef crop)
        {
            crop = null;
            return TryGetPlot(id, out plot)
                && plot.Status == PlotStatus.Growing
                && Definition.TryGetCrop(plot.CropId, out crop);
        }

        private bool TryGetPlot(PlotId id, out PlotState plot)
        {
            plot = null;
            if (!Definition.TryGetZoneIndex(id.ZoneId, out int index))
                return false;

            if (!Definition.Zones[index].TryGetPlotIndex(id.BedIndex, id.Col, id.Row, out int plotIndex))
                return false;

            plot = State.Zones[index].Plots[plotIndex];
            return true;
        }

        private void ValidateState()
        {
            if (State.Zones == null || State.Zones.Length != Definition.Zones.Count)
                throw new ArgumentException("Số khu trong state không khớp data.");

            for (int i = 0; i < State.Zones.Length; i++)
            {
                ZoneDef def = Definition.Zones[i];
                ZoneState zone = State.Zones[i];
                if (zone == null || zone.ZoneId != def.Id
                    || zone.Plots == null || zone.Plots.Length != def.PlotCount
                    || zone.OpenCount < 0 || zone.OpenCount > def.PlotCount)
                    throw new ArgumentException($"Khu thứ {i} trong state không khớp data '{def.Id}'.");
            }

            State.Seeds ??= new Dictionary<string, int>();
            State.Inventory ??= new Dictionary<string, int>();
            NormalizeOrders();
            NormalizeFishing();
            NormalizeMail();
            if (State.RandomState == 0)
                State.RandomState = SeededRandom.Seed(_clock.UtcNowSeconds);
        }

        // Số ô đơn theo data; đơn có cây không còn trong data hoặc số lượng sai thì bỏ để sinh đơn mới.
        private void NormalizeOrders()
        {
            int slots = Definition.Orders.SlotCount;
            OrderState[] current = State.Orders ?? Array.Empty<OrderState>();
            if (current.Length != slots)
            {
                var resized = new OrderState[slots];
                for (int i = 0; i < slots; i++)
                    resized[i] = i < current.Length ? current[i] : null;
                current = resized;
            }

            for (int i = 0; i < current.Length; i++)
            {
                OrderState order = current[i] ??= new OrderState();
                order.Items ??= Array.Empty<OrderItem>();
                if (order.HasOrder && !IsValidOrder(order))
                    order.Clear(0, OrderRank.Normal);
                // Người / câu không còn trong data: bỏ người, bỏ câu (đơn vẫn giữ).
                OrderDialogue dialogue = Definition.Orders.Dialogue;
                if (order.NpcId != null && !dialogue.TryGetNpc(order.NpcId, out _))
                    order.NpcId = null;
                if (!dialogue.TryGetLine(order.Line, out _))
                    order.Line = -1;
            }
            State.Orders = current;
            State.LastOrderLines ??= new Dictionary<string, int>();
        }

        private bool IsValidOrder(OrderState order)
        {
            if (order.RewardCoins < 0 || order.RewardXp < 0)
                return false;
            if (order.IsFish && order.Items.Length != 1)
                return false;
            foreach (OrderItem item in order.Items)
            {
                if (item == null || item.Count <= 0)
                    return false;
                bool known = order.IsFish ? Definition.Fishing.TryGetFish(item.Id, out _) : Definition.TryGetCrop(item.Id, out _);
                if (!known)
                    return false;
            }
            return true;
        }
    }
}
