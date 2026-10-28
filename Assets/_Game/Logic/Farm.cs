using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Luật chơi: gieo, thu hoạch, mua hạt, bán nông sản, mở ô. Mọi thay đổi FarmState đi qua đây.
    // Thời gian chỉ lấy từ IClock; không đếm giờ theo frame.
    // TODO(Phần 5): thêm sự kiện Changed để View cập nhật theo sự kiện.
    public sealed class Farm
    {
        private readonly IClock _clock;

        public FarmDef Definition { get; }
        public FarmState State { get; }

        public Farm(FarmDef definition, FarmState state, IClock clock)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            ValidateState();
            State.GrantAvailableStartPlots(Definition);
        }

        public FarmResult Plant(PlotId id, string cropId)
        {
            if (!Definition.TryGetCrop(cropId, out CropDef crop))
                return FarmResult.UnknownCrop;
            if (!TryGetPlot(id, out PlotState plot))
                return FarmResult.UnknownPlot;
            if (plot.Status == PlotStatus.Locked)
                return FarmResult.PlotLocked;
            if (plot.Status != PlotStatus.Empty)
                return FarmResult.PlotNotEmpty;
            if (State.GetSeeds(crop.Id) < 1)
                return FarmResult.NotEnoughSeeds;

            FarmState.AddCount(State.Seeds, crop.Id, -1);
            plot.Status = PlotStatus.Growing;
            plot.CropId = crop.Id;
            plot.PlantedAt = _clock.UtcNowSeconds;
            return FarmResult.Ok;
        }

        public FarmResult Harvest(PlotId id)
        {
            if (!TryGetPlot(id, out PlotState plot))
                return FarmResult.UnknownPlot;
            if (plot.Status == PlotStatus.Locked)
                return FarmResult.PlotLocked;
            if (plot.Status != PlotStatus.Growing)
                return FarmResult.NothingToHarvest;
            if (!Definition.TryGetCrop(plot.CropId, out CropDef crop))
                return FarmResult.UnknownCrop;
            if (GetElapsed(plot) < crop.GrowSeconds)
                return FarmResult.NotRipe;

            FarmState.AddCount(State.Inventory, crop.Id, crop.HarvestYield);
            plot.Clear();
            return FarmResult.Ok;
        }

        public FarmResult BuySeeds(string cropId, int amount)
        {
            if (!Definition.TryGetCrop(cropId, out CropDef crop))
                return FarmResult.UnknownCrop;
            if (amount <= 0)
                return FarmResult.InvalidAmount;

            long cost = (long)crop.BuyPrice * amount;
            if (State.Coins < cost)
                return FarmResult.NotEnoughCoins;

            State.Coins -= cost;
            FarmState.AddCount(State.Seeds, crop.Id, amount);
            return FarmResult.Ok;
        }

        public FarmResult Sell(string cropId, int amount)
        {
            if (!Definition.TryGetCrop(cropId, out CropDef crop))
                return FarmResult.UnknownCrop;
            if (amount <= 0)
                return FarmResult.InvalidAmount;
            if (State.GetItems(crop.Id) < amount)
                return FarmResult.NotEnoughItems;

            FarmState.AddCount(State.Inventory, crop.Id, -amount);
            State.Coins += (long)crop.SellPrice * amount;
            return FarmResult.Ok;
        }

        // Mở ô kế tiếp của khu (theo hàng, từ góc trên-trái). Mở xong ô cuối thì khu sau được mở khóa.
        public FarmResult UnlockNextPlot(string zoneId)
        {
            if (!Definition.TryGetZoneIndex(zoneId, out int index))
                return FarmResult.UnknownZone;

            ZoneState zone = State.Zones[index];
            if (zone.IsFull)
                return FarmResult.ZoneFull;
            if (!IsZoneAvailable(index))
                return FarmResult.ZoneLocked;

            int price = GetUnlockPrice(index);
            if (State.Coins < price)
                return FarmResult.NotEnoughCoins;

            State.Coins -= price;
            zone.OpenNext();
            if (zone.IsFull)
                State.GrantAvailableStartPlots(Definition);
            return FarmResult.Ok;
        }

        public bool TryGetNextUnlock(string zoneId, out PlotId plot, out int price)
        {
            plot = default;
            price = 0;
            if (!Definition.TryGetZoneIndex(zoneId, out int index))
                return false;

            ZoneState zone = State.Zones[index];
            if (zone.IsFull || !IsZoneAvailable(index))
                return false;

            plot = zone.IdAt(zone.OpenCount);
            price = GetUnlockPrice(index);
            return true;
        }

        public bool IsZoneAvailable(string zoneId)
        {
            return Definition.TryGetZoneIndex(zoneId, out int index) && IsZoneAvailable(index);
        }

        public bool TryGetPlotState(PlotId id, out PlotState plot) => TryGetPlot(id, out plot);

        // Giai đoạn 0..StageCount-1 của cây đang lớn; -1 nếu ô không có cây.
        public int GetStage(PlotId id)
        {
            if (!TryGetGrowing(id, out PlotState plot, out CropDef crop))
                return -1;

            long elapsed = GetElapsed(plot);
            if (elapsed >= crop.GrowSeconds)
                return CropDef.StageCount - 1;
            return (int)(elapsed * CropDef.StageCount / crop.GrowSeconds);
        }

        public bool IsRipe(PlotId id)
        {
            return TryGetGrowing(id, out PlotState plot, out CropDef crop) && GetElapsed(plot) >= crop.GrowSeconds;
        }

        private bool IsZoneAvailable(int index) => index == 0 || State.Zones[index - 1].IsFull;

        private int GetUnlockPrice(int index)
        {
            ZoneDef def = Definition.Zones[index];
            return def.UnlockPrices[State.Zones[index].OpenCount - def.StartOpenCount];
        }

        // Đồng hồ lùi (chỉnh giờ) thì coi như chưa trôi qua giây nào.
        private long GetElapsed(PlotState plot) => Math.Max(0L, _clock.UtcNowSeconds - plot.PlantedAt);

        private bool TryGetGrowing(PlotId id, out PlotState plot, out CropDef crop)
        {
            crop = null;
            return TryGetPlot(id, out plot)
                && plot.Status == PlotStatus.Growing
                && Definition.TryGetCrop(plot.CropId, out crop);
        }

        private bool TryGetPlot(PlotId id, out PlotState plot)
        {
            plot = null;
            if (!Definition.TryGetZoneIndex(id.ZoneId, out int index))
                return false;

            ZoneState zone = State.Zones[index];
            if (!zone.Contains(id.Col, id.Row))
                return false;

            plot = zone.Plots[zone.IndexOf(id.Col, id.Row)];
            return true;
        }

        private void ValidateState()
        {
            if (State.Zones == null || State.Zones.Length != Definition.Zones.Count)
                throw new ArgumentException("Số khu trong state không khớp data.");

            for (int i = 0; i < State.Zones.Length; i++)
            {
                ZoneDef def = Definition.Zones[i];
                ZoneState zone = State.Zones[i];
                if (zone == null || zone.ZoneId != def.Id || zone.Width != def.Width || zone.Height != def.Height
                    || zone.Plots == null || zone.Plots.Length != def.PlotCount
                    || zone.OpenCount < 0 || zone.OpenCount > def.PlotCount)
                    throw new ArgumentException($"Khu thứ {i} trong state không khớp data '{def.Id}'.");
            }

            State.Seeds ??= new Dictionary<string, int>();
            State.Inventory ??= new Dictionary<string, int>();
        }
    }
}
