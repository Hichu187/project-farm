using UnityEngine;
using UnityEngine.UI;

namespace VeQue.View.Fishing
{
    // Đốm lấp lánh của cá shiny (Đợt 2 Phần 18, thiết kế mục 2 "Đốm lấp lánh", khớp shiny_8kieu.py):
    // 3 đốm mỗi con, vị trí theo seed quanh hình (được tràn ra ngoài ô sprite), lệch pha nhau;
    // mỗi đốm sống 6 khung 0,095 giây trong vòng 16 khung: cánh 1 → 2 → 3 → 2 → 1 → tắt.
    // Gắn vào object của icon: Image (UI – đốm là Image con, bị RectMask2D cắt như icon) hoặc SpriteRenderer (world).
    // Chạy theo Time.unscaledTime (vẫn nháy khi tạm dừng); chỉ đổi sprite khi sang khung mới.
    [DisallowMultipleComponent]
    public sealed class ShinySparkles : MonoBehaviour
    {
        private const int Count = 3;
        private const int Frames = 16;
        private const float FrameSeconds = 0.095f;
        private static readonly int[] Arms = { 1, 2, 3, 2, 1, 0 };

        private readonly Vector2Int[] _points = new Vector2Int[Count];
        private readonly int[] _starts = new int[Count];
        private Image[] _images;
        private SpriteRenderer[] _renderers;
        private Color _tip;
        private bool _shown;
        private long _lastFrame = -1;

        // Bật đốm: tip = màu đầu cánh; seed cố định cho mỗi con để đốm không nhảy chỗ khi vẽ lại.
        public void Show(Color tip, int seed)
        {
            _tip = tip;
            _shown = true;
            _lastFrame = -1;
            uint random = (uint)seed * 2654435761u + 1u;
            if (random == 0)
                random = 1;
            Vector2Int size = SpritePixels();
            for (int i = 0; i < Count; i++)
            {
                // x lệch ra ngoài 2 px mỗi bên như mô phỏng; y trong hình (gốc trên-trái).
                int x = -2 + (int)(Next(ref random) % (uint)(size.x + 4));
                int y = (int)(Next(ref random) % (uint)size.y);
                _points[i] = new Vector2Int(x, y);
                _starts[i] = (int)((seed % Frames + Frames) % Frames + i * 5) % Frames;
            }
            EnsureChildren();
            Place();
            Tick();
        }

        public void Hide()
        {
            _shown = false;
            SetAll(false);
        }

        private void OnDisable()
        {
            _lastFrame = -1;
        }

        // Icon UI đổi kích thước (layout tính xong sau khi bật) → đặt lại đốm.
        private void OnRectTransformDimensionsChange()
        {
            if (_shown && _images != null)
                Place();
        }

        private void Update()
        {
            if (_shown)
                Tick();
        }

        private void Tick()
        {
            long frame = (long)(Time.unscaledTime / FrameSeconds) % Frames;
            if (frame == _lastFrame)
                return;
            _lastFrame = frame;
            for (int i = 0; i < Count; i++)
            {
                int age = (int)((frame - _starts[i] + Frames) % Frames);
                int arm = age < Arms.Length ? Arms[age] : 0;
                Sprite sprite = arm > 0 ? ShinySparkleSprites.Get(_tip, arm) : null;
                if (_images != null)
                {
                    _images[i].enabled = sprite != null;
                    _images[i].sprite = sprite;
                }
                else if (_renderers != null)
                {
                    _renderers[i].enabled = sprite != null;
                    _renderers[i].sprite = sprite;
                }
            }
        }

        private void EnsureChildren()
        {
            if (_images != null || _renderers != null)
            {
                SetAll(true);
                return;
            }
            if (GetComponent<Image>() != null)
            {
                _images = new Image[Count];
                for (int i = 0; i < Count; i++)
                {
                    var go = new GameObject("Sparkle", typeof(RectTransform));
                    go.transform.SetParent(transform, false);
                    var image = go.AddComponent<Image>();
                    image.raycastTarget = false;
                    _images[i] = image;
                }
            }
            else
            {
                var parent = GetComponent<SpriteRenderer>();
                _renderers = new SpriteRenderer[Count];
                for (int i = 0; i < Count; i++)
                {
                    var go = new GameObject("Sparkle");
                    go.transform.SetParent(transform, false);
                    var renderer = go.AddComponent<SpriteRenderer>();
                    if (parent != null)
                    {
                        renderer.sortingLayerID = parent.sortingLayerID;
                        renderer.sortingOrder = parent.sortingOrder + 1;
                    }
                    _renderers[i] = renderer;
                }
            }
        }

        // Đặt đốm theo tọa độ pixel sprite (tâm đốm ở tâm pixel).
        private void Place()
        {
            if (_images != null)
            {
                var rect = (RectTransform)transform;
                Vector2Int size = SpritePixels();
                float scale = Mathf.Min(rect.rect.width / size.x, rect.rect.height / size.y);
                Vector2 origin = new Vector2((rect.rect.width - size.x * scale) * 0.5f, -(rect.rect.height - size.y * scale) * 0.5f);
                for (int i = 0; i < Count; i++)
                {
                    RectTransform rt = _images[i].rectTransform;
                    rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(ShinySparkleSprites.Size * scale, ShinySparkleSprites.Size * scale);
                    rt.anchoredPosition = origin + new Vector2((_points[i].x + 0.5f) * scale, -(_points[i].y + 0.5f) * scale);
                }
            }
            else if (_renderers != null)
            {
                var parent = GetComponent<SpriteRenderer>();
                Sprite sprite = parent != null ? parent.sprite : null;
                float ppu = sprite != null ? sprite.pixelsPerUnit : 16f;
                Vector2 pivot = sprite != null ? sprite.pivot : Vector2.zero;
                float height = sprite != null ? sprite.rect.height : 32f;
                for (int i = 0; i < Count; i++)
                {
                    float x = (_points[i].x + 0.5f - pivot.x) / ppu;
                    float y = (height - _points[i].y - 0.5f - pivot.y) / ppu;
                    _renderers[i].transform.localPosition = new Vector3(x, y, 0f);
                }
            }
        }

        private Vector2Int SpritePixels()
        {
            Sprite sprite = null;
            if (TryGetComponent(out Image image))
                sprite = image.sprite;
            else if (TryGetComponent(out SpriteRenderer renderer))
                sprite = renderer.sprite;
            if (sprite == null)
                return new Vector2Int(32, 32);
            return new Vector2Int(Mathf.Max(1, Mathf.RoundToInt(sprite.rect.width)), Mathf.Max(1, Mathf.RoundToInt(sprite.rect.height)));
        }

        private void SetAll(bool active)
        {
            if (_images != null)
            {
                foreach (Image image in _images)
                    image.gameObject.SetActive(active);
            }
            if (_renderers != null)
            {
                foreach (SpriteRenderer renderer in _renderers)
                    renderer.gameObject.SetActive(active);
            }
        }

        private static uint Next(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }
    }
}
