using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Bộ máy sự kiện "brick" (Đợt 2 Phần 7, duyệt 21/11/2026). Một sự kiện = trigger (khi nào) + windows (khung giờ thật
    // hiệu ứng có tác dụng) + danh sách viên gạch hiệu ứng. Mỗi ngày: mọi sự kiện cố định (dương / âm / âm hằng tháng)
    // đang trong hạn đều chạy; ngày không có sự kiện cố định thì bốc ngẫu nhiên theo seed ngày (RandomChancePercent%),
    // loại sự kiện còn cooldown – tính tuần tự từ DayKey 0 nên không cần lưu gì.

    public enum EventTriggerKind
    {
        Random = 0,
        // Ngày dương cố định dd/mm, kéo dài LengthDays ngày.
        Solar = 1,
        // Ngày âm cố định dd/mm (không tính tháng nhuận), kéo dài LengthDays ngày.
        Lunar = 2,
        // Ngày âm dd mọi tháng (mùng 1, Rằm), kéo dài LengthDays ngày.
        LunarMonthly = 3,
    }

    public sealed class EventTrigger
    {
        public EventTriggerKind Kind { get; }
        public int Day { get; }
        public int Month { get; }
        public int LengthDays { get; }

        public static readonly EventTrigger Random = new EventTrigger(EventTriggerKind.Random, 0, 0, 1);

        public EventTrigger(EventTriggerKind kind, int day, int month, int lengthDays = 1)
        {
            if (!Enum.IsDefined(typeof(EventTriggerKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind));
            if (lengthDays < 1)
                throw new ArgumentOutOfRangeException(nameof(lengthDays), "Sự kiện kéo dài ít nhất 1 ngày.");
            if (kind != EventTriggerKind.Random)
            {
                if (day < 1 || day > 31)
                    throw new ArgumentOutOfRangeException(nameof(day));
                if (kind != EventTriggerKind.LunarMonthly && (month < 1 || month > 12))
                    throw new ArgumentOutOfRangeException(nameof(month));
            }
            Kind = kind;
            Day = day;
            Month = month;
            LengthDays = lengthDays;
        }

        public bool IsFixed => Kind != EventTriggerKind.Random;

        // Ngày dayKey nằm trong [ngày bắt đầu, ngày bắt đầu + LengthDays).
        public bool Matches(int dayKey, LunarTable lunar)
        {
            if (Kind == EventTriggerKind.Random)
                return false;
            for (int k = 0; k < LengthDays; k++)
            {
                if (StartsOn(dayKey - k, lunar))
                    return true;
            }
            return false;
        }

        private bool StartsOn(int dayKey, LunarTable lunar)
        {
            switch (Kind)
            {
                case EventTriggerKind.Solar:
                    DateTime date = Epoch.AddDays(dayKey);
                    return date.Day == Day && date.Month == Month;
                case EventTriggerKind.Lunar:
                    return lunar != null && lunar.TryGet(dayKey, out LunarDate l) && !l.IsLeapMonth && l.Day == Day && l.Month == Month;
                case EventTriggerKind.LunarMonthly:
                    return lunar != null && lunar.TryGet(dayKey, out LunarDate m) && m.Day == Day;
                default:
                    return false;
            }
        }

        private static readonly DateTime Epoch = new DateTime(2026, 1, 1);
    }

    public enum EffectTargetKind
    {
        All = 0,
        Crop = 1,
        Group = 2,
        // N cây bốc theo seed ngày (thương lái về làng).
        RandomCrop = 3,
        Fish = 4,
        Rarity = 5,
        Npc = 6,
        // Đơn cá (OrderBonus).
        FishOrders = 7,
    }

    // Đối tượng hiệu ứng: "all", "crop:id", "group:Fruit", "random:1", "fish:id", "rarity:Rare", "npc:id", "fishorders".
    public readonly struct EffectTarget
    {
        public EffectTargetKind Kind { get; }
        public string Id { get; }
        public int Count { get; }

        public EffectTarget(EffectTargetKind kind, string id = null, int count = 0)
        {
            Kind = kind;
            Id = id;
            Count = count;
        }

        public static readonly EffectTarget All = new EffectTarget(EffectTargetKind.All);

        public static EffectTarget Parse(string text)
        {
            if (string.IsNullOrEmpty(text) || text == "all")
                return All;
            if (text == "fishorders")
                return new EffectTarget(EffectTargetKind.FishOrders);
            int colon = text.IndexOf(':');
            if (colon <= 0 || colon == text.Length - 1)
                throw new FormatException($"Đối tượng hiệu ứng lạ '{text}'.");
            string head = text.Substring(0, colon);
            string tail = text.Substring(colon + 1);
            switch (head)
            {
                case "crop": return new EffectTarget(EffectTargetKind.Crop, tail);
                case "group":
                    if (!Enum.TryParse(tail, true, out CropGroup group) || !Enum.IsDefined(typeof(CropGroup), group))
                        throw new FormatException($"Nhóm cây lạ '{tail}'.");
                    return new EffectTarget(EffectTargetKind.Group, group.ToString());
                case "random":
                    if (!int.TryParse(tail, out int count) || count < 1)
                        throw new FormatException($"Số cây ngẫu nhiên lạ '{tail}'.");
                    return new EffectTarget(EffectTargetKind.RandomCrop, null, count);
                case "fish": return new EffectTarget(EffectTargetKind.Fish, tail);
                case "rarity":
                    if (!Enum.TryParse(tail, true, out FishRarity rarity) || !Enum.IsDefined(typeof(FishRarity), rarity))
                        throw new FormatException($"Độ hiếm lạ '{tail}'.");
                    return new EffectTarget(EffectTargetKind.Rarity, rarity.ToString());
                case "npc": return new EffectTarget(EffectTargetKind.Npc, tail);
                default: throw new FormatException($"Đối tượng hiệu ứng lạ '{text}'.");
            }
        }

        public bool MatchesFish(FishDef fish)
        {
            switch (Kind)
            {
                case EffectTargetKind.All: return true;
                case EffectTargetKind.Fish: return fish.Id == Id;
                case EffectTargetKind.Rarity: return fish.Rarity.ToString() == Id;
                default: return false;
            }
        }
    }

    public enum EventEffectType
    {
        // Giá bán nông sản × Mult (lúc bán).
        CropPriceMult = 0,
        // Trọng số chọn cá × Mult (lúc ném).
        FishWeightMult = 1,
        // Cân nặng cá × Mult (lúc câu được; được vượt cân tối đa, giá theo cân giữ trần).
        FishSizeMult = 2,
        // Thời gian lớn × Mult, chốt LÚC GIEO.
        GrowTimeMult = 3,
        // Gieo xong tự tưới.
        AutoWater = 4,
        // +Count sản lượng cho cây lớn ≥ Hours giờ, chốt LÚC GIEO.
        YieldBonus = 5,
        // Thưởng đơn: xu × Mult, XP × XpMult (lúc giao).
        OrderBonus = 6,
        // Giới hạn đơn trong ngày +Count.
        ExtraOrder = 7,
        // Mở bán hạt cây eventOnly Id.
        UnlockCrop = 8,
        // Quà gửi hộp thư, mỗi ngày 1 lần.
        MailReward = 9,
        // Hình ảnh (mưa, đèn lồng…) – View đọc Id.
        Overlay = 10,
    }

    // Một viên gạch hiệu ứng. Trường nào dùng tùy Type (xem EventEffectType).
    public sealed class EventEffect
    {
        public EventEffectType Type { get; }
        public EffectTarget Target { get; }
        public float Mult { get; }
        public float XpMult { get; }
        public int Count { get; }
        public float Hours { get; }
        public string Id { get; }
        public IReadOnlyList<Reward> Rewards { get; }

        public EventEffect(EventEffectType type, EffectTarget target = default, float mult = 1f, float xpMult = 1f,
            int count = 0, float hours = 0f, string id = null, IEnumerable<Reward> rewards = null)
        {
            if (!Enum.IsDefined(typeof(EventEffectType), type))
                throw new ArgumentOutOfRangeException(nameof(type));
            if (mult < 0f || xpMult < 0f || count < 0 || hours < 0f)
                throw new ArgumentOutOfRangeException(nameof(mult), $"Hiệu ứng {type}: số âm.");
            if ((type == EventEffectType.YieldBonus || type == EventEffectType.ExtraOrder) && count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), $"Hiệu ứng {type} cần count ≥ 1.");
            if ((type == EventEffectType.UnlockCrop || type == EventEffectType.Overlay) && string.IsNullOrEmpty(id))
                throw new ArgumentException($"Hiệu ứng {type} cần id.", nameof(id));
            var list = new List<Reward>(rewards ?? Array.Empty<Reward>());
            if (type == EventEffectType.MailReward && list.Count == 0)
                throw new ArgumentException("Hiệu ứng MailReward cần quà.", nameof(rewards));
            Type = type;
            Target = target;
            Mult = mult;
            XpMult = xpMult;
            Count = count;
            Hours = hours;
            Id = id;
            Rewards = list.AsReadOnly();
        }
    }

    public sealed class EventDef
    {
        public string Id { get; }
        public string Title { get; }
        public string LoaText { get; }
        // Cao hơn hiện trước (loa xã, icon HUD).
        public int Priority { get; }
        // Chỉ dùng cho sự kiện ngẫu nhiên.
        public int Weight { get; }
        public int CooldownDays { get; }
        // Người chơi dưới cấp này coi như ngày thường (không tham gia bốc – lịch giống nhau cho mọi người).
        public int MinLevel { get; }
        public EventTrigger Trigger { get; }
        // Khung giờ thật hiệu ứng có tác dụng; rỗng = cả ngày.
        public IReadOnlyList<DaySlot> Windows { get; }
        public IReadOnlyList<EventEffect> Effects { get; }
        // Người gửi thư quà (MailReward) – Đợt 2 Phần 12a (lì xì của Mẹ, Bà Lụa…). Mặc định "Loa xã".
        public string MailSender { get; }

        public const string DefaultMailSender = "Loa xã";

        public EventDef(string id, string title, string loaText, EventTrigger trigger, IEnumerable<EventEffect> effects,
            int priority = 0, int weight = 1, int cooldownDays = 0, int minLevel = 1, IEnumerable<DaySlot> windows = null,
            string mailSender = null)
        {
            MailSender = string.IsNullOrEmpty(mailSender) ? DefaultMailSender : mailSender;
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Sự kiện cần id.", nameof(id));
            if (weight < 0 || cooldownDays < 0 || minLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(weight), $"Sự kiện '{id}': số không hợp lệ.");
            Id = id;
            Title = title ?? "";
            LoaText = loaText ?? "";
            Trigger = trigger ?? EventTrigger.Random;
            if (!Trigger.IsFixed && weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(weight), $"Sự kiện ngẫu nhiên '{id}' cần trọng số > 0.");
            Effects = new List<EventEffect>(effects ?? Array.Empty<EventEffect>()).AsReadOnly();
            Priority = priority;
            Weight = weight;
            CooldownDays = cooldownDays;
            MinLevel = minLevel;
            Windows = new List<DaySlot>(windows ?? Array.Empty<DaySlot>()).AsReadOnly();
        }

        public bool IsActiveIn(DaySlot slot)
        {
            if (Windows.Count == 0)
                return true;
            foreach (DaySlot window in Windows)
            {
                if (window == slot)
                    return true;
            }
            return false;
        }
    }

    // Toàn bộ data sự kiện (Data/Events/events.json). None = không có sự kiện.
    public sealed class EventsDef
    {
        public static readonly EventsDef None = new EventsDef(Array.Empty<EventDef>(), 0);

        public IReadOnlyList<EventDef> Events { get; }
        // Ngày không có sự kiện cố định: xác suất (%) có sự kiện ngẫu nhiên (chốt 21/11: 25%).
        public int RandomChancePercent { get; }

        public EventsDef(IEnumerable<EventDef> events, int randomChancePercent)
        {
            if (randomChancePercent < 0 || randomChancePercent > 100)
                throw new ArgumentOutOfRangeException(nameof(randomChancePercent));
            var list = new List<EventDef>();
            var ids = new HashSet<string>();
            foreach (EventDef e in events ?? Array.Empty<EventDef>())
            {
                if (e == null)
                    throw new ArgumentException("Có sự kiện null.");
                if (!ids.Add(e.Id))
                    throw new ArgumentException($"Trùng id sự kiện '{e.Id}'.");
                list.Add(e);
            }
            Events = list.AsReadOnly();
            RandomChancePercent = randomChancePercent;
        }
    }

    // Lịch sự kiện theo ngày (không phụ thuộc người chơi). Phần ngẫu nhiên tính tuần tự từ DayKey 0 và nhớ lại,
    // nên cùng một ngày luôn ra cùng kết quả dù hỏi theo thứ tự nào.
    public sealed class EventCalendar
    {
        private readonly EventsDef _def;
        private readonly LunarTable _lunar;
        private readonly List<EventDef> _random = new List<EventDef>();
        private readonly List<EventDef> _fixed = new List<EventDef>();
        private readonly List<EventDef> _randomPool = new List<EventDef>();
        // Ngày được chọn gần nhất của từng sự kiện ngẫu nhiên (khi đã tính tới _random.Count - 1).
        private readonly Dictionary<string, int> _lastPicked = new Dictionary<string, int>();

        public EventCalendar(EventsDef def, LunarTable lunar)
        {
            _def = def ?? EventsDef.None;
            _lunar = lunar;
            foreach (EventDef e in _def.Events)
            {
                if (e.Trigger.IsFixed)
                    _fixed.Add(e);
                else
                    _randomPool.Add(e);
            }
            // Ưu tiên cao trước; cùng ưu tiên giữ thứ tự data.
            var order = new List<EventDef>(_fixed);
            _fixed.Sort((a, b) => b.Priority != a.Priority ? b.Priority.CompareTo(a.Priority) : order.IndexOf(a).CompareTo(order.IndexOf(b)));
        }

        // Sự kiện của ngày (chưa lọc theo cấp người chơi), ưu tiên cao trước.
        public List<EventDef> GetEvents(int dayKey)
        {
            var result = new List<EventDef>();
            AddFixed(dayKey, result);
            if (result.Count == 0)
            {
                EventDef random = GetRandom(dayKey);
                if (random != null)
                    result.Add(random);
            }
            return result;
        }

        public bool HasFixed(int dayKey)
        {
            foreach (EventDef e in _fixed)
            {
                if (e.Trigger.Matches(dayKey, _lunar))
                    return true;
            }
            return false;
        }

        private void AddFixed(int dayKey, List<EventDef> result)
        {
            foreach (EventDef e in _fixed)
            {
                if (e.Trigger.Matches(dayKey, _lunar))
                    result.Add(e);
            }
        }

        // Sự kiện ngẫu nhiên đã bốc cho ngày (null = không có). Ngày trước DayKey 0 không có.
        private EventDef GetRandom(int dayKey)
        {
            if (dayKey < 0 || _randomPool.Count == 0 || _def.RandomChancePercent <= 0)
                return null;
            while (_random.Count <= dayKey)
                _random.Add(Draw(_random.Count));
            return _random[dayKey];
        }

        private EventDef Draw(int dayKey)
        {
            if (HasFixed(dayKey))
                return null;
            if (GameCalendar.DaySeed(dayKey, "su_kien") % 100 >= (uint)_def.RandomChancePercent)
                return null;

            int total = 0;
            foreach (EventDef e in _randomPool)
            {
                if (!InCooldown(e, dayKey))
                    total += e.Weight;
            }
            if (total <= 0)
                return null;
            int pick = (int)(GameCalendar.DaySeed(dayKey, "su_kien_chon") % (uint)total);
            foreach (EventDef e in _randomPool)
            {
                if (InCooldown(e, dayKey))
                    continue;
                if (pick < e.Weight)
                {
                    _lastPicked[e.Id] = dayKey;
                    return e;
                }
                pick -= e.Weight;
            }
            return null;
        }

        // Còn cooldown: lần chọn trước cách chưa quá CooldownDays ngày (cooldown 3 = 3 ngày sau không ra lại).
        private bool InCooldown(EventDef e, int dayKey)
            => _lastPicked.TryGetValue(e.Id, out int last) && dayKey - last <= e.CooldownDays;
    }
}
