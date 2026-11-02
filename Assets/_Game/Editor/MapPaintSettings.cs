using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using VeQue.View.Data;

namespace VeQue.EditorTools
{
    // Thiết lập của tool Về Quê → Tạo map. Chỉ dùng trong Editor, game không tham chiếu nên asset không vào build.
    // Mọi tọa độ là cell Unity: góc dưới-trái map tại (0, 0), trục Y hướng lên.
    public sealed class MapPaintSettings : ScriptableObject
    {
        public enum MapEdge { Left, Right, Top, Bottom }

        [Serializable]
        public struct WeightedTile
        {
            public TileBase Tile;
            [Min(0)] public int Weight;
        }

        [Serializable]
        public struct EdgeOffset
        {
            [Min(0)] public int Left;
            [Min(0)] public int Right;
            [Min(0)] public int Top;
            [Min(0)] public int Bottom;
        }

        [Serializable]
        public struct NineSliceTiles
        {
            public TileBase TopLeft;
            public TileBase Top;
            public TileBase TopRight;
            public TileBase Left;
            public TileBase Center;
            public TileBase Right;
            public TileBase BottomLeft;
            public TileBase Bottom;
            public TileBase BottomRight;
        }

        [SerializeField] private Vector2Int _mapSize = new Vector2Int(44, 26);

        [Tooltip("Tile được chọn ngẫu nhiên theo Weight.")]
        [SerializeField] private WeightedTile[] _grassTiles = Array.Empty<WeightedTile>();
        [Tooltip("Cùng seed thì vẽ ra giống hệt.")]
        [SerializeField] private int _grassSeed = 1;

        [SerializeField] private bool _paintYard = true;
        [SerializeField] private RectInt _yardRect = new RectInt(1, 2, 42, 16);
        [Tooltip("Để trống Center thì phần giữa dùng cỏ random từ Grass Tiles.")]
        [SerializeField] private NineSliceTiles _yardTiles;

        [Tooltip("Không cần tile Center.")]
        [SerializeField] private NineSliceTiles _fenceTiles;
        [Tooltip("Khoảng cách từ mép map tới hàng rào ở từng cạnh, tính bằng ô.")]
        [SerializeField] private EdgeOffset _fenceOffset;
        [SerializeField] private MapEdge _gateEdge = MapEdge.Left;
        [Tooltip("Vị trí bắt đầu của cổng dọc theo cạnh hàng rào, tính từ góc dưới hoặc góc trái của hàng rào.")]
        [Min(0)] [SerializeField] private int _gateStart = 8;
        [Tooltip("0 = không có cổng.")]
        [Min(0)] [SerializeField] private int _gateSize = 2;

        [Tooltip("Vị trí ô đất đọc từ ZoneDefinition.Beds.")]
        [SerializeField] private FarmDefinition _farm;
        [SerializeField] private TileBase _plotTile;

        public Vector2Int MapSize => _mapSize;
        public WeightedTile[] GrassTiles => _grassTiles;
        public int GrassSeed => _grassSeed;
        public bool PaintYard => _paintYard;
        public RectInt YardRect => _yardRect;
        public NineSliceTiles YardTiles => _yardTiles;
        public NineSliceTiles FenceTiles => _fenceTiles;
        public EdgeOffset FenceOffset => _fenceOffset;
        public MapEdge GateEdge => _gateEdge;
        public int GateStart => _gateStart;
        public int GateSize => _gateSize;
        public TileBase PlotTile => _plotTile;

        public FarmDefinition Farm
        {
            get => _farm;
            set => _farm = value;
        }
    }
}
