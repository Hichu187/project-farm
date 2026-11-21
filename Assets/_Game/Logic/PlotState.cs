using System;

namespace VeQue.Logic
{
    public enum PlotStatus
    {
        Locked = 0,
        Empty = 1,
        Growing = 2,
    }

    // "Chín" không lưu, suy ra từ PlantedAt + GrowSeconds.
    [Serializable]
    public sealed class PlotState
    {
        public PlotStatus Status;
        public string CropId;
        public long PlantedAt;
        public bool Watered;
        // Chốt LÚC GIEO (Đợt 2 Phần 7): thời gian lớn (đã nhân sự kiện; 0 = theo CropDef) và sản lượng cộng thêm.
        // Sự kiện hết thì cây đang lớn không đổi lúc chín.
        public long GrowSeconds;
        public int BonusYield;

        internal void Clear()
        {
            Status = PlotStatus.Empty;
            CropId = null;
            PlantedAt = 0;
            Watered = false;
            GrowSeconds = 0;
            BonusYield = 0;
        }
    }
}
