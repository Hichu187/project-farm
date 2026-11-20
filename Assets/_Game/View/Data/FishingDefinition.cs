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
        [Tooltip("Số ô mở sẵn.")]
        [Min(1)] [SerializeField] private int _basketStartSlots = 8;
        [Tooltip("Giá từng ô mở thêm, lần lượt (phần tử 0 = ô thứ Start + 1). Tối đa = Start + số phần tử.")]
        [SerializeField] private long[] _basketSlotPrices = System.Array.Empty<long>();

        [Header("Đào giun khi thu hoạch")]
        [SerializeField] private BaitDefinition _wormBait;
        [Range(0, 100)] [SerializeField] private int _wormChancePercent = 10;
        [Min(0)] [SerializeField] private int _wormMin = 1;
        [Min(0)] [SerializeField] private int _wormMax = 2;

        [Header("Con số chung")]
        [SerializeField] private FishingTuning _tuning = new FishingTuning();

        [Header("Cá shiny")]
        [Tooltip("Bảng 8 kiểu (menu Về Quê → Cá shiny → Tạo material). Tham chiếu ở đây để shader vào build.")]
        [SerializeField] private ShinyPalette _shinyPalette;

        public IReadOnlyList<FishDefinition> Fish => _fish;
        public IReadOnlyList<RodDefinition> Rods => _rods;
        public IReadOnlyList<BaitDefinition> Baits => _baits;
        public ShinyPalette ShinyPalette => _shinyPalette;

        // Material của kiểu shiny; cá thường / chưa có bảng → null.
        public Material GetShinyMaterial(ShinyKind kind) => _shinyPalette != null ? _shinyPalette.GetMaterial(kind) : null;

        // Dòng bảng màu của kiểu shiny (material, tên, màu đốm); cá thường / chưa có bảng → null.
        public ShinyPalette.Entry GetShiny(ShinyKind kind)
            => _shinyPalette != null && _shinyPalette.TryGet(kind, out ShinyPalette.Entry entry) ? entry : null;

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
                _starterBaitCount, _unlockLevel, _basketStartSlots, _basketSlotPrices,
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
