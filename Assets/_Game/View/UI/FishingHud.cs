using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Fishing;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // HUD vùng câu (Đợt C): khung giờ + còn bao lâu, cần + độ bền, giỏ, mồi (chạm để đổi), thanh lực khi giữ Ném,
    // nút lớn Ném / Giật, nút Thu cần. Chỉ hiện ở vùng câu; nút câu chỉ hiện khi đứng ở chỗ câu.
    public sealed class FishingHud : MonoBehaviour
    {
        private const string CastText = "Ném";
        private const string StrikeText = "Giật!";
        private const float RefreshSeconds = 0.25f;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private RegionTravel _travel;
        [SerializeField] private FishingController _controller;
        [Tooltip("Toàn bộ HUD câu (chỉ bật ở vùng câu).")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _slotLabel;
        [SerializeField] private TMP_Text _rodLabel;
        [SerializeField] private TMP_Text _basketLabel;
        [Tooltip("Dòng cần + giỏ: ẩn khi kéo cá (nhường chỗ cho bảng kéo).")]
        [SerializeField] private GameObject _info;
        [Tooltip("Nút mồi, nút hành động, nút thu cần: chỉ hiện khi đứng ở chỗ câu.")]
        [SerializeField] private GameObject _spotControls;
        [SerializeField] private Button _baitButton;
        [SerializeField] private TMP_Text _baitLabel;
        [SerializeField] private HoldButton _actionButton;
        [SerializeField] private TMP_Text _actionLabel;
        [SerializeField] private Button _reelButton;
        [Header("Thanh lực")]
        [SerializeField] private GameObject _powerBar;
        [Tooltip("Vùng Tốt / Perfect và con trỏ: con của rãnh thanh, kéo dài theo chiều ngang bằng anchor.")]
        [SerializeField] private RectTransform _goodZone;
        [SerializeField] private RectTransform _perfectZone;
        [SerializeField] private RectTransform _cursor;
        [SerializeField] private Toast _toast;

        private float _nextRefresh;

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_travel == null)
                _travel = FindFirstObjectByType<RegionTravel>();
            if (_controller == null)
                _controller = FindFirstObjectByType<FishingController>();
            if (_controller == null || _travel == null)
            {
                _root.SetActive(false);
                enabled = false;
                return;
            }

            _controller.Changed += Refresh;
            _controller.Message += ShowMessage;
            _travel.RegionChanged += HandleRegionChanged;
            _game.Farm.Changed += Refresh;
            _actionButton.Pressed += _controller.PressAction;
            _actionButton.Released += _controller.ReleaseAction;
            _reelButton.onClick.AddListener(_controller.ReelIn);
            _baitButton.onClick.AddListener(_controller.CycleBait);
            _root.SetActive(_travel.IsFishing);
            Refresh();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.Changed -= Refresh;
                _controller.Message -= ShowMessage;
            }
            if (_travel != null)
                _travel.RegionChanged -= HandleRegionChanged;
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= Refresh;
        }

        private void Update()
        {
            if (!_root.activeSelf)
                return;

            if (_controller.CurrentPhase == FishingController.Phase.Charging)
                SetCursor(_controller.Power);

            // Khung giờ đếm ngược theo giây; không cần cập nhật mỗi khung hình.
            if (Time.unscaledTime >= _nextRefresh)
            {
                _nextRefresh = Time.unscaledTime + RefreshSeconds;
                RefreshSlot();
            }
        }

        private void HandleRegionChanged(PlayRegion region)
        {
            _root.SetActive(_travel.IsFishing);
            Refresh();
        }

        private void ShowMessage(string message)
        {
            if (_toast != null)
                _toast.Show(message);
        }

        private void Refresh()
        {
            if (!_root.activeSelf)
                return;

            Farm farm = _game.Farm;
            FishingDefinition data = _game.Definition.Fishing;
            FishingController.Phase phase = _controller.CurrentPhase;

            // Cần đang dùng + độ bền / độ bền tối đa thực tế.
            if (farm.TryGetEquippedRod(out RodDef rodDef, out RodState rod))
            {
                string name = data != null && data.TryGetRod(rodDef.Id, out RodDefinition rodData) ? rodData.DisplayName : rodDef.Id;
                _rodLabel.text = rod.IsBroken
                    ? $"{name} (hỏng)"
                    : $"{name} {rod.Durability}/{farm.GetRodMaxDurability(rodDef.Id)}";
            }
            else
            {
                _rodLabel.text = "Chưa có cần";
            }
            _basketLabel.text = $"Giỏ {farm.State.Fishing.Basket.Count}/{farm.BasketCapacity}";

            _baitLabel.text = _controller.TryGetBait(out BaitDefinition bait)
                ? $"Mồi: {bait.DisplayName} ({farm.State.Fishing.GetBaits(bait.Id)})"
                : "Mồi: hết";

            bool atSpot = phase != FishingController.Phase.Away;
            bool pulling = phase == FishingController.Phase.Pulling || phase == FishingController.Phase.Landing;
            _spotControls.SetActive(atSpot);
            _info.SetActive(!pulling);
            _actionButton.gameObject.SetActive(!pulling);
            _baitButton.gameObject.SetActive(!pulling);
            _actionLabel.text = phase == FishingController.Phase.Waiting ? StrikeText : CastText;
            _actionButton.interactable = phase == FishingController.Phase.Ready
                || phase == FishingController.Phase.Charging
                || phase == FishingController.Phase.Waiting;
            _reelButton.gameObject.SetActive(_controller.InSession && phase != FishingController.Phase.Landing);
            _baitButton.interactable = !_controller.InSession;

            bool charging = phase == FishingController.Phase.Charging;
            _powerBar.SetActive(charging);
            if (charging && rodDef != null)
                SetZones(farm.FishingDefinition.Tuning, rodDef);

            RefreshSlot();
        }

        private void RefreshSlot()
        {
            Farm farm = _game.Farm;
            long now = _game.NowSeconds;
            int offset = farm.State.UtcOffsetSeconds;
            DaySlot slot = DaySlots.Get(now, offset);
            _slotLabel.text = UiFormat.SlotStatus(slot, DaySlots.SecondsUntilEnd(now, offset));
        }

        // Vùng Tốt / Perfect quanh tâm Perfect theo cần đang dùng (cùng công thức FishingSession.Classify).
        private void SetZones(FishingTuning tuning, RodDef rod)
        {
            float half = rod.PerfectHalfWidth;
            SetSpan(_perfectZone, tuning.PerfectCenter - half, tuning.PerfectCenter + half);
            float good = half * tuning.GoodWidthMultiplier;
            SetSpan(_goodZone, tuning.PerfectCenter - good, tuning.PerfectCenter + good);
        }

        private void SetCursor(float power) => SetSpan(_cursor, power, power);

        private static void SetSpan(RectTransform rect, float from, float to)
        {
            rect.anchorMin = new Vector2(Mathf.Clamp01(from), 0f);
            rect.anchorMax = new Vector2(Mathf.Clamp01(to), 1f);
        }
    }
}
