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
    // chìm + "!" → Giật (Strike) hoặc quá giờ (Timeout) → kéo cá: nhập đúng chuỗi mũi tên trước khi hết giờ
    // (nhanh thì hạng cao; sai = đứt dây, hết giờ = cá thoát) → CompleteFishing → về Sẵn sàng.
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
        [Tooltip("Đầu dây câu vẽ sẵn trong art WAITING so với chân nhân vật, nhìn sang phải (pixel).")]
        [SerializeField] private Vector2Int _lineEndPixels = new Vector2Int(30, -22);
        [Tooltip("Dấu \"!\" so với chân nhân vật (pixel).")]
        [SerializeField] private Vector2Int _alertPixels = new Vector2Int(0, 22);
        [SerializeField] private float _nibbleSeconds = 0.15f;
        [SerializeField] private int _nibbleDropPixels = 1;
        [SerializeField] private int _biteDropPixels = 2;
        [SerializeField] private int _vibrateMilliseconds = 120;
        [SerializeField] private float _resultPauseSeconds = 0.6f;

        private Phase _phase;
        private FishingSpot _spot;
        private FishingSession _session;
        private float _phaseStart;
        private float _castEnd;
        private bool _biteShown;
        private string _baitId;
        private Vector3 _bobberBase;
        private float _landEnd;

        public Phase CurrentPhase => _phase;
        public FishingSession Session => _session;
        public string BaitId => _baitId;
        public bool InSession => _phase == Phase.Charging || _phase == Phase.Casting || _phase == Phase.Waiting
            || _phase == Phase.Pulling || _phase == Phase.Landing;

        // Kéo cá: đề, số mũi tên đã nhập đúng, giây đã dùng.
        public PullRound Pull => _phase == Phase.Pulling && _session != null ? _session.Pull : null;
        public int PullIndex { get; private set; }
        public float PullElapsed => _phase == Phase.Pulling ? Time.time - _phaseStart : 0f;
        public FishingTuning Tuning => _game.Farm.FishingDefinition.Tuning;

        // Lực ném hiện tại khi đang giữ nút (0..1).
        public float Power { get; private set; }

        // Phase hoặc mồi đổi (để HUD cập nhật).
        public event Action Changed;
        // Kéo cá: nhập đúng thêm 1 mũi tên.
        public event Action PullProgressed;
        public event Action<string> Message;

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

        // Kéo cá: bấm 1 mũi tên. Đúng thì sang mũi tên kế (hết chuỗi = câu được); sai thì đứt dây.
        public void PressArrow(Arrow arrow)
        {
            PullRound pull = Pull;
            if (pull == null)
                return;

            if (arrow != pull.Expected(PullIndex))
            {
                _session.WrongArrow();
                Finish();
                return;
            }

            PullIndex++;
            PullProgressed?.Invoke();
            if (PullIndex < pull.Count)
                return;

            if (_session.CompletePull(Time.time - _phaseStart) == FishingOutcome.Caught)
            {
                _character.View.Play(CharacterAnim.Caught, _spot.Facing, true);
                _landEnd = Time.time + Mathf.Max(0.1f, _character.View.GetDuration(CharacterAnim.Caught));
                ShowBobber(false);
                SetPhase(Phase.Landing);
                return;
            }
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

        // Đổi sang loại mồi kế tiếp đang có.
        public void CycleBait()
        {
            if (InSession)
                return;
            string next = FindBait(_baitId, true);
            if (next == null)
            {
                Message?.Invoke("Hết mồi rồi");
                return;
            }
            _baitId = next;
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
            Message?.Invoke(UiFormat.CastResult(session.Quality, session.Depth));
            _character.View.Play(CharacterAnim.Cast, _spot.Facing, true);
            _castEnd = Time.time + Mathf.Max(0.1f, _character.View.GetDuration(CharacterAnim.Cast));
            SetPhase(Phase.Casting);
        }

        private void BeginWaiting()
        {
            _character.View.Play(CharacterAnim.Wait, _spot.Facing, true);
            _bobberBase = LineEnd();
            _biteShown = false;
            ShowBobber(true);
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
                drop = _biteDropPixels;
                if (!_biteShown)
                {
                    _biteShown = true;
                    ShowAlert(true);
                    WebBridge.Vibrate(_vibrateMilliseconds);
                }
            }
            else if (FishingMath.IsNibbling(_session, t, _nibbleSeconds))
            {
                drop = _nibbleDropPixels;
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
                SetPhase(Phase.Pulling);
                return;
            }
            Finish();
        }

        // Phao giật qua lại 1 px trong lúc kéo; hết giờ thì cá thoát.
        private void UpdatePulling(float t)
        {
            if (t > _session.Pull.TimeLimitSeconds)
            {
                _session.PullTimeout();
                Finish();
                return;
            }
            int jitter = Mathf.FloorToInt(t * 10f) % 2 == 0 ? 1 : -1;
            _bobber.transform.position = _bobberBase + new Vector3(jitter / PixelsPerUnit, -_biteDropPixels / PixelsPerUnit, 0f);
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
            if (announce)
            {
                string text = outcome == FishingOutcome.Caught
                    ? UiFormat.CaughtText(FishName(session.Fish.Id), session.Grams, session.Grade, session.Xp)
                    : UiFormat.FishingOutcomeText(outcome);
                // Đứt dây làm gãy cần thì báo luôn.
                if (outcome == FishingOutcome.LineSnapped
                    && _game.Farm.State.Fishing.TryGetRod(session.Rod.Id, out RodState rod) && rod.Snapped)
                    text += " Cần gãy!";
                if (!string.IsNullOrEmpty(text))
                    Message?.Invoke(text);
            }
            SetPhase(Phase.Result);
        }

        private void AbandonSession()
        {
            if (_session == null)
                return;
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
                _alert.transform.position = new Vector3(feet.x + _alertPixels.x / PixelsPerUnit, feet.y + _alertPixels.y / PixelsPerUnit, 0f);
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

        private string FishName(string fishId)
        {
            FishingDefinition data = _game.Definition.Fishing;
            return data != null && data.TryGetFish(fishId, out FishDefinition fish) ? fish.DisplayName : fishId;
        }
    }
}
