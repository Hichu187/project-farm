namespace VeQue.Logic
{
    // Không bao giờ chạy lùi: lùi giờ máy thì thời gian đứng yên tới khi giờ thật đuổi kịp.
    // Không phát hiện được chỉnh giờ tới nếu không có server.
    public sealed class MonotonicClock : IClock
    {
        private readonly IClock _inner;
        private long _latest;

        public MonotonicClock(IClock inner, long latestSeen)
        {
            _inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
            _latest = latestSeen;
        }

        public long UtcNowSeconds
        {
            get
            {
                long now = _inner.UtcNowSeconds;
                if (now > _latest)
                    _latest = now;
                return _latest;
            }
        }
    }
}
