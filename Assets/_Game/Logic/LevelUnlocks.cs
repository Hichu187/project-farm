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
        // Cần câu / mồi (bán ở Cửa hàng) mua được từ cấp này (câu cá đang bật).
        public IReadOnlyList<string> RodIds { get; }
        public IReadOnlyList<string> BaitIds { get; }

        public LevelUnlocks(int level, long coins, IReadOnlyList<string> cropIds, int seedsPerCrop,
            IReadOnlyList<int> zoneIndices, bool ordersUnlocked,
            IReadOnlyList<string> rodIds = null, IReadOnlyList<string> baitIds = null)
        {
            Level = level;
            Coins = coins;
            CropIds = cropIds;
            SeedsPerCrop = seedsPerCrop;
            ZoneIndices = zoneIndices;
            OrdersUnlocked = ordersUnlocked;
            RodIds = rodIds ?? System.Array.Empty<string>();
            BaitIds = baitIds ?? System.Array.Empty<string>();
        }
    }
}
