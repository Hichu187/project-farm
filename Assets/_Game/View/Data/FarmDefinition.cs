using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Data gốc của nông trại. Lúc khởi động gọi ToDef() để chuyển sang class thuần cho VeQue.Logic.
    [CreateAssetMenu(menuName = "Về Quê/Farm Definition", fileName = "FarmDefinition")]
    public sealed class FarmDefinition : ScriptableObject
    {
        [Serializable]
        public struct StartSeed
        {
            public CropDefinition Crop;
            [Min(0)] public int Count;
        }

        [Tooltip("Theo thứ tự mở khóa.")]
        [SerializeField] private ZoneDefinition[] _zones = Array.Empty<ZoneDefinition>();
        [Tooltip("Mọi cây; chỉ cây Enabled được đưa vào game.")]
        [SerializeField] private CropDefinition[] _crops = Array.Empty<CropDefinition>();
        [SerializeField] private long _startCoins = 60;
        [SerializeField] private StartSeed[] _startSeeds = Array.Empty<StartSeed>();
        [Tooltip("Tưới giảm bao nhiêu % tổng thời gian lớn của cây.")]
        [Range(0, 100)] [SerializeField] private int _waterReducePercent = 20;

        [Header("Cấp độ")]
        [Tooltip("Phần tử i = XP để lên từ cấp i+1 lên cấp i+2. Cấp tối đa = số phần tử + 1. Rỗng = không có cấp độ.")]
        [SerializeField] private int[] _xpToNext = Array.Empty<int>();
        [Tooltip("Lên cấp N thưởng N × số này xu.")]
        [Min(0)] [SerializeField] private int _coinsPerLevel = 20;
        [Tooltip("Số hạt tặng cho mỗi cây mở ở cấp vừa lên.")]
        [Min(0)] [SerializeField] private int _unlockSeedCount = 3;
        [Tooltip("Tặng 1 hạt cây này khi người chơi kẹt (hết xu, hạt, nông sản, cây). Bỏ trống = không cứu.")]
        [SerializeField] private CropDefinition _rescueCrop;

        [Header("Đơn hàng")]
        [Tooltip("Số đơn cùng lúc. 0 = không có đơn hàng.")]
        [Min(0)] [SerializeField] private int _orderSlots = 5;
        [Min(1)] [SerializeField] private int _orderUnlockLevel = 2;
        [Tooltip("Thưởng xu = Σ(số lượng × giá bán) × % này, làm tròn 5.")]
        [Min(0)] [SerializeField] private int _orderCoinPercent = 160;
        [Tooltip("Thưởng XP = Σ(số lần thu × XP cây) × % này.")]
        [Min(0)] [SerializeField] private int _orderXpPercent = 200;
        [Tooltip("Bỏ đơn thì sau bao nhiêu phút có đơn mới.")]
        [Min(0)] [SerializeField] private int _orderDiscardMinutes = 5;
        [Tooltip("Số lần thu tối đa của một loại cây trong một đơn.")]
        [Min(1)] [SerializeField] private int _orderHarvestCap = 6;
        [Tooltip("Giao xong thì sau bao nhiêu phút có đơn thường mới.")]
        [Min(0)] [SerializeField] private int _orderRefillMinutes = 10;
        [Tooltip("Đơn kế tiếp là khẩn cấp thì chờ bao nhiêu phút (cả khi giao, bỏ, hết hạn).")]
        [Min(0)] [SerializeField] private int _orderUrgentRefillMinutes = 30;
        [Tooltip("Xác suất đơn kế tiếp là khẩn cấp (%).")]
        [Range(0, 100)] [SerializeField] private int _orderUrgentChancePercent = 20;
        [Tooltip("Đơn khẩn cấp: thưởng xu và XP × % này so với đơn thường cùng món.")]
        [Min(0)] [SerializeField] private int _orderUrgentRewardPercent = 200;
        [Tooltip("Đơn khẩn cấp hết hạn sau bao nhiêu phút kể từ lúc xuất hiện. 0 = không hết hạn.")]
        [Min(0)] [SerializeField] private int _orderUrgentExpireMinutes = 30;
        [Tooltip("Số đơn giao được mỗi ngày (giờ địa phương). Bỏ đơn không tính. 0 = không giới hạn.")]
        [Min(0)] [SerializeField] private int _orderDailyLimit = 15;

        [Header("Đơn hàng có thoại (Đợt 2 Phần 11)")]
        [Tooltip("Data/Orders/npcs.json: người làng (tên, cách xưng, cây hay đặt, trọng số).")]
        [SerializeField] private TextAsset _npcsJson;
        [Tooltip("Data/Orders/order_lines.csv: câu thoại (npcId, kind, text, cropTag).")]
        [SerializeField] private TextAsset _orderLinesCsv;
        [Tooltip("Data/Orders/dishes.json: món theo cây cho chỗ trống {dish}.")]
        [SerializeField] private TextAsset _dishesJson;

        [Tooltip("Chân dung 16×16 theo id người làng; người chưa có thì thẻ đơn hiện khung tròn + chữ cái đầu tên.")]
        [SerializeField] private NpcPortrait[] _npcPortraits = Array.Empty<NpcPortrait>();

        [Serializable]
        public sealed class NpcPortrait
        {
            [SerializeField] private string _npcId;
            [SerializeField] private Sprite _sprite;

            public string NpcId => _npcId;
            public Sprite Sprite => _sprite;
        }

        [Header("Lịch (Đợt 2 Phần 2)")]
        [Tooltip("Data/Calendar/lunar_2026_2040.json – sinh bằng Tools/lunar_table.py. Trống = không có âm lịch.")]
        [SerializeField] private TextAsset _lunarTable;

        [Header("Hộp thư (Đợt 2 Phần 3)")]
        [Tooltip("Data/Mail/build_mails.json: thư phát theo bản build (một lần theo id).")]
        [SerializeField] private TextAsset _buildMails;
        [Tooltip("Data/Mail/check_in.json: điểm danh 7 ngày (Đợt 2 Phần 10). Trống = tắt điểm danh.")]
        [SerializeField] private TextAsset _checkIn;

        [Header("Giá chợ (Đợt 2 Phần 9)")]
        [Tooltip("Bật giá chợ theo ngày. Tắt = bán theo giá gốc (vẫn nhân sự kiện).")]
        [SerializeField] private bool _marketEnabled = true;
        [Tooltip("Hệ số ngày của mỗi cây nằm trong [Min, Max] (noise theo seed ngày, giống nhau mọi người chơi).")]
        [SerializeField] private float _marketDailyMin = 0.85f;
        [SerializeField] private float _marketDailyMax = 1.15f;
        [Tooltip("Mỗi ngày 1 cây hút hàng × Hot, 1 cây rớt giá × Cold.")]
        [SerializeField] private float _marketHotMult = 1.4f;
        [SerializeField] private float _marketColdMult = 0.7f;
        [Tooltip("Rau lá: Bình minh + Sáng +%, Khuya −%.")]
        [Range(0, 50)] [SerializeField] private int _marketLeafyMorningPercent = 10;
        [Range(0, 50)] [SerializeField] private int _marketLeafyNightPercent = 10;

        [Header("Sự kiện (Đợt 2 Phần 7)")]
        [Tooltip("Data/Events/events.json: sự kiện theo ngày (cố định + ngẫu nhiên). Trống = không có sự kiện.")]
        [SerializeField] private TextAsset _events;

        [Header("Đơn cá")]
        [Tooltip("Tỉ lệ đơn cá (%) ở cấp mở câu cá; mỗi cấp sau +1%, tối đa Max. 0 = không có đơn cá.")]
        [Range(0, 100)] [SerializeField] private int _fishOrderMinPercent = 10;
        [Range(0, 100)] [SerializeField] private int _fishOrderMaxPercent = 20;
        [Tooltip("Chỉ đặt loài có độ hiếm từ mức này trở xuống.")]
        [SerializeField] private FishRarity _fishOrderMaxRarity = FishRarity.Uncommon;
        [Tooltip("Chỉ đặt loài câu được ở ít nhất bấy nhiêu khung giờ.")]
        [Min(1)] [SerializeField] private int _fishOrderMinSlots = 3;
        [Tooltip("Số con tối đa một đơn; 1 con ở cấp mở câu cá, +1 mỗi LevelsPerFish cấp.")]
        [Min(1)] [SerializeField] private int _fishOrderMaxCount = 3;
        [Min(1)] [SerializeField] private int _fishOrderLevelsPerFish = 3;

        [Header("Câu cá")]
        [Tooltip("Bỏ trống = chưa có câu cá.")]
        [SerializeField] private FishingDefinition _fishing;

        public IReadOnlyList<ZoneDefinition> Zones => _zones;
        public IReadOnlyList<CropDefinition> Crops => _crops;

        // Chân dung người làng; chưa có → null.
        public Sprite GetNpcPortrait(string npcId)
        {
            if (npcId == null || _npcPortraits == null)
                return null;
            foreach (NpcPortrait portrait in _npcPortraits)
            {
                if (portrait != null && portrait.NpcId == npcId)
                    return portrait.Sprite;
            }
            return null;
        }
        public FishingDefinition Fishing => _fishing;

        public FarmDef ToDef()
        {
            var zones = new List<ZoneDef>(_zones.Length);
            for (int i = 0; i < _zones.Length; i++)
            {
                if (_zones[i] == null)
                    throw new InvalidOperationException($"FarmDefinition '{name}': khu thứ {i} chưa gán.");
                zones.Add(_zones[i].ToDef());
            }

            var crops = new List<CropDef>(_crops.Length);
            for (int i = 0; i < _crops.Length; i++)
            {
                if (_crops[i] == null)
                    throw new InvalidOperationException($"FarmDefinition '{name}': cây thứ {i} chưa gán.");
                if (_crops[i].Enabled)
                    crops.Add(_crops[i].ToDef());
            }

            var seeds = new Dictionary<string, int>();
            foreach (StartSeed seed in _startSeeds)
            {
                if (seed.Crop == null || !seed.Crop.Enabled || seed.Count <= 0)
                    continue;
                seeds.TryGetValue(seed.Crop.Id, out int current);
                seeds[seed.Crop.Id] = current + seed.Count;
            }

            var levels = new LevelDef(_xpToNext ?? Array.Empty<int>(), _coinsPerLevel, _unlockSeedCount);
            string rescueId = _rescueCrop != null && _rescueCrop.Enabled ? _rescueCrop.Id : null;
            var orders = new OrderDef(_orderSlots, _orderUnlockLevel, _orderCoinPercent, _orderXpPercent,
                _orderDiscardMinutes * 60L, OrderDialogueLoader.Load(_npcsJson, _orderLinesCsv, _dishesJson), _orderHarvestCap,
                _orderRefillMinutes * 60L, _orderUrgentRefillMinutes * 60L, _orderUrgentChancePercent,
                _orderUrgentRewardPercent, _orderUrgentExpireMinutes * 60L, _orderDailyLimit,
                _fishOrderMinPercent, Mathf.Max(_fishOrderMinPercent, _fishOrderMaxPercent), _fishOrderMaxRarity,
                _fishOrderMinSlots, _fishOrderMaxCount, _fishOrderLevelsPerFish);
            FishingDef fishing = _fishing != null ? _fishing.ToDef() : null;
            return new FarmDef(zones, crops, _startCoins, seeds, _waterReducePercent, levels, rescueId, orders, fishing,
                LunarTableLoader.Load(_lunarTable), BuildMailLoader.Load(_buildMails), CheckInLoader.Load(_checkIn),
                EventsLoader.Load(_events),
                _marketEnabled
                    ? new MarketDef(_marketDailyMin, _marketDailyMax, _marketHotMult, _marketColdMult,
                        _marketLeafyMorningPercent, _marketLeafyNightPercent)
                    : MarketDef.None);
        }
    }
}
