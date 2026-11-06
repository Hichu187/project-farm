using System.Collections.Generic;

namespace VeQue.Logic
{
    // Những gì có được khi lên một cấp: để UI hiện hộp lên cấp mà không tự tính lại luật.
    public sealed class LevelUnlocks
    {
        public int Level { get; }
        public long Coins { get; }
        // Cây mở ở cấp này (mỗi cây được tặng SeedsPerCrop hạt).
        public IReadOnlyList<string> CropIds { get; }
        public int SeedsPerCrop { get; }
        // Chỉ số khu (theo thứ tự trong FarmDef) có RequiredLevel = cấp này.
        public IReadOnlyList<int> ZoneIndices { get; }
        public bool OrdersUnlocked { get; }

        public LevelUnlocks(int level, long coins, IReadOnlyList<string> cropIds, int seedsPerCrop,
            IReadOnlyList<int> zoneIndices, bool ordersUnlocked)
        {
            Level = level;
            Coins = coins;
            CropIds = cropIds;
            SeedsPerCrop = seedsPerCrop;
            ZoneIndices = zoneIndices;
            OrdersUnlocked = ordersUnlocked;
        }
    }
}
