using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Đọc Data/Fishing/dex_milestones.json → DexMilestoneDef (Logic). Đợt 2 Phần 5. File sai → báo lỗi rõ ràng.
    public static class DexMilestoneLoader
    {
        [Serializable]
        private sealed class MilestoneFile
        {
            public MilestoneRow[] milestones;
        }

        [Serializable]
        private sealed class MilestoneRow
        {
            public string id;
            // SpeciesCount | RarityComplete
            public string kind;
            public int count;
            // Common | Uncommon | Rare | Epic | Legendary (chỉ dùng với RarityComplete)
            public string rarity;
            public string sender;
            public string title;
            public string body;
            public BuildMailLoader.RewardRow[] rewards;
        }

        public static List<DexMilestoneDef> Load(TextAsset json)
        {
            var result = new List<DexMilestoneDef>();
            if (json == null)
                return result;
            MilestoneFile file = JsonUtility.FromJson<MilestoneFile>(json.text);
            if (file?.milestones == null)
                throw new FormatException($"{json.name}: không đọc được danh sách \"milestones\".");
            foreach (MilestoneRow row in file.milestones)
            {
                if (row == null)
                    continue;
                if (!Enum.TryParse(row.kind, true, out DexMilestoneKind kind) || !Enum.IsDefined(typeof(DexMilestoneKind), kind))
                    throw new FormatException($"{json.name}: mốc '{row.id}' có kind lạ '{row.kind}'.");
                FishRarity rarity = FishRarity.Common;
                if (kind == DexMilestoneKind.RarityComplete
                    && (!Enum.TryParse(row.rarity, true, out rarity) || !Enum.IsDefined(typeof(FishRarity), rarity)))
                    throw new FormatException($"{json.name}: mốc '{row.id}' có rarity lạ '{row.rarity}'.");
                List<Reward> rewards = BuildMailLoader.ParseRewards(row.rewards, json.name, row.id);
                result.Add(new DexMilestoneDef(row.id, kind, row.count, rarity, row.sender, row.title, row.body, rewards));
            }
            return result;
        }
    }
}
