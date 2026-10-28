using NUnit.Framework;
using UnityEditor;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.Tests
{
    // Kiểm tra data thật trong Assets/_Game/Data (tạo bằng menu Về Quê → Tạo data mặc định).
    public class FarmDefinitionTests
    {
        private const string FarmPath = "Assets/_Game/Data/FarmDefinition.asset";

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
                Assert.AreEqual(zone.Width * zone.Height, state.Zones[i].Plots.Length, zone.Id);
            }
            Assert.AreEqual(4, state.Zones[0].OpenCount);
            Assert.AreEqual(0, state.Zones[1].OpenCount);
            Assert.AreEqual(0, state.Zones[2].OpenCount);
        }

        [Test]
        public void ToDef_DefaultData_HasStartCoinsAndSeeds()
        {
            FarmState state = FarmState.CreateNew(LoadAsset().ToDef());

            Assert.AreEqual(60, state.Coins);
            Assert.AreEqual(5, state.GetSeeds("rau_muong"));
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
    }
}
