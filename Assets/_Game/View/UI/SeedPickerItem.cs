using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VeQue.View.Data;

namespace VeQue.View.UI
{
    // Một icon hạt trong thanh chọn hạt: chạm = gieo ô đang chọn, kéo = gieo các ô trống đi qua.
    public sealed class SeedPickerItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _count;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private float _dimAlpha = 0.45f;

        private SeedPicker _owner;
        private bool _dragging;

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
            _dragging = _owner.BeginDrag(this, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragging)
                _owner.Drag(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;
            _dragging = false;
            _owner.EndDrag();
        }
    }
}
