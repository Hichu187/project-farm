using System.Collections.Generic;
using UnityEngine;

namespace VeQue.View.Fishing
{
    // Sprite đốm lấp lánh hình dấu cộng 7×7 (Đợt 2 Phần 18), sinh bằng code: tâm + phần trong trắng, đầu cánh màu của kiểu.
    // Cánh dài 1 / 2 / 3. Mỗi màu sinh 1 lần rồi dùng lại.
    public static class ShinySparkleSprites
    {
        public const int Size = 7;
        public const int MaxArm = 3;
        private const float PixelsPerUnit = 16f;

        private static readonly Dictionary<int, Sprite[]> Cache = new Dictionary<int, Sprite[]>();

        // arm 1..MaxArm.
        public static Sprite Get(Color tip, int arm)
        {
            Color32 c = tip;
            int key = (c.r << 16) | (c.g << 8) | c.b;
            if (!Cache.TryGetValue(key, out Sprite[] sprites))
            {
                sprites = new Sprite[MaxArm];
                for (int a = 1; a <= MaxArm; a++)
                    sprites[a - 1] = Create(new Color32(c.r, c.g, c.b, 255), a);
                Cache.Add(key, sprites);
            }
            return sprites[Mathf.Clamp(arm, 1, MaxArm) - 1];
        }

        private static Sprite Create(Color32 tip, int arm)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = $"ShinySparkle_{arm}",
                hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color32[Size * Size];
            var white = new Color32(255, 255, 255, 255);
            int center = Size / 2;
            pixels[center * Size + center] = white;
            for (int d = 1; d <= arm; d++)
            {
                Color32 color = d == arm ? tip : white;
                pixels[center * Size + center + d] = color;
                pixels[center * Size + center - d] = color;
                pixels[(center + d) * Size + center] = color;
                pixels[(center - d) * Size + center] = color;
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }
    }
}
