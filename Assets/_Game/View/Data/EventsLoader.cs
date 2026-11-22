using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Đọc Data/Events/events.json → EventsDef (Logic). Đợt 2 Phần 7. Không có file = không có sự kiện; file sai → báo lỗi rõ ràng.
    // JsonUtility không đọc được danh sách nhiều kiểu → mỗi hiệu ứng là một dòng phẳng, loader dựng đúng EventEffect.
    public static class EventsLoader
    {
        [Serializable]
        private sealed class EventsFile
        {
            public int randomChancePercent = 25;
            public EventRow[] events;
        }

        [Serializable]
        private sealed class EventRow
        {
            public string id;
            public string title;
            public string loa;
            // Người gửi thư quà (MailReward); trống = "Loa xã".
            public string mailSender;
            public int priority;
            public int weight = 1;
            public int cooldownDays;
            public int minLevel = 1;
            public TriggerRow trigger;
            public string[] windows;
            public EffectRow[] effects;
        }

        [Serializable]
        private sealed class TriggerRow
        {
            public string kind = "Random";
            public int day;
            public int month;
            public int lengthDays = 1;
        }

        [Serializable]
        private sealed class EffectRow
        {
            public string type;
            public string target;
            public float mult = 1f;
            public float xpMult = 1f;
            public int count;
            public float hours;
            public string id;
            public BuildMailLoader.RewardRow[] rewards;
        }

        public static EventsDef Load(TextAsset json)
        {
            if (json == null)
                return EventsDef.None;
            EventsFile file = JsonUtility.FromJson<EventsFile>(json.text);
            if (file?.events == null)
                throw new FormatException($"{json.name}: không đọc được danh sách \"events\".");
            var events = new List<EventDef>();
            foreach (EventRow row in file.events)
            {
                if (row == null)
                    continue;
                string where = $"{json.name}: sự kiện '{row.id}'";
                try
                {
                    events.Add(ToDef(row, json.name));
                }
                catch (FormatException e)
                {
                    throw new FormatException($"{where}: {e.Message}");
                }
                catch (ArgumentException e)
                {
                    throw new FormatException($"{where}: {e.Message}");
                }
            }
            return new EventsDef(events, file.randomChancePercent);
        }

        private static EventDef ToDef(EventRow row, string fileName)
        {
            TriggerRow t = row.trigger ?? new TriggerRow();
            if (!Enum.TryParse(t.kind, true, out EventTriggerKind kind) || !Enum.IsDefined(typeof(EventTriggerKind), kind))
                throw new FormatException($"trigger.kind lạ '{t.kind}'.");
            EventTrigger trigger = kind == EventTriggerKind.Random ? EventTrigger.Random : new EventTrigger(kind, t.day, t.month, t.lengthDays);

            var windows = new List<DaySlot>();
            if (row.windows != null)
            {
                foreach (string w in row.windows)
                {
                    if (!Enum.TryParse(w, true, out DaySlot slot) || !Enum.IsDefined(typeof(DaySlot), slot))
                        throw new FormatException($"khung giờ lạ '{w}'.");
                    windows.Add(slot);
                }
            }

            var effects = new List<EventEffect>();
            if (row.effects != null)
            {
                foreach (EffectRow e in row.effects)
                {
                    if (e == null)
                        continue;
                    if (!Enum.TryParse(e.type, true, out EventEffectType type) || !Enum.IsDefined(typeof(EventEffectType), type))
                        throw new FormatException($"loại hiệu ứng lạ '{e.type}'.");
                    effects.Add(new EventEffect(type, EffectTarget.Parse(e.target), e.mult, e.xpMult, e.count, e.hours,
                        string.IsNullOrEmpty(e.id) ? null : e.id, BuildMailLoader.ParseRewards(e.rewards, fileName, row.id)));
                }
            }
            return new EventDef(row.id, row.title, row.loa, trigger, effects, row.priority, row.weight, row.cooldownDays,
                Math.Max(1, row.minLevel), windows, row.mailSender);
        }
    }
}
