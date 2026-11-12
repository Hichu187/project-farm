using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VeQue.EditorTools
{
    // Menu Về Quê → UI Sunnyside: ghép 9-slice.
    // Sunnyside để khung hộp thành 9 file 3×3 rời và nhãn thành 3 file rời, Unity không dùng thẳng làm Image Sliced được.
    // Công cụ ghép thành sprite mới trong Assets/_Game/Art/UI (không sửa asset gốc), đặt import PPU 16, Point,
    // không nén, không mipmap, kèm Border 9-slice. Chạy lại nhiều lần không sao (ghi đè file đã ghép).
    // Ngoài 3 khung gốc (lt / dt / w) còn tô lại khung w thành nhiều sắc độ: giữ viền, đổi màu vệt sáng + nền.
    public static class SunnysideUiImporter
    {
        public const string OutputFolder = "Assets/_Game/Art/UI";

        private const string SourceFolder = "Assets/Art/Sunnyside_World_ASSET_PACK_V2.1/Sunnyside_World_Assets/UI/";
        private const string SliceFolder = SourceFolder + "9slice_box_white/";
        private const int Piece = 3;
        private const int LabelEdge = 4;
        private const string Title = "UI Sunnyside";

        // Màu trong khung gốc w (vệt sáng, nền) và nhãn gốc (vệt sáng, nền); viền #181425 giữ nguyên.
        private static readonly Color32 WhiteHighlight = Hex("ffffff");
        private static readonly Color32 WhiteFill = Hex("c0cbdc");
        private static readonly Color32 LabelHighlight = Hex("ead4aa");
        private static readonly Color32 LabelFill = Hex("c28569");

        // Khung tô lại từ w: tên, vệt sáng, nền. Từ sáng tới đậm theo tông gỗ Sunnyside, thêm vài màu nút.
        private static readonly (string Name, string Highlight, string Fill)[] BoxShades =
        {
            ("paper", "fffbf0", "f7ebd2"),
            ("cream", "fbf2dc", "ead4aa"),
            ("sand", "f4e0bc", "d9ae84"),
            ("brown", "e4a672", "b86f50"),
            ("green", "b8e88a", "63c74d"),
            ("red", "f8b8a0", "e4573f"),
            ("blue", "c0e8f4", "5fb0d8"),
        };

        private static readonly (string Name, string Highlight, string Fill)[] LabelShades =
        {
            ("paper", "fffbf0", "f7ebd2"),
            ("cream", "fbf2dc", "ead4aa"),
        };

        // Tên khung: tan (lt gốc), dark (dt gốc), gray (w gốc) + các tên trong BoxShades.
        public static string BoxPath(string name) => $"{OutputFolder}/ui_box_{name}.png";

        // Tên nhãn: tan (gốc) + các tên trong LabelShades.
        public static string LabelPath(string name) => $"{OutputFolder}/ui_label_{name}.png";

        // Khung thanh (greenbar_00: viền + rãnh tối), 9-slice; phần đầy do UI vẽ bằng màu BarFill.
        public static string BarPath => $"{OutputFolder}/ui_bar.png";
        // Vị trí rãnh trong khung thanh (pixel): trái, dưới, phải, trên.
        public static readonly Vector4 BarTrackInset = new Vector4(2, 3, 2, 2);
        // Màu xanh của greenbar_06.
        public static readonly Color BarFill = new Color32(0x63, 0xc7, 0x4d, 0xff);

        // Icon chép nguyên từ Sunnyside: confirm, cancel, sandtimer, stopwatch, rod (chỗ câu),
        // mũi tên kéo cá (arrow_up-1 của Sunnyside là mũi tên xuống).
        public static string IconPath(string name) => $"{OutputFolder}/ui_icon_{name}.png";

        public const string ArrowUp = "arrow_up";
        public const string ArrowDown = "arrow_up-1";
        public const string ArrowLeft = "arrow_left";
        public const string ArrowRight = "arrow_right";

        private static readonly string[] Icons =
        {
            "confirm", "cancel", "sandtimer", "stopwatch", "rod", ArrowUp, ArrowDown, ArrowLeft, ArrowRight,
        };

        // Tên file của bản ghép đầu tiên (12/11/2026), xóa khi chạy lại cho gọn.
        // Không có ui_box_dark.png: tên đó vẫn dùng cho khung dt.
        private static readonly string[] OldOutputs =
        {
            OutputFolder + "/ui_box_light.png",
            OutputFolder + "/ui_box_white.png",
            OutputFolder + "/ui_label.png",
        };

        // Thứ tự đọc: hàng trên → dưới, trái → phải (toạ độ ảnh tính từ dưới lên nên đặt hàng trên ở y cao).
        private static readonly string[,] Parts =
        {
            { "tl", "tc", "tr" },
            { "lc", "c", "rc" },
            { "bl", "bc", "br" },
        };

        [MenuItem("Về Quê/UI Sunnyside: ghép 9-slice")]
        private static void Build()
        {
            var errors = new List<string>();
            Directory.CreateDirectory(OutputFolder);
            var boxBorder = new Vector4(Piece, Piece, Piece, Piece);
            int count = 0;

            Texture2D tan = BuildBox("lt", errors);
            Texture2D dark = BuildBox("dt", errors);
            Texture2D gray = BuildBox("w", errors);
            Texture2D label = BuildLabel(errors);
            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog(Title, "Có lỗi:\n" + string.Join("\n", errors), "OK");
                return;
            }

            foreach ((string name, string highlight, string fill) in BoxShades)
            {
                Save(Recolor(gray, WhiteHighlight, WhiteFill, Hex(highlight), Hex(fill)), BoxPath(name), boxBorder);
                count++;
            }
            var labelBorder = new Vector4(LabelEdge, LabelEdge, LabelEdge, LabelEdge);
            foreach ((string name, string highlight, string fill) in LabelShades)
            {
                Save(Recolor(label, LabelHighlight, LabelFill, Hex(highlight), Hex(fill)), LabelPath(name), labelBorder);
                count++;
            }

            Save(tan, BoxPath("tan"), boxBorder);
            Save(dark, BoxPath("dark"), boxBorder);
            Save(gray, BoxPath("gray"), boxBorder);
            Save(label, LabelPath("tan"), labelBorder);
            count += 4;

            // Khung thanh: viền dưới dày hơn (có bóng) nên border dưới 3.
            Texture2D bar = LoadPng(SourceFolder + "greenbar_00.png", errors);
            if (bar != null)
            {
                Save(bar, BarPath, new Vector4(2, 3, 2, 2));
                count++;
            }
            foreach (string icon in Icons)
            {
                Texture2D texture = LoadPng(SourceFolder + icon + ".png", errors);
                if (texture == null)
                    continue;
                Save(texture, IconPath(icon), Vector4.zero);
                count++;
            }
            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog(Title, "Có lỗi:\n" + string.Join("\n", errors), "OK");
                return;
            }

            foreach (string old in OldOutputs)
            {
                if (AssetDatabase.LoadMainAssetAtPath(old) != null)
                    AssetDatabase.DeleteAsset(old);
            }

            EditorUtility.DisplayDialog(Title,
                $"Đã tạo {count} sprite trong {OutputFolder}.\nChạy lại Về Quê → Tạo UI để dùng.", "OK");
        }

        private static Texture2D BuildBox(string prefix, List<string> errors)
        {
            var result = NewTexture(Piece * 3, Piece * 3);
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    string path = $"{SliceFolder}{prefix}_box_9slice_{Parts[row, col]}.png";
                    Texture2D piece = LoadPng(path, errors);
                    if (piece == null)
                        continue;
                    if (piece.width != Piece || piece.height != Piece)
                        errors.Add($"{path}: cần {Piece}×{Piece}, đang {piece.width}×{piece.height}.");
                    else
                        Blit(piece, result, col * Piece, (2 - row) * Piece);
                    Object.DestroyImmediate(piece);
                }
            }
            return result;
        }

        private static Texture2D BuildLabel(List<string> errors)
        {
            Texture2D left = LoadPng(SourceFolder + "label_left.png", errors);
            Texture2D middle = LoadPng(SourceFolder + "label_middle.png", errors);
            Texture2D right = LoadPng(SourceFolder + "label_right.png", errors);
            Texture2D result = null;
            if (left != null && middle != null && right != null)
            {
                if (left.height != middle.height || right.height != middle.height)
                {
                    errors.Add("label_left / middle / right không cùng chiều cao.");
                }
                else
                {
                    result = NewTexture(left.width + middle.width + right.width, middle.height);
                    Blit(left, result, 0, 0);
                    Blit(middle, result, left.width, 0);
                    Blit(right, result, left.width + middle.width, 0);
                }
            }

            if (left != null)
                Object.DestroyImmediate(left);
            if (middle != null)
                Object.DestroyImmediate(middle);
            if (right != null)
                Object.DestroyImmediate(right);
            return result;
        }

        // Bản sao đổi đúng 2 màu (vệt sáng, nền); viền và pixel trong suốt giữ nguyên.
        private static Texture2D Recolor(Texture2D source, Color32 fromHighlight, Color32 fromFill, Color32 toHighlight, Color32 toFill)
        {
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 p = pixels[i];
                if (p.a == 0)
                    continue;
                if (SameRgb(p, fromHighlight))
                    pixels[i] = new Color32(toHighlight.r, toHighlight.g, toHighlight.b, p.a);
                else if (SameRgb(p, fromFill))
                    pixels[i] = new Color32(toFill.r, toFill.g, toFill.b, p.a);
            }

            var result = NewTexture(source.width, source.height);
            result.SetPixels32(pixels);
            return result;
        }

        private static bool SameRgb(Color32 a, Color32 b) => a.r == b.r && a.g == b.g && a.b == b.b;

        private static Color32 Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }

        private static Texture2D NewTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(new Color32[width * height]);
            return texture;
        }

        // Đọc thẳng byte của PNG nên không cần bật Read/Write cho asset gốc.
        private static Texture2D LoadPng(string path, List<string> errors)
        {
            if (!File.Exists(path))
            {
                errors.Add($"Không thấy {path}.");
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(File.ReadAllBytes(path)))
            {
                errors.Add($"Không đọc được {path}.");
                Object.DestroyImmediate(texture);
                return null;
            }
            return texture;
        }

        private static void Blit(Texture2D source, Texture2D target, int x, int y)
        {
            target.SetPixels32(x, y, source.width, source.height, source.GetPixels32());
        }

        // border: trái, dưới, phải, trên (pixel). Hủy texture sau khi ghi.
        private static void Save(Texture2D texture, string path, Vector4 border)
        {
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteBorder = border;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
