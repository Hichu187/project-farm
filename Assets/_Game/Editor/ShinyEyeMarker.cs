using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VeQue.EditorTools
{
    // Cá shiny (Đợt 2 Phần 18, Bước 3): đánh dấu vòng trắng mắt trong PNG cá bằng alpha 254 (nhìn như 255) để shader
    // VeQue/ShinyIcon tô trắng mà không phủ màu kiểu. Cách nhận mắt giống ảnh mô phỏng shiny_8kieu.py:
    // con ngươi = khối màu viền #181425 từ 2×2 trở lên; pixel sáng (độ sáng > 0,8) cách con ngươi ≤ 2 ô là trắng mắt.
    // Sửa thẳng file PNG (giữ .meta). Chạy lại nhiều lần không sao; vẽ lại cá thì chạy lại.
    public static class ShinyEyeMarker
    {
        private const string FishFolder = "Assets/_Game/Art/Fishing/fish";
        private const byte Marker = 254;
        private static readonly Color32 Navy = new Color32(24, 20, 37, 255);

        [MenuItem("Về Quê/Cá shiny/Đánh dấu trắng mắt", priority = 401)]
        public static void MarkAll()
        {
            string[] files = Directory.GetFiles(FishFolder, "*.png");
            System.Array.Sort(files);
            var report = new StringBuilder();
            int missing = 0;
            foreach (string file in files)
            {
                string path = file.Replace('\\', '/');
                int marked = Mark(path, out bool changed);
                if (marked == 0)
                    missing++;
                report.AppendLine($"{Path.GetFileNameWithoutExtension(path)}: {marked} pixel{(changed ? "" : " (không đổi)")}"
                    + (marked == 0 ? "  ⚠ không tìm thấy mắt" : ""));
            }
            AssetDatabase.Refresh();
            Debug.Log($"Cá shiny – đánh dấu trắng mắt:\n{report}");
            EditorUtility.DisplayDialog("Cá shiny: đánh dấu trắng mắt",
                $"{files.Length} file, {missing} file không tìm thấy mắt.\nChi tiết từng loài trong Console.", "OK");
        }

        // Trả về số pixel trắng mắt; changed = có ghi lại file.
        private static int Mark(string path, out bool changed)
        {
            changed = false;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                    return 0;
                int w = texture.width;
                int h = texture.height;
                Color32[] px = texture.GetPixels32();

                var pupil = new bool[w * h];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                        pupil[y * w + x] = IsPupil(px, w, h, x, y);
                }

                int marked = 0;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        Color32 c = px[i];
                        bool eye = c.a > 0 && !IsNavy(c) && Luminance(c) > 0.8f && NearPupil(pupil, w, h, x, y);
                        byte alpha = eye ? Marker : (c.a == Marker ? (byte)255 : c.a);
                        if (eye)
                            marked++;
                        if (alpha != c.a)
                        {
                            px[i].a = alpha;
                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    texture.SetPixels32(px);
                    texture.Apply(false);
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                }
                return marked;
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        // Con ngươi: pixel navy không nằm ở mép ảnh và thuộc một khối navy 2×2.
        private static bool IsPupil(Color32[] px, int w, int h, int x, int y)
        {
            if (x <= 0 || y <= 0 || x >= w - 1 || y >= h - 1 || !IsNavyOpaque(px[y * w + x]))
                return false;
            for (int ox = -1; ox <= 0; ox++)
            {
                for (int oy = -1; oy <= 0; oy++)
                {
                    bool block = true;
                    for (int i = 0; i < 2 && block; i++)
                    {
                        for (int j = 0; j < 2 && block; j++)
                        {
                            int a = x + ox + i;
                            int b = y + oy + j;
                            block = a >= 0 && b >= 0 && a < w && b < h && IsNavyOpaque(px[b * w + a]);
                        }
                    }
                    if (block)
                        return true;
                }
            }
            return false;
        }

        private static bool NearPupil(bool[] pupil, int w, int h, int x, int y)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    int a = x + dx;
                    int b = y + dy;
                    if (a >= 0 && b >= 0 && a < w && b < h && pupil[b * w + a])
                        return true;
                }
            }
            return false;
        }

        private static bool IsNavy(Color32 c) => c.r == Navy.r && c.g == Navy.g && c.b == Navy.b;

        private static bool IsNavyOpaque(Color32 c) => c.a > 0 && IsNavy(c);

        private static float Luminance(Color32 c) => (0.299f * c.r + 0.587f * c.g + 0.114f * c.b) / 255f;
    }
}
