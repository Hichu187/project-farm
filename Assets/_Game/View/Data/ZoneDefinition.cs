using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Số giá mở ô phải bằng PlotCount - StartOpenCount (hết luống này mới mở sang luống sau).
    [CreateAssetMenu(menuName = "Về Quê/Zone Definition", fileName = "Zone")]
    public sealed class ZoneDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [Tooltip("Theo thứ tự mở ô.")]
        [SerializeField] private BedDefinition[] _beds = Array.Empty<BedDefinition>();
        [Tooltip("Số ô mở miễn phí khi khu được mở khóa.")]
        [Min(0)] [SerializeField] private int _startOpenCount;
        [SerializeField] private int[] _unlockPrices = Array.Empty<int>();

        public string Id => _id;
        public IReadOnlyList<BedDefinition> Beds => _beds;
        public int StartOpenCount => _startOpenCount;
        public int ExpectedPriceCount => Mathf.Max(0, PlotCount - _startOpenCount);

        public int PlotCount
        {
            get
            {
                int count = 0;
                foreach (BedDefinition bed in _beds)
                    count += bed.Width * bed.Height;
                return count;
            }
        }

        // Vùng bao các luống, tính bằng cell.
        public RectInt BoundingBox
        {
            get
            {
                if (_beds.Length == 0)
                    return default;

                int xMin = int.MaxValue, yMin = int.MaxValue, xMax = int.MinValue, yMax = int.MinValue;
                foreach (BedDefinition bed in _beds)
                {
                    xMin = Mathf.Min(xMin, bed.Origin.x);
                    yMin = Mathf.Min(yMin, bed.Origin.y);
                    xMax = Mathf.Max(xMax, bed.Origin.x + bed.Width);
                    yMax = Mathf.Max(yMax, bed.Origin.y + bed.Height);
                }
                return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
            }
        }

        public ZoneDef ToDef()
        {
            var beds = new BedDef[_beds.Length];
            for (int i = 0; i < _beds.Length; i++)
                beds[i] = _beds[i].ToDef();
            return new ZoneDef(_id, beds, _startOpenCount, _unlockPrices);
        }

        private void OnValidate()
        {
            int count = _unlockPrices == null ? 0 : _unlockPrices.Length;
            if (count != ExpectedPriceCount)
                Debug.LogWarning($"Zone '{name}': cần {ExpectedPriceCount} giá mở ô, đang có {count}.", this);
        }
    }
}
