using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Data → Sự kiện: xem lịch (Đợt 2 Phần 7). Đọc FarmDefinition (events.json + bảng âm lịch):
    // chọn 1 ngày xem ra sự kiện gì (kèm ngày âm, hiệu ứng); bảng 60 ngày tới; đếm số lần mỗi sự kiện trong 60 / 365 ngày.
    // Lịch giống nhau cho mọi người chơi (minLevel chỉ ẩn sự kiện với người chưa đủ cấp).
    public sealed class EventCalendarWindow : EditorWindow
    {
        private const int TableDays = 60;
        private const int YearDays = 365;
        private static readonly DateTime Epoch = new DateTime(2026, 1, 1);

        private int _year;
        private int _month;
        private int _day;
        private FarmDef _def;
        private EventCalendar _calendar;
        private string _error;
        private Vector2 _scroll;

        [MenuItem("Về Quê/Data/Sự kiện: xem lịch", priority = 210)]
        private static void Open()
        {
            var window = GetWindow<EventCalendarWindow>("Lịch sự kiện");
            window.minSize = new Vector2(460f, 400f);
            window.SetToday();
            window.Reload();
        }

        private void SetToday()
        {
            // Ngày theo giờ VN (UTC+7), giống GameCalendar.
            DateTime now = DateTime.UtcNow.AddSeconds(GameCalendar.OffsetSeconds);
            _year = now.Year;
            _month = now.Month;
            _day = now.Day;
        }

        private void Reload()
        {
            _error = null;
            _def = null;
            _calendar = null;
            var farm = AssetDatabase.LoadAssetAtPath<FarmDefinition>(DefaultDataGenerator.FarmPath);
            if (farm == null)
            {
                _error = "Chưa có FarmDefinition. Chạy Về Quê → Data → Tạo data mặc định.";
                return;
            }
            try
            {
                _def = farm.ToDef();
                _calendar = new EventCalendar(_def.Events, _def.Lunar);
            }
            catch (Exception e)
            {
                _error = "Data lỗi: " + e.Message;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            _day = EditorGUILayout.IntField("Ngày", _day);
            _month = EditorGUILayout.IntField("Tháng", _month);
            _year = EditorGUILayout.IntField("Năm", _year);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Hôm nay"))
                SetToday();
            if (GUILayout.Button("Nạp lại data"))
                Reload();
            EditorGUILayout.EndHorizontal();

            if (_error != null)
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
                return;
            }
            if (_calendar == null)
                return;
            if (!TryDayKey(out int dayKey))
            {
                EditorGUILayout.HelpBox("Ngày không hợp lệ (từ 01/01/2026).", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"Tỉ lệ ngày thường có sự kiện ngẫu nhiên: {_def.Events.RandomChancePercent}% · {_def.Events.Events.Count} sự kiện trong data",
                EditorStyles.miniLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{DateText(dayKey)}  ({LunarText(dayKey)})", EditorStyles.boldLabel);
            List<EventDef> today = _calendar.GetEvents(dayKey);
            if (today.Count == 0)
                EditorGUILayout.LabelField("Ngày thường.");
            foreach (EventDef e in today)
                DrawEvent(e);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{TableDays} ngày tới", EditorStyles.boldLabel);
            for (int d = dayKey; d < dayKey + TableDays; d++)
                EditorGUILayout.LabelField($"{DateText(d)}  {LunarText(d),-14}", Titles(_calendar.GetEvents(d)));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Phân bố", EditorStyles.boldLabel);
            DrawCounts(dayKey, TableDays);
            DrawCounts(dayKey, YearDays);
            EditorGUILayout.EndScrollView();
        }

        private void DrawEvent(EventDef e)
        {
            string kind = e.Trigger.Kind == EventTriggerKind.Random
                ? $"ngẫu nhiên, trọng số {e.Weight}, cooldown {e.CooldownDays} ngày"
                : $"{e.Trigger.Kind} {e.Trigger.Day:00}/{e.Trigger.Month:00} × {e.Trigger.LengthDays} ngày, ưu tiên {e.Priority}";
            string windows = e.Windows.Count == 0 ? "cả ngày" : string.Join(", ", e.Windows);
            EditorGUILayout.LabelField($"• {e.Title} ({e.Id})", $"{kind} · {windows} · cấp ≥ {e.MinLevel}");
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(e.LoaText, EditorStyles.wordWrappedMiniLabel);
            foreach (EventEffect effect in e.Effects)
                EditorGUILayout.LabelField(EffectText(effect), EditorStyles.miniLabel);
            EditorGUI.indentLevel--;
        }

        private void DrawCounts(int from, int days)
        {
            var counts = new Dictionary<string, int>();
            int eventDays = 0;
            for (int d = from; d < from + days; d++)
            {
                List<EventDef> events = _calendar.GetEvents(d);
                if (events.Count > 0)
                    eventDays++;
                foreach (EventDef e in events)
                    counts[e.Title] = (counts.TryGetValue(e.Title, out int n) ? n : 0) + 1;
            }
            var text = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in counts)
                text.Append(pair.Key).Append(": ").Append(pair.Value).Append("   ");
            EditorGUILayout.LabelField($"{days} ngày: {eventDays} ngày có sự kiện", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(text.Length > 0 ? text.ToString() : "(không có)", EditorStyles.wordWrappedMiniLabel);
        }

        private static string EffectText(EventEffect effect)
        {
            string target = effect.Target.Kind == EffectTargetKind.All ? "" : $" [{effect.Target.Kind} {effect.Target.Id}{(effect.Target.Count > 0 ? effect.Target.Count.ToString() : "")}]";
            switch (effect.Type)
            {
                case EventEffectType.OrderBonus: return $"{effect.Type}{target}: xu ×{effect.Mult}, XP ×{effect.XpMult}";
                case EventEffectType.YieldBonus: return $"{effect.Type}: +{effect.Count} cho cây ≥ {effect.Hours} giờ";
                case EventEffectType.ExtraOrder: return $"{effect.Type}: +{effect.Count} đơn";
                case EventEffectType.UnlockCrop:
                case EventEffectType.Overlay: return $"{effect.Type}: {effect.Id}";
                case EventEffectType.AutoWater: return "AutoWater";
                case EventEffectType.MailReward: return $"{effect.Type}: {effect.Rewards.Count} quà";
                default: return $"{effect.Type}{target}: ×{effect.Mult}";
            }
        }

        private static string Titles(List<EventDef> events)
        {
            if (events.Count == 0)
                return "–";
            var titles = new List<string>();
            foreach (EventDef e in events)
                titles.Add(e.Title);
            return string.Join(" + ", titles);
        }

        private bool TryDayKey(out int dayKey)
        {
            dayKey = 0;
            if (_year < 2026 || _month < 1 || _month > 12 || _day < 1 || _day > DateTime.DaysInMonth(_year, _month))
                return false;
            dayKey = GameCalendar.DayKeyFromDate(_year, _month, _day);
            return dayKey >= 0;
        }

        private static string DateText(int dayKey) => Epoch.AddDays(dayKey).ToString("ddd dd/MM/yyyy");

        private string LunarText(int dayKey)
            => _def.Lunar.TryGet(dayKey, out LunarDate l) ? $"âm {l.Day}/{l.Month}{(l.IsLeapMonth ? " nhuận" : "")}" : "âm ?";
    }
}
