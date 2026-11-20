using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Dựng scene → Nước pixel (vùng câu).
    // - Đặt import cho noise: Repeat, Point, không nén, không mipmap, không sRGB (dữ liệu, không phải màu).
    // - Tạo / cập nhật material PixelWater.mat (shader VeQue/PixelWater) và gán cho TilemapRenderer "FishingWater".
    // FishingRegionBuilder cũng gọi ApplyTo() nên dựng lại vùng câu vẫn giữ nước pixel.
    public static class PixelWaterSetup
    {
        private const string Title = "Nước pixel";
        private const string ShaderName = "VeQue/PixelWater";
        private const string NoisePath = "Assets/_Game/Art/PixelWaterNoise.png";
        private const string MaterialPath = "Assets/_Game/Art/PixelWater.mat";
        private const string WaterTilemapName = "FishingWater";

        [MenuItem("Về Quê/Dựng scene/Nước pixel (vùng câu)", priority = 105)]
        private static void Setup()
        {
            Tilemap water = FindWaterTilemap();
            if (water == null)
            {
                EditorUtility.DisplayDialog(Title,
                    "Không thấy Tilemap \"FishingWater\". Chạy Về Quê → Dựng scene → Vùng câu (tạm) trước.", "OK");
                return;
            }

            if (!ApplyTo(water))
                return;

            EditorSceneManager.MarkSceneDirty(water.gameObject.scene);
            Selection.activeGameObject = water.gameObject;
            EditorUtility.DisplayDialog(Title,
                "Đã gán PixelWater.mat cho FishingWater. Bấm Play hoặc bật Animated Materials trong Scene view để xem; Ctrl+S để lưu scene.", "OK");
        }

        // Trả về false nếu thiếu shader / noise (đã báo lỗi).
        public static bool ApplyTo(Tilemap water)
        {
            Material mat = EnsureMaterial();
            if (mat == null)
                return false;

            var renderer = water.GetComponent<TilemapRenderer>();
            Undo.RecordObject(renderer, Title);
            renderer.sharedMaterial = mat;
            EditorUtility.SetDirty(renderer);
            return true;
        }

        private static Material EnsureMaterial()
        {
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog(Title, $"Không tìm thấy shader {ShaderName} (Assets/_Game/Art/Shaders/PixelWater.shader).", "OK");
                return null;
            }

            Texture2D noise = EnsureNoiseImport();
            if (noise == null)
            {
                EditorUtility.DisplayDialog(Title, $"Không tìm thấy {NoisePath}.", "OK");
                return null;
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                mat = new Material(shader) { name = "PixelWater" };
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            // Chỉ gán noise; các thông số khác giữ nguyên để chỉnh tay trong Inspector không bị ghi đè.
            mat.SetTexture("_NoiseTex", noise);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        private static Texture2D EnsureNoiseImport()
        {
            var importer = AssetImporter.GetAtPath(NoisePath) as TextureImporter;
            if (importer == null)
                return null;

            bool changed = importer.textureType != TextureImporterType.Default
                           || importer.wrapMode != TextureWrapMode.Repeat
                           || importer.filterMode != FilterMode.Point
                           || importer.mipmapEnabled
                           || importer.sRGBTexture
                           || importer.textureCompression != TextureImporterCompression.Uncompressed
                           || importer.npotScale != TextureImporterNPOTScale.None;
            if (changed)
            {
                importer.textureType = TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = false;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(NoisePath);
        }

        private static Tilemap FindWaterTilemap()
        {
            foreach (Tilemap tm in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                if (tm.name == WaterTilemapName)
                    return tm;
            }
            return null;
        }
    }
}
