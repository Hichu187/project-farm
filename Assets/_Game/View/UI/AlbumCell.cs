using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Một ô trong lưới Album (Đợt 2 Phần 5): đã có = icon + tên, nền theo độ hiếm; chưa có = bóng đen + "???".
    public sealed class AlbumCell : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Button _button;
        [SerializeField] private Color _base = new Color(0.96f, 0.87f, 0.70f);
        [SerializeField] private Color _unknownBase = new Color(0.80f, 0.70f, 0.55f);
        [Tooltip("Bóng đen của món chưa có (nhân màu sprite).")]
        [SerializeField] private Color _silhouette = new Color(0.12f, 0.10f, 0.14f, 0.85f);
        [Range(0f, 1f)] [SerializeField] private float _tint = 0.45f;

        private Action _onClick;

        private void Awake()
        {
            _button.onClick.AddListener(() => _onClick?.Invoke());
        }

        public void SetKnown(Sprite icon, string name, Color rarity, Action onClick)
        {
            Apply(Color.Lerp(_base, rarity, _tint), icon, Color.white, name, onClick);
        }

        public void SetUnknown(Sprite icon, Action onClick)
        {
            Apply(_unknownBase, icon, _silhouette, "???", onClick);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onClick = null;
        }

        private void Apply(Color background, Sprite icon, Color iconColor, string name, Action onClick)
        {
            gameObject.SetActive(true);
            _background.color = background;
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _icon.color = iconColor;
            _name.text = name;
            _onClick = onClick;
        }
    }
}
