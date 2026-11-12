using UnityEngine;
using VeQue.Logic;
using VeQue.View.Character;

namespace VeQue.View.Map
{
    // Chỗ câu ở vùng câu: chạm vào vùng chạm (góc dưới-trái = transform.position) → nhân vật đi tới ô đứng,
    // quay mặt ra nước. Icon cần câu (con, tùy chọn) nhấp nhô 1 pixel để người chơi thấy chỗ chạm được.
    public sealed class FishingSpot : MonoBehaviour
    {
        private const float PixelsPerUnit = 16f;

        [SerializeField] private Vector2Int _tapSize = new Vector2Int(4, 3);
        [Tooltip("Ô nhân vật đứng câu (toạ độ ô của Grid).")]
        [SerializeField] private Vector2Int _standCell;
        [Tooltip("Hướng nhân vật quay mặt khi câu (về phía nước).")]
        [SerializeField] private Facing _facing = Facing.Left;
        [SerializeField] private SpriteRenderer _icon;
        [SerializeField] private float _bobPeriod = 1.2f;

        private Vector3 _iconBase;
        private bool _iconUp;

        public GridCell StandCell => new GridCell(_standCell.x, _standCell.y);
        public Facing Facing => _facing;

        public bool Contains(Vector2 world) => new Rect(transform.position, _tapSize).Contains(world);

        private void Awake()
        {
            if (_icon != null)
                _iconBase = _icon.transform.localPosition;
        }

        private void Update()
        {
            if (_icon == null || _bobPeriod <= 0f)
                return;

            bool up = Mathf.Repeat(Time.unscaledTime / _bobPeriod, 1f) < 0.5f;
            if (up == _iconUp)
                return;
            _iconUp = up;
            _icon.transform.localPosition = _iconBase + new Vector3(0f, up ? 1f / PixelsPerUnit : 0f, 0f);
        }

        private void OnDrawGizmos()
        {
            var rect = new Rect(transform.position, _tapSize);
            Gizmos.color = new Color(0.3f, 0.8f, 1f);
            Gizmos.DrawWireCube(rect.center, rect.size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(_standCell.x + 0.5f, _standCell.y + 0.5f, 0f), new Vector3(0.8f, 0.8f, 0f));
        }
    }
}
