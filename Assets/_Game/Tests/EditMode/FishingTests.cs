using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    // Khung giờ, chọn cá, ném / giật / kéo, độ bền, cần / mồi / giỏ, đào giun, save (MVP-Plan Phần 10 giai đoạn 3).
    public class FishingTests
    {
        private const string Zone1 = "khu1";
        private const string RauMuong = "rau_muong";
        private const long GrowSeconds = 60;

        private const string Ro = "ca_ro";          // Thường, mọi giờ, mọi độ sâu, ưa giun.
        private const string Loc = "ca_loc";        // Hiếm, bình minh + hoàng hôn, vừa / xa, ưa tép.
        private const string ChepVang = "chep_vang"; // Huyền thoại, chỉ bình minh, chỉ xa.

        private const string Tre = "can_tre";
        private const string Carbon = "can_carbon";
        private const string Giun = "giun";
        private const string Tep = "tep";
        private const string MoiThom = "moi_thom";

        private const int UnlockLevel = 1;
        private static readonly int[] Capacities = { 8, 10, 12 };
        private static readonly long[] UpgradePrices = { 0, 300, 1000 };

        // 1/1/2027 00:00 UTC là thứ Sáu; dùng mốc ngày tròn để đặt giờ địa phương dễ đọc.
        private const long Midnight = 1798761600;
        private static readonly PlotId Plot0 = new PlotId(Zone1, 0, 0, 0);

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock { UtcNowSeconds = Midnight + 8 * 3600 };
        }

        // ---------- Khung giờ ----------

        [TestCase(0, DaySlot.Night)]
        [TestCase(4, DaySlot.Night)]
        [TestCase(5, DaySlot.Dawn)]
        [TestCase(6, DaySlot.Dawn)]
        [TestCase(7, DaySlot.Morning)]
        [TestCase(11, DaySlot.Noon)]
        [TestCase(14, DaySlot.Afternoon)]
        [TestCase(17, DaySlot.Sunset)]
        [TestCase(19, DaySlot.Evening)]
        [TestCase(22, DaySlot.Evening)]
        [TestCase(23, DaySlot.Night)]
        public void DaySlot_FromHour(int hour, DaySlot expected)
        {
            Assert.AreEqual(expected, DaySlots.FromHour(hour));
        }

        [Test]
        public void DaySlot_Boundary_4h59_Night_5h00_Dawn()
        {
            Assert.AreEqual(DaySlot.Night, DaySlots.Get(Midnight + 5 * 3600 - 1, 0));
            Assert.AreEqual(DaySlot.Dawn, DaySlots.Get(Midnight + 5 * 3600, 0));
        }

        [Test]
        public void DaySlot_UsesUtcOffset()
        {
            // 22h UTC + 7 giờ = 5h sáng hôm sau ở Việt Nam.
            Assert.AreEqual(DaySlot.Dawn, DaySlots.Get(Midnight + 22 * 3600, 7 * 3600));
            // 2h UTC − 5 giờ = 21h hôm trước.
            Assert.AreEqual(DaySlot.Evening, DaySlots.Get(Midnight + 2 * 3600, -5 * 3600));
        }

        // ---------- Mở câu cá, quà tặng ----------

        [Test]
        public void NoFishingDef_Locked()
        {
            Farm farm = CreateFarm(withFishing: false);

            Assert.IsFalse(farm.FishingUnlocked);
            Assert.AreEqual(FarmResult.FishingLocked, farm.CanCast(Giun));
        }

        [Test]
        public void Unlock_GrantsStarterOnce()
        {
            Farm farm = CreateFarm();

            Assert.IsTrue(farm.FishingUnlocked);
            Assert.IsTrue(farm.TryGetEquippedRod(out RodDef def, out RodState rod));
            Assert.AreEqual(Tre, def.Id);
            Assert.AreEqual(def.MaxDurability, rod.Durability);
            Assert.AreEqual(10, farm.State.Fishing.GetBaits(Giun));

            Farm reloaded = RoundTrip(farm);
            Assert.AreEqual(1, reloaded.State.Fishing.Rods.Count);
            Assert.AreEqual(10, reloaded.State.Fishing.GetBaits(Giun));
        }

        [Test]
        public void UnlockLevelHigher_GrantsOnLevelUp()
        {
            Farm farm = CreateFarm(unlockLevel: 2);
            Assert.IsFalse(farm.FishingUnlocked);
            Assert.AreEqual(0, farm.State.Fishing.Rods.Count);

            farm.State.Xp = 2;
            PlantAndHarvest(farm);

            Assert.AreEqual(2, farm.State.Level);
            Assert.IsTrue(farm.FishingUnlocked);
            Assert.AreEqual(1, farm.State.Fishing.Rods.Count);
        }

        // ---------- Ném ----------

        [Test]
        public void Cast_ConsumesBaitAndDurability()
        {
            Farm farm = CreateFarm();
            farm.TryGetEquippedRod(out RodDef def, out RodState rod);

            Assert.AreEqual(FarmResult.Ok, farm.Cast(Giun, 0.6f, out FishingSession session));

            Assert.IsNotNull(session);
            Assert.AreEqual(9, farm.State.Fishing.GetBaits(Giun));
            Assert.AreEqual(def.MaxDurability - 1, rod.Durability);
            Assert.AreEqual(FishingPhase.Waiting, session.Phase);
        }

        [Test]
        public void Cast_Blocked_NoBait_BrokenRod_FullBasket()
        {
            Farm farm = CreateFarm();
            Assert.AreEqual(FarmResult.NotEnoughBait, farm.Cast(Tep, 0.6f, out _));

            farm.TryGetEquippedRod(out _, out RodState rod);
            rod.Snapped = true;
            Assert.AreEqual(FarmResult.RodBroken, farm.Cast(Giun, 0.6f, out _));
            rod.Snapped = false;

            for (int i = 0; i < Capacities[0]; i++)
                farm.State.Fishing.Basket.Add(new CaughtFish { FishId = Ro, Grams = 100 });
            Assert.AreEqual(FarmResult.BasketFull, farm.Cast(Giun, 0.6f, out _));
            Assert.AreEqual(10, farm.State.Fishing.GetBaits(Giun));
        }

        [TestCase(0.9f, CastQuality.Perfect)]
        [TestCase(0.85f, CastQuality.Good)]
        [TestCase(0.5f, CastQuality.Normal)]
        [TestCase(0.1f, CastQuality.Weak)]
        public void Classify_ByPower(float power, CastQuality expected)
        {
            var rod = new RodDef(Tre, 1, 0.03f, 1.2f, 100, 0, 0, 30, 0, 1, 50);

            Assert.AreEqual(expected, FishingSession.Classify(power, rod, new FishingTuning()));
        }

        // ---------- Chọn cá ----------

        [Test]
        public void FishWithZeroSlotFactor_NeverAppears()
        {
            SetLocalHour(12); // Trưa: cá lóc và chép vàng = 0.
            Farm farm = CreateFarm(baits: 1000);

            HashSet<string> seen = CastMany(farm, MoiThom, 0.9f, 300);

            CollectionAssert.AreEquivalent(new[] { Ro }, seen);
        }

        [Test]
        public void Dawn_PerfectFar_LegendaryCanAppear()
        {
            SetLocalHour(5);
            Farm farm = CreateFarm(baits: 2000);

            HashSet<string> seen = CastMany(farm, MoiThom, 0.9f, 1500);

            CollectionAssert.Contains(seen, ChepVang);
        }

        [Test]
        public void WeakCast_OnlyCommon()
        {
            SetLocalHour(5);
            Farm farm = CreateFarm(baits: 1000);

            HashSet<string> seen = CastMany(farm, MoiThom, 0.05f, 300);

            CollectionAssert.AreEquivalent(new[] { Ro }, seen);
        }

        [Test]
        public void PreferredBait_IncreasesChance()
        {
            SetLocalHour(5);
            int withTep = CountFish(CreateFarm(baits: 1000, randomState: 7), Tep, Loc, 600);
            int withGiun = CountFish(CreateFarm(baits: 1000, randomState: 7), Giun, Loc, 600);

            Assert.Greater(withTep, withGiun);
        }

        [Test]
        public void SameRandomState_SameSession()
        {
            Farm a = CreateFarm(randomState: 99);
            Farm b = CreateFarm(randomState: 99);

            a.Cast(Giun, 0.7f, out FishingSession sa);
            b.Cast(Giun, 0.7f, out FishingSession sb);

            Assert.AreEqual(sa.Fish.Id, sb.Fish.Id);
            Assert.AreEqual(sa.BiteTime, sb.BiteTime);
            CollectionAssert.AreEqual(sa.NibbleTimes, sb.NibbleTimes);
        }

        [Test]
        public void Schedule_NibblesBeforeBite()
        {
            Farm farm = CreateFarm(baits: 200);
            for (int i = 0; i < 100; i++)
            {
                farm.Cast(Giun, 0.6f, out FishingSession session);
                foreach (float nibble in session.NibbleTimes)
                    Assert.Less(nibble, session.BiteTime);
                Assert.GreaterOrEqual(session.BiteTime, 0.8f);
                session.Timeout();
                farm.CompleteFishing(session);
                RefreshRod(farm);
            }
        }

        // ---------- Giật ----------

        [Test]
        public void Strike_Early_Scared()
        {
            FishingSession session = CastOne(CreateFarm());

            Assert.AreEqual(FishingOutcome.Scared, session.Strike(session.BiteTime - 0.01f));
            Assert.AreEqual(FishingPhase.Finished, session.Phase);
        }

        [Test]
        public void Strike_Late_BaitStolen()
        {
            FishingSession session = CastOne(CreateFarm());

            Assert.AreEqual(FishingOutcome.BaitStolen, session.Strike(session.HookDeadline + 0.01f));
        }

        [Test]
        public void Strike_InWindow_NoSlip_Hooked()
        {
            FishingSession session = CastOne(CreateFarm(roSlip: 0));

            Assert.AreEqual(FishingOutcome.None, session.Strike(session.BiteTime + 0.1f));
            Assert.AreEqual(FishingPhase.Pulling, session.Phase);
            Assert.IsNotNull(session.Pull);
            Assert.That(session.Pull.Count, Is.InRange(1, session.Fish.MaxArrows));
            // Cần tre không thêm thời gian.
            Assert.AreEqual(session.Fish.PullSeconds, session.Pull.TimeLimitSeconds, 0.0001f);
        }

        [Test]
        public void Strike_InWindow_FullSlip_Slipped()
        {
            FishingSession session = CastOne(CreateFarm(roSlip: 100));

            Assert.AreEqual(FishingOutcome.Slipped, session.Strike(session.BiteTime));
        }

        // ---------- Kéo (1 lượt, chốt 13/11/2026) ----------

        [TestCase(0.0f, PullGrade.Perfect)]
        [TestCase(1.6f, PullGrade.Perfect)]   // 40% của 4 giây.
        [TestCase(1.7f, PullGrade.Good)]
        [TestCase(2.8f, PullGrade.Good)]      // 70%.
        [TestCase(2.9f, PullGrade.Normal)]
        [TestCase(4.0f, PullGrade.Normal)]
        public void GradeFor_FasterIsBetter(float seconds, PullGrade expected)
        {
            Assert.AreEqual(expected, FishingSession.GradeFor(seconds, 4f, new FishingTuning()));
        }

        [Test]
        public void Pull_CompleteInTime_Caught()
        {
            FishingSession session = HookOne(CreateFarm(roSlip: 0));

            Assert.AreEqual(FishingOutcome.Caught, session.CompletePull(0.5f));
            Assert.AreEqual(PullGrade.Perfect, session.Grade);
            Assert.That(session.Grams, Is.InRange(session.Fish.MinGrams, session.Fish.MaxGrams));
            Assert.Greater(session.Price, 0);
            Assert.Greater(session.Xp, 0);
        }

        [Test]
        public void Pull_TooSlow_Escaped()
        {
            FishingSession late = HookOne(CreateFarm(roSlip: 0));
            Assert.AreEqual(FishingOutcome.Escaped, late.CompletePull(late.Pull.TimeLimitSeconds + 0.1f));

            FishingSession timeout = HookOne(CreateFarm(roSlip: 0));
            Assert.AreEqual(FishingOutcome.Escaped, timeout.PullTimeout());
            Assert.AreEqual(FishingPhase.Finished, timeout.Phase);
        }

        [Test]
        public void Pull_WrongArrow_SnapsLine()
        {
            FishingSession session = HookOne(CreateFarm(roSlip: 0));

            Assert.AreEqual(FishingOutcome.LineSnapped, session.WrongArrow());
        }

        [Test]
        public void Pull_Perfect_HeavierAndMoreXpThanNormal()
        {
            // Cùng trạng thái ngẫu nhiên → cùng cá, cùng số ngẫu nhiên cân nặng; chỉ khác hạng.
            FishingSession fast = HookOne(CreateFarm(roSlip: 0));
            FishingSession slow = HookOne(CreateFarm(roSlip: 0));
            Assert.AreEqual(fast.Fish.Id, slow.Fish.Id);

            fast.CompletePull(0.1f);
            slow.CompletePull(slow.Pull.TimeLimitSeconds * 0.95f);

            Assert.AreEqual(PullGrade.Perfect, fast.Grade);
            Assert.AreEqual(PullGrade.Normal, slow.Grade);
            Assert.GreaterOrEqual(fast.Grams, slow.Grams);
            Assert.GreaterOrEqual(fast.Price, slow.Price);
            Assert.GreaterOrEqual(fast.Xp, slow.Xp);
        }

        [Test]
        public void Pull_PerfectXpBonusFiftyPercent()
        {
            // Cá rô XP gốc 1 quá nhỏ để thấy +50%: dùng công thức trên giá trị lớn qua cá XP 10.
            FishingSession session = HookOne(CreateFarm(roSlip: 0, roXp: 10));
            session.CompletePull(0.1f);

            int span = session.Fish.MaxGrams - session.Fish.MinGrams;
            float ratio = (session.Grams - session.Fish.MinGrams) / (float)span;
            float percent = 60f + 80f * ratio;
            Assert.AreEqual((int)System.Math.Round(10 * percent / 100f * 1.5f), session.Xp);
        }

        [Test]
        public void Pull_RodBonusTime()
        {
            Farm farm = CreateFarm(roSlip: 0, coins: 100000);
            farm.State.Level = 9;
            farm.BuyRod(Carbon);
            farm.EquipRod(Carbon);

            FishingSession session = HookOne(farm);

            // Cần carbon +30% thời gian.
            Assert.AreEqual(session.Fish.PullSeconds * 1.3f, session.Pull.TimeLimitSeconds, 0.0001f);
        }

        [Test]
        public void RedArrows_ExpectOpposite()
        {
            FishingSession session = HookOne(CreateFarm(roSlip: 0, roRed: true));
            PullRound round = session.Pull;

            for (int i = 0; i < round.Count; i++)
            {
                Assert.IsTrue(round.Red[i], "Tỉ lệ mũi tên đỏ 100%");
                Assert.AreEqual(Opposite(round.Arrows[i]), round.Expected(i));
            }
        }

        [Test]
        public void NormalArrows_ExpectSame()
        {
            FishingSession session = HookOne(CreateFarm(roSlip: 0));
            PullRound round = session.Pull;

            for (int i = 0; i < round.Count; i++)
            {
                Assert.IsFalse(round.Red[i]);
                Assert.AreEqual(round.Arrows[i], round.Expected(i));
            }
        }

        // ---------- Áp kết quả ----------

        [Test]
        public void Complete_Caught_AddsBasketXpRecord()
        {
            Farm farm = CreateFarm(roSlip: 0);
            FishingSession session = CatchOne(farm);
            long xp = farm.State.Xp;
            int level = farm.State.Level;

            Assert.AreEqual(FarmResult.Ok, farm.CompleteFishing(session));

            Assert.AreEqual(1, farm.State.Fishing.Basket.Count);
            Assert.AreEqual(session.Fish.Id, farm.State.Fishing.Basket[0].FishId);
            Assert.AreEqual(session.Grams, farm.State.Fishing.GetRecord(session.Fish.Id));
            Assert.IsTrue(farm.State.Level > level || farm.State.Xp == xp + session.Xp);
            Assert.AreEqual(FarmResult.FishingAlreadyApplied, farm.CompleteFishing(session));
        }

        [Test]
        public void Complete_Unfinished_Rejected()
        {
            Farm farm = CreateFarm();
            FishingSession session = CastOne(farm);

            Assert.AreEqual(FarmResult.FishingNotFinished, farm.CompleteFishing(session));
        }

        [Test]
        public void Record_KeepsHeaviest()
        {
            Farm farm = CreateFarm(roSlip: 0, baits: 100);
            int best = 0;
            for (int i = 0; i < 6; i++)
            {
                farm.State.Fishing.Basket.Clear();
                FishingSession session = CatchOne(farm);
                farm.CompleteFishing(session);
                if (session.Fish.Id == Ro)
                    best = System.Math.Max(best, session.Grams);
            }

            Assert.AreEqual(best, farm.State.Fishing.GetRecord(Ro));
        }

        [Test]
        public void Complete_Snap_OnlyCastCostAndCanBreak()
        {
            Farm farm = CreateFarm(roSlip: 0, snapBreakPercent: 100);
            farm.TryGetEquippedRod(out RodDef def, out RodState rod);
            FishingSession session = HookOne(farm);
            session.WrongArrow();

            farm.CompleteFishing(session);

            // Chốt 13/11/2026: mọi mức trừ là 1 (lúc ném); đứt dây không trừ thêm, chỉ có thể gãy.
            Assert.AreEqual(def.MaxDurability - 1, rod.Durability);
            Assert.IsTrue(rod.IsBroken);
            Assert.AreEqual(FarmResult.RodBroken, farm.CanCast(Giun));
        }

        [Test]
        public void Durability_ZeroBreaksRod()
        {
            Farm farm = CreateFarm(baits: 100);
            farm.TryGetEquippedRod(out _, out RodState rod);
            rod.Durability = 1;

            FishingSession session = CastOne(farm);
            session.Timeout();
            farm.CompleteFishing(session);

            Assert.IsTrue(rod.IsBroken);
            Assert.AreEqual(FarmResult.RodBroken, farm.CanCast(Giun));
        }

        // ---------- Cửa hàng ----------

        [Test]
        public void Repair_RestoresAndCosts()
        {
            Farm farm = CreateFarm(coins: 1000);
            farm.TryGetEquippedRod(out RodDef def, out RodState rod);
            Assert.AreEqual(FarmResult.RodNotDamaged, farm.RepairRod(Tre));

            rod.Durability = 0;
            rod.Snapped = true;
            Assert.AreEqual(FarmResult.Ok, farm.RepairRod(Tre));

            // Cần tre gốc 30: sửa lần 1 còn 90% = 27.
            Assert.AreEqual(27, rod.Durability);
            Assert.AreEqual(1, rod.RepairCount);
            Assert.AreEqual(27, farm.GetRodMaxDurability(Tre));
            Assert.IsFalse(rod.IsBroken);
            Assert.AreEqual(1000 - def.RepairPrice, farm.State.Coins);
            Assert.AreEqual(FarmResult.RodNotDamaged, farm.RepairRod(Tre));
        }

        [Test]
        public void Repair_EachTimeLowersMax_UntilBelow15Percent()
        {
            Farm farm = CreateFarm(coins: 100000);
            farm.TryGetEquippedRod(out _, out RodState rod);
            var expected = new[] { 27, 24, 21, 18, 15, 12, 9, 6, 3 };

            for (int i = 0; i < expected.Length; i++)
            {
                rod.Durability = 0;
                Assert.AreEqual(FarmResult.Ok, farm.RepairRod(Tre), $"Lần sửa {i + 1}");
                Assert.AreEqual(expected[i], rod.Durability, $"Lần sửa {i + 1}");
            }

            // Còn 10% < 15%: hết sửa, không mất xu.
            rod.Durability = 0;
            long coins = farm.State.Coins;
            Assert.IsFalse(farm.CanRepairAgain(Tre));
            Assert.AreEqual(FarmResult.RodWornOut, farm.RepairRod(Tre));
            Assert.AreEqual(coins, farm.State.Coins);
            Assert.AreEqual(9, rod.RepairCount);
        }

        [Test]
        public void BuyRod_AgainWhenBrokenOrRepaired_LikeNew()
        {
            Farm farm = CreateFarm(coins: 100000);
            farm.State.Level = 9;
            Assert.AreEqual(FarmResult.Ok, farm.BuyRod(Carbon));
            Assert.IsTrue(farm.State.Fishing.TryGetRod(Carbon, out RodState rod));

            // Còn dùng được (dù đã mòn chút) và chưa sửa: không mua lại.
            rod.Durability -= 3;
            Assert.AreEqual(FarmResult.AlreadyOwned, farm.BuyRod(Carbon));

            // Gãy: mua lại được, vẫn chỉ 1 cần.
            rod.Snapped = true;
            long coins = farm.State.Coins;
            Assert.AreEqual(FarmResult.Ok, farm.BuyRod(Carbon));
            Assert.AreEqual(coins - 6000, farm.State.Coins);
            Assert.IsFalse(rod.IsBroken);
            Assert.AreEqual(120, rod.Durability);
            Assert.AreEqual(1, CountRods(farm, Carbon));

            // Đã sửa: mua lại được, số lần sửa về 0, độ bền tối đa về gốc.
            rod.Durability = 0;
            Assert.AreEqual(FarmResult.Ok, farm.RepairRod(Carbon));
            Assert.AreEqual(108, farm.GetRodMaxDurability(Carbon));
            Assert.AreEqual(FarmResult.Ok, farm.BuyRod(Carbon));
            Assert.AreEqual(0, rod.RepairCount);
            Assert.AreEqual(120, rod.Durability);
            Assert.AreEqual(120, farm.GetRodMaxDurability(Carbon));
        }

        [Test]
        public void BuyRod_WornOut_CanBuyAgain()
        {
            Farm farm = CreateFarm(coins: 100000);
            farm.TryGetEquippedRod(out _, out RodState rod);
            rod.RepairCount = 9;
            rod.Durability = 0;
            Assert.AreEqual(FarmResult.RodWornOut, farm.RepairRod(Tre));

            Assert.AreEqual(FarmResult.Ok, farm.BuyRod(Tre));
            Assert.AreEqual(30, rod.Durability);
            Assert.AreEqual(FarmResult.Ok, farm.CanCast(Giun));
        }

        [TestCase(30, 30, 20)]
        [TestCase(15, 30, 40)]
        [TestCase(0, 30, 60)]
        [TestCase(-5, 30, 60)]
        [TestCase(40, 30, 20)]
        public void SnapBreakPercent_RisesAsDurabilityDrops(int durability, int max, int expected)
        {
            Assert.AreEqual(expected, new FishingTuning().SnapBreakPercentAt(durability, max));
        }

        [Test]
        public void Snap_LowDurability_BreaksMoreOften()
        {
            // Tỉ lệ 0% (đầy) → 100% (về 0) để thấy rõ chênh lệch.
            int full = CountSnapBreaks(30);
            int low = CountSnapBreaks(2);

            // Đầy: 30 → còn 29 sau ném ≈ 3%; thấp: 2 → còn 1 ≈ 97% (đứt dây không trừ thêm độ bền).
            Assert.Less(full, 80);
            Assert.Greater(low, 170);
        }

        [Test]
        public void BuyRod_LevelCoinsOwnedEquip()
        {
            Farm farm = CreateFarm(coins: 100000);
            Assert.AreEqual(FarmResult.LevelTooLow, farm.BuyRod(Carbon));

            farm.State.Level = 9;
            Assert.AreEqual(FarmResult.Ok, farm.BuyRod(Carbon));
            Assert.AreEqual(FarmResult.AlreadyOwned, farm.BuyRod(Carbon));
            Assert.AreEqual(Tre, farm.State.Fishing.EquippedRodId);

            Assert.AreEqual(FarmResult.Ok, farm.EquipRod(Carbon));
            Assert.AreEqual(Carbon, farm.State.Fishing.EquippedRodId);
            Assert.AreEqual(FarmResult.UnknownRod, farm.EquipRod("khong_co"));
        }

        [Test]
        public void BuyBait_Coins()
        {
            Farm farm = CreateFarm(coins: 20);

            Assert.AreEqual(FarmResult.Ok, farm.BuyBait(Tep, 4));
            Assert.AreEqual(4, farm.State.Fishing.GetBaits(Tep));
            Assert.AreEqual(0, farm.State.Coins);
            Assert.AreEqual(FarmResult.NotEnoughCoins, farm.BuyBait(Tep, 1));
            Assert.AreEqual(FarmResult.InvalidAmount, farm.BuyBait(Tep, 0));
            Assert.AreEqual(FarmResult.UnknownBait, farm.BuyBait(Giun, 1));
        }

        [Test]
        public void UpgradeBasket_UntilMax()
        {
            Farm farm = CreateFarm(coins: 2000);
            Assert.AreEqual(8, farm.BasketCapacity);
            Assert.AreEqual(300, farm.NextBasketPrice);

            Assert.AreEqual(FarmResult.Ok, farm.UpgradeBasket());
            Assert.AreEqual(10, farm.BasketCapacity);
            Assert.AreEqual(FarmResult.Ok, farm.UpgradeBasket());
            Assert.AreEqual(12, farm.BasketCapacity);
            Assert.AreEqual(FarmResult.BasketMaxed, farm.UpgradeBasket());
            Assert.AreEqual(700, farm.State.Coins);
        }

        // ---------- Đào giun ----------

        [Test]
        public void Harvest_FindsWorms()
        {
            Farm farm = CreateFarm(wormChance: 100);
            string foundId = null;
            int found = 0;
            farm.BaitFound += (id, count) => { foundId = id; found = count; };

            PlantAndHarvest(farm);

            Assert.AreEqual(Giun, foundId);
            Assert.That(found, Is.InRange(1, 2));
            Assert.AreEqual(10 + found, farm.State.Fishing.GetBaits(Giun));
        }

        [Test]
        public void Harvest_NoWormChance_NoWorms()
        {
            Farm farm = CreateFarm(wormChance: 0);
            int events = 0;
            farm.BaitFound += (_, __) => events++;

            PlantAndHarvest(farm);

            Assert.AreEqual(0, events);
            Assert.AreEqual(10, farm.State.Fishing.GetBaits(Giun));
        }

        // ---------- Save ----------

        [Test]
        public void Save_RoundTrip_KeepsFishing()
        {
            Farm farm = CreateFarm(roSlip: 0, coins: 10000);
            farm.State.Level = 9;
            farm.BuyRod(Carbon);
            farm.UpgradeBasket();
            farm.CompleteFishing(CatchOne(farm));
            farm.TryGetEquippedRod(out _, out RodState rod);
            rod.Durability = 0;
            farm.RepairRod(Tre);
            rod.Durability = 0;
            farm.RepairRod(Tre);
            rod.Durability -= 2;
            rod.Snapped = true;

            Farm loaded = RoundTrip(farm);
            FishingState a = farm.State.Fishing;
            FishingState b = loaded.State.Fishing;

            Assert.AreEqual(a.Rods.Count, b.Rods.Count);
            Assert.IsTrue(b.TryGetRod(Tre, out RodState loadedRod));
            Assert.AreEqual(rod.Durability, loadedRod.Durability);
            Assert.IsTrue(loadedRod.Snapped);
            Assert.AreEqual(2, loadedRod.RepairCount);
            Assert.AreEqual(24, loaded.GetRodMaxDurability(Tre));
            Assert.AreEqual(a.EquippedRodId, b.EquippedRodId);
            Assert.AreEqual(a.GetBaits(Giun), b.GetBaits(Giun));
            Assert.AreEqual(a.BasketLevel, b.BasketLevel);
            Assert.AreEqual(1, b.Basket.Count);
            Assert.AreEqual(a.Basket[0].FishId, b.Basket[0].FishId);
            Assert.AreEqual(a.Basket[0].Grams, b.Basket[0].Grams);
            Assert.AreEqual(a.Basket[0].Slot, b.Basket[0].Slot);
            Assert.AreEqual(a.GetRecord(a.Basket[0].FishId), b.GetRecord(a.Basket[0].FishId));
            Assert.IsTrue(b.StarterGranted);
        }

        [Test]
        public void Save_Version4WithoutFishing_GetsStarter()
        {
            Farm farm = CreateFarm();
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.version = 4;
            data.fishing = null;

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState state));
            var loaded = new Farm(farm.Definition, state, _clock);

            Assert.IsTrue(loaded.State.Fishing.StarterGranted);
            Assert.AreEqual(Tre, loaded.State.Fishing.EquippedRodId);
            Assert.AreEqual(10, loaded.State.Fishing.GetBaits(Giun));
        }

        [Test]
        public void Save_Version5WithoutRepairCount_NotRepaired()
        {
            Farm farm = CreateFarm();
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.version = 5;
            data.fishing.rods = new[] { new SaveRod { id = Tre, durability = 12 } };

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState state));
            var loaded = new Farm(farm.Definition, state, _clock);

            Assert.IsTrue(loaded.State.Fishing.TryGetRod(Tre, out RodState rod));
            Assert.AreEqual(0, rod.RepairCount);
            Assert.AreEqual(12, rod.Durability);
            Assert.AreEqual(30, loaded.GetRodMaxDurability(Tre));
        }

        [Test]
        public void Load_DurabilityAboveRepairedMax_Clamped()
        {
            Farm farm = CreateFarm();
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.fishing.rods = new[] { new SaveRod { id = Tre, durability = 30, repairCount = 3 } };

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState state));
            var loaded = new Farm(farm.Definition, state, _clock);

            loaded.State.Fishing.TryGetRod(Tre, out RodState rod);
            Assert.AreEqual(21, rod.Durability);
        }

        [Test]
        public void Save_BrokenEntries_Skipped()
        {
            Farm farm = CreateFarm();
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.fishing.basket = new[]
            {
                new SaveFish { id = Ro, grams = 120, slot = 99 },
                new SaveFish { id = "", grams = 50 },
                new SaveFish { id = Ro, grams = -1 },
            };
            data.fishing.baits = new[] { new SaveCount { id = Giun, count = -5 } };

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState state));

            Assert.AreEqual(1, state.Fishing.Basket.Count);
            Assert.AreEqual(DaySlot.Dawn, state.Fishing.Basket[0].Slot);
            Assert.AreEqual(0, state.Fishing.GetBaits(Giun));
        }

        // ---------- Phép tính cho View (Đợt C) ----------

        [TestCase(0f, 0f)]
        [TestCase(0.3f, 0.5f)]
        [TestCase(0.6f, 1f)]
        [TestCase(0.9f, 0.5f)]
        [TestCase(1.2f, 0f)]
        [TestCase(1.5f, 0.5f)]
        [TestCase(-1f, 0f)]
        public void CastPower_PingPongsOverPeriod(float elapsed, float expected)
        {
            Assert.AreEqual(expected, FishingMath.CastPower(elapsed, 1.2f), 0.0001f);
        }

        [Test]
        public void CastPower_ZeroPeriod_IsZero()
        {
            Assert.AreEqual(0f, FishingMath.CastPower(1f, 0f));
        }

        [TestCase(5 * 3600, 2 * 3600)]           // 5:00 Bình minh → hết lúc 7:00.
        [TestCase(6 * 3600 + 3599, 1)]           // 6:59:59 → còn 1 giây.
        [TestCase(18 * 3600 + 36 * 60, 24 * 60)] // 18:36 Hoàng hôn → 19:00.
        [TestCase(23 * 3600, 6 * 3600)]          // 23:00 Khuya → 5:00 hôm sau.
        [TestCase(23 * 3600 + 1800, 5 * 3600 + 1800)]
        [TestCase(2 * 3600, 3 * 3600)]           // 2:00 Khuya → 5:00.
        public void SecondsUntilEnd_ByLocalTime(long localSeconds, long expected)
        {
            Assert.AreEqual(expected, DaySlots.SecondsUntilEnd(Midnight + localSeconds, 0));
        }

        [Test]
        public void SecondsUntilEnd_UsesUtcOffset()
        {
            // 22:00 UTC + 7 giờ = 5:00 giờ địa phương (Bình minh) → còn 2 giờ.
            Assert.AreEqual(2 * 3600, DaySlots.SecondsUntilEnd(Midnight + 22 * 3600, 7 * 3600));
        }

        [Test]
        public void IsNibbling_OnlyDuringNibbles()
        {
            Farm farm = CreateFarm();
            FishingSession session = CastOne(farm);
            foreach (float nibble in session.NibbleTimes)
            {
                Assert.IsTrue(FishingMath.IsNibbling(session, nibble, 0.15f));
                Assert.IsTrue(FishingMath.IsNibbling(session, nibble + 0.1f, 0.15f));
                Assert.IsFalse(FishingMath.IsNibbling(session, nibble + 0.2f, 0.15f));
            }
            Assert.IsFalse(FishingMath.IsNibbling(session, session.BiteTime, 0.15f));
        }

        // ---------- Hỗ trợ ----------

        private static Arrow Opposite(Arrow arrow)
        {
            switch (arrow)
            {
                case Arrow.Up: return Arrow.Down;
                case Arrow.Down: return Arrow.Up;
                case Arrow.Left: return Arrow.Right;
                default: return Arrow.Left;
            }
        }

        private static int CountRods(Farm farm, string rodId)
        {
            int count = 0;
            foreach (RodState rod in farm.State.Fishing.Rods)
            {
                if (rod.Id == rodId)
                    count++;
            }
            return count;
        }

        // Số lần gãy cần trên 200 lần đứt dây (seed khác nhau), bắt đầu với độ bền cho trước.
        private int CountSnapBreaks(int startDurability)
        {
            int broken = 0;
            for (uint seed = 1; seed <= 200; seed++)
            {
                Farm farm = CreateFarm(roSlip: 0, randomState: seed * 7919u, snapBreakMin: 0, snapBreakMax: 100);
                farm.TryGetEquippedRod(out _, out RodState rod);
                rod.Durability = startDurability;
                FishingSession session = HookOne(farm);
                session.WrongArrow();
                Assert.AreEqual(FishingOutcome.LineSnapped, session.Outcome);
                farm.CompleteFishing(session);
                if (rod.Snapped)
                    broken++;
            }
            return broken;
        }

        private void SetLocalHour(int hour)
        {
            _clock.UtcNowSeconds = Midnight + hour * 3600;
        }

        private HashSet<string> CastMany(Farm farm, string bait, float power, int count)
        {
            var seen = new HashSet<string>();
            for (int i = 0; i < count; i++)
            {
                farm.State.Fishing.Baits[bait] = 5;
                Assert.AreEqual(FarmResult.Ok, farm.Cast(bait, power, out FishingSession session), $"Lần ném {i}");
                seen.Add(session.Fish.Id);
                session.Timeout();
                farm.CompleteFishing(session);
                RefreshRod(farm);
            }
            return seen;
        }

        private int CountFish(Farm farm, string bait, string fishId, int casts)
        {
            int count = 0;
            for (int i = 0; i < casts; i++)
            {
                farm.State.Fishing.Baits[bait] = 5;
                farm.Cast(bait, 0.6f, out FishingSession session);
                if (session.Fish.Id == fishId)
                    count++;
                session.Timeout();
                farm.CompleteFishing(session);
                RefreshRod(farm);
            }
            return count;
        }

        private static void RefreshRod(Farm farm)
        {
            farm.TryGetEquippedRod(out RodDef def, out RodState rod);
            rod.Durability = def.MaxDurability;
            rod.Snapped = false;
        }

        private static FishingSession CastOne(Farm farm)
        {
            Assert.AreEqual(FarmResult.Ok, farm.Cast(Giun, 0.3f, out FishingSession session));
            return session;
        }

        // Ném gần bờ (chỉ cá rô ở độ sâu Gần) rồi giật đúng lúc.
        private static FishingSession HookOne(Farm farm)
        {
            FishingSession session = CastOne(farm);
            Assert.AreEqual(Ro, session.Fish.Id);
            Assert.AreEqual(FishingOutcome.None, session.Strike(session.BiteTime));
            return session;
        }

        private static FishingSession CatchOne(Farm farm)
        {
            FishingSession session = HookOne(farm);
            Assert.AreEqual(FishingOutcome.Caught, session.CompletePull(0.5f));
            return session;
        }

        private void PlantAndHarvest(Farm farm)
        {
            if (farm.State.GetSeeds(RauMuong) == 0)
                farm.State.Seeds[RauMuong] = 1;
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot0, RauMuong));
            _clock.Advance(GrowSeconds);
            Assert.AreEqual(FarmResult.Ok, farm.Harvest(Plot0));
        }

        private Farm RoundTrip(Farm farm)
        {
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));
            return new Farm(farm.Definition, loaded, _clock);
        }

        private Farm CreateFarm(bool withFishing = true, int unlockLevel = UnlockLevel, int baits = 10,
            uint randomState = 12345, int roSlip = 0, int roXp = 1, int snapBreakPercent = 20,
            int wormChance = 0, long coins = 0, bool roRed = false, int snapBreakMin = -1, int snapBreakMax = -1)
        {
            FarmDef def = CreateDef(withFishing, unlockLevel, baits, roSlip, roXp,
                snapBreakMin >= 0 ? snapBreakMin : snapBreakPercent, snapBreakMax >= 0 ? snapBreakMax : snapBreakPercent,
                wormChance, roRed);
            FarmState state = FarmState.CreateNew(def);
            state.RandomState = randomState;
            state.Coins = coins;
            return new Farm(def, state, _clock);
        }

        private static FarmDef CreateDef(bool withFishing, int unlockLevel, int starterBaits, int roSlip, int roXp,
            int snapBreakMin, int snapBreakMax, int wormChance, bool roRed)
        {
            var zones = new[] { new ZoneDef(Zone1, new[] { new BedDef(2, 3) }, 2, new[] { 10, 10, 10, 10 }) };
            var crops = new[] { new CropDef(RauMuong, GrowSeconds, 5, 5, 2, xp: 1) };
            var levels = new LevelDef(new[] { 3, 20, 50, 100, 200, 300, 400, 500, 600 }, 0, 0);
            var seeds = new Dictionary<string, int> { { RauMuong, 5 } };

            FishingDef fishing = null;
            if (withFishing)
            {
                float[] allDay = { 1, 1, 1, 1, 1, 1, 1 };
                float[] dawnSunset = { 2, 0, 0, 0, 2, 0, 0 };
                float[] dawnOnly = { 1, 0, 0, 0, 0, 0, 0 };
                var fish = new[]
                {
                    new FishDef(Ro, FishRarity.Common, 80, 300, 10, roXp, allDay, FishDepth.All, new[] { Giun },
                        3, 4, 4f, 0.9f, roSlip, roRed),
                    new FishDef(Loc, FishRarity.Rare, 500, 2500, 80, 8, dawnSunset, FishDepth.Mid | FishDepth.Far, new[] { Tep },
                        5, 7, 3.5f, 0.6f, 15),
                    new FishDef(ChepVang, FishRarity.Legendary, 3000, 9000, 900, 50, dawnOnly, FishDepth.Far, new[] { MoiThom },
                        8, 10, 3f, 0.4f, 30, redArrows: true),
                };
                var rods = new[]
                {
                    new RodDef(Tre, 1, 0.03f, 1.2f, 0, 0, 0, 30, 0, 1, 50),
                    new RodDef(Carbon, 4, 0.06f, 1.6f, 30, 10, 30, 120, 6000, 9, 2400),
                };
                var baitDefs = new[]
                {
                    new BaitDef(Giun, 2, buyable: false),
                    new BaitDef(Tep, 5),
                    new BaitDef(MoiThom, 25, rareBonusPercent: 100),
                };
                var tuning = new FishingTuning
                {
                    SnapBreakMinPercent = snapBreakMin,
                    SnapBreakMaxPercent = snapBreakMax,
                    RedArrowPercent = 100,
                };
                fishing = new FishingDef(fish, rods, baitDefs, Tre, Giun, starterBaits, unlockLevel,
                    Capacities, UpgradePrices, Giun, wormChance, 1, 2, tuning);
            }

            return new FarmDef(zones, crops, 0, seeds, 0, levels, null, null, fishing);
        }
    }
}
