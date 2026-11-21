using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Sự kiện theo ngày (Đợt 2 Phần 7). Các hệ thống tự hỏi (giá, cá, gieo, đơn) theo thời điểm hiện tại,
    // nên vẫn đúng khi tab bị ẩn. Hiệu ứng lớn nhanh / được mùa chốt lúc gieo (lưu trong ô đất).
    public sealed partial class Farm
    {
        private EventCalendar _eventCalendar;
        // Cache sự kiện hôm nay theo (ngày, cấp) – hỏi mỗi giây / mỗi lần bán.
        private int _eventsDay = int.MinValue;
        private int _eventsLevel = -1;
        private readonly List<EventDef> _todayEvents = new List<EventDef>();
        private readonly Dictionary<string, HashSet<string>> _randomCrops = new Dictionary<string, HashSet<string>>();

        public EventCalendar EventCalendar => _eventCalendar ??= new EventCalendar(Definition.Events, Definition.Lunar);

        // Sự kiện hôm nay người chơi thấy (đủ cấp), ưu tiên cao trước.
        public IReadOnlyList<EventDef> TodayEvents
        {
            get
            {
                int day = Calendar.DayKey;
                if (day != _eventsDay || State.Level != _eventsLevel)
                {
                    _eventsDay = day;
                    _eventsLevel = State.Level;
                    _todayEvents.Clear();
                    _randomCrops.Clear();
                    foreach (EventDef e in EventCalendar.GetEvents(day))
                    {
                        if (State.Level >= e.MinLevel)
                            _todayEvents.Add(e);
                    }
                }
                return _todayEvents;
            }
        }

        // ---------- Hỏi hiệu ứng (chỉ sự kiện đang trong khung giờ) ----------

        public float GetCropPriceMult(CropDef crop)
        {
            float mult = 1f;
            ForEachActive(EventEffectType.CropPriceMult, (e, effect) =>
            {
                if (MatchesCrop(e, effect.Target, crop))
                    mult *= effect.Mult;
            });
            return mult;
        }

        // Giá bán 1 nông sản lúc này: gốc × giá chợ (ngày × trong ngày, Phần 9) × sự kiện. Đơn hàng không dùng hàm này.
        public int GetSellPrice(string cropId)
        {
            if (!Definition.TryGetCrop(cropId, out CropDef crop))
                return 0;
            float mult = GetCropPriceMult(crop);
            if (Definition.Market.IsEnabled)
            {
                mult *= Market.GetDailyMult(crop.Id, Calendar.DayKey) * Market.GetIntradayMult(crop, Calendar.TimeWindow);
                return MarketService.Round(crop.SellPrice * (double)mult);
            }
            return mult == 1f ? crop.SellPrice : Math.Max(0, (int)Math.Round(crop.SellPrice * mult));
        }

        public float GetFishWeightMult(FishDef fish) => FishMult(EventEffectType.FishWeightMult, fish);

        public float GetFishSizeMult(FishDef fish) => FishMult(EventEffectType.FishSizeMult, fish);

        // Hệ số thời gian lớn nếu gieo lúc này.
        public float GetGrowMultAtPlant()
        {
            float mult = 1f;
            ForEachActive(EventEffectType.GrowTimeMult, (e, effect) => mult *= effect.Mult);
            return mult;
        }

        public bool AutoWaterAtPlant()
        {
            bool on = false;
            ForEachActive(EventEffectType.AutoWater, (e, effect) => on = true);
            return on;
        }

        // Sản lượng cộng thêm nếu gieo cây này lúc này (cây lớn ≥ Hours giờ).
        public int GetYieldBonusAtPlant(CropDef crop)
        {
            int bonus = 0;
            ForEachActive(EventEffectType.YieldBonus, (e, effect) =>
            {
                if (crop.GrowSeconds >= effect.Hours * 3600f)
                    bonus += effect.Count;
            });
            return bonus;
        }

        // Hệ số thưởng đơn nếu giao lúc này (chốt 21/11: lúc giao).
        public void GetOrderMults(OrderState order, out float coinMult, out float xpMult)
        {
            float coins = 1f, xp = 1f;
            ForEachActive(EventEffectType.OrderBonus, (e, effect) =>
            {
                if (!MatchesOrder(effect.Target, order))
                    return;
                coins *= effect.Mult;
                xp *= effect.XpMult;
            });
            coinMult = coins;
            xpMult = xp;
        }

        // Thưởng đơn nếu giao lúc này (thẻ đơn hiện số này).
        public void GetOrderReward(OrderState order, out long coins, out long xp)
        {
            GetOrderMults(order, out float coinMult, out float xpMult);
            coins = coinMult == 1f ? order.RewardCoins : (long)Math.Round(order.RewardCoins * coinMult);
            xp = xpMult == 1f ? order.RewardXp : (long)Math.Round(order.RewardXp * xpMult);
        }

        // Giới hạn đơn hôm nay cộng thêm.
        public int ExtraOrdersToday
        {
            get
            {
                int extra = 0;
                ForEachActive(EventEffectType.ExtraOrder, (e, effect) => extra += effect.Count);
                return extra;
            }
        }

        // Cây eventOnly đang được mở bán (UnlockCrop hôm nay).
        public bool IsCropEventUnlocked(string cropId)
        {
            bool open = false;
            ForEachActive(EventEffectType.UnlockCrop, (e, effect) => open |= effect.Id == cropId);
            return open;
        }

        // Hình ảnh đang bật (mưa, đèn lồng…) – View đọc.
        public void GetOverlays(List<string> result)
        {
            result.Clear();
            ForEachActive(EventEffectType.Overlay, (e, effect) =>
            {
                if (!result.Contains(effect.Id))
                    result.Add(effect.Id);
            });
        }

        // Gửi thư quà sự kiện hôm nay (mỗi (sự kiện, ngày) một lần). View gọi cùng nhịp điểm danh. True nếu có thư mới.
        public bool TryEventMail()
        {
            int day = Calendar.DayKey;
            bool sent = false;
            foreach (EventDef e in TodayEvents)
            {
                if (!e.IsActiveIn(Calendar.TimeWindow))
                    continue;
                foreach (EventEffect effect in e.Effects)
                {
                    if (effect.Type != EventEffectType.MailReward)
                        continue;
                    sent |= AddMail(EventMailId(e.Id, day), "Loa xã", e.Title, e.LoaText, effect.Rewards, 7);
                }
            }
            if (!sent)
                return false;
            CleanupMail(day);
            NotifyChanged();
            return true;
        }

        public static string EventMailId(string eventId, int dayKey) => $"su_kien_{eventId}_{dayKey}";

        // ---------- Hỗ trợ ----------

        private float FishMult(EventEffectType type, FishDef fish)
        {
            float mult = 1f;
            ForEachActive(type, (e, effect) =>
            {
                if (effect.Target.MatchesFish(fish))
                    mult *= effect.Mult;
            });
            return mult;
        }

        private void ForEachActive(EventEffectType type, Action<EventDef, EventEffect> action)
        {
            IReadOnlyList<EventDef> events = TodayEvents;
            if (events.Count == 0)
                return;
            DaySlot slot = Calendar.TimeWindow;
            foreach (EventDef e in events)
            {
                if (!e.IsActiveIn(slot))
                    continue;
                foreach (EventEffect effect in e.Effects)
                {
                    if (effect.Type == type)
                        action(e, effect);
                }
            }
        }

        private bool MatchesCrop(EventDef e, EffectTarget target, CropDef crop)
        {
            switch (target.Kind)
            {
                case EffectTargetKind.All: return true;
                case EffectTargetKind.Crop: return crop.Id == target.Id;
                case EffectTargetKind.Group: return crop.Group.ToString() == target.Id;
                case EffectTargetKind.RandomCrop: return RandomCrops(e, target.Count).Contains(crop.Id);
                default: return false;
            }
        }

        private static bool MatchesOrder(EffectTarget target, OrderState order)
        {
            switch (target.Kind)
            {
                case EffectTargetKind.All: return true;
                case EffectTargetKind.Npc: return order.NpcId == target.Id;
                case EffectTargetKind.FishOrders: return order.IsFish;
                case EffectTargetKind.Crop:
                    if (order.IsFish || order.Items == null)
                        return false;
                    foreach (OrderItem item in order.Items)
                    {
                        if (item.Id == target.Id)
                            return true;
                    }
                    return false;
                default: return false;
            }
        }

        // N cây (không eventOnly) bốc theo seed (ngày, sự kiện) – giống nhau cho mọi người chơi.
        public HashSet<string> RandomCrops(EventDef e, int count)
        {
            if (_randomCrops.TryGetValue(e.Id, out HashSet<string> cached))
                return cached;
            var pool = new List<string>();
            foreach (CropDef crop in Definition.Crops)
            {
                if (!crop.EventOnly)
                    pool.Add(crop.Id);
            }
            uint random = GameCalendar.DaySeed(Calendar.DayKey, "cay_" + e.Id);
            if (random == 0)
                random = 1;
            var picked = new HashSet<string>();
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int index = SeededRandom.Range(ref random, 0, pool.Count);
                picked.Add(pool[index]);
                pool.RemoveAt(index);
            }
            _randomCrops[e.Id] = picked;
            return picked;
        }
    }
}
