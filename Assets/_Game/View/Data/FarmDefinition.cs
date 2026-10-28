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

        public IReadOnlyList<ZoneDefinition> Zones => _zones;
        public IReadOnlyList<CropDefinition> Crops => _crops;

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

            return new FarmDef(zones, crops, _startCoins, seeds);
        }
    }
}
