using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Popup điểm danh (Đợt 2 Phần 10; 22/11/2026 đổi sang người chơi tự bấm nhận):
    // - Đủ điều kiện nhận (Farm.CanCheckIn) → popup tự mở 1 lần / ngày: lịch 7 ô (đã nhận mờ, hôm nay sáng, sau hiện quà),
    //   nút "Nhận" + "Để sau". Chờ màn tạo nhân vật đóng mới mở. Thứ tự khi mở game (thiết kế mục 10):
    //   tóm tắt lúc vắng mặt → popup này → loa xã (hai cái kia chưa có).
    // - Bấm Nhận: Farm.TryCheckIn gửi thư quà; icon quà bay từ ô hôm nay về nút Thư, nút Thư nảy; nút đổi thành "Mở hộp thư".
    // - "Để sau": không tự mở lại trong ngày; nút "Điểm danh" trên HUD (CheckInButton) có chấm đỏ, bấm để mở lại.
    public sealed class CheckInPopup : MonoBehaviour
    {
        private enum Mode
        {
            // Hôm nay nhận được, chưa bấm.
            Ready,
            // Vừa nhận / đã nhận hôm nay.
            Claimed,
            // Chưa tới lúc nhận (xem lịch từ bảng Thư).
            Waiting,
        }

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _subtitle;
        [Tooltip("7 ô theo thứ tự ngày 1..7.")]
        [SerializeField] private Image[] _cellBackgrounds = Array.Empty<Image>();
        [SerializeField] private TMP_Text[] _cellDays = Array.Empty<TMP_Text>();
        [SerializeField] private Image[] _cellIcons = Array.Empty<Image>();
        [SerializeField] private TMP_Text[] _cellRewards = Array.Empty<TMP_Text>();
        [SerializeField] private Button _claim;
        [SerializeField] private Button _later;
        [SerializeField] private Button _openMail;
        [SerializeField] private Button _close;
        [Tooltip("Màn chặn: popup chờ tới khi màn này đóng (lần đầu vào game).")]
        [SerializeField] private CharacterCreator _creator;
        [Header("Bay về nút Thư")]
        [Tooltip("Nút Thư trên HUD (đích bay, nảy khi tới).")]
        [SerializeField] private RectTransform _mailTarget;
        [Tooltip("Icon mẫu (tắt sẵn) – nhân bản mỗi lần bay.")]
        [SerializeField] private Image _flyTemplate;
        [SerializeField] private float _flySeconds = 0.6f;
        [SerializeField] private float _flyArc = 120f;
        [SerializeField] private float _bounceSeconds = 0.25f;
        [SerializeField] private Sprite _coinIcon;
        [SerializeField] private Color _claimedColor = new Color(0.80f, 0.74f, 0.62f, 0.55f);
        [SerializeField] private Color _todayColor = new Color(0.98f, 0.82f, 0.35f);
        [SerializeField] private Color _futureColor = new Color(0.96f, 0.90f, 0.76f);

        private readonly StringBuilder _text = new StringBuilder();
        private readonly List<Image> _flyPool = new List<Image>();
        // Ngày (DayKey) đã tự mở popup – không tự mở lại trong ngày.
        private int _promptedDay = int.MinValue;
        private float _nextCheck;

        // "Mở hộp thư" – GameUI mở bảng Thư (đóng các bảng khác).
        public event Action MailRequested;

        public bool IsOpen => _root.activeSelf;

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_creator == null)
                _creator = FindFirstObjectByType<CharacterCreator>();
            _root.SetActive(false);
            if (_flyTemplate != null)
                _flyTemplate.gameObject.SetActive(false);
            _claim.onClick.AddListener(Claim);
            _later.onClick.AddListener(Hide);
            _close.onClick.AddListener(Hide);
            _openMail.onClick.AddListener(() =>
            {
                Hide();
                MailRequested?.Invoke();
            });
        }

        // Mỗi giây: đủ điều kiện nhận và hôm nay chưa tự mở → mở (mở game, quay lại tab, sang ngày, đủ 18 giờ).
        private void Update()
        {
            float now = Time.unscaledTime;
            if (now < _nextCheck)
                return;
            _nextCheck = now + 1f;
            if (IsOpen || _game == null || _game.Farm == null || (_creator != null && _creator.IsOpen))
                return;
            Farm farm = _game.Farm;
            int today = farm.Calendar.DayKey;
            if (_promptedDay == today || !farm.CanCheckIn)
                return;
            _promptedDay = today;
            ShowCalendar();
        }

        // Từ nút Điểm danh hoặc tự mở: nhận được thì hiện nút Nhận; không thì xem lịch.
        public void ShowCalendar()
        {
            Farm farm = _game.Farm;
            CheckInState state = farm.State.CheckIn;
            if (farm.CanCheckIn)
            {
                Show(state.NextDay, state.Cycle, Mode.Ready);
                return;
            }
            bool claimedToday = state.HasClaimed && state.LastClaimDayKey >= farm.Calendar.DayKey;
            if (claimedToday)
            {
                // Vừa nhận ngày 7 thì NextDay đã về 1 (vòng mới): vẫn hiện vòng vừa xong.
                int day = state.NextDay == 1 ? CheckInDef.DayCount : state.NextDay - 1;
                int cycle = state.NextDay == 1 ? state.Cycle - 1 : state.Cycle;
                Show(day, Mathf.Max(0, cycle), Mode.Claimed);
            }
            else
            {
                Show(state.NextDay, state.Cycle, Mode.Waiting);
            }
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        private void Claim()
        {
            Farm farm = _game.Farm;
            int day = farm.CheckInNextDay;
            int cycle = farm.CheckInCycle;
            if (!farm.TryCheckIn())
            {
                ShowCalendar();
                return;
            }
            // Ô hôm nay chuyển "Đã nhận", rồi icon quà bay về nút Thư.
            Show(day, cycle, Mode.Claimed);
            int cell = day - 1;
            if (cell >= 0 && cell < _cellIcons.Length)
                FlyRewards(_cellIcons[cell].rectTransform, farm.Definition.CheckIn.GetRewards(day, cycle));
        }

        private void Show(int today, int cycle, Mode mode)
        {
            Farm farm = _game.Farm;
            CheckInDef def = farm.Definition.CheckIn;
            if (!def.IsEnabled)
                return;

            bool claimed = mode == Mode.Claimed;
            _subtitle.text = UiFormat.CheckInSubtitle(today, ToFormat(mode));
            _claim.gameObject.SetActive(mode == Mode.Ready);
            _later.gameObject.SetActive(mode == Mode.Ready);
            _openMail.gameObject.SetActive(mode != Mode.Ready);
            _close.gameObject.SetActive(mode != Mode.Ready);

            for (int i = 0; i < _cellBackgrounds.Length && i < CheckInDef.DayCount; i++)
            {
                int day = i + 1;
                bool done = day < today || (claimed && day == today);
                bool highlight = day == today;
                _cellBackgrounds[i].color = highlight ? _todayColor : done ? _claimedColor : _futureColor;
                _cellDays[i].text = UiFormat.CheckInDayLabel(day, done, highlight, ToFormat(mode));

                List<Reward> rewards = def.GetRewards(day, cycle);
                _text.Clear();
                Sprite icon = null;
                for (int r = 0; r < rewards.Count; r++)
                {
                    if (r > 0)
                        _text.Append('\n');
                    _text.Append(RewardDisplay.ShortLabel(_game, rewards[r]));
                    // Icon: phần quà cuối có icon (ngày 7: cần câu thay vì xu).
                    Sprite candidate = RewardDisplay.Icon(_game, rewards[r], _coinIcon);
                    if (candidate != null)
                        icon = candidate;
                }
                _cellRewards[i].text = _text.ToString();
                _cellIcons[i].sprite = icon;
                _cellIcons[i].enabled = icon != null;
                _cellIcons[i].color = done && !highlight ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
            }
            _root.SetActive(true);
        }

        private static CheckInView ToFormat(Mode mode)
            => mode == Mode.Ready ? CheckInView.Ready : mode == Mode.Claimed ? CheckInView.Claimed : CheckInView.Waiting;

        // ---------- Bay về nút Thư ----------

        private void FlyRewards(RectTransform from, List<Reward> rewards)
        {
            if (_flyTemplate == null || _mailTarget == null)
                return;
            int launched = 0;
            foreach (Reward reward in rewards)
            {
                Sprite icon = RewardDisplay.Icon(_game, reward, _coinIcon);
                if (icon == null)
                    continue;
                Image fly = TakeFlyIcon();
                fly.sprite = icon;
                StartCoroutine(Fly(fly, from.position, launched * 0.12f, launched == 0));
                launched++;
            }
            // Không có icon nào (thiếu art): vẫn nảy nút Thư cho người chơi biết quà đã vào thư.
            if (launched == 0)
                StartCoroutine(Bounce(_mailTarget));
        }

        private Image TakeFlyIcon()
        {
            foreach (Image image in _flyPool)
            {
                if (!image.gameObject.activeSelf)
                    return image;
            }
            Image clone = Instantiate(_flyTemplate, _flyTemplate.transform.parent);
            _flyPool.Add(clone);
            return clone;
        }

        // Bay cong từ ô hôm nay tới nút Thư, nhỏ dần; con đầu tiên tới nơi thì nút Thư nảy.
        private IEnumerator Fly(Image fly, Vector3 from, float delay, bool bounceOnArrive)
        {
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);
            RectTransform rect = fly.rectTransform;
            rect.position = from;
            rect.localScale = Vector3.one;
            fly.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < _flySeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _flySeconds);
                Vector3 to = _mailTarget.position;
                Vector3 position = Vector3.Lerp(from, to, t * t);
                position.y += Mathf.Sin(t * Mathf.PI) * _flyArc * rect.lossyScale.y;
                rect.position = position;
                rect.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.6f, t);
                yield return null;
            }
            fly.gameObject.SetActive(false);
            if (bounceOnArrive)
                yield return Bounce(_mailTarget);
        }

        private IEnumerator Bounce(RectTransform target)
        {
            float elapsed = 0f;
            while (elapsed < _bounceSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _bounceSeconds);
                target.localScale = Vector3.one * (1f + 0.25f * Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            target.localScale = Vector3.one;
        }
    }
}
