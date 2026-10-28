using System;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Số giá mở ô phải bằng Width * Height - StartOpenCount (ô mở theo hàng, từ góc trên-trái).
    [CreateAssetMenu(menuName = "Về Quê/Zone Definition", fileName = "Zone")]
    public sealed class ZoneDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [Min(1)] [SerializeField] private int _width = 4;
        [Min(1)] [SerializeField] private int _height = 5;
        [Tooltip("Số ô mở miễn phí khi khu được mở khóa.")]
        [Min(0)] [SerializeField] private int _startOpenCount;
        [SerializeField] private int[] _unlockPrices = Array.Empty<int>();

        public string Id => _id;
        public int Width => _width;
        public int Height => _height;
        public int StartOpenCount => _startOpenCount;
        public int ExpectedPriceCount => Mathf.Max(0, _width * _height - _startOpenCount);

        public ZoneDef ToDef() => new ZoneDef(_id, _width, _height, _startOpenCount, _unlockPrices);

        private void OnValidate()
        {
            int count = _unlockPrices == null ? 0 : _unlockPrices.Length;
            if (count != ExpectedPriceCount)
                Debug.LogWarning($"Zone '{name}': cần {ExpectedPriceCount} giá mở ô, đang có {count}.", this);
        }
    }
}
