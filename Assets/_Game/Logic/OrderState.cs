using System;

namespace VeQue.Logic
{
    public enum OrderRank
    {
        Normal = 0,
        Urgent = 1,
    }

    // Đơn nông sản (giao từ Kho) hay đơn cá (giao từ giỏ, 1 loài) – Đợt 2 Phần 11.
    public enum OrderKind
    {
        Crops = 0,
        Fish = 1,
    }

    // Kết quả giao đơn (Farm.OrderDelivered): View hiện bong bóng cảm ơn. ThanksLine = -1 nếu không có câu.
    public readonly struct OrderDelivery
    {
        public readonly int Slot;
        public readonly string NpcId;
        public readonly int ThanksLine;
        public readonly long Coins;
        public readonly long Xp;

        public OrderDelivery(int slot, string npcId, int thanksLine, long coins, long xp)
        {
            Slot = slot;
            NpcId = npcId;
            ThanksLine = thanksLine;
            Coins = coins;
            Xp = xp;
        }
    }

    [Serializable]
    public sealed class OrderItem
    {
        // Id cây (đơn nông sản) hoặc id cá (đơn cá).
        public string Id;
        public int Count;
    }

    // Một ô đơn. Không có món nào = ô trống, chờ tới AvailableAt mới có đơn mới.
    // Rank: hạng của đơn đang có; ô trống thì là hạng của đơn sắp tới.
    [Serializable]
    public sealed class OrderState
    {
        public OrderKind Kind;
        // Người đặt (OrderDialogue); null = không gắn người.
        public string NpcId;
        // Chỉ số câu thoại trong OrderDialogue.Lines; -1 = không có.
        public int Line = -1;
        // Seed riêng của đơn: chọn món {dish}, câu cảm ơn. Lưu lại nên mở lại game không đổi câu.
        public uint Seed;
        public OrderItem[] Items = Array.Empty<OrderItem>();
        public long RewardCoins;
        public long RewardXp;
        public long AvailableAt;
        public OrderRank Rank;
        // Đơn khẩn cấp hết hạn lúc này; 0 = không hết hạn.
        public long ExpiresAt;

        public bool HasOrder => Items != null && Items.Length > 0;

        public bool IsUrgent => Rank == OrderRank.Urgent;

        public bool IsFish => Kind == OrderKind.Fish;

        public bool IsExpired(long now) => HasOrder && ExpiresAt > 0 && now >= ExpiresAt;

        internal void Clear(long availableAt, OrderRank nextRank)
        {
            Kind = OrderKind.Crops;
            NpcId = null;
            Line = -1;
            Seed = 0;
            Items = Array.Empty<OrderItem>();
            RewardCoins = 0;
            RewardXp = 0;
            AvailableAt = availableAt;
            Rank = nextRank;
            ExpiresAt = 0;
        }
    }
}
