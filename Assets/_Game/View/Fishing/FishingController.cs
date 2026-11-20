using System;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Character;
using VeQue.View.Data;
using VeQue.View.Map;
using VeQue.View.Save;
using VeQue.View.UI;

namespace VeQue.View.Fishing
{
    // Một lượt câu ở vùng câu (MVP-Plan Phần 10 giai đoạn 4, Đợt C): tới chỗ câu → giữ Ném (thanh lực) → nhả:
    // Farm.Cast + hoạt ảnh ném → phao ở đầu dây vẽ sẵn của art (cố định, độ sâu chỉ hiện bằng chữ) → phao rỉa,
    // chìm hẳn + "!" nảy → Giật (Strike) hoặc quá giờ (Timeout) → kéo cá: xem trước chuỗi mũi tên
    // (FishingTuning.PullPreviewSeconds, chưa tính giờ) rồi nhập đúng chuỗi trước khi hết giờ
    // (nhanh thì hạng cao; bấm sai = nhập lại từ đầu, giờ vẫn chạy; hết giờ = cá thoát);
    // bóng cá dưới nước lao theo mũi tên → CompleteFishing → về Sẵn sàng.
    // Phần thời gian thực ở đây (Time.time); chọn cá, rỉa, cắn, kết quả do FishingSession (Logic).
    // Bỏ lượt (mất mồi): Thu cần, chuyển vùng, ẩn tab. Đang câu thì chạm đất không đi.
    public sealed class FishingController : MonoBehaviour
    {
        public enum Phase
        {
            // Không đứng ở chỗ câu.
            Away,
            Ready,
            Charging,
            Casting,
            Waiting,
            // Đã móc cá: nhập chuỗi mũi tên (1 lượt).
            Pulling,
            // Câu được: chạy hoạt ảnh được cá rồi mới áp kết quả.
            Landing,
            // Vừa xong lượt: nghỉ chút cho người chơi đọc thông báo.
            Result,
        }

        private const float PixelsPerUnit = 16f;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private RegionTravel _travel;
        [SerializeField] private FarmCharacterController _character;
        [Tooltip("Phao (con của GameObject này). Pivot ở chỗ chạm nước.")]
        [SerializeField] private SpriteRenderer _bobber;
        [Tooltip("Dấu \"!\" khi cá cắn (con của GameObject này).")]
        [SerializeField] private SpriteRenderer _alert;
        [Tooltip("Bóng cá dưới nước lúc kéo (con của GameObject này).")]
        [SerializeField] private FishShadow _shadow;
        [Tooltip("Bóng cá hiện sau khi phao chạm nước bao lâu (giây, ngẫu nhiên trong khoảng); luôn trước lúc cá cắn.")]
        [SerializeField] private Vector2 _shadowAppearSeconds = new Vector2(0.5f, 1f);
        [Tooltip("Hình cá bay từ phao về tay khi câu được (con của GameObject này).")]
        [SerializeField] private CaughtFishFlyer _flyer;
        [Tooltip("Tay nhân vật (chỗ cá bay tới) so với chân, nhìn sang phải (pixel).")]
        [SerializeField] private Vector2Int _handPixels = new Vector2Int(8, 10);
        [Tooltip("Chữ nổi (Hoàn hảo! / Mất mồi!…) so với chân nhân vật (pixel).")]
        [SerializeField] private Vector2Int _headPixels = new Vector2Int(0, 26);
        [Tooltip("Đầu dây câu vẽ sẵn trong art WAITING so với chân nhân vật, nhìn sang phải (pixel).")]
        [SerializeField] private Vector2Int _lineEndPixels = new Vector2Int(30, -22);
        [Tooltip("Dấu \"!\" so với chân nhân vật (pixel).")]
        [SerializeField] private Vector2Int _alertPixels = new Vector2Int(0, 22);
        [SerializeField] private float _nibbleSeconds = 0.15f;
        [SerializeField] private int _nibbleDropPixels = 1;
        [Tooltip("Cá cắn: phao chìm bao nhiêu pixel (13/11/2026: 2 → 3 cho dễ thấy).")]
        [SerializeField] private int _biteSinkPixels = 3;
        [Tooltip("Dấu \"!\" nảy lên xuống (pixel, số lần / giây).")]
        [SerializeField] private int _alertBouncePixels = 1;
        [SerializeField] private float _alertBounceRate = 6f;
        [SerializeField] private int _vibrateMilliseconds = 120;
        [Header("Kéo cá – bấm sai (Đợt 2 Phần 6)")]
        [Tooltip("Phao giật ngược về phía nhân vật trong bao lâu (giây).")]
        [SerializeField] private float _jerkSeconds = 0.3f;
        [Tooltip("Phao giật ngược bao nhiêu pixel.")]
        [SerializeField] private float _jerkPixels = 4f;
        [SerializeField] private int _mistakeVibrateMilliseconds = 30;
        [SerializeField] private float _resultPauseSeconds = 0.6f;

        private Phase _phase;
        private FishingSpot _spot;
        private FishingSession _session;
        private float _phaseStart;
        private float _castEnd;
        private bool _biteShown;
        private string _baitId;
        private Vector3 _bobberBase;
        private float _jerkStart = -1f;
        private float _landEnd;
        // Kéo cá: lúc bắt đầu tính giờ (sau khung xem trước, hoặc lúc bấm đúng mũi tên đầu).
        private float _pullStart;
        private Vector3 _alertBase;
        // Lần rỉa đang diễn ra ở khung trước (để báo bóng cá đớp thử đúng 1 lần mỗi lần rỉa).
        private bool _wasNibbling;

        public Phase CurrentPhase => _phase;
        public FishingSession Session => _session;
        public string BaitId => _baitId;
        public bool InSession => _phase == Phase.Charging || _phase == Phase.Casting || _phase == Phase.Waiting
            || _phase == Phase.Pulling || _phase == Phase.Landing;

        // Kéo cá: đề, số mũi tên đã nhập đúng, giây đã dùng.
        public PullRound Pull => _phase == Phase.Pulling && _session != null ? _session.Pull : null;
        public int PullIndex { get; private set; }
        // Vị trí mũi tên vừa bấm sai (trước khi về đầu chuỗi) – bảng kéo cá rung ô này.
        public int LastMistakeIndex { get; private set; } = -1;
        public float PullElapsed => _phase == Phase.Pulling ? Mathf.Max(0f, Time.time - _pullStart) : 0f;
        // Kéo cá: đang xem trước chuỗi mũi tên (chưa tính giờ).
        public bool PullPreviewing => _phase == Phase.Pulling && Time.time < _pullStart;
        // Đang chờ và cá đã cắn (phao chìm, "!"): lúc này mới nên bấm Giật.
        public bool BiteShown => _phase == Phase.Waiting && _biteShown;
        public FishingTuning Tuning => _game.Farm.FishingDefinition.Tuning;

        // Lực ném hiện tại khi đang giữ nút (0..1).
        public float Power { get; private set; }

        // Phase hoặc mồi đổi (để HUD cập nhật).
        public event Action Changed;
        // Kéo cá: nhập đúng thêm 1 mũi tên.
        public event Action PullProgressed;
        // Kéo cá: bấm sai, chuỗi về mũi tên đầu (bảng kéo nháy đỏ).
        public event Action PullMistake;
        public event Action<string> Message;
        // Vừa ném: đánh giá cú ném (HUD hiện chữ trên đầu nhân vật).
        public event Action<CastQuality> CastJudged;
        // Lượt câu không ra cá (cá sợ, ăn mồi, trượt, thoát) – chữ trên đầu nhân vật. Thu cần / rời đi không báo ở đây.
        public event Action<FishingOutcome> Missed;
        // Câu được: phiên đã áp vào tiến trình (bảng cá vừa câu).
        public event Action<FishingSession> FishCaught;

        // Điểm đặt chữ nổi trên đầu nhân vật.
        public Vector3 HeadPosition
        {
            get
            {
                Vector3 feet = _character.Body.position;
                return new Vector3(feet.x + _headPixels.x / PixelsPerUnit, feet.y + _headPixels.y / PixelsPerUnit, 0f);
            }
        }

        public bool TryGetEquippedRod(out RodDef rod) => _game.Farm.TryGetEquippedRod(out rod, out _);

        // ---------- Gọi từ HUD ----------

        // Chạm xuống nút Ném / Giật.
        public void PressAction()
        {
            if (_phase == Phase.Ready)
                BeginCharge();
            else if (_phase == Phase.Waiting)
                Strike();
        }

        // Nhả nút Ném.
        public void ReleaseAction()
        {
            if (_phase == Phase.Charging)
                Cast();
        }

        // Kéo cá: bấm 1 mũi tên. Đúng thì sang mũi tên kế (hết chuỗi = câu được);
        // sai thì về mũi tên đầu, nhập lại (chốt 13/11/2026, phương án B – không đứt dây, giờ vẫn chạy).
        public void PressArrow(Arrow arrow)
        {
            PullRound pull = Pull;
            if (pull == null)
                return;

            if (arrow != pull.Expected(PullIndex))
            {
                _session.WrongArrow();
                LastMistakeIndex = PullIndex;
                PullIndex = 0;
                _jerkStart = Time.time;
                if (_shadow != null)
                    _shadow.Flinch();
                WebBridge.Vibrate(_mistakeVibrateMilliseconds);
                PullMistake?.Invoke();
                return;
            }

            // Bấm đúng trong lúc xem trước: bắt đầu tính giờ luôn.
            if (Time.time < _pullStart)
                _pullStart = Time.time;
            PullIndex++;
            // Bóng cá lao theo hướng mũi tên kế tiếp (chỉ là hình ảnh).
            if (_shadow != null)
                _shadow.Dart(PullIndex < pull.Count ? pull.Expected(PullIndex) : arrow);
            PullProgressed?.Invoke();
            if (PullIndex < pull.Count)
                return;

            if (_session.CompletePull(PullElapsed) == FishingOutcome.Caught)
            {
                // Cá bật khỏi nước ở chỗ phao, bay dọc dây về tay; bóng dưới nước tắt.
                if (_shadow != null)
                    _shadow.Hide();
                float flight = 0f;
                if (_flyer != null)
                {
                    FishingDefinition data = _game.Definition.Fishing;
                    _flyer.Launch(FishIcon(_session.Fish.Id), _bobberBase, HandPosition(),
                        data != null ? data.GetShiny(_session.Shiny) : null, _session.Grams);
                    flight = _flyer.Seconds;
                }
                _character.View.Play(CharacterAnim.Caught, _spot.Facing, true);
                _landEnd = Time.time + Mathf.Max(0.1f, Mathf.Max(flight, _character.View.GetDuration(CharacterAnim.Caught)));
                ShowBobber(false);
                SetPhase(Phase.Landing);
                return;
            }
            if (_shadow != null)
                _shadow.Escape();
            Finish();
        }

        // Thu cần: đang nạp lực thì chỉ hủy; đã ném thì bỏ lượt (mất mồi).
        public void ReelIn()
        {
            if (_phase == Phase.Charging)
            {
                SetPhase(Phase.Ready);
                return;
            }
            if (_phase == Phase.Casting || _phase == Phase.Waiting || _phase == Phase.Pulling)
            {
                AbandonSession();
                Message?.Invoke("Đã thu cần (mất mồi)");
                SetPhase(Phase.Ready);
            }
        }

        // Chọn mồi để ném (bảng Mồi). Đang câu hoặc hết loại đó thì bỏ qua.
        public void SelectBait(string baitId)
        {
            if (InSession || _game.Farm.State.Fishing.GetBaits(baitId) <= 0)
                return;
            _baitId = baitId;
            Changed?.Invoke();
        }

        public bool TryGetBait(out BaitDefinition bait)
        {
            bait = null;
            FishingDefinition data = _game.Definition.Fishing;
            return data != null && data.TryGetBait(_baitId, out bait);
        }

        // ---------- Vòng đời ----------

        private void Awake()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_travel == null)
                _travel = FindFirstObjectByType<RegionTravel>();
            if (_character == null)
                _character = FindFirstObjectByType<FarmCharacterController>();
            ShowBobber(false);
            ShowAlert(false);
            if (_shadow != null)
                _shadow.Hide();
            if (_flyer != null)
                _flyer.Hide();
        }

        private void OnEnable()
        {
            if (_travel != null)
            {
                _travel.SpotReached += HandleSpotReached;
                _travel.TravelStarted += HandleLeaving;
            }
            if (_game != null)
                _game.PageHiding += HandlePageHiding;
        }

        private void OnDisable()
        {
            if (_travel != null)
            {
                _travel.SpotReached -= HandleSpotReached;
                _travel.TravelStarted -= HandleLeaving;
            }
            if (_game != null)
                _game.PageHiding -= HandlePageHiding;
        }

        private void Update()
        {
            switch (_phase)
            {
                case Phase.Ready:
                    // Chạm đất chỗ khác → nhân vật đi khỏi chỗ câu.
                    if (_spot == null || !IsAtSpot())
                        SetPhase(Phase.Away);
                    break;
                case Phase.Charging:
                    if (TryGetEquippedRod(out RodDef rod))
                        Power = FishingMath.CastPower(Time.time - _phaseStart, rod.BarPeriodSeconds);
                    break;
                case Phase.Casting:
                    if (Time.time >= _castEnd)
                        BeginWaiting();
                    break;
                case Phase.Waiting:
                    UpdateWaiting(Time.time - _phaseStart);
                    break;
                case Phase.Pulling:
                    UpdatePulling(Time.time - _phaseStart);
                    break;
                case Phase.Landing:
                    if (Time.time >= _landEnd)
                        Finish();
                    break;
                case Phase.Result:
                    if (Time.time - _phaseStart >= _resultPauseSeconds)
                        SetPhase(IsAtSpot() ? Phase.Ready : Phase.Away);
                    break;
            }
        }

        private void HandleSpotReached(FishingSpot spot)
        {
            if (InSession)
                return;
            _spot = spot;
            _character.Face(spot.Facing);
            if (_baitId == null || _game.Farm.State.Fishing.GetBaits(_baitId) <= 0)
                _baitId = FindBait(_baitId, false) ?? _baitId;
            SetPhase(Phase.Ready);
        }

        // Bấm Về nhà / đi qua cổng: bỏ lượt rồi mới đi (đang hoạt ảnh được cá thì vẫn nhận cá).
        private void HandleLeaving()
        {
            if (_phase == Phase.Landing)
                Finish();
            else if (InSession)
            {
                AbandonSession();
                Message?.Invoke("Đã thu cần (mất mồi)");
            }
            _spot = null;
            SetPhase(Phase.Away);
        }

        // Ẩn tab: chốt lượt đang dở trước khi lưu (mất mồi).
        private void HandlePageHiding()
        {
            if (!InSession)
                return;
            if (_phase == Phase.Landing)
                Finish();
            else
                AbandonSession();
            SetPhase(Phase.Ready);
        }

        // ---------- Các bước ----------

        private void BeginCharge()
        {
            FarmResult check = _game.Farm.CanCast(_baitId);
            if (check != FarmResult.Ok)
            {
                // Hết loại mồi đang chọn nhưng còn loại khác: tự đổi rồi thử lại.
                if (check == FarmResult.NotEnoughBait || check == FarmResult.UnknownBait)
                {
                    string other = FindBait(_baitId, false);
                    if (other != null && other != _baitId)
                    {
                        _baitId = other;
                        Changed?.Invoke();
                        check = _game.Farm.CanCast(_baitId);
                    }
                }
                if (check != FarmResult.Ok)
                {
                    Message?.Invoke(UiFormat.CastBlocked(check));
                    return;
                }
            }
            Power = 0f;
            SetPhase(Phase.Charging);
        }

        private void Cast()
        {
            float power = Power;
            FarmResult result = _game.Farm.Cast(_baitId, power, out FishingSession session);
            if (result != FarmResult.Ok)
            {
                Message?.Invoke(UiFormat.CastBlocked(result));
                SetPhase(Phase.Ready);
                return;
            }

            _session = session;
            CastJudged?.Invoke(session.Quality);
            _character.View.Play(CharacterAnim.Cast, _spot.Facing, true);
            _castEnd = Time.time + Mathf.Max(0.1f, _character.View.GetDuration(CharacterAnim.Cast));
            SetPhase(Phase.Casting);
        }

        private void BeginWaiting()
        {
            _character.View.Play(CharacterAnim.Wait, _spot.Facing, true);
            _bobberBase = LineEnd();
            _biteShown = false;
            _wasNibbling = false;
            ShowBobber(true);
            if (_shadow != null)
            {
                // Hiện trước lần rỉa / cắn đầu tiên để người chơi kịp thấy cá tới.
                float delay = UnityEngine.Random.Range(_shadowAppearSeconds.x, _shadowAppearSeconds.y);
                float first = _session.BiteTime;
                foreach (float nibble in _session.NibbleTimes)
                    first = Mathf.Min(first, nibble);
                delay = Mathf.Clamp(delay, 0f, Mathf.Max(0f, first - 0.3f));
                _shadow.Approach(_bobberBase, _session.Fish.Rarity, _spot.Facing == Facing.Left, delay);
            }
            SetPhase(Phase.Waiting);
        }

        private void UpdateWaiting(float t)
        {
            if (t > _session.HookDeadline)
            {
                _session.Timeout();
                Finish();
                return;
            }

            int drop = 0;
            if (t >= _session.BiteTime)
            {
                drop = _biteSinkPixels;
                if (!_biteShown)
                {
                    _biteShown = true;
                    ShowAlert(true);
                    WebBridge.Vibrate(_vibrateMilliseconds);
                    if (_shadow != null)
                        _shadow.Bite();
                    // HUD đổi nút "Chờ…" → "GIẬT!".
                    Changed?.Invoke();
                }
                int bounce = Mathf.FloorToInt((t - _session.BiteTime) * _alertBounceRate * 2f) % 2 == 0 ? _alertBouncePixels : 0;
                _alert.transform.position = _alertBase + new Vector3(0f, bounce / PixelsPerUnit, 0f);
            }
            else if (FishingMath.IsNibbling(_session, t, _nibbleSeconds))
            {
                drop = _nibbleDropPixels;
                if (!_wasNibbling && _shadow != null)
                    _shadow.Nibble();
                _wasNibbling = true;
            }
            else
            {
                _wasNibbling = false;
            }
            _bobber.transform.position = _bobberBase + new Vector3(0f, -drop / PixelsPerUnit, 0f);
        }

        private void Strike()
        {
            FishingOutcome outcome = _session.Strike(Time.time - _phaseStart);
            if (outcome == FishingOutcome.None)
            {
                // Móc được cá: kéo (nhập chuỗi mũi tên).
                ShowAlert(false);
                PullIndex = 0;
                _character.View.Play(CharacterAnim.Reel, _spot.Facing, true);
                LastMistakeIndex = -1;
                _jerkStart = -1f;
                if (_shadow != null)
                {
                    _shadow.Hook();
                    _shadow.Dart(_session.Pull.Expected(0));
                }
                // Đặt trước SetPhase để HUD đọc đúng trạng thái xem trước.
                _pullStart = Time.time + Mathf.Max(0f, Tuning.PullPreviewSeconds);
                SetPhase(Phase.Pulling);
                return;
            }
            Finish();
        }

        // Phao giật qua lại 1 px trong lúc kéo; hết giờ (tính sau khung xem trước) thì cá thoát.
        private void UpdatePulling(float t)
        {
            if (PullElapsed > _session.Pull.TimeLimitSeconds)
            {
                _session.PullTimeout();
                if (_shadow != null)
                    _shadow.Escape();
                Finish();
                return;
            }
            if (_shadow != null)
                _shadow.SetTension(PullElapsed / _session.Pull.TimeLimitSeconds);
            int jitter = Mathf.FloorToInt(t * 10f) % 2 == 0 ? 1 : -1;
            Vector3 position = _bobberBase + new Vector3(jitter / PixelsPerUnit, -_biteSinkPixels / PixelsPerUnit, 0f);
            // Bấm sai: phao giật về phía nhân vật rồi trả lại trong _jerkSeconds.
            if (_jerkStart >= 0f && _jerkSeconds > 0f && Time.time - _jerkStart < _jerkSeconds)
            {
                float k = 1f - (Time.time - _jerkStart) / _jerkSeconds;
                Vector3 toCharacter = _character.Body.position - _bobberBase;
                toCharacter.z = 0f;
                if (toCharacter.sqrMagnitude > 0.0001f)
                    position += toCharacter.normalized * (Mathf.Round(_jerkPixels * k) / PixelsPerUnit);
            }
            _bobber.transform.position = position;
        }

        // Áp kết quả lượt câu vào tiến trình, về tư thế đứng.
        private void Finish(bool announce = true)
        {
            FishingSession session = _session;
            FishingOutcome outcome = session.Outcome;
            _game.Farm.CompleteFishing(session);
            _session = null;
            ShowBobber(false);
            ShowAlert(false);
            _character.View.Play(CharacterAnim.Idle, _spot != null ? _spot.Facing : _character.Facing, true);
            if (_flyer != null)
                _flyer.Hide();
            // Hụt (sợ, ăn mồi, trượt, thoát): bóng quay đầu bơi đi. Câu được thì bóng đã tắt lúc cá bay lên.
            if (_shadow != null && outcome != FishingOutcome.Caught)
                _shadow.Escape();
            SetPhase(Phase.Result);
            // Sau SetPhase để bảng / chữ nổi mở trên trạng thái đã xong lượt.
            if (announce)
            {
                if (outcome == FishingOutcome.Caught)
                    FishCaught?.Invoke(session);
                else
                    Missed?.Invoke(outcome);
            }
        }

        private void AbandonSession()
        {
            if (_session == null)
                return;
            if (_shadow != null)
                _shadow.Escape();
            _session.Abandon();
            Finish(false);
        }

        private void SetPhase(Phase phase)
        {
            _phase = phase;
            _phaseStart = Time.time;
            if (_character != null)
                _character.InputLocked = InSession;
            if (phase == Phase.Away)
            {
                _spot = null;
                ShowBobber(false);
                ShowAlert(false);
            }
            Changed?.Invoke();
        }

        // ---------- Hỗ trợ ----------

        private bool IsAtSpot()
        {
            if (_spot == null)
                return false;
            Vector3 feet = _character.Body.position;
            GridCell stand = _spot.StandCell;
            return Mathf.FloorToInt(feet.x) == stand.X && Mathf.FloorToInt(feet.y) == stand.Y;
        }

        // Đầu dây câu (chỗ đặt phao): art nhìn phải, quay trái thì lật ngang.
        private Vector3 LineEnd()
        {
            Vector3 feet = _character.Body.position;
            float dx = _lineEndPixels.x / PixelsPerUnit;
            if (_spot.Facing == Facing.Left)
                dx = -dx;
            return new Vector3(feet.x + dx, feet.y + _lineEndPixels.y / PixelsPerUnit, 0f);
        }

        private void ShowBobber(bool show)
        {
            if (_bobber != null)
                _bobber.gameObject.SetActive(show);
        }

        private void ShowAlert(bool show)
        {
            if (_alert == null)
                return;
            _alert.gameObject.SetActive(show);
            if (show)
            {
                Vector3 feet = _character.Body.position;
                _alertBase = new Vector3(feet.x + _alertPixels.x / PixelsPerUnit, feet.y + _alertPixels.y / PixelsPerUnit, 0f);
                _alert.transform.position = _alertBase;
            }
        }

        // Mồi đang có: next = true thì lấy loại sau current (vòng lại), false thì ưu tiên current.
        private string FindBait(string current, bool next)
        {
            FishingDef def = _game.Farm.FishingDefinition;
            FishingState state = _game.Farm.State.Fishing;
            int count = def.Baits.Count;
            if (count == 0)
                return null;

            int start = 0;
            for (int i = 0; i < count; i++)
            {
                if (def.Baits[i].Id == current)
                {
                    start = next ? i + 1 : i;
                    break;
                }
            }
            for (int k = 0; k < count; k++)
            {
                string id = def.Baits[(start + k) % count].Id;
                if (state.GetBaits(id) > 0)
                    return id;
            }
            return null;
        }

        private Sprite FishIcon(string fishId)
        {
            FishingDefinition data = _game.Definition.Fishing;
            return data != null && data.TryGetFish(fishId, out FishDefinition fish) ? fish.Icon : null;
        }

        // Tay nhân vật: art nhìn phải, quay trái thì lật ngang.
        private Vector3 HandPosition()
        {
            Vector3 feet = _character.Body.position;
            float dx = _handPixels.x / PixelsPerUnit;
            if (_spot.Facing == Facing.Left)
                dx = -dx;
            return new Vector3(feet.x + dx, feet.y + _handPixels.y / PixelsPerUnit, 0f);
        }
    }
}
