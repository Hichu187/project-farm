using System;

namespace VeQue.Logic
{
    // Ô được mở theo hàng, bắt đầu từ góc trên-trái: chỉ số = row * Width + col.
    // OpenCount ô đầu tiên (theo thứ tự đó) đã mở, còn lại đang khóa.
    [Serializable]
    public sealed class ZoneState
    {
        public string ZoneId;
        public int Width;
        public int Height;
        public int OpenCount;
        public PlotState[] Plots;

        public int PlotCount => Width * Height;
        public bool IsFull => OpenCount >= PlotCount;

        public bool Contains(int col, int row) => col >= 0 && col < Width && row >= 0 && row < Height;
        public int IndexOf(int col, int row) => row * Width + col;
        public PlotId IdAt(int index) => new PlotId(ZoneId, index % Width, index / Width);

        public static ZoneState CreateLocked(ZoneDef def)
        {
            var plots = new PlotState[def.PlotCount];
            for (int i = 0; i < plots.Length; i++)
                plots[i] = new PlotState { Status = PlotStatus.Locked };

            return new ZoneState
            {
                ZoneId = def.Id,
                Width = def.Width,
                Height = def.Height,
                OpenCount = 0,
                Plots = plots,
            };
        }

        internal void OpenNext()
        {
            Plots[OpenCount].Clear();
            OpenCount++;
        }
    }
}
