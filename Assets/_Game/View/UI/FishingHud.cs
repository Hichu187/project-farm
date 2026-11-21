using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Fishing;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // HUD vùng câu: 3 nút icon Cần / Giỏ / Mồi (số nhỏ ở góc: độ bền, số cá, số mồi; mở bảng tương ứng – duyệt 13/11/2026,
    // khung giờ chuyển sang đồng hồ chung GameClock), thanh lực khi giữ Ném, nút lớn Ném / Giật, nút Thu cần.
    // Chỉ hiện ở vùng câu; nút Ném / Thu cần chỉ hiện khi đứng ở chỗ câu; 3 nút icon khóa khi đang câu.
    public sealed class FishingHud : MonoBehaviour
    {
        private const string CastText = "Ném";
        private const string StrikeText = "GIẬT!";
        // Đang chờ, cá chưa cắn (bấm lúc này = cá hoảng, mất lượt).
        private const string WaitText = "Chờ...";

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private RegionTravel _travel;
        [SerializeField] private FishingController _controller;
        [Tooltip("Toàn bộ HUD câu (chỉ bật ở vùng câu).")]
        [SerializeField] private GameObject _root;
        [Header("Nút icon Cần / Giỏ / Mồi")]
        [Tooltip("Cột 3 nút: ẩn khi kéo cá (nhường chỗ cho bảng kéo).")]
        [SerializeField] private GameObject _itemButtons;
        [SerializeField] private Button _rodButton;
        [SerializeField] private TMP_Text _rodBadge;
        [SerializeField] private Button _basketButton;
        [SerializeField] private TMP_Text _basketBadge;
        [SerializeField] private Button _baitButton;
        [SerializeField] private TMP_Text _baitBadge;
        [Tooltip("Icon trên nút Cần / Mồi: đổi theo cần đang dùng / mồi đang chọn; data chưa có icon thì giữ icon dựng sẵn.")]
        [SerializeField] private Image _rodIcon;
        [SerializeField] private Image _baitIcon;
        [Tooltip("Phóng icon bao nhiêu lần (bội số nguyên, cùng UiBuilder.UiPixelScale).")]
        [SerializeField] private float _iconPixelScale = 2f;
        [SerializeField] private RodPanel _rodPanel;
        [SerializeField] private BasketPanel _basketPanel;
        [SerializeField] private BaitPanel _baitPanel;
        [SerializeField] private Color _badgeNormal = Color.white;
        [SerializeField] private Color _badgeWarning = new Color(1f, 0.4f, 0.35f);
        [Tooltip("Nút Ném, nút Thu cần: chỉ hiện khi đứng ở chỗ câu.")]
        [SerializeField] private GameObject _spotControls;
        [SerializeField] private HoldButton _actionButton;
        [SerializeField] private TMP_Text _actionLabel;
        [SerializeField] private Button _reelButton;
        [Tooltip("Đổi chữ thập kéo cá sang tay trái / phải (Đợt 2 Phần 6); chỉ hiện khi chưa câu.")]
        [SerializeField] private Button _handButton;
        [SerializeField] private TMP_Text _handLabel;
        [Header("Thanh lực")]
        [SerializeField] private GameObject _powerBar;
        [Tooltip("Vùng Tốt / Perfect và con trỏ: con của rãnh thanh, kéo dài theo chiều ngang bằng anchor.")]
        [SerializeField] private RectTransform _goodZone;
        [SerializeField] private RectTransform _perfectZone;
        [SerializeField] private RectTransform _cursor;
        [SerializeField] private Toast _toast;
        [Header("Chữ nổi trên đầu nhân vật")]
        [SerializeField] private WorldPopText _popText;
        [Tooltip("Màu đánh giá ném: Yếu, Tốt, Tuyệt, Hoàn hảo (theo CastQuality).")]
        [SerializeField] private Color[] _castColors =
        {
            new Color(0.75f, 0.75f, 0.75f),
            Color.white,
            new Color(1f, 0.85f, 0.3f),
            new Color(0.55f, 0.95f, 0.35f),
        };
        [Tooltip("Màu chữ khi hụt cá.")]
        [SerializeField] private Color _missColor = new Color(1f, 0.45f, 0.4f);

        private bool _lowRodWarned;
        [Header("Cá cắn: nút Giật phập phồng")]
        [SerializeField] private float _pulseScale = 0.12f;
        [SerializeField] private float _pulseRate = 8f;

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
            _controller.CastJudged += ShowCast;
            _controller.Missed += ShowMiss;
            _travel.RegionChanged += HandleRegionChanged;
            _game.Farm.Changed += Refresh;
            _actionButton.Pressed += _controller.PressAction;
            _actionButton.Released += _controller.ReleaseAction;
            _reelButton.onClick.AddListener(_controller.ReelIn);
            if (_handButton != null)
                _handButton.onClick.AddListener(PullHandSetting.Toggle);
            PullHandSetting.Changed += Refresh;
            _rodButton.onClick.AddListener(_rodPanel.Open);
            _basketButton.onClick.AddListener(_basketPanel.Open);
            _baitButton.onClick.AddListener(_baitPanel.Open);
            _root.SetActive(_travel.IsFishing);
            _rodFallback = _rodIcon != null ? _rodIcon.sprite : null;
            _baitFallback = _baitIcon != null ? _baitIcon.sprite : null;
            Refresh();
        }

        // Icon dựng sẵn (Sunnyside) – dùng khi cần / mồi chưa có icon trong data.
        private Sprite _rodFallback;
        private Sprite _baitFallback;

        private void OnDestroy()
        {
            PullHandSetting.Changed -= Refresh;
            if (_controller != null)
            {
                _controller.Changed -= Refresh;
                _controller.Message -= ShowMessage;
                _controller.CastJudged -= ShowCast;
                _controller.Missed -= ShowMiss;
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

            float scale = _controller.BiteShown ? 1f + _pulseScale * Mathf.Abs(Mathf.Sin(Time.time * _pulseRate)) : 1f;
            _actionButton.transform.localScale = new Vector3(scale, scale, 1f);
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

        private void ShowCast(CastQuality quality)
        {
            if (_popText == null)
                return;
            int i = (int)quality;
            _popText.Show(UiFormat.CastQualityName(quality), i < _castColors.Length ? _castColors[i] : Color.white, _controller.HeadPosition);
            WarnLowRod();
        }

        // Báo một lần khi lần ném làm cần tụt xuống ≤ 10% độ bền (sửa xong / đổi cần thì báo lại được).
        private void WarnLowRod()
        {
            Farm farm = _game.Farm;
            bool low = farm.TryGetEquippedRod(out RodDef rodDef, out RodState rod) && !rod.IsBroken && farm.IsRodLow(rodDef.Id);
            if (low && !_lowRodWarned)
                ShowMessage("Cần sắp hỏng, nên sửa");
            _lowRodWarned = low;
        }

        private void ShowMiss(FishingOutcome outcome)
        {
            if (_popText != null)
                _popText.Show(UiFormat.FishingOutcomeText(outcome, _controller.LastCastWasJunk), _missColor, _controller.HeadPosition);
        }

        private void Refresh()
        {
            if (!_root.activeSelf)
                return;

            Farm farm = _game.Farm;
            FishingController.Phase phase = _controller.CurrentPhase;

            // Số nhỏ ở góc: độ bền cần đang dùng (đỏ khi hỏng), số cá / sức chứa (đỏ khi đầy), số mồi đang chọn (đỏ khi hết).
            bool hasRod = farm.TryGetEquippedRod(out RodDef rodDef, out RodState rod);
            SetBadge(_rodBadge, hasRod ? $"{rod.Durability}/{farm.GetRodMaxDurability(rodDef.Id)}" : "-", !hasRod || farm.IsRodLow(rodDef.Id));
            SetBadge(_basketBadge, $"{farm.State.Fishing.BasketUsed}/{farm.BasketCapacity}", farm.IsBasketFull);
            int baits = _controller.BaitId != null ? farm.State.Fishing.GetBaits(_controller.BaitId) : 0;
            SetBadge(_baitBadge, baits.ToString(), baits <= 0);
            RefreshIcons(hasRod ? rodDef.Id : null, _controller.BaitId);

            bool atSpot = phase != FishingController.Phase.Away;
            bool pulling = phase == FishingController.Phase.Pulling || phase == FishingController.Phase.Landing;
            _spotControls.SetActive(atSpot);
            _itemButtons.SetActive(!pulling);
            _actionButton.gameObject.SetActive(!pulling);
            _actionLabel.text = phase != FishingController.Phase.Waiting ? CastText
                : _controller.BiteShown ? StrikeText : WaitText;
            _actionButton.interactable = phase == FishingController.Phase.Ready
                || phase == FishingController.Phase.Charging
                || phase == FishingController.Phase.Waiting;
            _reelButton.gameObject.SetActive(_controller.InSession && phase != FishingController.Phase.Landing);
            if (_handButton != null)
            {
                _handButton.gameObject.SetActive(!_controller.InSession);
                if (_handLabel != null)
                    _handLabel.text = PullHandSetting.LeftHanded ? "Tay trái" : "Tay phải";
            }
            // Không đổi cần / mồi giữa lượt câu; bảng (nền tối) cũng không được che nút Giật.
            _rodButton.interactable = !_controller.InSession;
            _basketButton.interactable = !_controller.InSession;
            _baitButton.interactable = !_controller.InSession;

            bool charging = phase == FishingController.Phase.Charging;
            _powerBar.SetActive(charging);
            if (charging && rodDef != null)
                SetZones(farm.FishingDefinition.Tuning, rodDef);
        }

        // Nút Cần / Mồi: icon cần đang dùng / mồi đang chọn (14/11/2026).
        private void RefreshIcons(string rodId, string baitId)
        {
            FishingDefinition data = _game.Definition.Fishing;
            Sprite rod = null;
            if (data != null && rodId != null && data.TryGetRod(rodId, out RodDefinition rodArt))
                rod = rodArt.Icon;
            Sprite bait = null;
            if (data != null && baitId != null && data.TryGetBait(baitId, out BaitDefinition baitArt))
                bait = baitArt.Icon;
            SetIcon(_rodIcon, rod != null ? rod : _rodFallback);
            SetIcon(_baitIcon, bait != null ? bait : _baitFallback);
        }

        private void SetIcon(Image image, Sprite sprite)
        {
            if (image == null || image.sprite == sprite)
                return;
            image.sprite = sprite;
            image.enabled = sprite != null;
            if (sprite != null)
                // Quy về pixel PPU 16: icon HD 32×32 (PPU 32) chiếm cùng khung với icon 16×16 cũ.
                image.rectTransform.sizeDelta = sprite.rect.size * (16f / sprite.pixelsPerUnit) * _iconPixelScale;
        }

        private void SetBadge(TMP_Text badge, string text, bool warning)
        {
            badge.text = text;
            badge.color = warning ? _badgeWarning : _badgeNormal;
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
