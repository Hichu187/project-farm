using System;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Một luống trên map. Origin = cell góc dưới-trái (trục Y của Unity hướng lên).
    [Serializable]
    public struct BedDefinition
    {
        public Vector3Int Origin;
        [Min(1)] public int Width;
        [Min(1)] public int Height;

        public BedDef ToDef() => new BedDef(Width, Height);
    }
}
