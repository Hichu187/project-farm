using UnityEngine;
using UnityEngine.Tilemaps;
using VeQue.Logic;
using VeQue.View.Map;

namespace VeQue.View.Character
{
    // Dựng lưới ô đi được cho nhân vật từ Tilemap, không dùng collider:
    // đi được = Ground có tile, Plots (luống) và Blockers (hàng rào, cây, rơm, nhà…) không có tile,
    // và không nằm trong vùng "sắp có" (chuồng, nhà kho, ao…).
    // Vùng khác (vùng câu) dùng Tilemap riêng: _extraGround thêm ô đi được, _extraBlockers (nước…) chặn.
    // Blockers chỉ dùng để đánh dấu, có thể tắt Tilemap Renderer của nó.
    public sealed class WalkGridSource : MonoBehaviour
    {
        [SerializeField] private Tilemap _ground;
        [SerializeField] private Tilemap _plots;
        [SerializeField] private Tilemap _blockers;
        [SerializeField] private ComingSoonArea[] _blockedAreas = System.Array.Empty<ComingSoonArea>();
        [Tooltip("Tilemap nền của vùng khác (vd. FishingGround): ô có tile là đi được.")]
        [SerializeField] private Tilemap[] _extraGround = System.Array.Empty<Tilemap>();
        [Tooltip("Tilemap chặn của vùng khác (vd. FishingWater): ô có tile là không đi được.")]
        [SerializeField] private Tilemap[] _extraBlockers = System.Array.Empty<Tilemap>();
        [Tooltip("Tô đỏ các ô bị chặn khi chọn GameObject này trong Scene view.")]
        [SerializeField] private bool _drawBlockedGizmos = true;

        private WalkGrid _grid;

        public WalkGrid Grid
        {
            get
            {
                if (_grid == null)
                    Build();
                return _grid;
            }
        }

        // Dựng lại lưới (vd. sau khi đổi tile lúc đang chơi).
        public void Build()
        {
            BoundsInt bounds = _ground.cellBounds;
            int xMin = bounds.xMin, yMin = bounds.yMin, xMax = bounds.xMax, yMax = bounds.yMax;
            foreach (Tilemap extra in _extraGround)
            {
                if (extra == null)
                    continue;
                BoundsInt b = extra.cellBounds;
                xMin = Mathf.Min(xMin, b.xMin);
                yMin = Mathf.Min(yMin, b.yMin);
                xMax = Mathf.Max(xMax, b.xMax);
                yMax = Mathf.Max(yMax, b.yMax);
            }

            var grid = new WalkGrid(xMin, yMin, Mathf.Max(1, xMax - xMin), Mathf.Max(1, yMax - yMin));
            for (int y = yMin; y < yMax; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    bool farm = _ground.HasTile(cell)
                        && (_plots == null || !_plots.HasTile(cell))
                        && (_blockers == null || !_blockers.HasTile(cell));
                    bool walkable = (farm || HasAny(_extraGround, cell)) && !HasAny(_extraBlockers, cell);
                    if (walkable)
                        grid.SetWalkable(new GridCell(x, y), true);
                }
            }

            for (int i = 0; i < _blockedAreas.Length; i++)
            {
                ComingSoonArea area = _blockedAreas[i];
                if (area == null)
                    continue;
                Vector3Int origin = _ground.WorldToCell(area.transform.position);
                grid.SetRect(origin.x, origin.y, area.SizeInCells.x, area.SizeInCells.y, false);
            }

            _grid = grid;
        }

        private static bool HasAny(Tilemap[] maps, Vector3Int cell)
        {
            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i] != null && maps[i].HasTile(cell))
                    return true;
            }
            return false;
        }

        public bool IsInBlockedArea(Vector2 world)
        {
            for (int i = 0; i < _blockedAreas.Length; i++)
            {
                ComingSoonArea area = _blockedAreas[i];
                if (area != null && area.Contains(world))
                    return true;
            }
            return false;
        }

        public GridCell WorldToCell(Vector3 world)
        {
            Vector3Int cell = _ground.WorldToCell(world);
            return new GridCell(cell.x, cell.y);
        }

        // Góc dưới-trái của ô (toạ độ world).
        public Vector3 CellToWorld(GridCell cell) => _ground.CellToWorld(new Vector3Int(cell.X, cell.Y, 0));

        private void OnDrawGizmosSelected()
        {
            if (!_drawBlockedGizmos || _ground == null)
                return;

            WalkGrid grid = Application.isPlaying ? Grid : BuildPreview();
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            for (int y = grid.MinY; y < grid.MinY + grid.Height; y++)
            {
                for (int x = grid.MinX; x < grid.MinX + grid.Width; x++)
                {
                    var cell = new GridCell(x, y);
                    if (grid.IsWalkable(cell))
                        continue;
                    Gizmos.DrawCube(CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f), new Vector3(0.9f, 0.9f, 0f));
                }
            }
        }

        // Trong Edit mode dựng lại mỗi lần vẽ để thấy ngay khi tô Blockers.
        private WalkGrid BuildPreview()
        {
            Build();
            WalkGrid preview = _grid;
            _grid = null;
            return preview;
        }
    }
}
