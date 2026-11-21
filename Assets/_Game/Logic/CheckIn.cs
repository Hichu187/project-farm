using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Điểm danh hằng ngày (Đợt 2 Phần 10, duyệt 21/11/2026): chu kỳ 7 ngày cộng dồn (lỡ ngày không mất chuỗi),
    // quà gửi vào hộp thư. Nhận khi DayKey hôm nay > ngày nhận trước VÀ đã qua ≥ MinHoursBetween giờ thật (chống chỉnh giờ tiến).
    public sealed class CheckInState
    {
        // Ngày sẽ nhận kế tiếp trong chu kỳ (1..CheckInDef.DayCount).
        public int NextDay = 1;
        // Số vòng 7 ngày đã nhận đủ.
        public int Cycle;
        // DayKey + thời điểm (giây UTC) lần nhận gần nhất; int.MinValue = chưa nhận lần nào.
        public int LastClaimDayKey = int.MinValue;
        public long LastClaimUtc;

        public bool HasClaimed => LastClaimDayKey != int.MinValue;
    }

    // Kết quả một lần điểm danh (popup đọc).
    public readonly struct CheckInResult
    {
        // Ngày vừa nhận (1..7) và vòng (0 = vòng đầu).
        public int Day { get; }
        public int Cycle { get; }
        public string MailId { get; }

        public CheckInResult(int day, int cycle, string mailId)
        {
            Day = day;
            Cycle = cycle;
            MailId = mailId;
        }
    }

    // Data điểm danh (Data/Mail/check_in.json). None = tắt điểm danh.
    public sealed class CheckInDef
    {
        public const int DayCount = 7;

        public static readonly CheckInDef None = new CheckInDef();

        private readonly IReadOnlyList<Reward>[] _days;

        public bool IsEnabled => _days != null;
        // Giờ thật tối thiểu giữa 2 lần nhận.
        public int MinHoursBetween { get; }
        // Xu thưởng tăng CoinBonusPercentPerCycle% mỗi vòng đã xong, tối đa MaxCoinBonusPercent%. Mồi / hạt / cần không đổi.
        public int CoinBonusPercentPerCycle { get; }
        public int MaxCoinBonusPercent { get; }
        public string Sender { get; }
        // {day} = ngày trong chu kỳ, {days} = 7.
        public string TitleFormat { get; }
        public string BodyFormat { get; }
        // Thư hết hạn sau bấy nhiêu ngày (0 = không).
        public int ExpireDays { get; }

        private CheckInDef()
        {
        }

        public CheckInDef(IReadOnlyList<IEnumerable<Reward>> days, int minHoursBetween = 18, int coinBonusPercentPerCycle = 10,
            int maxCoinBonusPercent = 100, string sender = "", string titleFormat = "Điểm danh ngày {day}/{days}",
            string bodyFormat = "", int expireDays = 7)
        {
            if (days == null || days.Count != DayCount)
                throw new ArgumentException($"Điểm danh cần đúng {DayCount} ngày quà.", nameof(days));
            if (minHoursBetween < 0 || coinBonusPercentPerCycle < 0 || maxCoinBonusPercent < 0 || expireDays < 0)
                throw new ArgumentOutOfRangeException(nameof(minHoursBetween));
            _days = new IReadOnlyList<Reward>[DayCount];
            for (int i = 0; i < DayCount; i++)
            {
                var list = new List<Reward>(days[i] ?? Array.Empty<Reward>());
                if (list.Count == 0)
                    throw new ArgumentException($"Điểm danh ngày {i + 1} chưa có quà.", nameof(days));
                _days[i] = list.AsReadOnly();
            }
            MinHoursBetween = minHoursBetween;
            CoinBonusPercentPerCycle = coinBonusPercentPerCycle;
            MaxCoinBonusPercent = maxCoinBonusPercent;
            Sender = sender ?? "";
            TitleFormat = titleFormat ?? "";
            BodyFormat = bodyFormat ?? "";
            ExpireDays = expireDays;
        }

        // Quà gốc của ngày (1..7) – data, chưa nhân hệ số vòng.
        public IReadOnlyList<Reward> GetBaseRewards(int day) => _days[day - 1];

        // Quà thực nhận ở ngày (1..7) của vòng cycle: xu nhân (100 + min(cycle × bonus, max))%, làm tròn xuống.
        public List<Reward> GetRewards(int day, int cycle)
        {
            int bonus = Math.Min(Math.Max(0, cycle) * CoinBonusPercentPerCycle, MaxCoinBonusPercent);
            var result = new List<Reward>();
            foreach (Reward reward in _days[day - 1])
            {
                long amount = reward.Kind == RewardKind.Coins ? reward.Amount * (100 + bonus) / 100 : reward.Amount;
                result.Add(new Reward(reward.Kind, reward.Id, amount));
            }
            return result;
        }

        public string FormatTitle(int day) => Fill(TitleFormat, day);
        public string FormatBody(int day) => Fill(BodyFormat, day);

        // Id thư không trùng: vòng (từ 1) + ngày.
        public static string MailId(int cycle, int day) => $"diem_danh_{cycle + 1}_{day}";

        private static string Fill(string format, int day)
            => format.Replace("{day}", day.ToString()).Replace("{days}", DayCount.ToString());
    }
}
