using System;

namespace VeQue.Logic
{
    // Một luống trong khu. Chỉ biết kích thước; tọa độ trên map do View giữ.
    public sealed class BedDef
    {
        public int Width { get; }
        public int Height { get; }

        public int PlotCount => Width * Height;

        public BedDef(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
        }
    }
}
