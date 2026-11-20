using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Fishing;

namespace VeQue.View.UI
{
    // Bảng kéo cá (Đợt D, 1 lượt – chốt 13/11/2026): chuỗi mũi tên (đỏ = bấm ngược), thanh thời gian
    // (xanh = còn Perfect, vàng = còn Tốt, xám = Thường).
    // Đợt 2 Phần 6 (20/11/2026):
    // - Nhập: chữ thập ở góc dưới phải (tay trái: dưới trái, PullHandSetting), nhận lúc chạm xuống; vuốt ở giữa màn hình
    //   (SwipePad, thử A/B cùng nút); phím mũi tên (Editor / máy tính). Mọi lần bấm cách nhau ≥ _minPressInterval
    //   để không nhận đôi / hai ngón cùng lúc.
    // - Bấm sai (luật cũ: về mũi tên đầu, giờ vẫn chạy): ô sai rung đỏ, các ô đã đúng sáng lại lần lượt từ phải sang trái;
    //   phao giật ngược (FishingController), rung máy (Android).
    // - Âm thanh: nốt cao dần mỗi mũi đúng, tiếng trầm khi sai (PullSound).
    // Móc được cá: xem trước chuỗi trong FishingTuning.PullPreviewSeconds (chữ "Sẵn sàng...", thanh đầy, chưa tính giờ).
    public sealed class FishingPullPanel : MonoBehaviour
    {
        [SerializeField] private FishingController _controller;
        [SerializeField] private GameObject _root;
        [Tooltip("Ô mũi tên theo thứ tự (đủ cho chuỗi dài nhất); ô thừa bị ẩn.")]
        [SerializeField] private Image[] _slots = System.Array.Empty<Image>();
        [SerializeField] private RectTransform _timeFill;
        [SerializeField] private Image _timeFillImage;
        [Tooltip("Chữ \"Sẵn sàng...\" trong lúc xem trước chuỗi mũi tên.")]
        [SerializeField] private GameObject _readyLabel;
        [Tooltip("Nút theo thứ tự Lên, Xuống, Trái, Phải.")]
        [SerializeField] private HoldButton[] _buttons = System.Array.Empty<HoldButton>();
        [Tooltip("Gốc chữ thập; anchor góc dưới phải, tay trái thì lật sang dưới trái.")]
        [SerializeField] private RectTransform _pad;
        [SerializeField] private SwipePad _swipe;
        [SerializeField] private PullSound _sound;
        [Header("Sprite mũi tên (Lên, Xuống, Trái, Phải)")]
        [SerializeField] private Sprite[] _arrowSprites = new Sprite[4];
        [Header("Màu")]
        [SerializeField] private Color _normal = Color.white;
        [SerializeField] private Color _red = new Color(1f, 0.35f, 0.3f);
        [SerializeField] private Color _done = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color _perfectTime = new Color(0.45f, 0.8f, 0.3f);
        [SerializeField] private Color _goodTime = new Color(0.95f, 0.78f, 0.3f);
        [SerializeField] private Color _normalTime = new Color(0.6f, 0.6f, 0.6f);
        [Header("Nhập")]
        [Tooltip("Bỏ qua lần bấm cách lần trước ít hơn khoảng này (giây).")]
        [SerializeField] private float _minPressInterval = 0.06f;
        [Header("Bấm sai")]
        [SerializeField] private Color _mistake = new Color(1f, 0.15f, 0.1f);
        [SerializeField] private float _mistakeSeconds = 0.3f;
        [Tooltip("Góc rung tối đa của ô sai (độ).")]
        [SerializeField] private float _shakeDegrees = 18f;
        [SerializeField] private float _shakeHz = 20f;
        [Tooltip("Mỗi ô đã đúng sáng lại trễ hơn ô bên phải nó bấy nhiêu giây.")]
        [SerializeField] private float _restoreStagger = 0.04f;

        private float _lastPressTime = float.NegativeInfinity;
        private float _mistakeStart = float.NegativeInfinity;
        private int _mistakeSlot = -1;
        private int _mistakeDoneCount;
        private bool _animating;
        private Vector2 _padRightPosition;

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
            _controller.PullProgressed += HandleProgress;
            _controller.PullMistake += HandleMistake;
            for (int i = 0; i < _buttons.Length; i++)
            {
                var arrow = (Arrow)i;
                _buttons[i].Pressed += () => Submit(arrow);
            }
            if (_swipe != null)
                _swipe.Swiped += Submit;
            if (_pad != null)
                _padRightPosition = _pad.anchoredPosition;
            PullHandSetting.Changed += ApplyHand;
            ApplyHand();
            Refresh();
        }

        private void OnDestroy()
        {
            PullHandSetting.Changed -= ApplyHand;
            if (_swipe != null)
                _swipe.Swiped -= Submit;
            if (_controller != null)
            {
                _controller.Changed -= Refresh;
                _controller.PullProgressed -= HandleProgress;
                _controller.PullMistake -= HandleMistake;
            }
        }

        private void Update()
        {
            PullRound pull = _controller.Pull;
            if (pull == null)
                return;

            if (_readyLabel != null)
                _readyLabel.SetActive(_controller.PullPreviewing);

            float used = _controller.PullElapsed / pull.TimeLimitSeconds;
            _timeFill.anchorMax = new Vector2(Mathf.Clamp01(1f - used), 1f);
            FishingTuning tuning = _controller.Tuning;
            float percent = used * 100f;
            _timeFillImage.color = percent <= tuning.PerfectTimePercent ? _perfectTime
                : percent <= tuning.GoodTimePercent ? _goodTime
                : _normalTime;

            if (_animating)
                Refresh();
            ReadKeyboard();
        }

        // Mọi nguồn nhập (nút, vuốt, bàn phím) đi qua đây: chặn nhận đôi trong _minPressInterval.
        private void Submit(Arrow arrow)
        {
            if (_controller.Pull == null)
                return;
            float now = Time.unscaledTime;
            if (now - _lastPressTime < _minPressInterval)
                return;
            _lastPressTime = now;
            _controller.PressArrow(arrow);
        }

        private void HandleProgress()
        {
            // Bấm đúng trong lúc ô sai còn rung: dừng hiệu ứng, hiện đúng tiến độ mới.
            _mistakeStart = float.NegativeInfinity;
            if (_sound != null)
                _sound.PlayCorrect(_controller.PullIndex - 1);
            Refresh();
        }

        private void HandleMistake()
        {
            _mistakeStart = Time.time;
            _mistakeSlot = _controller.LastMistakeIndex;
            _mistakeDoneCount = Mathf.Max(0, _mistakeSlot);
            if (_sound != null)
                _sound.PlayMistake();
            Refresh();
        }

        private void ApplyHand()
        {
            if (_pad == null)
                return;
            bool left = PullHandSetting.LeftHanded;
            float x = left ? 0f : 1f;
            _pad.anchorMin = new Vector2(x, 0f);
            _pad.anchorMax = new Vector2(x, 0f);
            _pad.anchoredPosition = new Vector2(left ? -_padRightPosition.x : _padRightPosition.x, _padRightPosition.y);
        }

        private void ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.upArrowKey.wasPressedThisFrame)
                Submit(Arrow.Up);
            else if (keyboard.downArrowKey.wasPressedThisFrame)
                Submit(Arrow.Down);
            else if (keyboard.leftArrowKey.wasPressedThisFrame)
                Submit(Arrow.Left);
            else if (keyboard.rightArrowKey.wasPressedThisFrame)
                Submit(Arrow.Right);
        }

        private void Refresh()
        {
            PullRound pull = _controller.Pull;
            _root.SetActive(pull != null);
            if (pull == null)
            {
                _animating = false;
                return;
            }

            float sinceMistake = Time.time - _mistakeStart;
            bool restoring = _mistakeDoneCount > 0
                && sinceMistake < _mistakeSeconds + _restoreStagger * _mistakeDoneCount;
            bool shaking = sinceMistake < _mistakeSeconds;
            _animating = shaking || restoring;

            for (int i = 0; i < _slots.Length; i++)
            {
                Image slot = _slots[i];
                bool used = i < pull.Count;
                slot.gameObject.SetActive(used);
                if (!used)
                    continue;
                slot.sprite = _arrowSprites[(int)pull.Arrows[i]];
                Color baseColor = pull.Red[i] ? _red : _normal;
                float angle = 0f;

                if (_animating && i == _mistakeSlot)
                {
                    // Ô bấm sai: đỏ, lắc qua lại, nhỏ dần về 0.
                    float k = shaking ? 1f - sinceMistake / _mistakeSeconds : 0f;
                    angle = Mathf.Sin(sinceMistake * _shakeHz * 2f * Mathf.PI) * _shakeDegrees * k;
                    slot.color = Color.Lerp(baseColor, _mistake, k);
                }
                else if (_animating && i < _mistakeDoneCount)
                {
                    // Ô đã đúng trước khi sai: sáng lại lần lượt từ phải sang trái.
                    float delay = _restoreStagger * (_mistakeDoneCount - 1 - i);
                    float t = Mathf.Clamp01((sinceMistake - delay) / Mathf.Max(0.01f, _mistakeSeconds));
                    slot.color = Color.Lerp(_done, baseColor, t);
                }
                else
                {
                    slot.color = i < _controller.PullIndex ? _done : baseColor;
                }
                slot.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}
