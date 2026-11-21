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
        private const int MoiThomLevel = 5;
        private const int StartSlots = 8;
        private static readonly long[] SlotPrices = { 300, 1000 };

        // 1/1/2027 00:00 UTC là thứ Sáu; dùng mốc ngày tròn để đặt giờ địa phương dễ đọc.
        private const long Midnight = 1798761600;
        // Khung giờ câu cá tính theo giờ VN cố định (Đợt 2 Phần 2).
        private const long VietnamOffset = GameCalendar.OffsetSeconds;
        private static readonly PlotId Plot0 = new PlotId(Zone1, 0, 0, 0);

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            // Giờ game là giờ VN (UTC+7, GameCalendar): 8 giờ sáng VN.
            _clock = new FakeClock { UtcNowSeconds = Midnight + 8 * 3600 - VietnamOffset };
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
            int durability = rod.Durability;
            rod.Durability = 0;
            Assert.AreEqual(FarmResult.RodBroken, farm.Cast(Giun, 0.6f, out _));
            rod.Durability = durability;

            for (int i = 0; i < StartSlots; i++)
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
            var rod = new RodDef(Tre, 1, 0.03f, 1.2f, 100, 0, 0, 30, 0, 1);

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
            Assert.That(session.Pull.Count, Is.InRange(4, session.Fish.MaxArrows));
            // Cần tre không thêm thời gian: giây kéo của cá + 0,45 giây mỗi mũi tên.
            Assert.AreEqual(session.Fish.PullSeconds + session.Pull.Count * 0.45f, session.Pull.TimeLimitSeconds, 0.0001f);
        }

        // 14/11/2026: ném Perfect bớt 1 mũi tên nhưng không dưới MinPullArrows (cá rô test có 3–4 mũi tên).
        [Test]
        public void Pull_PerfectCast_NeverBelowMinPullArrows()
        {
            int minArrows = new FishingTuning().MinPullArrows;
            float perfectPower = new FishingTuning().PerfectCenter;
            int roHooked = 0;
            for (uint seed = 1; seed <= 40; seed++)
            {
                Farm farm = CreateFarm(randomState: seed);
                Assert.AreEqual(FarmResult.Ok, farm.Cast(Giun, perfectPower, out FishingSession session));
                Assert.AreEqual(CastQuality.Perfect, session.Quality);
                if (session.Strike(session.BiteTime) != FishingOutcome.None)
                    continue;
                Assert.That(session.Pull.Count, Is.GreaterThanOrEqualTo(minArrows), session.Fish.Id);
                if (session.Fish.Id == Ro)
                    roHooked++;
            }
            Assert.Greater(roHooked, 0, "Cần ít nhất 1 lần móc cá rô (3–4 mũi tên) để kiểm tra chặn dưới.");
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
        public void Pull_WrongArrow_KeepsPullingAndCanStillCatch()
        {
            Farm farm = CreateFarm(roSlip: 0);
            farm.TryGetEquippedRod(out RodDef def, out RodState rod);
            FishingSession session = HookOne(farm);

            Assert.AreEqual(FishingOutcome.None, session.WrongArrow());
            Assert.AreEqual(FishingOutcome.None, session.WrongArrow());
            Assert.AreEqual(FishingPhase.Pulling, session.Phase);
            Assert.AreEqual(2, session.Mistakes);

            // Nhập lại đúng cả chuỗi vẫn câu được; cần không gãy, chỉ trừ 1 độ bền lúc ném.
            Assert.AreEqual(FishingOutcome.Caught, session.CompletePull(0.5f));
            farm.CompleteFishing(session);
            Assert.AreEqual(def.MaxDurability - 1, rod.Durability);
            Assert.IsFalse(rod.IsBroken);
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

            // Cần carbon +30% thời gian (tính cả phần theo số mũi tên).
            Assert.AreEqual((session.Fish.PullSeconds + session.Pull.Count * 0.45f) * 1.3f, session.Pull.TimeLimitSeconds, 0.0001f);
        }

        // Đợt 2 Phần 6: chuỗi dài hơn thì thêm thời gian; 0 giây mỗi mũi tên thì về như cũ.
        [Test]
        public void Pull_TimeGrowsWithArrowCount()
        {
            var tuning = new FishingTuning();
            Assert.AreEqual(0.45f, tuning.PullSecondsPerArrow, 0.0001f);

            Farm farm = CreateFarm(roSlip: 0);
            FishingSession session = HookOne(farm);
            float perArrow = (session.Pull.TimeLimitSeconds - session.Fish.PullSeconds) / session.Pull.Count;
            Assert.AreEqual(0.45f, perArrow, 0.0001f);
            // Ví dụ thiết kế: cá 3 giây, 10 mũi tên → 7,5 giây; reset ở mũi 9 còn 19 lần bấm ≈ 0,39 giây mỗi lần.
            Assert.AreEqual(7.5f, 3f + 10 * tuning.PullSecondsPerArrow, 0.0001f);
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

        // Đợt E: cờ kỷ lục trên phiên (bảng cá vừa câu đọc để hiện "Loài mới!" / "Kỷ lục mới!").
        [Test]
        public void Record_FirstCatch_IsNewSpecies()
        {
            Farm farm = CreateFarm(roSlip: 0);
            FishingSession session = CatchOne(farm);
            Assert.AreEqual(FarmResult.Ok, farm.CompleteFishing(session));

            Assert.IsTrue(session.IsNewRecord);
            Assert.IsTrue(session.IsNewSpecies);
            Assert.AreEqual(0, session.PreviousRecordGrams);
        }

        [Test]
        public void Record_Heavier_IsNewRecordWithPrevious()
        {
            Farm farm = CreateFarm(roSlip: 0);
            farm.State.Fishing.FishDex[Ro] = new FishDexEntry { Count = 1, MaxGrams = 1 };
            FishingSession session = CatchOne(farm);
            farm.CompleteFishing(session);

            Assert.IsTrue(session.IsNewRecord);
            Assert.IsFalse(session.IsNewSpecies);
            Assert.AreEqual(1, session.PreviousRecordGrams);
            Assert.AreEqual(session.Grams, farm.State.Fishing.GetRecord(Ro));
        }

        [Test]
        public void Record_Lighter_NotNewRecord()
        {
            Farm farm = CreateFarm(roSlip: 0);
            farm.State.Fishing.FishDex[Ro] = new FishDexEntry { Count = 1, MaxGrams = 100000 };
            FishingSession session = CatchOne(farm);
            farm.CompleteFishing(session);

            Assert.IsFalse(session.IsNewRecord);
            Assert.IsFalse(session.IsNewSpecies);
            Assert.AreEqual(100000, session.PreviousRecordGrams);
            Assert.AreEqual(100000, farm.State.Fishing.GetRecord(Ro));
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

        // ---------- Giỏ cá ----------

        [Test]
        public void ReleaseFish_RemovesOneKeepsRecord()
        {
            Farm farm = CreateFarm(roSlip: 0, baits: 10);
            for (int i = 0; i < 2; i++)
            {
                farm.CompleteFishing(CatchOne(farm));
            }
            List<CaughtFish> basket = farm.State.Fishing.Basket;
            Assert.AreEqual(2, basket.Count);
            CaughtFish second = basket[1];
            int record = farm.State.Fishing.GetRecord(basket[0].FishId);

            Assert.AreEqual(FarmResult.Ok, farm.ReleaseFish(0));

            Assert.AreEqual(1, basket.Count);
            Assert.AreSame(second, basket[0]);
            Assert.AreEqual(record, farm.State.Fishing.GetRecord(second.FishId));
        }

        [Test]
        public void SellFish_AddsPriceRemovesOne()
        {
            Farm farm = CreateFarm(roSlip: 0);
            farm.CompleteFishing(CatchOne(farm));
            farm.CompleteFishing(CatchOne(farm));
            long coins = farm.State.Coins;
            long price = farm.GetFishPrice(farm.State.Fishing.Basket[0]);

            Assert.AreEqual(FarmResult.Ok, farm.SellFish(0, out long earned));

            Assert.AreEqual(price, earned);
            Assert.AreEqual(coins + price, farm.State.Coins);
            Assert.AreEqual(1, farm.State.Fishing.Basket.Count);
            Assert.AreEqual(FarmResult.UnknownFish, farm.SellFish(5, out _));
        }

        [Test]
        public void SellAllFish_SumsAndEmpties()
        {
            Farm farm = CreateFarm(roSlip: 0);
            farm.CompleteFishing(CatchOne(farm));
            farm.CompleteFishing(CatchOne(farm));
            long value = farm.GetBasketValue();
            long coins = farm.State.Coins;

            Assert.AreEqual(FarmResult.Ok, farm.SellAllFish(out long earned));

            Assert.AreEqual(value, earned);
            Assert.AreEqual(coins + value, farm.State.Coins);
            CollectionAssert.IsEmpty(farm.State.Fishing.Basket);
            Assert.AreEqual(FarmResult.UnknownFish, farm.SellAllFish(out _));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void ReleaseFish_BadIndex_Unknown(int index)
        {
            Farm farm = CreateFarm();
            Assert.AreEqual(FarmResult.UnknownFish, farm.ReleaseFish(index));
        }

        [Test]
        public void GetFishPrice_MatchesPriceWhenCaught()
        {
            Farm farm = CreateFarm(roSlip: 0);
            FishingSession session = CatchOne(farm);
            farm.CompleteFishing(session);

            Assert.AreEqual(session.Price, farm.GetFishPrice(farm.State.Fishing.Basket[0]));
        }

        // ---------- Cửa hàng ----------

        // Đợt 2 Phần 1: sửa về đầy, độ bền tối đa giữ nguyên, giá = giá cần × 30% × phần thiếu (tròn lên, tối thiểu 1).
        [Test]
        public void Repair_RestoresFull_PriceByMissingDurability()
        {
            Farm farm = CreateFarm(coins: 100000);
            farm.State.Level = 9;
            Assert.AreEqual(FarmResult.Ok, farm.BuyRod(Carbon));
            Assert.IsTrue(farm.State.Fishing.TryGetRod(Carbon, out RodState rod));

            Assert.AreEqual(0, farm.GetRodRepairPrice(Carbon));
            Assert.AreEqual(FarmResult.RodNotDamaged, farm.RepairRod(Carbon));

            // Carbon: giá 6000, bền 120.
            rod.Durability = 119;
            Assert.AreEqual(15, farm.GetRodRepairPrice(Carbon));
            rod.Durability = 60;
            Assert.AreEqual(900, farm.GetRodRepairPrice(Carbon));
            rod.Durability = 0;
            Assert.AreEqual(1800, farm.GetRodRepairPrice(Carbon));

            long coins = farm.State.Coins;
            Assert.AreEqual(FarmResult.Ok, farm.RepairRod(Carbon));
            Assert.AreEqual(coins - 1800, farm.State.Coins);
            Assert.AreEqual(120, rod.Durability);
            Assert.AreEqual(120, farm.GetRodMaxDurability(Carbon));
            Assert.IsFalse(rod.IsBroken);
            Assert.AreEqual(FarmResult.RodNotDamaged, farm.RepairRod(Carbon));
        }

        [Test]
        public void Repair_FreeRod_CostsAtLeastOneCoin()
        {
            Farm farm = CreateFarm(coins: 5);
            farm.TryGetEquippedRod(out _, out RodState rod);
            rod.Durability = 0;

            Assert.AreEqual(1, farm.GetRodRepairPrice(Tre));
            Assert.AreEqual(FarmResult.Ok, farm.RepairRod(Tre));
            Assert.AreEqual(4, farm.State.Coins);
            Assert.AreEqual(30, rod.Durability);
        }

        [TestCase(11L, 2, 3, 2L)]
        [TestCase(10L, 2, 3, 1L)]
        [TestCase(100L, 0, 7, 30L)]
        [TestCase(100L, 7, 7, 0L)]
        [TestCase(0L, 0, 30, 1L)]
        public void RepairPrice_RoundsUp_MinOne(long rodPrice, int durability, int max, long expected)
        {
            Assert.AreEqual(expected, new FishingTuning().RepairPrice(rodPrice, durability, max));
        }

        [Test]
        public void Repair_ManyTimes_MaxDurabilityNeverDrops()
        {
            Farm farm = CreateFarm(coins: 100000);
            farm.TryGetEquippedRod(out _, out RodState rod);

            for (int i = 0; i < 20; i++)
            {
                rod.Durability = 0;
                Assert.AreEqual(FarmResult.Ok, farm.RepairRod(Tre), $"Lần sửa {i + 1}");
                Assert.AreEqual(30, rod.Durability, $"Lần sửa {i + 1}");
                Assert.AreEqual(30, farm.GetRodMaxDurability(Tre), $"Lần sửa {i + 1}");
            }
        }

        [Test]
        public void Repair_NotEnoughCoins_NoChange()
        {
            Farm farm = CreateFarm(coins: 100000);
            farm.State.Level = 9;
            farm.BuyRod(Carbon);
            farm.State.Fishing.TryGetRod(Carbon, out RodState rod);
            rod.Durability = 0;
            farm.State.Coins = 1799;

            Assert.AreEqual(FarmResult.NotEnoughCoins, farm.RepairRod(Carbon));
            Assert.AreEqual(1799, farm.State.Coins);
            Assert.AreEqual(0, rod.Durability);
        }

        [Test]
        public void IsRodLow_AtOrBelowTenPercent()
        {
            Farm farm = CreateFarm(coins: 100000);
            farm.State.Level = 9;
            farm.BuyRod(Carbon);
            farm.State.Fishing.TryGetRod(Carbon, out RodState rod);

            rod.Durability = 13;
            Assert.IsFalse(farm.IsRodLow(Carbon));
            rod.Durability = 12;
            Assert.IsTrue(farm.IsRodLow(Carbon));
            rod.Durability = 0;
            Assert.IsTrue(farm.IsRodLow(Carbon));
            Assert.IsFalse(farm.IsRodLow("khong_co"));
        }

        [Test]
        public void BuyRod_OwnedEvenBroken_AlreadyOwned()
        {
            Farm farm = CreateFarm(coins: 100000);
            farm.State.Level = 9;
            Assert.AreEqual(FarmResult.Ok, farm.BuyRod(Carbon));
            farm.State.Fishing.TryGetRod(Carbon, out RodState rod);
            rod.Durability = 0;

            long coins = farm.State.Coins;
            Assert.AreEqual(FarmResult.AlreadyOwned, farm.BuyRod(Carbon));
            Assert.AreEqual(coins, farm.State.Coins);
            Assert.AreEqual(1, CountRods(farm, Carbon));
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
        public void BuyBait_NeedsLevel()
        {
            Farm farm = CreateFarm(coins: 1000);

            Assert.AreEqual(FarmResult.LevelTooLow, farm.BuyBait(MoiThom, 1));
            Assert.AreEqual(1000, farm.State.Coins);

            farm.State.Level = MoiThomLevel;
            Assert.AreEqual(FarmResult.Ok, farm.BuyBait(MoiThom, 2));
            Assert.AreEqual(950, farm.State.Coins);
        }

        [Test]
        public void GetUnlocksAtLevel_ListsRodsAndBaits()
        {
            Farm farm = CreateFarm();

            CollectionAssert.AreEqual(new[] { Carbon }, farm.GetUnlocksAtLevel(9).RodIds);
            CollectionAssert.AreEqual(new[] { MoiThom }, farm.GetUnlocksAtLevel(MoiThomLevel).BaitIds);
            CollectionAssert.IsEmpty(farm.GetUnlocksAtLevel(MoiThomLevel).RodIds);
            // Cấp 1 không báo (cần tre / mồi cấp 1 có sẵn từ đầu).
            CollectionAssert.IsEmpty(farm.GetUnlocksAtLevel(1).RodIds);
            CollectionAssert.IsEmpty(farm.GetUnlocksAtLevel(1).BaitIds);
        }

        [Test]
        public void UnlockBasketSlot_OneByOneUntilMax()
        {
            Farm farm = CreateFarm(coins: 2000);
            Assert.AreEqual(8, farm.BasketCapacity);
            Assert.AreEqual(300, farm.NextBasketSlotPrice);

            Assert.AreEqual(FarmResult.Ok, farm.UnlockBasketSlot());
            Assert.AreEqual(9, farm.BasketCapacity);
            Assert.AreEqual(1000, farm.NextBasketSlotPrice);
            Assert.AreEqual(FarmResult.Ok, farm.UnlockBasketSlot());
            Assert.AreEqual(10, farm.BasketCapacity);
            Assert.IsFalse(farm.CanUnlockBasketSlot);
            Assert.AreEqual(-1, farm.NextBasketSlotPrice);
            Assert.AreEqual(FarmResult.BasketMaxed, farm.UnlockBasketSlot());
            Assert.AreEqual(700, farm.State.Coins);
        }

        [Test]
        public void UnlockBasketSlot_NotEnoughCoins()
        {
            Farm farm = CreateFarm(coins: 299);

            Assert.AreEqual(FarmResult.NotEnoughCoins, farm.UnlockBasketSlot());
            Assert.AreEqual(8, farm.BasketCapacity);
            Assert.AreEqual(299, farm.State.Coins);
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
            farm.UnlockBasketSlot();
            farm.CompleteFishing(CatchOne(farm));
            farm.TryGetEquippedRod(out _, out RodState rod);
            rod.Durability -= 2;

            Farm loaded = RoundTrip(farm);
            FishingState a = farm.State.Fishing;
            FishingState b = loaded.State.Fishing;

            Assert.AreEqual(a.Rods.Count, b.Rods.Count);
            Assert.IsTrue(b.TryGetRod(Tre, out RodState loadedRod));
            Assert.AreEqual(rod.Durability, loadedRod.Durability);
            Assert.AreEqual(30, loaded.GetRodMaxDurability(Tre));
            Assert.AreEqual(a.EquippedRodId, b.EquippedRodId);
            Assert.AreEqual(a.GetBaits(Giun), b.GetBaits(Giun));
            Assert.AreEqual(9, loaded.BasketCapacity);
            Assert.AreEqual(a.BasketSlots, b.BasketSlots);
            Assert.AreEqual(1, b.Basket.Count);
            Assert.AreEqual(a.Basket[0].FishId, b.Basket[0].FishId);
            Assert.AreEqual(a.Basket[0].Grams, b.Basket[0].Grams);
            Assert.AreEqual(a.Basket[0].Slot, b.Basket[0].Slot);
            Assert.AreEqual(a.GetRecord(a.Basket[0].FishId), b.GetRecord(a.Basket[0].FishId));
            Assert.IsTrue(b.StarterGranted);
        }

        [Test]
        public void Load_DurabilityAboveMax_Clamped()
        {
            Farm farm = CreateFarm();
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.fishing.rods = new[] { new SaveRod { id = Tre, durability = 99 } };

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState state));
            var loaded = new Farm(farm.Definition, state, _clock);

            loaded.State.Fishing.TryGetRod(Tre, out RodState rod);
            Assert.AreEqual(30, rod.Durability);
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

        // ---------- Cá shiny (Đợt 2 Phần 18) ----------

        [Test]
        public void RollShiny_100k_AboutOnePercent()
        {
            var tuning = new FishingTuning();
            uint random = 12345;
            int shiny = 0;
            const int Rolls = 100000;
            for (int i = 0; i < Rolls; i++)
            {
                if (FishingSession.RollShiny(ref random, tuning) != ShinyKind.None)
                    shiny++;
            }
            Assert.AreEqual(1.0, shiny * 100.0 / Rolls, 0.15);
        }

        [Test]
        public void RollShiny_TierAndKinds_Distribution()
        {
            // Luôn ra shiny để đếm phân bố bậc / kiểu.
            var tuning = new FishingTuning { ShinyChancePer10000 = 10000 };
            uint random = 777;
            var counts = new Dictionary<ShinyKind, int>();
            const int Rolls = 100000;
            for (int i = 0; i < Rolls; i++)
            {
                ShinyKind kind = FishingSession.RollShiny(ref random, tuning);
                Assert.AreNotEqual(ShinyKind.None, kind);
                counts[kind] = counts.TryGetValue(kind, out int c) ? c + 1 : 1;
            }

            int rare = 0;
            foreach (ShinyKind kind in tuning.ShinyRareKinds)
                rare += counts.TryGetValue(kind, out int c) ? c : 0;
            Assert.AreEqual(3.2, rare * 100.0 / Rolls, 0.3, "Bậc hiếm ≈ 3,2%");

            foreach (ShinyKind kind in tuning.ShinyRareKinds)
                Assert.AreEqual(50.0, counts[kind] * 100.0 / rare, 5.0, $"{kind} ≈ 50% bậc hiếm");

            int common = Rolls - rare;
            foreach (ShinyKind kind in tuning.ShinyCommonKinds)
                Assert.AreEqual(100.0 / 6, counts[kind] * 100.0 / common, 1.0, $"{kind} ≈ 1/6 bậc thường");
            Assert.AreEqual(8, counts.Count);
        }

        [Test]
        public void RollShiny_ZeroChance_NeverShiny()
        {
            var tuning = new FishingTuning { ShinyChancePer10000 = 0 };
            uint random = 1;
            for (int i = 0; i < 10000; i++)
                Assert.AreEqual(ShinyKind.None, FishingSession.RollShiny(ref random, tuning));
        }

        [Test]
        public void ShinyPrice_CommonTimesThree_RareTimesTen()
        {
            var tuning = new FishingTuning();
            var fish = new FishDef(Ro, FishRarity.Common, 100, 300, 10, 1, new float[] { 1, 1, 1, 1, 1, 1, 1 },
                FishDepth.All, null, 3, 4, 4f, 0.9f, 0);
            int normal = FishingMath.FishPrice(fish, 200, tuning);

            Assert.AreEqual(normal, FishingMath.FishPrice(fish, 200, tuning, ShinyKind.None));
            foreach (ShinyKind kind in tuning.ShinyCommonKinds)
                Assert.AreEqual(normal * 3, FishingMath.FishPrice(fish, 200, tuning, kind), kind.ToString());
            Assert.AreEqual(normal * 10, FishingMath.FishPrice(fish, 200, tuning, ShinyKind.Aurora));
            Assert.AreEqual(normal * 10, FishingMath.FishPrice(fish, 200, tuning, ShinyKind.DarkAurora));
        }

        [Test]
        public void Shiny_DoesNotShiftOtherRandomRolls()
        {
            // Cùng trạng thái random: bật / tắt shiny không đổi cá, lịch phao, đề kéo, cân nặng.
            Farm plain = CreateFarm();
            Farm shiny = CreateFarm(shiny: ShinyKind.Gold);
            FishingSession a = CatchOne(plain);
            FishingSession b = CatchOne(shiny);

            Assert.AreEqual(ShinyKind.None, a.Shiny);
            Assert.AreEqual(ShinyKind.Gold, b.Shiny);
            Assert.AreEqual(a.Fish.Id, b.Fish.Id);
            Assert.AreEqual(a.BiteTime, b.BiteTime);
            CollectionAssert.AreEqual(a.NibbleTimes, b.NibbleTimes);
            CollectionAssert.AreEqual(a.Pull.Arrows, b.Pull.Arrows);
            Assert.AreEqual(a.Grams, b.Grams);
            Assert.AreEqual(a.Xp, b.Xp);
            Assert.AreEqual(a.Price * 3, b.Price);
        }

        [Test]
        public void Shiny_Caught_GoesToBasketAndDexOnce()
        {
            Farm farm = CreateFarm(shiny: ShinyKind.Aurora);
            FishingSession first = CatchOne(farm);
            farm.CompleteFishing(first);
            FishingSession second = CatchOne(farm);
            farm.CompleteFishing(second);

            FishingState fishing = farm.State.Fishing;
            Assert.AreEqual(ShinyKind.Aurora, fishing.Basket[0].Shiny);
            Assert.AreEqual(first.Price, farm.GetFishPrice(fishing.Basket[0]));
            Assert.IsTrue(first.IsNewShiny);
            Assert.IsFalse(second.IsNewShiny);
            Assert.AreEqual(1, fishing.ShinyDex.Count);
            Assert.AreEqual(Ro, fishing.ShinyDex[0].FishId);
            Assert.AreEqual(ShinyKind.Aurora, fishing.ShinyDex[0].Kind);
            Assert.AreEqual(_clock.UtcNowSeconds, fishing.ShinyDex[0].FirstCaughtAt);
        }

        [Test]
        public void Shiny_NotCaught_NoDex()
        {
            Farm farm = CreateFarm(shiny: ShinyKind.Gold);
            FishingSession session = CastOne(farm);
            session.Timeout();
            farm.CompleteFishing(session);

            CollectionAssert.IsEmpty(farm.State.Fishing.ShinyDex);
            CollectionAssert.IsEmpty(farm.State.Fishing.Basket);
        }

        [Test]
        public void SellAllFish_SkipsShiny()
        {
            Farm farm = CreateFarm(shiny: ShinyKind.Ruby);
            farm.CompleteFishing(CatchOne(farm));
            farm.Definition.Fishing.Tuning.ShinyChancePer10000 = 0;
            farm.CompleteFishing(CatchOne(farm));
            farm.CompleteFishing(CatchOne(farm));
            List<CaughtFish> basket = farm.State.Fishing.Basket;
            long normalValue = farm.GetFishPrice(basket[1]) + farm.GetFishPrice(basket[2]);

            Assert.AreEqual(2, farm.GetSellAllCount());
            Assert.AreEqual(normalValue, farm.GetBasketValue());
            Assert.AreEqual(FarmResult.Ok, farm.SellAllFish(out long earned));

            Assert.AreEqual(normalValue, earned);
            Assert.AreEqual(1, basket.Count);
            Assert.AreEqual(ShinyKind.Ruby, basket[0].Shiny);
            // Chỉ còn cá shiny: Bán hết không bán gì.
            Assert.AreEqual(FarmResult.UnknownFish, farm.SellAllFish(out _));
            Assert.AreEqual(1, basket.Count);
            // Bán lẻ vẫn được.
            Assert.AreEqual(FarmResult.Ok, farm.SellFish(0, out _));
        }

        [Test]
        public void Save_RoundTrip_KeepsShinyAndDex()
        {
            Farm farm = CreateFarm(shiny: ShinyKind.DarkAurora);
            farm.CompleteFishing(CatchOne(farm));

            Farm loaded = RoundTrip(farm);

            Assert.AreEqual(ShinyKind.DarkAurora, loaded.State.Fishing.Basket[0].Shiny);
            Assert.AreEqual(1, loaded.State.Fishing.ShinyDex.Count);
            ShinyRecord record = loaded.State.Fishing.ShinyDex[0];
            Assert.AreEqual(Ro, record.FishId);
            Assert.AreEqual(ShinyKind.DarkAurora, record.Kind);
            Assert.AreEqual(farm.State.Fishing.ShinyDex[0].FirstCaughtAt, record.FirstCaughtAt);
        }

        [Test]
        public void Load_UnknownShinyKind_ReadsAsNone()
        {
            Farm farm = CreateFarm();
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.fishing.basket = new[] { new SaveFish { id = Ro, grams = 120, shiny = 99 } };
            data.fishing.shinyDex = new[]
            {
                new SaveShiny { fishId = Ro, kind = 99 },
                new SaveShiny { fishId = Ro, kind = (int)ShinyKind.Gold, firstCaughtAt = 5 },
                new SaveShiny { fishId = Ro, kind = (int)ShinyKind.Gold, firstCaughtAt = 9 },
                new SaveShiny { fishId = "", kind = (int)ShinyKind.Ice },
                null,
            };

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState state));

            Assert.AreEqual(ShinyKind.None, state.Fishing.Basket[0].Shiny);
            Assert.AreEqual(1, state.Fishing.ShinyDex.Count);
            Assert.AreEqual(5, state.Fishing.ShinyDex[0].FirstCaughtAt);
        }

        [Test]
        public void Tuning_InvalidShinyKinds_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => MakeFishing(new FishingTuning
            {
                ShinyCommonKinds = new[] { ShinyKind.Gold, ShinyKind.Aurora },
            }));
            Assert.Throws<System.ArgumentException>(() => MakeFishing(new FishingTuning
            {
                ShinyRareKinds = new ShinyKind[0],
            }));
            Assert.Throws<System.ArgumentException>(() => MakeFishing(new FishingTuning
            {
                ShinyCommonKinds = new[] { ShinyKind.None },
            }));
        }

        private static FishingDef MakeFishing(FishingTuning tuning)
        {
            var fish = new[]
            {
                new FishDef(Ro, FishRarity.Common, 80, 300, 10, 1, new float[] { 1, 1, 1, 1, 1, 1, 1 }, FishDepth.All, null,
                    3, 4, 4f, 0.9f, 0),
            };
            var rods = new[] { new RodDef(Tre, 1, 0.03f, 1.2f, 0, 0, 0, 30, 0, 1) };
            var baits = new[] { new BaitDef(Giun, 2) };
            return new FishingDef(fish, rods, baits, Tre, Giun, 1, tuning: tuning);
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

        private void SetLocalHour(int hour)
        {
            _clock.UtcNowSeconds = Midnight + hour * 3600 - VietnamOffset;
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
            uint randomState = 12345, int roSlip = 0, int roXp = 1,
            int wormChance = 0, long coins = 0, bool roRed = false, ShinyKind shiny = ShinyKind.None)
        {
            FarmDef def = CreateDef(withFishing, unlockLevel, baits, roSlip, roXp, wormChance, roRed, shiny);
            FarmState state = FarmState.CreateNew(def);
            state.RandomState = randomState;
            state.Coins = coins;
            return new Farm(def, state, _clock);
        }

        private static FarmDef CreateDef(bool withFishing, int unlockLevel, int starterBaits, int roSlip, int roXp,
            int wormChance, bool roRed, ShinyKind shiny = ShinyKind.None)
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
                    new RodDef(Tre, 1, 0.03f, 1.2f, 0, 0, 0, 30, 0, 1),
                    new RodDef(Carbon, 4, 0.06f, 1.6f, 30, 10, 30, 120, 6000, 9),
                };
                var baitDefs = new[]
                {
                    new BaitDef(Giun, 2, buyable: false),
                    new BaitDef(Tep, 5),
                    new BaitDef(MoiThom, 25, rareBonusPercent: 100, unlockLevel: MoiThomLevel),
                };
                // Mặc định tắt shiny để test cũ không đổi giá ngẫu nhiên; shiny != None thì luôn ra đúng kiểu đó.
                var tuning = new FishingTuning
                {
                    RedArrowPercent = 100,
                    ShinyChancePer10000 = shiny == ShinyKind.None ? 0 : 10000,
                };
                if (shiny != ShinyKind.None)
                {
                    bool rare = tuning.IsRareShiny(shiny);
                    tuning.ShinyRareTierPercent = rare ? 100f : 0f;
                    if (rare)
                        tuning.ShinyRareKinds = new[] { shiny };
                    else
                        tuning.ShinyCommonKinds = new[] { shiny };
                }
                fishing = new FishingDef(fish, rods, baitDefs, Tre, Giun, starterBaits, unlockLevel,
                    StartSlots, SlotPrices, Giun, wormChance, 1, 2, tuning);
            }

            return new FarmDef(zones, crops, 0, seeds, 0, levels, null, null, fishing);
        }
    }
}
