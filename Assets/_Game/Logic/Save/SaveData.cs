using System;

namespace VeQue.Logic
{
    // Cấu trúc file lưu. Dùng mảng và trường public để JsonUtility đọc được; đổi cấu trúc thì tăng CurrentVersion.
    [Serializable]
    public sealed class SaveData
    {
        // 2: thêm orders, randomState (save 1 nạp được, đơn sinh mới).
        public const int CurrentVersion = 2;

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
    }

    // Ô đơn trống: items rỗng, availableAt = lúc có đơn mới.
    [Serializable]
    public sealed class SaveOrder
    {
        public string customer;
        public SaveCount[] items;
        public long rewardCoins;
        public long rewardXp;
        public long availableAt;
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
