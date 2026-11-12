using System.Collections.Generic;

namespace VeQue.Logic
{
    // Một cần người chơi đang có. Hỏng = gãy (khi đứt dây) hoặc hết độ bền; sửa thì đầy lại
    // nhưng độ bền tối đa giảm theo số lần sửa (FishingTuning). Mua lại thì như mới.
    public sealed class RodState
    {
        public string Id;
        public int Durability;
        public bool Snapped;
        public int RepairCount;

        public bool IsBroken => Snapped || Durability <= 0;
    }

    // Cá trong giỏ.
    public sealed class CaughtFish
    {
        public string FishId;
        public int Grams;
        public long CaughtAt;
        public DaySlot Slot;
    }

    // Phần câu cá của tiến trình người chơi (lưu trong FarmState).
    public sealed class FishingState
    {
        public List<RodState> Rods = new List<RodState>();
        public string EquippedRodId;
        public Dictionary<string, int> Baits = new Dictionary<string, int>();
        public List<CaughtFish> Basket = new List<CaughtFish>();
        // Mức nâng cấp giỏ (chỉ số vào FishingDef.BasketCapacities).
        public int BasketLevel;
        // Kỷ lục cân nặng (gam) theo loài.
        public Dictionary<string, int> RecordGrams = new Dictionary<string, int>();
        // Đã tặng cần + mồi lúc mở câu cá.
        public bool StarterGranted;

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
