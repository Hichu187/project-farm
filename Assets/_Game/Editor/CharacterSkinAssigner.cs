using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using VeQue.View.Character;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Nhân vật Sunnyside: import + gán sprite.
    // 1. Đặt import cho các strip nhân vật Sunnyside dùng tới (PPU 16, Point, không nén, tắt mipmap) và cắt
    //    khung 96×64, pivot (0.5, 25/64) = chân nhân vật. Chạy lại giữ nguyên ID sprite (không mất tham chiếu).
    // 2. Tạo (nếu chưa có) Assets/_Game/Data/CharacterSkin.asset: thân + dụng cụ cho 4 hoạt ảnh, 6 kiểu tóc
    //    (đo khoảng độ sáng của tóc để shader phủ màu), material Assets/_Game/Art/HairTint.mat.
    // Bảng màu tóc giữ nguyên phần đã chỉnh trong Inspector.
    public static class CharacterSkinAssigner
    {
        private const string HumanFolder = "Assets/Art/Sunnyside_World_ASSET_PACK_V2.1/Sunnyside_World_Assets/Characters/Human/";
        private const string SkinPath = DefaultDataGenerator.DataFolder + "/CharacterSkin.asset";
        private const string HairMaterialPath = "Assets/_Game/Art/HairTint.mat";
        private const string HairShader = "VeQue/HairTint";
        private const int FrameWidth = 96;
        private const int FrameHeight = 64;
        private const float PixelsPerUnit = 16f;
        // Chân nhân vật ở hàng pixel thứ 25 tính từ đáy khung (đo trên mọi strip đang dùng).
        private static readonly Vector2 FeetPivot = new Vector2(0.5f, 25f / 64f);
        // Pixel tối hơn mức này là viền (giữ nguyên khi phủ màu); khớp _OutlineLum của shader.
        private const float OutlineLum = 0.12f;

        private readonly struct ClipSource
        {
            public readonly string Folder;
            public readonly string Strip;
            public readonly int Frames;
            public readonly string Field;
            public readonly string HairField;

            public ClipSource(string folder, string strip, int frames, string field, string hairField)
            {
                Folder = folder;
                Strip = strip;
                Frames = frames;
                Field = field;
                HairField = hairField;
            }
        }

        private static readonly ClipSource[] Clips =
        {
            new ClipSource("IDLE", "idle_strip9", 9, "_idle", "Idle"),
            new ClipSource("WALKING", "walk_strip8", 8, "_walk", "Walk"),
            new ClipSource("DOING", "doing_strip8", 8, "_work", "Work"),
            new ClipSource("WATERING", "watering_strip5", 5, "_water", "Water"),
        };

        // Thứ tự = thứ tự hiện ở màn tạo nhân vật; kiểu đầu tiên là mặc định (CharacterLook.DefaultHairStyle).
        private static readonly (string Id, string Name)[] Hairs =
        {
            ("shorthair", "Tóc ngắn"),
            ("longhair", "Tóc dài"),
            ("curlyhair", "Tóc xoăn"),
            ("bowlhair", "Tóc bát úp"),
            ("mophair", "Tóc bù xù"),
            ("spikeyhair", "Tóc dựng"),
        };

        [MenuItem("Về Quê/Nhân vật Sunnyside: import + gán sprite")]
        private static void Run()
        {
            var prefixes = new List<string> { "base", "tools" };
            foreach ((string id, _) in Hairs)
                prefixes.Add(id);

            var errors = new List<string>();
            foreach (ClipSource clip in Clips)
            {
                foreach (string prefix in prefixes)
                {
                    string path = StripPath(clip, prefix);
                    if (!(AssetImporter.GetAtPath(path) is TextureImporter))
                        errors.Add($"Không thấy {path}.");
                }
            }
            var shader = Shader.Find(HairShader);
            if (shader == null)
                errors.Add($"Không thấy shader {HairShader}.");
            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog("Nhân vật Sunnyside", "Chưa làm gì vì:\n" + string.Join("\n", errors), "OK");
                return;
            }

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var frames = new Dictionary<string, Sprite[]>();
            try
            {
                int done = 0;
                int total = Clips.Length * prefixes.Count;
                foreach (ClipSource clip in Clips)
                {
                    foreach (string prefix in prefixes)
                    {
                        string path = StripPath(clip, prefix);
                        EditorUtility.DisplayProgressBar("Nhân vật Sunnyside", path, (float)done++ / total);
                        ImportStrip(factory, path, clip.Frames);
                        Sprite[] sprites = LoadFrames(path);
                        if (sprites.Length != clip.Frames)
                            errors.Add($"{path}: có {sprites.Length} khung, cần {clip.Frames}.");
                        frames[path] = sprites;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog("Nhân vật Sunnyside", "Đã import nhưng chưa gán vì:\n" + string.Join("\n", errors), "OK");
                return;
            }

            Material material = EnsureMaterial(shader);
            var skin = AssetDatabase.LoadAssetAtPath<CharacterSkin>(SkinPath);
            bool created = skin == null;
            if (created)
            {
                skin = ScriptableObject.CreateInstance<CharacterSkin>();
                AssetDatabase.CreateAsset(skin, SkinPath);
            }

            var so = new SerializedObject(skin);
            foreach (ClipSource clip in Clips)
            {
                SetArray(so.FindProperty(clip.Field + ".Body"), frames[StripPath(clip, "base")]);
                SetArray(so.FindProperty(clip.Field + ".Tools"), frames[StripPath(clip, "tools")]);
            }

            SerializedProperty hairs = so.FindProperty("_hairs");
            hairs.arraySize = Hairs.Length;
            var summary = new List<string>();
            for (int i = 0; i < Hairs.Length; i++)
            {
                (string id, string name) = Hairs[i];
                SerializedProperty hair = hairs.GetArrayElementAtIndex(i);
                hair.FindPropertyRelative("Id").stringValue = id;
                hair.FindPropertyRelative("DisplayName").stringValue = name;
                foreach (ClipSource clip in Clips)
                    SetArray(hair.FindPropertyRelative(clip.HairField), frames[StripPath(clip, id)]);

                MeasureHair(StripPath(Clips[0], id), out float min, out float max);
                hair.FindPropertyRelative("MinLum").floatValue = min;
                hair.FindPropertyRelative("MaxLum").floatValue = max;
                summary.Add($"{name}: {min:0.00}–{max:0.00}");
            }
            so.FindProperty("_hairMaterial").objectReferenceValue = material;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skin);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(skin);

            EditorUtility.DisplayDialog("Nhân vật Sunnyside",
                (created ? "Đã tạo " : "Đã cập nhật ") + SkinPath + ": đứng, đi, làm việc, tưới; 6 kiểu tóc.\n"
                + "Độ sáng tóc: " + string.Join("; ", summary), "OK");
        }

        private static string StripPath(ClipSource clip, string prefix) => $"{HumanFolder}{clip.Folder}/{prefix}_{clip.Strip}.png";

        private static Material EnsureMaterial(Shader shader)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(HairMaterialPath);
            if (material != null)
            {
                if (material.shader != shader)
                    material.shader = shader;
                return material;
            }

            material = new Material(shader) { name = "HairTint" };
            AssetDatabase.CreateAsset(material, HairMaterialPath);
            return material;
        }

        private static void SetArray(SerializedProperty array, Sprite[] sprites)
        {
            array.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        // Độ sáng (gamma, như shader) tối nhất / sáng nhất của tóc, bỏ viền và pixel trong suốt.
        private static void MeasureHair(string path, out float min, out float max)
        {
            min = 1f;
            max = 0f;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                    return;
                foreach (Color32 c in texture.GetPixels32())
                {
                    if (c.a == 0)
                        continue;
                    float lum = (0.299f * c.r + 0.587f * c.g + 0.114f * c.b) / 255f;
                    if (lum <= OutlineLum)
                        continue;
                    min = Mathf.Min(min, lum);
                    max = Mathf.Max(max, lum);
                }
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }

            if (max <= min)
            {
                min = 0f;
                max = 1f;
            }
        }

        private static void ImportStrip(SpriteDataProviderFactories factory, string path, int frameCount)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] old = provider.GetSpriteRects();

            string baseName = Path.GetFileNameWithoutExtension(path);
            var rects = new SpriteRect[frameCount];
            var pairs = new List<SpriteNameFileIdPair>(frameCount);
            for (int i = 0; i < frameCount; i++)
            {
                var rect = new SpriteRect
                {
                    name = $"{baseName}_{i}",
                    rect = new Rect(i * FrameWidth, 0f, FrameWidth, FrameHeight),
                    alignment = SpriteAlignment.Custom,
                    pivot = FeetPivot,
                    // Giữ ID cũ để scene / asset đang tham chiếu không bị mất.
                    spriteID = i < old.Length ? old[i].spriteID : GUID.Generate(),
                };
                rects[i] = rect;
                pairs.Add(new SpriteNameFileIdPair(rect.name, rect.spriteID));
            }

            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(pairs);
            provider.Apply();
            importer.SaveAndReimport();
        }

        // Các khung theo thứ tự từ trái sang phải trên strip.
        private static Sprite[] LoadFrames(string path)
        {
            var sprites = new List<Sprite>();
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                    sprites.Add(sprite);
            }
            sprites.Sort((a, b) => a.rect.x.CompareTo(b.rect.x));
            return sprites.ToArray();
        }
    }
}
