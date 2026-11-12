using System;

namespace VeQue.Logic
{
    public enum OrderRank
    {
        Normal = 0,
        Urgent = 1,
    }

    [Serializable]
    public sealed class OrderItem
    {
        public string CropId;
        public int Count;
    }

    // Một ô đơn. Không có món nào = ô trống, chờ tới AvailableAt mới có đơn mới.
    // Rank: hạng của đơn đang có; ô trống thì là hạng của đơn sắp tới.
    [Serializable]
    public sealed class OrderState
    {
        public string Customer;
        public OrderItem[] Items = Array.Empty<OrderItem>();
        public long RewardCoins;
        public long RewardXp;
        public long AvailableAt;
        public OrderRank Rank;
        // Đơn khẩn cấp hết hạn lúc này; 0 = không hết hạn.
        public long ExpiresAt;

        public bool HasOrder => Items != null && Items.Length > 0;

        public bool IsUrgent => Rank == OrderRank.Urgent;

        public bool IsExpired(long now) => HasOrder && ExpiresAt > 0 && now >= ExpiresAt;

        internal void Clear(long availableAt, OrderRank nextRank)
        {
            Customer = null;
            Items = Array.Empty<OrderItem>();
            RewardCoins = 0;
            RewardXp = 0;
            AvailableAt = availableAt;
            Rank = nextRank;
            ExpiresAt = 0;
        }
    }
}
