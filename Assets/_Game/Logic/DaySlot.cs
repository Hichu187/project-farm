using System;

namespace VeQue.Logic
{
    // 7 khung giờ trong ngày (giờ địa phương của máy), dùng cho cá xuất hiện theo giờ (MVP-Plan Phần 10 mục 4b).
    public enum DaySlot
    {
        Dawn = 0,       // Bình minh 5–7h
        Morning = 1,    // Sáng 7–11h
        Noon = 2,       // Trưa 11–14h
        Afternoon = 3,  // Chiều 14–17h
        Sunset = 4,     // Hoàng hôn 17–19h
        Evening = 5,    // Tối 19–23h
        Night = 6,      // Khuya 23–5h
    }

    public static class DaySlots
    {
        public const int Count = 7;

        // Giờ bắt đầu của từng khung (Khuya bắt đầu 23h, qua nửa đêm tới 5h).
        private static readonly int[] StartHours = { 5, 7, 11, 14, 17, 19, 23 };

        public static int StartHour(DaySlot slot) => StartHours[(int)slot];

        public static int EndHour(DaySlot slot) => StartHours[((int)slot + 1) % Count];

        public static DaySlot FromHour(int hour)
        {
            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour));
            for (int i = Count - 1; i >= 0; i--)
            {
                if (hour >= StartHours[i])
                    return (DaySlot)i;
            }
            // 0–4h: Khuya.
            return DaySlot.Night;
        }

        // utcSeconds: giờ Unix; utcOffsetSeconds: độ lệch múi giờ (FarmState.UtcOffsetSeconds).
        public static DaySlot Get(long utcSeconds, int utcOffsetSeconds)
        {
            return FromHour((int)(SecondsOfDay(utcSeconds, utcOffsetSeconds) / 3600));
        }

        // Số giây còn lại tới hết khung giờ hiện tại (Khuya 23h → 5h qua nửa đêm). Luôn > 0.
        public static long SecondsUntilEnd(long utcSeconds, int utcOffsetSeconds)
        {
            long secondsOfDay = SecondsOfDay(utcSeconds, utcOffsetSeconds);
            DaySlot slot = FromHour((int)(secondsOfDay / 3600));
            long end = EndHour(slot) * 3600L;
            if (end <= secondsOfDay)
                end += 86400;
            return end - secondsOfDay;
        }

        private static long SecondsOfDay(long utcSeconds, int utcOffsetSeconds)
        {
            long local = utcSeconds + utcOffsetSeconds;
            return ((local % 86400) + 86400) % 86400;
        }
    }
}
