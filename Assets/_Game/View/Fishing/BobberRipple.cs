using UnityEngine;

namespace VeQue.View.Fishing
{
    // Sóng quanh phao (duyệt 21/11/2026): shader VeQue/PixelWater vẽ tối đa 2 vòng elip tỏa ra rồi mờ dần,
    // tự bị cắt ở mép nước. Component này chỉ gửi tâm / tuổi / độ mạnh của 2 vòng qua biến global mỗi frame
    // (không cấp phát). Gắn trên GameObject phao: phao ẩn → tắt sóng.
    // - Splash: kích 1 vòng mới (thay vòng cũ hơn). FishingController gọi lúc phao chạm nước, cá rỉa, cá cắn, bấm sai.
    // - Lặp nhẹ: tự kích vòng nhạt theo chu kỳ; Agitated (cá cắn / đang kéo) → nhanh và mạnh hơn.
    public sealed class BobberRipple : MonoBehaviour
    {
        private static readonly int PosId = Shader.PropertyToID("_BobberRipplePos");
        private static readonly int RingsId = Shader.PropertyToID("_BobberRippleRings");

        [Tooltip("Chu kỳ vòng nhạt lúc chờ (giây).")]
        [SerializeField] private float _idlePeriod = 1.6f;
        [Tooltip("Độ mạnh vòng nhạt lúc chờ (0–1).")]
        [SerializeField, Range(0f, 1f)] private float _idleStrength = 0.45f;
        [Tooltip("Chu kỳ vòng lúc cá cắn / đang kéo (giây).")]
        [SerializeField] private float _agitatedPeriod = 0.45f;
        [Tooltip("Độ mạnh vòng lúc cá cắn / đang kéo (0–1).")]
        [SerializeField, Range(0f, 1f)] private float _agitatedStrength = 0.8f;

        // Mỗi vòng: tâm (thế giới), lúc bắt đầu, độ mạnh.
        private Vector2 _center0, _center1;
        private float _start0 = -100f, _start1 = -100f;
        private float _strength0, _strength1;
        private float _nextIdle;
        private bool _agitated;

        public void SetAgitated(bool agitated)
        {
            if (_agitated == agitated)
                return;
            _agitated = agitated;
            _nextIdle = Time.time + Period;
        }

        public void Splash(Vector3 center, float strength)
        {
            // Thay vòng bắt đầu sớm hơn (cũ hơn).
            if (_start0 <= _start1)
            {
                _center0 = center;
                _start0 = Time.time;
                _strength0 = strength;
            }
            else
            {
                _center1 = center;
                _start1 = Time.time;
                _strength1 = strength;
            }
            _nextIdle = Time.time + Period;
        }

        private float Period => Mathf.Max(0.1f, _agitated ? _agitatedPeriod : _idlePeriod);

        private void OnEnable()
        {
            _agitated = false;
            _strength0 = _strength1 = 0f;
            _start0 = _start1 = -100f;
            _nextIdle = Time.time + Period;
        }

        private void OnDisable()
        {
            Shader.SetGlobalVector(RingsId, Vector4.zero);
        }

        private void Update()
        {
            float now = Time.time;
            if (now >= _nextIdle)
                Splash(transform.position, _agitated ? _agitatedStrength : _idleStrength);

            Shader.SetGlobalVector(PosId, new Vector4(_center0.x, _center0.y, _center1.x, _center1.y));
            Shader.SetGlobalVector(RingsId, new Vector4(now - _start0, _strength0, now - _start1, _strength1));
        }
    }
}
