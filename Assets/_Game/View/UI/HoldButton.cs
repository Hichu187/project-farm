using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Nút báo cả lúc chạm xuống lẫn lúc nhả (giữ để nạp lực ném, chạm xuống để giật câu cho nhanh).
    // Kéo ngón ra ngoài nút rồi nhả vẫn tính là nhả. Tắt Interactable thì không báo gì.
    [RequireComponent(typeof(Image))]
    public sealed class HoldButton : Selectable, IPointerDownHandler, IPointerUpHandler
    {
        private bool _held;

        public event Action Pressed;
        public event Action Released;

        public bool IsHeld => _held;

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!IsInteractable() || _held)
                return;
            _held = true;
            Pressed?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            Release();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Release();
        }

        private void Release()
        {
            if (!_held)
                return;
            _held = false;
            Released?.Invoke();
        }
    }
}
