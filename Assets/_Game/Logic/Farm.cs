using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Luật chơi: gieo, tưới, thu hoạch, mua hạt, bán nông sản, mở ô. Mọi thay đổi FarmState đi qua đây.
    // Thời gian chỉ lấy từ IClock; không đếm giờ theo frame.
    public sealed class Farm
    {
        private readonly IClock _clock;

        // Báo sau mỗi lệnh thành công. Cây lớn theo thời gian không phát sự kiện, View tự làm mới định kỳ.
        public event Action Changed;
        // Báo cấp mới, sau khi đã trao thưởng; phát trước Changed của lệnh đó.
        public event Action<int> LeveledUp;
        // Báo id cây được tặng 1 hạt vì người chơi bị kẹt; phát trước Changed của lệnh đó.
        public event Action<string> Rescued;

        public FarmDef Definition { get; }
        public FarmState State { get; }

        public Farm(FarmDef definition, FarmState state, IClock clock)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            ValidateState();
            State.GrantAvailableStartPlots(Definition);
            TryRescue();
            FillOrders();
        }

        public int OrderSlotCount => State.Orders.Length;

        public bool OrdersUnlocked => Definition.Orders.SlotCount > 0 && State.Level >= Definition.Orders.UnlockLevel;

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

        // Đủ nông sản trong Kho để giao đơn này.
        public bool CanDeliver(int slot)
        {
            if (!OrdersUnlocked || !TryGetOrder(slot, out OrderState order) || !order.HasOrder)
                return false;
            foreach (OrderItem item in order.Items)
            {
                if (State.GetItems(item.CropId) < item.Count)
                    return false;
            }
            return true;
        }

        public FarmResult DeliverOrder(int slot)
        {
            if (!OrdersUnlocked)
                return FarmResult.OrdersLocked;
            if (!TryGetOrder(slot, out OrderState order))
                return FarmResult.UnknownOrder;
            if (!order.HasOrder)
                return FarmResult.OrderNotReady;
            if (!CanDeliver(slot))
                return FarmResult.NotEnoughItems;

            foreach (OrderItem item in order.Items)
                FarmState.AddCount(State.Inventory, item.CropId, -item.Count);
            State.Coins += order.RewardCoins;
            long xp = order.RewardXp;
            order.Clear(_clock.UtcNowSeconds);
            AddXp(xp);
            GenerateOrder(order);
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

            order.Clear(_clock.UtcNowSeconds + Definition.Orders.DiscardCooldownSeconds);
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Điền đơn vào các ô đã hết thời gian chờ. Lệnh nào cũng tự gọi; View gọi thêm định kỳ vì thời gian chờ
        // có thể hết khi người chơi không làm gì. Trả về true (và báo Changed) nếu có đơn mới.
        public bool RefreshOrders()
        {
            if (!FillOrders())
                return false;
            Changed?.Invoke();
            return true;
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
            }
            return new LevelUnlocks(level, coins, crops, seeds, zones, orders);
        }

        // Giai đoạn 0..StageCount-1 của cây đang lớn; -1 nếu ô không có cây.
        public int GetStage(PlotId id)
        {
            if (!TryGetGrowing(id, out PlotState plot, out CropDef crop))
                return -1;

            long elapsed = GetElapsed(plot);
            if (elapsed >= crop.GrowSeconds)
                return crop.StageCount - 1;
            return (int)(elapsed * crop.StageCount / crop.GrowSeconds);
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

            bool filled = false;
            long now = _clock.UtcNowSeconds;
            foreach (OrderState order in State.Orders)
            {
                if (order.HasOrder || order.AvailableAt > now)
                    continue;
                filled |= GenerateOrder(order);
            }
            return filled;
        }

        private bool GenerateOrder(OrderState order)
        {
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
                items[i] = new OrderItem { CropId = crop.Id, Count = count };
                coins += (long)count * crop.SellPrice;
                xp += (long)harvests * crop.Xp;
            }

            IReadOnlyList<string> names = def.CustomerNames;
            order.Customer = names.Count > 0 ? names[SeededRandom.Range(ref State.RandomState, 0, names.Count)] : null;
            order.Items = items;
            // Làm tròn tới 5 xu gần nhất, ít nhất 5.
            order.RewardCoins = Math.Max(5L, (coins * def.CoinPercent + 250) / 500 * 5);
            order.RewardXp = Math.Max(1L, (xp * def.XpPercent + 50) / 100);
            order.AvailableAt = 0;
            return true;
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
                    order.Clear(0);
            }
            State.Orders = current;
        }

        private bool IsValidOrder(OrderState order)
        {
            if (order.RewardCoins < 0 || order.RewardXp < 0)
                return false;
            foreach (OrderItem item in order.Items)
            {
                if (item == null || item.Count <= 0 || !Definition.TryGetCrop(item.CropId, out _))
                    return false;
            }
            return true;
        }
    }
}
