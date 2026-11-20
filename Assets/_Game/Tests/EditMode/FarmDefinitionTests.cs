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
            Assert.AreEqual(22, def.Crops.Count);
            Assert.IsTrue(def.TryGetCrop("rau_muong", out CropDef rauMuong));
            Assert.AreEqual(120, rauMuong.GrowSeconds);
            Assert.IsTrue(def.TryGetCrop("ca_chua", out CropDef caChua));
            Assert.AreEqual(4 * 3600, caChua.GrowSeconds);
            Assert.IsTrue(def.TryGetCrop("bi_ngo", out CropDef biNgo));
            Assert.AreEqual(6 * 3600, biNgo.GrowSeconds);
            Assert.IsFalse(def.TryGetCrop("hoa_dao", out _));
            Assert.IsFalse(def.TryGetCrop("hoa_cuc", out _));
            Assert.IsFalse(def.TryGetCrop("huong_duong", out _));
            Assert.IsFalse(def.TryGetCrop("bi_do", out _), "cây cũ đã bỏ");
        }

        // Thứ tự trong data theo thời gian lớn ngoài đời: cây sau không chín nhanh hơn cây trước.
        [Test]
        public void ToDef_DefaultData_GrowTimesFollowRealLifeOrder()
        {
            FarmDef def = LoadAsset().ToDef();

            for (int i = 1; i < def.Crops.Count; i++)
                Assert.GreaterOrEqual(def.Crops[i].GrowSeconds, def.Crops[i - 1].GrowSeconds, def.Crops[i].Id);
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
            var farm = new Farm(def, FarmState.CreateNew(def), new FakeClock());
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
