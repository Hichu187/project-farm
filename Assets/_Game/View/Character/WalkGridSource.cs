using UnityEngine;
using UnityEngine.Tilemaps;
using VeQue.Logic;
using VeQue.View.Map;

namespace VeQue.View.Character
{
    // Dựng lưới ô đi được cho nhân vật từ Tilemap, không dùng collider:
    // đi được = Ground có tile, Plots (luống) và Blockers (hàng rào, cây, rơm, nhà…) không có tile,
    // và không nằm trong vùng "sắp có" (chuồng, nhà kho, ao…).
    // Blockers chỉ dùng để đánh dấu, có thể tắt Tilemap Renderer của nó.
    public sealed class WalkGridSource : MonoBehaviour
    {
        [SerializeField] private Tilemap _ground;
        [SerializeField] private Tilemap _plots;
        [SerializeField] private Tilemap _blockers;
        [SerializeField] private ComingSoonArea[] _blockedAreas = System.Array.Empty<ComingSoonArea>();
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
            var grid = new WalkGrid(bounds.xMin, bounds.yMin, Mathf.Max(1, bounds.size.x), Mathf.Max(1, bounds.size.y));
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    bool walkable = _ground.HasTile(cell)
                        && (_plots == null || !_plots.HasTile(cell))
                        && (_blockers == null || !_blockers.HasTile(cell));
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
