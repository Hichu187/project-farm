using System;

namespace VeQue.Logic
{
    // "Hôm nay là ngày gì, giờ nào" cho mọi tính năng theo ngày (Đợt 2 Phần 2, thiết kế mục 0).
    // Luôn theo giờ Việt Nam UTC+7 cố định (chốt 20/11/2026, phương án A): mọi người chơi cùng một ngày, một khung giờ,
    // kể cả khi máy ở múi giờ khác. Chống lùi ngày: DayKey không bao giờ nhỏ hơn ngày lớn nhất đã thấy (lưu trong save).
    public sealed class GameCalendar
    {
        public const int OffsetSeconds = 7 * 3600;
        public const long SecondsPerDay = 86400;
        // 01/01/2026 tính bằng số ngày từ 01/01/1970.
        public const long EpochDay = 20454;

        private readonly IClock _clock;
        private readonly FarmState _state;
        private int _lastSeenDayKey;

        public LunarTable Lunar { get; }

        // state khác null: ngày lớn nhất đã thấy đọc / ghi vào FarmState.LastSeenDayKey (để lưu).
        public GameCalendar(IClock clock, LunarTable lunar, FarmState state = null)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Lunar = lunar ?? LunarTable.Empty;
            _state = state;
            _lastSeenDayKey = state != null ? state.LastSeenDayKey : int.MinValue;
        }

        public long NowSeconds => _clock.UtcNowSeconds;

        // Ngày theo đồng hồ, chưa chống lùi.
        public int RawDayKey => DayKeyOf(_clock.UtcNowSeconds);

        // Số ngày kể từ 01/01/2026 (giờ VN), không lùi.
        public int DayKey
        {
            get
            {
                int raw = RawDayKey;
                int seen = _state != null ? _state.LastSeenDayKey : _lastSeenDayKey;
                if (raw > seen)
                {
                    seen = raw;
                    _lastSeenDayKey = raw;
                    if (_state != null)
                        _state.LastSeenDayKey = raw;
                }
                return seen;
            }
        }

        // Số ngày kể từ 01/01/1970 (giờ VN), không lùi – dùng cho giới hạn đơn mỗi ngày.
        public long AbsoluteDay => DayKey + EpochDay;

        // Giây tới 0 giờ hôm sau (giờ VN). Đồng hồ bị lùi về hôm trước thì tính tới hết ngày đã thấy.
        public long SecondsUntilNextDay => (AbsoluteDay + 1) * SecondsPerDay - (_clock.UtcNowSeconds + OffsetSeconds);

        // Khung giờ (7 khung câu cá) theo giờ VN.
        public DaySlot TimeWindow => DaySlots.Get(_clock.UtcNowSeconds, OffsetSeconds);

        public bool TryGetLunar(out LunarDate date) => Lunar.TryGet(DayKey, out date);

        // Số ổn định cho cả ngày + salt: mọi máy cùng ngày ra cùng số (sự kiện, giá chợ…). Không dùng string.GetHashCode.
        public uint DaySeed(string salt) => DaySeed(DayKey, salt);

        public static uint DaySeed(int dayKey, string salt)
        {
            uint hash = 2166136261u;
            if (salt != null)
            {
                foreach (char c in salt)
                {
                    hash ^= c;
                    hash *= 16777619u;
                }
            }
            hash ^= (uint)dayKey;
            hash *= 16777619u;
            return SeededRandom.Mix(hash);
        }

        // DayKey của một ngày dương (giờ VN).
        public static int DayKeyFromDate(int year, int month, int day)
            => (int)(new DateTime(year, month, day) - new DateTime(2026, 1, 1)).TotalDays;

        // "yyyy-MM-dd" → DayKey (data: thư theo build, sự kiện…).
        public static bool TryParseDate(string text, out int dayKey)
        {
            dayKey = 0;
            if (string.IsNullOrEmpty(text))
                return false;
            string[] parts = text.Trim().Split('-');
            if (parts.Length != 3 || !int.TryParse(parts[0], out int y) || !int.TryParse(parts[1], out int m)
                || !int.TryParse(parts[2], out int d))
                return false;
            if (y < 1970 || m < 1 || m > 12 || d < 1 || d > DateTime.DaysInMonth(y, m))
                return false;
            dayKey = DayKeyFromDate(y, m, d);
            return true;
        }

        public static int DayKeyOf(long utcSeconds)
            => (int)(FloorDiv(utcSeconds + OffsetSeconds, SecondsPerDay) - EpochDay);

        private static long FloorDiv(long value, long divisor)
        {
            long quotient = value / divisor;
            return value % divisor < 0 ? quotient - 1 : quotient;
        }
    }
}
