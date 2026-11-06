using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Anim;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Thu hoạch: 1–3 icon nông sản bay cong từ ô đất về tab Kho; tới nơi tab nảy lên và hiện "+N [icon]" trên tab.
    // Canvas Screen Space Overlay: đổi world → màn hình bằng camera map, rồi màn hình → toạ độ của lớp bay.
    public sealed class HarvestFlyer : MonoBehaviour
    {
        private sealed class Flight
        {
            public Image Icon;
            public Vector2 From;
            public Vector2 Control;
            public float Start = -1f;
            public bool Last;
            public string CropId;
            public int Count;
            public Sprite Sprite;
        }

        [SerializeField] private FarmInteraction _interaction;
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmView _view;
        [SerializeField] private Camera _camera;
        [Tooltip("Lớp phủ toàn màn hình chứa các icon bay.")]
        [SerializeField] private RectTransform _layer;
        [Tooltip("Icon dựng sẵn (pool), tắt lúc đầu.")]
        [SerializeField] private Image[] _icons = System.Array.Empty<Image>();
        [Tooltip("Tab Kho: đích bay và nảy khi nhận hàng.")]
        [SerializeField] private RectTransform _target;
        [SerializeField] private GameObject _badge;
        [SerializeField] private Image _badgeIcon;
        [SerializeField] private TMP_Text _badgeText;

        [Header("Bay")]
        [SerializeField] private int _maxIconsPerHarvest = 3;
        [SerializeField] private float _flightDuration = 0.6f;
        [SerializeField] private float _stagger = 0.08f;
        [Tooltip("Độ cao vòng cung (đơn vị canvas).")]
        [SerializeField] private float _arcHeight = 140f;
        [SerializeField] private float _endScale = 0.6f;

        [Header("Tab Kho")]
        [SerializeField] private float _punchDuration = 0.25f;
        [SerializeField] private float _punchAmount = 0.15f;
        [Tooltip("Nhãn +N hiện bao lâu rồi mờ.")]
        [SerializeField] private float _badgeHold = 1.2f;
        [SerializeField] private float _badgeFade = 0.3f;

        private Flight[] _flights;
        private float _punchStart = -1f;
        private float _badgeShownAt = -1f;
        private string _badgeCropId;
        private int _badgeCount;
        private CanvasGroup _badgeGroup;

        private void Awake()
        {
            _flights = new Flight[_icons.Length];
            for (int i = 0; i < _icons.Length; i++)
            {
                _flights[i] = new Flight { Icon = _icons[i] };
                _icons[i].gameObject.SetActive(false);
            }

            _badgeGroup = _badge.GetComponent<CanvasGroup>();
            if (_badgeGroup == null)
                _badgeGroup = _badge.AddComponent<CanvasGroup>();
            _badge.SetActive(false);
        }

        private void OnEnable()
        {
            _interaction.Harvested += HandleHarvested;
        }

        private void OnDisable()
        {
            _interaction.Harvested -= HandleHarvested;
        }

        private void HandleHarvested(PlotId plot, string cropId, int count)
        {
            Sprite sprite = _game.TryGetCrop(cropId, out CropDefinition crop) ? crop.ProduceIcon : null;
            if (!_view.TryGetPlotCenter(plot, out Vector3 world) || !TryWorldToLayer(world, out Vector2 from))
            {
                // Không bay được (ô ngoài màn hình): vẫn báo trên tab Kho.
                Arrive(cropId, count, sprite);
                return;
            }

            Vector2 to = TargetInLayer();
            int icons = Mathf.Clamp(count, 1, _maxIconsPerHarvest);
            float now = Time.unscaledTime;
            for (int i = 0; i < icons; i++)
            {
                Flight flight = FindFree(now);
                flight.From = from + new Vector2((i - (icons - 1) * 0.5f) * 14f, 0f);
                flight.Control = (flight.From + to) * 0.5f + new Vector2(0f, _arcHeight);
                flight.Start = now + i * _stagger;
                flight.Last = i == icons - 1;
                flight.CropId = cropId;
                flight.Count = count;
                flight.Sprite = sprite;

                flight.Icon.sprite = sprite;
                flight.Icon.enabled = sprite != null;
                flight.Icon.rectTransform.anchoredPosition = flight.From;
                flight.Icon.rectTransform.localScale = Vector3.one;
                flight.Icon.gameObject.SetActive(false);
            }
        }

        private Flight FindFree(float now)
        {
            Flight oldest = _flights[0];
            foreach (Flight flight in _flights)
            {
                if (flight.Start < 0f)
                    return flight;
                if (flight.Start < oldest.Start)
                    oldest = flight;
            }
            return oldest;
        }

        private bool TryWorldToLayer(Vector3 world, out Vector2 local)
        {
            Camera camera = _camera != null ? _camera : Camera.main;
            local = default;
            if (camera == null)
                return false;

            Vector3 screen = camera.WorldToScreenPoint(world);
            if (screen.z < 0f)
                return false;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, screen, null, out local);
        }

        private Vector2 TargetInLayer()
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, _target.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, screen, null, out Vector2 local);
            return local;
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            Vector2 to = TargetInLayer();
            foreach (Flight flight in _flights)
                AnimateFlight(flight, now, to);
            AnimatePunch(now);
            AnimateBadge(now);
        }

        private void AnimateFlight(Flight flight, float now, Vector2 to)
        {
            if (flight.Start < 0f)
                return;

            float t = (now - flight.Start) / _flightDuration;
            if (t < 0f)
                return;
            if (t >= 1f)
            {
                flight.Start = -1f;
                flight.Icon.gameObject.SetActive(false);
                if (flight.Last)
                    Arrive(flight.CropId, flight.Count, flight.Sprite);
                return;
            }

            if (!flight.Icon.gameObject.activeSelf)
                flight.Icon.gameObject.SetActive(true);
            // Bezier bậc 2, nhanh dần về cuối như bị hút vào Kho.
            float e = Ease.InQuad(t);
            float u = 1f - e;
            Vector2 position = u * u * flight.From + 2f * u * e * flight.Control + e * e * to;
            flight.Icon.rectTransform.anchoredPosition = position;
            flight.Icon.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, _endScale, e);
        }

        // Cùng loại cây khi nhãn còn hiện thì cộng dồn, khác loại thì thay.
        private void Arrive(string cropId, int count, Sprite sprite)
        {
            float now = Time.unscaledTime;
            _punchStart = now;

            bool visible = _badgeShownAt >= 0f && now - _badgeShownAt < _badgeHold + _badgeFade;
            _badgeCount = visible && _badgeCropId == cropId ? _badgeCount + count : count;
            _badgeCropId = cropId;
            _badgeShownAt = now;

            _badgeText.text = $"+{_badgeCount}";
            _badgeIcon.sprite = sprite;
            _badgeIcon.enabled = sprite != null;
            _badgeGroup.alpha = 1f;
            _badge.SetActive(true);
        }

        private void AnimatePunch(float now)
        {
            if (_punchStart < 0f)
                return;

            float t = (now - _punchStart) / _punchDuration;
            if (t >= 1f)
            {
                _punchStart = -1f;
                _target.localScale = Vector3.one;
                return;
            }

            // DampedWave bắt đầu bằng nhịp âm; đảo dấu để phóng lên trước.
            _target.localScale = Vector3.one * (1f - Ease.DampedWave(t) * _punchAmount);
        }

        private void AnimateBadge(float now)
        {
            if (_badgeShownAt < 0f)
                return;

            float elapsed = now - _badgeShownAt;
            if (elapsed >= _badgeHold + _badgeFade)
            {
                _badgeShownAt = -1f;
                _badge.SetActive(false);
                return;
            }
            if (elapsed > _badgeHold)
                _badgeGroup.alpha = 1f - (elapsed - _badgeHold) / _badgeFade;
        }
    }
}
