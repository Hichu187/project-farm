using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VeQue.Logic;

namespace VeQue.View.UI
{
    // Vùng vuốt thay nút khi kéo cá (Đợt 2 Phần 6, thử A/B cùng chữ thập): vuốt ở giữa màn hình, tránh mép dưới
    // (thanh home iOS). Mỗi lần chạm chỉ tính 1 lần vuốt, báo ngay khi ngón đi đủ xa (không chờ nhấc tay).
    // Chỉ nhận ngón chạm đầu tiên; chạm không vuốt thì bỏ qua.
    [RequireComponent(typeof(Image))]
    public sealed class SwipePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Tooltip("Quãng vuốt tối thiểu (đơn vị UI, khung 390 pt).")]
        [SerializeField] private float _minDistance = 24f;

        private const int NoPointer = int.MinValue;

        private Canvas _canvas;
        private int _pointerId = NoPointer;
        private Vector2 _start;
        private bool _fired;

        public event Action<Arrow> Swiped;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        private void OnDisable()
        {
            _pointerId = NoPointer;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != NoPointer)
                return;
            _pointerId = eventData.pointerId;
            _start = eventData.position;
            _fired = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId || _fired)
                return;
            Vector2 delta = eventData.position - _start;
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            float min = _minDistance * scale;
            if (delta.sqrMagnitude < min * min)
                return;
            _fired = true;
            Arrow arrow = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? (delta.x < 0f ? Arrow.Left : Arrow.Right)
                : (delta.y < 0f ? Arrow.Down : Arrow.Up);
            Swiped?.Invoke(arrow);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == _pointerId)
                _pointerId = NoPointer;
        }
    }
}
