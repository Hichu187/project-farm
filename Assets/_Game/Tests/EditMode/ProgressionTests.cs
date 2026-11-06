using System;
using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    // Cấp độ (XP, lên cấp, thưởng, cây/khu theo cấp) và chống kẹt.
    public class ProgressionTests
    {
        private const string Zone1 = "khu1";
        private const string Zone2 = "khu2";
        private const string RauMuong = "rau_muong";
        private const string CaiXanh = "cai_xanh";
        private const string CaChua = "ca_chua";
        private const long GrowSeconds = 60;
        private const int RauMuongPrice = 5;
        private const int RauMuongXp = 2;
        private const int CaiXanhLevel = 2;
        private const int CaChuaLevel = 3;
        private const int Zone2Level = 3;
        private const int UnlockPrice = 10;
        private const int CoinsPerLevel = 10;
        private const int UnlockSeedCount = 3;
        private const long StartCoins = 100;
        private const int StartSeeds = 5;
        private static readonly int[] XpToNext = { 4, 10, 20 };
        private const int MaxLevel = 4;

        private static readonly PlotId Plot0 = new PlotId(Zone1, 0, 0, 0);

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
        }

        // ---------- XP và lên cấp ----------

        [Test]
        public void NewFarm_StartsAtLevel1WithNoXp()
        {
            Farm farm = CreateFarm();

            Assert.AreEqual(1, farm.State.Level);
            Assert.AreEqual(0, farm.State.Xp);
            Assert.AreEqual(XpToNext[0], farm.GetXpToNext());
        }

        [Test]
        public void Harvest_AddsCropXp()
        {
            Farm farm = CreateFarm();

            PlantAndHarvest(farm);

            Assert.AreEqual(1, farm.State.Level);
            Assert.AreEqual(RauMuongXp, farm.State.Xp);
        }

        [Test]
        public void Harvest_ReachesThreshold_LevelsUpAndKeepsRemainder()
        {
            Farm farm = CreateFarm();
            farm.State.Xp = XpToNext[0] - 1;

            PlantAndHarvest(farm);

            Assert.AreEqual(2, farm.State.Level);
            Assert.AreEqual(RauMuongXp - 1, farm.State.Xp);
            Assert.AreEqual(XpToNext[1], farm.GetXpToNext());
        }

        [Test]
        public void Harvest_EnoughXpForSeveralLevels_LevelsUpSeveralTimes()
        {
            Farm farm = CreateFarm();
            farm.State.Xp = XpToNext[0] + XpToNext[1] - 1;

            PlantAndHarvest(farm);

            Assert.AreEqual(3, farm.State.Level);
            Assert.AreEqual(RauMuongXp - 1, farm.State.Xp);
        }

        [Test]
        public void Harvest_ReachesMaxLevel_StopsAndClearsXp()
        {
            Farm farm = CreateFarm();
            farm.State.Level = MaxLevel - 1;
            farm.State.Xp = XpToNext[MaxLevel - 2] - 1;

            PlantAndHarvest(farm);

            Assert.AreEqual(MaxLevel, farm.State.Level);
            Assert.AreEqual(0, farm.State.Xp);
            Assert.AreEqual(0, farm.GetXpToNext());
        }

        [Test]
        public void Harvest_AtMaxLevel_XpStaysZero()
        {
            Farm farm = CreateFarm();
            farm.State.Level = MaxLevel;

            PlantAndHarvest(farm);

            Assert.AreEqual(MaxLevel, farm.State.Level);
            Assert.AreEqual(0, farm.State.Xp);
        }

        [Test]
        public void Harvest_NoLevelTable_StaysAtLevel1()
        {
            FarmDef def = CreateDef(levels: null);
            var farm = new Farm(def, FarmState.CreateNew(def), _clock);

            PlantAndHarvest(farm);

            Assert.AreEqual(1, farm.State.Level);
            Assert.AreEqual(0, farm.State.Xp);
            Assert.AreEqual(0, farm.GetXpToNext());
        }

        [Test]
        public void LevelUp_RaisesLeveledUpBeforeChanged()
        {
            Farm farm = CreateFarm();
            farm.State.Xp = XpToNext[0] + XpToNext[1] - 1;
            var events = new List<string>();
            farm.LeveledUp += level => events.Add($"level {level}");
            farm.Changed += () => events.Add("changed");

            PlantAndHarvest(farm, collectEventsFromHarvestOnly: events);

            CollectionAssert.AreEqual(new[] { "level 2", "level 3", "changed" }, events);
        }

        [Test]
        public void LevelUp_GrantsCoinsAndSeedsOfNewCrops()
        {
            Farm farm = CreateFarm();
            farm.State.Xp = XpToNext[0] + XpToNext[1] - 1;
            long coinsBefore = farm.State.Coins;

            PlantAndHarvest(farm);

            Assert.AreEqual(coinsBefore + CoinsPerLevel * 2 + CoinsPerLevel * 3, farm.State.Coins);
            Assert.AreEqual(UnlockSeedCount, farm.State.GetSeeds(CaiXanh));
            Assert.AreEqual(UnlockSeedCount, farm.State.GetSeeds(CaChua));
        }

        [Test]
        public void GetUnlocksAtLevel_ListsRewardsCropsAndZones()
        {
            Farm farm = CreateFarm();

            LevelUnlocks level2 = farm.GetUnlocksAtLevel(CaiXanhLevel);
            Assert.AreEqual(CoinsPerLevel * CaiXanhLevel, level2.Coins);
            CollectionAssert.AreEqual(new[] { CaiXanh }, level2.CropIds);
            Assert.AreEqual(UnlockSeedCount, level2.SeedsPerCrop);
            CollectionAssert.IsEmpty(level2.ZoneIndices);
            Assert.IsFalse(level2.OrdersUnlocked);

            LevelUnlocks level3 = farm.GetUnlocksAtLevel(Zone2Level);
            CollectionAssert.AreEqual(new[] { CaChua }, level3.CropIds);
            CollectionAssert.AreEqual(new[] { 1 }, level3.ZoneIndices);
        }

        [Test]
        public void GetUnlocksAtLevel_Level1_HasNoRewards()
        {
            LevelUnlocks level1 = CreateFarm().GetUnlocksAtLevel(1);

            Assert.AreEqual(0, level1.Coins);
            CollectionAssert.IsEmpty(level1.CropIds);
            CollectionAssert.IsEmpty(level1.ZoneIndices);
        }

        [Test]
        public void GetUnlocksAtLevel_OrdersLevel_FlagsOrders()
        {
            FarmDef baseDef = CreateDef();
            var def = new FarmDef(baseDef.Zones, baseDef.Crops, StartCoins, null, 0, baseDef.Levels, RauMuong,
                new OrderDef(3, CaiXanhLevel, 160, 200, 300, new[] { "Bà Tư" }));
            var farm = new Farm(def, FarmState.CreateNew(def), _clock);

            Assert.IsTrue(farm.GetUnlocksAtLevel(CaiXanhLevel).OrdersUnlocked);
            Assert.IsFalse(farm.GetUnlocksAtLevel(Zone2Level).OrdersUnlocked);
        }

        // ---------- Cây theo cấp ----------

        [Test]
        public void IsCropUnlocked_DependsOnLevel()
        {
            Farm farm = CreateFarm();

            Assert.IsTrue(farm.IsCropUnlocked(RauMuong));
            Assert.IsFalse(farm.IsCropUnlocked(CaiXanh));
            Assert.IsFalse(farm.IsCropUnlocked("khong_co"));

            farm.State.Level = CaiXanhLevel;
            Assert.IsTrue(farm.IsCropUnlocked(CaiXanh));
        }

        [Test]
        public void Plant_CropAboveLevel_ReturnsCropLocked()
        {
            Farm farm = CreateFarm();
            farm.State.Seeds[CaiXanh] = 1;

            Assert.AreEqual(FarmResult.CropLocked, farm.Plant(Plot0, CaiXanh));
            Assert.AreEqual(1, farm.State.GetSeeds(CaiXanh));
            Assert.IsTrue(farm.TryGetPlotState(Plot0, out PlotState plot));
            Assert.AreEqual(PlotStatus.Empty, plot.Status);
        }

        [Test]
        public void BuySeeds_CropAboveLevel_ReturnsCropLocked()
        {
            Farm farm = CreateFarm();

            Assert.AreEqual(FarmResult.CropLocked, farm.BuySeeds(CaiXanh, 1));
            Assert.AreEqual(StartCoins, farm.State.Coins);
            Assert.AreEqual(0, farm.State.GetSeeds(CaiXanh));
        }

        [Test]
        public void BuySeeds_CropReachedLevel_Succeeds()
        {
            Farm farm = CreateFarm();
            farm.State.Level = CaiXanhLevel;

            Assert.AreEqual(FarmResult.Ok, farm.BuySeeds(CaiXanh, 1));
            Assert.AreEqual(1, farm.State.GetSeeds(CaiXanh));
        }

        // ---------- Khu theo cấp ----------

        [Test]
        public void Zone2_Zone1FullButLevelTooLow_StaysLocked()
        {
            Farm farm = CreateFarm();

            FillZone1(farm);

            Assert.AreEqual(0, farm.State.Zones[1].OpenCount);
            Assert.IsFalse(farm.IsZoneAvailable(Zone2));
            Assert.IsFalse(farm.TryGetNextUnlock(Zone2, out _, out _));
            Assert.AreEqual(FarmResult.LevelTooLow, farm.UnlockNextPlot(Zone2));
        }

        [Test]
        public void Zone2_LevelReachedAfterZone1Full_GrantsFreePlot()
        {
            Farm farm = CreateFarm();
            FillZone1(farm);
            farm.State.Xp = XpToNext[0] + XpToNext[1] - 1;

            PlantAndHarvest(farm);

            Assert.AreEqual(Zone2Level, farm.State.Level);
            Assert.AreEqual(1, farm.State.Zones[1].OpenCount);
            Assert.IsTrue(farm.IsZoneAvailable(Zone2));
            Assert.AreEqual(FarmResult.Ok, farm.UnlockNextPlot(Zone2));
        }

        [Test]
        public void Zone2_LevelReachedBeforeZone1Full_GrantsFreePlotWhenZone1Full()
        {
            Farm farm = CreateFarm();
            farm.State.Level = Zone2Level;

            Assert.AreEqual(FarmResult.ZoneLocked, farm.UnlockNextPlot(Zone2));
            FillZone1(farm);

            Assert.AreEqual(1, farm.State.Zones[1].OpenCount);
        }

        // ---------- Chống kẹt ----------

        [Test]
        public void Rescue_StuckAfterCommand_GivesOneRescueSeed()
        {
            FarmState state = StuckCandidate(coins: UnlockPrice);
            Farm farm = CreateFarm(state);
            string rescued = null;
            farm.Rescued += id => rescued = id;

            Assert.AreEqual(FarmResult.Ok, farm.UnlockNextPlot(Zone1));

            Assert.AreEqual(RauMuong, rescued);
            Assert.AreEqual(1, farm.State.GetSeeds(RauMuong));
        }

        [Test]
        public void Rescue_StuckOnLoad_GivesOneRescueSeed()
        {
            Farm farm = CreateFarm(StuckCandidate(coins: 0));

            Assert.AreEqual(1, farm.State.GetSeeds(RauMuong));
        }

        [Test]
        public void Rescue_OnlyLockedCropSeeds_StillRescues()
        {
            FarmState state = StuckCandidate(coins: 0);
            state.Seeds[CaiXanh] = 5;

            Farm farm = CreateFarm(state);

            Assert.AreEqual(1, farm.State.GetSeeds(RauMuong));
        }

        [Test]
        public void Rescue_CropGrowing_DoesNotRescue()
        {
            FarmState state = StuckCandidate(coins: 0);
            state.Seeds[RauMuong] = 1;
            Farm farm = CreateFarm(state);

            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot0, RauMuong));

            Assert.AreEqual(0, farm.State.GetSeeds(RauMuong));
        }

        [Test]
        public void Rescue_HasItemsToSell_DoesNotRescue()
        {
            FarmState state = StuckCandidate(coins: 0);
            state.Inventory[CaiXanh] = 1;

            Farm farm = CreateFarm(state);

            Assert.AreEqual(0, farm.State.GetSeeds(RauMuong));
        }

        [Test]
        public void Rescue_CanAffordSeed_DoesNotRescue()
        {
            Farm farm = CreateFarm(StuckCandidate(coins: RauMuongPrice));

            Assert.AreEqual(0, farm.State.GetSeeds(RauMuong));
        }

        [Test]
        public void Rescue_NoRescueCrop_DoesNotRescue()
        {
            FarmDef def = CreateDef(rescueCropId: null);
            FarmState state = FarmState.CreateNew(def);
            state.Coins = 0;
            state.Seeds.Clear();

            var farm = new Farm(def, state, _clock);

            Assert.AreEqual(0, farm.State.GetSeeds(RauMuong));
        }

        // ---------- Kiểm tra data ----------

        [Test]
        public void FarmDef_UnknownRescueCrop_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateDef(rescueCropId: "khong_co"));
        }

        [Test]
        public void FarmDef_RescueCropAboveLevel1_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateDef(rescueCropId: CaiXanh));
        }

        [Test]
        public void LevelDef_NonPositiveXp_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LevelDef(new[] { 4, 0 }, 0, 0));
        }

        [Test]
        public void CropDef_UnlockLevelBelow1_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CropDef(RauMuong, GrowSeconds, 1, 1, 1, unlockLevel: 0));
        }

        [Test]
        public void ZoneDef_RequiredLevelBelow1_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ZoneDef(Zone1, new[] { new BedDef(2, 2) }, 2, new[] { 10, 10 }, 0));
        }

        // ---------- Save ----------

        [Test]
        public void Save_RoundTrip_KeepsLevelAndXp()
        {
            FarmDef def = CreateDef();
            FarmState state = FarmState.CreateNew(def);
            state.Level = 3;
            state.Xp = 7;

            SaveData data = SaveMapper.ToSave(state, def, _clock.UtcNowSeconds);

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, def, out FarmState loaded));
            Assert.AreEqual(3, loaded.Level);
            Assert.AreEqual(7, loaded.Xp);
        }

        [Test]
        public void Save_OldSaveWithoutLevel_LoadsAsLevel1()
        {
            FarmDef def = CreateDef();
            SaveData data = SaveMapper.ToSave(FarmState.CreateNew(def), def, _clock.UtcNowSeconds);
            data.level = 0;
            data.xp = 0;

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, def, out FarmState loaded));
            Assert.AreEqual(1, loaded.Level);
            Assert.AreEqual(0, loaded.Xp);
        }

        [Test]
        public void Save_LevelAndXpOutOfTable_AreClamped()
        {
            FarmDef def = CreateDef();
            SaveData data = SaveMapper.ToSave(FarmState.CreateNew(def), def, _clock.UtcNowSeconds);

            data.level = 99;
            data.xp = 50;
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, def, out FarmState maxed));
            Assert.AreEqual(MaxLevel, maxed.Level);
            Assert.AreEqual(0, maxed.Xp);

            data.level = 1;
            data.xp = 50;
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, def, out FarmState capped));
            Assert.AreEqual(1, capped.Level);
            Assert.AreEqual(XpToNext[0] - 1, capped.Xp);
        }

        [Test]
        public void Save_NegativeXp_IsInvalid()
        {
            FarmDef def = CreateDef();
            SaveData data = SaveMapper.ToSave(FarmState.CreateNew(def), def, _clock.UtcNowSeconds);
            data.xp = -1;

            Assert.AreEqual(SaveLoadResult.Invalid, SaveMapper.TryFromSave(data, def, out _));
        }

        // ---------- Hỗ trợ ----------

        // Gieo rau muống vào ô đầu, chờ chín, thu hoạch. Nếu truyền danh sách thì chỉ giữ sự kiện của lần thu hoạch.
        private void PlantAndHarvest(Farm farm, List<string> collectEventsFromHarvestOnly = null)
        {
            if (farm.State.GetSeeds(RauMuong) == 0)
                farm.State.Seeds[RauMuong] = 1;
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot0, RauMuong));
            _clock.Advance(GrowSeconds);
            collectEventsFromHarvestOnly?.Clear();
            Assert.AreEqual(FarmResult.Ok, farm.Harvest(Plot0));
        }

        private static void FillZone1(Farm farm)
        {
            while (!farm.State.Zones[0].IsFull)
                Assert.AreEqual(FarmResult.Ok, farm.UnlockNextPlot(Zone1));
        }

        // State sẽ kẹt nếu không còn xu: không hạt, không nông sản, không cây.
        private static FarmState StuckCandidate(long coins)
        {
            FarmState state = FarmState.CreateNew(CreateDef());
            state.Coins = coins;
            state.Seeds.Clear();
            state.Inventory.Clear();
            return state;
        }

        private Farm CreateFarm(FarmState state = null)
        {
            FarmDef def = CreateDef();
            return new Farm(def, state ?? FarmState.CreateNew(def), _clock);
        }

        private static FarmDef CreateDef(string rescueCropId = RauMuong)
        {
            return CreateDef(new LevelDef(XpToNext, CoinsPerLevel, UnlockSeedCount), rescueCropId);
        }

        private static FarmDef CreateDef(LevelDef levels, string rescueCropId = RauMuong)
        {
            var zones = new[]
            {
                new ZoneDef(Zone1, new[] { new BedDef(2, 2) }, 2, new[] { UnlockPrice, UnlockPrice }),
                new ZoneDef(Zone2, new[] { new BedDef(2, 2) }, 1, new[] { UnlockPrice, UnlockPrice, UnlockPrice }, Zone2Level),
            };
            var crops = new[]
            {
                new CropDef(RauMuong, GrowSeconds, RauMuongPrice, 3, 2, xp: RauMuongXp),
                new CropDef(CaiXanh, GrowSeconds, 10, 10, 1, xp: 5, unlockLevel: CaiXanhLevel),
                new CropDef(CaChua, GrowSeconds, 20, 10, 3, xp: 1, unlockLevel: CaChuaLevel),
            };
            var seeds = new Dictionary<string, int> { { RauMuong, StartSeeds } };
            return new FarmDef(zones, crops, StartCoins, seeds, 0, levels, rescueCropId);
        }
    }
}
