using System;

namespace VeQue.Logic
{
    // Cấu trúc file lưu. Dùng mảng và trường public để JsonUtility đọc được; đổi cấu trúc thì tăng CurrentVersion.
    [Serializable]
    public sealed class SaveData
    {
        // 2: thêm orders, randomState (save 1 nạp được, đơn sinh mới).
        // 3: thêm character (save 1, 2 nạp được: chưa tạo nhân vật, tóc mặc định).
        // 4: thêm hạng / hạn đơn, giới hạn đơn mỗi ngày, múi giờ (save 1–3 nạp được: đơn thường, lượt hôm nay = 0).
        // 5: thêm fishing – cần, mồi, giỏ cá, kỷ lục (save 1–4 nạp được: chưa có gì, được tặng cần + mồi khi mở).
        // 6: thêm repairCount của cần (save 1–5 nạp được: chưa sửa lần nào).
        public const int CurrentVersion = 6;

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
        public int utcOffsetSeconds;
        public SaveFishing fishing;
    }

    [Serializable]
    public sealed class SaveFishing
    {
        public SaveRod[] rods;
        public string equippedRod;
        public SaveCount[] baits;
        public SaveFish[] basket;
        public int basketLevel;
        // Kỷ lục cân nặng (gam) theo loài.
        public SaveCount[] records;
        public bool starterGranted;
    }

    [Serializable]
    public sealed class SaveRod
    {
        public string id;
        public int durability;
        public bool snapped;
        public int repairCount;
    }

    [Serializable]
    public sealed class SaveFish
    {
        public string id;
        public int grams;
        public long caughtAt;
        // DaySlot (0–6).
        public int slot;
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
        public string customer;
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
    }

    public enum SaveLoadResult
    {
        Ok = 0,
        Invalid = 1,
        NewerVersion = 2,
    }
}
