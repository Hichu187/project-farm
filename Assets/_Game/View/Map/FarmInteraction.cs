using System;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.CameraControl;
using VeQue.View.Data;

namespace VeQue.View.Map
{
    // Chạm ô khi đang zoom: ô khóa kế tiếp → hỏi mở; ô trống → gieo hạt đang chọn (chưa chọn hoặc hết hạt → hỏi chọn hạt);
    // chưa tưới → tưới rồi hiện tiến độ; đã tưới, chưa chín → hiện tiến độ; chín → thu hoạch.
    public sealed class FarmInteraction : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private CameraTouchInput _touch;
        [SerializeField] private FarmView _view;

        public event Action<PlotId, int> UnlockRequested;
        public event Action<PlotId> SeedRequested;
        public event Action<string> Message;
        public event Action<PlotId> CropInspected;
        public event Action SelectionChanged;

        public CropDefinition SelectedCrop { get; private set; }

        public void SelectCrop(CropDefinition crop)
        {
            SelectedCrop = crop;
            SelectionChanged?.Invoke();
        }

        public void PlantSelected(PlotId id)
        {
            if (SelectedCrop != null)
                Report(_game.Farm.Plant(id, SelectedCrop.Id));
        }

        public void ConfirmUnlock(string zoneId)
        {
            Report(_game.Farm.UnlockNextPlot(zoneId));
        }

        private void OnEnable()
        {
            _touch.TappedInFocus += HandleTap;
        }

        private void OnDisable()
        {
            _touch.TappedInFocus -= HandleTap;
        }

        private void HandleTap(Vector2 world)
        {
            if (!_view.TryGetPlotAt(world, out PlotId id))
                return;

            Farm farm = _game.Farm;
            if (!farm.TryGetPlotState(id, out PlotState plot))
                return;

            switch (plot.Status)
            {
                case PlotStatus.Locked:
                    HandleLocked(farm, id);
                    break;
                case PlotStatus.Empty:
                    HandleEmpty(farm, id);
                    break;
                case PlotStatus.Growing:
                    HandleGrowing(farm, id, plot);
                    break;
            }
        }

        private void HandleLocked(Farm farm, PlotId id)
        {
            if (!farm.IsZoneAvailable(id.ZoneId))
                Message?.Invoke("Mở hết khu trước để mở khóa khu này");
            else if (farm.TryGetNextUnlock(id.ZoneId, out PlotId next, out int price) && next == id)
                UnlockRequested?.Invoke(id, price);
            else
                Message?.Invoke("Mở lần lượt từng ô, bắt đầu từ ô sáng hơn");
        }

        private void HandleEmpty(Farm farm, PlotId id)
        {
            if (SelectedCrop != null && farm.State.GetSeeds(SelectedCrop.Id) > 0)
                PlantSelected(id);
            else
                SeedRequested?.Invoke(id);
        }

        private void HandleGrowing(Farm farm, PlotId id, PlotState plot)
        {
            if (farm.IsRipe(id))
            {
                // Thu hoạch xóa CropId của ô nên lấy tên và sản lượng trước.
                _game.TryGetCrop(plot.CropId, out CropDefinition crop);
                string name = crop != null ? crop.DisplayName : plot.CropId;
                farm.Definition.TryGetCrop(plot.CropId, out CropDef def);

                FarmResult result = farm.Harvest(id);
                Report(result);
                if (result == FarmResult.Ok && def != null)
                    Message?.Invoke($"+{def.HarvestYield} {name}");
                return;
            }

            if (!plot.Watered)
            {
                FarmResult watered = farm.Water(id);
                Report(watered);
                if (watered != FarmResult.Ok)
                    return;
            }

            // Tưới có thể làm cây chín ngay nếu đã lớn gần hết; khi đó thanh tự ẩn.
            CropInspected?.Invoke(id);
        }

        private void Report(FarmResult result)
        {
            if (result == FarmResult.Ok)
                return;

            switch (result)
            {
                case FarmResult.NotEnoughCoins:
                    Message?.Invoke("Không đủ xu");
                    break;
                case FarmResult.NotEnoughSeeds:
                    Message?.Invoke("Hết hạt");
                    break;
                default:
                    Debug.LogWarning($"FarmInteraction: {result}", this);
                    break;
            }
        }
    }
}
