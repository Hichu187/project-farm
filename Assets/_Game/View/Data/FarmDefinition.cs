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
        [SerializeField] private string[] _customerNames = Array.Empty<string>();

        [Header("Câu cá")]
        [Tooltip("Bỏ trống = chưa có câu cá.")]
        [SerializeField] private FishingDefinition _fishing;

        public IReadOnlyList<ZoneDefinition> Zones => _zones;
        public IReadOnlyList<CropDefinition> Crops => _crops;
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
                _orderDiscardMinutes * 60L, _customerNames, _orderHarvestCap,
                _orderRefillMinutes * 60L, _orderUrgentRefillMinutes * 60L, _orderUrgentChancePercent,
                _orderUrgentRewardPercent, _orderUrgentExpireMinutes * 60L, _orderDailyLimit);
            FishingDef fishing = _fishing != null ? _fishing.ToDef() : null;
            return new FarmDef(zones, crops, _startCoins, seeds, _waterReducePercent, levels, rescueId, orders, fishing);
        }
    }
}
