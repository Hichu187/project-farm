using NUnit.Framework;
using VeQue.Logic;
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

        // ---------- Câu cá ----------

        [TestCase(DaySlot.Sunset, 84L, "Hoàng hôn 17–19h · còn 1:24")]
        [TestCase(DaySlot.Night, 3 * 3600L, "Khuya 23–5h · còn 3:00:00")]
        [TestCase(DaySlot.Dawn, 59L, "Bình minh 5–7h · còn 0:59")]
        public void SlotStatus_NameRangeAndCountdown(DaySlot slot, long seconds, string expected)
        {
            Assert.AreEqual(expected, UiFormat.SlotStatus(slot, seconds));
        }

        [TestCase(CastQuality.Perfect, FishDepth.Far, "Perfect! · Phao: Xa")]
        [TestCase(CastQuality.Good, FishDepth.Far, "Tốt · Phao: Xa")]
        [TestCase(CastQuality.Normal, FishDepth.Mid, "Thường · Phao: Vừa")]
        [TestCase(CastQuality.Weak, FishDepth.Near, "Ném hỏng · Phao: Gần")]
        public void CastResult_QualityAndDepth(CastQuality quality, FishDepth depth, string expected)
        {
            Assert.AreEqual(expected, UiFormat.CastResult(quality, depth));
        }

        [TestCase(5, "5 g")]
        [TestCase(999, "999 g")]
        [TestCase(1000, "1 kg")]
        [TestCase(1250, "1,25 kg")]
        [TestCase(1230, "1,23 kg")]
        [TestCase(30000, "30 kg")]
        public void Weight_GramsOrKilograms(int grams, string expected)
        {
            Assert.AreEqual(expected, UiFormat.Weight(grams));
        }

        [Test]
        public void CaughtText_NameWeightGradeXp()
        {
            Assert.AreEqual("Câu được Cá rô 350 g! Perfect! · +2 XP", UiFormat.CaughtText("Cá rô", 350, PullGrade.Perfect, 2));
            Assert.AreEqual("Câu được Cá chép 1,5 kg! Tốt · +3 XP", UiFormat.CaughtText("Cá chép", 1500, PullGrade.Good, 3));
        }

        [TestCase(FishingOutcome.Scared)]
        [TestCase(FishingOutcome.BaitStolen)]
        [TestCase(FishingOutcome.Slipped)]
        [TestCase(FishingOutcome.LineSnapped)]
        [TestCase(FishingOutcome.Escaped)]
        public void FishingOutcomeText_HasMessageForEveryMiss(FishingOutcome outcome)
        {
            Assert.IsNotEmpty(UiFormat.FishingOutcomeText(outcome));
        }

        [TestCase(FarmResult.RodBroken)]
        [TestCase(FarmResult.NotEnoughBait)]
        [TestCase(FarmResult.BasketFull)]
        [TestCase(FarmResult.NoRod)]
        public void CastBlocked_SpecificReasons(FarmResult result)
        {
            Assert.AreNotEqual("Không ném được", UiFormat.CastBlocked(result));
        }
    }
}
