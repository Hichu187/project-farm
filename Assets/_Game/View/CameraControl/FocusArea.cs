using UnityEngine;

namespace VeQue.View.CameraControl
{
    // Vùng camera zoom vào. Góc dưới-trái = transform.position, kích thước tính bằng ô (Grid cell 1).
    // Padding: khi đang zoom, camera được kéo ra ngoài khu thêm bao nhiêu ô ở từng cạnh.
    public sealed class FocusArea : MonoBehaviour
    {
        [SerializeField] private Vector2Int _sizeInCells = new Vector2Int(4, 5);
        [SerializeField] private EdgePadding _padding;

        public Rect WorldRect => new Rect(transform.position, _sizeInCells);
        public Vector2 Center => WorldRect.center;

        public Rect PaddedRect => _padding.Apply(WorldRect);

        public bool Contains(Vector2 worldPoint) => WorldRect.Contains(worldPoint);

        private void OnDrawGizmos()
        {
            Rect rect = WorldRect;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(rect.center, rect.size);

            Rect padded = PaddedRect;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(padded.center, padded.size);
        }
    }
}
