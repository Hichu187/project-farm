using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Giá chợ theo ngày (Đợt 2 Phần 9, duyệt 22/11/2026). Giá bán = gốc × hệ số ngày × hệ số trong ngày (× sự kiện, Farm nhân).
    // - Hệ số ngày: noise theo seed ngày (giờ VN) trong [DailyMin, DailyMax]; mỗi ngày 1 cây "hút hàng" × HotMult và
    //   1 cây "rớt giá" × ColdMult (khác nhau, không lấy cây eventOnly). Giống nhau cho mọi người chơi, không lưu gì.
    // - Hệ số trong ngày: chỉ rau lá – Bình minh + Sáng +LeafyMorningPercent%, Khuya −LeafyNightPercent%.
    // Đơn hàng tính theo giá gốc (không qua đây).
    public sealed class MarketDef
    {
        // Tắt giá chợ: mọi hệ số = 1.
        public static readonly MarketDef None = new MarketDef(1f, 1f, 1f, 1f, 0, 0);

        public float DailyMin { get; }
        public float DailyMax { get; }
        public float HotMult { get; }
        public float ColdMult { get; }
        public int LeafyMorningPercent { get; }
        public int LeafyNightPercent { get; }

        public bool IsEnabled => DailyMin != 1f || DailyMax != 1f || HotMult != 1f || ColdMult != 1f
            || LeafyMorningPercent != 0 || LeafyNightPercent != 0;

        public MarketDef(float dailyMin = 0.85f, float dailyMax = 1.15f, float hotMult = 1.4f, float coldMult = 0.7f,
            int leafyMorningPercent = 10, int leafyNightPercent = 10)
        {
            if (dailyMin <= 0f || dailyMax < dailyMin)
                throw new ArgumentOutOfRangeException(nameof(dailyMax), "Biên độ giá ngày không hợp lệ.");
            if (hotMult <= 0f || coldMult <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hotMult), "Hệ số hút hàng / rớt giá phải > 0.");
            if (leafyMorningPercent < 0 || leafyNightPercent < 0 || leafyNightPercent >= 100)
                throw new ArgumentOutOfRangeException(nameof(leafyNightPercent));
            DailyMin = dailyMin;
            DailyMax = dailyMax;
            HotMult = hotMult;
            ColdMult = coldMult;
            LeafyMorningPercent = leafyMorningPercent;
            LeafyNightPercent = leafyNightPercent;
        }
    }

    public sealed class MarketService
    {
        private readonly MarketDef _def;
        private readonly List<CropDef> _pool = new List<CropDef>();
        // Cache hệ số của 1 ngày gần nhất (Kho hỏi từng dòng mỗi lần vẽ lại).
        private int _cacheDay = int.MinValue;
        private readonly Dictionary<string, float> _cache = new Dictionary<string, float>();
        private string _cacheHot;
        private string _cacheCold;

        public MarketService(MarketDef def, IEnumerable<CropDef> crops)
        {
            _def = def ?? MarketDef.None;
            foreach (CropDef crop in crops ?? Array.Empty<CropDef>())
            {
                if (!crop.EventOnly)
                    _pool.Add(crop);
            }
        }

        public MarketDef Definition => _def;

        // Cây hút hàng / rớt giá của ngày; null nếu tắt giá chợ hoặc không đủ 2 cây.
        public string GetHotCrop(int dayKey)
        {
            Fill(dayKey);
            return _cacheHot;
        }

        public string GetColdCrop(int dayKey)
        {
            Fill(dayKey);
            return _cacheCold;
        }

        // Hệ số ngày của cây (đã gồm hút hàng / rớt giá). Cây không có trong data / cây sự kiện → 1.
        public float GetDailyMult(string cropId, int dayKey)
        {
            Fill(dayKey);
            return cropId != null && _cache.TryGetValue(cropId, out float mult) ? mult : 1f;
        }

        public float GetIntradayMult(CropDef crop, DaySlot slot)
        {
            if (crop == null || crop.Group != CropGroup.LeafyGreen)
                return 1f;
            if (slot == DaySlot.Dawn || slot == DaySlot.Morning)
                return 1f + _def.LeafyMorningPercent / 100f;
            if (slot == DaySlot.Night)
                return 1f - _def.LeafyNightPercent / 100f;
            return 1f;
        }

        // Giá ngày (không gồm hệ số trong ngày / sự kiện) – biểu đồ 7 ngày, so với hôm qua.
        public int GetDailyPrice(CropDef crop, int dayKey) => Round(crop.SellPrice * GetDailyMult(crop.Id, dayKey));

        public static int Round(double price) => Math.Max(1, (int)Math.Round(price));

        private void Fill(int dayKey)
        {
            if (dayKey == _cacheDay)
                return;
            _cacheDay = dayKey;
            _cache.Clear();
            _cacheHot = null;
            _cacheCold = null;
            if (!_def.IsEnabled || _pool.Count == 0)
                return;

            float span = _def.DailyMax - _def.DailyMin;
            foreach (CropDef crop in _pool)
            {
                uint hash = GameCalendar.DaySeed(dayKey, "gia_" + crop.Id);
                float u = (hash & 0xFFFFFF) / 16777216f;
                _cache[crop.Id] = _def.DailyMin + span * u;
            }
            if (_pool.Count < 2)
                return;
            int hot = (int)(GameCalendar.DaySeed(dayKey, "cho_hut_hang") % (uint)_pool.Count);
            int cold = (int)(GameCalendar.DaySeed(dayKey, "cho_rot_gia") % (uint)(_pool.Count - 1));
            if (cold >= hot)
                cold++;
            _cacheHot = _pool[hot].Id;
            _cacheCold = _pool[cold].Id;
            _cache[_cacheHot] *= _def.HotMult;
            _cache[_cacheCold] *= _def.ColdMult;
        }
    }
}
