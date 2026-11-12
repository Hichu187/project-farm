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

        // "Hoàng hôn 17–19h · còn 1:24".
        public static string SlotStatus(DaySlot slot, long secondsLeft) =>
            $"{SlotName(slot)} {DaySlots.StartHour(slot)}–{DaySlots.EndHour(slot)}h · còn {Countdown(secondsLeft)}";

        public static string CastQualityName(CastQuality quality)
        {
            switch (quality)
            {
                case CastQuality.Perfect: return "Perfect!";
                case CastQuality.Good: return "Tốt";
                case CastQuality.Normal: return "Thường";
                default: return "Ném hỏng";
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

        // "Perfect! · Phao: Xa".
        public static string CastResult(CastQuality quality, FishDepth depth) => $"{CastQualityName(quality)} · Phao: {DepthName(depth)}";

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

        // Tạm thời bằng thông báo; Đợt E thay bằng hộp kết quả. "Câu được Cá rô 350 g! Perfect! · +2 XP".
        public static string CaughtText(string fishName, int grams, PullGrade grade, int xp) =>
            $"Câu được {fishName} {Weight(grams)}! {PullGradeName(grade)} · +{xp} XP";

        // Kết quả không ra cá (cá ra thì hộp kết quả riêng – Đợt E).
        public static string FishingOutcomeText(FishingOutcome outcome)
        {
            switch (outcome)
            {
                case FishingOutcome.Scared: return "Giật sớm quá, cá sợ bỏ đi";
                case FishingOutcome.BaitStolen: return "Cá ăn mất mồi rồi";
                case FishingOutcome.Slipped: return "Trượt lưỡi, cá sổng mất!";
                case FishingOutcome.LineSnapped: return "Bấm sai, đứt dây rồi!";
                case FishingOutcome.Escaped: return "Chậm quá, cá thoát mất!";
                default: return "";
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
