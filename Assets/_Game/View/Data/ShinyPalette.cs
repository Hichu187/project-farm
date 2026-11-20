using System;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Bảng 8 kiểu cá shiny (Đợt 2 Phần 18): tên hiển thị, material (shader VeQue/ShinyIcon), màu đầu cánh đốm lấp lánh.
    // Tạo bằng menu Về Quê → Cá shiny: tạo material. FishingDefinition tham chiếu tới để shader chắc chắn vào build.
    [CreateAssetMenu(menuName = "Về Quê/Shiny Palette", fileName = "ShinyPalette")]
    public sealed class ShinyPalette : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private ShinyKind _kind;
            [SerializeField] private string _displayName;
            [SerializeField] private Material _material;
            [SerializeField] private Color _sparkleColor = Color.white;

            public ShinyKind Kind => _kind;
            public string DisplayName => _displayName;
            public Material Material => _material;
            public Color SparkleColor => _sparkleColor;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        public int Count => _entries.Length;

        public bool TryGet(ShinyKind kind, out Entry entry)
        {
            if (kind != ShinyKind.None)
            {
                foreach (Entry e in _entries)
                {
                    if (e != null && e.Kind == kind)
                    {
                        entry = e;
                        return true;
                    }
                }
            }
            entry = null;
            return false;
        }

        // Material của kiểu; cá thường hoặc chưa có → null (dùng material mặc định).
        public Material GetMaterial(ShinyKind kind) => TryGet(kind, out Entry entry) ? entry.Material : null;
    }
}
