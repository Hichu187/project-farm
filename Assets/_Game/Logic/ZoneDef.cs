using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Định nghĩa một khu trồng gồm một hay nhiều luống.
    // Thứ tự ô (chỉ số phẳng): hết luống 0 mới sang luống 1; trong mỗi luống theo hàng, từ góc trên-trái.
    // StartOpenCount: số ô mở miễn phí khi khu được mở khóa (Khu 1: lúc bắt đầu game).
    // UnlockPrices[i]: giá của ô thứ i được mở tiếp sau các ô miễn phí.
    public sealed class ZoneDef
    {
        public string Id { get; }
        public IReadOnlyList<BedDef> Beds { get; }
        public int PlotCount { get; }
        public int StartOpenCount { get; }
        public IReadOnlyList<int> UnlockPrices { get; }
        // Cấp cần để khu được mở khóa (ngoài điều kiện khu trước đã mở hết ô).
        public int RequiredLevel { get; }

        public ZoneDef(string id, IReadOnlyList<BedDef> beds, int startOpenCount, IReadOnlyList<int> unlockPrices,
            int requiredLevel = 1)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Zone id rỗng.", nameof(id));
            if (beds == null)
                throw new ArgumentNullException(nameof(beds));
            if (beds.Count == 0)
                throw new ArgumentException($"Khu '{id}' không có luống.", nameof(beds));

            var bedCopy = new BedDef[beds.Count];
            int plotCount = 0;
            for (int i = 0; i < beds.Count; i++)
            {
                bedCopy[i] = beds[i] ?? throw new ArgumentException($"Luống thứ {i} của khu '{id}' null.", nameof(beds));
                plotCount += bedCopy[i].PlotCount;
            }

            if (startOpenCount < 0 || startOpenCount > plotCount)
                throw new ArgumentOutOfRangeException(nameof(startOpenCount));
            if (unlockPrices == null)
                throw new ArgumentNullException(nameof(unlockPrices));
            if (requiredLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(requiredLevel), "Cấp yêu cầu phải ≥ 1.");

            int expected = plotCount - startOpenCount;
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
            Beds = Array.AsReadOnly(bedCopy);
            PlotCount = plotCount;
            StartOpenCount = startOpenCount;
            UnlockPrices = Array.AsReadOnly(prices);
            RequiredLevel = requiredLevel;
        }

        // Đổi (bedIndex, col, row) sang chỉ số phẳng; false nếu ngoài khu.
        public bool TryGetPlotIndex(int bedIndex, int col, int row, out int plotIndex)
        {
            plotIndex = 0;
            if (bedIndex < 0 || bedIndex >= Beds.Count)
                return false;

            BedDef bed = Beds[bedIndex];
            if (col < 0 || col >= bed.Width || row < 0 || row >= bed.Height)
                return false;

            for (int i = 0; i < bedIndex; i++)
                plotIndex += Beds[i].PlotCount;
            plotIndex += row * bed.Width + col;
            return true;
        }

        // Đổi chỉ số phẳng sang định danh ô.
        public PlotId PlotIdAt(int plotIndex)
        {
            if (plotIndex < 0 || plotIndex >= PlotCount)
                throw new ArgumentOutOfRangeException(nameof(plotIndex));

            int remaining = plotIndex;
            int bedIndex = 0;
            while (remaining >= Beds[bedIndex].PlotCount)
            {
                remaining -= Beds[bedIndex].PlotCount;
                bedIndex++;
            }

            int width = Beds[bedIndex].Width;
            return new PlotId(Id, bedIndex, remaining % width, remaining / width);
        }
    }
}
