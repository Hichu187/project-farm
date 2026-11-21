using System;

namespace VeQue.Logic
{
    // Giá chợ (Đợt 2 Phần 9): Kho hiện giá hôm nay, % so với hôm qua, nhãn hút hàng / rớt giá, biểu đồ 7 ngày;
    // "Bán hết" giữ lại phần đang cần cho đơn.
    public sealed partial class Farm
    {
        private MarketService _market;

        public MarketService Market => _market ??= new MarketService(Definition.Market, Definition.Crops);

        public string TodayHotCrop => Market.GetHotCrop(Calendar.DayKey);
        public string TodayColdCrop => Market.GetColdCrop(Calendar.DayKey);

        // % giá ngày hôm nay so với hôm qua (không tính hệ số trong ngày / sự kiện), làm tròn; cây lạ → 0.
        public int GetPriceChangePercent(string cropId)
        {
            if (!Definition.TryGetCrop(cropId, out CropDef crop))
                return 0;
            int today = Calendar.DayKey;
            int now = Market.GetDailyPrice(crop, today);
            int before = Market.GetDailyPrice(crop, today - 1);
            return before <= 0 ? 0 : (int)Math.Round((now - before) * 100.0 / before);
        }

        // Giá ngày của `days` ngày gần nhất, cũ → mới (phần tử cuối = hôm nay) – biểu đồ. Tính lại từ seed, không lưu.
        public void GetPriceHistory(string cropId, int[] result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            Definition.TryGetCrop(cropId, out CropDef crop);
            int today = Calendar.DayKey;
            for (int i = 0; i < result.Length; i++)
                result[i] = crop == null ? 0 : Market.GetDailyPrice(crop, today - (result.Length - 1 - i));
        }

        // Số nông sản đang cần cho các đơn nông sản đang có (chưa hết hạn).
        public int GetReservedForOrders(string cropId)
        {
            if (!OrdersUnlocked || cropId == null)
                return 0;
            long now = _clock.UtcNowSeconds;
            int reserved = 0;
            foreach (OrderState order in State.Orders)
            {
                if (order == null || order.IsFish || !order.HasOrder || order.IsExpired(now))
                    continue;
                foreach (OrderItem item in order.Items)
                {
                    if (item.Id == cropId)
                        reserved += item.Count;
                }
            }
            return reserved;
        }

        // "Bán hết" bán được bao nhiêu: đang có trừ phần cần cho đơn (không âm).
        public int GetSellableCount(string cropId) => Math.Max(0, State.GetItems(cropId) - GetReservedForOrders(cropId));
    }
}
