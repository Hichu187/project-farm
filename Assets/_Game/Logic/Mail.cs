using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Phần quà dùng chung cho mọi hệ thống phát quà (hộp thư, điểm danh, album, sự kiện) – Đợt 2 Phần 3.
    // Số của enum lưu trong save: không đổi số, loại mới thêm số mới.
    public enum RewardKind
    {
        Coins = 0,
        Xp = 1,
        // Hạt giống (Id = id cây).
        Seeds = 2,
        // Nông sản vào Kho (Id = id cây).
        Produce = 3,
        // Mồi câu (Id = id mồi).
        Bait = 4,
    }

    public sealed class Reward
    {
        public RewardKind Kind;
        // Xu / XP: bỏ trống.
        public string Id;
        public long Amount;

        public Reward()
        {
        }

        public Reward(RewardKind kind, string id, long amount)
        {
            Kind = kind;
            Id = id;
            Amount = amount;
        }

        public bool NeedsId => Kind == RewardKind.Seeds || Kind == RewardKind.Produce || Kind == RewardKind.Bait;
    }

    // Một lá thư trong hộp thư (lưu nguyên nội dung trong save, để thư sinh trong code vẫn hiện đúng khi data đổi).
    public sealed class Mail
    {
        public string Id;
        public string Sender;
        public string Title;
        public string Body;
        public List<Reward> Rewards = new List<Reward>();
        public int CreatedDayKey;
        // Hết hạn sau ngày này (GameCalendar.DayKey); NoExpiry = không hết hạn.
        public int ExpiresDayKey = NoExpiry;
        public bool Read;
        public bool Claimed;
        public int ClaimedDayKey;

        public const int NoExpiry = int.MaxValue;

        public bool HasRewards => Rewards != null && Rewards.Count > 0;

        // Còn việc cho người chơi: chưa đọc, hoặc có quà chưa nhận.
        public bool NeedsAttention => !Read || (HasRewards && !Claimed);

        public bool IsExpired(int dayKey) => ExpiresDayKey != NoExpiry && dayKey > ExpiresDayKey;
    }

    // Hộp thư trong FarmState. SentIds ghi mọi id đã từng gửi (kể cả thư đã bị dọn) để không gửi lại.
    public sealed class MailboxState
    {
        public List<Mail> Mails = new List<Mail>();
        public HashSet<string> SentIds = new HashSet<string>();
        // Ngày (DayKey) lần cuối dọn thư + phát thư theo build; int.MinValue = chưa.
        public int LastRefreshDayKey = int.MinValue;

        public bool TryGet(string id, out Mail mail)
        {
            foreach (Mail m in Mails)
            {
                if (m != null && m.Id == id)
                {
                    mail = m;
                    return true;
                }
            }
            mail = null;
            return false;
        }
    }

    // Thư theo bản build (Data/Mail/build_mails.json): phát một lần theo id cho người chơi mở game trong khoảng ngày.
    public sealed class BuildMailDef
    {
        public string Id { get; }
        // Ngày bắt đầu / kết thúc (GameCalendar.DayKey, gồm cả hai ngày).
        public int StartDayKey { get; }
        public int EndDayKey { get; }
        public string Sender { get; }
        public string Title { get; }
        public string Body { get; }
        public IReadOnlyList<Reward> Rewards { get; }
        // Hết hạn sau bao nhiêu ngày kể từ lúc gửi; 0 = không hết hạn.
        public int ExpireDays { get; }

        public BuildMailDef(string id, int startDayKey, int endDayKey, string sender, string title, string body,
            IEnumerable<Reward> rewards, int expireDays = 0)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Thư cần id.", nameof(id));
            if (endDayKey < startDayKey)
                throw new ArgumentException($"Thư '{id}': ngày kết thúc trước ngày bắt đầu.");
            if (expireDays < 0)
                throw new ArgumentOutOfRangeException(nameof(expireDays));
            Id = id;
            StartDayKey = startDayKey;
            EndDayKey = endDayKey;
            Sender = sender ?? "";
            Title = title ?? "";
            Body = body ?? "";
            Rewards = new List<Reward>(rewards ?? Array.Empty<Reward>()).AsReadOnly();
            ExpireDays = expireDays;
        }
    }
}
