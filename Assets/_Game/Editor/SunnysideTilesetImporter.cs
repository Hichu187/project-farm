using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Sunnyside: import tileset 16px.
    // Đặt import (PPU 16, Point, không nén, tắt mipmap) và cắt lưới 16×16, bỏ ô trống.
    // Sprite đặt tên theo tọa độ trên sheet: ss_x_y (x đếm từ trái, y đếm từ trên, bắt đầu từ 0) để hướng dẫn chỉ đúng tile.
    // Chạy lại giữ nguyên ID sprite theo tên (tile / scene đang dùng không mất tham chiếu).
    public static class SunnysideTilesetImporter
    {
        public const string TilesetPath =
            "Assets/Art/Sunnyside_World_ASSET_PACK_V2.1/Sunnyside_World_Assets/Tileset/spr_tileset_sunnysideworld_16px.png";
        private const int Cell = 16;

        [MenuItem("Về Quê/Import art/Tileset Sunnyside 16px", priority = 300)]
        private static void Run()
        {
            var importer = AssetImporter.GetAtPath(TilesetPath) as TextureImporter;
            if (importer == null)
            {
                EditorUtility.DisplayDialog("Tileset Sunnyside", $"Không thấy {TilesetPath}.", "OK");
                return;
            }

            // Đọc ảnh gốc để biết ô nào trống (không phụ thuộc cài đặt import hiện tại).
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(System.IO.File.ReadAllBytes(TilesetPath)))
            {
                Object.DestroyImmediate(texture);
                EditorUtility.DisplayDialog("Tileset Sunnyside", "Không đọc được ảnh tileset.", "OK");
                return;
            }

            int columns = texture.width / Cell;
            int rows = texture.height / Cell;
            int width = texture.width;
            Color32[] pixels = texture.GetPixels32();
            Object.DestroyImmediate(texture);

            var cells = new List<Vector2Int>();
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (HasPixels(pixels, width, x, y, rows))
                        cells.Add(new Vector2Int(x, y));
                }
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = Cell;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            var oldIds = new Dictionary<string, GUID>();
            foreach (SpriteRect old in provider.GetSpriteRects())
                oldIds[old.name] = old.spriteID;

            var rects = new SpriteRect[cells.Count];
            var pairs = new List<SpriteNameFileIdPair>(cells.Count);
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                string name = $"ss_{cell.x}_{cell.y}";
                var rect = new SpriteRect
                {
                    name = name,
                    // Rect của sprite tính từ góc dưới-trái của ảnh.
                    rect = new Rect(cell.x * Cell, (rows - 1 - cell.y) * Cell, Cell, Cell),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = oldIds.TryGetValue(name, out GUID id) ? id : GUID.Generate(),
                };
                rects[i] = rect;
                pairs.Add(new SpriteNameFileIdPair(name, rect.spriteID));
            }

            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(pairs);
            provider.Apply();
            importer.SaveAndReimport();

            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(TilesetPath));
            EditorUtility.DisplayDialog("Tileset Sunnyside",
                $"Đã cắt {cells.Count} tile 16×16 (bỏ ô trống), tên ss_x_y theo tọa độ trên sheet.", "OK");
        }

        // y đếm từ trên; mảng pixel của Texture2D đếm từ dưới.
        private static bool HasPixels(Color32[] pixels, int width, int x, int y, int rows)
        {
            int px = x * Cell;
            int py = (rows - 1 - y) * Cell;
            for (int j = 0; j < Cell; j++)
            {
                int row = (py + j) * width;
                for (int i = 0; i < Cell; i++)
                {
                    if (pixels[row + px + i].a > 0)
                        return true;
                }
            }
            return false;
        }
    }
}
