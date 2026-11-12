using System;

namespace VeQue.Logic
{
    // Câu cá (MVP-Plan Phần 10): cần, mồi, giỏ cá, phiên câu. Mọi thay đổi tiến trình đi qua đây như phần trồng trọt.
    public sealed partial class Farm
    {
        // Báo mồi đào được khi thu hoạch (id mồi, số lượng); phát trước Changed của lệnh đó.
        public event Action<string, int> BaitFound;

        public FishingDef FishingDefinition => Definition.Fishing;

        public bool FishingUnlocked => Definition.Fishing.IsEnabled && State.Level >= Definition.Fishing.UnlockLevel;

        public int BasketCapacity
        {
            get
            {
                var capacities = Definition.Fishing.BasketCapacities;
                return capacities[Math.Min(State.Fishing.BasketLevel, capacities.Count - 1)];
            }
        }

        public bool IsBasketFull => State.Fishing.Basket.Count >= BasketCapacity;

        public bool CanUpgradeBasket => State.Fishing.BasketLevel < Definition.Fishing.BasketCapacities.Count - 1;

        // Giá lên mức giỏ kế tiếp; -1 nếu đã tối đa.
        public long NextBasketPrice => CanUpgradeBasket ? Definition.Fishing.BasketUpgradePrices[State.Fishing.BasketLevel + 1] : -1;

        public bool TryGetEquippedRod(out RodDef def, out RodState rod)
        {
            rod = null;
            def = null;
            FishingState fishing = State.Fishing;
            return fishing.EquippedRodId != null
                && Definition.Fishing.TryGetRod(fishing.EquippedRodId, out def)
                && fishing.TryGetRod(fishing.EquippedRodId, out rod);
        }

        // Độ bền tối đa thực tế của cần người chơi đang có (giảm theo số lần sửa); -1 nếu không có.
        public int GetRodMaxDurability(string rodId)
        {
            if (!Definition.Fishing.TryGetRod(rodId, out RodDef def) || !State.Fishing.TryGetRod(rodId, out RodState rod))
                return -1;
            return Definition.Fishing.Tuning.MaxDurabilityAfterRepairs(def.MaxDurability, rod.RepairCount);
        }

        // Còn sửa được nữa không (chưa tính xu / cần có hỏng hay không).
        public bool CanRepairAgain(string rodId)
            => State.Fishing.TryGetRod(rodId, out RodState rod) && Definition.Fishing.Tuning.CanRepairAgain(rod.RepairCount);

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
        // Khung giờ tính lúc ném theo giờ địa phương.
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

            DaySlot slot = DaySlots.Get(_clock.UtcNowSeconds, State.UtcOffsetSeconds);
            uint seed = SeededRandom.Next(ref State.RandomState);
            session = new FishingSession(def, rodDef, bait, slot, power, seed);
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Áp kết quả phiên đã xong: cá vào giỏ + XP + kỷ lục; đứt dây (bấm sai khi kéo) trừ thêm SnapDurabilityCost
        // (mặc định 0) và có thể gãy cần (độ bền càng thấp càng dễ gãy). Cá thoát / sợ / ăn mồi / trượt: không thêm gì.
        public FarmResult CompleteFishing(FishingSession session)
        {
            if (session == null || session.Phase != FishingPhase.Finished)
                return FarmResult.FishingNotFinished;
            if (session.Applied)
                return FarmResult.FishingAlreadyApplied;
            session.Applied = true;

            FishingState fishing = State.Fishing;
            if (session.Outcome == FishingOutcome.Caught)
            {
                fishing.Basket.Add(new CaughtFish
                {
                    FishId = session.Fish.Id,
                    Grams = session.Grams,
                    CaughtAt = _clock.UtcNowSeconds,
                    Slot = session.Slot,
                });
                if (session.Grams > fishing.GetRecord(session.Fish.Id))
                    fishing.RecordGrams[session.Fish.Id] = session.Grams;
                AddXp(session.Xp);
            }
            else if (session.Outcome == FishingOutcome.LineSnapped && fishing.TryGetRod(session.Rod.Id, out RodState rod))
            {
                FishingTuning tuning = Definition.Fishing.Tuning;
                rod.Durability = Math.Max(0, rod.Durability - tuning.SnapDurabilityCost);
                int breakPercent = tuning.SnapBreakPercentAt(rod.Durability, GetRodMaxDurability(rod.Id));
                if (SeededRandom.Range(ref State.RandomState, 0, 100) < breakPercent)
                    rod.Snapped = true;
            }

            NotifyChanged();
            return FarmResult.Ok;
        }

        // Mua cần mới, hoặc mua lại cần đang có khi nó hỏng / đã từng sửa (cần về như mới).
        public FarmResult BuyRod(string rodId)
        {
            if (!FishingUnlocked)
                return FarmResult.FishingLocked;
            if (!Definition.Fishing.TryGetRod(rodId, out RodDef def))
                return FarmResult.UnknownRod;
            bool owned = State.Fishing.TryGetRod(rodId, out RodState rod);
            if (owned && !rod.IsBroken && rod.RepairCount == 0)
                return FarmResult.AlreadyOwned;
            if (State.Level < def.UnlockLevel)
                return FarmResult.LevelTooLow;
            if (State.Coins < def.Price)
                return FarmResult.NotEnoughCoins;

            State.Coins -= def.Price;
            if (owned)
            {
                rod.Durability = def.MaxDurability;
                rod.Snapped = false;
                rod.RepairCount = 0;
            }
            else
            {
                State.Fishing.Rods.Add(new RodState { Id = rodId, Durability = def.MaxDurability });
            }
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

        // Sửa: hết gãy, đầy độ bền nhưng độ bền tối đa giảm theo số lần sửa.
        // Cần còn nguyên → RodNotDamaged; độ bền tối đa đã dưới mức tối thiểu → RodWornOut (mua lại).
        public FarmResult RepairRod(string rodId)
        {
            if (!Definition.Fishing.TryGetRod(rodId, out RodDef def) || !State.Fishing.TryGetRod(rodId, out RodState rod))
                return FarmResult.UnknownRod;
            FishingTuning tuning = Definition.Fishing.Tuning;
            if (!rod.Snapped && rod.Durability >= tuning.MaxDurabilityAfterRepairs(def.MaxDurability, rod.RepairCount))
                return FarmResult.RodNotDamaged;
            if (!tuning.CanRepairAgain(rod.RepairCount))
                return FarmResult.RodWornOut;
            if (State.Coins < def.RepairPrice)
                return FarmResult.NotEnoughCoins;

            State.Coins -= def.RepairPrice;
            rod.RepairCount++;
            rod.Durability = tuning.MaxDurabilityAfterRepairs(def.MaxDurability, rod.RepairCount);
            rod.Snapped = false;
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
            long cost = bait.Price * amount;
            if (State.Coins < cost)
                return FarmResult.NotEnoughCoins;

            State.Coins -= cost;
            FarmState.AddCount(State.Fishing.Baits, baitId, amount);
            NotifyChanged();
            return FarmResult.Ok;
        }

        public FarmResult UpgradeBasket()
        {
            if (!FishingUnlocked)
                return FarmResult.FishingLocked;
            if (!CanUpgradeBasket)
                return FarmResult.BasketMaxed;
            long price = NextBasketPrice;
            if (State.Coins < price)
                return FarmResult.NotEnoughCoins;

            State.Coins -= price;
            State.Fishing.BasketLevel++;
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

        // Nạp save: giỏ / mức giỏ / cần hợp lệ với data.
        private void NormalizeFishing()
        {
            FishingState fishing = State.Fishing ??= new FishingState();
            fishing.Rods ??= new System.Collections.Generic.List<RodState>();
            fishing.Rods.RemoveAll(r => r == null || string.IsNullOrEmpty(r.Id));
            foreach (RodState rod in fishing.Rods)
            {
                rod.RepairCount = Math.Max(0, rod.RepairCount);
                int max = GetRodMaxDurability(rod.Id);
                if (max > 0)
                    rod.Durability = Math.Min(rod.Durability, max);
            }
            fishing.Baits ??= new System.Collections.Generic.Dictionary<string, int>();
            fishing.Basket ??= new System.Collections.Generic.List<CaughtFish>();
            fishing.Basket.RemoveAll(f => f == null || string.IsNullOrEmpty(f.FishId) || f.Grams <= 0);
            fishing.RecordGrams ??= new System.Collections.Generic.Dictionary<string, int>();
            int maxLevel = Math.Max(0, Definition.Fishing.BasketCapacities.Count - 1);
            fishing.BasketLevel = Math.Max(0, Math.Min(fishing.BasketLevel, maxLevel));
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
