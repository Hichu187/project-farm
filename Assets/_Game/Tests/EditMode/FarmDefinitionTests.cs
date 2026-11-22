using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.Tests
{
    // Kiểm tra data thật trong Assets/_Game/Data (tạo bằng menu Về Quê → Tạo data mặc định).
    public class FarmDefinitionTests
    {
        private const string FarmPath = "Assets/_Game/Data/FarmDefinition.asset";
        private const int PlotsPerZone = 24;

        private static FarmDefinition LoadAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<FarmDefinition>(FarmPath);
            Assert.IsNotNull(asset, $"Không thấy {FarmPath}. Chạy menu Về Quê → Tạo data mặc định.");
            return asset;
        }

        [Test]
        public void ToDef_DefaultData_CreatesStateWithExpectedPlots()
        {
            FarmDef def = LoadAsset().ToDef();
            FarmState state = FarmState.CreateNew(def);

            Assert.AreEqual(3, state.Zones.Length);
            for (int i = 0; i < state.Zones.Length; i++)
            {
                ZoneDef zone = def.Zones[i];
                Assert.AreEqual(2, zone.Beds.Count, zone.Id);
                Assert.AreEqual(PlotsPerZone, zone.PlotCount, zone.Id);
                Assert.AreEqual(PlotsPerZone, state.Zones[i].Plots.Length, zone.Id);
            }
            Assert.AreEqual(6, state.Zones[0].OpenCount);
            Assert.AreEqual(0, state.Zones[1].OpenCount);
            Assert.AreEqual(0, state.Zones[2].OpenCount);
        }

        [Test]
        public void ToDef_DefaultData_LaterZonesHaveOneFreePlot()
        {
            FarmDef def = LoadAsset().ToDef();

            Assert.AreEqual(6, def.Zones[0].StartOpenCount);
            Assert.AreEqual(1, def.Zones[1].StartOpenCount);
            Assert.AreEqual(1, def.Zones[2].StartOpenCount);
        }

        [Test]
        public void ToDef_DefaultData_HasStartCoinsAndSeeds()
        {
            FarmState state = FarmState.CreateNew(LoadAsset().ToDef());

            Assert.AreEqual(60, state.Coins);
            Assert.AreEqual(5, state.GetSeeds("rau_muong"));
        }

        [Test]
        public void ZoneDefinition_EveryPlot_CellAndIdRoundTrip()
        {
            foreach (ZoneDefinition zone in LoadAsset().Zones)
            {
                ZoneDef def = zone.ToDef();
                for (int i = 0; i < def.PlotCount; i++)
                {
                    PlotId id = def.PlotIdAt(i);
                    Assert.IsTrue(zone.TryGetPlotId(zone.GetCell(id), out PlotId result), id.ToString());
                    Assert.AreEqual(id, result);
                }
            }
        }

        [Test]
        public void ZoneDefinition_RowZero_IsTopOfBed()
        {
            ZoneDefinition zone = LoadAsset().Zones[0];
            BedDefinition bed = zone.Beds[0];

            Vector3Int cell = zone.GetCell(new PlotId(zone.Id, 0, 0, 0));

            Assert.AreEqual(bed.Origin + new Vector3Int(0, bed.Height - 1, 0), cell);
        }

        [Test]
        public void ZoneDefinition_GapBetweenBeds_IsNotAPlot()
        {
            ZoneDefinition zone = LoadAsset().Zones[0];
            BedDefinition lowerBed = zone.Beds[1];
            Vector3Int gap = lowerBed.Origin + new Vector3Int(0, lowerBed.Height, 0);

            Assert.IsFalse(zone.TryGetPlotId(gap, out _));
        }

        [Test]
        public void ToDef_DefaultData_WaterReducesTwentyPercent()
        {
            Assert.AreEqual(20, LoadAsset().ToDef().WaterReducePercent);
        }

        [Test]
        public void ToDef_DefaultData_RegularCropsEnabledEventCropsNot()
        {
            FarmDefinition asset = LoadAsset();
            FarmDef def = asset.ToDef();

            Assert.AreEqual(25, asset.Crops.Count);
            // 22 cây thường + 2 cây sự kiện bật từ Phần 8 (hoa cúc, hướng dương); hoa đào chờ Tết.
            Assert.AreEqual(24, def.Crops.Count);
            Assert.IsTrue(def.TryGetCrop("rau_muong", out CropDef rauMuong));
            Assert.AreEqual(120, rauMuong.GrowSeconds);
            Assert.IsTrue(def.TryGetCrop("ca_chua", out CropDef caChua));
            Assert.AreEqual(4 * 3600, caChua.GrowSeconds);
            Assert.IsTrue(def.TryGetCrop("bi_ngo", out CropDef biNgo));
            Assert.AreEqual(6 * 3600, biNgo.GrowSeconds);
            Assert.IsFalse(def.TryGetCrop("hoa_dao", out _));
            Assert.IsTrue(def.TryGetCrop("hoa_cuc", out CropDef cuc) && cuc.EventOnly);
            Assert.IsTrue(def.TryGetCrop("huong_duong", out CropDef duong) && duong.EventOnly);
            Assert.AreEqual(4 * 3600, cuc.GrowSeconds);
            Assert.AreEqual(2, cuc.HarvestYield);
            foreach (CropDef crop in def.Crops)
            {
                if (crop.Id != "hoa_cuc" && crop.Id != "huong_duong")
                    Assert.IsFalse(crop.EventOnly, crop.Id);
            }
            Assert.IsFalse(def.TryGetCrop("bi_do", out _), "cây cũ đã bỏ");
        }

        // Thứ tự trong data theo thời gian lớn ngoài đời: cây sau không chín nhanh hơn cây trước.
        [Test]
        public void ToDef_DefaultData_GrowTimesFollowRealLifeOrder()
        {
            FarmDef def = LoadAsset().ToDef();

            // Chỉ xét cây thường (cây sự kiện xếp cuối data, ngoài thứ tự này).
            CropDef previous = null;
            foreach (CropDef crop in def.Crops)
            {
                if (crop.EventOnly)
                    continue;
                if (previous != null)
                    Assert.GreaterOrEqual(crop.GrowSeconds, previous.GrowSeconds, crop.Id);
                previous = crop;
            }
        }

        // Mỗi cấp 1–13 có ít nhất 1 cây mới; cấp 14–15 dành cho cần và mồi.
        [Test]
        public void ToDef_DefaultData_EveryLevelUpTo13UnlocksACrop()
        {
            FarmDef def = LoadAsset().ToDef();

            for (int level = 1; level <= 13; level++)
            {
                bool found = false;
                foreach (CropDef crop in def.Crops)
                    found |= crop.UnlockLevel == level;
                Assert.IsTrue(found, $"cấp {level}");
            }
            foreach (CropDef crop in def.Crops)
                Assert.LessOrEqual(crop.UnlockLevel, 13, crop.Id);
        }

        [TestCase("rau_muong", CropGroup.LeafyGreen)]
        [TestCase("mong_toi", CropGroup.LeafyGreen)]
        [TestCase("dua_hau", CropGroup.Fruit)]
        [TestCase("ca_chua", CropGroup.Fruit)]
        [TestCase("lua", CropGroup.RootGrain)]
        [TestCase("bi_ngo", CropGroup.RootGrain)]
        public void ToDef_DefaultData_CropGroup(string id, CropGroup group)
        {
            Assert.IsTrue(LoadAsset().ToDef().TryGetCrop(id, out CropDef crop));
            Assert.AreEqual(group, crop.Group);
        }

        [Test]
        public void ToDef_DefaultData_UnlockPricesStartAt40AndIncrease()
        {
            FarmDef def = LoadAsset().ToDef();

            Assert.AreEqual(18, def.Zones[0].UnlockPrices.Count);
            Assert.AreEqual(23, def.Zones[1].UnlockPrices.Count);
            Assert.AreEqual(23, def.Zones[2].UnlockPrices.Count);
            Assert.AreEqual(40, def.Zones[0].UnlockPrices[0]);
            int previous = 0;
            foreach (ZoneDef zone in def.Zones)
            {
                foreach (int price in zone.UnlockPrices)
                {
                    Assert.Greater(price, previous, zone.Id);
                    previous = price;
                }
            }
        }

        // Bảng XP đã duyệt: 3 × cấp^3,2, cấp 1 → 15.
        [Test]
        public void ToDef_DefaultData_LevelTable()
        {
            LevelDef levels = LoadAsset().ToDef().Levels;

            CollectionAssert.AreEqual(
                new[] { 3, 28, 101, 253, 517, 927, 1519, 2328, 3394, 4755, 6450, 8521, 11009, 13955 },
                levels.XpToNext);
            Assert.AreEqual(15, levels.MaxLevel);
            Assert.AreEqual(20, levels.CoinsPerLevel);
            Assert.AreEqual(3, levels.UnlockSeedCount);
        }

        // Bảng cây ở thiet-ke-dot-2.md mục 1: (id, giá hạt, sản lượng, giá bán, XP, cấp mở).
        [TestCase("rau_muong", 5, 2, 5, 1, 1)]
        [TestCase("cai_xanh", 15, 2, 20, 4, 2)]
        [TestCase("dau_dua", 37, 4, 28, 9, 3)]
        [TestCase("ca_chua", 92, 3, 107, 22, 5)]
        [TestCase("bi_ngo", 121, 1, 440, 29, 7)]
        [TestCase("lua", 148, 2, 274, 35, 4)]
        [TestCase("khoai_so", 195, 3, 250, 46, 13)]
        public void ToDef_DefaultData_MvpCropBalance(string id, int seed, int yield, int sell, int xp, int level)
        {
            Assert.IsTrue(LoadAsset().ToDef().TryGetCrop(id, out CropDef crop));

            Assert.AreEqual(seed, crop.BuyPrice, "giá hạt");
            Assert.AreEqual(yield, crop.HarvestYield, "sản lượng");
            Assert.AreEqual(sell, crop.SellPrice, "giá bán");
            Assert.AreEqual(xp, crop.Xp, "XP");
            Assert.AreEqual(level, crop.UnlockLevel, "cấp mở");
        }

        [Test]
        public void ToDef_DefaultData_ZoneLevelsAndZone3Total()
        {
            FarmDef def = LoadAsset().ToDef();

            Assert.AreEqual(1, def.Zones[0].RequiredLevel);
            Assert.AreEqual(5, def.Zones[1].RequiredLevel);
            Assert.AreEqual(9, def.Zones[2].RequiredLevel);
            long total = 0;
            foreach (int price in def.Zones[2].UnlockPrices)
                total += price;
            Assert.AreEqual(614075, total);
        }

        [Test]
        public void ToDef_DefaultData_RescueCropIsRauMuong()
        {
            Assert.AreEqual("rau_muong", LoadAsset().ToDef().RescueCropId);
        }

        [Test]
        public void ToDef_DefaultData_Orders()
        {
            OrderDef orders = LoadAsset().ToDef().Orders;

            // 5 ô, chờ 10 phút (khẩn cấp 30), 20% khẩn cấp ×2 hết hạn 30 phút, 15 đơn/ngày (chốt 12/11/2026).
            Assert.AreEqual(5, orders.SlotCount);
            Assert.AreEqual(2, orders.UnlockLevel);
            Assert.AreEqual(160, orders.CoinPercent);
            Assert.AreEqual(200, orders.XpPercent);
            Assert.AreEqual(300, orders.DiscardCooldownSeconds);
            Assert.AreEqual(6, orders.MaxHarvestsCap);
            Assert.AreEqual(600, orders.RefillSeconds);
            Assert.AreEqual(1800, orders.UrgentRefillSeconds);
            Assert.AreEqual(20, orders.UrgentChancePercent);
            Assert.AreEqual(200, orders.UrgentRewardPercent);
            Assert.AreEqual(1800, orders.UrgentExpireSeconds);
            Assert.AreEqual(15, orders.DailyLimit);
            Assert.GreaterOrEqual(orders.Dialogue.Npcs.Count, 1);
        }

        [Test]
        public void BoundingBox_DefaultData_IsSixByFive()
        {
            foreach (ZoneDefinition zone in LoadAsset().Zones)
            {
                RectInt box = zone.BoundingBox;
                Assert.AreEqual(6, box.width, zone.Id);
                Assert.AreEqual(5, box.height, zone.Id);
            }
        }

        // ---------- Câu cá (Phần 10 giai đoạn 4) ----------

        [Test]
        public void ToDef_DefaultData_FishingEnabled()
        {
            FishingDef fishing = LoadAsset().ToDef().Fishing;

            Assert.IsTrue(fishing.IsEnabled, "Chạy Về Quê → Tạo data câu cá.");
            Assert.AreEqual(20, fishing.Fish.Count);
            // 14/11/2026: mở rộng 10 cần, 10 mồi.
            Assert.AreEqual(10, fishing.Rods.Count);
            Assert.AreEqual(10, fishing.Baits.Count);
            // Cấp mở mua mồi (duyệt 13/11/2026, mở rộng 14/11/2026).
            var baitLevels = new System.Collections.Generic.Dictionary<string, int>
            {
                { "cam", 1 }, { "moi_ruoi", 2 }, { "moi_tep", 3 }, { "moi_de", 4 }, { "moi_nhai", 5 },
                { "moi_gia", 6 }, { "moi_thom", 9 }, { "moi_nhai_hoi", 11 }, { "moi_thia_vang", 13 },
            };
            foreach (var pair in baitLevels)
            {
                Assert.IsTrue(fishing.TryGetBait(pair.Key, out BaitDef bait), pair.Key);
                Assert.AreEqual(pair.Value, bait.UnlockLevel, pair.Key);
            }
            Assert.AreEqual(1, fishing.UnlockLevel);
            Assert.AreEqual("can_tre", fishing.StarterRodId);
            Assert.AreEqual("giun", fishing.StarterBaitId);
            Assert.AreEqual(10, fishing.StarterBaitCount);
            Assert.AreEqual(8, fishing.BasketStartSlots);
            Assert.AreEqual(25, fishing.BasketMaxSlots);
            CollectionAssert.AreEqual(new long[] { 100, 130, 170, 220, 290, 370, 480, 630, 820, 1060, 1380, 1790, 2330, 3030, 3940, 5120, 6650 },
                fishing.BasketSlotPrices);
            Assert.AreEqual("giun", fishing.WormBaitId);
            Assert.AreEqual(10, fishing.WormChancePercent);
        }

        // Đợt 2 Phần 5: tỉ lệ rác theo mồi (thiet-ke-dot-2 mục 3), 7 món rác, mốc album, gợi ý cá không nhắc giờ.
        [Test]
        public void ToDef_DefaultData_JunkAndDex()
        {
            FarmDefinition asset = LoadAsset();
            FishingDef fishing = asset.ToDef().Fishing;

            var junkChance = new System.Collections.Generic.Dictionary<string, int>
            {
                { "giun", 12 }, { "cam", 12 }, { "moi_ruoi", 9 }, { "moi_tep", 9 }, { "moi_de", 6 }, { "moi_nhai", 6 },
                { "moi_gia", 3 }, { "moi_thom", 0 }, { "moi_nhai_hoi", 0 }, { "moi_thia_vang", 0 },
            };
            foreach (var pair in junkChance)
            {
                Assert.IsTrue(fishing.TryGetBait(pair.Key, out BaitDef bait), pair.Key);
                Assert.AreEqual(pair.Value, bait.JunkChancePercent, pair.Key);
            }

            // 21/11/2026: 10 món theo art người dùng (9 thường cùng trọng số + hũ tiền xu cổ hiếm).
            Assert.AreEqual(10, fishing.Junk.Count, "Chạy Về Quê → Data → Tạo data câu cá.");
            Assert.IsTrue(fishing.TryGetJunk("hu_tien_co", out JunkDef jar));
            int commonWeight = -1;
            foreach (JunkDef junk in fishing.Junk)
            {
                Assert.IsTrue(asset.Fishing.TryGetJunk(junk.Id, out JunkDefinition view), junk.Id);
                Assert.IsNotNull(view.Icon, junk.Id);
                // Art thật 32×32, import Single (cả hình, không phải 1 ô 16×16 cắt nhầm), PPU 32 như icon cá.
                Assert.AreEqual(32f, view.Icon.rect.width, junk.Id);
                Assert.AreEqual(32f, view.Icon.rect.height, junk.Id);
                Assert.AreEqual(32f, view.Icon.pixelsPerUnit, junk.Id);
                Assert.IsFalse(string.IsNullOrEmpty(view.DisplayName), junk.Id);
                Assert.IsFalse(string.IsNullOrEmpty(view.FlavorText), junk.Id);
                if (junk == jar)
                    continue;
                if (commonWeight < 0)
                    commonWeight = junk.Weight;
                Assert.AreEqual(commonWeight, junk.Weight, $"Rác thường cùng trọng số: {junk.Id}");
                // Hũ tiền xu cổ hiếm hơn và đắt hơn mọi món rác thường.
                Assert.Greater(junk.Weight, jar.Weight, junk.Id);
                Assert.Less(junk.SellPrice, jar.SellPrice, junk.Id);
            }

            // Mốc 5 / 10 / 15 / 20 loài + đủ từng bậc độ hiếm; quà hợp lệ đã được FarmDef kiểm lúc ToDef.
            var species = new System.Collections.Generic.List<int>();
            var rarities = new System.Collections.Generic.List<FishRarity>();
            foreach (DexMilestoneDef m in fishing.DexMilestones)
            {
                if (m.Kind == DexMilestoneKind.SpeciesCount)
                    species.Add(m.Count);
                else
                    rarities.Add(m.Rarity);
                Assert.IsFalse(string.IsNullOrEmpty(m.Title), m.Id);
                Assert.Greater(m.Rewards.Count, 0, m.Id);
            }
            CollectionAssert.AreEqual(new[] { 5, 10, 15, 20 }, species);
            CollectionAssert.AreEquivalent(new[] { FishRarity.Common, FishRarity.Uncommon, FishRarity.Rare, FishRarity.Epic, FishRarity.Legendary },
                rarities);

            // Gợi ý: có cho mọi loài, không nhắc giờ (giờ ra cá chỉ dev biết – chốt 21/11).
            string[] timeWords = { "giờ", "sáng", "trưa", "chiều", "tối", "đêm", "khuya", "bình minh", "hoàng hôn", "ban ngày" };
            foreach (FishDefinition fish in asset.Fishing.Fish)
            {
                Assert.IsFalse(string.IsNullOrEmpty(fish.Hint), fish.Id);
                string hint = fish.Hint.ToLowerInvariant();
                foreach (string word in timeWords)
                    Assert.IsFalse(hint.Contains(word), $"Gợi ý '{fish.Id}' nhắc giờ: \"{word}\"");
            }
        }

        // Đợt 2 Phần 10: điểm danh 7 ngày, ngày 7 = xu + 1 cần (quà đã được FarmDef kiểm hợp lệ lúc ToDef).
        [Test]
        public void ToDef_DefaultData_CheckIn()
        {
            CheckInDef checkIn = LoadAsset().ToDef().CheckIn;
            Assert.IsTrue(checkIn.IsEnabled, "Chạy Về Quê → Tạo data mặc định (gán check_in.json).");
            Assert.AreEqual(18, checkIn.MinHoursBetween);
            bool coins = false, rod = false;
            foreach (Reward reward in checkIn.GetBaseRewards(7))
            {
                coins |= reward.Kind == RewardKind.Coins;
                rod |= reward.Kind == RewardKind.Rod;
            }
            Assert.IsTrue(coins && rod, "Ngày 7: xu + 1 cần câu");
            StringAssert.Contains("{day}", checkIn.TitleFormat);
        }

        // Đợt 2 Phần 7: 8 sự kiện ngẫu nhiên ban đầu (thiết kế mục 4), 33% (22/11); hiệu ứng đã được FarmDef kiểm lúc ToDef.
        [Test]
        public void ToDef_DefaultData_Events()
        {
            FarmDef def = LoadAsset().ToDef();
            EventsDef events = def.Events;
            Assert.AreEqual(33, events.RandomChancePercent, "Chạy Về Quê → Tạo data mặc định (gán events.json).");
            var ids = new System.Collections.Generic.List<string>();
            foreach (EventDef e in events.Events)
            {
                if (e.Trigger.Kind == EventTriggerKind.Random)
                    ids.Add(e.Id);
                Assert.IsFalse(string.IsNullOrEmpty(e.Title), e.Id);
                Assert.IsFalse(string.IsNullOrEmpty(e.LoaText), e.Id);
                Assert.Greater(e.Effects.Count, 0, e.Id);
            }
            CollectionAssert.AreEquivalent(
                new[] { "mua_rao", "nang_dep", "thuong_lai", "cho_phien", "nuoc_len", "gio_mua", "hoi_lang", "duoc_mua" }, ids);

            // Một năm: mọi sự kiện đều ra, tổng số ngày có sự kiện ngẫu nhiên quanh 33%.
            var calendar = new EventCalendar(events, def.Lunar);
            var seen = new System.Collections.Generic.HashSet<string>();
            int randomDays = 0, plainDays = 0;
            for (int d = GameCalendar.DayKeyFromDate(2027, 1, 1); d < GameCalendar.DayKeyFromDate(2028, 1, 1); d++)
            {
                if (calendar.HasFixed(d))
                    continue;
                plainDays++;
                foreach (EventDef e in calendar.GetEvents(d))
                {
                    seen.Add(e.Id);
                    randomDays++;
                }
            }
            CollectionAssert.AreEquivalent(ids, seen);
            Assert.AreEqual(0.33, randomDays / (double)plainDays, 0.06);
        }

        // Đợt 2 Phần 8: Chợ đầu tháng (mùng 1 âm) mở hoa cúc / hướng dương + nhóm quả ×1,5;
        // Ngày trăng tròn (Rằm) cá Quý / Huyền thoại ×2 cả ngày.
        [Test]
        public void DefaultData_Mung1AndRam()
        {
            FarmDef def = LoadAsset().ToDef();
            // Mùng 1 tháng 11 âm = 09/12/2026; Rằm tháng Chạp = 22/01/2027 (bảng âm lịch).
            long mung1 = VietnamUtc(2026, 12, 9, 8);
            long ram = VietnamUtc(2027, 1, 22, 3);
            var clock = new FakeClock { UtcNowSeconds = mung1 };
            var farm = new Farm(def, FarmState.CreateNew(def), clock);

            Assert.IsTrue(ContainsEvent(farm, "cho_dau_thang"), "Mùng 1: Chợ đầu tháng");
            Assert.IsTrue(farm.IsCropUnlocked("hoa_cuc"));
            Assert.IsTrue(farm.IsCropUnlocked("huong_duong"));
            def.TryGetCrop("dua_hau", out CropDef duaHau);
            Assert.AreEqual(1.5f, farm.GetCropPriceMult(duaHau), 1e-5, "Nhóm quả ×1,5");
            Assert.IsTrue(farm.TryEventMail(), "Quà hạt hoa trong hộp thư");

            clock.UtcNowSeconds = VietnamUtc(2026, 12, 10, 8);
            Assert.IsFalse(farm.IsCropUnlocked("hoa_cuc"), "Mùng 2: hết bán hạt hoa");

            clock.UtcNowSeconds = ram;
            Assert.IsTrue(ContainsEvent(farm, "trang_tron"), "Rằm: Ngày trăng tròn");
            def.Fishing.TryGetFish("ca_chep_vang", out FishDef legendary);
            def.Fishing.TryGetFish("ca_ro", out FishDef common);
            Assert.AreEqual(2f, farm.GetFishWeightMult(legendary), 1e-5, "3 giờ sáng (Khuya)");
            Assert.AreEqual(1f, farm.GetFishWeightMult(common), 1e-5);
            clock.UtcNowSeconds = ram + 11 * 3600;
            Assert.AreEqual(2f, farm.GetFishWeightMult(legendary), 1e-5, "14 giờ (Chiều) – cả ngày");
        }

        // Đợt 2 Phần 12a: Tết 2027 (mùng 1 = 06/02/2027). Đón Tết từ 23 tháng Chạp (30/01), trang trí tới mùng 10 (15/02),
        // lì xì mùng 1–3 mỗi ngày 1 thư đúng người gửi, không gửi lại sau khi nạp save.
        [Test]
        public void DefaultData_Tet2027()
        {
            FarmDef def = LoadAsset().ToDef();
            var clock = new FakeClock { UtcNowSeconds = VietnamUtc(2027, 1, 29, 9) };
            var farm = new Farm(def, FarmState.CreateNew(def), clock);
            var overlays = new System.Collections.Generic.List<string>();

            Assert.IsFalse(ContainsEvent(farm, "don_tet"), "22 tháng Chạp chưa đón Tết");
            farm.GetOverlays(overlays);
            CollectionAssert.DoesNotContain(overlays, "tet_trang_tri");

            clock.UtcNowSeconds = VietnamUtc(2027, 1, 30, 9);
            Assert.IsTrue(ContainsEvent(farm, "don_tet"), "23 tháng Chạp");
            farm.GetOverlays(overlays);
            CollectionAssert.Contains(overlays, "tet_trang_tri");

            string[] senders = { "Mẹ", "Bà Lụa", "Trưởng thôn" };
            for (int i = 0; i < 3; i++)
            {
                clock.UtcNowSeconds = VietnamUtc(2027, 2, 6 + i, 9);
                Assert.IsTrue(ContainsEvent(farm, "tet_nguyen_dan"), $"Mùng {i + 1}");
                Assert.AreEqual("tet_nguyen_dan", farm.TodayEvents[0].Id, "Tết hiện đầu (loa xã chúc Tết)");
                farm.TryEventMail();
                string id = Farm.EventMailId($"li_xi_mung_{i + 1}", farm.Calendar.DayKey);
                Assert.IsTrue(farm.State.Mailbox.TryGet(id, out Mail mail), id);
                Assert.AreEqual(senders[i], mail.Sender);
                Assert.IsTrue(mail.HasRewards);
            }
            // Nạp lại save cùng ngày: không gửi lại lì xì mùng 3.
            SaveData data = SaveMapper.ToSave(farm.State, def, clock.UtcNowSeconds);
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, def, out FarmState loaded));
            var reloaded = new Farm(def, loaded, clock);
            Assert.IsFalse(reloaded.TryEventMail());

            clock.UtcNowSeconds = VietnamUtc(2027, 2, 9, 9);
            Assert.IsFalse(ContainsEvent(farm, "li_xi_mung_3"), "Mùng 4 hết lì xì");
            clock.UtcNowSeconds = VietnamUtc(2027, 2, 15, 9);
            farm.GetOverlays(overlays);
            CollectionAssert.Contains(overlays, "tet_trang_tri", "Mùng 10 vẫn trang trí");
            clock.UtcNowSeconds = VietnamUtc(2027, 2, 16, 9);
            Assert.IsFalse(ContainsEvent(farm, "tet_nguyen_dan"), "Mùng 11 hết Tết");
            farm.GetOverlays(overlays);
            CollectionAssert.DoesNotContain(overlays, "tet_trang_tri");
        }

        private static bool ContainsEvent(Farm farm, string id)
        {
            foreach (EventDef e in farm.TodayEvents)
            {
                if (e.Id == id)
                    return true;
            }
            return false;
        }

        private static long VietnamUtc(int year, int month, int day, int hour)
            => (long)GameCalendar.DayKeyFromDate(year, month, day) * 86400 + hour * 3600L + 1767225600L - GameCalendar.OffsetSeconds;

        // Đợt 2 Phần 9: giá chợ theo thiết kế mục 6 (0,85–1,15; hút hàng ×1,4; rớt giá ×0,7; rau lá ±10%).
        [Test]
        public void ToDef_DefaultData_Market()
        {
            FarmDef def = LoadAsset().ToDef();
            MarketDef market = def.Market;
            Assert.IsTrue(market.IsEnabled, "Chạy Về Quê → Tạo data mặc định.");
            Assert.AreEqual(0.85f, market.DailyMin, 1e-5);
            Assert.AreEqual(1.15f, market.DailyMax, 1e-5);
            Assert.AreEqual(1.4f, market.HotMult, 1e-5);
            Assert.AreEqual(0.7f, market.ColdMult, 1e-5);
            Assert.AreEqual(10, market.LeafyMorningPercent);
            Assert.AreEqual(10, market.LeafyNightPercent);
            // Mỗi ngày trong 1 năm: đúng 1 cây hút hàng, 1 cây rớt giá, khác nhau.
            var service = new MarketService(market, def.Crops);
            for (int d = 0; d < 365; d++)
            {
                Assert.IsNotNull(service.GetHotCrop(d));
                Assert.AreNotEqual(service.GetHotCrop(d), service.GetColdCrop(d));
            }
        }

        [Test]
        public void ToDef_DefaultData_NewFarmGetsStarterRodAndWorms()
        {
            FarmDef def = LoadAsset().ToDef();
            var farm = new Farm(def, FarmState.CreateNew(def), new FakeClock { UtcNowSeconds = 1798761600 });

            Assert.AreEqual("can_tre", farm.State.Fishing.EquippedRodId);
            Assert.AreEqual(10, farm.State.Fishing.GetBaits("giun"));
            Assert.AreEqual(FarmResult.Ok, farm.CanCast("giun"));
        }

        [Test]
        public void ToDef_DefaultData_EverySlotHasCommonFishNearShore()
        {
            FishingDef fishing = LoadAsset().ToDef().Fishing;

            // Chốt 12/11/2026: ném hỏng (gần bờ) khung giờ nào cũng có cá Thường.
            for (int s = 0; s < DaySlots.Count; s++)
            {
                bool found = false;
                foreach (FishDef fish in fishing.Fish)
                {
                    if (fish.Rarity == FishRarity.Common && (fish.Depths & FishDepth.Near) != 0
                        && fish.GetSlotFactor((DaySlot)s) > 0f)
                        found = true;
                }
                Assert.IsTrue(found, ((DaySlot)s).ToString());
            }
        }

        [Test]
        public void ToDef_DefaultData_RareOrBelowHasTwoSlots_RarestHavePreferredBait()
        {
            FishingDef fishing = LoadAsset().ToDef().Fishing;

            foreach (FishDef fish in fishing.Fish)
            {
                int slots = 0;
                for (int s = 0; s < DaySlots.Count; s++)
                {
                    if (fish.GetSlotFactor((DaySlot)s) > 0f)
                        slots++;
                }
                Assert.GreaterOrEqual(slots, 1, fish.Id);
                if (fish.Rarity <= FishRarity.Rare)
                    Assert.GreaterOrEqual(slots, 2, fish.Id);

                Assert.GreaterOrEqual(fish.PreferredBaits.Count, 1, fish.Id);
                foreach (string bait in fish.PreferredBaits)
                    Assert.IsTrue(fishing.TryGetBait(bait, out _), $"{fish.Id}: mồi '{bait}'");
                Assert.AreEqual(fish.Rarity == FishRarity.Legendary, fish.RedArrows, fish.Id);
            }
        }

        // 14/11/2026: chuỗi kéo cá tối thiểu 4 mũi tên.
        [Test]
        public void ToDef_DefaultData_FishHaveAtLeastMinPullArrows()
        {
            FishingDef fishing = LoadAsset().ToDef().Fishing;

            Assert.AreEqual(4, fishing.Tuning.MinPullArrows);
            foreach (FishDef fish in fishing.Fish)
                Assert.GreaterOrEqual(fish.MinArrows, fishing.Tuning.MinPullArrows, fish.Id);
        }

        [Test]
        public void ToDef_DefaultData_RodsGetBetterAndCostMore()
        {
            FishingDef fishing = LoadAsset().ToDef().Fishing;

            for (int i = 1; i < fishing.Rods.Count; i++)
            {
                RodDef prev = fishing.Rods[i - 1];
                RodDef rod = fishing.Rods[i];
                Assert.Greater(rod.Rank, prev.Rank, rod.Id);
                Assert.Greater(rod.Price, prev.Price, rod.Id);
                Assert.Greater(rod.MaxDurability, prev.MaxDurability, rod.Id);
                Assert.Greater(rod.PerfectHalfWidth, prev.PerfectHalfWidth, rod.Id);
                Assert.Greater(rod.BonusTimePercent, prev.BonusTimePercent, rod.Id);
                Assert.GreaterOrEqual(rod.UnlockLevel, prev.UnlockLevel, rod.Id);
            }
            Assert.AreEqual(0, fishing.Rods[0].Price, "Cần tre tặng");
        }

        [Test]
        public void FishingDefinition_DefaultData_FishHaveNamesAndIcons()
        {
            FishingDefinition fishing = LoadAsset().Fishing;
            Assert.IsNotNull(fishing);

            foreach (FishDefinition fish in fishing.Fish)
            {
                Assert.IsFalse(string.IsNullOrEmpty(fish.DisplayName), fish.Id);
                Assert.IsNotNull(fish.Icon, $"{fish.Id}: chạy Về Quê → Câu cá: tạo art tạm.");
            }
            // 14/11/2026: cần, mồi có icon (Art/Fishing/rods, bait – gán bằng Tạo data câu cá).
            foreach (RodDefinition rod in fishing.Rods)
            {
                Assert.IsFalse(string.IsNullOrEmpty(rod.DisplayName), rod.Id);
                Assert.IsNotNull(rod.Icon, $"{rod.Id}: chạy Về Quê → Tạo data câu cá.");
            }
            foreach (BaitDefinition bait in fishing.Baits)
            {
                Assert.IsFalse(string.IsNullOrEmpty(bait.DisplayName), bait.Id);
                Assert.IsNotNull(bait.Icon, $"{bait.Id}: chạy Về Quê → Tạo data câu cá.");
            }
        }

        // 14/11/2026: mồi nào cũng có ít nhất 1 loài cá ưa (mua mồi mới không vô dụng).
        [Test]
        public void ToDef_DefaultData_EveryBaitAttractsSomeFish()
        {
            FishingDef fishing = LoadAsset().ToDef().Fishing;

            foreach (BaitDef bait in fishing.Baits)
            {
                bool attracts = false;
                foreach (FishDef fish in fishing.Fish)
                {
                    foreach (string preferred in fish.PreferredBaits)
                    {
                        if (preferred == bait.Id)
                            attracts = true;
                    }
                }
                Assert.IsTrue(attracts, bait.Id);
            }
        }

        // ---------- Đơn hàng có thoại (Đợt 2 Phần 11) ----------

        [Test]
        public void Dialogue_DefaultData_EnoughLinesPerNpc()
        {
            OrderDialogue dialogue = LoadAsset().ToDef().Orders.Dialogue;
            Assert.GreaterOrEqual(dialogue.Npcs.Count, 5);
            int sharedThanks = 0;
            foreach (OrderLineDef line in dialogue.Lines)
            {
                if (line.NpcId == null && line.Kind == OrderLineKind.Thanks)
                    sharedThanks++;
            }
            Assert.GreaterOrEqual(sharedThanks, 3, "Ít nhất 3 câu cảm ơn dùng chung");
            foreach (NpcDef npc in dialogue.Npcs)
            {
                int requests = 0;
                foreach (OrderLineDef line in dialogue.Lines)
                {
                    if (line.NpcId == npc.Id && line.Kind == OrderLineKind.Request && line.CropTag == null)
                        requests++;
                }
                Assert.GreaterOrEqual(requests, 4, $"{npc.Id}: cần ít nhất 4 câu đặt đơn");
            }
        }

        [Test]
        public void Dialogue_DefaultData_TagsAndPreferredCropsExist()
        {
            FarmDef def = LoadAsset().ToDef();
            OrderDialogue dialogue = def.Orders.Dialogue;
            foreach (NpcDef npc in dialogue.Npcs)
            {
                foreach (string crop in npc.PreferredCrops)
                    Assert.IsTrue(def.TryGetCrop(crop, out _), $"{npc.Id}: cây '{crop}' không có");
            }
            foreach (OrderLineDef line in dialogue.Lines)
            {
                if (line.CropTag != null)
                    Assert.IsTrue(def.TryGetCrop(line.CropTag, out _) || def.Fishing.TryGetFish(line.CropTag, out _), line.Text);
            }
        }

        // Câu dài nhất sau khi điền (số lượng 2 chữ số, tên cây / món / cá dài nhất có thể gặp) ≤ 72 ký tự.
        [Test]
        public void Dialogue_DefaultData_LinesFitTwoRows()
        {
            const int MaxLength = 72;
            FarmDefinition asset = LoadAsset();
            FarmDef def = asset.ToDef();
            OrderDialogue dialogue = def.Orders.Dialogue;
            // Đơn cá chỉ đặt loài trong album (Phần 5): cho album đủ loài để xét mọi tên cá.
            FarmState fullDex = FarmState.CreateNew(def);
            foreach (FishDef fish in def.Fishing.Fish)
                fullDex.Fishing.FishDex[fish.Id] = new FishDexEntry { Count = 1, MaxGrams = fish.MinGrams };
            var farm = new Farm(def, fullDex, new FakeClock());
            var fishNames = new System.Collections.Generic.List<string>();
            foreach (FishDef fish in farm.GetFishOrderPool())
            {
                if (asset.Fishing.TryGetFish(fish.Id, out FishDefinition name))
                    fishNames.Add(name.DisplayName.ToLowerInvariant());
            }

            foreach (OrderLineDef line in dialogue.Lines)
            {
                var titles = new System.Collections.Generic.List<string>();
                if (dialogue.TryGetNpc(line.NpcId, out NpcDef owner))
                    titles.Add(owner.Title);
                else
                    foreach (NpcDef npc in dialogue.Npcs)
                        titles.Add(npc.Title);

                foreach (string title in titles)
                {
                    if (line.Kind == OrderLineKind.Fish)
                    {
                        foreach (string fish in fishNames)
                            AssertFits(line, title, fish, null, MaxLength);
                        continue;
                    }
                    foreach (CropDefinition crop in asset.Crops)
                    {
                        if (crop == null || (line.CropTag != null && line.CropTag != crop.Id))
                            continue;
                        string name = crop.DisplayName.ToLowerInvariant();
                        var dishes = dialogue.GetDishes(crop.Id);
                        if (line.NeedsDish)
                        {
                            foreach (string dish in dishes)
                                AssertFits(line, title, name, dish, MaxLength);
                        }
                        else
                        {
                            AssertFits(line, title, name, null, MaxLength);
                        }
                    }
                }
            }
        }

        private static void AssertFits(OrderLineDef line, string title, string item, string dish, int max)
        {
            string text = OrderDialogue.Format(line.Text, title, 18, item, dish);
            Assert.LessOrEqual(text.Length, max, text);
        }
    }
}
