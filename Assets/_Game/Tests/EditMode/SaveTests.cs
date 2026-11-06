using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Save;

namespace VeQue.Tests
{
    public class SaveTests
    {
        private const string Zone1 = "khu1";
        private const string Zone2 = "khu2";
        private const string RauMuong = "rau_muong";
        private const long GrowSeconds = 120;
        private const int SeedPrice = 5;
        private const long StartCoins = 100;
        private const int StartSeeds = 3;
        private const int Yield = 2;
        private const int WaterReducePercent = 20;
        private const int Zone1StartOpen = 2;
        private const string NewerSave = "{\"version\":99,\"coins\":1,\"zones\":[]}";

        private static readonly PlotId Plot0 = new PlotId(Zone1, 0, 0, 0);
        private static readonly PlotId Plot1 = new PlotId(Zone1, 0, 1, 0);

        private sealed class MemoryStore : ISaveStore
        {
            public readonly Dictionary<string, string> Values = new Dictionary<string, string>();
            public int FlushCount;

            public string Read(string key) => Values.TryGetValue(key, out string value) ? value : null;
            public void Write(string key, string value) => Values[key] = value;
            public void Flush() => FlushCount++;
        }

        private sealed class Session
        {
            public SaveService Save;
            public Farm Farm;
        }

        private FakeClock _clock;
        private MemoryStore _store;
        private FarmDef _def;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _store = new MemoryStore();
            _def = CreateDef();
        }

        // ---------- Lưu rồi đọc lại ----------

        [Test]
        public void Load_NoSave_CreatesNewGame()
        {
            Session session = Open();

            Assert.AreEqual(StartCoins, session.Farm.State.Coins);
            Assert.AreEqual(StartSeeds, session.Farm.State.GetSeeds(RauMuong));
            Assert.AreEqual(Zone1StartOpen, session.Farm.State.Zones[0].OpenCount);
            Assert.IsFalse(session.Save.SavingBlocked);
        }

        [Test]
        public void SaveThenLoad_RestoresCoinsSeedsAndInventory()
        {
            Session first = Open();
            Assert.AreEqual(FarmResult.Ok, first.Farm.BuySeeds(RauMuong, 2));
            Plant(first, Plot0);
            _clock.Advance(GrowSeconds);
            Assert.AreEqual(FarmResult.Ok, first.Farm.Harvest(Plot0));
            first.Save.Save();

            Session second = Open();

            Assert.AreEqual(StartCoins - 2 * SeedPrice, second.Farm.State.Coins);
            Assert.AreEqual(StartSeeds + 2 - 1, second.Farm.State.GetSeeds(RauMuong));
            Assert.AreEqual(Yield, second.Farm.State.GetItems(RauMuong));
        }

        [Test]
        public void SaveThenLoad_RestoresOpenPlots()
        {
            Session first = Open();
            Assert.AreEqual(FarmResult.Ok, first.Farm.UnlockNextPlot(Zone1));
            first.Save.Save();

            Session second = Open();

            Assert.AreEqual(Zone1StartOpen + 1, second.Farm.State.Zones[0].OpenCount);
            Assert.AreEqual(PlotStatus.Empty, second.Farm.State.Zones[0].Plots[Zone1StartOpen].Status);
            Assert.AreEqual(PlotStatus.Locked, second.Farm.State.Zones[0].Plots[Zone1StartOpen + 1].Status);
        }

        [Test]
        public void SaveThenLoad_RestoresGrowingPlot()
        {
            Session first = Open();
            Plant(first, Plot1);
            long plantedAt = _clock.UtcNowSeconds;
            first.Save.Save();

            Session second = Open();

            Assert.IsTrue(second.Farm.TryGetPlotState(Plot1, out PlotState plot));
            Assert.AreEqual(PlotStatus.Growing, plot.Status);
            Assert.AreEqual(RauMuong, plot.CropId);
            Assert.AreEqual(plantedAt, plot.PlantedAt);
            Assert.IsFalse(plot.Watered);
        }

        [Test]
        public void SaveThenLoad_WateredPlotStaysWateredAndCannotWaterAgain()
        {
            Session first = Open();
            Plant(first, Plot0);
            Assert.AreEqual(FarmResult.Ok, first.Farm.Water(Plot0));
            first.Save.Save();

            Session second = Open();

            Assert.AreEqual(FarmResult.AlreadyWatered, second.Farm.Water(Plot0));
            Assert.AreEqual(GrowSeconds * (100 - WaterReducePercent) / 100, second.Farm.GetRemainingSeconds(Plot0));
        }

        [Test]
        public void Load_AfterOfflineTime_CropKeepsGrowing()
        {
            Session first = Open();
            Plant(first, Plot0);
            first.Save.Save();

            _clock.Advance(GrowSeconds);
            Session second = Open();

            Assert.IsTrue(second.Farm.IsRipe(Plot0));
        }

        [Test]
        public void Save_Json_HasVersionAndStablePlotIds()
        {
            Session session = Open();
            Plant(session, Plot1);
            session.Save.Save();

            SaveData data = JsonUtility.FromJson<SaveData>(_store.Read(SaveService.MainKey));

            Assert.AreEqual(SaveData.CurrentVersion, data.version);
            Assert.AreEqual(Zone1, data.zones[0].zoneId);
            Assert.AreEqual(1, data.zones[0].plots.Length);
            Assert.AreEqual(0, data.zones[0].plots[0].bed);
            Assert.AreEqual(1, data.zones[0].plots[0].col);
            Assert.AreEqual(0, data.zones[0].plots[0].row);
            Assert.AreEqual(1, _store.FlushCount);
        }

        // ---------- Đồng hồ ----------

        [Test]
        public void MonotonicClock_InnerGoesBack_StaysAtLatestUntilCaughtUp()
        {
            var clock = new MonotonicClock(_clock, 0);
            long latest = clock.UtcNowSeconds;

            _clock.Advance(-100);
            Assert.AreEqual(latest, clock.UtcNowSeconds);

            _clock.Advance(150);
            Assert.AreEqual(latest + 50, clock.UtcNowSeconds);
        }

        [Test]
        public void Load_ClockRolledBack_TimeKeepsLastSeenValue()
        {
            Session first = Open();
            Plant(first, Plot0);
            _clock.Advance(GrowSeconds);
            first.Save.Save();
            long lastSeen = _clock.UtcNowSeconds;

            _clock.Advance(-10 * GrowSeconds);
            Session second = Open();

            Assert.AreEqual(lastSeen, second.Save.Clock.UtcNowSeconds);
            Assert.IsTrue(second.Farm.IsRipe(Plot0));
        }

        [Test]
        public void Load_ClockRolledBack_NewCropDoesNotGrowUntilCaughtUp()
        {
            Session first = Open();
            first.Save.Save();

            _clock.Advance(-10 * GrowSeconds);
            Session second = Open();
            Plant(second, Plot0);
            _clock.Advance(GrowSeconds);

            Assert.AreEqual(GrowSeconds, second.Farm.GetRemainingSeconds(Plot0));
        }

        // ---------- Save hỏng hoặc khác phiên bản ----------

        [TestCase("{{{khong phai json")]
        [TestCase("{}")]
        [TestCase("{\"version\":1,\"coins\":-5,\"zones\":[]}")]
        [TestCase("{\"version\":1,\"coins\":5}")]
        public void Load_MainInvalid_UsesPreviousSave(string corrupt)
        {
            Session first = Open();
            Assert.AreEqual(FarmResult.Ok, first.Farm.BuySeeds(RauMuong, 1));
            first.Save.Save();
            long coinsInPrevious = first.Farm.State.Coins;
            Assert.AreEqual(FarmResult.Ok, first.Farm.BuySeeds(RauMuong, 1));
            first.Save.Save();

            _store.Write(SaveService.MainKey, corrupt);
            Session second = Open();

            Assert.AreEqual(coinsInPrevious, second.Farm.State.Coins);
        }

        [Test]
        public void Load_MainAndPreviousInvalid_CreatesNewGame()
        {
            _store.Write(SaveService.MainKey, "hong");
            _store.Write(SaveService.PreviousKey, "hong");

            Session session = Open();

            Assert.AreEqual(StartCoins, session.Farm.State.Coins);
        }

        [Test]
        public void Save_AfterLoadingPrevious_DoesNotKeepCorruptMainAsPrevious()
        {
            Session first = Open();
            first.Save.Save();
            string good = _store.Read(SaveService.MainKey);
            first.Save.Save();

            _store.Write(SaveService.MainKey, "hong");
            _store.Write(SaveService.PreviousKey, good);
            Session second = Open();
            second.Save.Save();

            Assert.AreEqual(good, _store.Read(SaveService.PreviousKey));
        }

        [Test]
        public void Load_NewerVersion_BlocksSavingAndKeepsSave()
        {
            _store.Write(SaveService.MainKey, NewerSave);

            Session session = Open();
            session.Save.Save();

            Assert.IsTrue(session.Save.SavingBlocked);
            Assert.AreEqual(NewerSave, _store.Read(SaveService.MainKey));
        }

        // ---------- Data đổi so với lúc lưu ----------

        [Test]
        public void Load_PlotNoLongerExists_IsIgnored()
        {
            WriteSave(Plot(5, 0, 0, RauMuong));

            Session session = Open();

            AssertAllEmptyOrLocked(session);
        }

        [Test]
        public void Load_PlotStillLocked_IsIgnored()
        {
            WriteSave(Plot(0, 0, 1, RauMuong));

            Session session = Open();

            AssertAllEmptyOrLocked(session);
        }

        [Test]
        public void Load_CropNoLongerExists_PlotBecomesEmpty()
        {
            WriteSave(Plot(0, 0, 0, "cay_da_bo"));

            Session session = Open();

            Assert.IsTrue(session.Farm.TryGetPlotState(Plot0, out PlotState plot));
            Assert.AreEqual(PlotStatus.Empty, plot.Status);
        }

        [Test]
        public void Load_OpenCountAboveData_IsClamped()
        {
            var data = NewSave();
            data.zones[0].openCount = 999;
            _store.Write(SaveService.MainKey, JsonUtility.ToJson(data));

            Session session = Open();

            Assert.AreEqual(session.Farm.State.Zones[0].Plots.Length, session.Farm.State.Zones[0].OpenCount);
            Assert.AreEqual(1, session.Farm.State.Zones[1].OpenCount);
        }

        [Test]
        public void Load_UnknownZone_IsIgnored()
        {
            var data = NewSave();
            data.zones = new[] { data.zones[0], new SaveZone { zoneId = "khu_da_bo", openCount = 3 } };
            _store.Write(SaveService.MainKey, JsonUtility.ToJson(data));

            Session session = Open();

            Assert.AreEqual(Zone1StartOpen, session.Farm.State.Zones[0].OpenCount);
        }

        [Test]
        public void Load_SeedsOfDisabledCrop_AreKept()
        {
            var data = NewSave();
            data.seeds = new[] { new SaveCount { id = "cay_chua_bat", count = 4 } };
            _store.Write(SaveService.MainKey, JsonUtility.ToJson(data));

            Session session = Open();

            Assert.AreEqual(4, session.Farm.State.GetSeeds("cay_chua_bat"));
        }

        // ---------- Mã sao lưu ----------

        [TestCase("{\"a\":1}")]
        [TestCase("{\"ten\":\"Rau muống\"}")]
        public void BackupCode_EncodeThenDecode_ReturnsSameJson(string json)
        {
            Assert.IsTrue(BackupCode.TryDecode(BackupCode.Encode(json), out string decoded));
            Assert.AreEqual(json, decoded);
        }

        [Test]
        public void BackupCode_WithWhitespace_StillDecodes()
        {
            string code = BackupCode.Encode("{\"a\":1}");

            Assert.IsTrue(BackupCode.TryDecode($"  {code.Substring(0, 5)}\n{code.Substring(5)} ", out string decoded));
            Assert.AreEqual("{\"a\":1}", decoded);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("khong phai ma")]
        [TestCase("VQ1-")]
        [TestCase("VQ1-!!!-00000000")]
        [TestCase("VQ1-e30=-00000000")]
        public void BackupCode_Invalid_IsRejected(string code)
        {
            Assert.IsFalse(BackupCode.TryDecode(code, out _));
        }

        [Test]
        public void BackupCode_Truncated_IsRejected()
        {
            string code = BackupCode.Encode("{\"coins\":123456}");

            Assert.IsFalse(BackupCode.TryDecode(code.Substring(0, code.Length - 12), out _));
        }

        [Test]
        public void ExportThenImportOnOtherDevice_RestoresProgress()
        {
            Session source = Open();
            Assert.AreEqual(FarmResult.Ok, source.Farm.BuySeeds(RauMuong, 1));
            Plant(source, Plot0);
            string code = source.Save.ExportCode();

            _store = new MemoryStore();
            Session target = Open();
            Assert.AreEqual(SaveLoadResult.Ok, target.Save.Import(code));

            Session reopened = Open();
            Assert.AreEqual(StartCoins - SeedPrice, reopened.Farm.State.Coins);
            Assert.IsTrue(reopened.Farm.TryGetPlotState(Plot0, out PlotState plot));
            Assert.AreEqual(PlotStatus.Growing, plot.Status);
        }

        [Test]
        public void Import_Success_LaterSaveDoesNotOverwriteImportedData()
        {
            Session source = Open();
            Assert.AreEqual(FarmResult.Ok, source.Farm.BuySeeds(RauMuong, 1));
            string code = source.Save.ExportCode();

            _store = new MemoryStore();
            Session target = Open();
            Assert.AreEqual(SaveLoadResult.Ok, target.Save.Import(code));
            string imported = _store.Read(SaveService.MainKey);

            Assert.AreEqual(FarmResult.Ok, target.Farm.BuySeeds(RauMuong, 1));
            target.Save.Save();

            Assert.AreEqual(imported, _store.Read(SaveService.MainKey));
        }

        [Test]
        public void Import_InvalidCode_KeepsCurrentSave()
        {
            Session session = Open();
            session.Save.Save();
            string saved = _store.Read(SaveService.MainKey);

            Assert.AreEqual(SaveLoadResult.Invalid, session.Save.Import("khong phai ma"));
            Assert.AreEqual(SaveLoadResult.Invalid, session.Save.Import(BackupCode.Encode("{}")));
            Assert.AreEqual(saved, _store.Read(SaveService.MainKey));
        }

        [Test]
        public void Import_NewerVersion_IsRejected()
        {
            Session session = Open();

            Assert.AreEqual(SaveLoadResult.NewerVersion, session.Save.Import(BackupCode.Encode(NewerSave)));
            Assert.IsNull(_store.Read(SaveService.MainKey));
        }

        // ---------- Hỗ trợ ----------

        private Session Open()
        {
            var save = new SaveService(_store, _def, _clock);
            FarmState state = save.Load();
            return new Session { Save = save, Farm = new Farm(_def, state, save.Clock) };
        }

        private static void Plant(Session session, PlotId plot)
        {
            Assert.AreEqual(FarmResult.Ok, session.Farm.Plant(plot, RauMuong));
        }

        private SaveData NewSave()
        {
            return SaveMapper.ToSave(FarmState.CreateNew(_def), _def, _clock.UtcNowSeconds);
        }

        private void WriteSave(SavePlot plot)
        {
            SaveData data = NewSave();
            data.zones[0].plots = new[] { plot };
            _store.Write(SaveService.MainKey, JsonUtility.ToJson(data));
        }

        private SavePlot Plot(int bed, int col, int row, string crop)
        {
            return new SavePlot { bed = bed, col = col, row = row, crop = crop, plantedAt = _clock.UtcNowSeconds };
        }

        private static void AssertAllEmptyOrLocked(Session session)
        {
            foreach (ZoneState zone in session.Farm.State.Zones)
            {
                foreach (PlotState plot in zone.Plots)
                    Assert.AreNotEqual(PlotStatus.Growing, plot.Status);
            }
        }

        // Khu 1: 2 ô miễn phí + 2 ô mua, ô thứ 3 ở hàng dưới (bed 0, row 1). Khu 2: 1 ô miễn phí khi mở khóa.
        private static FarmDef CreateDef()
        {
            var zones = new[]
            {
                new ZoneDef(Zone1, new[] { new BedDef(2, 2) }, Zone1StartOpen, new[] { 10, 10 }),
                new ZoneDef(Zone2, new[] { new BedDef(2, 2) }, 1, new[] { 10, 10, 10 }),
            };
            var crops = new[] { new CropDef(RauMuong, GrowSeconds, SeedPrice, 12, Yield) };
            return new FarmDef(zones, crops, StartCoins, new Dictionary<string, int> { { RauMuong, StartSeeds } }, WaterReducePercent);
        }
    }
}
