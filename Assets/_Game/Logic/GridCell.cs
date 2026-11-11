using System;

namespace VeQue.Logic
{
    // Một ô trên lưới đi bộ, cùng hệ tọa độ với Grid của Unity (trục Y hướng lên). Tọa độ có thể âm.
    public readonly struct GridCell : IEquatable<GridCell>
    {
        public int X { get; }
        public int Y { get; }

        public GridCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static int Manhattan(GridCell a, GridCell b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        public bool Equals(GridCell other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridCell other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(GridCell a, GridCell b) => a.Equals(b);
        public static bool operator !=(GridCell a, GridCell b) => !a.Equals(b);
    }
}
