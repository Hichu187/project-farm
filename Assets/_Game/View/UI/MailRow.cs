using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Một dòng trong danh sách Hộp thư (Đợt 2 Phần 3): người gửi, tiêu đề, tóm tắt quà, nhãn trạng thái. Chạm = xem thư.
    public sealed class MailRow : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _sender;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _rewards;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Color _attentionColor = Color.white;
        [SerializeField] private Color _doneColor = new Color(0.85f, 0.82f, 0.76f);
        [SerializeField] private Color _statusNew = new Color(0.75f, 0.2f, 0.15f);
        [SerializeField] private Color _statusDone = new Color(0.45f, 0.42f, 0.38f);

        private Action _onClick;

        private void Awake()
        {
            _button.onClick.AddListener(() => _onClick?.Invoke());
        }

        // status: "Mới", "Có quà", "Đã nhận"…; attention = còn việc (nền sáng, nhãn đỏ).
        public void Show(string sender, string title, string rewards, string status, bool attention, Action onClick)
        {
            _sender.text = sender;
            _title.text = title;
            _rewards.text = rewards;
            _rewards.gameObject.SetActive(!string.IsNullOrEmpty(rewards));
            _status.text = status;
            _status.color = attention ? _statusNew : _statusDone;
            if (_background != null)
                _background.color = attention ? _attentionColor : _doneColor;
            _onClick = onClick;
        }
    }
}
