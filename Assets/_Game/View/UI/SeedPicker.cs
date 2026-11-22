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
    // Thanh chọn hạt nhỏ bám ngay trên ô trống vừa chạm (thay bảng trượt): chỉ hiện cây đang có hạt trong kho,
    // mỗi cây một icon + số hạt, cây gieo gần nhất đứng đầu. Chạm icon = xếp việc gieo ô đó;
    // kéo icon = xếp việc gieo hàng loạt qua FarmGesture.
    // Số hạt đã trừ hạt giữ chỗ cho việc gieo đang chờ; về 0 thì icon ẩn (đang kéo thì đợi thả tay mới ẩn).
    // Không còn hạt nào: không mở thanh, báo và mở Cửa hàng. Có hạt nhưng chưa tới cấp: icon mờ.
    public sealed class SeedPicker : MonoBehaviour
    {
        private const float GapAbovePlot = 2f;
        private const string NoSeedsMessage = "Hết hạt giống, mua ở Cửa hàng";

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
        [SerializeField] private RectTransform _content;
        [Tooltip("Cuộn ngang khi nhiều cây hơn số icon hiện được.")]
        [SerializeField] private ScrollRect _scroll;
        [Tooltip("Số icon hiện tối đa; để lẻ .5 để thấy nửa icon cuối, biết còn cuộn được.")]
        [SerializeField] private float _maxVisibleItems = 5.5f;
        [Tooltip("Lề hai bên màn hình khi thanh rộng hơn màn hình.")]
        [SerializeField] private float _screenMargin = 16f;
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

        // Icon chuyển lượt kéo ngang sang đây để cuộn thanh.
        public ScrollRect Scroll => _scroll;

        public void Show(PlotId plot)
        {
            if (_dragging)
                EndDrag();
            if (RefreshItems() == 0)
            {
                Hide();
                Message?.Invoke(NoSeedsMessage);
                ShopRequested?.Invoke();
                return;
            }

            _plot = plot;
            _isShowing = true;
            MoveLastUsedFirst();
            ResizeBox();
            ScrollToStart();
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
            _game.Farm.Changed += HandleSeedsChanged;
            _game.Tasks.Changed += HandleSeedsChanged;
            RefreshItems();
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
            {
                _game.Farm.Changed -= HandleSeedsChanged;
                _game.Tasks.Changed -= HandleSeedsChanged;
            }
        }

        // Số hạt đổi khi đang mở thanh: hết sạch thì đóng thanh.
        private void HandleSeedsChanged()
        {
            int withSeeds = RefreshItems();
            if (!_isShowing || _dragging)
                return;
            if (withSeeds == 0)
                Hide();
            else
                ResizeBox();
        }

        // Bề ngang thanh = vừa số icon đang hiện, tối đa _maxVisibleItems icon và không quá bề ngang màn hình.
        private void ResizeBox()
        {
            int visible = 0;
            foreach (SeedPickerItem item in _items)
            {
                if (item.gameObject.activeSelf)
                    visible++;
            }

            float itemWidth = ((RectTransform)_itemTemplate.transform).rect.width;
            float spacing = _content.TryGetComponent(out HorizontalLayoutGroup layout) ? layout.spacing : 0f;
            float padding = _box.rect.width - _scroll.viewport.rect.width;
            float contentWidth = visible * itemWidth + Mathf.Max(0, visible - 1) * spacing;
            float maxWidth = _maxVisibleItems * itemWidth + Mathf.Floor(_maxVisibleItems) * spacing;
            float screenWidth = _area.rect.width - _screenMargin * 2f - padding;
            float limit = Mathf.Min(maxWidth, screenWidth);
            float width = contentWidth;
            if (contentWidth > limit)
            {
                // Phải cuộn: cắt sao cho icon cuối lộ một nửa, để biết còn kéo sang được.
                float step = itemWidth + spacing;
                int whole = Mathf.Max(0, Mathf.FloorToInt((limit - itemWidth * 0.5f) / step));
                width = whole * step + itemWidth * 0.5f;
            }

            _box.sizeDelta = new Vector2(width + padding, _box.sizeDelta.y);
            // Đặt thẳng bề ngang Content (không dựa vào ContentSizeFitter): ScrollRect chỉ cuộn khi Content rộng hơn khung nhìn.
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        }

        // Content neo mép trái (pivot trái) nên x = 0 là đầu danh sách; đặt thẳng vì thanh có thể đang tắt.
        private void ScrollToStart()
        {
            _scroll.StopMovement();
            _content.anchoredPosition = new Vector2(0f, _content.anchoredPosition.y);
        }

        // Số hạt còn dùng được: trừ hạt đã giữ chỗ cho việc gieo đang chờ.
        private int AvailableSeeds(string cropId) => _game.Tasks.GetAvailableSeeds(_game.Farm, cropId);

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

        // Cập nhật số hạt, ẩn cây hết hạt; trả về số cây còn hạt.
        // Đang kéo thì không bật / tắt icon: icon đang kéo là con của thanh, tắt đi thì mất sự kiện kéo.
        private int RefreshItems()
        {
            Farm farm = _game.Farm;
            int withSeeds = 0;
            foreach (SeedPickerItem item in _items)
            {
                int seeds = AvailableSeeds(item.Crop.Id);
                if (seeds > 0)
                    withSeeds++;
                item.Refresh(seeds, farm.IsCropPlantable(item.Crop.Id));
                if (!_dragging)
                    item.gameObject.SetActive(seeds > 0);
            }
            return withSeeds;
        }

        // Trả về false (kèm thông báo) nếu không gieo được bằng cây này.
        private bool CanUse(CropDefinition crop)
        {
            Farm farm = _game.Farm;
            if (!farm.IsCropPlantable(crop.Id))
            {
                Message?.Invoke($"{crop.DisplayName} mở ở cấp {crop.UnlockLevel}");
                return false;
            }
            if (AvailableSeeds(crop.Id) <= 0)
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

            _interaction.Enqueue(FarmTask.Plant(_plot, item.Crop.Id));
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
