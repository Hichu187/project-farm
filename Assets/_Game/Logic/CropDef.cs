using System;

namespace VeQue.Logic
{
    // Nhóm giá của cây, dùng cho giá chợ trong ngày (Đợt 2 Phần 9): rau lá đổi theo chợ sáng / khuya.
    public enum CropGroup
    {
        LeafyGreen = 0,
        Fruit = 1,
        RootGrain = 2,
    }

    // Định nghĩa một loại cây. Lớn qua StageCount giai đoạn đều nhau, chín khi đủ GrowSeconds.
    public sealed class CropDef
    {
        public const int DefaultStageCount = 3;

        public string Id { get; }
        public long GrowSeconds { get; }
        public int BuyPrice { get; }
        public int SellPrice { get; }
        public int HarvestYield { get; }
        public int StageCount { get; }
        // XP mỗi lần thu hoạch.
        public int Xp { get; }
        // Cấp cần để gieo và mua hạt.
        public int UnlockLevel { get; }
        public CropGroup Group { get; }
        // Cây sự kiện (Đợt 2 Phần 7): chỉ mua hạt được khi sự kiện UnlockCrop đang chạy; không vào đơn, quà lên cấp, cứu kẹt.
        public bool EventOnly { get; }

        public CropDef(string id, long growSeconds, int buyPrice, int sellPrice, int harvestYield,
            int stageCount = DefaultStageCount, int xp = 0, int unlockLevel = 1, CropGroup group = CropGroup.LeafyGreen,
            bool eventOnly = false)
        {
            EventOnly = eventOnly;
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Crop id rỗng.", nameof(id));
            if (growSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(growSeconds), "Thời gian lớn phải > 0.");
            if (buyPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(buyPrice));
            if (sellPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(sellPrice));
            if (harvestYield < 1)
                throw new ArgumentOutOfRangeException(nameof(harvestYield), "Sản lượng phải ≥ 1.");
            if (stageCount < 1)
                throw new ArgumentOutOfRangeException(nameof(stageCount), "Số giai đoạn phải ≥ 1.");
            if (xp < 0)
                throw new ArgumentOutOfRangeException(nameof(xp));
            if (unlockLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(unlockLevel), "Cấp mở phải ≥ 1.");

            Id = id;
            GrowSeconds = growSeconds;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            HarvestYield = harvestYield;
            StageCount = stageCount;
            Xp = xp;
            UnlockLevel = unlockLevel;
            Group = group;
        }
    }
}
