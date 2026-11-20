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

        // 0 UTC + 7 giờ = 07:00; 15:42 UTC+7 = 08:42 UTC; qua nửa đêm; múi giờ âm.
        [TestCase(0L, 7 * 3600, "07:00")]
        [TestCase(8 * 3600L + 42 * 60, 7 * 3600, "15:42")]
        [TestCase(23 * 3600L + 30 * 60, 7 * 3600, "06:30")]
        [TestCase(3600L, -5 * 3600, "20:00")]
        public void Clock_LocalTime(long utc, int offset, string expected)
        {
            Assert.AreEqual(expected, UiFormat.Clock(utc, offset));
        }

        [TestCase(FarmResult.NotEnoughCoins, "Không đủ xu")]
        [TestCase(FarmResult.LevelTooLow, "Chưa đủ cấp")]
        [TestCase(FarmResult.RodNotDamaged, "Cần còn đầy độ bền")]
        [TestCase(FarmResult.UnknownRod, "Không mua được")]
        public void ShopBlocked_Messages(FarmResult result, string expected)
        {
            Assert.AreEqual(expected, UiFormat.ShopBlocked(result));
        }

        [TestCase(CastQuality.Perfect, "Hoàn hảo!")]
        [TestCase(CastQuality.Good, "Tuyệt!")]
        [TestCase(CastQuality.Normal, "Tốt")]
        [TestCase(CastQuality.Weak, "Yếu")]
        public void CastQualityName_PopText(CastQuality quality, string expected)
        {
            Assert.AreEqual(expected, UiFormat.CastQualityName(quality));
        }

        // Đợt E: dòng kỷ lục trong bảng cá vừa câu.
        [TestCase(false, 0, "")]
        [TestCase(false, 1250, "")]
        [TestCase(true, 0, "Loài mới!")]
        [TestCase(true, 1250, "Kỷ lục mới! (cũ 1,25 kg)")]
        [TestCase(true, 300, "Kỷ lục mới! (cũ 300 g)")]
        public void CatchRecord_Lines(bool isNewRecord, int previousGrams, string expected)
        {
            Assert.AreEqual(expected, UiFormat.CatchRecord(isNewRecord, previousGrams));
        }

        // Đợt 2 Phần 18: dòng cá shiny.
        [TestCase(false, "Đột biến: Tinh dạ!")]
        [TestCase(true, "Đột biến: Tinh dạ! Lần đầu!")]
        public void ShinyCatch_Lines(bool firstTime, string expected)
        {
            Assert.AreEqual(expected, UiFormat.ShinyCatch("Tinh dạ", firstTime));
        }

        [Test]
        public void ShinyDetail_Line()
        {
            Assert.AreEqual("Đột biến: Hư không", UiFormat.ShinyDetail("Hư không"));
        }

        [TestCase(FishRarity.Common, "Thường")]
        [TestCase(FishRarity.Uncommon, "Khá")]
        [TestCase(FishRarity.Rare, "Hiếm")]
        [TestCase(FishRarity.Epic, "Quý")]
        [TestCase(FishRarity.Legendary, "Huyền thoại")]
        public void RarityName_AllRarities(FishRarity rarity, string expected)
        {
            Assert.AreEqual(expected, UiFormat.RarityName(rarity));
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
        public void CatchDetail_GradeAndXp()
        {
            Assert.AreEqual("Perfect! · +2 XP", UiFormat.CatchDetail(PullGrade.Perfect, 2));
            Assert.AreEqual("Tốt · +3 XP", UiFormat.CatchDetail(PullGrade.Good, 3));
        }

        [TestCase(FishingOutcome.Scared)]
        [TestCase(FishingOutcome.BaitStolen)]
        [TestCase(FishingOutcome.Slipped)]
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
