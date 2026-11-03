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

        // Logic tính row từ trên xuống, còn trục Y của cell hướng lên.
        public Vector3Int GetCell(PlotId id)
        {
            BedDefinition bed = _beds[id.BedIndex];
            return bed.Origin + new Vector3Int(id.Col, bed.Height - 1 - id.Row, 0);
        }

        // False nếu cell không thuộc luống nào, gồm cả khe giữa các luống.
        public bool TryGetPlotId(Vector3Int cell, out PlotId id)
        {
            for (int i = 0; i < _beds.Length; i++)
            {
                BedDefinition bed = _beds[i];
                int col = cell.x - bed.Origin.x;
                int fromBottom = cell.y - bed.Origin.y;
                if (col >= 0 && col < bed.Width && fromBottom >= 0 && fromBottom < bed.Height)
                {
                    id = new PlotId(_id, i, col, bed.Height - 1 - fromBottom);
                    return true;
                }
            }

            id = default;
            return false;
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
