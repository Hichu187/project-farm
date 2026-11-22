using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Icon sự kiện trên HUD (Đợt 2 Phần 8): nút nhỏ dưới đồng hồ ghi tên sự kiện hôm nay ("Chợ phiên", "+1" nếu nhiều);
    // ngày thường thì ẩn. Chạm → bảng chi tiết: tên, câu loa, hiệu ứng bằng lời, khung giờ. Xét mỗi giây (sang ngày, lên cấp).
    public sealed class EventBadge : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private GameObject _button;
        [SerializeField] private TMP_Text _label;
        [Header("Chi tiết")]
        [SerializeField] private GameObject _detailRoot;
        [SerializeField] private TMP_Text _detailTitle;
        [SerializeField] private TMP_Text _detailBody;
        [SerializeField] private Button _detailClose;

        private float _nextCheck;
        private int _shownDay = int.MinValue;
        private int _shownCount = -1;

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            _detailRoot.SetActive(false);
            _button.GetComponent<Button>().onClick.AddListener(ShowDetail);
            _detailClose.onClick.AddListener(() => _detailRoot.SetActive(false));
            Refresh();
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            if (now < _nextCheck)
                return;
            _nextCheck = now + 1f;
            Refresh();
        }

        private void Refresh()
        {
            if (_game == null || _game.Farm == null)
                return;
            Farm farm = _game.Farm;
            var events = farm.TodayEvents;
            int day = farm.Calendar.DayKey;
            if (day == _shownDay && events.Count == _shownCount)
                return;
            _shownDay = day;
            _shownCount = events.Count;
            _button.SetActive(events.Count > 0);
            if (events.Count > 0)
                _label.text = UiFormat.EventBadge(events[0].Title, events.Count);
            else
                _detailRoot.SetActive(false);
        }

        private void ShowDetail()
        {
            var events = _game.Farm.TodayEvents;
            if (events.Count == 0)
                return;
            var titles = new System.Collections.Generic.List<string>(events.Count);
            var body = new System.Text.StringBuilder();
            foreach (EventDef e in events)
            {
                titles.Add(e.Title);
                if (body.Length > 0)
                    body.Append("\n\n");
                if (events.Count > 1)
                    body.Append("<b>").Append(e.Title).Append("</b>\n");
                if (!string.IsNullOrEmpty(e.LoaText))
                    body.Append(e.LoaText).Append('\n');
                body.Append(EventText.Effects(_game, e));
                string windows = UiFormat.EventWindows(e.Windows);
                if (windows.Length > 0)
                    body.Append('\n').Append(windows);
            }
            _detailTitle.text = string.Join(" + ", titles);
            _detailBody.text = body.ToString();
            _detailRoot.SetActive(true);
        }
    }
}
