using UnityEngine;

namespace VeQue.View.Map
{
    // Vùng art chưa có tính năng (ao, chuồng, nhà kho, đất mở rộng). Góc dưới-trái = transform.position, kích thước tính bằng ô.
    public sealed class ComingSoonArea : MonoBehaviour
    {
        [SerializeField] private string _label = "Ao cá";
        [SerializeField] private Vector2Int _sizeInCells = new Vector2Int(6, 6);

        public string Label => _label;

        public bool Contains(Vector2 worldPoint) => new Rect(transform.position, _sizeInCells).Contains(worldPoint);

        private void OnDrawGizmos()
        {
            Rect rect = new Rect(transform.position, _sizeInCells);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(rect.center, rect.size);
        }
    }
}
