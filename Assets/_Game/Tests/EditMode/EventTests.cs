using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    // Bộ máy sự kiện (Đợt 2 Phần 7): lịch theo ngày (seed, cooldown, ưu tiên, âm lịch), từng viên gạch hiệu ứng,
    // chốt lúc gieo, thư quà 1 lần / ngày, save.
    public class EventTests
    {
        private const string Zone1 = "khu1";
        private const string RauMuong = "rau_muong";   // 60 giây, nhóm LeafyGreen.
        private const string BiDo = "bi_do";           // 5 giờ, nhóm Fruit.
        private const string HoaCuc = "hoa_cuc";       // eventOnly.
        private const string Ro = "ca_ro";
        private const string Loc = "ca_loc";
        private const string Giun = "giun";
        private const string Can = "can_tre";

        private static readonly PlotId Plot0 = new PlotId(Zone1, 0, 0, 0);
        private static readonly PlotId Plot1 = new PlotId(Zone1, 0, 1, 0);

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock { UtcNowSeconds = VietnamTime(2026, 12, 1, 8) };
        }

        private static long VietnamTime(int year, int month, int day, int hour = 0)
            => (long)GameCalendar.DayKeyFromDate(year, month, day) * 86400 + hour * 3600L
               + 1767225600L - GameCalendar.OffsetSeconds;

        private static int Day(int year, int month, int day) => GameCalendar.DayKeyFromDate(year, month, day);

        // ---------- Lịch sự kiện ----------

        [Test]
        public void SameDay_SameResult_AnyQueryOrder()
        {
            EventsDef def = RandomPool(100, 3);
            var forward = new EventCalendar(def, Lunar());
            var backward = new EventCalendar(def, Lunar());
            var a = new string[300];
            for (int d = 0; d < 300; d++)
                a[d] = Ids(forward.GetEvents(d));
            for (int d = 299; d >= 0; d--)
                Assert.AreEqual(a[d], Ids(backward.GetEvents(d)), $"Ngày {d}");
        }

        [Test]
        public void Cooldown_NoRepeatWithinDays()
        {
            var calendar = new EventCalendar(RandomPool(100, 3), null);
            var last = new Dictionary<string, int>();
            for (int d = 0; d < 1000; d++)
            {
                foreach (EventDef e in calendar.GetEvents(d))
                {
                    if (last.TryGetValue(e.Id, out int prev))
                        Assert.Greater(d - prev, 3, $"{e.Id} lặp lại ngày {prev} → {d}");
                    last[e.Id] = d;
                }
            }
            Assert.AreEqual(4, last.Count, "Đủ 4 sự kiện đều ra");
        }

        [Test]
        public void RandomChance_About25Percent()
        {
            var calendar = new EventCalendar(RandomPool(25, 0), null);
            int days = 4000, hits = 0;
            for (int d = 0; d < days; d++)
                hits += calendar.GetEvents(d).Count;
            Assert.AreEqual(0.25, hits / (double)days, 0.03);
        }

        [Test]
        public void Fixed_BeatsRandom_AndStacks()
        {
            var events = new List<EventDef>(RandomPool(100, 0).Events)
            {
                new EventDef("noel", "Noel", "", new EventTrigger(EventTriggerKind.Solar, 24, 12, 2), null, priority: 1),
                new EventDef("ca_thang_12", "Cả tháng", "", new EventTrigger(EventTriggerKind.Solar, 1, 12, 31), null, priority: 5),
            };
            var calendar = new EventCalendar(new EventsDef(events, 100), Lunar());

            List<EventDef> xmas = calendar.GetEvents(Day(2026, 12, 25));
            Assert.AreEqual("ca_thang_12,noel", Ids(xmas), "Cố định chạy chồng, ưu tiên cao trước, không bốc ngẫu nhiên");
            Assert.AreEqual("ca_thang_12", Ids(calendar.GetEvents(Day(2026, 12, 26))));
            Assert.AreEqual(1, calendar.GetEvents(Day(2027, 1, 5)).Count, "Ngày thường (100%) bốc 1 sự kiện ngẫu nhiên");
        }

        [Test]
        public void LunarTriggers_MatchTable()
        {
            var events = new[]
            {
                new EventDef("tet", "Tết", "", new EventTrigger(EventTriggerKind.Lunar, 1, 1, 3), null),
                new EventDef("ong_tao", "Tiễn ông Táo", "", new EventTrigger(EventTriggerKind.Lunar, 23, 12, 3), null),
                new EventDef("ram", "Ngày trăng tròn", "", new EventTrigger(EventTriggerKind.LunarMonthly, 15, 0), null),
            };
            var calendar = new EventCalendar(new EventsDef(events, 0), Lunar());

            // Tết Đinh Mùi 06/02/2027 (DayKey 401), 3 ngày.
            Assert.AreEqual("", Ids(calendar.GetEvents(Day(2027, 2, 5))));
            Assert.AreEqual("tet", Ids(calendar.GetEvents(Day(2027, 2, 6))));
            Assert.AreEqual("tet", Ids(calendar.GetEvents(Day(2027, 2, 8))));
            Assert.AreEqual("", Ids(calendar.GetEvents(Day(2027, 2, 9))));
            // 23 tháng Chạp = 30/01/2027, 3 ngày.
            Assert.AreEqual("ong_tao", Ids(calendar.GetEvents(Day(2027, 1, 30))));
            Assert.AreEqual("ong_tao", Ids(calendar.GetEvents(Day(2027, 2, 1))));
            Assert.AreEqual("", Ids(calendar.GetEvents(Day(2027, 2, 2))));
            // Rằm mọi tháng: tháng 12 âm bắt đầu 08/01/2027 → Rằm 22/01/2027; tháng Giêng → 20/02/2027.
            Assert.AreEqual("ram", Ids(calendar.GetEvents(Day(2027, 1, 22))));
            Assert.AreEqual("ram", Ids(calendar.GetEvents(Day(2027, 2, 20))));
        }

        [Test]
        public void MinLevel_HidesEvent()
        {
            var e = new EventDef("hoi", "Hội", "", Today(), new[] { Price("all", 2f) }, minLevel: 5);
            Farm farm = CreateFarm(e);
            CollectionAssert.IsEmpty(farm.TodayEvents);
            Assert.AreEqual(5, farm.GetSellPrice(RauMuong));
            farm.State.Level = 5;
            Assert.AreEqual(1, farm.TodayEvents.Count);
            Assert.AreEqual(10, farm.GetSellPrice(RauMuong));
        }

        [Test]
        public void Windows_EffectOnlyInSlot()
        {
            var e = new EventDef("gio", "Gió", "", Today(), new[] { Price("all", 2f) }, windows: new[] { DaySlot.Morning });
            Farm farm = CreateFarm(e);
            Assert.AreEqual(10, farm.GetSellPrice(RauMuong), "8 giờ = Sáng");
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 1, 14);
            Assert.AreEqual(5, farm.GetSellPrice(RauMuong), "14 giờ = Chiều");
            Assert.AreEqual(1, farm.TodayEvents.Count, "Sự kiện vẫn hiện cả ngày");
        }

        // ---------- Giá ----------

        [Test]
        public void CropPrice_Targets_AndSell()
        {
            Farm group = CreateFarm(new EventDef("cho", "Chợ", "", Today(), new[] { Price("group:Fruit", 1.5f) }));
            Assert.AreEqual(5, group.GetSellPrice(RauMuong));
            Assert.AreEqual(30, group.GetSellPrice(BiDo));

            Farm crop = CreateFarm(new EventDef("cho", "Chợ", "", Today(), new[] { Price("crop:rau_muong", 1.5f), Price("all", 2f) }));
            Assert.AreEqual(15, crop.GetSellPrice(RauMuong), "Nhân dồn 1,5 × 2");
            crop.State.Inventory[RauMuong] = 4;
            long coins = crop.State.Coins;
            Assert.AreEqual(FarmResult.Ok, crop.Sell(RauMuong, 4));
            Assert.AreEqual(coins + 60, crop.State.Coins);
        }

        [Test]
        public void CropPrice_RandomOne_SameForEveryone()
        {
            var e = new EventDef("thuong_lai", "Thương lái", "", Today(), new[] { Price("random:1", 1.5f) });
            Farm a = CreateFarm(e);
            Farm b = CreateFarm(e);
            int raised = 0;
            foreach (string id in new[] { RauMuong, BiDo })
            {
                Assert.AreEqual(a.GetSellPrice(id), b.GetSellPrice(id));
                if (a.GetSellPrice(id) != (id == RauMuong ? 5 : 20))
                    raised++;
            }
            Assert.AreEqual(1, raised, "Đúng 1 cây được giá");
            Assert.AreEqual(1, a.RandomCrops(e, 1).Count);
            CollectionAssert.DoesNotContain(a.RandomCrops(e, 1), HoaCuc, "Không chọn cây sự kiện");
        }

        // ---------- Gieo (chốt lúc gieo) ----------

        [Test]
        public void GrowMult_LockedAtPlant_SurvivesEventEndAndSave()
        {
            var e = new EventDef("nang", "Nắng đẹp", "", Today(), new[] { new EventEffect(EventEffectType.GrowTimeMult, mult: 0.5f) });
            Farm farm = CreateFarm(e);
            farm.State.Seeds[BiDo] = 2;
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot0, BiDo));
            Assert.AreEqual(9000, farm.GetGrowSeconds(Plot0), "5 giờ × 0,5");

            // Sang ngày sau (hết sự kiện): cây đang lớn giữ nguyên; cây gieo mới lớn đủ 5 giờ.
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 2, 1);
            Assert.AreEqual(9000, farm.GetGrowSeconds(Plot0));
            Assert.IsTrue(farm.IsRipe(Plot0), "Gieo 8h hôm trước, đủ 2,5 giờ");
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot1, BiDo));
            Assert.AreEqual(18000, farm.GetGrowSeconds(Plot1));

            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));
            var reloaded = new Farm(farm.Definition, loaded, _clock);
            Assert.AreEqual(9000, reloaded.GetGrowSeconds(Plot0));
            Assert.AreEqual(18000, reloaded.GetGrowSeconds(Plot1));
        }

        [Test]
        public void AutoWater_OnPlant()
        {
            var e = new EventDef("mua", "Mưa rào", "", Today(), new[] { new EventEffect(EventEffectType.AutoWater) });
            Farm farm = CreateFarm(e);
            farm.State.Seeds[BiDo] = 1;
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot0, BiDo));
            Assert.IsTrue(farm.TryGetPlotState(Plot0, out PlotState plot) && plot.Watered);
            // Tưới giảm 20%: còn 4 giờ.
            Assert.AreEqual(4 * 3600, farm.GetRemainingSeconds(Plot0));
            Assert.AreEqual(FarmResult.AlreadyWatered, farm.Water(Plot0));
        }

        [Test]
        public void YieldBonus_OnlyLongCrops_LockedAtPlant()
        {
            var e = new EventDef("mua_ma", "Được mùa", "", Today(), new[] { new EventEffect(EventEffectType.YieldBonus, count: 1, hours: 4f) });
            Farm farm = CreateFarm(e);
            farm.State.Seeds[BiDo] = 1;
            farm.State.Seeds[RauMuong] = 1;
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot0, BiDo));
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot1, RauMuong));

            _clock.UtcNowSeconds = VietnamTime(2026, 12, 3, 8);
            Assert.AreEqual(FarmResult.Ok, farm.Harvest(Plot0));
            Assert.AreEqual(FarmResult.Ok, farm.Harvest(Plot1));
            Assert.AreEqual(3, farm.State.GetItems(BiDo), "2 + 1");
            Assert.AreEqual(2, farm.State.GetItems(RauMuong), "Cây ngắn không thêm");
        }

        // ---------- Đơn ----------

        [Test]
        public void OrderBonus_Targets()
        {
            var e = new EventDef("hoi_lang", "Hội làng", "", Today(), new[]
            {
                new EventEffect(EventEffectType.OrderBonus, EffectTarget.Parse("npc:ba_lua"), mult: 1.5f, xpMult: 2f),
            });
            Farm farm = CreateFarm(e);
            var lua = new OrderState { NpcId = "ba_lua", RewardCoins = 100, RewardXp = 10 };
            var ca = new OrderState { NpcId = "bac_ca", RewardCoins = 100, RewardXp = 10 };
            farm.GetOrderReward(lua, out long coins, out long xp);
            Assert.AreEqual(150, coins);
            Assert.AreEqual(20, xp);
            farm.GetOrderReward(ca, out coins, out xp);
            Assert.AreEqual(100, coins);
            Assert.AreEqual(10, xp);
        }

        [Test]
        public void ExtraOrder_Counts()
        {
            var e = new EventDef("cho_phien", "Chợ phiên", "", Today(), new[]
            {
                new EventEffect(EventEffectType.ExtraOrder, count: 1),
                new EventEffect(EventEffectType.ExtraOrder, count: 2),
            });
            Assert.AreEqual(3, CreateFarm(e).ExtraOrdersToday);
            Assert.AreEqual(0, CreateFarm().ExtraOrdersToday);
        }

        // ---------- Cây sự kiện ----------

        [Test]
        public void UnlockCrop_OnlyDuringEvent()
        {
            var e = new EventDef("dau_thang", "Chợ đầu tháng", "", Today(), new[] { new EventEffect(EventEffectType.UnlockCrop, id: HoaCuc) });
            Farm farm = CreateFarm(e);
            farm.State.Coins = 1000;
            farm.State.Level = 2;
            Assert.IsTrue(farm.IsCropUnlocked(HoaCuc));
            Assert.AreEqual(FarmResult.Ok, farm.BuySeeds(HoaCuc, 2));
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot0, HoaCuc));

            _clock.UtcNowSeconds = VietnamTime(2026, 12, 2, 8);
            Assert.IsFalse(farm.IsCropUnlocked(HoaCuc));
            Assert.AreEqual(FarmResult.CropLocked, farm.BuySeeds(HoaCuc, 1));
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot1, HoaCuc), "Hạt đã mua vẫn gieo được");
            Assert.IsTrue(farm.IsRipe(Plot0), "Cây đã gieo vẫn lớn sau sự kiện");
            Assert.AreEqual(FarmResult.Ok, farm.Harvest(Plot0));
        }

        [Test]
        public void EventOnlyCrop_NotInLevelUnlocks()
        {
            Farm farm = CreateFarm();
            farm.State.Level = 2;
            CollectionAssert.DoesNotContain(farm.GetUnlocksAtLevel(2).CropIds, HoaCuc);
            Assert.IsFalse(farm.IsCropUnlocked(HoaCuc), "Đủ cấp nhưng không có sự kiện");
        }

        // ---------- Thư quà ----------

        [Test]
        public void MailReward_OncePerDay()
        {
            var e = new EventDef("le", "Ngày lễ", "Chúc mừng!", new EventTrigger(EventTriggerKind.Solar, 1, 12, 2), new[]
            {
                new EventEffect(EventEffectType.MailReward, rewards: new[] { new Reward(RewardKind.Coins, null, 50) }),
            });
            Farm farm = CreateFarm(e);
            Assert.IsTrue(farm.TryEventMail());
            Assert.IsFalse(farm.TryEventMail(), "Cùng ngày không gửi lại");
            Assert.IsTrue(farm.State.Mailbox.TryGet(Farm.EventMailId("le", Day(2026, 12, 1)), out Mail mail));
            Assert.AreEqual("Ngày lễ", mail.Title);

            _clock.UtcNowSeconds = VietnamTime(2026, 12, 2, 8);
            Assert.IsTrue(farm.TryEventMail(), "Ngày thứ 2 của sự kiện: thư mới");
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 3, 8);
            Assert.IsFalse(farm.TryEventMail(), "Hết sự kiện");
        }

        // ---------- Cá ----------

        [Test]
        public void FishWeightMult_FavorsTarget()
        {
            var e = new EventDef("gio_mua", "Gió mùa", "", Today(), new[]
            {
                new EventEffect(EventEffectType.FishWeightMult, EffectTarget.Parse("rarity:Rare"), mult: 1000f),
            });
            Farm boosted = CreateFarm(e);
            Farm normal = CreateFarm();
            int boostedLoc = CountFish(boosted, Loc, 40, out _);
            int normalLoc = CountFish(normal, Loc, 40, out _);
            Assert.Greater(boostedLoc, 35);
            Assert.Less(normalLoc, 25);
        }

        [Test]
        public void FishSizeMult_CanExceedMax_PriceCapped()
        {
            var e = new EventDef("nuoc_len", "Nước lên", "", Today(), new[]
            {
                new EventEffect(EventEffectType.FishSizeMult, EffectTarget.Parse("fish:ca_ro"), mult: 3f),
            });
            Farm farm = CreateFarm(e);
            CountFish(farm, Ro, 30, out List<FishingSession> sessions);
            bool over = false;
            foreach (FishingSession s in sessions)
            {
                if (s.Fish.Id != Ro)
                    continue;
                Assert.GreaterOrEqual(s.Grams, 240);
                over |= s.Grams > 300;
                Assert.LessOrEqual(s.Price, 14, "Giá theo cân giữ trần (10 × 140%)");
            }
            Assert.IsTrue(over, "Có con vượt cân tối đa");
        }

        // ---------- Data ----------

        [Test]
        public void FarmDef_ValidatesEffects()
        {
            var notEventOnly = new EventDef("x", "", "", Today(), new[] { new EventEffect(EventEffectType.UnlockCrop, id: RauMuong) });
            Assert.Throws<System.ArgumentException>(() => CreateDef(notEventOnly));
            var badCrop = new EventDef("x", "", "", Today(), new[] { Price("crop:khong_co", 2f) });
            Assert.Throws<System.ArgumentException>(() => CreateDef(badCrop));
            var badFish = new EventDef("x", "", "", Today(), new[]
            {
                new EventEffect(EventEffectType.FishSizeMult, EffectTarget.Parse("fish:khong_co"), mult: 2f),
            });
            Assert.Throws<System.ArgumentException>(() => CreateDef(badFish));
        }

        [TestCase("all", EffectTargetKind.All)]
        [TestCase("crop:bi_do", EffectTargetKind.Crop)]
        [TestCase("group:Fruit", EffectTargetKind.Group)]
        [TestCase("random:2", EffectTargetKind.RandomCrop)]
        [TestCase("fish:ca_ro", EffectTargetKind.Fish)]
        [TestCase("rarity:Epic", EffectTargetKind.Rarity)]
        [TestCase("npc:ba_lua", EffectTargetKind.Npc)]
        [TestCase("fishorders", EffectTargetKind.FishOrders)]
        public void EffectTarget_Parse(string text, EffectTargetKind kind)
        {
            Assert.AreEqual(kind, EffectTarget.Parse(text).Kind);
        }

        [TestCase("group:Hoa")]
        [TestCase("random:0")]
        [TestCase("abc")]
        public void EffectTarget_ParseBad_Throws(string text)
        {
            Assert.Throws<System.FormatException>(() => EffectTarget.Parse(text));
        }

        [Test]
        public void Save_Version16()
        {
            Assert.AreEqual(16, SaveData.CurrentVersion);
        }

        // ---------- Hỗ trợ ----------

        private static string Ids(List<EventDef> events)
        {
            var ids = new List<string>();
            foreach (EventDef e in events)
                ids.Add(e.Id);
            return string.Join(",", ids);
        }

        // 4 sự kiện ngẫu nhiên cùng trọng số.
        private static EventsDef RandomPool(int chance, int cooldown)
        {
            var list = new List<EventDef>();
            foreach (string id in new[] { "mua_rao", "nang_dep", "nuoc_len", "hoi_lang" })
                list.Add(new EventDef(id, id, "", EventTrigger.Random, null, weight: 10, cooldownDays: cooldown));
            return new EventsDef(list, chance);
        }

        // Sự kiện cố định đúng ngày test (01/12/2026).
        private static EventTrigger Today() => new EventTrigger(EventTriggerKind.Solar, 1, 12);

        private static EventEffect Price(string target, float mult)
            => new EventEffect(EventEffectType.CropPriceMult, EffectTarget.Parse(target), mult: mult);

        private int CountFish(Farm farm, string fishId, int casts, out List<FishingSession> sessions)
        {
            sessions = new List<FishingSession>();
            int count = 0;
            for (int i = 0; i < casts; i++)
            {
                Assert.AreEqual(FarmResult.Ok, farm.Cast(Giun, 0.6f, out FishingSession s));
                Assert.AreEqual(FishingOutcome.None, s.Strike(s.BiteTime));
                Assert.AreEqual(FishingOutcome.Caught, s.CompletePull(0.1f));
                farm.CompleteFishing(s);
                farm.State.Fishing.Basket.Clear();
                sessions.Add(s);
                if (s.Fish.Id == fishId)
                    count++;
            }
            return count;
        }

        // 6 tháng âm thật (bảng sinh bằng Tools/lunar_table.py): tháng 9/2026 → tháng 2/2027.
        private static LunarTable Lunar() => new LunarTable(new[]
        {
            new LunarMonth(282, 30, 9, false, 2026),
            new LunarMonth(312, 30, 10, false, 2026),
            new LunarMonth(342, 30, 11, false, 2026),
            new LunarMonth(372, 29, 12, false, 2026),
            new LunarMonth(401, 30, 1, false, 2027),
            new LunarMonth(431, 30, 2, false, 2027),
        });

        private Farm CreateFarm(params EventDef[] events)
        {
            FarmDef def = CreateDef(events);
            FarmState state = FarmState.CreateNew(def);
            state.RandomState = 777;
            return new Farm(def, state, _clock);
        }

        private static FarmDef CreateDef(params EventDef[] events)
        {
            var zones = new[] { new ZoneDef(Zone1, new[] { new BedDef(2, 3) }, 6, new int[0]) };
            var crops = new[]
            {
                new CropDef(RauMuong, 60, 2, 5, 2, xp: 1),
                new CropDef(BiDo, 5 * 3600, 10, 20, 2, xp: 5, group: CropGroup.Fruit),
                new CropDef(HoaCuc, 3600, 20, 40, 1, xp: 3, unlockLevel: 2, eventOnly: true),
            };
            var levels = new LevelDef(new[] { 3, 20, 50, 100, 200, 300, 400, 500, 600 }, 0, 0);
            var fish = new[]
            {
                new FishDef(Ro, FishRarity.Common, 80, 300, 10, 1, new float[] { 1, 1, 1, 1, 1, 1, 1 }, FishDepth.All, null, 3, 4, 4f, 0.9f, 0),
                new FishDef(Loc, FishRarity.Rare, 500, 2500, 80, 8, new float[] { 1, 1, 1, 1, 1, 1, 1 }, FishDepth.All, null, 3, 4, 4f, 0.9f, 0),
            };
            var fishing = new FishingDef(fish, new[] { new RodDef(Can, 1, 0.03f, 1.2f, 0, 0, 0, 1000, 0, 1) },
                new[] { new BaitDef(Giun, 2) }, Can, Giun, 200, basketStartSlots: 40,
                tuning: new FishingTuning { ShinyChancePer10000 = 0 });
            return new FarmDef(zones, crops, 0, null, 20, levels, null, null, fishing,
                events: new EventsDef(events ?? new EventDef[0], 0));
        }
    }
}
