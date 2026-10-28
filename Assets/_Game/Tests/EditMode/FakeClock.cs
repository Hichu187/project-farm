using VeQue.Logic;

namespace VeQue.Tests
{
    public sealed class FakeClock : IClock
    {
        public long UtcNowSeconds { get; set; } = 1_000_000;

        public void Advance(long seconds) => UtcNowSeconds += seconds;
    }
}
