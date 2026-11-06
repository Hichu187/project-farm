using System;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    public class ClockTests
    {
        [Test]
        public void OffsetClock_NoOffset_MatchesInner()
        {
            var inner = new FakeClock();
            var clock = new OffsetClock(inner);

            Assert.AreEqual(inner.UtcNowSeconds, clock.UtcNowSeconds);
        }

        [Test]
        public void OffsetClock_Advance_AddsToInnerTime()
        {
            var inner = new FakeClock();
            var clock = new OffsetClock(inner, 60);

            clock.Advance(3600);
            inner.Advance(5);

            Assert.AreEqual(3660, clock.OffsetSeconds);
            Assert.AreEqual(inner.UtcNowSeconds + 3660, clock.UtcNowSeconds);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void OffsetClock_AdvanceNotForward_Throws(long seconds)
        {
            var clock = new OffsetClock(new FakeClock());

            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(seconds));
        }

        [Test]
        public void OffsetClock_NegativeOffset_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new OffsetClock(new FakeClock(), -1));
        }

        [Test]
        public void OffsetClock_Advance_RipensCrop()
        {
            var clock = new OffsetClock(new FakeClock());
            var zones = new[] { new ZoneDef("khu1", new[] { new BedDef(1, 1) }, 1, new int[0]) };
            var crops = new[] { new CropDef("rau_muong", 120, 5, 5, 2) };
            var def = new FarmDef(zones, crops, 0, new System.Collections.Generic.Dictionary<string, int> { { "rau_muong", 1 } });
            var farm = new Farm(def, FarmState.CreateNew(def), clock);
            var plot = new PlotId("khu1", 0, 0, 0);
            Assert.AreEqual(FarmResult.Ok, farm.Plant(plot, "rau_muong"));

            clock.Advance(120);

            Assert.IsTrue(farm.IsRipe(plot));
        }
    }
}
