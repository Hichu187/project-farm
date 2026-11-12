using UnityEngine;

namespace VeQue.View.Anim
{
    // Khung chọn 4 góc ôm một vùng (toạ độ world). Hoạt ảnh chỉ dời góc theo bước pixel và đổi độ trong suốt,
    // không phóng sprite, nên luôn sắc nét với Pixel Perfect Camera.
    // Đặt góc theo sprite.bounds nên không phụ thuộc pivot của sprite.
    public sealed class SelectBox : MonoBehaviour
    {
        public enum Motion
        {
            Static,
            // Góc dời ra / vào 1 nhịp pixel.
            Breathe,
            // Khung giãn ra rồi co về liên tục, góc đi từng bước 1 pixel (không phóng sprite).
            Scale,
        }

        // Truyền vào Show để dùng OutsetPixels mặc định của skin.
        public const int DefaultOutset = int.MinValue;

        private const float PixelsPerUnit = 16f;
        private const int CornerCount = 4;

        private readonly SpriteRenderer[] _corners = new SpriteRenderer[CornerCount];
        private SelectBoxSkin _skin;
        private Rect _rect;
        private Motion _motion;
        private Color _color = Color.white;
        private float _phase;
        private int _outset;
        private float _snapStart = -1f;
        private float _fadeStart = -1f;
        private int _placedOutset = int.MinValue;
        private float _appliedAlpha = -1f;

        public bool IsFading => _fadeStart >= 0f;

        // Tạo sẵn (đang ẩn). parent = null thì khung đứng riêng ở gốc scene.
        public static SelectBox Create(SelectBoxSkin skin, Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var box = go.AddComponent<SelectBox>();
            box._skin = skin;
            for (int i = 0; i < CornerCount; i++)
            {
                var corner = new GameObject($"Corner {i}").AddComponent<SpriteRenderer>();
                corner.transform.SetParent(go.transform, false);
                corner.sprite = skin.GetCorner(i);
                corner.sortingOrder = sortingOrder;
                box._corners[i] = corner;
            }
            go.SetActive(false);
            return box;
        }

        // phase (0..1): lệch nhịp giữa các khung. snapIn: góc thu từ ngoài vào.
        // outsetPixels: mép ngoài góc nằm ngoài rect bao nhiêu pixel; DefaultOutset = theo skin.
        public void Show(Rect rect, Motion motion, Color color, float phase = 0f, bool snapIn = false,
            int outsetPixels = DefaultOutset)
        {
            float now = Time.unscaledTime;
            _rect = rect;
            _outset = outsetPixels == DefaultOutset ? _skin.OutsetPixels : outsetPixels;
            _motion = motion;
            _color = color;
            _phase = phase;
            _snapStart = snapIn ? now : -1f;
            _fadeStart = -1f;
            _placedOutset = int.MinValue;
            _appliedAlpha = -1f;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            Apply(now);
        }

        public void FadeOut()
        {
            if (gameObject.activeSelf && _fadeStart < 0f)
            {
                _fadeStart = Time.unscaledTime;
                enabled = true;
            }
        }

        public void Hide()
        {
            _snapStart = -1f;
            _fadeStart = -1f;
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            Apply(Time.unscaledTime);
        }

        private void Apply(float now)
        {
            int outset = _outset;
            float alpha = 1f;

            if (_snapStart >= 0f)
            {
                float t = _skin.SnapDuration > 0f ? (now - _snapStart) / _skin.SnapDuration : 1f;
                if (t >= 1f)
                    _snapStart = -1f;
                else
                    outset += Mathf.RoundToInt((1f - Ease.OutQuad(t)) * _skin.SnapPixels);
            }

            if (_snapStart < 0f && _motion == Motion.Breathe && _skin.BreathePeriod > 0f)
            {
                float phase = Mathf.Repeat(now / _skin.BreathePeriod + _phase, 1f);
                if (phase < 0.5f)
                    outset += _skin.BreathePixels;
            }

            if (_snapStart < 0f && _motion == Motion.Scale && _skin.ScalePeriod > 0f)
            {
                // Sóng tam giác 0 → ScalePixels → 0, làm tròn theo pixel.
                float wave = Mathf.PingPong((now / _skin.ScalePeriod + _phase) * 2f, 1f);
                outset += Mathf.RoundToInt(wave * _skin.ScalePixels);
            }

            if (_fadeStart >= 0f)
            {
                float t = _skin.FadeDuration > 0f ? (now - _fadeStart) / _skin.FadeDuration : 1f;
                if (t >= 1f)
                {
                    Hide();
                    return;
                }
                alpha *= 1f - Ease.InQuad(t);
            }

            // Khung đứng yên không cần Update mỗi frame (nhiều ô chờ cùng lúc).
            enabled = _motion != Motion.Static || _snapStart >= 0f || _fadeStart >= 0f;

            if (outset != _placedOutset)
                Place(outset);
            if (alpha != _appliedAlpha)
            {
                Color color = _color;
                color.a *= alpha;
                for (int i = 0; i < CornerCount; i++)
                    _corners[i].color = color;
                _appliedAlpha = alpha;
            }
        }

        private void Place(int outsetPixels)
        {
            _placedOutset = outsetPixels;
            float o = outsetPixels / PixelsPerUnit;
            for (int i = 0; i < CornerCount; i++)
            {
                SpriteRenderer corner = _corners[i];
                if (corner.sprite == null)
                    continue;

                Bounds b = corner.sprite.bounds;
                bool left = i == SelectBoxSkin.TopLeft || i == SelectBoxSkin.BottomLeft;
                bool top = i == SelectBoxSkin.TopLeft || i == SelectBoxSkin.TopRight;
                float x = left ? _rect.xMin - o - b.min.x : _rect.xMax + o - b.max.x;
                float y = top ? _rect.yMax + o - b.max.y : _rect.yMin - o - b.min.y;
                corner.transform.position = new Vector3(
                    Ease.SnapToPixel(x, PixelsPerUnit),
                    Ease.SnapToPixel(y, PixelsPerUnit),
                    0f);
            }
        }
    }
}
