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
        public void ToDef_DefaultData_OnlyMvpCropsEnabled()
        {
            FarmDefinition asset = LoadAsset();
            FarmDef def = asset.ToDef();

            Assert.AreEqual(25, asset.Crops.Count);
            Assert.AreEqual(4, def.Crops.Count);
            Assert.IsTrue(def.TryGetCrop("rau_muong", out CropDef rauMuong));
            Assert.AreEqual(120, rauMuong.GrowSeconds);
            Assert.IsTrue(def.TryGetCrop("cai_xanh", out _));
            Assert.IsTrue(def.TryGetCrop("ca_chua", out _));
            Assert.IsTrue(def.TryGetCrop("bi_do", out CropDef biDo));
            Assert.AreEqual(4 * 3600, biDo.GrowSeconds);
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
    }
}
