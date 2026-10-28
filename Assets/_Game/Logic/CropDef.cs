using System;

namespace VeQue.Logic
{
    // Định nghĩa một loại cây. Lớn qua StageCount giai đoạn đều nhau, chín khi đủ GrowSeconds.
    public sealed class CropDef
    {
        public const int StageCount = 3;

        public string Id { get; }
        public long GrowSeconds { get; }
        public int BuyPrice { get; }
        public int SellPrice { get; }
        public int HarvestYield { get; }

        public CropDef(string id, long growSeconds, int buyPrice, int sellPrice, int harvestYield)
        {
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

            Id = id;
            GrowSeconds = growSeconds;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            HarvestYield = harvestYield;
        }
    }
}
