using System;

namespace VeQue.Logic
{
    // Cấu trúc file lưu. Dùng mảng và trường public để JsonUtility đọc được; đổi cấu trúc thì tăng CurrentVersion.
    [Serializable]
    public sealed class SaveData
    {
        // 8: bộ 25 cây thuần Việt (Đợt 2 Phần 4).
        // 9: sửa cần không giảm độ bền tối đa – bỏ repairCount, snapped của cần; bỏ basketLevel (Đợt 2 Phần 1).
        // 10: cá shiny – SaveFish.shiny, SaveFishing.shinyDex (Đợt 2 Phần 18).
        // 11: đơn có thoại + đơn cá – SaveOrder bỏ customer, thêm kind / npc / line / seed; orderLines, lastThanks (Đợt 2 Phần 11).
        // 12: GameCalendar UTC+7 cố định – bỏ utcOffsetSeconds, thêm lastSeenDayKey (Đợt 2 Phần 2).
        // 13: hộp thư – mailbox (Đợt 2 Phần 3).
        // 14: album + rác – SaveFishing bỏ records, thêm dex / junk / junkDex / autoSellJunk (Đợt 2 Phần 5).
        // 15: điểm danh – checkIn (Đợt 2 Phần 10).
        // 16: sự kiện – SavePlot thêm growSeconds, bonusYield (chốt lúc gieo, Đợt 2 Phần 7).
        public const int CurrentVersion = 16;
        // Đợt 2, trước bản thử kín: không migration – save cũ hơn mốc này bị bỏ, tạo save mới.
        // Phần nào đổi cấu trúc save thì tăng cả CurrentVersion và MinSupportedVersion.
        public const int MinSupportedVersion = 16;

        public int version;
        public long coins;
        // Save cũ không có 2 trường này: JsonUtility để 0 → đọc ra cấp 1, XP 0.
        public int level;
        public long xp;
        // Thời điểm lớn nhất đã thấy, để đồng hồ không chạy lùi khi người chơi chỉnh giờ máy.
        public long lastSeenAt;
        public SaveCount[] seeds;
        public SaveCount[] inventory;
        public SaveZone[] zones;
        public SaveOrder[] orders;
        // Trạng thái SeededRandom (uint), lưu dạng long cho chắc với JsonUtility.
        public long randomState;
        public SaveCharacter character;
        // Chỉ số ngày (giờ địa phương) và số đơn đã giao trong ngày đó; 0 = chưa có.
        public long orderDay;
        public int deliveredToday;
        // Ngày lớn nhất đã thấy (GameCalendar.DayKey) – chống lùi ngày.
        public int lastSeenDayKey = int.MinValue;
        public SaveMailbox mailbox;
        public SaveCheckIn checkIn;
        public SaveFishing fishing;
        // Đơn có thoại: câu đặt đơn vừa dùng của từng người (id người → chỉ số câu), câu cảm ơn vừa dùng (-1 = chưa có).
        public SaveCount[] orderLines;
        public int lastThanks = -1;
    }

    [Serializable]
    public sealed class SaveFishing
    {
        public SaveRod[] rods;
        public string equippedRod;
        public SaveCount[] baits;
        public SaveFish[] basket;
        // Số ô giỏ đã mở.
        public int basketSlots;
        public bool starterGranted;
        // Các cặp (loài, kiểu shiny) đã câu được.
        public SaveShiny[] shinyDex;
        // Album: loài đã câu được (số lần, kỷ lục gam, ngày đầu tiên).
        public SaveDexFish[] dex;
        // Rác trong giỏ, album rác, tự bán rác.
        public SaveJunk[] junk;
        public SaveDexJunk[] junkDex;
        public bool autoSellJunk;
    }

    [Serializable]
    public sealed class SaveDexFish
    {
        public string id;
        public int count;
        public int maxGrams;
        public int firstDay;
    }

    [Serializable]
    public sealed class SaveDexJunk
    {
        public string id;
        public int count;
        public int firstDay;
    }

    [Serializable]
    public sealed class SaveJunk
    {
        public string id;
        public long caughtAt;
    }

    [Serializable]
    public sealed class SaveCheckIn
    {
        public int nextDay = 1;
        public int cycle;
        public int lastDay = int.MinValue;
        public long lastUtc;
    }

    [Serializable]
    public sealed class SaveMailbox
    {
        public SaveMail[] mails;
        // Mọi id thư đã từng gửi (chống gửi lại thư đã bị dọn).
        public string[] sentIds;
        public int lastRefreshDayKey = int.MinValue;
    }

    [Serializable]
    public sealed class SaveMail
    {
        public string id;
        public string sender;
        public string title;
        public string body;
        public SaveReward[] rewards;
        public int createdDayKey;
        public int expiresDayKey = int.MaxValue;
        public bool read;
        public bool claimed;
        public int claimedDayKey;
    }

    [Serializable]
    public sealed class SaveReward
    {
        // RewardKind (số cố định).
        public int kind;
        public string id;
        public long amount;
    }

    [Serializable]
    public sealed class SaveShiny
    {
        public string fishId;
        // ShinyKind (số cố định).
        public int kind;
        public long firstCaughtAt;
    }

    [Serializable]
    public sealed class SaveRod
    {
        public string id;
        public int durability;
    }

    [Serializable]
    public sealed class SaveFish
    {
        public string id;
        public int grams;
        public long caughtAt;
        // DaySlot (0–6).
        public int slot;
        // ShinyKind (0 = cá thường; kiểu lạ đọc ra None).
        public int shiny;
    }

    [Serializable]
    public sealed class SaveCharacter
    {
        public bool created;
        public string name;
        public string hairStyle;
        public string hairColor;
    }

    // Ô đơn trống: items rỗng, availableAt = lúc có đơn mới, rank = hạng đơn sắp tới.
    // rank: 0 thường, 1 khẩn cấp. expiresAt: đơn khẩn cấp hết hạn lúc này (0 = không).
    [Serializable]
    public sealed class SaveOrder
    {
        // OrderKind (0 nông sản, 1 cá).
        public int kind;
        // Người đặt (id trong npcs.json), câu thoại (chỉ số trong order_lines.csv, -1 = không có), seed của đơn.
        public string npc;
        public int line = -1;
        public long seed;
        public SaveCount[] items;
        public long rewardCoins;
        public long rewardXp;
        public long availableAt;
        public int rank;
        public long expiresAt;
    }

    [Serializable]
    public sealed class SaveCount
    {
        public string id;
        public int count;
    }

    // Ô khóa hoặc trống suy ra từ openCount, nên plots chỉ chứa ô đang có cây.
    [Serializable]
    public sealed class SaveZone
    {
        public string zoneId;
        public int openCount;
        public SavePlot[] plots;
    }

    [Serializable]
    public sealed class SavePlot
    {
        public int bed;
        public int col;
        public int row;
        public string crop;
        public long plantedAt;
        public bool watered;
        // Thời gian lớn đã chốt lúc gieo (0 = theo cây) và sản lượng cộng thêm (Đợt 2 Phần 7).
        public long growSeconds;
        public int bonusYield;
    }

    public enum SaveLoadResult
    {
        Ok = 0,
        Invalid = 1,
        NewerVersion = 2,
        // Save của bản cũ không còn dùng được (version < MinSupportedVersion).
        Outdated = 3,
    }
}
