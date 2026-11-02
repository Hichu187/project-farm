using System;

namespace VeQue.Logic
{
    // Plots xếp theo chỉ số phẳng của ZoneDef (đổi sang định danh ô qua ZoneDef).
    // OpenCount ô đầu tiên (theo thứ tự đó) đã mở, còn lại đang khóa.
    [Serializable]
    public sealed class ZoneState
    {
        public string ZoneId;
        public int OpenCount;
        public PlotState[] Plots;

        public bool IsFull => OpenCount >= Plots.Length;

        public static ZoneState CreateLocked(ZoneDef def)
        {
            var plots = new PlotState[def.PlotCount];
            for (int i = 0; i < plots.Length; i++)
                plots[i] = new PlotState { Status = PlotStatus.Locked };

            return new ZoneState
            {
                ZoneId = def.Id,
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
