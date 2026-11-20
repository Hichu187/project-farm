using System.Collections.Generic;

namespace VeQue.Logic
{
    // Một cần người chơi đang có. Hỏng = hết độ bền; sửa thì đầy lại, độ bền tối đa không đổi.
    public sealed class RodState
    {
        public string Id;
        public int Durability;

        public bool IsBroken => Durability <= 0;
    }

    // Cá trong giỏ.
    public sealed class CaughtFish
    {
        public string FishId;
        public int Grams;
        public long CaughtAt;
        public DaySlot Slot;
        public ShinyKind Shiny;

        public bool IsShiny => Shiny != ShinyKind.None;
    }

    // Một cặp (loài, kiểu shiny) đã câu được – Album (Phần 5) đọc từ đây.
    public sealed class ShinyRecord
    {
        public string FishId;
        public ShinyKind Kind;
        public long FirstCaughtAt;
    }

    // Phần câu cá của tiến trình người chơi (lưu trong FarmState).
    public sealed class FishingState
    {
        public List<RodState> Rods = new List<RodState>();
        public string EquippedRodId;
        public Dictionary<string, int> Baits = new Dictionary<string, int>();
        public List<CaughtFish> Basket = new List<CaughtFish>();
        // Số ô giỏ đã mở (FishingDef.BasketStartSlots..BasketMaxSlots; Farm chuẩn hóa lúc nạp).
        public int BasketSlots;
        // Kỷ lục cân nặng (gam) theo loài.
        public Dictionary<string, int> RecordGrams = new Dictionary<string, int>();
        // Đã tặng cần + mồi lúc mở câu cá.
        public bool StarterGranted;
        // Các cặp (loài, kiểu shiny) đã câu được, không trùng.
        public List<ShinyRecord> ShinyDex = new List<ShinyRecord>();

        public bool HasShiny(string fishId, ShinyKind kind)
        {
            foreach (ShinyRecord record in ShinyDex)
            {
                if (record != null && record.FishId == fishId && record.Kind == kind)
                    return true;
            }
            return false;
        }

        public int GetBaits(string baitId) => baitId != null && Baits.TryGetValue(baitId, out int count) ? count : 0;

        public bool TryGetRod(string rodId, out RodState rod)
        {
            foreach (RodState r in Rods)
            {
                if (r != null && r.Id == rodId)
                {
                    rod = r;
                    return true;
                }
            }
            rod = null;
            return false;
        }

        public int GetRecord(string fishId) => fishId != null && RecordGrams.TryGetValue(fishId, out int grams) ? grams : 0;
    }
}
