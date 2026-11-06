using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Cấu hình đơn hàng làng quê (MVP-Plan Phần 6b).
    // Mỗi đơn: 1..GetMaxItemTypes(cấp) loại cây đã mở; mỗi loại 1..GetMaxHarvests(cấp) lần thu × sản lượng cây.
    // Thưởng xu = Σ(số lượng × giá bán) × CoinPercent%, làm tròn 5; XP = Σ(số lần thu × XP cây) × XpPercent%.
    public sealed class OrderDef
    {
        // Không có đơn hàng.
        public static readonly OrderDef None = new OrderDef(0, 1, 100, 100, 0, Array.Empty<string>());

        public int SlotCount { get; }
        public int UnlockLevel { get; }
        public int CoinPercent { get; }
        public int XpPercent { get; }
        // Bỏ đơn thì ô đơn trống bao lâu mới có đơn mới.
        public long DiscardCooldownSeconds { get; }
        public IReadOnlyList<string> CustomerNames { get; }
        public int MaxHarvestsCap { get; }

        public OrderDef(int slotCount, int unlockLevel, int coinPercent, int xpPercent, long discardCooldownSeconds,
            IReadOnlyList<string> customerNames, int maxHarvestsCap = 6)
        {
            if (slotCount < 0)
                throw new ArgumentOutOfRangeException(nameof(slotCount));
            if (unlockLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(unlockLevel), "Cấp mở đơn hàng phải ≥ 1.");
            if (coinPercent < 0)
                throw new ArgumentOutOfRangeException(nameof(coinPercent));
            if (xpPercent < 0)
                throw new ArgumentOutOfRangeException(nameof(xpPercent));
            if (discardCooldownSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(discardCooldownSeconds));
            if (maxHarvestsCap < 1)
                throw new ArgumentOutOfRangeException(nameof(maxHarvestsCap));

            var names = new List<string>();
            if (customerNames != null)
            {
                foreach (string name in customerNames)
                {
                    if (!string.IsNullOrEmpty(name))
                        names.Add(name);
                }
            }

            SlotCount = slotCount;
            UnlockLevel = unlockLevel;
            CoinPercent = coinPercent;
            XpPercent = xpPercent;
            DiscardCooldownSeconds = discardCooldownSeconds;
            CustomerNames = names.AsReadOnly();
            MaxHarvestsCap = maxHarvestsCap;
        }

        // Cấp dưới 4: 1 loại; cấp 4–6: tối đa 2; từ cấp 7: tối đa 3.
        public int GetMaxItemTypes(int level) => level < 4 ? 1 : level < 7 ? 2 : 3;

        public int GetMaxHarvests(int level) => Math.Min(MaxHarvestsCap, 2 + level / 2);
    }
}
