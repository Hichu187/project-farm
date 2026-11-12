using System.IO;
using UnityEditor;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.EditorTools
{
    // Art tạm cho câu cá (gói Sunnyside chưa có cá, phao): phao 8×8, dấu "!" 5×8 và icon cá 16×16 tô màu theo độ hiếm
    // → Assets/_Game/Art/Fishing/. Gán icon cho cá chưa có icon (hoặc đang dùng icon tạm). Thay bằng art thật sau.
    public static class FishingPlaceholderArt
    {
        public const string Folder = "Assets/_Game/Art/Fishing";
        public const string BobberPath = Folder + "/bobber.png";

        // '.' trong suốt, 'o' viền, 'b' thân, 'l' bụng, 'e' mắt. Hàng đầu = trên cùng. Cá quay đầu sang trái.
        private static readonly string[] FishPattern =
        {
            "................",
            "................",
            "................",
            "................",
            "......ooo.......",
            "....oobbboo..oo.",
            "...obbbbbbbo.obo",
            "..obebbbbbbboobo",
            "..obbbbbbbbbbbo.",
            "...olllllllloobo",
            "....oollllo..obo",
            "......ooo.....oo",
            "................",
            "................",
            "................",
            "................",
        };

        // 'r' đỏ, 'w' trắng. Đáy phao (chỗ chạm nước) ở hàng 'w'.
        private static readonly string[] BobberPattern =
        {
            "...o....",
            "..oro...",
            ".orrro..",
            ".orrro..",
            ".owwwo..",
            "..owo...",
            "...o....",
            "........",
        };

        // Dấu "!" khi cá cắn: 'y' vàng, 'o' viền. Pivot ở đáy.
        private static readonly string[] AlertPattern =
        {
            ".ooo.",
            ".oyo.",
            ".oyo.",
            ".oyo.",
            ".oyo.",
            ".ooo.",
            ".oyo.",
            ".ooo.",
        };

        public const string AlertPath = Folder + "/alert.png";

        private static readonly Color32 Yellow = new Color32(0xf8, 0xd8, 0x48, 0xff);
        private static readonly Color32 Outline = new Color32(0x2b, 0x2b, 0x3a, 0xff);
        private static readonly Color32 Eye = new Color32(0x10, 0x10, 0x18, 0xff);
        private static readonly Color32 Red = new Color32(0xd8, 0x44, 0x40, 0xff);
        private static readonly Color32 White = new Color32(0xf4, 0xf0, 0xe8, 0xff);

        // Thường, Khá, Hiếm, Quý, Huyền thoại.
        private static readonly Color32[] RarityColors =
        {
            new Color32(0x8a, 0x9b, 0xb0, 0xff),
            new Color32(0x6c, 0xbf, 0x5a, 0xff),
            new Color32(0x4a, 0x8f, 0xe0, 0xff),
            new Color32(0xa6, 0x5a, 0xd8, 0xff),
            new Color32(0xf0, 0xc0, 0x40, 0xff),
        };

        private static readonly string[] RarityNames = { "common", "uncommon", "rare", "epic", "legendary" };

        [MenuItem("Về Quê/Câu cá: tạo art tạm")]
        private static void Generate()
        {
            EnsureFolder(Folder);
            Save(Draw(BobberPattern, c => c == 'r' ? Red : c == 'w' ? White : Outline), BobberPath, new Vector2(0.5f, 0.25f));
            Save(Draw(AlertPattern, c => c == 'y' ? Yellow : Outline), AlertPath, new Vector2(0.5f, 0f));

            var icons = new Sprite[RarityColors.Length];
            for (int r = 0; r < RarityColors.Length; r++)
            {
                Color32 body = RarityColors[r];
                Color32 belly = Color32.Lerp(body, White, 0.55f);
                string path = $"{Folder}/fish_{RarityNames[r]}.png";
                Save(Draw(FishPattern, c => c == 'b' ? body : c == 'l' ? belly : c == 'e' ? Eye : Outline), path, new Vector2(0.5f, 0.5f));
                icons[r] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            int assigned = 0;
            var fishing = AssetDatabase.LoadAssetAtPath<FishingDefinition>(FishingDataGenerator.FishingPath);
            if (fishing != null)
            {
                foreach (FishDefinition fish in fishing.Fish)
                {
                    if (fish == null)
                        continue;
                    var so = new SerializedObject(fish);
                    SerializedProperty icon = so.FindProperty("_icon");
                    // Chỉ thay icon trống hoặc icon tạm; art thật đã gán thì giữ.
                    if (icon.objectReferenceValue != null
                        && !AssetDatabase.GetAssetPath(icon.objectReferenceValue).StartsWith(Folder + "/"))
                        continue;
                    icon.objectReferenceValue = icons[(int)fish.Rarity];
                    so.ApplyModifiedPropertiesWithoutUndo();
                    assigned++;
                }
                AssetDatabase.SaveAssets();
            }

            Debug.Log(fishing != null
                ? $"Về Quê: đã tạo art tạm câu cá, gán icon cho {assigned} cá → {Folder}"
                : $"Về Quê: đã tạo art tạm câu cá → {Folder}. Chưa có {FishingDataGenerator.FishingPath}: chạy Tạo data câu cá rồi chạy lại menu này để gán icon.");
        }

        private static Texture2D Draw(string[] pattern, System.Func<char, Color32> colorOf)
        {
            int height = pattern.Length;
            int width = pattern[0].Length;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var clear = new Color32(0, 0, 0, 0);
            for (int row = 0; row < height; row++)
            {
                for (int x = 0; x < width; x++)
                {
                    char c = pattern[row][x];
                    texture.SetPixel(x, height - 1 - row, c == '.' ? clear : (Color)colorOf(c));
                }
            }
            return texture;
        }

        private static void Save(Texture2D texture, string path, Vector2 pivot)
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
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
