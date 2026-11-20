using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using VeQue.View.Data;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Câu cá: import sprite cá (duyệt 13/11/2026).
    // 1. Icon cá (Assets/_Game/Art/Fishing/fish/fish_NN_<id>.png, bản HD 32×32 từ 20/11/2026): Single, PPU 32, Point, không nén, tắt mipmap;
    //    gán vào FishDefinition theo id (cá chưa có trong data thì bỏ qua). Từ 20/11/2026 menu Tạo data câu cá cũng tự gán.
    // 2. Bóng cá: chép dải "small fish" / "mediuml fish" (gói Ocean Pack, sẽ xóa cùng Sprout Lands) sang
    //    Assets/_Game/Art/Fishing/shadow/, cắt 4 khung theo hướng (Lên, Xuống, Trái, Phải), bó sát hình, pivot giữa.
    // 3. (14/11/2026) Icon cần rods/rod_NN_<id>.png và mồi bait/bait_NN_<tên>.png: Single như icon cá
    //    (meta mặc định chép theo preset là Multiple, sprite "Piknik basket_0"). Gán vào data: menu Về Quê → Data → Tạo data câu cá.
    public static class FishingSpriteImporter
    {
        public const string IconFolder = FishingPlaceholderArt.Folder + "/fish";
        public const string RodFolder = FishingPlaceholderArt.Folder + "/rods";
        public const string BaitFolder = FishingPlaceholderArt.Folder + "/bait";
        public const string ShadowFolder = FishingPlaceholderArt.Folder + "/shadow";
        public const string SmallShadowPath = ShadowFolder + "/fish_shadow_small.png";
        public const string MediumShadowPath = ShadowFolder + "/fish_shadow_medium.png";

        private const string OceanPack = "Assets/Art/Sprout Sorry pack/Early Access/Ocean Pack";
        private const int Cell = 16;
        private const int PixelsPerUnit = 16;
        // (20/11/2026) Icon cá / cần / mồi đổi sang bản HD 32×32: PPU 32 để giữ nguyên kích thước hiển thị (1 unit). Bóng cá vẫn 16.
        private const int IconPixelsPerUnit = 32;

        // Tên file khác id trong data.
        private static readonly Dictionary<string, string> IdAliases = new Dictionary<string, string>
        {
            { "tom_cang_xanh", "tom_cang" },
        };

        // Khung trong dải 15 khung theo thứ tự Lên, Xuống, Trái, Phải (enum Arrow).
        private static readonly int[] SmallFrames = { 2, 9, 14, 5 };
        private static readonly int[] MediumFrames = { 3, 10, 14, 7 };
        public static readonly string[] DirectionNames = { "up", "down", "left", "right" };

        [MenuItem("Về Quê/Import art/Sprite cá", priority = 303)]
        private static void Run()
        {
            int icons = ImportIcons(out int assigned, out List<string> unused);
            int rodIcons = ImportItemIcons(RodFolder, "rod_");
            int baitIcons = ImportItemIcons(BaitFolder, "bait_");
            bool small = ImportShadow($"{OceanPack}/small fish.png", SmallShadowPath, SmallFrames);
            bool medium = ImportShadow($"{OceanPack}/mediuml fish.png", MediumShadowPath, MediumFrames);
            AssetDatabase.SaveAssets();

            string message = $"Icon: import {icons} file, gán cho {assigned} cá.";
            if (unused.Count > 0)
                message += $"\nChưa có trong data (để sau): {string.Join(", ", unused)}.";
            message += $"\nIcon cần: {rodIcons}, mồi: {baitIcons} (gán bằng Tạo data câu cá).";
            message += small && medium
                ? $"\nBóng cá: đã tạo {ShadowFolder}. Chạy Tạo UI để gắn vào scene."
                : $"\nBóng cá: không thấy dải gốc trong {OceanPack}.";
            Debug.Log("Về Quê: " + message);
            EditorUtility.DisplayDialog("Sprite cá", message, "OK");
        }

        private static int ImportIcons(out int assigned, out List<string> unused)
        {
            assigned = 0;
            unused = new List<string>();
            var byId = new Dictionary<string, Sprite>();
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { IconFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string id = IdFromFile(Path.GetFileNameWithoutExtension(path));
                if (id == null)
                    continue;
                ConfigureSingle(path);
                count++;
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                    byId[id] = sprite;
            }

            var fishing = AssetDatabase.LoadAssetAtPath<FishingDefinition>(FishingDataGenerator.FishingPath);
            var known = new HashSet<string>();
            if (fishing != null)
            {
                foreach (FishDefinition fish in fishing.Fish)
                {
                    if (fish == null)
                        continue;
                    known.Add(fish.Id);
                    if (!byId.TryGetValue(fish.Id, out Sprite sprite))
                        continue;
                    var so = new SerializedObject(fish);
                    so.FindProperty("_icon").objectReferenceValue = sprite;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    assigned++;
                }
            }
            foreach (string id in byId.Keys)
            {
                if (!known.Contains(id))
                    unused.Add(id);
            }
            unused.Sort();
            return count;
        }

        // "fish_13_tom_cang_xanh" → "tom_cang".
        public static string IdFromFile(string name)
        {
            if (!name.StartsWith("fish_") || name.Length < 9 || name[7] != '_')
                return null;
            string id = name.Substring(8);
            return IdAliases.TryGetValue(id, out string alias) ? alias : id;
        }

        // Icon cần / mồi "<prefix>NN_*.png" trong folder → Single. Trả số file.
        private static int ImportItemIcons(string folder, string prefix)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                return 0;
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                if (!name.StartsWith(prefix) || name.Length < prefix.Length + 3 || name[prefix.Length + 2] != '_')
                    continue;
                ConfigureSingle(path);
                count++;
            }
            return count;
        }

        // Đưa icon về Single + thiết lập pixel nếu chưa (không reimport khi đã đúng). false nếu không có file.
        public static bool EnsureSingleIcon(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                return false;
            bool ok = importer.textureType == TextureImporterType.Sprite
                && importer.spriteImportMode == SpriteImportMode.Single
                && Mathf.Approximately(importer.spritePixelsPerUnit, IconPixelsPerUnit)
                && importer.filterMode == FilterMode.Point
                && importer.textureCompression == TextureImporterCompression.Uncompressed
                && !importer.mipmapEnabled;
            if (!ok)
                ConfigureSingle(path);
            return true;
        }

        private static void ConfigureSingle(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            ApplyPixelSettings(importer, IconPixelsPerUnit);
            importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static bool ImportShadow(string source, string target, int[] frames)
        {
            if (!File.Exists(source))
                return false;
            if (!AssetDatabase.IsValidFolder(ShadowFolder))
                AssetDatabase.CreateFolder(FishingPlaceholderArt.Folder, "shadow");

            byte[] bytes = File.ReadAllBytes(source);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(bytes);
            int width = texture.width;
            int height = texture.height;
            Color32[] pixels = texture.GetPixels32();
            Object.DestroyImmediate(texture);

            File.WriteAllBytes(target, bytes);
            AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(target);
            ApplyPixelSettings(importer);
            importer.spriteImportMode = SpriteImportMode.Multiple;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var oldIds = new Dictionary<string, GUID>();
            foreach (SpriteRect old in provider.GetSpriteRects())
                oldIds[old.name] = old.spriteID;

            string baseName = Path.GetFileNameWithoutExtension(target);
            var rects = new SpriteRect[frames.Length];
            var pairs = new List<SpriteNameFileIdPair>(frames.Length);
            for (int i = 0; i < frames.Length; i++)
            {
                string name = $"{baseName}_{DirectionNames[i]}";
                rects[i] = new SpriteRect
                {
                    name = name,
                    rect = TightRect(pixels, width, height, frames[i]),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = oldIds.TryGetValue(name, out GUID id) ? id : GUID.Generate(),
                };
                pairs.Add(new SpriteNameFileIdPair(name, rects[i].spriteID));
            }
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(pairs);
            provider.Apply();
            importer.SaveAndReimport();
            return true;
        }

        // Hình chữ nhật bó sát pixel có màu trong khung thứ frame (dải ngang, ô 16×16).
        private static Rect TightRect(Color32[] pixels, int width, int height, int frame)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
            int x0 = frame * Cell;
            for (int y = 0; y < Mathf.Min(Cell, height); y++)
            {
                for (int x = x0; x < x0 + Cell && x < width; x++)
                {
                    if (pixels[y * width + x].a == 0)
                        continue;
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }
            if (maxX < 0)
                return new Rect(x0, 0, Cell, Cell);
            return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        // Sprite đã cắt của một dải bóng cá theo thứ tự Lên, Xuống, Trái, Phải (null nếu chưa import).
        public static Sprite[] LoadShadowSprites(string path)
        {
            var result = new Sprite[DirectionNames.Length];
            string baseName = Path.GetFileNameWithoutExtension(path);
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (!(asset is Sprite sprite))
                    continue;
                for (int i = 0; i < DirectionNames.Length; i++)
                {
                    if (sprite.name == $"{baseName}_{DirectionNames[i]}")
                        result[i] = sprite;
                }
            }
            return result;
        }

        private static void ApplyPixelSettings(TextureImporter importer, int pixelsPerUnit = PixelsPerUnit)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
