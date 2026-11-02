using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using VeQue.View.Data;

namespace VeQue.EditorTools
{
    // Vẽ từng lớp Tilemap theo MapPaintSettings. Mỗi lần vẽ/xóa hoàn tác được bằng Ctrl+Z.
    // Ground và Plots được xóa sạch trước khi vẽ; hàng rào chỉ xóa các ô đang dùng Fence Tiles để giữ phần vẽ tay trên Decor.
    public static class MapPainter
    {
        // Các hàm Validate trả về nội dung lỗi; null nếu hợp lệ.
        public static string ValidateLayer(Tilemap tilemap, string layerName)
        {
            return tilemap == null ? $"Chưa gán Tilemap {layerName}." : null;
        }

        public static string ValidateGround(MapPaintSettings settings, Tilemap tilemap)
        {
            string error = ValidateLayer(tilemap, "Ground") ?? ValidateMapSize(settings);
            if (error != null)
                return error;

            if (TotalWeight(settings.GrassTiles) <= 0)
                return "Cần ít nhất 1 tile cỏ có Weight > 0.";

            if (settings.PaintYard)
            {
                RectInt yard = settings.YardRect;
                if (yard.width < 1 || yard.height < 1 || !Contains(MapRect(settings), yard))
                    return "Sân đất phải có kích thước > 0 và nằm trong map.";
                if (!IsComplete(settings.YardTiles))
                    return "Sân đất cần đủ 4 góc và 4 cạnh.";
            }
            return null;
        }

        public static string ValidateFence(MapPaintSettings settings, Tilemap tilemap)
        {
            string error = ValidateLayer(tilemap, "Decor") ?? ValidateMapSize(settings);
            if (error != null)
                return error;

            RectInt fence = FenceRect(settings);
            if (fence.width < 2 || fence.height < 2)
                return "Offset quá lớn: hàng rào phải có kích thước từ 2×2.";
            if (!IsComplete(settings.FenceTiles))
                return "Hàng rào cần đủ 4 góc và 4 cạnh.";

            bool vertical = settings.GateEdge == MapPaintSettings.MapEdge.Left
                || settings.GateEdge == MapPaintSettings.MapEdge.Right;
            int edgeLength = vertical ? fence.height : fence.width;
            if (settings.GateStart + settings.GateSize > edgeLength)
                return "Cổng vượt quá chiều dài cạnh.";
            return null;
        }

        public static string ValidatePlots(MapPaintSettings settings, Tilemap tilemap)
        {
            string error = ValidateLayer(tilemap, "Plots") ?? ValidateMapSize(settings);
            if (error != null)
                return error;

            if (settings.PlotTile == null)
                return "Chưa gán tile ô đất.";
            if (settings.Farm == null)
                return "Chưa gán FarmDefinition.";

            RectInt map = MapRect(settings);
            foreach (ZoneDefinition zone in settings.Farm.Zones)
            {
                if (zone == null)
                    return "FarmDefinition có khu chưa gán.";
                if (zone.Beds.Count == 0)
                    return $"Khu '{zone.Id}' không có luống.";

                foreach (BedDefinition bed in zone.Beds)
                {
                    var rect = new RectInt(bed.Origin.x, bed.Origin.y, bed.Width, bed.Height);
                    if (rect.width < 1 || rect.height < 1 || !Contains(map, rect))
                        return $"Khu '{zone.Id}' có luống nằm ngoài map hoặc kích thước không hợp lệ.";
                }
            }
            return null;
        }

        public static void PaintGround(MapPaintSettings settings, Tilemap tilemap)
        {
            BeginLayer(tilemap, "Vẽ Ground");
            tilemap.ClearAllTiles();
            PaintGrass(settings, tilemap);
            if (settings.PaintYard)
                PaintNineSlice(tilemap, settings.YardRect, settings.YardTiles);
            EndLayer(tilemap);
        }

        public static void PaintFence(MapPaintSettings settings, Tilemap tilemap)
        {
            BeginLayer(tilemap, "Vẽ hàng rào");
            ClearFenceTiles(tilemap, settings.FenceTiles);

            RectInt fence = FenceRect(settings);
            for (int y = fence.yMin; y < fence.yMax; y++)
            {
                for (int x = fence.xMin; x < fence.xMax; x++)
                {
                    bool onEdge = x == fence.xMin || x == fence.xMax - 1 || y == fence.yMin || y == fence.yMax - 1;
                    if (!onEdge)
                        continue;

                    TileBase tile = IsGate(settings, fence, x, y) ? null : TileAt(fence, x, y, settings.FenceTiles);
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
            EndLayer(tilemap);
        }

        public static void PaintPlots(MapPaintSettings settings, Tilemap tilemap)
        {
            BeginLayer(tilemap, "Vẽ Plots");
            tilemap.ClearAllTiles();
            foreach (ZoneDefinition zone in settings.Farm.Zones)
            {
                foreach (BedDefinition bed in zone.Beds)
                {
                    for (int row = 0; row < bed.Height; row++)
                    {
                        for (int col = 0; col < bed.Width; col++)
                            tilemap.SetTile(bed.Origin + new Vector3Int(col, row, 0), settings.PlotTile);
                    }
                }
            }
            EndLayer(tilemap);
        }

        public static void ClearLayer(Tilemap tilemap)
        {
            BeginLayer(tilemap, "Xóa lớp");
            tilemap.ClearAllTiles();
            EndLayer(tilemap);
        }

        private static string ValidateMapSize(MapPaintSettings settings)
        {
            Vector2Int size = settings.MapSize;
            return size.x < 1 || size.y < 1 ? "Kích thước map phải lớn hơn 0." : null;
        }

        private static RectInt MapRect(MapPaintSettings settings)
        {
            return new RectInt(0, 0, settings.MapSize.x, settings.MapSize.y);
        }

        private static RectInt FenceRect(MapPaintSettings settings)
        {
            MapPaintSettings.EdgeOffset offset = settings.FenceOffset;
            Vector2Int size = settings.MapSize;
            return new RectInt(offset.Left, offset.Bottom,
                size.x - offset.Left - offset.Right, size.y - offset.Bottom - offset.Top);
        }

        // Xóa hàng rào cũ để đổi offset hay dời cổng không để lại tile thừa.
        private static void ClearFenceTiles(Tilemap tilemap, MapPaintSettings.NineSliceTiles tiles)
        {
            foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(cell);
                if (tile != null && IsFenceTile(tile, tiles))
                    tilemap.SetTile(cell, null);
            }
        }

        private static bool IsFenceTile(TileBase tile, MapPaintSettings.NineSliceTiles tiles)
        {
            return tile == tiles.TopLeft || tile == tiles.Top || tile == tiles.TopRight
                || tile == tiles.Left || tile == tiles.Right
                || tile == tiles.BottomLeft || tile == tiles.Bottom || tile == tiles.BottomRight;
        }

        private static void BeginLayer(Tilemap tilemap, string undoName)
        {
            Undo.IncrementCurrentGroup();
            Undo.RegisterCompleteObjectUndo(new Object[] { tilemap, tilemap.gameObject }, undoName);
        }

        private static void EndLayer(Tilemap tilemap)
        {
            tilemap.CompressBounds();
            EditorUtility.SetDirty(tilemap);
            EditorSceneManager.MarkSceneDirty(tilemap.gameObject.scene);
        }

        private static void PaintGrass(MapPaintSettings settings, Tilemap tilemap)
        {
            MapPaintSettings.WeightedTile[] tiles = settings.GrassTiles;
            int totalWeight = TotalWeight(tiles);
            var random = new System.Random(settings.GrassSeed);
            Vector2Int size = settings.MapSize;

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                    tilemap.SetTile(new Vector3Int(x, y, 0), PickTile(tiles, random.Next(totalWeight)));
            }
        }

        private static TileBase PickTile(MapPaintSettings.WeightedTile[] tiles, int roll)
        {
            TileBase last = null;
            foreach (MapPaintSettings.WeightedTile entry in tiles)
            {
                if (entry.Tile == null || entry.Weight <= 0)
                    continue;

                last = entry.Tile;
                roll -= entry.Weight;
                if (roll < 0)
                    break;
            }
            return last;
        }

        // Center để trống thì giữ nguyên tile đã có ở phần giữa (cỏ random).
        private static void PaintNineSlice(Tilemap tilemap, RectInt rect, MapPaintSettings.NineSliceTiles tiles)
        {
            for (int y = rect.yMin; y < rect.yMax; y++)
            {
                for (int x = rect.xMin; x < rect.xMax; x++)
                {
                    TileBase tile = TileAt(rect, x, y, tiles);
                    if (tile != null)
                        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        private static bool IsGate(MapPaintSettings settings, RectInt fence, int x, int y)
        {
            bool vertical = settings.GateEdge == MapPaintSettings.MapEdge.Left
                || settings.GateEdge == MapPaintSettings.MapEdge.Right;
            int along = vertical ? y - fence.yMin : x - fence.xMin;
            if (along < settings.GateStart || along >= settings.GateStart + settings.GateSize)
                return false;

            switch (settings.GateEdge)
            {
                case MapPaintSettings.MapEdge.Left:
                    return x == fence.xMin;
                case MapPaintSettings.MapEdge.Right:
                    return x == fence.xMax - 1;
                case MapPaintSettings.MapEdge.Top:
                    return y == fence.yMax - 1;
                default:
                    return y == fence.yMin;
            }
        }

        private static TileBase TileAt(RectInt rect, int x, int y, MapPaintSettings.NineSliceTiles tiles)
        {
            bool left = x == rect.xMin;
            bool right = x == rect.xMax - 1;

            if (y == rect.yMax - 1)
                return left ? tiles.TopLeft : right ? tiles.TopRight : tiles.Top;
            if (y == rect.yMin)
                return left ? tiles.BottomLeft : right ? tiles.BottomRight : tiles.Bottom;
            return left ? tiles.Left : right ? tiles.Right : tiles.Center;
        }

        private static int TotalWeight(MapPaintSettings.WeightedTile[] tiles)
        {
            int total = 0;
            foreach (MapPaintSettings.WeightedTile entry in tiles)
            {
                if (entry.Tile != null && entry.Weight > 0)
                    total += entry.Weight;
            }
            return total;
        }

        // Không xét Center: sân để trống thì dùng cỏ random, hàng rào không dùng tile này.
        private static bool IsComplete(MapPaintSettings.NineSliceTiles tiles)
        {
            return tiles.TopLeft != null && tiles.Top != null && tiles.TopRight != null
                && tiles.Left != null && tiles.Right != null
                && tiles.BottomLeft != null && tiles.Bottom != null && tiles.BottomRight != null;
        }

        private static bool Contains(RectInt outer, RectInt inner)
        {
            return inner.xMin >= outer.xMin && inner.yMin >= outer.yMin
                && inner.xMax <= outer.xMax && inner.yMax <= outer.yMax;
        }
    }
}
