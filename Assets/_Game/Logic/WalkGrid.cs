using System;

namespace VeQue.Logic
{
    // Lưới ô đi được cho nhân vật. Không biết Tilemap: View đổ dữ liệu vào (hàng rào, nước, nhà, luống chặn).
    // Ô ngoài lưới coi như bị chặn.
    public sealed class WalkGrid
    {
        private readonly bool[] _walkable;

        public int MinX { get; }
        public int MinY { get; }
        public int Width { get; }
        public int Height { get; }
        public int CellCount => _walkable.Length;

        public WalkGrid(int minX, int minY, int width, int height, bool walkable = false)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            MinX = minX;
            MinY = minY;
            Width = width;
            Height = height;
            _walkable = new bool[width * height];
            if (walkable)
                Array.Fill(_walkable, true);
        }

        public bool Contains(GridCell cell)
        {
            return cell.X >= MinX && cell.X < MinX + Width && cell.Y >= MinY && cell.Y < MinY + Height;
        }

        public bool IsWalkable(GridCell cell) => Contains(cell) && _walkable[IndexOf(cell)];

        public void SetWalkable(GridCell cell, bool walkable)
        {
            if (!Contains(cell))
                throw new ArgumentOutOfRangeException(nameof(cell), $"Ô {cell} nằm ngoài lưới.");
            _walkable[IndexOf(cell)] = walkable;
        }

        public void SetBlocked(GridCell cell) => SetWalkable(cell, false);

        // Đặt cả hình chữ nhật (x, y là góc dưới-trái); phần nằm ngoài lưới bỏ qua.
        public void SetRect(int x, int y, int width, int height, bool walkable)
        {
            int fromX = Math.Max(x, MinX);
            int fromY = Math.Max(y, MinY);
            int toX = Math.Min(x + width, MinX + Width);
            int toY = Math.Min(y + height, MinY + Height);
            for (int cy = fromY; cy < toY; cy++)
            {
                for (int cx = fromX; cx < toX; cx++)
                    _walkable[(cy - MinY) * Width + (cx - MinX)] = walkable;
            }
        }

        internal int IndexOf(GridCell cell) => (cell.Y - MinY) * Width + (cell.X - MinX);

        internal GridCell CellAt(int index) => new GridCell(MinX + index % Width, MinY + index / Width);

        internal bool IsWalkableIndex(int index) => _walkable[index];
    }
}
