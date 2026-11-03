using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Bảng trượt từ dưới lên. Backdrop chặn chạm xuống map, chạm vào thì đóng.
    // Gắn trên GameObject luôn bật; Backdrop và Sheet là con, được bật khi mở.
    public sealed class BottomSheet : MonoBehaviour
    {
        [SerializeField] private GameObject _backdrop;
        [SerializeField] private RectTransform _sheet;
        [SerializeField] private Button _backdropButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private float _duration = 0.18f;

        private float _shownY;
        private Coroutine _slide;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            _shownY = _sheet.anchoredPosition.y;
            _backdropButton.onClick.AddListener(Close);
            _closeButton.onClick.AddListener(Close);
        }

        public void Open()
        {
            if (IsOpen)
                return;

            IsOpen = true;
            _backdrop.SetActive(true);
            _sheet.gameObject.SetActive(true);
            Slide(_shownY - _sheet.sizeDelta.y, _shownY, false);
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            _backdrop.SetActive(false);
            Slide(_sheet.anchoredPosition.y, _shownY - _sheet.sizeDelta.y, true);
        }

        private void Slide(float fromY, float toY, bool hideWhenDone)
        {
            if (_slide != null)
                StopCoroutine(_slide);
            _slide = StartCoroutine(SlideRoutine(fromY, toY, hideWhenDone));
        }

        private IEnumerator SlideRoutine(float fromY, float toY, bool hideWhenDone)
        {
            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                SetY(Mathf.Lerp(fromY, toY, t * t * (3f - 2f * t)));
                yield return null;
            }

            SetY(toY);
            if (hideWhenDone)
                _sheet.gameObject.SetActive(false);
            _slide = null;
        }

        private void SetY(float y)
        {
            Vector2 position = _sheet.anchoredPosition;
            position.y = y;
            _sheet.anchoredPosition = position;
        }
    }
}
