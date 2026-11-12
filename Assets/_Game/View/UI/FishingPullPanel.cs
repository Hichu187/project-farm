using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Fishing;

namespace VeQue.View.UI
{
    // Bảng kéo cá (Đợt D, 1 lượt – chốt 13/11/2026): chuỗi mũi tên (đỏ = bấm ngược), thanh thời gian
    // (xanh = còn Perfect, vàng = còn Tốt, xám = Thường), 4 nút mũi tên chữ thập ở giữa dưới.
    // Bàn phím: phím mũi tên (thử trong Editor / máy tính).
    public sealed class FishingPullPanel : MonoBehaviour
    {
        [SerializeField] private FishingController _controller;
        [SerializeField] private GameObject _root;
        [Tooltip("Ô mũi tên theo thứ tự (đủ cho chuỗi dài nhất); ô thừa bị ẩn.")]
        [SerializeField] private Image[] _slots = System.Array.Empty<Image>();
        [SerializeField] private RectTransform _timeFill;
        [SerializeField] private Image _timeFillImage;
        [Tooltip("Nút theo thứ tự Lên, Xuống, Trái, Phải.")]
        [SerializeField] private HoldButton[] _buttons = System.Array.Empty<HoldButton>();
        [Header("Sprite mũi tên (Lên, Xuống, Trái, Phải)")]
        [SerializeField] private Sprite[] _arrowSprites = new Sprite[4];
        [Header("Màu")]
        [SerializeField] private Color _normal = Color.white;
        [SerializeField] private Color _red = new Color(1f, 0.35f, 0.3f);
        [SerializeField] private Color _done = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color _perfectTime = new Color(0.45f, 0.8f, 0.3f);
        [SerializeField] private Color _goodTime = new Color(0.95f, 0.78f, 0.3f);
        [SerializeField] private Color _normalTime = new Color(0.6f, 0.6f, 0.6f);

        private void Start()
        {
            if (_controller == null)
                _controller = FindFirstObjectByType<FishingController>();
            if (_controller == null)
            {
                _root.SetActive(false);
                enabled = false;
                return;
            }

            _controller.Changed += Refresh;
            _controller.PullProgressed += Refresh;
            for (int i = 0; i < _buttons.Length; i++)
            {
                var arrow = (Arrow)i;
                _buttons[i].Pressed += () => _controller.PressArrow(arrow);
            }
            Refresh();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.Changed -= Refresh;
                _controller.PullProgressed -= Refresh;
            }
        }

        private void Update()
        {
            PullRound pull = _controller.Pull;
            if (pull == null)
                return;

            float used = _controller.PullElapsed / pull.TimeLimitSeconds;
            _timeFill.anchorMax = new Vector2(Mathf.Clamp01(1f - used), 1f);
            FishingTuning tuning = _controller.Tuning;
            float percent = used * 100f;
            _timeFillImage.color = percent <= tuning.PerfectTimePercent ? _perfectTime
                : percent <= tuning.GoodTimePercent ? _goodTime
                : _normalTime;

            ReadKeyboard();
        }

        private void ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.upArrowKey.wasPressedThisFrame)
                _controller.PressArrow(Arrow.Up);
            else if (keyboard.downArrowKey.wasPressedThisFrame)
                _controller.PressArrow(Arrow.Down);
            else if (keyboard.leftArrowKey.wasPressedThisFrame)
                _controller.PressArrow(Arrow.Left);
            else if (keyboard.rightArrowKey.wasPressedThisFrame)
                _controller.PressArrow(Arrow.Right);
        }

        private void Refresh()
        {
            PullRound pull = _controller.Pull;
            _root.SetActive(pull != null);
            if (pull == null)
                return;

            for (int i = 0; i < _slots.Length; i++)
            {
                Image slot = _slots[i];
                bool used = i < pull.Count;
                slot.gameObject.SetActive(used);
                if (!used)
                    continue;
                slot.sprite = _arrowSprites[(int)pull.Arrows[i]];
                slot.color = i < _controller.PullIndex ? _done : pull.Red[i] ? _red : _normal;
            }
        }
    }
}
