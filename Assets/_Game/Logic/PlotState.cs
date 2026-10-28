using System;

namespace VeQue.Logic
{
    public enum PlotStatus
    {
        Locked = 0,
        Empty = 1,
        Growing = 2,
    }

    // "Chín" không lưu, suy ra từ PlantedAt + CropDef.GrowSeconds.
    [Serializable]
    public sealed class PlotState
    {
        public PlotStatus Status;
        public string CropId;
        public long PlantedAt;

        internal void Clear()
        {
            Status = PlotStatus.Empty;
            CropId = null;
            PlantedAt = 0;
        }
    }
}
