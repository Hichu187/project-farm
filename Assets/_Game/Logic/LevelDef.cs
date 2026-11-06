using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Bảng cấp độ. XpToNext[i] = XP cần để lên từ cấp i+1 lên cấp i+2; cấp tối đa = XpToNext.Count + 1.
    // Lên cấp N: thưởng CoinsPerLevel × N xu và UnlockSeedCount hạt cho mỗi cây mở ở cấp N.
    public sealed class LevelDef
    {
        // Không có cấp độ: luôn ở cấp 1.
        public static readonly LevelDef None = new LevelDef(Array.Empty<int>(), 0, 0);

        public IReadOnlyList<int> XpToNext { get; }
        public int MaxLevel { get; }
        public int CoinsPerLevel { get; }
        public int UnlockSeedCount { get; }

        public LevelDef(IReadOnlyList<int> xpToNext, int coinsPerLevel, int unlockSeedCount)
        {
            if (xpToNext == null)
                throw new ArgumentNullException(nameof(xpToNext));
            if (coinsPerLevel < 0)
                throw new ArgumentOutOfRangeException(nameof(coinsPerLevel));
            if (unlockSeedCount < 0)
                throw new ArgumentOutOfRangeException(nameof(unlockSeedCount));

            var copy = new int[xpToNext.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                if (xpToNext[i] <= 0)
                    throw new ArgumentOutOfRangeException(nameof(xpToNext), $"XP lên cấp {i + 2} phải > 0.");
                copy[i] = xpToNext[i];
            }

            XpToNext = Array.AsReadOnly(copy);
            MaxLevel = copy.Length + 1;
            CoinsPerLevel = coinsPerLevel;
            UnlockSeedCount = unlockSeedCount;
        }

        // XP cần để lên cấp kế tiếp; 0 nếu đã ở cấp tối đa.
        public int GetXpToNext(int level)
        {
            if (level < 1 || level >= MaxLevel)
                return 0;
            return XpToNext[level - 1];
        }
    }
}
