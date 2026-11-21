using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Câu cá (MVP-Plan Phần 10): cần, mồi, giỏ cá, phiên câu. Mọi thay đổi tiến trình đi qua đây như phần trồng trọt.
    public sealed partial class Farm
    {
        // Báo mồi đào được khi thu hoạch (id mồi, số lượng); phát trước Changed của lệnh đó.
        public event Action<string, int> BaitFound;

        public FishingDef FishingDefinition => Definition.Fishing;

        public bool FishingUnlocked => Definition.Fishing.IsEnabled && State.Level >= Definition.Fishing.UnlockLevel;

        // Sức chứa giỏ = số ô đã mở.
        public int BasketCapacity => Math.Max(Definition.Fishing.BasketStartSlots,
            Math.Min(State.Fishing.BasketSlots, Definition.Fishing.BasketMaxSlots));

        // Cá + rác cùng chiếm ô giỏ.
        public bool IsBasketFull => State.Fishing.BasketUsed >= BasketCapacity;

        public bool CanUnlockBasketSlot => BasketCapacity < Definition.Fishing.BasketMaxSlots;

        // Giá ô giỏ kế tiếp; đã đủ ô thì -1.
        public long NextBasketSlotPrice => CanUnlockBasketSlot
            ? Definition.Fishing.BasketSlotPrices[BasketCapacity - Definition.Fishing.BasketStartSlots] : -1;

        public bool TryGetEquippedRod(out RodDef def, out RodState rod)
        {
            rod = null;
            def = null;
            FishingState fishing = State.Fishing;
            return fishing.EquippedRodId != null
                && Definition.Fishing.TryGetRod(fishing.EquippedRodId, out def)
                && fishing.TryGetRod(fishing.EquippedRodId, out rod);
        }

        // Độ bền tối đa của cần người chơi đang có (không đổi khi sửa); -1 nếu không có.
        public int GetRodMaxDurability(string rodId)
        {
            if (!Definition.Fishing.TryGetRod(rodId, out RodDef def) || !State.Fishing.TryGetRod(rodId, out _))
                return -1;
            return def.MaxDurability;
        }

        // Giá sửa cần đang có về đầy (0 = đang đầy); -1 nếu không có cần.
        public long GetRodRepairPrice(string rodId)
        {
            if (!Definition.Fishing.TryGetRod(rodId, out RodDef def) || !State.Fishing.TryGetRod(rodId, out RodState rod))
                return -1;
            return Definition.Fishing.Tuning.RepairPrice(def.Price, rod.Durability, def.MaxDurability);
        }

        // Cần đang có còn ≤ 10% độ bền (gồm cả đã hỏng).
        public bool IsRodLow(string rodId)
        {
            if (!Definition.Fishing.TryGetRod(rodId, out RodDef def) || !State.Fishing.TryGetRod(rodId, out RodState rod))
                return false;
            return Definition.Fishing.Tuning.IsLowDurability(rod.Durability, def.MaxDurability);
        }

        // Kiểm tra trước khi ném (để UI làm mờ nút / báo lý do).
        public FarmResult CanCast(string baitId)
        {
            if (!FishingUnlocked)
                return FarmResult.FishingLocked;
            if (!Definition.Fishing.TryGetBait(baitId, out _))
                return FarmResult.UnknownBait;
            if (!TryGetEquippedRod(out _, out RodState rod))
                return FarmResult.NoRod;
            if (rod.IsBroken)
                return FarmResult.RodBroken;
            if (State.Fishing.GetBaits(baitId) <= 0)
                return FarmResult.NotEnoughBait;
            if (IsBasketFull)
                return FarmResult.BasketFull;
            return FarmResult.Ok;
        }

        // Ném cần với lực 0..1 (View đọc từ thanh lực). Trừ 1 mồi và độ bền ngay (bỏ dở giữa chừng vẫn mất).
        // Khung giờ tính lúc ném theo giờ VN (GameCalendar, UTC+7 cố định).
        public FarmResult Cast(string baitId, float power, out FishingSession session)
        {
            session = null;
            FarmResult check = CanCast(baitId);
            if (check != FarmResult.Ok)
                return check;

            FishingDef def = Definition.Fishing;
            def.TryGetBait(baitId, out BaitDef bait);
            TryGetEquippedRod(out RodDef rodDef, out RodState rod);

            FarmState.AddCount(State.Fishing.Baits, baitId, -1);
            rod.Durability = Math.Max(0, rod.Durability - def.Tuning.CastDurabilityCost);

            DaySlot slot = Calendar.TimeWindow;
            uint seed = SeededRandom.Next(ref State.RandomState);
            // Hệ số sự kiện chốt lúc ném (Đợt 2 Phần 7).
            session = new FishingSession(def, rodDef, bait, slot, power, seed, GetFishWeightMult, GetFishSizeMult);
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Áp kết quả phiên đã xong: cá vào giỏ + XP + album + mốc thưởng. Rác: vào giỏ hoặc tự bán, ghi album rác.
        // Không ra gì: không thêm gì (mồi + độ bền đã trừ lúc ném – trúng rác cũng là mất mồi).
        public FarmResult CompleteFishing(FishingSession session)
        {
            if (session == null || session.Phase != FishingPhase.Finished)
                return FarmResult.FishingNotFinished;
            if (session.Applied)
                return FarmResult.FishingAlreadyApplied;
            session.Applied = true;

            FishingState fishing = State.Fishing;
            if (session.Outcome == FishingOutcome.Caught && session.IsJunk)
            {
                string junkId = session.Junk.Id;
                session.IsNewJunk = !fishing.JunkDex.TryGetValue(junkId, out JunkDexEntry junkEntry);
                if (junkEntry == null)
                    fishing.JunkDex[junkId] = junkEntry = new JunkDexEntry { FirstDayKey = Calendar.DayKey };
                junkEntry.Count++;
                if (fishing.AutoSellJunk)
                {
                    State.Coins += session.Price;
                    session.JunkAutoSold = true;
                }
                else
                {
                    fishing.JunkBasket.Add(new CaughtJunk { JunkId = junkId, CaughtAt = _clock.UtcNowSeconds });
                }
            }
            else if (session.Outcome == FishingOutcome.Caught)
            {
                fishing.Basket.Add(new CaughtFish
                {
                    FishId = session.Fish.Id,
                    Grams = session.Grams,
                    CaughtAt = _clock.UtcNowSeconds,
                    Slot = session.Slot,
                    Shiny = session.Shiny,
                });
                session.IsNewShiny = session.Shiny != ShinyKind.None && !fishing.HasShiny(session.Fish.Id, session.Shiny);
                if (session.IsNewShiny)
                    fishing.ShinyDex.Add(new ShinyRecord
                    {
                        FishId = session.Fish.Id,
                        Kind = session.Shiny,
                        FirstCaughtAt = _clock.UtcNowSeconds,
                    });
                int record = fishing.GetRecord(session.Fish.Id);
                session.PreviousRecordGrams = record;
                session.IsNewRecord = session.Grams > record;
                if (!fishing.FishDex.TryGetValue(session.Fish.Id, out FishDexEntry entry))
                    fishing.FishDex[session.Fish.Id] = entry = new FishDexEntry { FirstDayKey = Calendar.DayKey };
                entry.Count++;
                entry.MaxGrams = Math.Max(entry.MaxGrams, session.Grams);
                session.ReachedMilestones = SendDexMilestones();
                AddXp(session.Xp);
            }

            NotifyChanged();
            return FarmResult.Ok;
        }

        // Số loài (còn trong data) đã câu được.
        public int DexSpeciesCount
        {
            get
            {
                int count = 0;
                foreach (FishDef fish in Definition.Fishing.Fish)
                {
                    if (State.Fishing.HasCaught(fish.Id))
                        count++;
                }
                return count;
            }
        }

        // Đã đạt mốc chưa (không xét đã gửi thư hay chưa).
        public bool IsDexMilestoneReached(DexMilestoneDef milestone)
        {
            if (milestone.Kind == DexMilestoneKind.SpeciesCount)
                return DexSpeciesCount >= milestone.Count;
            bool any = false;
            foreach (FishDef fish in Definition.Fishing.Fish)
            {
                if (fish.Rarity != milestone.Rarity)
                    continue;
                any = true;
                if (!State.Fishing.HasCaught(fish.Id))
                    return false;
            }
            return any;
        }

        // Gửi thư cho các mốc đã đạt mà chưa gửi (id thư = id mốc → mỗi mốc 1 lần). Không báo Changed.
        private IReadOnlyList<DexMilestoneDef> SendDexMilestones()
        {
            List<DexMilestoneDef> reached = null;
            foreach (DexMilestoneDef milestone in Definition.Fishing.DexMilestones)
            {
                if (State.Mailbox.SentIds.Contains(milestone.Id) || !IsDexMilestoneReached(milestone))
                    continue;
                if (AddMail(milestone.Id, milestone.Sender, milestone.Title, milestone.Body, milestone.Rewards, 0))
                    (reached ??= new List<DexMilestoneDef>()).Add(milestone);
            }
            if (reached == null)
                return Array.Empty<DexMilestoneDef>();
            CleanupMail(Calendar.DayKey);
            return reached.AsReadOnly();
        }

        // Giá bán 1 món rác cho cô đồng nát (rác không còn trong data → 0).
        public int GetJunkPrice(CaughtJunk junk)
            => junk != null && Definition.Fishing.TryGetJunk(junk.JunkId, out JunkDef def) ? def.SellPrice : 0;

        // Bán 1 món rác trong giỏ.
        public FarmResult SellJunk(int junkIndex, out long earned)
        {
            earned = 0;
            List<CaughtJunk> junk = State.Fishing.JunkBasket;
            if (junkIndex < 0 || junkIndex >= junk.Count)
                return FarmResult.UnknownFish;
            earned = GetJunkPrice(junk[junkIndex]);
            junk.RemoveAt(junkIndex);
            State.Coins += earned;
            NotifyChanged();
            return FarmResult.Ok;
        }

        public void SetAutoSellJunk(bool on)
        {
            if (State.Fishing.AutoSellJunk == on)
                return;
            State.Fishing.AutoSellJunk = on;
            NotifyChanged();
        }

        private long GetJunkBasketValue()
        {
            long total = 0;
            foreach (CaughtJunk junk in State.Fishing.JunkBasket)
                total += GetJunkPrice(junk);
            return total;
        }

        // Mua cần chưa có. Cần đã có (kể cả hỏng) thì sửa, không mua lại.
        public FarmResult BuyRod(string rodId)
        {
            if (!FishingUnlocked)
                return FarmResult.FishingLocked;
            if (!Definition.Fishing.TryGetRod(rodId, out RodDef def))
                return FarmResult.UnknownRod;
            if (State.Fishing.TryGetRod(rodId, out _))
                return FarmResult.AlreadyOwned;
            if (State.Level < def.UnlockLevel)
                return FarmResult.LevelTooLow;
            if (State.Coins < def.Price)
                return FarmResult.NotEnoughCoins;

            State.Coins -= def.Price;
            State.Fishing.Rods.Add(new RodState { Id = rodId, Durability = def.MaxDurability });
            State.Fishing.EquippedRodId ??= rodId;
            NotifyChanged();
            return FarmResult.Ok;
        }

        public FarmResult EquipRod(string rodId)
        {
            if (!State.Fishing.TryGetRod(rodId, out _) || !Definition.Fishing.TryGetRod(rodId, out _))
                return FarmResult.UnknownRod;
            if (State.Fishing.EquippedRodId == rodId)
                return FarmResult.Ok;
            State.Fishing.EquippedRodId = rodId;
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Sửa (Đợt 2 Phần 1): sửa lúc nào cũng được khi chưa đầy, về đầy độ bền, độ bền tối đa giữ nguyên.
        // Giá theo phần độ bền còn thiếu (FishingTuning.RepairPrice). Đang đầy → RodNotDamaged.
        public FarmResult RepairRod(string rodId)
        {
            if (!Definition.Fishing.TryGetRod(rodId, out RodDef def) || !State.Fishing.TryGetRod(rodId, out RodState rod))
                return FarmResult.UnknownRod;
            long price = Definition.Fishing.Tuning.RepairPrice(def.Price, rod.Durability, def.MaxDurability);
            if (price <= 0)
                return FarmResult.RodNotDamaged;
            if (State.Coins < price)
                return FarmResult.NotEnoughCoins;

            State.Coins -= price;
            rod.Durability = def.MaxDurability;
            NotifyChanged();
            return FarmResult.Ok;
        }

        public FarmResult BuyBait(string baitId, int amount)
        {
            if (!FishingUnlocked)
                return FarmResult.FishingLocked;
            if (!Definition.Fishing.TryGetBait(baitId, out BaitDef bait) || !bait.Buyable)
                return FarmResult.UnknownBait;
            if (amount <= 0)
                return FarmResult.InvalidAmount;
            if (State.Level < bait.UnlockLevel)
                return FarmResult.LevelTooLow;
            long cost = bait.Price * amount;
            if (State.Coins < cost)
                return FarmResult.NotEnoughCoins;

            State.Coins -= cost;
            FarmState.AddCount(State.Fishing.Baits, baitId, amount);
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Giá 1 con trong giỏ theo cân nặng, cá shiny nhân hệ số (cá không còn trong data → 0).
        public int GetFishPrice(CaughtFish fish)
        {
            if (fish == null || !Definition.Fishing.TryGetFish(fish.FishId, out FishDef def))
                return 0;
            return FishingMath.FishPrice(def, fish.Grams, Definition.Fishing.Tuning, fish.Shiny);
        }

        // Số món "Bán hết" sẽ bán: mọi rác + cá, bỏ qua cá shiny (chốt 20/11/2026) và cá đang cần cho đơn cá (Đợt 2 Phần 11).
        public int GetSellAllCount()
        {
            bool[] kept = GetSellAllKeptMask();
            int count = State.Fishing.JunkBasket.Count;
            for (int i = 0; i < kept.Length; i++)
            {
                if (!kept[i])
                    count++;
            }
            return count;
        }

        // Con trong giỏ được giữ lại khi "Bán hết": cá shiny, và cá sẽ được giao cho đơn cá đang có
        // (những con nhẹ nhất cùng loài, đúng con mà lúc giao sẽ lấy).
        public bool IsKeptOnSellAll(int basketIndex)
        {
            bool[] kept = GetSellAllKeptMask();
            return basketIndex >= 0 && basketIndex < kept.Length && kept[basketIndex];
        }

        private bool[] GetSellAllKeptMask()
        {
            List<CaughtFish> basket = State.Fishing.Basket;
            var kept = new bool[basket.Count];
            for (int i = 0; i < basket.Count; i++)
                kept[i] = basket[i].IsShiny;

            long now = _clock.UtcNowSeconds;
            var need = new Dictionary<string, int>();
            foreach (OrderState order in State.Orders)
            {
                if (order == null || !order.IsFish || !order.HasOrder || order.IsExpired(now))
                    continue;
                foreach (OrderItem item in order.Items)
                    need[item.Id] = (need.TryGetValue(item.Id, out int n) ? n : 0) + item.Count;
            }
            foreach (KeyValuePair<string, int> pair in need)
            {
                for (int k = 0; k < pair.Value; k++)
                {
                    int index = FindLightestOrderFish(pair.Key, kept);
                    if (index < 0)
                        break;
                    kept[index] = true;
                }
            }
            return kept;
        }

        // Số cá thường (không shiny) của loài trong giỏ – dùng cho đơn cá.
        private int CountOrderFish(string fishId)
        {
            int count = 0;
            foreach (CaughtFish fish in State.Fishing.Basket)
            {
                if (fish.FishId == fishId && !fish.IsShiny)
                    count++;
            }
            return count;
        }

        // Giao đơn cá: lấy những con nhẹ nhất (bỏ qua cân nặng, giữ con nặng cho người chơi), không lấy cá shiny.
        private void TakeOrderFish(string fishId, int count)
        {
            List<CaughtFish> basket = State.Fishing.Basket;
            for (int k = 0; k < count; k++)
            {
                int index = FindLightestOrderFish(fishId, null);
                if (index < 0)
                    return;
                basket.RemoveAt(index);
            }
        }

        // Con cá thường nhẹ nhất của loài, bỏ qua các ô skip[i] = true; -1 nếu không có.
        private int FindLightestOrderFish(string fishId, bool[] skip)
        {
            List<CaughtFish> basket = State.Fishing.Basket;
            int best = -1;
            for (int i = 0; i < basket.Count; i++)
            {
                CaughtFish fish = basket[i];
                if (fish.FishId != fishId || fish.IsShiny || (skip != null && skip[i]))
                    continue;
                if (best < 0 || fish.Grams < basket[best].Grams)
                    best = i;
            }
            return best;
        }

        // Bán 1 con trong giỏ theo giá cân nặng (Phần 10 giai đoạn 5, duyệt 13/11/2026).
        public FarmResult SellFish(int basketIndex, out long earned)
        {
            earned = 0;
            List<CaughtFish> basket = State.Fishing.Basket;
            if (basketIndex < 0 || basketIndex >= basket.Count)
                return FarmResult.UnknownFish;
            earned = GetFishPrice(basket[basketIndex]);
            basket.RemoveAt(basketIndex);
            State.Coins += earned;
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Tổng giá "Bán hết" sẽ bán (rác + cá, không gồm cá giữ lại) – hiện ở nút / hộp xác nhận.
        public long GetBasketValue()
        {
            List<CaughtFish> basket = State.Fishing.Basket;
            bool[] kept = GetSellAllKeptMask();
            long total = GetJunkBasketValue();
            for (int i = 0; i < basket.Count; i++)
            {
                if (!kept[i])
                    total += GetFishPrice(basket[i]);
            }
            return total;
        }

        // Bán hết (cả rác), giữ lại cá shiny và cá đang cần cho đơn cá (bán lẻ từng con vẫn được). Không có gì để bán → UnknownFish.
        public FarmResult SellAllFish(out long earned)
        {
            earned = 0;
            List<CaughtFish> basket = State.Fishing.Basket;
            if (GetSellAllCount() == 0)
                return FarmResult.UnknownFish;
            earned = GetBasketValue();
            bool[] kept = GetSellAllKeptMask();
            for (int i = basket.Count - 1; i >= 0; i--)
            {
                if (!kept[i])
                    basket.RemoveAt(i);
            }
            State.Fishing.JunkBasket.Clear();
            State.Coins += earned;
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Thả 1 con khỏi giỏ (không được gì) – tạm khi chưa bán được cá; kỷ lục giữ nguyên.
        public FarmResult ReleaseFish(int basketIndex)
        {
            List<CaughtFish> basket = State.Fishing.Basket;
            if (basketIndex < 0 || basketIndex >= basket.Count)
                return FarmResult.UnknownFish;
            basket.RemoveAt(basketIndex);
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Mở thêm 1 ô giỏ (ô kế tiếp, lần lượt).
        public FarmResult UnlockBasketSlot()
        {
            if (!FishingUnlocked)
                return FarmResult.FishingLocked;
            if (!CanUnlockBasketSlot)
                return FarmResult.BasketMaxed;
            long price = NextBasketSlotPrice;
            if (State.Coins < price)
                return FarmResult.NotEnoughCoins;

            State.Coins -= price;
            State.Fishing.BasketSlots = BasketCapacity + 1;
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Tặng cần + mồi lần đầu mở câu cá (khi nạp hoặc khi lên đủ cấp). Gọi lại nhiều lần không sao.
        private void GrantFishingStarter()
        {
            FishingState fishing = State.Fishing;
            if (fishing.StarterGranted || !FishingUnlocked)
                return;

            FishingDef def = Definition.Fishing;
            if (def.StarterRodId != null && def.TryGetRod(def.StarterRodId, out RodDef rod) && !fishing.TryGetRod(rod.Id, out _))
                fishing.Rods.Add(new RodState { Id = rod.Id, Durability = rod.MaxDurability });
            if (def.StarterRodId != null)
                fishing.EquippedRodId ??= def.StarterRodId;
            if (def.StarterBaitId != null && def.StarterBaitCount > 0)
                FarmState.AddCount(fishing.Baits, def.StarterBaitId, def.StarterBaitCount);
            fishing.StarterGranted = true;
        }

        // Thu hoạch có khi đào được mồi (giun).
        private void TryFindBait()
        {
            FishingDef def = Definition.Fishing;
            if (!FishingUnlocked || def.WormBaitId == null || def.WormChancePercent <= 0)
                return;
            if (SeededRandom.Range(ref State.RandomState, 0, 100) >= def.WormChancePercent)
                return;

            int amount = SeededRandom.Range(ref State.RandomState, def.WormMin, def.WormMax + 1);
            if (amount <= 0)
                return;
            FarmState.AddCount(State.Fishing.Baits, def.WormBaitId, amount);
            BaitFound?.Invoke(def.WormBaitId, amount);
        }

        private static void RemoveInvalid<T>(Dictionary<string, T> map, Func<T, bool> invalid) where T : class
        {
            List<string> bad = null;
            foreach (KeyValuePair<string, T> pair in map)
            {
                if (string.IsNullOrEmpty(pair.Key) || pair.Value == null || invalid(pair.Value))
                    (bad ??= new List<string>()).Add(pair.Key);
            }
            if (bad == null)
                return;
            foreach (string key in bad)
                map.Remove(key);
        }

        // Nạp save: giỏ / mức giỏ / cần hợp lệ với data.
        private void NormalizeFishing()
        {
            FishingState fishing = State.Fishing ??= new FishingState();
            fishing.Rods ??= new System.Collections.Generic.List<RodState>();
            fishing.Rods.RemoveAll(r => r == null || string.IsNullOrEmpty(r.Id));
            foreach (RodState rod in fishing.Rods)
            {
                int max = GetRodMaxDurability(rod.Id);
                if (max > 0)
                    rod.Durability = Math.Min(rod.Durability, max);
            }
            fishing.Baits ??= new System.Collections.Generic.Dictionary<string, int>();
            fishing.Basket ??= new System.Collections.Generic.List<CaughtFish>();
            fishing.Basket.RemoveAll(f => f == null || string.IsNullOrEmpty(f.FishId) || f.Grams <= 0);
            fishing.FishDex ??= new Dictionary<string, FishDexEntry>();
            RemoveInvalid(fishing.FishDex, e => e.Count <= 0 || e.MaxGrams <= 0);
            fishing.JunkDex ??= new Dictionary<string, JunkDexEntry>();
            RemoveInvalid(fishing.JunkDex, e => e.Count <= 0);
            fishing.JunkBasket ??= new List<CaughtJunk>();
            // Rác không còn trong data: bỏ (không chiếm ô giỏ mãi).
            fishing.JunkBasket.RemoveAll(j => j == null || !Definition.Fishing.TryGetJunk(j.JunkId, out _));
            fishing.ShinyDex ??= new System.Collections.Generic.List<ShinyRecord>();
            fishing.ShinyDex.RemoveAll(r => r == null || string.IsNullOrEmpty(r.FishId) || r.Kind == ShinyKind.None);
            fishing.BasketSlots = Math.Max(Definition.Fishing.BasketStartSlots,
                Math.Min(fishing.BasketSlots, Definition.Fishing.BasketMaxSlots));
            if (fishing.EquippedRodId != null && !fishing.TryGetRod(fishing.EquippedRodId, out _))
                fishing.EquippedRodId = null;
            if (fishing.EquippedRodId == null)
            {
                foreach (RodState rod in fishing.Rods)
                {
                    if (Definition.Fishing.TryGetRod(rod.Id, out _))
                    {
                        fishing.EquippedRodId = rod.Id;
                        break;
                    }
                }
            }
        }
    }
}
