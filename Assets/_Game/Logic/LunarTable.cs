using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Ngày âm lịch Việt Nam (Đợt 2 Phần 2).
    public readonly struct LunarDate
    {
        public readonly int Day;
        public readonly int Month;
        public readonly bool IsLeapMonth;
        public readonly int Year;

        public LunarDate(int day, int month, bool isLeapMonth, int year)
        {
            Day = day;
            Month = month;
            IsLeapMonth = isLeapMonth;
            Year = year;
        }

        public bool IsFirstDay => Day == 1;
        public bool IsFullMoon => Day == 15;
    }

    // Một tháng âm trong bảng: bắt đầu ở StartDayKey (GameCalendar.DayKey), dài Days ngày (29/30).
    public readonly struct LunarMonth
    {
        public readonly int StartDayKey;
        public readonly int Days;
        public readonly int Month;
        public readonly bool IsLeap;
        public readonly int Year;

        public LunarMonth(int startDayKey, int days, int month, bool isLeap, int year)
        {
            StartDayKey = startDayKey;
            Days = days;
            Month = month;
            IsLeap = isLeap;
            Year = year;
        }
    }

    // Bảng tháng âm sinh sẵn (Tools/lunar_table.py, thuật toán Hồ Ngọc Đức, múi giờ +7). Chỉ tra bảng, không tính thiên văn.
    // Ngoài phạm vi bảng: TryGet trả false.
    public sealed class LunarTable
    {
        public static readonly LunarTable Empty = new LunarTable(null);

        private readonly LunarMonth[] _months;

        public int Count => _months.Length;
        public int FirstDayKey => _months.Length > 0 ? _months[0].StartDayKey : 0;
        public int EndDayKey => _months.Length > 0 ? _months[_months.Length - 1].StartDayKey + _months[_months.Length - 1].Days : 0;

        // Các tháng phải liền nhau, theo thứ tự, mỗi tháng 29 hoặc 30 ngày.
        public LunarTable(IEnumerable<LunarMonth> months)
        {
            var list = new List<LunarMonth>();
            if (months != null)
                list.AddRange(months);
            for (int i = 0; i < list.Count; i++)
            {
                LunarMonth m = list[i];
                if (m.Days < 29 || m.Days > 30 || m.Month < 1 || m.Month > 12)
                    throw new ArgumentException($"Tháng âm thứ {i} không hợp lệ.");
                if (i > 0 && list[i - 1].StartDayKey + list[i - 1].Days != m.StartDayKey)
                    throw new ArgumentException($"Bảng âm lịch không liền mạch ở tháng thứ {i}.");
            }
            _months = list.ToArray();
        }

        public bool TryGet(int dayKey, out LunarDate date)
        {
            date = default;
            if (_months.Length == 0 || dayKey < FirstDayKey || dayKey >= EndDayKey)
                return false;
            int lo = 0;
            int hi = _months.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (_months[mid].StartDayKey <= dayKey)
                    lo = mid;
                else
                    hi = mid - 1;
            }
            LunarMonth month = _months[lo];
            date = new LunarDate(dayKey - month.StartDayKey + 1, month.Month, month.IsLeap, month.Year);
            return true;
        }
    }
}
