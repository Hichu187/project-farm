using UnityEngine;
using VeQue.View.CameraControl;
using VeQue.View.Data;

namespace VeQue.View.Map
{
    // Nối một khu trên map với data của nó. Gắn cùng GameObject với FocusArea.
    [RequireComponent(typeof(FocusArea))]
    public sealed class ZoneAnchor : MonoBehaviour
    {
        [SerializeField] private ZoneDefinition _zone;

        public ZoneDefinition Zone => _zone;

        private void Awake()
        {
            if (_zone == null)
            {
                Debug.LogWarning("ZoneAnchor: chưa gán Zone.", this);
                return;
            }

            // Vị trí FocusArea và Beds đều nhập tay nên kiểm tra để phát hiện sớm khi lệch.
            RectInt box = _zone.BoundingBox;
            Rect expected = new Rect(box.x, box.y, box.width, box.height);
            Rect actual = GetComponent<FocusArea>().WorldRect;
            if (actual != expected)
                Debug.LogWarning($"ZoneAnchor: FocusArea {actual} không khớp các luống của '{_zone.Id}' {expected}.", this);
        }
    }
}
