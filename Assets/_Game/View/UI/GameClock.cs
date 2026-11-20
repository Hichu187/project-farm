using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Đồng hồ chung trên HUD (cả nông trại và vùng câu):
    // - Giờ TRONG GAME hh:mm (GameDayClock qua DayNightController – 1 ngày game ≈ 30 phút thật, chốt 21/11/2026),
    //   chỉ để nhìn ngày đêm; icon mặt trời (6–18h game) / mặt trăng thay cho đồng hồ bấm giờ để khỏi nhầm với giờ thật.
    // - 21/11/2026: không hiện âm lịch (người chơi tự xem lịch ngoài đời); âm lịch vẫn dùng cho logic sự kiện.
    // - Chạm: đổi ngày đêm Đầy đủ → Nhẹ → Tắt (Đợt 2 Phần 13), báo bằng toast.
    public sealed class GameClock : MonoBehaviour
    {
        private const float RefreshSeconds = 0.25f;
        private const int IconSize = 12;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private DayNightController _dayNight;
        [SerializeField] private TMP_Text _time;
        [SerializeField] private Image _icon;
        [Tooltip("Để trống thì dùng icon pixel sinh bằng code.")]
        [SerializeField] private Sprite _sunIcon;
        [SerializeField] private Sprite _moonIcon;

        [SerializeField] private Button _button;
        [SerializeField] private Toast _toast;

        private readonly GameDayClock _fallbackClock = new GameDayClock();
        private int _minute = -1;
        private int _daytime = -1;
        private float _nextRefresh;
        private static Sprite _generatedSun;
        private static Sprite _generatedMoon;

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_dayNight == null)
                _dayNight = FindFirstObjectByType<DayNightController>();
            if (_toast == null)
                _toast = FindFirstObjectByType<Toast>();
            if (_button != null)
                _button.onClick.AddListener(CycleDayNight);
            Refresh();
        }

        private void CycleDayNight()
        {
            DayNightMode mode = DayNightSetting.Cycle();
            if (_toast != null)
                _toast.Show(DayNightSetting.Label(mode));
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
                return;
            _nextRefresh = Time.unscaledTime + RefreshSeconds;
            Refresh();
        }

        private void Refresh()
        {
            if (_game == null || _game.Farm == null)
                return;

            // 1 phút game ≈ 1,25 giây thật: chỉ dựng lại chuỗi khi đổi phút.
            float hour = _dayNight != null ? _dayNight.GameHour : _fallbackClock.HourOf(_game.NowSeconds);
            int minute = GameDayClock.MinuteOfDay(hour);
            if (minute != _minute)
            {
                _minute = minute;
                _time.text = GameDayClock.Format(hour);
            }
            int daytime = GameDayClock.IsDaytime(hour) ? 1 : 0;
            if (daytime != _daytime && _icon != null)
            {
                _daytime = daytime;
                _icon.sprite = daytime == 1 ? (_sunIcon != null ? _sunIcon : GeneratedSun()) : (_moonIcon != null ? _moonIcon : GeneratedMoon());
                _icon.preserveAspect = true;
            }
        }

        // ---------- Icon pixel 12×12 sinh bằng code (viền #181425 như Sunnyside) ----------

        private static readonly Color32 Outline = new Color32(24, 20, 37, 255);

        private static Sprite GeneratedSun()
        {
            if (_generatedSun != null)
                return _generatedSun;
            var pixels = new Color32[IconSize * IconSize];
            var core = new Color32(255, 206, 64, 255);
            var light = new Color32(255, 236, 140, 255);
            var ray = new Color32(255, 176, 48, 255);
            for (int y = 0; y < IconSize; y++)
            {
                for (int x = 0; x < IconSize; x++)
                {
                    float dx = x - 5.5f;
                    float dy = y - 5.5f;
                    float d = dx * dx + dy * dy;
                    if (d <= 2.6f * 2.6f)
                        pixels[y * IconSize + x] = dx < 0 && dy > 0 ? light : core;
                    else if (d <= 3.5f * 3.5f)
                        pixels[y * IconSize + x] = Outline;
                }
            }
            // 4 tia thẳng + 4 tia chéo (1 pixel).
            SetPixel(pixels, 5, 0, ray); SetPixel(pixels, 6, 0, ray);
            SetPixel(pixels, 5, 11, ray); SetPixel(pixels, 6, 11, ray);
            SetPixel(pixels, 0, 5, ray); SetPixel(pixels, 0, 6, ray);
            SetPixel(pixels, 11, 5, ray); SetPixel(pixels, 11, 6, ray);
            SetPixel(pixels, 1, 1, ray); SetPixel(pixels, 10, 1, ray);
            SetPixel(pixels, 1, 10, ray); SetPixel(pixels, 10, 10, ray);
            _generatedSun = CreateSprite(pixels, "ClockSun");
            return _generatedSun;
        }

        private static Sprite GeneratedMoon()
        {
            if (_generatedMoon != null)
                return _generatedMoon;
            var pixels = new Color32[IconSize * IconSize];
            var fill = new Color32(240, 236, 200, 255);
            var shade = new Color32(200, 196, 168, 255);
            for (int y = 0; y < IconSize; y++)
            {
                for (int x = 0; x < IconSize; x++)
                {
                    float dx = x - 5.5f;
                    float dy = y - 5.5f;
                    // Trăng khuyết: đĩa lớn trừ đĩa lệch sang phải-trên.
                    float cx = x - 8f;
                    float cy = y - 7.5f;
                    bool inMoon = dx * dx + dy * dy <= 4.6f * 4.6f;
                    bool inCut = cx * cx + cy * cy <= 3.6f * 3.6f;
                    bool inner = dx * dx + dy * dy <= 3.7f * 3.7f && cx * cx + cy * cy > 4.5f * 4.5f;
                    if (!inMoon || inCut)
                        continue;
                    pixels[y * IconSize + x] = inner ? (dy < -1f ? shade : fill) : Outline;
                }
            }
            _generatedMoon = CreateSprite(pixels, "ClockMoon");
            return _generatedMoon;
        }

        private static void SetPixel(Color32[] pixels, int x, int y, Color32 color) => pixels[y * IconSize + x] = color;

        private static Sprite CreateSprite(Color32[] pixels, string name)
        {
            var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = name,
                hideFlags = HideFlags.DontSave,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, IconSize, IconSize), new Vector2(0.5f, 0.5f), 16f);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }
    }
}
