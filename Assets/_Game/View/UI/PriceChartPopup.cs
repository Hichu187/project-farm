using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Biểu đồ giá 7 ngày của 1 nông sản (Đợt 2 Phần 9): 7 cột giá ngày (cũ → mới), số trên cột, thứ dưới cột,
    // cột hôm nay sáng hơn. Giá tính lại từ seed (Farm.GetPriceHistory), không lưu. Mở từ icon dòng trong Kho.
    public sealed class PriceChartPopup : MonoBehaviour
    {
        private const int Days = 7;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _note;
        [Tooltip("7 cột (cũ → mới); chiều cao đặt lúc chạy, neo đáy.")]
        [SerializeField] private RectTransform[] _bars = Array.Empty<RectTransform>();
        [SerializeField] private TMP_Text[] _values = Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] _days = Array.Empty<TMP_Text>();
        [SerializeField] private Button _close;
        [SerializeField] private float _maxBarHeight = 110f;
        [SerializeField] private Color _barColor = new Color(0.60f, 0.40f, 0.24f);
        [SerializeField] private Color _todayColor = new Color(0.45f, 0.62f, 0.28f);

        private readonly int[] _history = new int[Days];

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            _root.SetActive(false);
            _close.onClick.AddListener(() => _root.SetActive(false));
        }

        public void Show(CropDefinition crop)
        {
            if (crop == null)
                return;
            var farm = _game.Farm;
            farm.GetPriceHistory(crop.Id, _history);
            int max = 1;
            foreach (int price in _history)
                max = Mathf.Max(max, price);

            _icon.sprite = crop.ProduceIcon;
            _icon.enabled = _icon.sprite != null;
            _title.text = UiFormat.PriceChartTitle(crop.DisplayName);
            _note.text = UiFormat.MarketTag(crop.Id == farm.TodayHotCrop, crop.Id == farm.TodayColdCrop)
                + UiFormat.PriceChange(farm.GetPriceChangePercent(crop.Id));

            int today = farm.Calendar.DayKey;
            for (int i = 0; i < _bars.Length && i < Days; i++)
            {
                bool isToday = i == Days - 1;
                Vector2 size = _bars[i].sizeDelta;
                // Cột thấp nhất vẫn cao ≥ 20% để so được; tỉ lệ theo giá cao nhất trong 7 ngày.
                size.y = _maxBarHeight * Mathf.Max(0.2f, _history[i] / (float)max);
                _bars[i].sizeDelta = size;
                if (_bars[i].TryGetComponent(out Image bar))
                    bar.color = isToday ? _todayColor : _barColor;
                _values[i].text = UiFormat.Coins(_history[i]);
                _days[i].text = isToday ? "Nay" : UiFormat.WeekdayShort(today - (Days - 1 - i));
            }
            _root.SetActive(true);
        }
    }
}
