using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VeQue.View.Data;

namespace VeQue.View.UI
{
    // Một icon hạt trong thanh chọn hạt: chạm = gieo ô đang chọn.
    // Kéo: lúc bắt đầu kéo ngang nhiều hơn dọc thì cả lượt kéo chuyển cho ScrollRect để cuộn thanh;
    // ngược lại (kéo xuống ruộng) = gieo các ô trống đi qua.
    public sealed class SeedPickerItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private enum DragMode
        {
            None,
            Plant,
            Scroll,
        }

        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _count;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private float _dimAlpha = 0.45f;

        private SeedPicker _owner;
        private DragMode _drag;

        public CropDefinition Crop { get; private set; }

        public void Setup(SeedPicker owner, CropDefinition crop)
        {
            _owner = owner;
            Crop = crop;
            _icon.sprite = crop.SeedSprite;
            _icon.enabled = crop.SeedSprite != null;
        }

        // Chưa tới cấp: hiện "Cấp N"; hết hạt: "x0". Cả hai đều mờ.
        public void Refresh(int seeds, bool unlocked)
        {
            _count.text = unlocked ? $"x{seeds}" : $"Cấp {Crop.UnlockLevel}";
            _group.alpha = unlocked && seeds > 0 ? 1f : _dimAlpha;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!eventData.dragging)
                _owner.Choose(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 moved = eventData.position - eventData.pressPosition;
            ScrollRect scroll = _owner.Scroll;
            if (scroll != null && Mathf.Abs(moved.x) > Mathf.Abs(moved.y))
            {
                _drag = DragMode.Scroll;
                scroll.OnBeginDrag(eventData);
                return;
            }
            _drag = _owner.BeginDrag(this, eventData.position) ? DragMode.Plant : DragMode.None;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_drag == DragMode.Scroll)
                _owner.Scroll.OnDrag(eventData);
            else if (_drag == DragMode.Plant)
                _owner.Drag(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragMode mode = _drag;
            _drag = DragMode.None;
            if (mode == DragMode.Scroll)
                _owner.Scroll.OnEndDrag(eventData);
            else if (mode == DragMode.Plant)
                _owner.EndDrag();
        }
    }
}
