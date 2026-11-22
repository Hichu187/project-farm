using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Loa xã (Đợt 2 Phần 8, duyệt 22/11/2026): banner trượt xuống dưới HUD lúc mở game lần đầu mỗi ngày, không chặn màn
    // hình (chỉ chính banner nhận chạm để đóng sớm), tự ẩn sau _showSeconds. Nội dung: thứ + ngày dương + ngày âm,
    // câu loa của sự kiện hôm nay (hoặc ngày thường), cây hút hàng / rớt giá.
    // Thứ tự khi mở game: tóm tắt lúc vắng mặt (chưa có) → popup điểm danh → loa xã: chờ màn tạo nhân vật và popup điểm danh.
    // Ngày đã đọc lưu trong PlayerPrefs (tiện ích hiển thị, không cần save).
    public sealed class LoaXaBanner : MonoBehaviour
    {
        private const string PrefKey = "veque.loa_xa_ngay";

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private TMP_Text _market;
        [SerializeField] private Button _closeButton;
        [SerializeField] private CheckInPopup _checkInPopup;
        [SerializeField] private CharacterCreator _creator;
        [SerializeField] private float _showSeconds = 8f;
        [SerializeField] private float _slideSeconds = 0.25f;

        private float _nextCheck;
        private float _hideAt = -1f;
        private Vector2 _shownPosition;
        private Coroutine _slide;

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            _shownPosition = _panel.anchoredPosition;
            _panel.gameObject.SetActive(false);
            _closeButton.onClick.AddListener(Hide);
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            if (_hideAt > 0f && now >= _hideAt)
                Hide();
            if (now < _nextCheck)
                return;
            _nextCheck = now + 1f;
            if (_panel.gameObject.activeSelf || _game == null || _game.Farm == null)
                return;
            int today = _game.Farm.Calendar.DayKey;
            if (LastShownDay() == today)
                return;
            if ((_creator != null && _creator.IsOpen)
                || (_checkInPopup != null && (_checkInPopup.IsOpen || _checkInPopup.WillPromptToday)))
                return;
            SaveShownDay(today);
            Show();
        }

        private void Show()
        {
            Farm farm = _game.Farm;
            int today = farm.Calendar.DayKey;
            string lunar = farm.Calendar.TryGetLunar(out LunarDate date) ? UiFormat.LunarDay(date) : null;
            _header.text = UiFormat.LoaHeader(today, lunar);

            var events = farm.TodayEvents;
            _body.text = events.Count > 0 && !string.IsNullOrEmpty(events[0].LoaText) ? events[0].LoaText : UiFormat.LoaQuietDay;
            _market.text = UiFormat.LoaMarket(CropName(farm.TodayHotCrop), CropName(farm.TodayColdCrop));
            _market.gameObject.SetActive(_market.text.Length > 0);

            _panel.gameObject.SetActive(true);
            _hideAt = Time.unscaledTime + _showSeconds;
            Slide(true);
        }

        public void Hide()
        {
            _hideAt = -1f;
            if (_panel.gameObject.activeSelf)
                Slide(false);
        }

        private void Slide(bool show)
        {
            if (_slide != null)
                StopCoroutine(_slide);
            _slide = StartCoroutine(SlideRoutine(show));
        }

        // Trượt từ trên xuống (ẩn: ngược lại rồi tắt).
        private IEnumerator SlideRoutine(bool show)
        {
            Vector2 hidden = _shownPosition + new Vector2(0f, _panel.rect.height + 20f);
            Vector2 from = show ? hidden : _panel.anchoredPosition;
            Vector2 to = show ? _shownPosition : hidden;
            float elapsed = 0f;
            while (elapsed < _slideSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _slideSeconds);
                _panel.anchoredPosition = Vector2.Lerp(from, to, t * t * (3f - 2f * t));
                yield return null;
            }
            _panel.anchoredPosition = to;
            if (!show)
                _panel.gameObject.SetActive(false);
            _slide = null;
        }

        private string CropName(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            return _game.TryGetCrop(id, out CropDefinition crop) ? crop.DisplayName : id;
        }

        private static int LastShownDay()
        {
            try
            {
                return PlayerPrefs.GetInt(PrefKey, int.MinValue);
            }
            catch
            {
                return int.MinValue;
            }
        }

        private static void SaveShownDay(int day)
        {
            try
            {
                PlayerPrefs.SetInt(PrefKey, day);
                PlayerPrefs.Save();
            }
            catch
            {
                // Trình duyệt chặn lưu: banner có thể hiện lại lần mở sau, không sao.
            }
        }
    }
}
