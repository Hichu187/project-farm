using UnityEngine;
using VeQue.View.Data;

namespace VeQue.View.Fishing
{
    // Câu được cá: hình cá (icon loài) bật lên khỏi mặt nước ở chỗ phao, bay theo đường cong dọc dây câu
    // về tay nhân vật rồi biến mất (duyệt 13/11/2026). Vị trí làm tròn theo pixel. GameObject tắt sẵn trong scene.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CaughtFishFlyer : MonoBehaviour
    {
        private const float PixelsPerUnit = 16f;

        [SerializeField] private float _seconds = 0.55f;
        [Tooltip("Độ cao đỉnh đường cong so với đường thẳng phao → tay (pixel).")]
        [SerializeField] private float _arcPixels = 18f;
        [Tooltip("Bật khỏi mặt nước: phần đầu (0..1 thời gian) cá trồi từ dưới nước lên.")]
        [Range(0f, 0.5f)] [SerializeField] private float _emergeFraction = 0.15f;
        [SerializeField] private float _emergeDepthPixels = 6f;

        private SpriteRenderer _renderer;
        private Material _normalMaterial;
        private MaterialPropertyBlock _block;
        private Vector3 _from;
        private Vector3 _to;
        private float _start = -1f;

        public float Seconds => _seconds;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (_renderer != null)
                return;
            _renderer = GetComponent<SpriteRenderer>();
            _normalMaterial = _renderer.sharedMaterial;
            _block = new MaterialPropertyBlock();
        }

        // from: chỗ phao; to: tay nhân vật. shiny: kiểu cá shiny (null = cá thường); seed: vị trí đốm lấp lánh.
        public void Launch(Sprite sprite, Vector3 from, Vector3 to, ShinyPalette.Entry shiny = null, int seed = 0)
        {
            if (sprite == null)
                return;
            Init();
            _renderer.sprite = sprite;
            ShinyLook.Apply(_renderer, shiny, seed, _normalMaterial, _block);
            // Icon vẽ đầu hướng trái: bay sang phải thì lật cho đầu đi trước.
            _renderer.flipX = to.x > from.x;
            _from = from;
            _to = to;
            _start = Time.time;
            gameObject.SetActive(true);
            Apply(0f);
        }

        public void Hide()
        {
            _start = -1f;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_start < 0f)
                return;
            float t = (Time.time - _start) / Mathf.Max(0.01f, _seconds);
            if (t >= 1f)
            {
                Hide();
                return;
            }
            Apply(t);
        }

        private void Apply(float t)
        {
            Vector3 pos;
            if (t < _emergeFraction)
            {
                // Trồi lên từ dưới nước tại chỗ phao.
                float k = t / Mathf.Max(0.001f, _emergeFraction);
                pos = _from + new Vector3(0f, -(1f - k) * _emergeDepthPixels / PixelsPerUnit, 0f);
            }
            else
            {
                float k = (t - _emergeFraction) / (1f - _emergeFraction);
                // Nhanh dần về cuối như bị giật lên.
                float e = k * k * (3f - 2f * k);
                pos = Vector3.Lerp(_from, _to, e);
                pos.y += 4f * k * (1f - k) * _arcPixels / PixelsPerUnit;
            }
            transform.position = new Vector3(
                Mathf.Round(pos.x * PixelsPerUnit) / PixelsPerUnit,
                Mathf.Round(pos.y * PixelsPerUnit) / PixelsPerUnit,
                pos.z);
        }
    }
}
