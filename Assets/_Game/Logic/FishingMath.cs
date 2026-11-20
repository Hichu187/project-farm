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

        // % giá trị theo cân nặng: nhẹ nhất WeightValueMinPercent, nặng nhất WeightValueMaxPercent, tuyến tính ở giữa.
        public static float WeightValuePercent(FishDef fish, int grams, FishingTuning tuning)
        {
            int span = fish.MaxGrams - fish.MinGrams;
            float ratio = span == 0 ? 0.5f : (grams - fish.MinGrams) / (float)span;
            if (ratio < 0f)
                ratio = 0f;
            else if (ratio > 1f)
                ratio = 1f;
            return tuning.WeightValueMinPercent + (tuning.WeightValueMaxPercent - tuning.WeightValueMinPercent) * ratio;
        }

        // Giá 1 con cá theo cân nặng (ít nhất 1 xu); cá shiny nhân hệ số của bậc (Đợt 2 Phần 18).
        public static int FishPrice(FishDef fish, int grams, FishingTuning tuning, ShinyKind shiny = ShinyKind.None)
            => Math.Max(1, (int)Math.Round(fish.BasePrice * WeightValuePercent(fish, grams, tuning) / 100f))
                * tuning.ShinyPriceMultiplier(shiny);

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
