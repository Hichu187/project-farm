using UnityEngine;
using VeQue.Logic;
using VeQue.View.CameraControl;

namespace VeQue.View.Map
{
    // Một vùng chơi trong cùng Grid: nông trại (Map D) hoặc vùng câu. Góc dưới-trái = transform.position,
    // kích thước tính bằng ô. Cổng: chạm vào thì đi sang vùng kia (nhân vật đi tới cổng rồi chuyển cảnh);
    // tới vùng mới thì nhân vật đứng ở cổng của vùng đó.
    public sealed class PlayRegion : MonoBehaviour
    {
        public enum RegionKind
        {
            Farm,
            Fishing,
        }

        [SerializeField] private RegionKind _kind;
        [SerializeField] private string _displayName = "Nông trại";
        [SerializeField] private Vector2Int _sizeInCells = new Vector2Int(24, 26);
        [Tooltip("Nới (dương) / thu (âm) giới hạn camera so với khung vùng, tính bằng ô. Nông trại dùng giới hạn của CameraController.")]
        [SerializeField] private EdgePadding _cameraPadding;
        [Tooltip("Ô cổng (toạ độ ô của Grid). Nhân vật đi tới ô đi được gần nhất.")]
        [SerializeField] private Vector2Int _gateCell;
        [SerializeField] private Vector2Int _gateSize = new Vector2Int(1, 2);
        [Tooltip("Chỉ vùng câu: các chỗ câu.")]
        [SerializeField] private FishingSpot[] _spots = System.Array.Empty<FishingSpot>();

        public RegionKind Kind => _kind;
        public string DisplayName => _displayName;
        public Rect WorldRect => new Rect(transform.position, _sizeInCells);
        public Rect CameraRect => _cameraPadding.Apply(WorldRect);
        public GridCell GateCell => new GridCell(_gateCell.x, _gateCell.y);
        public FishingSpot[] Spots => _spots;

        // Vùng chạm của cổng: các ô cổng nới thêm 1 ô mỗi bên cho dễ chạm.
        public bool GateContains(Vector2 world)
        {
            var rect = new Rect(_gateCell.x - 1f, _gateCell.y - 1f, _gateSize.x + 2f, _gateSize.y + 2f);
            return rect.Contains(world);
        }

        public bool TryGetSpotAt(Vector2 world, out FishingSpot spot)
        {
            for (int i = 0; i < _spots.Length; i++)
            {
                if (_spots[i] != null && _spots[i].Contains(world))
                {
                    spot = _spots[i];
                    return true;
                }
            }
            spot = null;
            return false;
        }

        private void OnDrawGizmos()
        {
            Rect rect = WorldRect;
            Gizmos.color = _kind == RegionKind.Farm ? new Color(0.4f, 1f, 0.4f) : new Color(0.3f, 0.7f, 1f);
            Gizmos.DrawWireCube(rect.center, rect.size);

            Gizmos.color = new Color(1f, 0.6f, 0.1f);
            var gate = new Rect(_gateCell, _gateSize);
            Gizmos.DrawWireCube(gate.center, gate.size);
        }
    }
}
