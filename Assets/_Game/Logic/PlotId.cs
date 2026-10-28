using System;

namespace VeQue.Logic
{
    // Định danh một ô: (zoneId, col, row). col tính từ trái, row tính từ trên xuống (row 0 = hàng trên cùng).
    public readonly struct PlotId : IEquatable<PlotId>
    {
        public string ZoneId { get; }
        public int Col { get; }
        public int Row { get; }

        public PlotId(string zoneId, int col, int row)
        {
            ZoneId = zoneId;
            Col = col;
            Row = row;
        }

        public bool Equals(PlotId other) => ZoneId == other.ZoneId && Col == other.Col && Row == other.Row;
        public override bool Equals(object obj) => obj is PlotId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ZoneId, Col, Row);
        public override string ToString() => $"({ZoneId}, {Col}, {Row})";

        public static bool operator ==(PlotId a, PlotId b) => a.Equals(b);
        public static bool operator !=(PlotId a, PlotId b) => !a.Equals(b);
    }
}
