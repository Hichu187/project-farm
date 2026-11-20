using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Một mốc màu trời (Đợt 2 Phần 13): lúc Hour giờ (0–24, giờ VN), hệ số nhân màu (R, G, B; 1 = không đổi).
    public readonly struct DayNightKey
    {
        public readonly float Hour;
        public readonly float R;
        public readonly float G;
        public readonly float B;

        public DayNightKey(float hour, float r, float g, float b)
        {
            Hour = hour;
            R = r;
            G = g;
            B = b;
        }

        // Độ sáng (0,299R + 0,587G + 0,114B).
        public float Luminance => 0.299f * R + 0.587f * G + 0.114f * B;
    }

    // Màu trời theo giờ: nội suy tuyến tính giữa các mốc, vòng qua nửa đêm. Chạy theo giờ thật (GameCalendar, UTC+7).
    public static class DayNightCurve
    {
        // Giờ trong ngày (0 ≤ h < 24, có phần lẻ) theo giờ VN.
        public static float HourOfDay(long utcSeconds)
        {
            long local = utcSeconds + GameCalendar.OffsetSeconds;
            long ofDay = ((local % GameCalendar.SecondsPerDay) + GameCalendar.SecondsPerDay) % GameCalendar.SecondsPerDay;
            return ofDay / 3600f;
        }

        // keys: sắp theo Hour tăng dần, mỗi Hour trong [0, 24). Không có mốc nào → trắng (1, 1, 1).
        public static DayNightKey Evaluate(IReadOnlyList<DayNightKey> keys, float hour)
        {
            if (keys == null || keys.Count == 0)
                return new DayNightKey(hour, 1f, 1f, 1f);
            hour = ((hour % 24f) + 24f) % 24f;
            if (keys.Count == 1)
                return new DayNightKey(hour, keys[0].R, keys[0].G, keys[0].B);

            // Mốc a ≤ hour < mốc b (b có thể là mốc đầu của ngày hôm sau).
            int last = keys.Count - 1;
            DayNightKey a = keys[last];
            DayNightKey b = keys[0];
            float aHour = a.Hour - 24f;
            float bHour = b.Hour;
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Hour > hour)
                {
                    b = keys[i];
                    bHour = b.Hour;
                    a = i > 0 ? keys[i - 1] : keys[last];
                    aHour = i > 0 ? a.Hour : a.Hour - 24f;
                    break;
                }
                if (i == last)
                {
                    a = keys[last];
                    aHour = a.Hour;
                    b = keys[0];
                    bHour = b.Hour + 24f;
                }
            }
            float span = bHour - aHour;
            float t = span > 0f ? (hour - aHour) / span : 0f;
            return new DayNightKey(hour, Lerp(a.R, b.R, t), Lerp(a.G, b.G, t), Lerp(a.B, b.B, t));
        }

        // Kiểm tra mốc: tăng dần, trong [0, 24), hệ số 0–1.
        public static void Validate(IReadOnlyList<DayNightKey> keys)
        {
            if (keys == null)
                return;
            for (int i = 0; i < keys.Count; i++)
            {
                DayNightKey k = keys[i];
                if (k.Hour < 0f || k.Hour >= 24f)
                    throw new ArgumentException($"Mốc ngày đêm {i}: giờ phải trong 0–24.");
                if (i > 0 && keys[i - 1].Hour >= k.Hour)
                    throw new ArgumentException($"Mốc ngày đêm {i}: giờ phải tăng dần.");
                if (k.R < 0f || k.R > 1f || k.G < 0f || k.G > 1f || k.B < 0f || k.B > 1f)
                    throw new ArgumentException($"Mốc ngày đêm {i}: màu phải trong 0–1.");
            }
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
