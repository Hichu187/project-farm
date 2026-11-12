using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Cấu hình đơn hàng làng quê (MVP-Plan Phần 6b mục B, 6c mục 2).
    // Mỗi đơn: 1..GetMaxItemTypes(cấp) loại cây đã mở; mỗi loại 1..GetMaxHarvests(cấp) lần thu × sản lượng cây.
    // Thưởng xu = Σ(số lượng × giá bán) × CoinPercent%, làm tròn 5; XP = Σ(số lần thu × XP cây) × XpPercent%.
    // Đơn khẩn cấp: thưởng xu và XP thêm × UrgentRewardPercent%, chờ lâu hơn mới có, hết hạn sau UrgentExpireSeconds.
    // Hạng của đơn kế tiếp quyết định ngay lúc ô trống, để thời gian chờ theo đúng hạng.
    public sealed class OrderDef
    {
        // Không có đơn hàng.
        public static readonly OrderDef None = new OrderDef(0, 1, 100, 100, 0, Array.Empty<string>());

        public int SlotCount { get; }
        public int UnlockLevel { get; }
        public int CoinPercent { get; }
        public int XpPercent { get; }
        // Bỏ đơn thường thì ô trống bao lâu mới có đơn mới.
        public long DiscardCooldownSeconds { get; }
        public IReadOnlyList<string> CustomerNames { get; }
        public int MaxHarvestsCap { get; }
        // Giao xong (hoặc đơn khẩn cấp hết hạn) thì ô trống bao lâu mới có đơn thường / khẩn cấp.
        public long RefillSeconds { get; }
        public long UrgentRefillSeconds { get; }
        // Xác suất đơn kế tiếp là khẩn cấp (0–100).
        public int UrgentChancePercent { get; }
        public int UrgentRewardPercent { get; }
        // 0 = đơn khẩn cấp không hết hạn.
        public long UrgentExpireSeconds { get; }
        // Số đơn giao được mỗi ngày (giờ địa phương); 0 = không giới hạn.
        public int DailyLimit { get; }

        public OrderDef(int slotCount, int unlockLevel, int coinPercent, int xpPercent, long discardCooldownSeconds,
            IReadOnlyList<string> customerNames, int maxHarvestsCap = 6,
            long refillSeconds = 0, long urgentRefillSeconds = 0, int urgentChancePercent = 0,
            int urgentRewardPercent = 200, long urgentExpireSeconds = 0, int dailyLimit = 0)
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
            if (refillSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(refillSeconds));
            if (urgentRefillSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(urgentRefillSeconds));
            if (urgentChancePercent < 0 || urgentChancePercent > 100)
                throw new ArgumentOutOfRangeException(nameof(urgentChancePercent), "Tỉ lệ đơn khẩn cấp phải trong 0–100.");
            if (urgentRewardPercent < 0)
                throw new ArgumentOutOfRangeException(nameof(urgentRewardPercent));
            if (urgentExpireSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(urgentExpireSeconds));
            if (dailyLimit < 0)
                throw new ArgumentOutOfRangeException(nameof(dailyLimit));

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
            RefillSeconds = refillSeconds;
            UrgentRefillSeconds = urgentRefillSeconds;
            UrgentChancePercent = urgentChancePercent;
            UrgentRewardPercent = urgentRewardPercent;
            UrgentExpireSeconds = urgentExpireSeconds;
            DailyLimit = dailyLimit;
        }

        // Cấp dưới 4: 1 loại; cấp 4–6: tối đa 2; từ cấp 7: tối đa 3.
        public int GetMaxItemTypes(int level) => level < 4 ? 1 : level < 7 ? 2 : 3;

        public int GetMaxHarvests(int level) => Math.Min(MaxHarvestsCap, 2 + level / 2);

        public long GetRefillSeconds(OrderRank rank) => rank == OrderRank.Urgent ? UrgentRefillSeconds : RefillSeconds;

        // Bỏ đơn: đơn kế tiếp khẩn cấp thì vẫn chờ đủ thời gian của đơn khẩn cấp.
        public long GetDiscardSeconds(OrderRank rank) => rank == OrderRank.Urgent ? UrgentRefillSeconds : DiscardCooldownSeconds;
    }
}
