namespace VeQue.Logic
{
    // Xorshift32. Trạng thái nằm trong FarmState nên lưu được: tải lại trang không đổi được đơn, test lặp lại được.
    internal static class SeededRandom
    {
        // Trạng thái 0 làm xorshift đứng yên ở 0.
        public static uint Seed(long value)
        {
            uint seed = (uint)(value ^ (value >> 32)) ^ 0x9E3779B9u;
            return seed == 0 ? 1u : seed;
        }

        public static uint Next(ref uint state)
        {
            if (state == 0)
                state = 1;
            uint x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x;
        }

        // Băm 32 bit (murmur3 fmix32) để tạo random riêng từ một seed mà không dính chuỗi xorshift gốc. Không trả 0.
        public static uint Mix(uint value)
        {
            uint h = value ^ 0x9E3779B9u;
            h ^= h >> 16;
            h *= 0x85EBCA6Bu;
            h ^= h >> 13;
            h *= 0xC2B2AE35u;
            h ^= h >> 16;
            return h == 0 ? 1u : h;
        }

        // Số nguyên trong [minInclusive, maxExclusive).
        public static int Range(ref uint state, int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                return minInclusive;
            uint span = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(Next(ref state) % span);
        }
    }
}
