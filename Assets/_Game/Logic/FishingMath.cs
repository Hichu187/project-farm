using System;

namespace VeQue.Logic
{
    // Phép tính thuần cho phần thời gian thực của câu cá (View gọi, có test).
    public static class FishingMath
    {
        // Lực ném 0..1 sau khi giữ nút elapsedSeconds: chạy 0 → 1 → 0 hết periodSeconds rồi lặp lại.
        public static float CastPower(float elapsedSeconds, float periodSeconds)
        {
            if (periodSeconds <= 0f || elapsedSeconds <= 0f)
                return 0f;
            float phase = elapsedSeconds / periodSeconds;
            phase -= (float)Math.Floor(phase);
            return 1f - Math.Abs(phase * 2f - 1f);
        }

        // Lần rỉa đang diễn ra ở giây t (mỗi lần kéo dài durationSeconds)?
        public static bool IsNibbling(FishingSession session, float t, float durationSeconds)
        {
            foreach (float start in session.NibbleTimes)
            {
                if (t >= start && t < start + durationSeconds)
                    return true;
            }
            return false;
        }
    }
}
