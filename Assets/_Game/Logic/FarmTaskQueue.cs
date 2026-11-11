using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Hàng việc của nhân vật ở nông trại: gieo / tưới / thu hoạch làm lần lượt đúng thứ tự được chọn.
    // Chỉ giữ thứ tự và kiểm tra lúc xếp việc; việc thật do View gọi Farm khi nhân vật tới nơi
    // (Farm tự kiểm tra lại), xong thì gọi CompleteCurrent.
    // - Mỗi ô chỉ có 1 việc (tính cả việc đang làm).
    // - Gieo giữ chỗ 1 hạt cho tới khi việc xong hoặc bị hủy, để kéo qua nhiều ô không xếp quá số hạt.
    public sealed class FarmTaskQueue
    {
        private readonly List<FarmTask> _pending = new List<FarmTask>();
        private readonly Dictionary<string, int> _reservedSeeds = new Dictionary<string, int>();
        private FarmTask _current;

        // Báo mỗi khi hàng việc đổi (thêm, bắt đầu, xong, hủy).
        public event Action Changed;

        public bool HasCurrent { get; private set; }
        public FarmTask Current => HasCurrent ? _current : throw new InvalidOperationException("Không có việc đang làm.");

        // Số việc chưa bắt đầu.
        public int PendingCount => _pending.Count;

        // Tổng số việc, gồm cả việc đang làm.
        public int Count => _pending.Count + (HasCurrent ? 1 : 0);

        public bool IsEmpty => Count == 0;

        public FarmTask GetPending(int index) => _pending[index];

        public bool IsQueued(PlotId plot)
        {
            if (HasCurrent && _current.Plot == plot)
                return true;
            for (int i = 0; i < _pending.Count; i++)
            {
                if (_pending[i].Plot == plot)
                    return true;
            }
            return false;
        }

        public int GetReservedSeeds(string cropId)
        {
            return cropId != null && _reservedSeeds.TryGetValue(cropId, out int count) ? count : 0;
        }

        // Số hạt còn dùng được để xếp thêm việc gieo (thanh hạt hiện số này).
        public int GetAvailableSeeds(Farm farm, string cropId)
        {
            if (farm == null)
                throw new ArgumentNullException(nameof(farm));
            return Math.Max(0, farm.State.GetSeeds(cropId) - GetReservedSeeds(cropId));
        }

        // Kiểm tra như lúc xếp việc, không thay đổi gì.
        public FarmResult CanEnqueue(Farm farm, FarmTask task)
        {
            if (farm == null)
                throw new ArgumentNullException(nameof(farm));

            CropDef crop = null;
            if (task.Kind == FarmTaskKind.Plant)
            {
                if (!farm.Definition.TryGetCrop(task.CropId, out crop))
                    return FarmResult.UnknownCrop;
                if (crop.UnlockLevel > farm.State.Level)
                    return FarmResult.CropLocked;
            }

            if (!farm.TryGetPlotState(task.Plot, out PlotState plot))
                return FarmResult.UnknownPlot;
            if (IsQueued(task.Plot))
                return FarmResult.AlreadyQueued;
            if (plot.Status == PlotStatus.Locked)
                return FarmResult.PlotLocked;

            switch (task.Kind)
            {
                case FarmTaskKind.Plant:
                    if (plot.Status != PlotStatus.Empty)
                        return FarmResult.PlotNotEmpty;
                    if (GetAvailableSeeds(farm, crop.Id) < 1)
                        return FarmResult.NotEnoughSeeds;
                    return FarmResult.Ok;

                case FarmTaskKind.Water:
                    if (plot.Status != PlotStatus.Growing)
                        return FarmResult.NothingToWater;
                    if (plot.Watered)
                        return FarmResult.AlreadyWatered;
                    if (farm.IsRipe(task.Plot))
                        return FarmResult.AlreadyRipe;
                    return FarmResult.Ok;

                case FarmTaskKind.Harvest:
                    if (plot.Status != PlotStatus.Growing)
                        return FarmResult.NothingToHarvest;
                    if (!farm.IsRipe(task.Plot))
                        return FarmResult.NotRipe;
                    return FarmResult.Ok;

                default:
                    throw new ArgumentOutOfRangeException(nameof(task), task.Kind, "Loại việc không hợp lệ.");
            }
        }

        public FarmResult TryEnqueue(Farm farm, FarmTask task)
        {
            FarmResult result = CanEnqueue(farm, task);
            if (result != FarmResult.Ok)
                return result;

            _pending.Add(task);
            if (task.Kind == FarmTaskKind.Plant)
                AddReserved(task.CropId, 1);
            Changed?.Invoke();
            return FarmResult.Ok;
        }

        // Lấy việc kế tiếp làm việc đang làm. Đang có việc dở hoặc hết việc thì trả false.
        public bool TryBeginNext(out FarmTask task)
        {
            if (HasCurrent || _pending.Count == 0)
            {
                task = default;
                return false;
            }

            _current = _pending[0];
            _pending.RemoveAt(0);
            HasCurrent = true;
            task = _current;
            Changed?.Invoke();
            return true;
        }

        // Việc đang làm đã xong (thành công hay không): trả hạt giữ chỗ. Hạt thật do Farm.Plant trừ.
        public void CompleteCurrent()
        {
            if (!HasCurrent)
                throw new InvalidOperationException("Không có việc đang làm.");

            ReleaseReservation(_current);
            _current = default;
            HasCurrent = false;
            Changed?.Invoke();
        }

        // Xóa hết, kể cả việc đang làm; trả toàn bộ hạt giữ chỗ. Trả số việc đã xóa.
        public int Cancel()
        {
            int removed = Count;
            if (removed == 0)
                return 0;

            _pending.Clear();
            _reservedSeeds.Clear();
            _current = default;
            HasCurrent = false;
            Changed?.Invoke();
            return removed;
        }

        private void ReleaseReservation(FarmTask task)
        {
            if (task.Kind == FarmTaskKind.Plant)
                AddReserved(task.CropId, -1);
        }

        private void AddReserved(string cropId, int delta)
        {
            int count = GetReservedSeeds(cropId) + delta;
            if (count > 0)
                _reservedSeeds[cropId] = count;
            else
                _reservedSeeds.Remove(cropId);
        }
    }
}
