using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Đọc Data/Mail/build_mails.json → BuildMailDef (Logic). Đợt 2 Phần 3. File sai → báo lỗi rõ ràng.
    public static class BuildMailLoader
    {
        [Serializable]
        private sealed class MailFile
        {
            public MailRow[] mails;
        }

        [Serializable]
        private sealed class MailRow
        {
            public string id;
            public string start;
            public string end;
            public string sender;
            public string title;
            public string body;
            public int expireDays;
            public RewardRow[] rewards;
        }

        [Serializable]
        internal sealed class RewardRow
        {
            public string kind;
            public string id;
            public long amount;
        }

        // Dòng quà JSON → Reward (dùng chung với mốc album). owner chỉ để báo lỗi.
        internal static List<Reward> ParseRewards(RewardRow[] rows, string fileName, string owner)
        {
            var rewards = new List<Reward>();
            if (rows == null)
                return rewards;
            foreach (RewardRow r in rows)
            {
                if (r == null)
                    continue;
                if (!Enum.TryParse(r.kind, true, out RewardKind kind) || !Enum.IsDefined(typeof(RewardKind), kind))
                    throw new FormatException($"{fileName}: '{owner}' có loại quà lạ '{r.kind}'.");
                rewards.Add(new Reward(kind, string.IsNullOrEmpty(r.id) ? null : r.id, r.amount));
            }
            return rewards;
        }

        public static List<BuildMailDef> Load(TextAsset json)
        {
            var result = new List<BuildMailDef>();
            if (json == null)
                return result;
            MailFile file = JsonUtility.FromJson<MailFile>(json.text);
            if (file?.mails == null)
                throw new FormatException($"{json.name}: không đọc được danh sách \"mails\".");
            foreach (MailRow row in file.mails)
            {
                if (row == null)
                    continue;
                if (!GameCalendar.TryParseDate(row.start, out int start) || !GameCalendar.TryParseDate(row.end, out int end))
                    throw new FormatException($"{json.name}: thư '{row.id}' cần start / end dạng yyyy-MM-dd.");
                List<Reward> rewards = ParseRewards(row.rewards, json.name, row.id);
                result.Add(new BuildMailDef(row.id, start, end, row.sender, row.title, row.body, rewards, row.expireDays));
            }
            return result;
        }
    }
}
