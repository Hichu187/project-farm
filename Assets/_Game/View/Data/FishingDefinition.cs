using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Toàn bộ data câu cá (MVP-Plan Phần 10). FarmDefinition giữ 1 cái; lúc khởi động ToDef() sang FishingDef.
    [CreateAssetMenu(menuName = "Về Quê/Fishing Definition", fileName = "FishingDefinition")]
    public sealed class FishingDefinition : ScriptableObject
    {
        [SerializeField] private FishDefinition[] _fish = Array.Empty<FishDefinition>();
        [SerializeField] private RodDefinition[] _rods = Array.Empty<RodDefinition>();
        [SerializeField] private BaitDefinition[] _baits = Array.Empty<BaitDefinition>();
        [Min(1)] [SerializeField] private int _unlockLevel = 1;

        [Header("Quà khi mở câu cá")]
        [SerializeField] private RodDefinition _starterRod;
        [SerializeField] private BaitDefinition _starterBait;
        [Min(0)] [SerializeField] private int _starterBaitCount = 10;

        [Header("Giỏ cá")]
        [Tooltip("Sức chứa theo mức nâng cấp (phần tử 0 = ban đầu), tăng dần.")]
        [SerializeField] private int[] _basketCapacities = { 8, 10, 12, 16, 20 };
        [Tooltip("Giá lên từng mức (phần tử 0 bỏ qua).")]
        [SerializeField] private long[] _basketUpgradePrices = { 0, 300, 1000, 3000, 8000 };

        [Header("Đào giun khi thu hoạch")]
        [SerializeField] private BaitDefinition _wormBait;
        [Range(0, 100)] [SerializeField] private int _wormChancePercent = 10;
        [Min(0)] [SerializeField] private int _wormMin = 1;
        [Min(0)] [SerializeField] private int _wormMax = 2;

        [Header("Con số chung")]
        [SerializeField] private FishingTuning _tuning = new FishingTuning();

        public IReadOnlyList<FishDefinition> Fish => _fish;
        public IReadOnlyList<RodDefinition> Rods => _rods;
        public IReadOnlyList<BaitDefinition> Baits => _baits;

        public bool TryGetFish(string id, out FishDefinition fish) => TryFind(_fish, id, f => f.Id, out fish);
        public bool TryGetRod(string id, out RodDefinition rod) => TryFind(_rods, id, r => r.Id, out rod);
        public bool TryGetBait(string id, out BaitDefinition bait) => TryFind(_baits, id, b => b.Id, out bait);

        public FishingDef ToDef()
        {
            var fish = new List<FishDef>(_fish.Length);
            for (int i = 0; i < _fish.Length; i++)
                fish.Add(Require(_fish[i], "cá", i).ToDef());
            var rods = new List<RodDef>(_rods.Length);
            for (int i = 0; i < _rods.Length; i++)
                rods.Add(Require(_rods[i], "cần", i).ToDef());
            var baits = new List<BaitDef>(_baits.Length);
            for (int i = 0; i < _baits.Length; i++)
                baits.Add(Require(_baits[i], "mồi", i).ToDef());

            return new FishingDef(fish, rods, baits,
                _starterRod != null ? _starterRod.Id : null,
                _starterBait != null ? _starterBait.Id : null,
                _starterBaitCount, _unlockLevel, _basketCapacities, _basketUpgradePrices,
                _wormBait != null ? _wormBait.Id : null, _wormChancePercent, _wormMin, _wormMax,
                _tuning ?? new FishingTuning());
        }

        private T Require<T>(T item, string label, int index) where T : UnityEngine.Object
        {
            if (item == null)
                throw new InvalidOperationException($"FishingDefinition '{name}': {label} thứ {index} chưa gán.");
            return item;
        }

        private static bool TryFind<T>(T[] items, string id, Func<T, string> getId, out T found) where T : UnityEngine.Object
        {
            if (id != null && items != null)
            {
                foreach (T item in items)
                {
                    if (item != null && getId(item) == id)
                    {
                        found = item;
                        return true;
                    }
                }
            }
            found = null;
            return false;
        }
    }
}
