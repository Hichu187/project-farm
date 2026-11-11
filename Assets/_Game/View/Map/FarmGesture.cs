using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.CameraControl;

namespace VeQue.View.Map
{
    // Kéo để làm hàng loạt khi đang zoom vào khu:
    // - Gieo: kéo icon hạt từ thanh chọn hạt (SeedPicker gọi BeginPlant / DragTo / EndDrag) qua các ô trống.
    // - Thu hoạch / tưới: bắt đầu kéo từ một ô chín / cây chưa tưới (CameraTouchInput hỏi qua IWorldDragHandler).
    // Đường kéo được lấy mẫu dày để kéo nhanh không bỏ sót ô. Mỗi ô chỉ xử lý 1 lần trong một lần kéo.
    // Các ô xếp vào hàng việc theo đúng thứ tự ngón tay đi qua; nhân vật làm lần lượt.
    public sealed class FarmGesture : MonoBehaviour, IWorldDragHandler
    {
        private enum Tool
        {
            None,
            Plant,
            Harvest,
            Water,
        }

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmInteraction _interaction;
        [SerializeField] private FarmView _view;
        [SerializeField] private CameraTouchInput _touch;
        [SerializeField] private Camera _camera;
        [Tooltip("Khoảng lấy mẫu dọc đường kéo (đơn vị world, 1 = 1 ô).")]
        [SerializeField] private float _sampleStep = 0.25f;

        private readonly HashSet<PlotId> _visited = new HashSet<PlotId>();
        private Tool _tool;
        private string _cropId;
        private Vector2 _lastWorld;
        private bool _outOfSeedsReported;

        public event Action<string> Message;

        public bool IsDragging => _tool != Tool.None;

        private void OnEnable()
        {
            _touch.DragHandler = this;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(_touch.DragHandler, this))
                _touch.DragHandler = null;
        }

        // Gieo bằng kéo từ thanh chọn hạt: gieo luôn ô đã mở thanh chọn hạt, rồi các ô trống mà ngón tay đi qua.
        public void BeginPlant(string cropId, PlotId firstPlot, Vector2 screen)
        {
            Begin(Tool.Plant, ToWorld(screen));
            _cropId = cropId;
            Visit(firstPlot);
        }

        public bool TryBeginDrag(Vector2 screenStart)
        {
            Vector2 world = ToWorld(screenStart);
            if (!_view.TryGetPlotAt(world, out PlotId id) || !_game.Farm.TryGetPlotState(id, out PlotState plot))
                return false;
            if (plot.Status != PlotStatus.Growing)
                return false;

            Tool tool = _game.Farm.IsRipe(id) ? Tool.Harvest : !plot.Watered ? Tool.Water : Tool.None;
            if (tool == Tool.None)
                return false;

            Begin(tool, world);
            Visit(id);
            return true;
        }

        public void DragTo(Vector2 screen)
        {
            if (_tool == Tool.None)
                return;

            Vector2 world = ToWorld(screen);
            float distance = Vector2.Distance(_lastWorld, world);
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / _sampleStep));
            for (int i = 1; i <= steps; i++)
            {
                Vector2 sample = Vector2.Lerp(_lastWorld, world, (float)i / steps);
                if (_view.TryGetPlotAt(sample, out PlotId id))
                    Visit(id);
            }
            _lastWorld = world;
        }

        public void EndDrag()
        {
            _tool = Tool.None;
            _cropId = null;
            _visited.Clear();
        }

        private void Begin(Tool tool, Vector2 world)
        {
            _tool = tool;
            _lastWorld = world;
            _outOfSeedsReported = false;
            _visited.Clear();
        }

        private void Visit(PlotId id)
        {
            if (!_visited.Add(id))
                return;

            Farm farm = _game.Farm;
            if (!farm.TryGetPlotState(id, out PlotState plot))
                return;

            // Ô đã có việc thì bỏ qua im lặng. Việc hợp lệ vào hàng việc; ô hiện dấu chờ (FarmView),
            // sáng lên khi nhân vật làm xong.
            if (_game.Tasks.IsQueued(id))
                return;

            switch (_tool)
            {
                case Tool.Plant:
                    if (plot.Status != PlotStatus.Empty)
                        return;
                    if (_game.Tasks.GetAvailableSeeds(farm, _cropId) <= 0)
                    {
                        // Kéo tiếp qua ô khác cũng không gieo được; báo 1 lần.
                        if (!_outOfSeedsReported)
                            Message?.Invoke("Hết hạt");
                        _outOfSeedsReported = true;
                        return;
                    }
                    _interaction.Enqueue(FarmTask.Plant(id, _cropId));
                    break;
                case Tool.Harvest:
                    if (!farm.IsRipe(id))
                        return;
                    _interaction.Enqueue(FarmTask.Harvest(id));
                    break;
                case Tool.Water:
                    if (plot.Status != PlotStatus.Growing || plot.Watered || farm.IsRipe(id))
                        return;
                    _interaction.Enqueue(FarmTask.Water(id));
                    break;
            }
        }

        private Vector2 ToWorld(Vector2 screen)
        {
            Camera camera = _camera != null ? _camera : Camera.main;
            return camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
        }
    }
}
