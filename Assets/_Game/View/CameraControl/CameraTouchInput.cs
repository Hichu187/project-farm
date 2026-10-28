using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace VeQue.View.CameraControl
{
    // Kéo 1 ngón để di chuyển; chạm khu để zoom vào, chạm bãi cỏ để về toàn cảnh. Không pinch zoom.
    public sealed class CameraTouchInput : MonoBehaviour
    {
        [SerializeField] private CameraController _controller;
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private FocusArea[] _areas = Array.Empty<FocusArea>();
        [SerializeField] private float _dragThreshold = 10f;

        // Chạm vào bên trong khu đang zoom (toạ độ world) — Phần 5 dùng để gieo / thu hoạch / mở ô.
        public event Action<Vector2> TappedInFocus;

        private bool _isPressing;
        private bool _isDragging;
        private bool _checkUiNextFrame;
        private Vector2 _pressStart;
        private Vector2 _lastPosition;

        private void Awake()
        {
            if (_camera == null)
                _camera = GetComponent<UnityEngine.Camera>();
            if (_controller == null)
                _controller = GetComponent<CameraController>();
        }

        private void Update()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
                return;

            Vector2 position = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                _isPressing = !IsOverUi(pointer);
                _isDragging = false;
                _checkUiNextFrame = _isPressing;
                _pressStart = position;
                _lastPosition = position;
                return;
            }

            if (!_isPressing)
                return;

            // EventSystem cập nhật trạng thái UI sau input, nên kiểm tra lại ở frame kế tiếp.
            if (_checkUiNextFrame)
            {
                _checkUiNextFrame = false;
                if (IsOverUi(pointer))
                {
                    _isPressing = false;
                    return;
                }
            }

            if (pointer.press.isPressed)
            {
                if (!_isDragging && (position - _pressStart).sqrMagnitude >= _dragThreshold * _dragThreshold)
                    _isDragging = true;

                if (_isDragging)
                {
                    _controller.Pan(position - _lastPosition);
                    _lastPosition = position;
                }
                return;
            }

            _isPressing = false;
            if (!_isDragging)
                HandleTap(_pressStart);
        }

        private void HandleTap(Vector2 screenPosition)
        {
            if (_controller.IsTransitioning)
                return;

            Vector2 world = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
            FocusArea hit = FindArea(world);

            if (_controller.CurrentMode == CameraController.Mode.Focused)
            {
                if (hit == _controller.Current)
                    TappedInFocus?.Invoke(world);
                else if (hit != null)
                    _controller.FocusOn(hit);
                else
                    _controller.ShowOverview();
                return;
            }

            if (hit != null)
                _controller.FocusOn(hit);
        }

        private FocusArea FindArea(Vector2 world)
        {
            for (int i = 0; i < _areas.Length; i++)
            {
                FocusArea area = _areas[i];
                if (area != null && area.Contains(world))
                    return area;
            }
            return null;
        }

        private static bool IsOverUi(Pointer pointer)
        {
            EventSystem eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject(pointer.deviceId);
        }
    }
}
