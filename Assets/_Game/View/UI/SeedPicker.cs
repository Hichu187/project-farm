using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.CameraControl;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Thanh chọn hạt nhỏ bám ngay trên ô trống vừa chạm (thay bảng trượt): mỗi cây đang bật một icon + số hạt,
    // cây gieo gần nhất đứng đầu. Chạm icon = gieo ô đó; kéo icon = gieo hàng loạt qua FarmGesture.
    // Hết hạt hoặc chưa tới cấp: icon mờ; chạm vào hết hạt thì mở Cửa hàng.
    public sealed class SeedPicker : MonoBehaviour
    {
        private const float GapAbovePlot = 2f;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmInteraction _interaction;
        [SerializeField] private FarmGesture _gesture;
        [SerializeField] private FarmView _view;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private Camera _camera;
        [SerializeField] private RectTransform _area;
        [SerializeField] private RectTransform _box;
        // Ẩn thanh khi kéo bằng alpha, không tắt GameObject: icon đang kéo là con của thanh, tắt thì hết nhận sự kiện kéo.
        [SerializeField] private CanvasGroup _boxGroup;
        [SerializeField] private Transform _content;
        [SerializeField] private SeedPickerItem _itemTemplate;
        [Tooltip("Icon hạt đi theo ngón tay khi kéo, nâng lên để ngón tay không che.")]
        [SerializeField] private Image _dragIcon;
        [SerializeField] private float _dragIconLift = 48f;

        private readonly List<SeedPickerItem> _items = new List<SeedPickerItem>();
        private bool _isShowing;
        private bool _dragging;
        private PlotId _plot;

        public event Action<string> Message;
        public event Action ShopRequested;

        public void Show(PlotId plot)
        {
            _plot = plot;
            _isShowing = true;
            MoveLastUsedFirst();
            RefreshItems();
            _boxGroup.alpha = 1f;
            _boxGroup.blocksRaycasts = true;
            _box.gameObject.SetActive(true);
            UpdatePosition();
        }

        public void Hide()
        {
            if (_dragging)
                EndDrag();
            _isShowing = false;
            _box.gameObject.SetActive(false);
        }

        private void Start()
        {
            foreach (CropDefinition crop in _game.Definition.Crops)
            {
                if (crop == null || !crop.Enabled || !_game.Farm.Definition.TryGetCrop(crop.Id, out _))
                    continue;

                SeedPickerItem item = Instantiate(_itemTemplate, _content);
                item.gameObject.name = $"Seed {crop.Id}";
                item.gameObject.SetActive(true);
                item.Setup(this, crop);
                _items.Add(item);
            }

            _dragIcon.gameObject.SetActive(false);
            _game.Farm.Changed += RefreshItems;
            RefreshItems();
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= RefreshItems;
        }

        private void MoveLastUsedFirst()
        {
            string last = _interaction.LastCropId;
            foreach (SeedPickerItem item in _items)
            {
                if (item.Crop.Id == last)
                {
                    item.transform.SetAsFirstSibling();
                    return;
                }
            }
        }

        private void RefreshItems()
        {
            Farm farm = _game.Farm;
            foreach (SeedPickerItem item in _items)
                item.Refresh(farm.State.GetSeeds(item.Crop.Id), farm.IsCropUnlocked(item.Crop.Id));
        }

        // Trả về false (kèm thông báo) nếu không gieo được bằng cây này.
        private bool CanUse(CropDefinition crop)
        {
            Farm farm = _game.Farm;
            if (!farm.IsCropUnlocked(crop.Id))
            {
                Message?.Invoke($"{crop.DisplayName} mở ở cấp {crop.UnlockLevel}");
                return false;
            }
            if (farm.State.GetSeeds(crop.Id) <= 0)
            {
                Hide();
                Message?.Invoke($"Hết hạt {crop.DisplayName}, mua ở Cửa hàng");
                ShopRequested?.Invoke();
                return false;
            }
            return true;
        }

        public void Choose(SeedPickerItem item)
        {
            if (!_isShowing || !CanUse(item.Crop))
                return;

            FarmResult result = _interaction.Plant(_plot, item.Crop.Id);
            _interaction.Report(result);
            Hide();
        }

        public bool BeginDrag(SeedPickerItem item, Vector2 screen)
        {
            if (!_isShowing || !CanUse(item.Crop))
                return false;

            _dragging = true;
            // Ẩn thanh để thấy ruộng; icon hạt đi theo ngón tay.
            _boxGroup.alpha = 0f;
            _boxGroup.blocksRaycasts = false;
            _dragIcon.sprite = item.Crop.SeedSprite;
            _dragIcon.enabled = item.Crop.SeedSprite != null;
            _dragIcon.gameObject.SetActive(true);
            MoveDragIcon(screen);
            _gesture.BeginPlant(item.Crop.Id, _plot, screen);
            return true;
        }

        public void Drag(Vector2 screen)
        {
            if (!_dragging)
                return;
            _gesture.DragTo(screen);
            MoveDragIcon(screen);
        }

        public void EndDrag()
        {
            if (!_dragging)
                return;
            _dragging = false;
            _gesture.EndDrag();
            _dragIcon.gameObject.SetActive(false);
            Hide();
        }

        private void MoveDragIcon(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, screen, null, out Vector2 local);
            ((RectTransform)_dragIcon.transform).anchoredPosition = local + new Vector2(0f, _dragIconLift);
        }

        // Chạy sau khi camera đã di chuyển trong Update để thanh không trễ so với ô.
        private void LateUpdate()
        {
            if (!_isShowing || _dragging)
                return;
            if (_cameraController.CurrentMode != CameraController.Mode.Focused)
            {
                Hide();
                return;
            }
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (!_view.TryGetPlotCenter(_plot, out Vector3 center))
                return;

            Vector3 top = center + new Vector3(0f, 0.5f, 0f);
            Vector2 screen = _camera.WorldToScreenPoint(top);
            // Canvas Screen Space - Overlay nên không truyền camera.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, screen, null, out Vector2 local);

            // Giữ trong màn hình; pivot của thanh nằm giữa cạnh dưới.
            Vector2 halfArea = _area.rect.size * 0.5f;
            Vector2 size = _box.rect.size;
            local.x = Mathf.Clamp(local.x, -halfArea.x + size.x * 0.5f, halfArea.x - size.x * 0.5f);
            local.y = Mathf.Clamp(local.y + GapAbovePlot, -halfArea.y, halfArea.y - size.y);
            _box.anchoredPosition = local;
        }
    }
}
