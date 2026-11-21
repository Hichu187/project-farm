using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Album cá + câu rác (Đợt 2 Phần 5, duyệt 21/11/2026).

    // Một loài đã câu được: số lần, kỷ lục cân nặng (gam), ngày câu được lần đầu (GameCalendar.DayKey).
    public sealed class FishDexEntry
    {
        public int Count;
        public int MaxGrams;
        public int FirstDayKey;
    }

    // Một món rác đã vớt được (tab "Đồ vớt được").
    public sealed class JunkDexEntry
    {
        public int Count;
        public int FirstDayKey;
    }

    // Rác đang nằm trong giỏ (chiếm 1 ô như cá).
    public sealed class CaughtJunk
    {
        public string JunkId;
        public long CaughtAt;
    }

    // Một món rác (6 món thường + hũ tiền xu cổ hiếm). Chọn theo trọng số khi phiên câu ra rác.
    public sealed class JunkDef
    {
        public string Id { get; }
        public int SellPrice { get; }
        public int Weight { get; }

        public JunkDef(string id, int sellPrice, int weight)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Rác cần id.", nameof(id));
            if (sellPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(sellPrice), $"Giá rác '{id}' không hợp lệ.");
            if (weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(weight), $"Trọng số rác '{id}' phải > 0.");
            Id = id;
            SellPrice = sellPrice;
            Weight = weight;
        }
    }

    public enum DexMilestoneKind
    {
        // Câu được ít nhất Count loài khác nhau.
        SpeciesCount = 0,
        // Câu được đủ mọi loài có độ hiếm Rarity.
        RarityComplete = 1,
    }

    // Mốc thưởng album: đạt thì gửi 1 thư (id thư = Id, cố định → hộp thư chặn gửi lại).
    public sealed class DexMilestoneDef
    {
        public string Id { get; }
        public DexMilestoneKind Kind { get; }
        public int Count { get; }
        public FishRarity Rarity { get; }
        public string Sender { get; }
        public string Title { get; }
        public string Body { get; }
        public IReadOnlyList<Reward> Rewards { get; }

        public DexMilestoneDef(string id, DexMilestoneKind kind, int count, FishRarity rarity,
            string sender, string title, string body, IEnumerable<Reward> rewards)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Mốc album cần id.", nameof(id));
            if (!Enum.IsDefined(typeof(DexMilestoneKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind));
            if (kind == DexMilestoneKind.SpeciesCount && count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), $"Mốc album '{id}': số loài phải ≥ 1.");
            Id = id;
            Kind = kind;
            Count = count;
            Rarity = rarity;
            Sender = sender ?? "";
            Title = title ?? "";
            Body = body ?? "";
            Rewards = new List<Reward>(rewards ?? Array.Empty<Reward>()).AsReadOnly();
        }
    }
}
