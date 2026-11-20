using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Một dòng trong danh sách Cần / Mồi: icon, tên, dòng phụ, tối đa 3 nút ở góc dưới-phải (Dùng / Sửa / Mua…).
    // Nền đổi theo trạng thái: thường, đang dùng (xanh), khóa cấp (mờ).
    public sealed class FishingItemRow : MonoBehaviour
    {
        public enum Look
        {
            Normal,
            Selected,
            Locked,
        }

        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _subtitle;
        [Tooltip("Nút theo thứ tự trái → phải; nút không dùng bị ẩn.")]
        [SerializeField] private Button[] _buttons = Array.Empty<Button>();
        [SerializeField] private TMP_Text[] _labels = Array.Empty<TMP_Text>();
        [SerializeField] private Color _normal = new Color(0.90f, 0.78f, 0.58f);
        [SerializeField] private Color _selected = new Color(0.75f, 0.90f, 0.55f);
        [SerializeField] private Color _locked = new Color(0.75f, 0.70f, 0.62f);

        private Action[] _actions;

        public int ButtonCount => _buttons.Length;

        private void Awake()
        {
            _actions ??= new Action[_buttons.Length];
            for (int i = 0; i < _buttons.Length; i++)
            {
                int index = i;
                _buttons[i].onClick.AddListener(() => _actions[index]?.Invoke());
            }
        }

        // Đặt nội dung và ẩn mọi nút; gọi SetButton sau đó cho các nút cần hiện.
        public void Set(Sprite icon, string title, string subtitle, Look look)
        {
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            Color iconColor = Color.white;
            iconColor.a = look == Look.Locked ? 0.45f : 1f;
            _icon.color = iconColor;
            _title.text = title;
            _subtitle.text = subtitle;
            _background.color = look == Look.Selected ? _selected : look == Look.Locked ? _locked : _normal;
            for (int i = 0; i < _buttons.Length; i++)
                _buttons[i].gameObject.SetActive(false);
        }

        // onClick null hoặc interactable false = nút mờ (vd. "Đang dùng", "Cấp 5", thiếu xu).
        public void SetButton(int index, string text, Action onClick, bool interactable = true)
        {
            if (index < 0 || index >= _buttons.Length)
                return;
            _actions ??= new Action[_buttons.Length];
            _actions[index] = onClick;
            _labels[index].text = text;
            _buttons[index].interactable = interactable && onClick != null;
            _buttons[index].gameObject.SetActive(true);
        }
    }
}
