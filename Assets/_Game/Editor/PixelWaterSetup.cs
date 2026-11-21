using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Dựng scene → Nước pixel (vùng câu).
    // 1. Đặt import cho 2 texture của shader: Caustic (Point) và Noise (Bilinear); cả hai Repeat, không nén, không mipmap, không sRGB.
    // 2. Bake bản đồ khoảng cách tới bờ từ Tilemap "FishingWater" (8 texel / ô, R8, ~15 KB) → PixelWaterShore_Fishing.asset.
    //    Bờ = ô không có nước nhưng có tile ở Tilemap khác cùng Grid (cỏ, đất, bến). Ô trống (ngoài map) coi như nước
    //    → mép map không hiện bọt.
    // 3. Tạo / cập nhật PixelWater.mat (shader VeQue/PixelWater), gán shore map + rect, gán cho TilemapRenderer FishingWater.
    // FishingRegionBuilder gọi ApplyTo() sau khi vẽ hồ, nên dựng lại vùng câu vẫn giữ nước pixel. Đổi hình hồ → chạy lại menu này.
    public static class PixelWaterSetup
    {
        private const string Title = "Nước pixel";
        private const string ShaderName = "VeQue/PixelWater";
        private const string CausticPath = "Assets/_Game/Art/PixelWaterCaustic.png";
        private const string NoisePath = "Assets/_Game/Art/PixelWaterNoise.png";
        private const string ShorePath = "Assets/_Game/Art/PixelWaterShore_Fishing.asset";
        private const string MaterialPath = "Assets/_Game/Art/PixelWater.mat";
        private const string WaterTilemapName = "FishingWater";

        private const int TexelsPerCell = 8;
        private const float MaxDist = 5f;   // ô; khớp _ShoreMaxDist trong material
        private const int Margin = 1;       // ô viền quanh hồ trong shore map

        [MenuItem("Về Quê/Dựng scene/Nước pixel (vùng câu)", priority = 105)]
        private static void Setup()
        {
            Tilemap water = FindTilemap(WaterTilemapName);
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
                "Đã bake bờ hồ và gán PixelWater.mat cho FishingWater.\n"
                + "Bấm Play (hoặc bật Animated Materials trong Scene view) để xem; Ctrl+S để lưu scene.", "OK");
        }

        // Trả về false nếu thiếu shader / texture (đã báo lỗi).
        public static bool ApplyTo(Tilemap water)
        {
            Shader shader = Shader.Find(ShaderName);
            Texture2D caustic = EnsureImport(CausticPath, FilterMode.Point);
            Texture2D noise = EnsureImport(NoisePath, FilterMode.Bilinear);
            if (shader == null || caustic == null || noise == null)
            {
                EditorUtility.DisplayDialog(Title,
                    $"Thiếu shader {ShaderName} hoặc texture ({CausticPath}, {NoisePath}).", "OK");
                return false;
            }

            water.CompressBounds();
            Texture2D shore = BakeShore(water, out Vector4 rect);
            if (shore == null)
            {
                EditorUtility.DisplayDialog(Title, "FishingWater chưa có ô nước nào.", "OK");
                return false;
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

            // Chỉ gán texture + dữ liệu bake; các thông số màu / tốc độ giữ nguyên để chỉnh tay không bị ghi đè.
            mat.SetTexture("_CausticTex", caustic);
            mat.SetTexture("_NoiseTex", noise);
            mat.SetTexture("_ShoreTex", shore);
            mat.SetVector("_ShoreRect", rect);
            mat.SetFloat("_ShoreMaxDist", MaxDist);
            EditorUtility.SetDirty(mat);

            var renderer = water.GetComponent<TilemapRenderer>();
            Undo.RecordObject(renderer, Title);
            renderer.sharedMaterial = mat;
            EditorUtility.SetDirty(renderer);
            AssetDatabase.SaveAssets();
            return true;
        }

        // Mỗi texel lưu khoảng cách (ô) từ tâm texel tới ô bờ gần nhất, chia MaxDist. Ô bờ = 0.
        private static Texture2D BakeShore(Tilemap water, out Vector4 rect)
        {
            rect = Vector4.zero;
            BoundsInt b = water.cellBounds;
            if (b.size.x <= 0 || b.size.y <= 0)
                return null;

            int minX = b.xMin - Margin, minY = b.yMin - Margin;
            int cw = b.size.x + Margin * 2, ch = b.size.y + Margin * 2;
            int reach = Mathf.CeilToInt(MaxDist);

            // Lưới ô mở rộng thêm "reach" để tính bờ nằm ngoài vùng texture.
            int gx0 = minX - reach, gy0 = minY - reach, gw = cw + reach * 2, gh = ch + reach * 2;
            var land = new bool[gw, gh];
            List<Tilemap> others = OtherTilemaps(water);
            for (int x = 0; x < gw; x++)
            {
                for (int y = 0; y < gh; y++)
                {
                    var cell = new Vector3Int(gx0 + x, gy0 + y, 0);
                    if (water.HasTile(cell))
                        continue;
                    foreach (Tilemap tm in others)
                    {
                        if (tm.HasTile(cell)) { land[x, y] = true; break; }
                    }
                }
            }

            int tw = cw * TexelsPerCell, th = ch * TexelsPerCell;
            var pixels = new byte[tw * th];
            for (int ty = 0; ty < th; ty++)
            {
                for (int tx = 0; tx < tw; tx++)
                {
                    // Tâm texel, đơn vị ô, so với gốc lưới mở rộng.
                    float wx = reach + (tx + 0.5f) / TexelsPerCell;
                    float wy = reach + (ty + 0.5f) / TexelsPerCell;
                    int cx = Mathf.FloorToInt(wx), cy = Mathf.FloorToInt(wy);
                    float best = MaxDist;
                    for (int x = cx - reach; x <= cx + reach; x++)
                    {
                        for (int y = cy - reach; y <= cy + reach; y++)
                        {
                            if (x < 0 || y < 0 || x >= gw || y >= gh || !land[x, y])
                                continue;
                            float dx = Mathf.Max(Mathf.Max(x - wx, wx - (x + 1)), 0f);
                            float dy = Mathf.Max(Mathf.Max(y - wy, wy - (y + 1)), 0f);
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            if (d < best) best = d;
                        }
                    }
                    pixels[ty * tw + tx] = (byte)Mathf.RoundToInt(Mathf.Clamp01(best / MaxDist) * 255f);
                }
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ShorePath);
            if (tex == null || tex.width != tw || tex.height != th || tex.format != TextureFormat.R8)
            {
                if (tex != null)
                    AssetDatabase.DeleteAsset(ShorePath);
                tex = new Texture2D(tw, th, TextureFormat.R8, false, true) { name = "PixelWaterShore_Fishing" };
                tex.SetPixelData(pixels, 0);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                tex.Apply(false, false);
                AssetDatabase.CreateAsset(tex, ShorePath);
            }
            else
            {
                tex.SetPixelData(pixels, 0);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                tex.Apply(false, false);
                EditorUtility.SetDirty(tex);
            }

            Vector3 origin = water.CellToWorld(new Vector3Int(minX, minY, 0));
            Vector3 cellSize = water.layoutGrid.cellSize;
            rect = new Vector4(origin.x, origin.y, cw * cellSize.x, ch * cellSize.y);
            return tex;
        }

        private static List<Tilemap> OtherTilemaps(Tilemap water)
        {
            var list = new List<Tilemap>();
            Grid grid = water.layoutGrid;
            if (grid == null)
                return list;
            foreach (Tilemap tm in grid.GetComponentsInChildren<Tilemap>(true))
            {
                if (tm != water)
                    list.Add(tm);
            }
            return list;
        }

        private static Texture2D EnsureImport(string path, FilterMode filter)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return null;

            bool changed = importer.textureType != TextureImporterType.Default
                           || importer.wrapMode != TextureWrapMode.Repeat
                           || importer.filterMode != filter
                           || importer.mipmapEnabled
                           || importer.sRGBTexture
                           || importer.alphaSource != TextureImporterAlphaSource.None
                           || importer.textureCompression != TextureImporterCompression.Uncompressed
                           || importer.npotScale != TextureImporterNPOTScale.None;
            if (changed)
            {
                importer.textureType = TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = filter;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = false;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Tilemap FindTilemap(string name)
        {
            foreach (Tilemap tm in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                if (tm.name == name)
                    return tm;
            }
            return null;
        }
    }
}
