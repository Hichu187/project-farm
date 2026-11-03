using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.CameraControl;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Thanh tiến độ và thời gian còn lại ngay trên cây vừa chạm. Mỗi lúc chỉ có một thanh, chạm cây khác thì chuyển sang cây đó.
    // Ẩn khi hết thời gian hiện, cây chín hoặc về toàn cảnh. Tiến độ tính từ IClock qua Farm, không đếm giờ ở đây.
    public sealed class CropProgressPopup : MonoBehaviour
    {
        private const float GapAboveCrop = 4f;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmInteraction _interaction;
        [SerializeField] private FarmView _view;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private RectTransform _area;
        [SerializeField] private RectTransform _box;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _duration = 3f;

        private bool _isShowing;
        private PlotId _plot;
        private float _hideAt;
        private long _shownSeconds;

        public void Show(PlotId plot)
        {
            _plot = plot;
            _hideAt = Time.unscaledTime + _duration;
            _shownSeconds = -1;
            _isShowing = true;
            _box.gameObject.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            _isShowing = false;
            _box.gameObject.SetActive(false);
        }

        private void Start()
        {
            _interaction.CropInspected += Show;
        }

        private void OnDestroy()
        {
            if (_interaction != null)
                _interaction.CropInspected -= Show;
        }

        // Chạy sau khi camera đã di chuyển trong Update để thanh không trễ so với cây.
        private void LateUpdate()
        {
            if (_isShowing)
                Refresh();
        }

        private void Refresh()
        {
            Farm farm = _game.Farm;
            long remaining = farm.GetRemainingSeconds(_plot);
            if (remaining <= 0
                || Time.unscaledTime >= _hideAt
                || _cameraController.CurrentMode != CameraController.Mode.Focused
                || !farm.TryGetPlotState(_plot, out PlotState plot)
                || !farm.Definition.TryGetCrop(plot.CropId, out CropDef crop)
                || !_view.TryGetCropTop(_plot, out Vector3 top))
            {
                Hide();
                return;
            }

            _slider.SetValueWithoutNotify(1f - (float)remaining / crop.GrowSeconds);
            if (remaining != _shownSeconds)
            {
                _shownSeconds = remaining;
                _text.text = UiFormat.Countdown(remaining);
            }

            UpdatePosition(top);
        }

        private void UpdatePosition(Vector3 worldTop)
        {
            Vector2 screen = _camera.WorldToScreenPoint(worldTop);
            // Canvas Screen Space - Overlay nên không truyền camera.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, screen, null, out Vector2 local);

            // Giữ trong màn hình; pivot của thanh nằm giữa cạnh dưới.
            Vector2 halfArea = _area.rect.size * 0.5f;
            Vector2 size = _box.rect.size;
            local.x = Mathf.Clamp(local.x, -halfArea.x + size.x * 0.5f, halfArea.x - size.x * 0.5f);
            local.y = Mathf.Clamp(local.y + GapAboveCrop, -halfArea.y, halfArea.y - size.y);
            _box.anchoredPosition = local;
        }
    }
}
