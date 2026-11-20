using TMPro;
using UnityEngine;

namespace VeQue.View.UI
{
    // Chữ nổi ngắn tại một điểm trên map (vd. trên đầu nhân vật): hiện, bay lên vài pixel rồi mờ dần.
    // Nằm trên Canvas Screen Space - Overlay; mỗi khung hình đổi vị trí theo camera. Gọi Show lần nữa thì thay chữ cũ.
    public sealed class WorldPopText : MonoBehaviour
    {
        private const float PixelsPerUnit = 16f;

        [SerializeField] private Camera _camera;
        [Tooltip("Vùng chứa (cùng kích thước Canvas / safe area) để quy đổi tọa độ màn hình.")]
        [SerializeField] private RectTransform _area;
        [SerializeField] private RectTransform _box;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private float _seconds = 0.9f;
        [Tooltip("Bay lên bao nhiêu pixel (pixel của art) trong thời gian hiện.")]
        [SerializeField] private float _risePixels = 8f;
        [Tooltip("Giữ nguyên độ đậm phần đầu (0..1 thời gian), sau đó mờ dần.")]
        [Range(0f, 1f)] [SerializeField] private float _holdFraction = 0.55f;
        [Tooltip("Nảy lên lúc mới hiện (phóng to rồi về 1).")]
        [SerializeField] private float _popScale = 1.3f;
        [SerializeField] private float _popSeconds = 0.12f;

        private Vector3 _anchor;
        private Color _color;
        private float _start = -1f;

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;
            _box.gameObject.SetActive(false);
        }

        public void Show(string text, Color color, Vector3 worldPoint)
        {
            if (string.IsNullOrEmpty(text))
                return;
            _label.text = text;
            _color = color;
            _anchor = worldPoint;
            _start = Time.time;
            _box.gameObject.SetActive(true);
            Apply(0f);
        }

        private void LateUpdate()
        {
            if (_start < 0f)
                return;
            float t = (Time.time - _start) / Mathf.Max(0.01f, _seconds);
            if (t >= 1f)
            {
                _start = -1f;
                _box.gameObject.SetActive(false);
                return;
            }
            Apply(t);
        }

        private void Apply(float t)
        {
            // Bay lên làm tròn theo pixel của art để không rung.
            float rise = Mathf.Round(_risePixels * (1f - (1f - t) * (1f - t))) / PixelsPerUnit;
            Vector2 screen = _camera.WorldToScreenPoint(_anchor + new Vector3(0f, rise, 0f));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, screen, null, out Vector2 local);
            _box.anchoredPosition = local;

            float elapsed = t * _seconds;
            float scale = elapsed < _popSeconds ? Mathf.Lerp(_popScale, 1f, elapsed / Mathf.Max(0.01f, _popSeconds)) : 1f;
            _box.localScale = new Vector3(scale, scale, 1f);

            Color c = _color;
            c.a *= t <= _holdFraction ? 1f : 1f - (t - _holdFraction) / (1f - _holdFraction);
            _label.color = c;
        }
    }
}
