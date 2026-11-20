using System.Globalization;
using VeQue.Logic;

namespace VeQue.View.UI
{
    public static class UiFormat
    {
        private const long SecondsPerMinute = 60;
        private const long SecondsPerHour = 3600;

        // Không dùng CultureInfo "vi-VN" vì có thể không có sẵn trên WebGL.
        public static string Coins(long coins)
        {
            return coins.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', '.');
        }

        public static string Countdown(long seconds)
        {
            long hours = seconds / SecondsPerHour;
            long minutes = seconds % SecondsPerHour / SecondsPerMinute;
            long rest = seconds % SecondsPerMinute;
            return hours > 0 ? $"{hours}:{minutes:00}:{rest:00}" : $"{minutes}:{rest:00}";
        }

        public static string Reward(long coins, long xp) => $"+{Coins(coins)} xu · +{Coins(xp)} XP";

        // Khu theo thứ tự mở khóa, đếm từ 1.
        public static string ZoneName(int index) => $"Khu {index + 1}";

        public static string GrowTime(long seconds)
        {
            if (seconds >= SecondsPerHour && seconds % SecondsPerHour == 0)
                return $"{seconds / SecondsPerHour} giờ";
            return $"{seconds / SecondsPerMinute} phút";
        }

        // ---------- Câu cá ----------

        private static readonly string[] SlotNames = { "Bình minh", "Sáng", "Trưa", "Chiều", "Hoàng hôn", "Tối", "Khuya" };

        public static string SlotName(DaySlot slot) => SlotNames[(int)slot];

        // Đồng hồ chung trên HUD (giờ địa phương theo độ lệch múi giờ đã lưu): "15:42". Tên khung giờ hiện dòng dưới (SlotName).
        // Ngày âm lịch: "Mùng 1 tháng 11", "Rằm tháng Chạp", "20 tháng Giêng", tháng nhuận thêm " nhuận".
        public static string LunarDay(LunarDate date)
        {
            string day = date.Day <= 10 ? $"Mùng {date.Day}" : date.Day == 15 ? "Rằm" : date.Day.ToString();
            string month = date.Month == 1 ? "Giêng" : date.Month == 12 ? "Chạp" : date.Month.ToString();
            return $"{day} tháng {month}{(date.IsLeapMonth ? " nhuận" : "")}";
        }

        public static string Clock(long utcSeconds, int utcOffsetSeconds)
        {
            long local = utcSeconds + utcOffsetSeconds;
            long ofDay = ((local % 86400) + 86400) % 86400;
            long hour = ofDay / 3600;
            long minute = ofDay % 3600 / 60;
            return $"{hour:00}:{minute:00}";
        }

        // "Hoàng hôn 17–19h · còn 1:24".
        public static string SlotStatus(DaySlot slot, long secondsLeft) =>
            $"{SlotName(slot)} {DaySlots.StartHour(slot)}–{DaySlots.EndHour(slot)}h · còn {Countdown(secondsLeft)}";

        // Chữ nổi trên đầu nhân vật sau khi ném (chốt 13/11/2026).
        public static string CastQualityName(CastQuality quality)
        {
            switch (quality)
            {
                case CastQuality.Perfect: return "Hoàn hảo!";
                case CastQuality.Good: return "Tuyệt!";
                case CastQuality.Normal: return "Tốt";
                default: return "Yếu";
            }
        }

        public static string RarityName(FishRarity rarity)
        {
            switch (rarity)
            {
                case FishRarity.Uncommon: return "Khá";
                case FishRarity.Rare: return "Hiếm";
                case FishRarity.Epic: return "Quý";
                case FishRarity.Legendary: return "Huyền thoại";
                default: return "Thường";
            }
        }

        public static string DepthName(FishDepth depth)
        {
            switch (depth)
            {
                case FishDepth.Far: return "Xa";
                case FishDepth.Mid: return "Vừa";
                default: return "Gần";
            }
        }

        // Dưới 1 kg: "350 g"; từ 1 kg: "1,25 kg" (tối đa 2 số lẻ, dấu phẩy).
        public static string Weight(int grams)
        {
            if (grams < 1000)
                return $"{grams} g";
            return (grams / 1000.0).ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',') + " kg";
        }

        public static string PullGradeName(PullGrade grade)
        {
            switch (grade)
            {
                case PullGrade.Perfect: return "Perfect!";
                case PullGrade.Good: return "Tốt";
                default: return "Thường";
            }
        }

        // Bảng cá vừa câu: "Perfect! · +2 XP".
        public static string CatchDetail(PullGrade grade, int xp) => $"{PullGradeName(grade)} · +{xp} XP";

        // Dòng kỷ lục trong bảng cá vừa câu (Đợt E): lần đầu câu loài → "Loài mới!"; nặng hơn kỷ lục cũ → "Kỷ lục mới! (cũ …)";
        // không phá kỷ lục → "" (ẩn dòng).
        // Cá shiny (Đợt 2 Phần 18): dòng trên bảng cá vừa câu.
        public static string ShinyCatch(string kindName, bool firstTime)
            => firstTime ? $"Đột biến: {kindName}! Lần đầu!" : $"Đột biến: {kindName}!";

        // Cá shiny: dòng trong chi tiết cá ở giỏ.
        public static string ShinyDetail(string kindName) => $"Đột biến: {kindName}";

        public static string CatchRecord(bool isNewRecord, int previousRecordGrams)
        {
            if (!isNewRecord)
                return "";
            return previousRecordGrams <= 0 ? "Loài mới!" : $"Kỷ lục mới! (cũ {Weight(previousRecordGrams)})";
        }

        // Kết quả không ra cá: chữ ngắn nổi trên đầu nhân vật (chốt 13/11/2026). Cá ra thì bảng cá vừa câu.
        public static string FishingOutcomeText(FishingOutcome outcome)
        {
            switch (outcome)
            {
                case FishingOutcome.Scared: return "Sớm quá!";
                case FishingOutcome.BaitStolen: return "Mất mồi!";
                case FishingOutcome.Slipped: return "Trượt lưỡi!";
                case FishingOutcome.Escaped: return "Cá thoát!";
                default: return "";
            }
        }

        // Lý do không mua / sửa được đồ câu (Farm.BuyRod / BuyBait / RepairRod).
        public static string ShopBlocked(FarmResult result)
        {
            switch (result)
            {
                case FarmResult.NotEnoughCoins: return "Không đủ xu";
                case FarmResult.LevelTooLow: return "Chưa đủ cấp";
                case FarmResult.AlreadyOwned: return "Đã có cần này";
                case FarmResult.RodNotDamaged: return "Cần còn đầy độ bền";
                case FarmResult.FishingLocked: return "Chưa mở câu cá";
                default: return "Không mua được";
            }
        }

        // Lý do không ném được (Farm.CanCast).
        public static string CastBlocked(FarmResult result)
        {
            switch (result)
            {
                case FarmResult.FishingLocked: return "Chưa mở câu cá";
                case FarmResult.NoRod: return "Chưa có cần câu";
                case FarmResult.RodBroken: return "Cần hỏng rồi, cần sửa hoặc mua cần mới";
                case FarmResult.NotEnoughBait: return "Hết mồi rồi";
                case FarmResult.BasketFull: return "Giỏ cá đầy rồi";
                default: return "Không ném được";
            }
        }
    }
}
