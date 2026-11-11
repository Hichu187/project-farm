using System;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.CameraControl;
using VeQue.View.UI;

namespace VeQue.View.Map
{
    // Chạm ô khi đang zoom: ô khóa kế tiếp → hỏi mở (làm ngay khi xác nhận); ô trống → hiện thanh chọn hạt trên ô;
    // chưa tưới → xếp việc tưới + hiện tiến độ; đã tưới, chưa chín → hiện tiến độ; chín → xếp việc thu hoạch.
    // Gieo / tưới / thu hoạch không làm ngay mà vào hàng việc (GameBootstrap.Tasks); nhân vật tới nơi thì
    // FarmCharacterController gọi Execute. Enqueue dùng chung cho chạm và kéo (FarmGesture, SeedPicker).
    public sealed class FarmInteraction : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private CameraTouchInput _touch;
        [SerializeField] private FarmView _view;

        // Mỗi lần chạm vào khu đang zoom, trước khi xử lý (UI đóng thanh chọn hạt cũ).
        public event Action Tapped;
        public event Action<PlotId, int> UnlockRequested;
        public event Action<PlotId> SeedRequested;
        public event Action<string> Message;
        public event Action<PlotId> CropInspected;
        // Thu hoạch thành công: ô, cây, số nông sản vào Kho (UI cho icon bay về tab Kho).
        public event Action<PlotId, string, int> Harvested;

        // Cây gieo gần nhất trong phiên, để thanh chọn hạt đưa lên đầu.
        public string LastCropId { get; private set; }

        // Xếp việc vào hàng việc của nhân vật; lỗi thì báo (trừ ô đã có việc: bỏ qua im lặng).
        public FarmResult Enqueue(FarmTask task)
        {
            FarmResult result = _game.Tasks.TryEnqueue(_game.Farm, task);
            if (result == FarmResult.Ok && task.Kind == FarmTaskKind.Plant)
                LastCropId = task.CropId;
            Report(result);
            return result;
        }

        // Làm việc thật khi nhân vật đã tới nơi; Farm tự kiểm tra lại. Không báo lỗi (người gọi quyết định).
        public FarmResult Execute(FarmTask task)
        {
            switch (task.Kind)
            {
                case FarmTaskKind.Plant: return Plant(task.Plot, task.CropId);
                case FarmTaskKind.Water: return Water(task.Plot);
                case FarmTaskKind.Harvest: return Harvest(task.Plot);
                default: throw new ArgumentOutOfRangeException(nameof(task), task.Kind, null);
            }
        }

        public FarmResult Plant(PlotId id, string cropId)
        {
            FarmResult result = _game.Farm.Plant(id, cropId);
            if (result == FarmResult.Ok)
                LastCropId = cropId;
            return result;
        }

        public FarmResult Water(PlotId id) => _game.Farm.Water(id);

        public FarmResult Harvest(PlotId id)
        {
            Farm farm = _game.Farm;
            // Thu hoạch xóa CropId của ô nên lấy cây và sản lượng trước.
            string cropId = farm.TryGetPlotState(id, out PlotState plot) ? plot.CropId : null;
            farm.Definition.TryGetCrop(cropId, out CropDef def);

            FarmResult result = farm.Harvest(id);
            if (result == FarmResult.Ok && def != null)
                Harvested?.Invoke(id, cropId, def.HarvestYield);
            return result;
        }

        public void ConfirmUnlock(string zoneId)
        {
            Report(_game.Farm.UnlockNextPlot(zoneId));
        }

        public void Report(FarmResult result)
        {
            if (result == FarmResult.Ok || result == FarmResult.AlreadyQueued)
                return;

            switch (result)
            {
                case FarmResult.NotEnoughCoins:
                    Message?.Invoke("Không đủ xu");
                    break;
                case FarmResult.NotEnoughSeeds:
                    Message?.Invoke("Hết hạt");
                    break;
                case FarmResult.CropLocked:
                    Message?.Invoke("Chưa đủ cấp để trồng cây này");
                    break;
                case FarmResult.LevelTooLow:
                    Message?.Invoke("Chưa đủ cấp để mở khu này");
                    break;
                default:
                    Debug.LogWarning($"FarmInteraction: {result}", this);
                    break;
            }
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
            Tapped?.Invoke();
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
                    // Ô đã chờ gieo thì không mở lại thanh chọn hạt.
                    if (!_game.Tasks.IsQueued(id))
                        SeedRequested?.Invoke(id);
                    break;
                case PlotStatus.Growing:
                    HandleGrowing(farm, id, plot);
                    break;
            }
        }

        private void HandleLocked(Farm farm, PlotId id)
        {
            if (!farm.IsZoneAvailable(id.ZoneId))
                Message?.Invoke(DescribeLockedZone(farm, id.ZoneId));
            else if (farm.TryGetNextUnlock(id.ZoneId, out PlotId next, out int price) && next == id)
                UnlockRequested?.Invoke(id, price);
            else
                Message?.Invoke("Mở lần lượt từng ô, bắt đầu từ ô sáng hơn");
        }

        // Khu trước chưa mở hết thì báo khu trước; nếu không thì khu này thiếu cấp.
        private static string DescribeLockedZone(Farm farm, string zoneId)
        {
            if (!farm.Definition.TryGetZoneIndex(zoneId, out int index))
                return "Khu này chưa mở";
            if (index > 0 && !farm.State.Zones[index - 1].IsFull)
                return $"Mở hết {UiFormat.ZoneName(index - 1)} trước";
            return $"{UiFormat.ZoneName(index)} mở ở cấp {farm.Definition.Zones[index].RequiredLevel}";
        }

        private void HandleGrowing(Farm farm, PlotId id, PlotState plot)
        {
            if (farm.IsRipe(id))
            {
                Enqueue(FarmTask.Harvest(id));
                return;
            }

            if (!plot.Watered)
            {
                FarmResult watered = Enqueue(FarmTask.Water(id));
                if (watered != FarmResult.Ok && watered != FarmResult.AlreadyQueued)
                    return;
            }

            // Thanh tiến độ hiện ngay; nhân vật tới nơi mới tưới (thanh tự cập nhật, chín thì tự ẩn).
            CropInspected?.Invoke(id);
        }
    }
}
