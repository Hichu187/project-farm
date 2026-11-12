using UnityEngine;
using VeQue.View.Anim;

namespace VeQue.View.CameraControl
{
    // Chạm khu → khung 4 góc thu vào ôm khu, camera zoom xong thì mờ dần (zoom rồi khu đã chiếm gần hết màn hình).
    // Theo dõi CameraController.Current nên không cần sửa luồng chạm.
    public sealed class ZoneSelectHighlight : MonoBehaviour
    {
        // Trên ô đất, cây, nhân vật; dưới icon gợi ý tưới.
        private const int SortingOrder = 290;

        [SerializeField] private CameraController _controller;
        [SerializeField] private SelectBoxSkin _skin;
        [SerializeField] private Color _color = Color.white;

        private SelectBox _box;
        private FocusArea _shown;

        private void Awake()
        {
            if (_controller == null)
                _controller = GetComponent<CameraController>();
            if (_skin != null && _skin.IsValid)
                _box = SelectBox.Create(_skin, null, "ZoneSelectBox", SortingOrder);
            else
                Debug.LogWarning("ZoneSelectHighlight: chưa gán SelectBoxSkin đủ 4 góc, không hiện khung chọn khu.", this);
        }

        private void OnDestroy()
        {
            if (_box != null)
                Destroy(_box.gameObject);
        }

        private void LateUpdate()
        {
            if (_box == null || _controller == null)
                return;

            FocusArea current = _controller.Current;
            if (current != _shown)
            {
                _shown = current;
                if (current != null)
                    _box.Show(current.WorldRect, SelectBox.Motion.Static, _color, 0f, true);
                else
                    _box.Hide();
                return;
            }

            if (_shown != null && !_controller.IsTransitioning && !_box.IsFading)
                _box.FadeOut();
        }
    }
}
