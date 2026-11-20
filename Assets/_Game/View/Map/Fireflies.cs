using UnityEngine;

namespace VeQue.View.Map
{
    // Đom đóm ban đêm (Đợt 2 Phần 13): vài chấm 1 pixel vàng bay lượn trong vùng Size quanh object, nhấp nháy,
    // mỗi con là một vòng sáng nhỏ (WorldLight). Vẽ trên lớp ngày đêm (sortingOrder 285) để không bị tint tối.
    // Chỉ hiện khi DayNightController.NightAmount đủ lớn và cài đặt không Tắt. Không cấp phát mỗi frame.
    public sealed class Fireflies : MonoBehaviour
    {
        private const float PixelsPerUnit = 16f;

        [SerializeField] private DayNightController _dayNight;
        [Min(1)] [SerializeField] private int _count = 5;
        [Tooltip("Vùng bay (đơn vị thế giới, 1 = 1 tile), tâm ở object.")]
        [SerializeField] private Vector2 _size = new Vector2(12f, 6f);
        [SerializeField] private float _speed = 0.6f;
        [SerializeField] private int _sortingOrder = 285;
        [SerializeField] private Color _color = new Color(1f, 0.95f, 0.45f);
        [Tooltip("Hiện khi độ tối (NightAmount) vượt mức này.")]
        [Range(0f, 1f)] [SerializeField] private float _showAbove = 0.5f;

        private Transform[] _flies;
        private SpriteRenderer[] _renderers;
        private WorldLight[] _lights;
        private Vector2[] _targets;
        private float[] _phase;
        private bool _visible;
        private uint _random = 2463534242u;
        private static Sprite _dot;

        private void Start()
        {
            if (_dayNight == null)
                _dayNight = FindFirstObjectByType<DayNightController>();
            _flies = new Transform[_count];
            _renderers = new SpriteRenderer[_count];
            _lights = new WorldLight[_count];
            _targets = new Vector2[_count];
            _phase = new float[_count];
            for (int i = 0; i < _count; i++)
            {
                var go = new GameObject($"Firefly{i + 1}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = RandomPoint();
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = Dot();
                renderer.color = _color;
                renderer.sortingOrder = _sortingOrder;
                var light = go.AddComponent<WorldLight>();
                light.RadiusPixels = 6f;
                light.SoftPixels = 4f;
                light.Color = _color;
                light.NightOnly = true;
                _flies[i] = go.transform;
                _renderers[i] = renderer;
                _lights[i] = light;
                _targets[i] = RandomPoint();
                _phase[i] = Next01() * 6.28f;
                go.SetActive(false);
            }
        }

        private void Update()
        {
            bool show = _dayNight != null && _dayNight.isActiveAndEnabled
                && DayNightSetting.Mode != DayNightMode.Off && _dayNight.NightAmount >= _showAbove;
            if (show != _visible)
            {
                _visible = show;
                for (int i = 0; i < _flies.Length; i++)
                    _flies[i].gameObject.SetActive(show);
            }
            if (!show)
                return;

            float dt = Time.deltaTime;
            float time = Time.time;
            for (int i = 0; i < _flies.Length; i++)
            {
                Transform fly = _flies[i];
                Vector2 pos = fly.localPosition;
                Vector2 to = _targets[i] - pos;
                if (to.sqrMagnitude < 0.04f)
                    _targets[i] = RandomPoint();
                else
                    pos += to.normalized * (_speed * dt);
                // Làm tròn theo pixel cho đúng phong cách.
                fly.localPosition = new Vector3(Mathf.Round(pos.x * PixelsPerUnit) / PixelsPerUnit,
                    Mathf.Round(pos.y * PixelsPerUnit) / PixelsPerUnit, 0f);
                // Nhấp nháy chậm.
                float glow = 0.55f + 0.45f * Mathf.Sin(time * 2.2f + _phase[i]);
                Color c = _color;
                c.a = glow;
                _renderers[i].color = c;
                _lights[i].Intensity = 0.5f * glow;
            }
        }

        private Vector2 RandomPoint() => new Vector2((Next01() - 0.5f) * _size.x, (Next01() - 0.5f) * _size.y);

        private float Next01()
        {
            _random ^= _random << 13;
            _random ^= _random >> 17;
            _random ^= _random << 5;
            return (_random >> 8) / 16777216f;
        }

        // Chấm 1 pixel trắng (tô màu bằng SpriteRenderer.color), sinh bằng code.
        private static Sprite Dot()
        {
            if (_dot != null)
                return _dot;
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                name = "FireflyDot",
                hideFlags = HideFlags.DontSave,
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, true);
            _dot = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            _dot.hideFlags = HideFlags.DontSave;
            return _dot;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.6f);
            Gizmos.DrawWireCube(transform.position, new Vector3(_size.x, _size.y, 0f));
        }
    }
}
