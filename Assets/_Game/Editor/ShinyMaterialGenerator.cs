using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Fishing;
using VeQue.View.Map;

namespace VeQue.EditorTools
{
    // Cá shiny (Đợt 2 Phần 18): tạo 8 material dùng shader VeQue/ShinyIcon theo bảng màu Docs/thiet-ke-shiny-ca.md mục 2,
    // asset ShinyPalette và gán vào FishingDefinition. Chạy lại thì ghi đè số (giữ GUID asset).
    public static class ShinyMaterialGenerator
    {
        public const string MaterialFolder = "Assets/_Game/Art/Shiny";
        public const string PalettePath = DefaultDataGenerator.DataFolder + "/Fishing/ShinyPalette.asset";
        private const string ShaderName = "VeQue/ShinyIcon";

        private sealed class Row
        {
            public ShinyKind Kind;
            public string Name;
            public string[] Tone;
            public string[] Sweep;
            public string Arm;
            public string Trail;
            public bool Aurora;
            public bool AuroraDark;
            public bool Stars;
        }

        private static readonly Row[] Rows =
        {
            new Row { Kind = ShinyKind.Gold, Name = "Hoàng kim", Tone = new[] { "b8701a", "ffbe2e", "ffe98a" },
                Sweep = new[] { "ffd04a", "ffffff", "fffbe0", "ffd04a" }, Arm = "ffd23a" },
            new Row { Kind = ShinyKind.Aurora, Name = "Cực quang", Tone = new[] { "c8fff0", "e0d0ff", "ffffff" },
                Sweep = new[] { "ffffff", "ffffff", "e0fff8", "b8f8ff" }, Arm = "a0fff0", Aurora = true },
            new Row { Kind = ShinyKind.Ice, Name = "Băng tinh", Tone = new[] { "3a8ac8", "8ad8f5", "e4fbff" },
                Sweep = new[] { "bff4ff", "ffffff", "ffffff", "bff4ff" }, Arm = "7ae0ff" },
            new Row { Kind = ShinyKind.Nebula, Name = "Tinh vân", Tone = new[] { "2a1e5a", "5a3aa8", "9a7ae0" },
                Sweep = new[] { "ff6ad8", "ffb0f0", "a0f0ff", "6ad8ff" }, Arm = "ff8ae0", Stars = true },
            new Row { Kind = ShinyKind.Ruby, Name = "Hồng ngọc", Tone = new[] { "8a1030", "e0283c", "ff8a6a" },
                Sweep = new[] { "ff9a3a", "ffe45a", "fff8c0", "ff9a3a" }, Arm = "ffb03a" },
            new Row { Kind = ShinyKind.Onyx, Name = "Hư không", Tone = new[] { "1c1a24", "34303f", "56526a" },
                Sweep = new[] { "5a5a78", "d8d8f0", "ffffff", "7a7a9a" }, Arm = "c8c8e8" },
            new Row { Kind = ShinyKind.Silver, Name = "Bạc", Tone = new[] { "6e778c", "b8c2d2", "f0f4fa" },
                Sweep = new[] { "ffffff", "ffffff", "dce6ff", "a0aec8" }, Arm = "dce6ff", Trail = "4e5670" },
            new Row { Kind = ShinyKind.DarkAurora, Name = "Tinh dạ", Tone = new[] { "16141e", "2a2638", "46405c" },
                Sweep = new[] { "ff6ad8", "ffb0f0", "a0f0ff", "6ad8ff" }, Arm = "a0fff0", AuroraDark = true, Stars = true },
        };

        [MenuItem("Về Quê/Cá shiny/Tạo material", priority = 400)]
        public static void Generate()
        {
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog("Cá shiny", $"Không tìm thấy shader {ShaderName}.", "OK");
                return;
            }
            Directory.CreateDirectory(MaterialFolder);

            ShinyPalette palette = AssetDatabase.LoadAssetAtPath<ShinyPalette>(PalettePath);
            if (palette == null)
            {
                palette = ScriptableObject.CreateInstance<ShinyPalette>();
                AssetDatabase.CreateAsset(palette, PalettePath);
            }

            var so = new SerializedObject(palette);
            SerializedProperty entries = so.FindProperty("_entries");
            entries.arraySize = Rows.Length;
            for (int i = 0; i < Rows.Length; i++)
            {
                Row row = Rows[i];
                Material material = CreateMaterial(shader, row);
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_kind").intValue = (int)row.Kind;
                entry.FindPropertyRelative("_displayName").stringValue = row.Name;
                entry.FindPropertyRelative("_material").objectReferenceValue = material;
                entry.FindPropertyRelative("_sparkleColor").colorValue = Hex(row.Arm);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(palette);

            AssignToFishing(palette);
            AssetDatabase.SaveAssets();
            Debug.Log($"Cá shiny: đã tạo {Rows.Length} material trong {MaterialFolder} và {PalettePath}.");
        }

        // FishingDataGenerator gọi lại sau khi tạo data câu cá.
        public static void AssignToFishing(ShinyPalette palette = null)
        {
            palette ??= AssetDatabase.LoadAssetAtPath<ShinyPalette>(PalettePath);
            FishingDefinition fishing = AssetDatabase.LoadAssetAtPath<FishingDefinition>(FishingDataGenerator.FishingPath);
            if (palette == null || fishing == null)
                return;
            var so = new SerializedObject(fishing);
            so.FindProperty("_shinyPalette").objectReferenceValue = palette;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(fishing);
        }

        private static Material CreateMaterial(Shader shader, Row row)
        {
            string path = $"{MaterialFolder}/Shiny_{(int)row.Kind}_{row.Kind}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.name = $"Shiny_{row.Kind}";
            material.SetColor("_ToneDark", Hex(row.Tone[0]));
            material.SetColor("_ToneMid", Hex(row.Tone[1]));
            material.SetColor("_ToneLight", Hex(row.Tone[2]));
            for (int i = 0; i < 4; i++)
                material.SetColor($"_Sweep{i}", Hex(row.Sweep[i]));
            material.SetColor("_TrailColor", Hex(row.Trail ?? "000000"));
            material.SetFloat("_Trail", row.Trail != null ? 1f : 0f);
            material.SetFloat("_Aurora", row.Aurora ? 1f : 0f);
            material.SetFloat("_AuroraDark", row.AuroraDark ? 1f : 0f);
            material.SetFloat("_Stars", row.Stars ? 1f : 0f);
            material.SetFloat("_MixK", 0.7f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }

        // ---------- Xem thử ----------

        [MenuItem("Về Quê/Cá shiny/Xem thử (khi Play)", true)]
        private static bool CanPreview() => EditorApplication.isPlaying;

        [MenuItem("Về Quê/Cá shiny/Xem thử (khi Play)", priority = 402)]
        private static void Preview()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Cá shiny", "Bấm Play rồi chạy lại menu này.", "OK");
                return;
            }
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            FishingDefinition fishing = game != null && game.Definition != null ? game.Definition.Fishing : null;
            if (fishing == null || fishing.ShinyPalette == null)
            {
                EditorUtility.DisplayDialog("Cá shiny",
                    "Chưa có ShinyPalette trong FishingDefinition. Chạy menu Về Quê → Cá shiny → Tạo material (ngoài Play) trước.", "OK");
                return;
            }
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PixelFontBuilder.AssetPath);
            ShinyPreview.Toggle(fishing, font);
        }
    }
}
