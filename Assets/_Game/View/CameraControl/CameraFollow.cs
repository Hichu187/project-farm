using UnityEngine;

namespace VeQue.View.CameraControl
{
    // Camera đi theo nhân vật (dùng ở vùng câu). Người chơi kéo để xem xung quanh; thả tay _returnDelay giây
    // (tính từ lần kéo cuối) thì camera tự lướt về nhân vật. Giới hạn trong mép vùng do CameraController lo.
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private CameraController _controller;
        [SerializeField] private Transform _target;
        [Tooltip("Độ bám: lớn = theo sát hơn.")]
        [SerializeField] private float _sharpness = 6f;
        [SerializeField] private float _returnDelay = 2f;

        public bool Following { get; set; }

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        // Đặt ngay camera vào nhân vật (sau chuyển cảnh).
        public void Snap()
        {
            if (_target != null)
                _controller.JumpTo(_target.position);
        }

        private void Awake()
        {
            if (_controller == null)
                _controller = GetComponent<CameraController>();
        }

        private void LateUpdate()
        {
            if (!Following || _target == null)
                return;
            if (Time.unscaledTime - _controller.LastPanTime < _returnDelay)
                return;

            float t = 1f - Mathf.Exp(-_sharpness * Time.unscaledDeltaTime);
            _controller.MoveTowards(_target.position, t);
        }
    }
}
