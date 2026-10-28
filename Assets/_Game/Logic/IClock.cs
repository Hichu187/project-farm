using System;

namespace VeQue.Logic
{
    public interface IClock
    {
        long UtcNowSeconds { get; }
    }

    // TODO(Phần 7): chống chỉnh giờ máy.
    public sealed class SystemClock : IClock
    {
        public long UtcNowSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
