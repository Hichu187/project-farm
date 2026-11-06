using System.Globalization;

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
    }
}
