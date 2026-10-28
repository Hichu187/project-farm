using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Toàn bộ tiến trình người chơi. Dữ liệu thô để Phần 7 lưu/đọc.
    [Serializable]
    public sealed class FarmState
    {
        public long Coins;
        public Dictionary<string, int> Seeds = new Dictionary<string, int>();
        public Dictionary<string, int> Inventory = new Dictionary<string, int>();
        public ZoneState[] Zones = Array.Empty<ZoneState>();

        public int GetSeeds(string cropId) => GetCount(Seeds, cropId);
        public int GetItems(string cropId) => GetCount(Inventory, cropId);

        public static FarmState CreateNew(FarmDef def)
        {
            if (def == null)
                throw new ArgumentNullException(nameof(def));

            var state = new FarmState
            {
                Coins = def.StartCoins,
                Zones = new ZoneState[def.Zones.Count],
            };
            for (int i = 0; i < def.Zones.Count; i++)
                state.Zones[i] = ZoneState.CreateLocked(def.Zones[i]);
            foreach (KeyValuePair<string, int> pair in def.StartSeeds)
                state.Seeds[pair.Key] = pair.Value;

            state.GrantAvailableStartPlots(def);
            return state;
        }

        // Mở các ô miễn phí của mọi khu đã được mở khóa (khu trước đã mở hết ô). Gọi lại nhiều lần không sao.
        internal void GrantAvailableStartPlots(FarmDef def)
        {
            for (int i = 0; i < Zones.Length; i++)
            {
                if (i > 0 && !Zones[i - 1].IsFull)
                    return;

                ZoneState zone = Zones[i];
                int start = def.Zones[i].StartOpenCount;
                while (zone.OpenCount < start)
                    zone.OpenNext();
            }
        }

        internal static void AddCount(Dictionary<string, int> counts, string cropId, int delta)
        {
            int value = GetCount(counts, cropId) + delta;
            if (value > 0)
                counts[cropId] = value;
            else
                counts.Remove(cropId);
        }

        private static int GetCount(Dictionary<string, int> counts, string cropId)
        {
            if (cropId == null)
                return 0;
            return counts.TryGetValue(cropId, out int value) ? value : 0;
        }
    }
}
