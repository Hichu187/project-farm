using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Đọc Data/Mail/check_in.json → CheckInDef (Logic). Đợt 2 Phần 10. Không có file = tắt điểm danh; file sai → báo lỗi rõ ràng.
    public static class CheckInLoader
    {
        [Serializable]
        private sealed class CheckInFile
        {
            public int minHoursBetween = 18;
            public int coinBonusPercentPerCycle = 10;
            public int maxCoinBonusPercent = 100;
            public int expireDays = 7;
            public string sender;
            public string title;
            public string body;
            public DayRow[] days;
        }

        [Serializable]
        private sealed class DayRow
        {
            public BuildMailLoader.RewardRow[] rewards;
        }

        public static CheckInDef Load(TextAsset json)
        {
            if (json == null)
                return CheckInDef.None;
            CheckInFile file = JsonUtility.FromJson<CheckInFile>(json.text);
            if (file?.days == null || file.days.Length != CheckInDef.DayCount)
                throw new FormatException($"{json.name}: cần đúng {CheckInDef.DayCount} ngày trong \"days\".");
            var days = new List<IEnumerable<Reward>>(CheckInDef.DayCount);
            for (int i = 0; i < file.days.Length; i++)
                days.Add(BuildMailLoader.ParseRewards(file.days[i]?.rewards, json.name, $"ngày {i + 1}"));
            return new CheckInDef(days, file.minHoursBetween, file.coinBonusPercentPerCycle, file.maxCoinBonusPercent,
                file.sender, file.title, file.body, file.expireDays);
        }
    }
}
