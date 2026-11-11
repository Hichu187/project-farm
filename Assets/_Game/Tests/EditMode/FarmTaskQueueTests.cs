using System;
using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    public class FarmTaskQueueTests
    {
        private const string Zone1 = "khu1";
        private const string RauMuong = "rau_muong";
        private const string CaChua = "ca_chua";
        private const long RauMuongSeconds = 120;
        private const int StartSeeds = 3;
        private const int Zone1StartOpen = 6;

        private FakeClock _clock;
        private Farm _farm;
        private FarmTaskQueue _queue;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _farm = CreateFarm();
            _queue = new FarmTaskQueue();
        }

        // ---------- Thứ tự ----------

        [Test]
        public void TryBeginNext_KeepsSelectionOrder()
        {
            PlotId[] order = { Plot(4), Plot(0), Plot(2) };
            foreach (PlotId plot in order)
                Assert.AreEqual(FarmResult.Ok, _queue.TryEnqueue(_farm, FarmTask.Plant(plot, RauMuong)));

            foreach (PlotId expected in order)
            {
                Assert.IsTrue(_queue.TryBeginNext(out FarmTask task));
                Assert.AreEqual(expected, task.Plot);
                _queue.CompleteCurrent();
            }
            Assert.IsFalse(_queue.TryBeginNext(out _));
            Assert.IsTrue(_queue.IsEmpty);
        }

        [Test]
        public void TryBeginNext_WhileCurrentNotDone_False()
        {
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), RauMuong));
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(1), RauMuong));

            Assert.IsTrue(_queue.TryBeginNext(out _));
            Assert.IsFalse(_queue.TryBeginNext(out _));
            Assert.AreEqual(Plot(0), _queue.Current.Plot);
            Assert.AreEqual(2, _queue.Count);
            Assert.AreEqual(1, _queue.PendingCount);
        }

        [Test]
        public void CompleteCurrent_NoCurrent_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _queue.CompleteCurrent());
        }

        // ---------- Trùng ô ----------

        [Test]
        public void TryEnqueue_SamePlotTwice_AlreadyQueued()
        {
            Assert.AreEqual(FarmResult.Ok, _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), RauMuong)));

            Assert.AreEqual(FarmResult.AlreadyQueued, _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), RauMuong)));
            Assert.AreEqual(1, _queue.Count);
            Assert.AreEqual(1, _queue.GetReservedSeeds(RauMuong));
        }

        [Test]
        public void TryEnqueue_PlotOfCurrentTask_AlreadyQueued()
        {
            PlantAndRipen(Plot(0));
            _queue.TryEnqueue(_farm, FarmTask.Harvest(Plot(0)));
            _queue.TryBeginNext(out _);

            Assert.IsTrue(_queue.IsQueued(Plot(0)));
            Assert.AreEqual(FarmResult.AlreadyQueued, _queue.TryEnqueue(_farm, FarmTask.Harvest(Plot(0))));
        }

        [Test]
        public void TryEnqueue_AfterTaskCompleted_PlotFreeAgain()
        {
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), RauMuong));
            _queue.TryBeginNext(out FarmTask task);
            Assert.AreEqual(FarmResult.Ok, _farm.Plant(task.Plot, task.CropId));
            _queue.CompleteCurrent();

            Assert.IsFalse(_queue.IsQueued(Plot(0)));
            Assert.AreEqual(FarmResult.Ok, _queue.TryEnqueue(_farm, FarmTask.Water(Plot(0))));
        }

        // ---------- Giữ chỗ hạt ----------

        [Test]
        public void TryEnqueue_DragOverMorePlotsThanSeeds_OnlyQueuesSeedCount()
        {
            int ok = 0;
            int outOfSeeds = 0;
            for (int i = 0; i < 5; i++)
            {
                FarmResult result = _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(i), RauMuong));
                if (result == FarmResult.Ok)
                    ok++;
                else if (result == FarmResult.NotEnoughSeeds)
                    outOfSeeds++;
            }

            Assert.AreEqual(StartSeeds, ok);
            Assert.AreEqual(5 - StartSeeds, outOfSeeds);
            Assert.AreEqual(StartSeeds, _queue.GetReservedSeeds(RauMuong));
            Assert.AreEqual(0, _queue.GetAvailableSeeds(_farm, RauMuong));
            // Hạt thật chưa bị trừ cho tới khi nhân vật gieo.
            Assert.AreEqual(StartSeeds, _farm.State.GetSeeds(RauMuong));
        }

        [Test]
        public void CompleteCurrent_AfterPlanting_AvailableSeedsUnchanged()
        {
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), RauMuong));
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(1), RauMuong));
            Assert.AreEqual(StartSeeds - 2, _queue.GetAvailableSeeds(_farm, RauMuong));

            _queue.TryBeginNext(out FarmTask task);
            _farm.Plant(task.Plot, task.CropId);
            _queue.CompleteCurrent();

            Assert.AreEqual(1, _queue.GetReservedSeeds(RauMuong));
            Assert.AreEqual(StartSeeds - 1, _farm.State.GetSeeds(RauMuong));
            Assert.AreEqual(StartSeeds - 2, _queue.GetAvailableSeeds(_farm, RauMuong));
        }

        [Test]
        public void CompleteCurrent_PlantFailed_ReleasesReservation()
        {
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), RauMuong));
            _queue.TryBeginNext(out _);

            // Việc không làm được (vd. View bỏ qua): vẫn gọi CompleteCurrent, hạt được trả lại.
            _queue.CompleteCurrent();

            Assert.AreEqual(0, _queue.GetReservedSeeds(RauMuong));
            Assert.AreEqual(StartSeeds, _queue.GetAvailableSeeds(_farm, RauMuong));
        }

        [Test]
        public void Cancel_ReleasesAllReservationsIncludingCurrent()
        {
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), RauMuong));
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(1), RauMuong));
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(2), RauMuong));
            _queue.TryBeginNext(out _);

            Assert.AreEqual(3, _queue.Cancel());

            Assert.IsTrue(_queue.IsEmpty);
            Assert.IsFalse(_queue.HasCurrent);
            Assert.AreEqual(0, _queue.GetReservedSeeds(RauMuong));
            Assert.AreEqual(StartSeeds, _queue.GetAvailableSeeds(_farm, RauMuong));
            Assert.IsFalse(_queue.IsQueued(Plot(0)));
        }

        [Test]
        public void Cancel_Empty_ReturnsZeroWithoutEvent()
        {
            int changed = 0;
            _queue.Changed += () => changed++;

            Assert.AreEqual(0, _queue.Cancel());
            Assert.AreEqual(0, changed);
        }

        // ---------- Kiểm tra theo loại việc ----------

        [Test]
        public void TryEnqueue_LockedPlot_PlotLocked()
        {
            Assert.AreEqual(FarmResult.PlotLocked, _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(Zone1StartOpen), RauMuong)));
        }

        [Test]
        public void TryEnqueue_UnknownPlot_UnknownPlot()
        {
            Assert.AreEqual(FarmResult.UnknownPlot, _queue.TryEnqueue(_farm, FarmTask.Harvest(new PlotId("khong_co", 0, 0, 0))));
        }

        [Test]
        public void TryEnqueue_CropAboveLevel_CropLocked()
        {
            _farm.State.Seeds[CaChua] = 5;

            Assert.AreEqual(FarmResult.CropLocked, _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), CaChua)));
        }

        [Test]
        public void TryEnqueue_UnknownCrop_UnknownCrop()
        {
            Assert.AreEqual(FarmResult.UnknownCrop, _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), "khong_co")));
        }

        [Test]
        public void TryEnqueue_PlantOnGrowingPlot_PlotNotEmpty()
        {
            _farm.Plant(Plot(0), RauMuong);

            Assert.AreEqual(FarmResult.PlotNotEmpty, _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), RauMuong)));
        }

        [Test]
        public void TryEnqueue_WaterEmptyPlot_NothingToWater()
        {
            Assert.AreEqual(FarmResult.NothingToWater, _queue.TryEnqueue(_farm, FarmTask.Water(Plot(0))));
        }

        [Test]
        public void TryEnqueue_WaterGrowing_OkThenWateredIsRejected()
        {
            _farm.Plant(Plot(0), RauMuong);
            Assert.AreEqual(FarmResult.Ok, _queue.TryEnqueue(_farm, FarmTask.Water(Plot(0))));

            _farm.Plant(Plot(1), RauMuong);
            _farm.Water(Plot(1));
            Assert.AreEqual(FarmResult.AlreadyWatered, _queue.TryEnqueue(_farm, FarmTask.Water(Plot(1))));
        }

        [Test]
        public void TryEnqueue_WaterRipe_AlreadyRipe()
        {
            PlantAndRipen(Plot(0));

            Assert.AreEqual(FarmResult.AlreadyRipe, _queue.TryEnqueue(_farm, FarmTask.Water(Plot(0))));
        }

        [Test]
        public void TryEnqueue_HarvestNotRipe_NotRipe()
        {
            _farm.Plant(Plot(0), RauMuong);

            Assert.AreEqual(FarmResult.NotRipe, _queue.TryEnqueue(_farm, FarmTask.Harvest(Plot(0))));
        }

        [Test]
        public void TryEnqueue_HarvestEmpty_NothingToHarvest()
        {
            Assert.AreEqual(FarmResult.NothingToHarvest, _queue.TryEnqueue(_farm, FarmTask.Harvest(Plot(0))));
        }

        [Test]
        public void TryEnqueue_HarvestRipe_OkAndFarmHarvestsOnArrival()
        {
            PlantAndRipen(Plot(0));
            Assert.AreEqual(FarmResult.Ok, _queue.TryEnqueue(_farm, FarmTask.Harvest(Plot(0))));

            _queue.TryBeginNext(out FarmTask task);
            Assert.AreEqual(FarmResult.Ok, _farm.Harvest(task.Plot));
            _queue.CompleteCurrent();

            Assert.AreEqual(2, _farm.State.GetItems(RauMuong));
        }

        [Test]
        public void TryEnqueue_Rejected_NoChangedEvent()
        {
            int changed = 0;
            _queue.Changed += () => changed++;

            _queue.TryEnqueue(_farm, FarmTask.Harvest(Plot(0)));

            Assert.AreEqual(0, changed);
        }

        [Test]
        public void Changed_FiresOnEnqueueBeginCompleteCancel()
        {
            int changed = 0;
            _queue.Changed += () => changed++;

            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(0), RauMuong));
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(1), RauMuong));
            _queue.TryBeginNext(out _);
            _queue.CompleteCurrent();
            _queue.Cancel();

            Assert.AreEqual(5, changed);
        }

        // ---------- Làm hết khi ẩn tab ----------

        [Test]
        public void DrainAll_MixedTasks_EachRecheckedByFarm()
        {
            PlantAndRipen(Plot(0));
            _farm.Plant(Plot(1), RauMuong);
            _queue.TryEnqueue(_farm, FarmTask.Harvest(Plot(0)));
            _queue.TryEnqueue(_farm, FarmTask.Water(Plot(1)));
            _queue.TryEnqueue(_farm, FarmTask.Plant(Plot(2), RauMuong));

            // Đúng cách View làm khi tab bị ẩn: lấy từng việc theo thứ tự, gọi Farm, xong thì CompleteCurrent.
            var results = new List<FarmResult>();
            while (_queue.TryBeginNext(out FarmTask task))
            {
                results.Add(Execute(task));
                _queue.CompleteCurrent();
            }

            CollectionAssert.AreEqual(new[] { FarmResult.Ok, FarmResult.Ok, FarmResult.Ok }, results);
            Assert.IsTrue(_queue.IsEmpty);
            Assert.AreEqual(0, _queue.GetReservedSeeds(RauMuong));
            _farm.TryGetPlotState(Plot(1), out PlotState watered);
            Assert.IsTrue(watered.Watered);
            _farm.TryGetPlotState(Plot(2), out PlotState planted);
            Assert.AreEqual(PlotStatus.Growing, planted.Status);
        }

        [Test]
        public void Arrival_TaskNoLongerValid_FarmRejects()
        {
            // Xếp việc tưới, trong lúc nhân vật đi tới thì cây đã chín.
            _farm.Plant(Plot(0), RauMuong);
            _queue.TryEnqueue(_farm, FarmTask.Water(Plot(0)));
            _clock.Advance(RauMuongSeconds);

            _queue.TryBeginNext(out FarmTask task);

            Assert.AreEqual(FarmResult.AlreadyRipe, Execute(task));
            _queue.CompleteCurrent();
            Assert.IsTrue(_queue.IsEmpty);
        }

        // ---------- Helpers ----------

        private FarmResult Execute(FarmTask task)
        {
            switch (task.Kind)
            {
                case FarmTaskKind.Plant: return _farm.Plant(task.Plot, task.CropId);
                case FarmTaskKind.Water: return _farm.Water(task.Plot);
                case FarmTaskKind.Harvest: return _farm.Harvest(task.Plot);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void PlantAndRipen(PlotId plot)
        {
            Assert.AreEqual(FarmResult.Ok, _farm.Plant(plot, RauMuong));
            _clock.Advance(RauMuongSeconds);
        }

        // Khu 1: 2 luống 6×2 như MVP; index theo thứ tự luống → hàng → cột.
        private static PlotId Plot(int index)
        {
            int bed = index / 12;
            int inBed = index % 12;
            return new PlotId(Zone1, bed, inBed % 6, inBed / 6);
        }

        private Farm CreateFarm()
        {
            var zone = new ZoneDef(Zone1, new[] { new BedDef(6, 2), new BedDef(6, 2) }, Zone1StartOpen, new int[24 - Zone1StartOpen]);
            var crops = new[]
            {
                new CropDef(RauMuong, RauMuongSeconds, 5, 12, 2),
                new CropDef(CaChua, 3600, 30, 80, 1, unlockLevel: 3),
            };
            var def = new FarmDef(new[] { zone }, crops, 100, new Dictionary<string, int> { { RauMuong, StartSeeds } }, 20);
            return new Farm(def, FarmState.CreateNew(def), _clock);
        }
    }
}
