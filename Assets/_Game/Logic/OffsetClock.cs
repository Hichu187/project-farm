using System;

namespace VeQue.Logic
{
    // Giờ thật + khoảng lệch, để tua nhanh thời gian khi test. Chỉ tua tới: lùi giờ sẽ làm MonotonicClock đứng yên.
    public sealed class OffsetClock : IClock
    {
        private readonly IClock _inner;

        public OffsetClock(IClock inner, long offsetSeconds = 0)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            if (offsetSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(offsetSeconds), "Khoảng lệch không được âm.");
            OffsetSeconds = offsetSeconds;
        }

        public long OffsetSeconds { get; private set; }

        public long UtcNowSeconds => _inner.UtcNowSeconds + OffsetSeconds;

        public void Advance(long seconds)
        {
            if (seconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(seconds), "Chỉ tua tới.");
            OffsetSeconds += seconds;
        }
    }
}
