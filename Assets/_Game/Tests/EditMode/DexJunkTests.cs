using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    // Album cá + câu rác (Đợt 2 Phần 5): tỉ lệ rác theo mồi / kiểu ném, phiên rác, giỏ, tự bán, album, mốc thưởng, save.
    public class DexJunkTests
    {
        private const string Zone1 = "khu1";
        private const string RauMuong = "rau_muong";

        private const string Ro = "ca_ro";   // Thường, mọi giờ, mọi độ sâu.
        private const string Loc = "ca_loc"; // Hiếm, chỉ vừa / xa (ném gần không ra).
        private const string Can = "can_tre";
        private const string Giun = "giun";
        private const string MoiThom = "moi_thom";

        private const string Dep = "dep_to_ong";
        private const string Hu = "hu_tien_co";
        private const int DepPrice = 2;
        private const int HuPrice = 150;

        private const string Mail5 = "dex_species_1";
        private const string MailCommon = "dex_rarity_common";
        private const string MailRare = "dex_rarity_rare";

        private const long Midnight = 1798761600;

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock { UtcNowSeconds = Midnight + 8 * 3600 - GameCalendar.OffsetSeconds };
        }

        // ---------- Tỉ lệ rác ----------

        [Test]
        public void JunkChanceZero_NeverJunk_10000Rolls()
        {
            FishingDef def = CreateFishing(giunJunk: 0, roSlip: 0);
            def.TryGetBait(Giun, out BaitDef bait);
            uint random = 777;
            for (int i = 0; i < 10000; i++)
            {
                foreach (CastQuality quality in new[] { CastQuality.Weak, CastQuality.Normal, CastQuality.Good, CastQuality.Perfect })
                    Assert.IsNull(FishingSession.RollJunk(ref random, def, bait, quality));
            }
        }

        [TestCase(CastQuality.Normal, 0.12)]
        [TestCase(CastQuality.Weak, 0.12)]
        [TestCase(CastQuality.Good, 0.072)]
        [TestCase(CastQuality.Perfect, 0.036)]
        public void JunkChance_MatchesConfig(CastQuality quality, double expected)
        {
            FishingDef def = CreateFishing(giunJunk: 12, roSlip: 0);
            def.TryGetBait(Giun, out BaitDef bait);
            Assert.AreEqual((int)System.Math.Round(expected * 10000), def.Tuning.JunkChancePer10000(bait, quality));

            const int rolls = 40000;
            uint random = 12345;
            int junk = 0;
            for (int i = 0; i < rolls; i++)
            {
                if (FishingSession.RollJunk(ref random, def, bait, quality) != null)
                    junk++;
            }
            Assert.AreEqual(expected, junk / (double)rolls, 0.01);
        }

        [Test]
        public void JunkPick_FollowsWeights()
        {
            // Dép 10, hũ 2 → hũ ≈ 1/6 số lần ra rác.
            FishingDef def = CreateFishing(giunJunk: 100, roSlip: 0);
            def.TryGetBait(Giun, out BaitDef bait);
            uint random = 99;
            int hu = 0;
            const int rolls = 30000;
            for (int i = 0; i < rolls; i++)
            {
                JunkDef junk = FishingSession.RollJunk(ref random, def, bait, CastQuality.Normal);
                Assert.IsNotNull(junk);
                if (junk.Id == Hu)
                    hu++;
            }
            Assert.AreEqual(2.0 / 12.0, hu / (double)rolls, 0.01);
        }

        [Test]
        public void JunkChanceOffBait_SameFishSequenceAsBefore()
        {
            // Tung rác dùng random riêng: bật rác không làm xê dịch cá được chọn / lịch cắn của chuỗi câu.
            Farm a = CreateFarm(giunJunk: 0, baits: 50);
            Farm b = CreateFarm(giunJunk: 50, baits: 50);
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(FarmResult.Ok, a.Cast(Giun, 0.6f, out FishingSession sa));
                Assert.AreEqual(FarmResult.Ok, b.Cast(Giun, 0.6f, out FishingSession sb));
                Assert.AreEqual(sa.Fish.Id, sb.Fish.Id);
                Assert.AreEqual(sa.BiteTime, sb.BiteTime);
                sa.Abandon();
                sb.Abandon();
            }
        }

        // ---------- Phiên rác ----------

        [Test]
        public void JunkSession_ShortPull_NoSlip_NoShiny_NoXp()
        {
            // Cá trượt 100% nhưng rác không trượt; shiny 100% nhưng rác không shiny.
            Farm farm = CreateFarm(giunJunk: 100, roSlip: 100, shinyAlways: true);
            long xp = farm.State.Xp;
            FishingSession session = CastJunk(farm);
            Assert.AreEqual(ShinyKind.None, session.Shiny);

            Assert.AreEqual(FishingOutcome.None, session.Strike(session.BiteTime));
            Assert.That(session.Pull.Count, Is.InRange(2, 3));
            for (int i = 0; i < session.Pull.Count; i++)
                Assert.IsFalse(session.Pull.Red[i]);
            Assert.AreEqual(FishingOutcome.Caught, session.CompletePull(0.5f));
            Assert.AreEqual(0, session.Xp);
            Assert.AreEqual(0, session.Grams);
            Assert.AreEqual(session.Junk.SellPrice, session.Price);

            Assert.AreEqual(FarmResult.Ok, farm.CompleteFishing(session));
            Assert.AreEqual(xp, farm.State.Xp);
            Assert.AreEqual(0, farm.State.Fishing.Basket.Count);
            Assert.AreEqual(1, farm.State.Fishing.JunkBasket.Count);
            Assert.AreEqual(session.Junk.Id, farm.State.Fishing.JunkBasket[0].JunkId);
            Assert.AreEqual(1, farm.State.Fishing.BasketUsed);
            Assert.IsTrue(session.IsNewJunk);
            Assert.IsFalse(session.JunkAutoSold);
            CollectionAssert.IsEmpty(farm.State.Fishing.FishDex);
            Assert.AreEqual(1, farm.State.Fishing.JunkDex[session.Junk.Id].Count);
            Assert.AreEqual(farm.Calendar.DayKey, farm.State.Fishing.JunkDex[session.Junk.Id].FirstDayKey);
        }

        [Test]
        public void JunkSession_LosesBait()
        {
            Farm farm = CreateFarm(giunJunk: 100);
            int baits = farm.State.Fishing.GetBaits(Giun);
            FishingSession session = CatchJunk(farm);
            farm.CompleteFishing(session);
            Assert.AreEqual(baits - 1, farm.State.Fishing.GetBaits(Giun));
        }

        [Test]
        public void Junk_SecondTime_NotNew_CountUp()
        {
            Farm farm = CreateFarm(giunJunk: 100);
            var counts = new Dictionary<string, int>();
            for (int i = 0; i < 6; i++)
            {
                FishingSession session = CatchJunk(farm);
                bool seen = counts.ContainsKey(session.Junk.Id);
                farm.CompleteFishing(session);
                Assert.AreEqual(!seen, session.IsNewJunk);
                counts[session.Junk.Id] = (seen ? counts[session.Junk.Id] : 0) + 1;
            }
            foreach (KeyValuePair<string, int> pair in counts)
                Assert.AreEqual(pair.Value, farm.State.Fishing.JunkDex[pair.Key].Count);
        }

        [Test]
        public void AutoSellJunk_CoinsNotBasket()
        {
            Farm farm = CreateFarm(giunJunk: 100);
            farm.SetAutoSellJunk(true);
            long coins = farm.State.Coins;
            FishingSession session = CatchJunk(farm);
            farm.CompleteFishing(session);

            Assert.IsTrue(session.JunkAutoSold);
            Assert.AreEqual(coins + session.Junk.SellPrice, farm.State.Coins);
            Assert.AreEqual(0, farm.State.Fishing.BasketUsed);
            Assert.AreEqual(1, farm.State.Fishing.JunkDex[session.Junk.Id].Count);
        }

        [Test]
        public void Junk_FillsBasketSlots()
        {
            Farm farm = CreateFarm(giunJunk: 100, startSlots: 2);
            farm.CompleteFishing(CatchJunk(farm));
            farm.State.Fishing.Basket.Add(new CaughtFish { FishId = Ro, Grams = 100 });

            Assert.IsTrue(farm.IsBasketFull);
            Assert.AreEqual(FarmResult.BasketFull, farm.CanCast(Giun));
        }

        [Test]
        public void SellJunk_One_And_SellAllIncludesJunk()
        {
            Farm farm = CreateFarm(giunJunk: 0);
            List<CaughtJunk> junk = farm.State.Fishing.JunkBasket;
            junk.Add(new CaughtJunk { JunkId = Dep });
            junk.Add(new CaughtJunk { JunkId = Hu });
            junk.Add(new CaughtJunk { JunkId = Dep });
            farm.State.Fishing.Basket.Add(new CaughtFish { FishId = Ro, Grams = 100 });
            long fishPrice = farm.GetFishPrice(farm.State.Fishing.Basket[0]);
            long coins = farm.State.Coins;

            Assert.AreEqual(FarmResult.Ok, farm.SellJunk(1, out long earned));
            Assert.AreEqual(HuPrice, earned);
            Assert.AreEqual(coins + HuPrice, farm.State.Coins);
            Assert.AreEqual(2, junk.Count);
            Assert.AreEqual(FarmResult.UnknownFish, farm.SellJunk(5, out _));

            Assert.AreEqual(3, farm.GetSellAllCount());
            Assert.AreEqual(2 * DepPrice + fishPrice, farm.GetBasketValue());
            Assert.AreEqual(FarmResult.Ok, farm.SellAllFish(out long all));
            Assert.AreEqual(2 * DepPrice + fishPrice, all);
            Assert.AreEqual(0, farm.State.Fishing.BasketUsed);
        }

        [Test]
        public void SellAll_OnlyJunk_Works()
        {
            Farm farm = CreateFarm(giunJunk: 0);
            farm.State.Fishing.JunkBasket.Add(new CaughtJunk { JunkId = Dep });
            Assert.AreEqual(FarmResult.Ok, farm.SellAllFish(out long earned));
            Assert.AreEqual(DepPrice, earned);
        }

        // ---------- Album ----------

        [Test]
        public void Dex_CountMaxFirstDay()
        {
            Farm farm = CreateFarm(giunJunk: 0, baits: 50);
            int firstDay = farm.Calendar.DayKey;
            int max = 0;
            for (int i = 0; i < 4; i++)
            {
                FishingSession session = CatchFish(farm);
                farm.CompleteFishing(session);
                max = System.Math.Max(max, session.Grams);
                _clock.Advance(86400);
            }

            FishDexEntry entry = farm.State.Fishing.FishDex[Ro];
            Assert.AreEqual(4, entry.Count);
            Assert.AreEqual(max, entry.MaxGrams);
            Assert.AreEqual(max, farm.State.Fishing.GetRecord(Ro));
            Assert.AreEqual(firstDay, entry.FirstDayKey);
            Assert.AreEqual(1, farm.DexSpeciesCount);
        }

        [Test]
        public void Milestones_SentOnce()
        {
            Farm farm = CreateFarm(giunJunk: 0, baits: 50);
            FishingSession first = CatchFish(farm);
            farm.CompleteFishing(first);

            var reached = new List<string>();
            foreach (DexMilestoneDef m in first.ReachedMilestones)
                reached.Add(m.Id);
            CollectionAssert.AreEquivalent(new[] { Mail5, MailCommon }, reached);
            Assert.IsTrue(farm.State.Mailbox.TryGet(Mail5, out Mail mail));
            Assert.AreEqual(500, mail.Rewards[0].Amount);
            Assert.IsTrue(farm.State.Mailbox.TryGet(MailCommon, out _));
            Assert.IsFalse(farm.State.Mailbox.TryGet(MailRare, out _));

            // Nhận quà, thư bị dọn sau 7 ngày, câu tiếp: không gửi lại.
            Assert.AreEqual(FarmResult.Ok, farm.ClaimAllMail(out _));
            _clock.Advance(8 * 86400);
            farm.RefreshMail();
            Assert.IsFalse(farm.State.Mailbox.TryGet(Mail5, out _));
            FishingSession second = CatchFish(farm);
            farm.CompleteFishing(second);
            CollectionAssert.IsEmpty(second.ReachedMilestones);
            Assert.IsFalse(farm.State.Mailbox.TryGet(Mail5, out _));

            // Qua save vẫn không gửi lại.
            Farm loaded = RoundTrip(farm);
            FishingSession third = CatchFish(loaded);
            loaded.CompleteFishing(third);
            CollectionAssert.IsEmpty(third.ReachedMilestones);
        }

        [Test]
        public void Milestone_RarityComplete_NeedsAllOfRarity()
        {
            Farm farm = CreateFarm(giunJunk: 0);
            DexMilestoneDef rare = null;
            foreach (DexMilestoneDef m in farm.Definition.Fishing.DexMilestones)
            {
                if (m.Id == MailRare)
                    rare = m;
            }
            Assert.IsFalse(farm.IsDexMilestoneReached(rare));
            farm.State.Fishing.FishDex[Loc] = new FishDexEntry { Count = 1, MaxGrams = 600 };
            Assert.IsTrue(farm.IsDexMilestoneReached(rare));
        }

        [Test]
        public void DexSpecies_IgnoresFishNotInData()
        {
            Farm farm = CreateFarm(giunJunk: 0);
            farm.State.Fishing.FishDex["ca_da_bo"] = new FishDexEntry { Count = 1, MaxGrams = 100 };
            Assert.AreEqual(0, farm.DexSpeciesCount);
        }

        [Test]
        public void Milestone_InvalidReward_Throws()
        {
            var bad = new DexMilestoneDef("x", DexMilestoneKind.SpeciesCount, 1, FishRarity.Common, "", "", "",
                new[] { new Reward(RewardKind.Bait, "khong_co", 1) });
            Assert.Throws<System.ArgumentException>(() => CreateDef(0, 0, 0, 10, 8, false, new[] { bad }));
        }

        [Test]
        public void JunkIdSameAsFish_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => new FishingDef(
                new[] { RoDef(0) }, new[] { CanDef() }, new[] { new BaitDef(Giun, 2) }, Can, Giun, 10,
                junk: new[] { new JunkDef(Ro, 1, 1) }));
        }

        // ---------- Save ----------

        [Test]
        public void Save_RoundTrip_DexJunkAutoSell()
        {
            Farm farm = CreateFarm(giunJunk: 0);
            FishingState fishing = farm.State.Fishing;
            fishing.FishDex[Ro] = new FishDexEntry { Count = 3, MaxGrams = 250, FirstDayKey = 320 };
            fishing.JunkDex[Hu] = new JunkDexEntry { Count = 2, FirstDayKey = 321 };
            fishing.JunkBasket.Add(new CaughtJunk { JunkId = Dep, CaughtAt = 99 });
            fishing.AutoSellJunk = true;

            FishingState loaded = RoundTrip(farm).State.Fishing;
            Assert.AreEqual(3, loaded.FishDex[Ro].Count);
            Assert.AreEqual(250, loaded.FishDex[Ro].MaxGrams);
            Assert.AreEqual(320, loaded.FishDex[Ro].FirstDayKey);
            Assert.AreEqual(2, loaded.JunkDex[Hu].Count);
            Assert.AreEqual(321, loaded.JunkDex[Hu].FirstDayKey);
            Assert.AreEqual(1, loaded.JunkBasket.Count);
            Assert.AreEqual(Dep, loaded.JunkBasket[0].JunkId);
            Assert.AreEqual(99, loaded.JunkBasket[0].CaughtAt);
            Assert.IsTrue(loaded.AutoSellJunk);
        }

        [Test]
        public void Load_DropsJunkNotInData()
        {
            Farm farm = CreateFarm(giunJunk: 0);
            farm.State.Fishing.JunkBasket.Add(new CaughtJunk { JunkId = "rac_da_bo" });
            farm.State.Fishing.JunkBasket.Add(new CaughtJunk { JunkId = Dep });
            Farm loaded = RoundTrip(farm);
            Assert.AreEqual(1, loaded.State.Fishing.JunkBasket.Count);
        }

        // ---------- Hỗ trợ ----------

        // Ném gần bờ (chỉ cá rô), lực 0.3 = Thường.
        private static FishingSession CastJunk(Farm farm)
        {
            Assert.AreEqual(FarmResult.Ok, farm.Cast(Giun, 0.3f, out FishingSession session));
            Assert.IsTrue(session.IsJunk);
            return session;
        }

        private static FishingSession CatchJunk(Farm farm)
        {
            FishingSession session = CastJunk(farm);
            Assert.AreEqual(FishingOutcome.None, session.Strike(session.BiteTime));
            Assert.AreEqual(FishingOutcome.Caught, session.CompletePull(0.5f));
            return session;
        }

        private static FishingSession CatchFish(Farm farm)
        {
            Assert.AreEqual(FarmResult.Ok, farm.Cast(Giun, 0.3f, out FishingSession session));
            Assert.IsFalse(session.IsJunk);
            Assert.AreEqual(Ro, session.Fish.Id);
            Assert.AreEqual(FishingOutcome.None, session.Strike(session.BiteTime));
            Assert.AreEqual(FishingOutcome.Caught, session.CompletePull(0.5f));
            return session;
        }

        private Farm RoundTrip(Farm farm)
        {
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));
            return new Farm(farm.Definition, loaded, _clock);
        }

        private Farm CreateFarm(int giunJunk, int moiThomJunk = 0, int roSlip = 0, bool shinyAlways = false,
            int baits = 10, int startSlots = 8)
        {
            FarmDef def = CreateDef(giunJunk, moiThomJunk, roSlip, baits, startSlots, shinyAlways, DefaultMilestones());
            FarmState state = FarmState.CreateNew(def);
            state.RandomState = 4242;
            return new Farm(def, state, _clock);
        }

        private static FishingDef CreateFishing(int giunJunk, int roSlip)
            => CreateDef(giunJunk, 0, roSlip, 10, 8, false, DefaultMilestones()).Fishing;

        private static DexMilestoneDef[] DefaultMilestones() => new[]
        {
            new DexMilestoneDef(Mail5, DexMilestoneKind.SpeciesCount, 1, FishRarity.Common, "Hội câu", "1 loài", "",
                new[] { new Reward(RewardKind.Coins, null, 500), new Reward(RewardKind.Bait, MoiThom, 2) }),
            new DexMilestoneDef(MailCommon, DexMilestoneKind.RarityComplete, 0, FishRarity.Common, "Hội câu", "Thường", "",
                new[] { new Reward(RewardKind.Coins, null, 100) }),
            new DexMilestoneDef(MailRare, DexMilestoneKind.RarityComplete, 0, FishRarity.Rare, "Hội câu", "Hiếm", "",
                new[] { new Reward(RewardKind.Coins, null, 600) }),
        };

        private static FishDef RoDef(int slip) => new FishDef(Ro, FishRarity.Common, 80, 300, 10, 1,
            new float[] { 1, 1, 1, 1, 1, 1, 1 }, FishDepth.All, new[] { Giun }, 3, 4, 4f, 0.9f, slip);

        private static RodDef CanDef() => new RodDef(Can, 1, 0.03f, 1.2f, 0, 0, 0, 1000, 0, 1);

        private static FarmDef CreateDef(int giunJunk, int moiThomJunk, int roSlip, int baits, int startSlots,
            bool shinyAlways, DexMilestoneDef[] milestones)
        {
            var zones = new[] { new ZoneDef(Zone1, new[] { new BedDef(2, 3) }, 2, new[] { 10, 10, 10, 10 }) };
            var crops = new[] { new CropDef(RauMuong, 60, 5, 5, 2, xp: 1) };
            var levels = new LevelDef(new[] { 3, 20, 50, 100, 200, 300, 400, 500, 600 }, 0, 0);
            var fish = new[]
            {
                RoDef(roSlip),
                new FishDef(Loc, FishRarity.Rare, 500, 2500, 80, 8, new float[] { 1, 1, 1, 1, 1, 1, 1 },
                    FishDepth.Mid | FishDepth.Far, null, 5, 7, 3.5f, 0.6f, 0),
            };
            var bait = new[]
            {
                new BaitDef(Giun, 2, junkChancePercent: giunJunk),
                new BaitDef(MoiThom, 25, rareBonusPercent: 100, junkChancePercent: moiThomJunk),
            };
            var junk = new[] { new JunkDef(Dep, DepPrice, 10), new JunkDef(Hu, HuPrice, 2) };
            var tuning = new FishingTuning { ShinyChancePer10000 = shinyAlways ? 10000 : 0 };
            var fishing = new FishingDef(fish, new[] { CanDef() }, bait, Can, Giun, baits, 1, startSlots, null,
                tuning: tuning, junk: junk, dexMilestones: milestones);
            return new FarmDef(zones, crops, 0, null, 0, levels, null, null, fishing);
        }
    }
}
