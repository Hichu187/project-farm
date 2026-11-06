using System;

namespace VeQue.Logic
{
    [Serializable]
    public sealed class OrderItem
    {
        public string CropId;
        public int Count;
    }

    // Một ô đơn. Không có món nào = ô trống, chờ tới AvailableAt mới có đơn mới.
    [Serializable]
    public sealed class OrderState
    {
        public string Customer;
        public OrderItem[] Items = Array.Empty<OrderItem>();
        public long RewardCoins;
        public long RewardXp;
        public long AvailableAt;

        public bool HasOrder => Items != null && Items.Length > 0;

        internal void Clear(long availableAt)
        {
            Customer = null;
            Items = Array.Empty<OrderItem>();
            RewardCoins = 0;
            RewardXp = 0;
            AvailableAt = availableAt;
        }
    }
}
