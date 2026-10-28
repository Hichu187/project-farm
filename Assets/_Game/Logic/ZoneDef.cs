using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Định nghĩa một khu trồng.
    // StartOpenCount: số ô mở miễn phí khi khu được mở khóa (Khu 1: lúc bắt đầu game).
    // UnlockPrices[i]: giá của ô thứ i được mở tiếp sau các ô miễn phí.
    public sealed class ZoneDef
    {
        public string Id { get; }
        public int Width { get; }
        public int Height { get; }
        public int StartOpenCount { get; }
        public IReadOnlyList<int> UnlockPrices { get; }

        public int PlotCount => Width * Height;

        public ZoneDef(string id, int width, int height, int startOpenCount, IReadOnlyList<int> unlockPrices)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Zone id rỗng.", nameof(id));
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (startOpenCount < 0 || startOpenCount > width * height)
                throw new ArgumentOutOfRangeException(nameof(startOpenCount));
            if (unlockPrices == null)
                throw new ArgumentNullException(nameof(unlockPrices));

            int expected = width * height - startOpenCount;
            if (unlockPrices.Count != expected)
                throw new ArgumentException($"Khu '{id}' cần {expected} giá mở ô, nhận {unlockPrices.Count}.", nameof(unlockPrices));

            var prices = new int[expected];
            for (int i = 0; i < expected; i++)
            {
                if (unlockPrices[i] < 0)
                    throw new ArgumentOutOfRangeException(nameof(unlockPrices), $"Giá mở ô thứ {i} của khu '{id}' âm.");
                prices[i] = unlockPrices[i];
            }

            Id = id;
            Width = width;
            Height = height;
            StartOpenCount = startOpenCount;
            UnlockPrices = Array.AsReadOnly(prices);
        }
    }
}
