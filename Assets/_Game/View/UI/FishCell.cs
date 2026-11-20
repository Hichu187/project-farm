using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.View.Data;
using VeQue.View.Fishing;

namespace VeQue.View.UI
{
    // Một ô vuông trong lưới giỏ cá (duyệt 13/11/2026):
    // - có cá: icon cá, cân nặng; nền theo độ hiếm; chạm = xem chi tiết.
    // - trống (đã mở): nền mờ.
    // - khóa: ổ khóa; ô khóa kế tiếp hiện giá và bấm được (mở ô), các ô khóa sau mờ hơn, không bấm được.
    public sealed class FishCell : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _weight;
        [SerializeField] private Image _lock;
        [SerializeField] private Button _button;
        [SerializeField] private Color _empty = new Color(0.85f, 0.74f, 0.55f, 0.6f);
        [SerializeField] private Color _lockedNext = new Color(0.80f, 0.68f, 0.50f);
        [SerializeField] private Color _lockedFar = new Color(0.70f, 0.60f, 0.45f, 0.5f);
        [Tooltip("Độ đậm của màu độ hiếm trên nền ô (0..1).")]
        [Range(0f, 1f)] [SerializeField] private float _tint = 0.45f;
        [SerializeField] private Color _base = new Color(0.96f, 0.87f, 0.70f);
        [SerializeField] private Color _selectedBase = Color.white;

        private Action _onClick;

        private void Awake()
        {
            _button.onClick.AddListener(() => _onClick?.Invoke());
        }

        public void SetEmpty()
        {
            Apply(_empty, null, "", false, null);
        }

        // shiny: kiểu cá shiny (null = cá thường); seed: vị trí đốm lấp lánh của con này.
        public void SetFish(Sprite icon, string weight, Color rarity, bool selected, Action onClick,
            ShinyPalette.Entry shiny = null, int seed = 0)
        {
            Apply(Color.Lerp(selected ? _selectedBase : _base, rarity, _tint), icon, weight, false, onClick);
            ShinyLook.Apply(_icon, shiny, seed);
        }

        // next: ô khóa kế tiếp (hiện giá, bấm được); còn lại chỉ hiện ổ khóa mờ.
        public void SetLocked(bool next, string price, Action onClick)
        {
            Apply(next ? _lockedNext : _lockedFar, null, next ? price : "", true, next ? onClick : null);
            Color c = _lock.color;
            c.a = next ? 1f : 0.45f;
            _lock.color = c;
        }

        private void Apply(Color background, Sprite icon, string label, bool locked, Action onClick)
        {
            _background.color = background;
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            ShinyLook.Apply(_icon, null, 0);
            _weight.text = label;
            if (_lock != null)
                _lock.enabled = locked && _lock.sprite != null;
            _button.interactable = onClick != null;
            _onClick = onClick;
        }
    }
}
