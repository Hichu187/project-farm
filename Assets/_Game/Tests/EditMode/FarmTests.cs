using System;
using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    public class FarmTests
    {
        private const string Zone1 = "khu1";
        private const string Zone2 = "khu2";
        private const string Zone3 = "khu3";
        private const string RauMuong = "rau_muong";
        private const string CaiXanh = "cai_xanh";
        private const long RauMuongSeconds = 120;
        private const int RauMuongYield = 2;
        private const int StartSeeds = 3;
        private const long StartCoins = 100;
        private const int Zone1StartOpen = 2;
        private const int LaterZoneStartOpen = 1;

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
        }

        // ---------- Khởi tạo ----------

        [TestCase(3, 4)]
        [TestCase(4, 5)]
        [TestCase(5, 6)]
        public void CreateNew_AnySize_PlotCountMatchesZoneSize(int width, int height)
        {
            Farm farm = CreateFarm(width, height);

            foreach (ZoneState zone in farm.State.Zones)
                Assert.AreEqual(width * height, zone.Plots.Length);
        }

        [TestCase(3, 4)]
        [TestCase(4, 5)]
        [TestCase(5, 6)]
        public void CreateNew_AnySize_OnlyZone1StartPlotsOpen(int width, int height)
        {
            Farm farm = CreateFarm(width, height);

            Assert.AreEqual(Zone1StartOpen, farm.State.Zones[0].OpenCount);
            Assert.AreEqual(0, farm.State.Zones[1].OpenCount);
            Assert.AreEqual(0, farm.State.Zones[2].OpenCount);
            Assert.AreEqual(PlotStatus.Empty, farm.State.Zones[0].Plots[0].Status);
            Assert.AreEqual(PlotStatus.Locked, farm.State.Zones[0].Plots[Zone1StartOpen].Status);
            Assert.IsTrue(farm.IsZoneAvailable(Zone1));
            Assert.IsFalse(farm.IsZoneAvailable(Zone2));
        }

        [Test]
        public void CreateNew_Default_HasStartCoinsAndSeeds()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(StartCoins, farm.State.Coins);
            Assert.AreEqual(StartSeeds, farm.State.GetSeeds(RauMuong));
            Assert.AreEqual(0, farm.State.GetSeeds(CaiXanh));
        }

        [Test]
        public void Constructor_StateDoesNotMatchDef_Throws()
        {
            FarmDef def = CreateDef(4, 5);
            FarmState otherState = FarmState.CreateNew(CreateDef(3, 4));

            Assert.Throws<ArgumentException>(() => new Farm(def, otherState, _clock));
        }

        [Test]
        public void ZoneDef_WrongPriceCount_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ZoneDef(Zone1, 4, 5, 2, new int[17]));
        }

        // ---------- Gieo ----------

        [Test]
        public void Plant_EmptyPlotWithSeed_StartsGrowingAndUsesSeed()
        {
            Farm farm = CreateFarm(4, 5);
            PlotId plot = new PlotId(Zone1, 0, 0);

            Assert.AreEqual(FarmResult.Ok, farm.Plant(plot, RauMuong));

            farm.TryGetPlotState(plot, out PlotState state);
            Assert.AreEqual(PlotStatus.Growing, state.Status);
            Assert.AreEqual(RauMuong, state.CropId);
            Assert.AreEqual(_clock.UtcNowSeconds, state.PlantedAt);
            Assert.AreEqual(StartSeeds - 1, farm.State.GetSeeds(RauMuong));
        }

        [Test]
        public void Plant_NoSeed_ReturnsNotEnoughSeeds()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.NotEnoughSeeds, farm.Plant(new PlotId(Zone1, 0, 0), CaiXanh));
        }

        [Test]
        public void Plant_LockedPlot_ReturnsPlotLocked()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.PlotLocked, farm.Plant(new PlotId(Zone1, 3, 4), RauMuong));
        }

        [Test]
        public void Plant_GrowingPlot_ReturnsPlotNotEmpty()
        {
            Farm farm = CreateFarm(4, 5);
            PlotId plot = new PlotId(Zone1, 0, 0);
            farm.Plant(plot, RauMuong);

            Assert.AreEqual(FarmResult.PlotNotEmpty, farm.Plant(plot, RauMuong));
            Assert.AreEqual(StartSeeds - 1, farm.State.GetSeeds(RauMuong));
        }

        [Test]
        public void Plant_UnknownCrop_ReturnsUnknownCrop()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.UnknownCrop, farm.Plant(new PlotId(Zone1, 0, 0), "khong_co"));
        }

        [TestCase(-1, 0)]
        [TestCase(4, 0)]
        [TestCase(0, 5)]
        public void Plant_OutOfRange_ReturnsUnknownPlot(int col, int row)
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.UnknownPlot, farm.Plant(new PlotId(Zone1, col, row), RauMuong));
        }

        [Test]
        public void Plant_UnknownZone_ReturnsUnknownPlot()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.UnknownPlot, farm.Plant(new PlotId("khong_co", 0, 0), RauMuong));
        }

        // ---------- Lớn ----------

        [Test]
        public void GetStage_OverTime_AdvancesThroughThreeStagesThenRipe()
        {
            Farm farm = CreateFarm(4, 5);
            PlotId plot = new PlotId(Zone1, 0, 0);
            farm.Plant(plot, RauMuong);
            long third = RauMuongSeconds / CropDef.StageCount;

            Assert.AreEqual(0, farm.GetStage(plot));
            _clock.Advance(third);
            Assert.AreEqual(1, farm.GetStage(plot));
            _clock.Advance(third);
            Assert.AreEqual(2, farm.GetStage(plot));
            Assert.IsFalse(farm.IsRipe(plot));
            _clock.Advance(third);
            Assert.AreEqual(2, farm.GetStage(plot));
            Assert.IsTrue(farm.IsRipe(plot));
        }

        [Test]
        public void GetStage_EmptyPlot_ReturnsMinusOne()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(-1, farm.GetStage(new PlotId(Zone1, 0, 0)));
        }

        [Test]
        public void GetStage_ClockMovedBack_StaysAtFirstStage()
        {
            Farm farm = CreateFarm(4, 5);
            PlotId plot = new PlotId(Zone1, 0, 0);
            farm.Plant(plot, RauMuong);

            _clock.Advance(-10_000);

            Assert.AreEqual(0, farm.GetStage(plot));
            Assert.IsFalse(farm.IsRipe(plot));
        }

        // ---------- Thu hoạch ----------

        [Test]
        public void Harvest_Ripe_AddsYieldAndEmptiesPlot()
        {
            Farm farm = CreateFarm(4, 5);
            PlotId plot = new PlotId(Zone1, 0, 0);
            farm.Plant(plot, RauMuong);
            _clock.Advance(RauMuongSeconds);

            Assert.AreEqual(FarmResult.Ok, farm.Harvest(plot));

            farm.TryGetPlotState(plot, out PlotState state);
            Assert.AreEqual(PlotStatus.Empty, state.Status);
            Assert.IsNull(state.CropId);
            Assert.AreEqual(RauMuongYield, farm.State.GetItems(RauMuong));
        }

        [Test]
        public void Harvest_NotRipe_ReturnsNotRipe()
        {
            Farm farm = CreateFarm(4, 5);
            PlotId plot = new PlotId(Zone1, 0, 0);
            farm.Plant(plot, RauMuong);
            _clock.Advance(RauMuongSeconds - 1);

            Assert.AreEqual(FarmResult.NotRipe, farm.Harvest(plot));
            Assert.AreEqual(0, farm.State.GetItems(RauMuong));
        }

        [Test]
        public void Harvest_EmptyPlot_ReturnsNothingToHarvest()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.NothingToHarvest, farm.Harvest(new PlotId(Zone1, 0, 0)));
        }

        [Test]
        public void Harvest_LockedPlot_ReturnsPlotLocked()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.PlotLocked, farm.Harvest(new PlotId(Zone1, 3, 4)));
        }

        // ---------- Mua hạt / bán ----------

        [Test]
        public void BuySeeds_EnoughCoins_AddsSeedsAndSpendsCoins()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.Ok, farm.BuySeeds(CaiXanh, 4));

            Assert.AreEqual(4, farm.State.GetSeeds(CaiXanh));
            Assert.AreEqual(StartCoins - 4 * 10, farm.State.Coins);
        }

        [Test]
        public void BuySeeds_NotEnoughCoins_ReturnsNotEnoughCoinsAndChangesNothing()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.NotEnoughCoins, farm.BuySeeds(CaiXanh, 11));

            Assert.AreEqual(0, farm.State.GetSeeds(CaiXanh));
            Assert.AreEqual(StartCoins, farm.State.Coins);
        }

        [TestCase(0)]
        [TestCase(-3)]
        public void BuySeeds_NonPositiveAmount_ReturnsInvalidAmount(int amount)
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.InvalidAmount, farm.BuySeeds(RauMuong, amount));
        }

        [Test]
        public void BuySeeds_UnknownCrop_ReturnsUnknownCrop()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.UnknownCrop, farm.BuySeeds("khong_co", 1));
        }

        [Test]
        public void Sell_EnoughItems_AddsCoinsAndRemovesItems()
        {
            Farm farm = CreateFarmWithHarvest();

            Assert.AreEqual(FarmResult.Ok, farm.Sell(RauMuong, RauMuongYield));

            Assert.AreEqual(0, farm.State.GetItems(RauMuong));
            Assert.AreEqual(StartCoins + RauMuongYield * 12, farm.State.Coins);
        }

        [Test]
        public void Sell_NotEnoughItems_ReturnsNotEnoughItemsAndChangesNothing()
        {
            Farm farm = CreateFarmWithHarvest();

            Assert.AreEqual(FarmResult.NotEnoughItems, farm.Sell(RauMuong, RauMuongYield + 1));

            Assert.AreEqual(RauMuongYield, farm.State.GetItems(RauMuong));
            Assert.AreEqual(StartCoins, farm.State.Coins);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Sell_NonPositiveAmount_ReturnsInvalidAmount(int amount)
        {
            Farm farm = CreateFarmWithHarvest();

            Assert.AreEqual(FarmResult.InvalidAmount, farm.Sell(RauMuong, amount));
        }

        // ---------- Mở ô ----------

        [TestCase(3, 4)]
        [TestCase(4, 5)]
        [TestCase(5, 6)]
        public void UnlockNextPlot_WholeZone_OpensRowByRowFromTopLeft(int width, int height)
        {
            Farm farm = CreateRichFarm(width, height);

            for (int index = Zone1StartOpen; index < width * height; index++)
            {
                Assert.IsTrue(farm.TryGetNextUnlock(Zone1, out PlotId next, out int price));
                Assert.AreEqual(new PlotId(Zone1, index % width, index / width), next);
                Assert.AreEqual(PriceAt(index - Zone1StartOpen), price);

                long coinsBefore = farm.State.Coins;
                Assert.AreEqual(FarmResult.Ok, farm.UnlockNextPlot(Zone1));
                Assert.AreEqual(coinsBefore - price, farm.State.Coins);

                farm.TryGetPlotState(next, out PlotState state);
                Assert.AreEqual(PlotStatus.Empty, state.Status);
            }

            Assert.IsFalse(farm.TryGetNextUnlock(Zone1, out _, out _));
            Assert.AreEqual(FarmResult.ZoneFull, farm.UnlockNextPlot(Zone1));
        }

        [Test]
        public void UnlockNextPlot_NotEnoughCoins_ReturnsNotEnoughCoinsAndChangesNothing()
        {
            Farm farm = CreateFarm(4, 5);
            farm.State.Coins = PriceAt(0) - 1;

            Assert.AreEqual(FarmResult.NotEnoughCoins, farm.UnlockNextPlot(Zone1));

            Assert.AreEqual(Zone1StartOpen, farm.State.Zones[0].OpenCount);
            Assert.AreEqual(PriceAt(0) - 1, farm.State.Coins);
        }

        [Test]
        public void UnlockNextPlot_PreviousZoneNotFull_ReturnsZoneLocked()
        {
            Farm farm = CreateRichFarm(4, 5);

            Assert.AreEqual(FarmResult.ZoneLocked, farm.UnlockNextPlot(Zone2));
            Assert.IsFalse(farm.TryGetNextUnlock(Zone2, out _, out _));
        }

        [Test]
        public void UnlockNextPlot_UnknownZone_ReturnsUnknownZone()
        {
            Farm farm = CreateFarm(4, 5);

            Assert.AreEqual(FarmResult.UnknownZone, farm.UnlockNextPlot("khong_co"));
        }

        [TestCase(3, 4)]
        [TestCase(4, 5)]
        [TestCase(5, 6)]
        public void UnlockNextPlot_LastPlotOfZone_UnlocksNextZoneWithFreePlot(int width, int height)
        {
            Farm farm = CreateRichFarm(width, height);

            OpenWholeZone(farm, Zone1);

            Assert.IsTrue(farm.IsZoneAvailable(Zone2));
            Assert.AreEqual(LaterZoneStartOpen, farm.State.Zones[1].OpenCount);
            Assert.AreEqual(PlotStatus.Empty, farm.State.Zones[1].Plots[0].Status);
            Assert.IsFalse(farm.IsZoneAvailable(Zone3));
            Assert.AreEqual(0, farm.State.Zones[2].OpenCount);

            Assert.IsTrue(farm.TryGetNextUnlock(Zone2, out PlotId next, out int price));
            Assert.AreEqual(new PlotId(Zone2, 1 % width, 1 / width), next);
            Assert.AreEqual(PriceAt(0), price);
        }

        [Test]
        public void UnlockNextPlot_AllZonesFull_LastZoneReturnsZoneFull()
        {
            Farm farm = CreateRichFarm(3, 4);

            OpenWholeZone(farm, Zone1);
            OpenWholeZone(farm, Zone2);
            OpenWholeZone(farm, Zone3);

            Assert.AreEqual(FarmResult.ZoneFull, farm.UnlockNextPlot(Zone3));
        }

        // ---------- Helpers ----------

        private static int PriceAt(int index) => 10 * (index + 1);

        private static ZoneDef CreateZone(string id, int width, int height, int startOpen)
        {
            int count = width * height - startOpen;
            var prices = new int[count];
            for (int i = 0; i < count; i++)
                prices[i] = PriceAt(i);
            return new ZoneDef(id, width, height, startOpen, prices);
        }

        private static FarmDef CreateDef(int width, int height)
        {
            var zones = new[]
            {
                CreateZone(Zone1, width, height, Zone1StartOpen),
                CreateZone(Zone2, width, height, LaterZoneStartOpen),
                CreateZone(Zone3, width, height, LaterZoneStartOpen),
            };
            var crops = new[]
            {
                new CropDef(RauMuong, RauMuongSeconds, 5, 12, RauMuongYield),
                new CropDef(CaiXanh, 900, 10, 30, 1),
            };
            return new FarmDef(zones, crops, StartCoins, new Dictionary<string, int> { { RauMuong, StartSeeds } });
        }

        private Farm CreateFarm(int width, int height)
        {
            FarmDef def = CreateDef(width, height);
            return new Farm(def, FarmState.CreateNew(def), _clock);
        }

        private Farm CreateRichFarm(int width, int height)
        {
            Farm farm = CreateFarm(width, height);
            farm.State.Coins = 1_000_000;
            return farm;
        }

        private Farm CreateFarmWithHarvest()
        {
            Farm farm = CreateFarm(4, 5);
            PlotId plot = new PlotId(Zone1, 0, 0);
            farm.Plant(plot, RauMuong);
            _clock.Advance(RauMuongSeconds);
            farm.Harvest(plot);
            return farm;
        }

        private static void OpenWholeZone(Farm farm, string zoneId)
        {
            while (farm.TryGetNextUnlock(zoneId, out _, out _))
                Assert.AreEqual(FarmResult.Ok, farm.UnlockNextPlot(zoneId));
        }
    }
}
