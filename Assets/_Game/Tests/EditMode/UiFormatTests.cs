using NUnit.Framework;
using VeQue.View.UI;

namespace VeQue.Tests
{
    public class UiFormatTests
    {
        [TestCase(0L, "0")]
        [TestCase(999L, "999")]
        [TestCase(1250L, "1.250")]
        [TestCase(1000000L, "1.000.000")]
        public void Coins_GroupsThousandsWithDot(long coins, string expected)
        {
            Assert.AreEqual(expected, UiFormat.Coins(coins));
        }

        [TestCase(0L, "0:00")]
        [TestCase(72L, "1:12")]
        [TestCase(3599L, "59:59")]
        [TestCase(3661L, "1:01:01")]
        public void Countdown_ShowsHoursOnlyWhenNeeded(long seconds, string expected)
        {
            Assert.AreEqual(expected, UiFormat.Countdown(seconds));
        }

        [TestCase(85L, 16L, "+85 xu · +16 XP")]
        [TestCase(1250L, 1000L, "+1.250 xu · +1.000 XP")]
        public void Reward_ShowsCoinsAndXp(long coins, long xp, string expected)
        {
            Assert.AreEqual(expected, UiFormat.Reward(coins, xp));
        }

        [TestCase(0, "Khu 1")]
        [TestCase(2, "Khu 3")]
        public void ZoneName_CountsFromOne(int index, string expected)
        {
            Assert.AreEqual(expected, UiFormat.ZoneName(index));
        }

        [TestCase(120L, "2 phút")]
        [TestCase(900L, "15 phút")]
        [TestCase(3600L, "1 giờ")]
        [TestCase(14400L, "4 giờ")]
        [TestCase(5400L, "90 phút")]
        public void GrowTime_UsesLargestWholeUnit(long seconds, string expected)
        {
            Assert.AreEqual(expected, UiFormat.GrowTime(seconds));
        }
    }
}
