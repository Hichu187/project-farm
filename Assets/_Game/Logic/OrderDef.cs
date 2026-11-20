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
        public static readonly OrderDef None = new OrderDef(0, 1, 100, 100, 0, null);

        public int SlotCount { get; }
        public int UnlockLevel { get; }
        public int CoinPercent { get; }
        public int XpPercent { get; }
        // Bỏ đơn thường thì ô trống bao lâu mới có đơn mới.
        public long DiscardCooldownSeconds { get; }
        // Người làng + câu thoại (Đợt 2 Phần 11). Empty = đơn không gắn người.
        public OrderDialogue Dialogue { get; }
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

        // Đơn cá (Đợt 2 Phần 11, chốt 20/11/2026): chỉ khi đã mở câu cá, chỉ đơn hạng Thường.
        // Tỉ lệ = FishOrderMinPercent% ở cấp mở câu cá, +1% mỗi cấp, tối đa FishOrderMaxPercent%. 0 = không có đơn cá.
        public int FishOrderMinPercent { get; }
        public int FishOrderMaxPercent { get; }
        // Chỉ đặt loài có độ hiếm ≤ FishOrderMaxRarity và câu được ở ≥ FishOrderMinSlots khung giờ.
        public FishRarity FishOrderMaxRarity { get; }
        public int FishOrderMinSlots { get; }
        // Số con: 1 ở cấp mở câu cá, +1 mỗi FishOrderLevelsPerFish cấp, tối đa FishOrderMaxCount.
        public int FishOrderMaxCount { get; }
        public int FishOrderLevelsPerFish { get; }

        public OrderDef(int slotCount, int unlockLevel, int coinPercent, int xpPercent, long discardCooldownSeconds,
            OrderDialogue dialogue, int maxHarvestsCap = 6,
            long refillSeconds = 0, long urgentRefillSeconds = 0, int urgentChancePercent = 0,
            int urgentRewardPercent = 200, long urgentExpireSeconds = 0, int dailyLimit = 0,
            int fishOrderMinPercent = 0, int fishOrderMaxPercent = 0, FishRarity fishOrderMaxRarity = FishRarity.Uncommon,
            int fishOrderMinSlots = 3, int fishOrderMaxCount = 3, int fishOrderLevelsPerFish = 3)
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

            if (fishOrderMinPercent < 0 || fishOrderMaxPercent < fishOrderMinPercent || fishOrderMaxPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(fishOrderMaxPercent), "Tỉ lệ đơn cá phải trong 0–100, min ≤ max.");
            if (fishOrderMinSlots < 1 || fishOrderMaxCount < 1 || fishOrderLevelsPerFish < 1)
                throw new ArgumentOutOfRangeException(nameof(fishOrderMaxCount));

            SlotCount = slotCount;
            UnlockLevel = unlockLevel;
            CoinPercent = coinPercent;
            XpPercent = xpPercent;
            DiscardCooldownSeconds = discardCooldownSeconds;
            Dialogue = dialogue ?? OrderDialogue.Empty;
            MaxHarvestsCap = maxHarvestsCap;
            RefillSeconds = refillSeconds;
            UrgentRefillSeconds = urgentRefillSeconds;
            UrgentChancePercent = urgentChancePercent;
            UrgentRewardPercent = urgentRewardPercent;
            UrgentExpireSeconds = urgentExpireSeconds;
            DailyLimit = dailyLimit;
            FishOrderMinPercent = fishOrderMinPercent;
            FishOrderMaxPercent = fishOrderMaxPercent;
            FishOrderMaxRarity = fishOrderMaxRarity;
            FishOrderMinSlots = fishOrderMinSlots;
            FishOrderMaxCount = fishOrderMaxCount;
            FishOrderLevelsPerFish = fishOrderLevelsPerFish;
        }

        // levelsAbove = số cấp trên cấp mở câu cá (≥ 0).
        public int GetFishOrderPercent(int levelsAbove)
            => FishOrderMaxPercent <= 0 ? 0 : Math.Min(FishOrderMaxPercent, FishOrderMinPercent + Math.Max(0, levelsAbove));

        public int GetFishOrderMaxCount(int levelsAbove)
            => Math.Min(FishOrderMaxCount, 1 + Math.Max(0, levelsAbove) / FishOrderLevelsPerFish);

        // Cấp dưới 4: 1 loại; cấp 4–6: tối đa 2; từ cấp 7: tối đa 3.
        public int GetMaxItemTypes(int level) => level < 4 ? 1 : level < 7 ? 2 : 3;

        public int GetMaxHarvests(int level) => Math.Min(MaxHarvestsCap, 2 + level / 2);

        public long GetRefillSeconds(OrderRank rank) => rank == OrderRank.Urgent ? UrgentRefillSeconds : RefillSeconds;

        // Bỏ đơn: đơn kế tiếp khẩn cấp thì vẫn chờ đủ thời gian của đơn khẩn cấp.
        public long GetDiscardSeconds(OrderRank rank) => rank == OrderRank.Urgent ? UrgentRefillSeconds : DiscardCooldownSeconds;
    }
}
