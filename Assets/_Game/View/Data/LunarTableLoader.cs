using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Đọc Data/Calendar/lunar_2026_2040.json (Tools/lunar_table.py) → LunarTable (Logic). Đợt 2 Phần 2.
    public static class LunarTableLoader
    {
        [Serializable]
        private sealed class LunarFile
        {
            public string baseDate;
            public int timeZone;
            public LunarRow[] months;
        }

        // s: DayKey ngày đầu tháng, d: số ngày, m: tháng, l: 1 = nhuận, y: năm âm.
        [Serializable]
        private sealed class LunarRow
        {
            public int s;
            public int d;
            public int m;
            public int l;
            public int y;
        }

        public static LunarTable Load(TextAsset json)
        {
            if (json == null)
                return LunarTable.Empty;
            LunarFile file = JsonUtility.FromJson<LunarFile>(json.text);
            if (file?.months == null)
                throw new FormatException($"{json.name}: không đọc được danh sách \"months\".");
            if (file.baseDate != "2026-01-01" || file.timeZone != 7)
                throw new FormatException($"{json.name}: cần baseDate 2026-01-01 và timeZone 7 (GameCalendar).");
            var months = new List<LunarMonth>(file.months.Length);
            foreach (LunarRow row in file.months)
                months.Add(new LunarMonth(row.s, row.d, row.m, row.l != 0, row.y));
            return new LunarTable(months);
        }
    }
}
