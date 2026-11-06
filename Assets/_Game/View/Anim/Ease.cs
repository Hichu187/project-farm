using System;

namespace VeQue.View.Anim
{
    // Hàm làm mượt cho hoạt ảnh tự viết (không dùng UnityEngine để test được ngoài Unity). t ngoài [0, 1] bị kẹp lại.
    public static class Ease
    {
        // Overshoot chuẩn của Robert Penner: OutBack lố khoảng 10% rồi về 1.
        public const float DefaultOvershoot = 1.70158f;

        public static float Clamp01(float t) => t < 0f ? 0f : t > 1f ? 1f : t;

        // Nhanh rồi chậm dần.
        public static float OutQuad(float t)
        {
            t = Clamp01(t);
            return 1f - (1f - t) * (1f - t);
        }

        // Chậm rồi nhanh dần.
        public static float InQuad(float t)
        {
            t = Clamp01(t);
            return t * t;
        }

        // Đi từ 0, lố quá 1 rồi về đúng 1.
        public static float OutBack(float t, float overshoot = DefaultOvershoot)
        {
            t = Clamp01(t) - 1f;
            return 1f + (overshoot + 1f) * t * t * t + overshoot * t * t;
        }

        // Một nhịp sóng tắt dần: 0 → âm → dương → 0. Dùng cho nhún (ép dẹt rồi vươn cao).
        public static float DampedWave(float t)
        {
            t = Clamp01(t);
            return -(float)Math.Sin(2.0 * Math.PI * t) * (1f - t);
        }

        // Làm tròn về số nguyên pixel (đơn vị world), để ảnh pixel art không bị nhòe khi di chuyển.
        public static float SnapToPixel(float value, float pixelsPerUnit)
        {
            return (float)Math.Round(value * pixelsPerUnit, MidpointRounding.AwayFromZero) / pixelsPerUnit;
        }
    }
}
