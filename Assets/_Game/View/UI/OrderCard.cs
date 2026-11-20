using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Thẻ một ô đơn: người đặt, nhãn khẩn cấp + đếm ngược hết hạn, tối đa 3 món (icon + đang có/cần), thưởng,
    // nút Giao/Bỏ; hoặc dòng chờ đơn mới. Đơn khẩn cấp đổi màu nền thẻ.
    public sealed class OrderCard : MonoBehaviour
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private TMP_Text _customer;
        [SerializeField] private TMP_Text _reward;
        [SerializeField] private GameObject[] _itemRoots;
        [SerializeField] private Image[] _itemIcons;
        [SerializeField] private TMP_Text[] _itemLabels;
        [SerializeField] private Button _deliver;
        [SerializeField] private Button _discard;
        [SerializeField] private TMP_Text _waiting;
        [SerializeField] private Color _doneColor = new Color(0.25f, 0.50f, 0.15f);
        [SerializeField] private Color _missingColor = new Color(0.27f, 0.16f, 0.09f);

        [Header("Đơn khẩn cấp")]
        [SerializeField] private Image _background;
        [Tooltip("Góc trên-phải: \"KHẨN CẤP · còn 12:34\".")]
        [SerializeField] private TMP_Text _badge;
        [SerializeField] private Color _normalColor = new Color(0.90f, 0.78f, 0.58f);
        [SerializeField] private Color _urgentColor = new Color(0.96f, 0.66f, 0.52f);

        [Header("Người đặt + thoại (Đợt 2 Phần 11)")]
        [Tooltip("Chân dung 16×16; chưa có art thì hiện khung tròn màu + chữ cái đầu tên.")]
        [SerializeField] private Image _portrait;
        [SerializeField] private TMP_Text _initial;
        [Tooltip("Câu đặt đơn, tối đa 2 dòng.")]
        [SerializeField] private TMP_Text _speech;
        [Tooltip("Bong bóng câu cảm ơn khi giao xong (nằm ngoài Content để vẫn hiện khi thẻ chuyển sang chờ đơn mới).")]
        [SerializeField] private GameObject _bubble;
        [SerializeField] private TMP_Text _bubbleText;

        private float _bubbleUntil = -1f;
        private static Sprite _circle;

        public Button Deliver => _deliver;
        public Button Discard => _discard;
        public int ItemSlots => _itemRoots.Length;

        // badge rỗng = đơn thường.
        public void ShowOrder(string customer, string reward, bool canDeliver, string badge)
        {
            _content.SetActive(true);
            _waiting.gameObject.SetActive(false);
            _customer.text = customer;
            _reward.text = reward;
            _deliver.interactable = canDeliver;
            bool urgent = !string.IsNullOrEmpty(badge);
            SetUrgent(urgent);
            if (_badge != null)
            {
                _badge.gameObject.SetActive(urgent);
                _badge.text = badge ?? "";
            }
        }

        // Người đặt: tên, câu thoại (rỗng = ẩn), chân dung (null = khung tròn màu + chữ cái).
        public void SetSpeaker(string name, string speech, Sprite portrait, string initial, Color circleColor)
        {
            _customer.text = name ?? "";
            if (_speech != null)
            {
                _speech.text = speech ?? "";
                _speech.gameObject.SetActive(!string.IsNullOrEmpty(speech));
            }
            if (_portrait == null)
                return;
            bool hasName = !string.IsNullOrEmpty(name);
            _portrait.gameObject.SetActive(hasName);
            if (!hasName)
                return;
            _portrait.sprite = portrait != null ? portrait : Circle();
            _portrait.color = portrait != null ? Color.white : circleColor;
            if (_initial != null)
            {
                _initial.gameObject.SetActive(portrait == null);
                _initial.text = initial ?? "";
            }
        }

        // Bong bóng câu cảm ơn trong seconds giây (thời gian thật).
        public void ShowBubble(string text, float seconds)
        {
            if (_bubble == null || string.IsNullOrEmpty(text))
                return;
            _bubbleText.text = text;
            _bubble.SetActive(true);
            _bubbleUntil = Time.unscaledTime + seconds;
        }

        private void Update()
        {
            if (_bubbleUntil >= 0f && Time.unscaledTime >= _bubbleUntil)
            {
                _bubbleUntil = -1f;
                _bubble.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (_bubble != null)
                _bubble.SetActive(false);
            _bubbleUntil = -1f;
        }

        // Khung tròn pixel 16×16 (sinh bằng code, dùng chung), tô màu theo người.
        private static Sprite Circle()
        {
            if (_circle != null)
                return _circle;
            const int Size = 16;
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "NpcCircle",
                hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color32[Size * Size];
            var fill = new Color32(255, 255, 255, 255);
            var rim = new Color32(110, 90, 80, 255);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float dx = x + 0.5f - Size * 0.5f;
                    float dy = y + 0.5f - Size * 0.5f;
                    float d = dx * dx + dy * dy;
                    if (d <= 7.0f * 7.0f)
                        pixels[y * Size + x] = fill;
                    else if (d <= 8.0f * 8.0f)
                        pixels[y * Size + x] = rim;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            _circle = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 16f);
            _circle.hideFlags = HideFlags.DontSave;
            return _circle;
        }

        public void SetItem(int index, Sprite icon, string label, bool done)
        {
            _itemRoots[index].SetActive(true);
            _itemIcons[index].sprite = icon;
            _itemIcons[index].enabled = icon != null;
            _itemLabels[index].text = label;
            _itemLabels[index].color = done ? _doneColor : _missingColor;
        }

        public void HideItemsFrom(int index)
        {
            for (int i = index; i < _itemRoots.Length; i++)
                _itemRoots[i].SetActive(false);
        }

        public void ShowWaiting(string text, bool urgent)
        {
            _content.SetActive(false);
            _waiting.gameObject.SetActive(true);
            _waiting.text = text;
            SetUrgent(urgent);
        }

        private void SetUrgent(bool urgent)
        {
            if (_background != null)
                _background.color = urgent ? _urgentColor : _normalColor;
        }
    }
}
