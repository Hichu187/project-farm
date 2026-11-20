using System;

namespace VeQue.Logic
{
    // Giờ TRONG GAME (chốt 21/11/2026): 1 ngày game dài DayLengthSeconds giây thật (mặc định 30 phút, chỉnh trong data).
    // CHỈ dùng cho hiển thị: màu trời ngày đêm và đồng hồ hh:mm trên HUD. Mọi logic theo thời gian (giờ ra cá, âm lịch,
    // sự kiện, điểm danh, giới hạn đơn/ngày, cây lớn) dùng giờ thật qua GameCalendar.
    // Tính thẳng từ giờ Unix nên không cần lưu, mọi người chơi cùng lúc thấy cùng giờ game.
    public sealed class GameDayClock
    {
        public const int DefaultDayLengthSeconds = 30 * 60;
        public const int MinDayLengthSeconds = 60;
        // Ban ngày (icon mặt trời): DayStartHour ≤ giờ < NightStartHour.
        public const float DayStartHour = 6f;
        public const float NightStartHour = 18f;

        public int DayLengthSeconds { get; }
        // Lệch pha (giây thật) để chọn lúc nào bình minh; 0 = 0 giờ game ở mỗi bội số DayLengthSeconds của giờ Unix.
        public int PhaseSeconds { get; }

        public GameDayClock(int dayLengthSeconds = DefaultDayLengthSeconds, int phaseSeconds = 0)
        {
            if (dayLengthSeconds < MinDayLengthSeconds)
                throw new ArgumentOutOfRangeException(nameof(dayLengthSeconds), $"Ngày game phải dài ít nhất {MinDayLengthSeconds} giây.");
            DayLengthSeconds = dayLengthSeconds;
            PhaseSeconds = phaseSeconds;
        }

        // Giờ game 0 ≤ h < 24 (có phần lẻ).
        public float HourOf(long utcSeconds)
        {
            long t = utcSeconds + PhaseSeconds;
            long ofDay = ((t % DayLengthSeconds) + DayLengthSeconds) % DayLengthSeconds;
            return (float)(ofDay * 24.0 / DayLengthSeconds);
        }

        // Phút game trong ngày (0–1439) – để đồng hồ chỉ dựng lại chuỗi khi đổi phút.
        public static int MinuteOfDay(float hour)
        {
            int minute = (int)Math.Floor(hour * 60f);
            return ((minute % 1440) + 1440) % 1440;
        }

        public static string Format(float hour)
        {
            int minute = MinuteOfDay(hour);
            return $"{minute / 60:00}:{minute % 60:00}";
        }

        public static bool IsDaytime(float hour) => hour >= DayStartHour && hour < NightStartHour;
    }
}
