using System;
using UnityEngine;

namespace VeQue.View.CameraControl
{
    // Nới (số dương) hoặc thu (số âm) một vùng theo từng cạnh, tính bằng ô.
    [Serializable]
    public struct EdgePadding
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public Rect Apply(Rect rect)
        {
            rect.xMin -= Left;
            rect.xMax += Right;
            rect.yMin -= Bottom;
            rect.yMax += Top;
            return rect;
        }
    }
}
