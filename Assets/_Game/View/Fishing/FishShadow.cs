using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Fishing
{
    // Bóng cá dưới nước (duyệt 13/11/2026), FishingController điều khiển theo từng bước của lượt câu:
    // - Approach: phao chạm nước được 0,5–1 giây thì bóng mờ dần hiện ra, lượn qua lại phía nước xa nhân vật như đang xem xét.
    // - Nibble: đớp thử – lao nhanh vào phao rồi lùi lại (khớp lúc phao rỉa).
    // - Bite: cắn thật – lao vào phao và giữ ở đó.
    // - Hook (kéo cá): ghì ra xa nhân vật, rung / vùng liên tục, càng gần hết giờ càng mạnh (SetTension);
    //   bấm đúng mũi tên → lao theo hướng đó (Dart); bấm sai → vùng mạnh (Flinch).
    // - Reel: câu được → bị kéo về phía nhân vật; Escape: hụt / thoát / thu cần → quay đầu bơi đi, mờ dần.
    // Vị trí làm tròn theo pixel. Sprite theo thứ tự Lên, Xuống, Trái, Phải.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FishShadow : MonoBehaviour
    {
        private const float PixelsPerUnit = 16f;

        private enum Mode
        {
            Hidden,
            // Đã ném, chờ hiện.
            Waiting,
            Circling,
            Biting,
            Struggling,
            Escaping,
            Reeling,
        }

        [Tooltip("Bóng nhỏ (6×3 px).")]
        [SerializeField] private Sprite[] _smallSprites = new Sprite[4];
        [Tooltip("Bóng vừa (9×5 px).")]
        [SerializeField] private Sprite[] _mediumSprites = new Sprite[4];
        [Tooltip("Từ độ hiếm này trở lên dùng bóng vừa (mặc định Thường = mọi cá dùng bóng vừa; bóng nhỏ quá khó thấy).")]
        [SerializeField] private FishRarity _mediumFrom = FishRarity.Common;
        [SerializeField] private Color _color = new Color(0.05f, 0.1f, 0.2f, 0.6f);
        [SerializeField] private float _fadeInSeconds = 0.4f;

        [Header("Xem xét (lượn quanh phao)")]
        [Tooltip("Bán kính lượn (pixel): ngang, dọc. Chỉ lượn phía nước xa nhân vật.")]
        [SerializeField] private Vector2 _circleRadiusPixels = new Vector2(9f, 5f);
        [Tooltip("Góc lượn tối đa hai bên (độ) quanh hướng xa nhân vật.")]
        [Range(10f, 90f)] [SerializeField] private float _circleArcDegrees = 75f;
        [Tooltip("Tốc độ lượn (vòng qua lại mỗi giây, rad/giây).")]
        [SerializeField] private float _circleSpeed = 1.2f;

        [Header("Đớp thử / cắn")]
        [Tooltip("Đầu bóng cách phao bao nhiêu pixel lúc đớp / cắn.")]
        [SerializeField] private float _mouthGapPixels = 4f;
        [SerializeField] private float _nibbleSeconds = 0.35f;
        [SerializeField] private float _biteSeconds = 0.12f;

        [Header("Kéo cá (căng)")]
        [Tooltip("Chỗ bóng so với phao lúc kéo (pixel), khi nhân vật nhìn phải; nhìn trái thì lật x. x dương = xa nhân vật.")]
        [SerializeField] private Vector2Int _waterOffsetPixels = new Vector2Int(9, -2);
        [Tooltip("Ghì ra xa (pixel) và rung dọc theo dây / ngang dây (pixel) lúc căng thường; nhân thêm theo độ căng.")]
        [SerializeField] private float _pullPixels = 2f;
        [SerializeField] private float _shakeAlongPixels = 1.5f;
        [SerializeField] private float _shakeAcrossPixels = 1.5f;
        [SerializeField] private float _shakeSpeed = 22f;
        [Tooltip("Gần hết giờ (độ căng 1) thì rung mạnh gấp mấy lần.")]
        [SerializeField] private float _maxTensionMultiplier = 2.2f;
        [Tooltip("Bấm mũi tên: lao bao nhiêu pixel theo hướng đó.")]
        [SerializeField] private float _dartPixels = 4f;
        [SerializeField] private float _dartReturnSeconds = 0.25f;
        [Tooltip("Bấm sai: vùng ra xa bao nhiêu pixel, trong bao lâu.")]
        [SerializeField] private float _flinchPixels = 6f;
        [SerializeField] private float _flinchSeconds = 0.35f;

        [Header("Kết thúc")]
        [SerializeField] private float _escapePixels = 14f;
        [SerializeField] private float _escapeSeconds = 0.5f;
        [SerializeField] private float _reelSeconds = 0.35f;

        private SpriteRenderer _renderer;
        private Sprite[] _sprites;
        private Mode _mode = Mode.Hidden;
        private Vector3 _bobber;
        private Vector3 _home;
        private Vector3 _target;
        private Vector3 _from;
        private Vector2 _dart;
        private Vector2 _escapeDir;
        private float _modeStart;
        private float _appearAt;
        private float _shownAt;
        private float _nibbleStart = -1f;
        private float _dartStart = -1f;
        private float _dartSeconds;
        private float _tension;
        private float _alpha;
        // Độ đậm lúc bắt đầu bơi đi / bị kéo vào (mờ dần từ mức này).
        private float _fadeFrom;
        // Hướng ra xa nhân vật (phải = 1, trái = -1).
        private float _away = 1f;

        // GameObject để tắt sẵn trong scene (Tạo UI làm việc này); Approach bật lên.
        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        // ---------- Gọi từ FishingController ----------

        // Phao vừa chạm nước: sau appearDelay giây bóng hiện ra và lượn quanh phao.
        public void Approach(Vector3 bobber, FishRarity rarity, bool facingLeft, float appearDelay)
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            _sprites = rarity >= _mediumFrom ? _mediumSprites : _smallSprites;
            _away = facingLeft ? -1f : 1f;
            _bobber = bobber;
            _home = bobber + new Vector3(_away * _waterOffsetPixels.x / PixelsPerUnit, _waterOffsetPixels.y / PixelsPerUnit, 0f);
            _appearAt = Time.time + Mathf.Max(0f, appearDelay);
            _nibbleStart = -1f;
            _dartStart = -1f;
            _tension = 0f;
            _alpha = 0f;
            _renderer.color = new Color(0f, 0f, 0f, 0f);
            gameObject.SetActive(true);
            SetMode(Mode.Waiting);
        }

        // Phao rỉa: đớp thử 1 lần.
        public void Nibble()
        {
            EnsureShown();
            if (_mode == Mode.Circling)
                _nibbleStart = Time.time;
        }

        // Cá cắn thật: lao vào phao và giữ.
        public void Bite()
        {
            EnsureShown();
            if (_mode != Mode.Circling)
                return;
            _from = transform.position;
            SetFacing(TowardBobber);
            SetMode(Mode.Biting);
        }

        // Móc được cá: chuyển sang ghì / vùng liên tục.
        public void Hook()
        {
            EnsureShown();
            if (_mode == Mode.Hidden || _mode == Mode.Escaping || _mode == Mode.Reeling)
                return;
            _from = transform.position;
            _dartStart = -1f;
            _tension = 0f;
            SetFacing(TowardBobber);
            SetMode(Mode.Struggling);
        }

        // Độ căng 0..1 (phần thời gian kéo đã dùng): càng cao rung càng mạnh.
        public void SetTension(float tension) => _tension = Mathf.Clamp01(tension);

        // Bấm đúng 1 mũi tên.
        public void Dart(Arrow arrow)
        {
            if (_mode != Mode.Struggling)
                return;
            _dart = Direction(arrow) * _dartPixels;
            _dartStart = Time.time;
            _dartSeconds = _dartReturnSeconds;
        }

        // Bấm sai: cá vùng ra xa nhân vật rồi về (vẫn đang kéo).
        public void Flinch()
        {
            if (_mode != Mode.Struggling)
                return;
            _dart = Direction(AwayFromBobber) * _flinchPixels;
            _dartStart = Time.time;
            _dartSeconds = _flinchSeconds;
        }

        // Hụt / thoát / thu cần: quay đầu ra xa phao rồi bơi đi. Chưa hiện thì tắt luôn; đang bơi đi thì giữ nguyên.
        public void Escape()
        {
            if (_mode == Mode.Escaping || _mode == Mode.Reeling || _mode == Mode.Hidden)
                return;
            if (_mode == Mode.Waiting)
            {
                Hide();
                return;
            }
            _from = transform.position;
            _fadeFrom = _alpha;
            SetFacing(AwayFromBobber);
            _escapeDir = Direction(AwayFromBobber);
            SetMode(Mode.Escaping);
        }

        // Câu được: bóng bị kéo về phía chân nhân vật rồi biến mất.
        public void Reel(Vector3 towards)
        {
            if (_mode == Mode.Hidden || _mode == Mode.Waiting)
            {
                Hide();
                return;
            }
            _from = transform.position;
            _fadeFrom = _alpha;
            _target = towards;
            SetFacing(towards.x < _from.x ? Arrow.Left : Arrow.Right);
            SetMode(Mode.Reeling);
        }

        public void Hide()
        {
            _mode = Mode.Hidden;
            gameObject.SetActive(false);
        }

        // ---------- Vòng cập nhật ----------

        private void Update()
        {
            float now = Time.time;
            float t = now - _modeStart;
            switch (_mode)
            {
                case Mode.Waiting:
                    if (now >= _appearAt)
                        EnsureShown();
                    break;
                case Mode.Circling:
                    UpdateCircling(now);
                    break;
                case Mode.Biting:
                {
                    float k = Mathf.Clamp01(t / Mathf.Max(0.01f, _biteSeconds));
                    // Giữ sát phao, rung nhẹ 1 px như đang giằng mồi.
                    Vector3 mouth = MouthPosition();
                    Vector3 pos = Vector3.Lerp(_from, mouth, k);
                    if (k >= 1f)
                        pos.x += (Mathf.FloorToInt(t * 12f) % 2 == 0 ? 1f : 0f) * _away / PixelsPerUnit;
                    SetPosition(pos);
                    SetAlpha(FadeIn(now));
                    break;
                }
                case Mode.Struggling:
                    UpdateStruggling(now, t);
                    break;
                case Mode.Escaping:
                {
                    float k = Mathf.Clamp01(t / _escapeSeconds);
                    SetPosition(_from + (Vector3)(_escapeDir * (_escapePixels * k * k / PixelsPerUnit)));
                    SetAlpha(_fadeFrom * (1f - k));
                    if (k >= 1f)
                        Hide();
                    break;
                }
                case Mode.Reeling:
                {
                    float k = Mathf.Clamp01(t / _reelSeconds);
                    SetPosition(Vector3.Lerp(_from, _target, k * k));
                    SetAlpha(_fadeFrom * (1f - k));
                    if (k >= 1f)
                        Hide();
                    break;
                }
            }
        }

        // Lượn qua lại theo cung phía xa nhân vật; đang đớp thử thì lao vào phao rồi lùi lại.
        private void UpdateCircling(float now)
        {
            float time = now - _shownAt;
            float arc = _circleArcDegrees * Mathf.Deg2Rad;
            float angle = arc * Mathf.Sin(time * _circleSpeed);
            Vector3 circle = _bobber + new Vector3(
                _away * Mathf.Cos(angle) * _circleRadiusPixels.x / PixelsPerUnit,
                Mathf.Sin(angle) * _circleRadiusPixels.y / PixelsPerUnit, 0f);

            // Hướng bơi theo đạo hàm của cung (đi lên / xuống là chính, đổi chiều ở hai đầu cung).
            float dAngle = Mathf.Cos(time * _circleSpeed);
            Vector2 velocity = new Vector2(-_away * Mathf.Sin(angle) * _circleRadiusPixels.x, Mathf.Cos(angle) * _circleRadiusPixels.y) * dAngle;

            Vector3 pos = circle;
            if (_nibbleStart >= 0f)
            {
                float k = (now - _nibbleStart) / Mathf.Max(0.01f, _nibbleSeconds);
                if (k >= 1f)
                {
                    _nibbleStart = -1f;
                }
                else
                {
                    // Lao vào nhanh (40% đầu), lùi ra chậm.
                    float amount = k < 0.4f ? k / 0.4f : 1f - (k - 0.4f) / 0.6f;
                    pos = Vector3.Lerp(circle, MouthPosition(), amount);
                    SetFacing(TowardBobber);
                    SetPosition(pos);
                    SetAlpha(FadeIn(now));
                    return;
                }
            }
            if (velocity.sqrMagnitude > 0.0001f)
                SetFacing(Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y)
                    ? (velocity.x < 0f ? Arrow.Left : Arrow.Right)
                    : (velocity.y < 0f ? Arrow.Down : Arrow.Up));
            SetPosition(pos);
            SetAlpha(FadeIn(now));
        }

        // Ghì ra xa nhân vật + rung dọc / ngang dây liên tục; độ căng nhân biên độ; cú lao (Dart / Flinch) cộng thêm.
        private void UpdateStruggling(float now, float t)
        {
            float strength = Mathf.Lerp(1f, _maxTensionMultiplier, _tension);
            float along = _pullPixels + Mathf.Sin(t * _shakeSpeed) * _shakeAlongPixels * strength;
            float across = Mathf.Sin(t * _shakeSpeed * 1.7f + 1.3f) * _shakeAcrossPixels * strength;
            Vector2 offset = new Vector2(_away * along, across);

            if (_dartStart >= 0f)
            {
                float k = _dartSeconds > 0f ? Mathf.Clamp01((now - _dartStart) / _dartSeconds) : 1f;
                // Lao nhanh (30% đầu), trôi về chậm.
                float amount = k < 0.3f ? k / 0.3f : 1f - (k - 0.3f) / 0.7f;
                offset += _dart * amount;
                if (k >= 1f)
                    _dartStart = -1f;
            }

            // Vừa móc: trượt từ chỗ cũ về chỗ ghì trong 0,15 giây.
            Vector3 struggle = _home + new Vector3(offset.x / PixelsPerUnit, offset.y / PixelsPerUnit, 0f);
            float settle = Mathf.Clamp01(t / 0.15f);
            SetPosition(Vector3.Lerp(_from, struggle, settle));
            SetAlpha(1f);
        }

        // ---------- Hỗ trợ ----------

        private void EnsureShown()
        {
            if (_mode != Mode.Waiting)
                return;
            _shownAt = Time.time;
            SetFacing(TowardBobber);
            SetMode(Mode.Circling);
        }

        private float FadeIn(float now) => _fadeInSeconds > 0f ? Mathf.Clamp01((now - _shownAt) / _fadeInSeconds) : 1f;

        // Chỗ đặt tâm bóng khi đầu chạm mồi (phía xa nhân vật của phao).
        private Vector3 MouthPosition() => _bobber + new Vector3(_away * _mouthGapPixels / PixelsPerUnit, -1f / PixelsPerUnit, 0f);

        // Làm tròn theo pixel để không nhòe.
        private void SetPosition(Vector3 pos)
        {
            transform.position = new Vector3(
                Mathf.Round(pos.x * PixelsPerUnit) / PixelsPerUnit,
                Mathf.Round(pos.y * PixelsPerUnit) / PixelsPerUnit,
                pos.z);
        }

        private void SetAlpha(float alpha)
        {
            _alpha = alpha;
            Color c = _color;
            c.a *= alpha;
            _renderer.color = c;
        }

        // Bóng nằm phía xa nhân vật so với phao (_away), nên hướng về phao là ngược lại.
        private Arrow TowardBobber => _away > 0f ? Arrow.Left : Arrow.Right;
        private Arrow AwayFromBobber => _away > 0f ? Arrow.Right : Arrow.Left;

        private void SetFacing(Arrow arrow)
        {
            if (_sprites != null && (int)arrow < _sprites.Length && _sprites[(int)arrow] != null)
                _renderer.sprite = _sprites[(int)arrow];
        }

        private void SetMode(Mode mode)
        {
            _mode = mode;
            _modeStart = Time.time;
        }

        private static Vector2 Direction(Arrow arrow)
        {
            switch (arrow)
            {
                case Arrow.Up: return Vector2.up;
                case Arrow.Down: return Vector2.down;
                case Arrow.Left: return Vector2.left;
                default: return Vector2.right;
            }
        }
    }
}
