using System;

namespace VeQue.Logic
{
    public enum FarmTaskKind
    {
        Plant = 0,
        Water = 1,
        Harvest = 2,
    }

    // Một việc nhân vật sẽ đi tới làm ở ô đất. CropId chỉ dùng cho Plant.
    public readonly struct FarmTask : IEquatable<FarmTask>
    {
        public PlotId Plot { get; }
        public FarmTaskKind Kind { get; }
        public string CropId { get; }

        public FarmTask(PlotId plot, FarmTaskKind kind, string cropId = null)
        {
            Plot = plot;
            Kind = kind;
            CropId = kind == FarmTaskKind.Plant ? cropId : null;
        }

        public static FarmTask Plant(PlotId plot, string cropId) => new FarmTask(plot, FarmTaskKind.Plant, cropId);
        public static FarmTask Water(PlotId plot) => new FarmTask(plot, FarmTaskKind.Water);
        public static FarmTask Harvest(PlotId plot) => new FarmTask(plot, FarmTaskKind.Harvest);

        public bool Equals(FarmTask other) => Plot == other.Plot && Kind == other.Kind && CropId == other.CropId;
        public override bool Equals(object obj) => obj is FarmTask other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Plot, (int)Kind, CropId);
        public override string ToString() => $"{Kind} {Plot}{(CropId != null ? " " + CropId : "")}";
    }
}
