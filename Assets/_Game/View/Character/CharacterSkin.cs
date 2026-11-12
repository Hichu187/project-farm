using System;
using UnityEngine;

namespace VeQue.View.Character
{
    // Hướng di chuyển / nhìn (4 hướng, dùng cho tìm đường và chọn chỗ đứng).
    // Art Sunnyside chỉ có nhìn ngang: CharacterView lật hình khi quay trái, lên / xuống giữ hướng ngang gần nhất.
    public enum Facing
    {
        Down = 0,
        Up = 1,
        Left = 2,
        Right = 3,
    }

    public enum CharacterAnim
    {
        Idle = 0,
        Walk = 1,
        // Gieo, thu hoạch (Sunnyside DOING).
        Work = 2,
        // Tưới (Sunnyside WATERING).
        Water = 3,
        // Câu cá (Sunnyside CASTING / WAITING / REELING / CAUGHT).
        Cast = 4,
        Wait = 5,
        Reel = 6,
        Caught = 7,
    }

    // Bộ khung hình nhân vật Sunnyside, vẽ nhìn sang phải, 3 lớp đồng bộ khung: thân + dụng cụ (theo hoạt ảnh),
    // tóc (theo kiểu tóc người chơi chọn). Màu tóc phủ bằng shader VeQue/HairTint.
    // Gán bằng menu Về Quê → Nhân vật Sunnyside: import + gán sprite.
    [CreateAssetMenu(menuName = "Về Quê/Character Skin", fileName = "CharacterSkin")]
    public sealed class CharacterSkin : ScriptableObject
    {
        public const int LayerCount = 3;
        public const int BodyLayer = 0;
        public const int HairLayer = 1;
        public const int ToolsLayer = 2;

        [Serializable]
        public sealed class Clip
        {
            [Min(0.1f)] public float Fps = 10f;
            // Hoạt ảnh lặp (đứng, đi) hay chạy 1 lần (gieo, tưới).
            public bool Loop = true;
            public Sprite[] Body = Array.Empty<Sprite>();
            public Sprite[] Tools = Array.Empty<Sprite>();

            public int FrameCount => Body.Length;
        }

        // Một kiểu tóc: khung cho từng hoạt ảnh (cùng số khung với thân) + khoảng độ sáng để phủ màu.
        [Serializable]
        public sealed class HairSet
        {
            public string Id = "shorthair";
            public string DisplayName = "Tóc ngắn";
            public Sprite[] Idle = Array.Empty<Sprite>();
            public Sprite[] Walk = Array.Empty<Sprite>();
            public Sprite[] Work = Array.Empty<Sprite>();
            public Sprite[] Water = Array.Empty<Sprite>();
            public Sprite[] Cast = Array.Empty<Sprite>();
            public Sprite[] Wait = Array.Empty<Sprite>();
            public Sprite[] Reel = Array.Empty<Sprite>();
            public Sprite[] Caught = Array.Empty<Sprite>();
            [Range(0f, 1f)] public float MinLum = 0.15f;
            [Range(0f, 1f)] public float MaxLum = 0.5f;

            public Sprite[] Get(CharacterAnim anim)
            {
                switch (anim)
                {
                    case CharacterAnim.Walk: return Walk;
                    case CharacterAnim.Work: return Work;
                    case CharacterAnim.Water: return Water;
                    case CharacterAnim.Cast: return Cast;
                    case CharacterAnim.Wait: return Wait;
                    case CharacterAnim.Reel: return Reel;
                    case CharacterAnim.Caught: return Caught;
                    default: return Idle;
                }
            }
        }

        [Serializable]
        public sealed class HairColorOption
        {
            public string Id = "goc";
            public string DisplayName = "Màu gốc";
            // true = giữ màu tóc nguyên bản (không phủ).
            public bool Original;
            public Color Color = Color.white;
        }

        [SerializeField] private Clip _idle = new Clip { Fps = 8f };
        [SerializeField] private Clip _walk = new Clip { Fps = 10f };
        // 8 khung × 16 fps = 0,5 giây mỗi việc.
        [SerializeField] private Clip _work = new Clip { Fps = 16f, Loop = false };
        // 5 khung × 10 fps = 0,5 giây.
        [SerializeField] private Clip _water = new Clip { Fps = 10f, Loop = false };
        // Câu cá: ném 15 khung × 15 fps = 1 giây (chạy 1 lần); chờ phao lặp; kéo cá lặp; được cá 10 khung = 1 giây.
        [SerializeField] private Clip _cast = new Clip { Fps = 15f, Loop = false };
        [SerializeField] private Clip _wait = new Clip { Fps = 6f };
        [SerializeField] private Clip _reel = new Clip { Fps = 12f };
        [SerializeField] private Clip _caught = new Clip { Fps = 10f, Loop = false };

        [Header("Tóc")]
        [SerializeField] private HairSet[] _hairs = Array.Empty<HairSet>();
        [Tooltip("Material dùng shader VeQue/HairTint.")]
        [SerializeField] private Material _hairMaterial;
        [SerializeField] private HairColorOption[] _hairColors =
        {
            new HairColorOption { Id = "goc", DisplayName = "Màu gốc", Original = true },
            new HairColorOption { Id = "den", DisplayName = "Đen", Color = new Color32(0x3a, 0x34, 0x40, 0xff) },
            new HairColorOption { Id = "nau", DisplayName = "Nâu", Color = new Color32(0x8a, 0x55, 0x36, 0xff) },
            new HairColorOption { Id = "vang", DisplayName = "Vàng", Color = new Color32(0xf0, 0xc4, 0x50, 0xff) },
            new HairColorOption { Id = "bac", DisplayName = "Bạc", Color = new Color32(0xd4, 0xd8, 0xe4, 0xff) },
            new HairColorOption { Id = "do", DisplayName = "Đỏ", Color = new Color32(0xd8, 0x44, 0x40, 0xff) },
            new HairColorOption { Id = "xanh", DisplayName = "Xanh", Color = new Color32(0x44, 0x80, 0xe0, 0xff) },
            new HairColorOption { Id = "hong", DisplayName = "Hồng", Color = new Color32(0xf0, 0x88, 0xb8, 0xff) },
        };

        public int HairCount => _hairs.Length;
        public int HairColorCount => _hairColors.Length;
        public Material HairMaterial => _hairMaterial;

        public Clip Get(CharacterAnim anim)
        {
            switch (anim)
            {
                case CharacterAnim.Walk: return _walk;
                case CharacterAnim.Work: return _work;
                case CharacterAnim.Water: return _water;
                case CharacterAnim.Cast: return _cast;
                case CharacterAnim.Wait: return _wait;
                case CharacterAnim.Reel: return _reel;
                case CharacterAnim.Caught: return _caught;
                default: return _idle;
            }
        }

        public HairSet GetHair(int index) => _hairs.Length > 0 ? _hairs[Mathf.Clamp(index, 0, _hairs.Length - 1)] : null;

        public HairColorOption GetHairColor(int index) =>
            _hairColors.Length > 0 ? _hairColors[Mathf.Clamp(index, 0, _hairColors.Length - 1)] : null;

        // Không thấy id (data đổi) thì về kiểu đầu tiên.
        public int FindHairIndex(string id)
        {
            for (int i = 0; i < _hairs.Length; i++)
            {
                if (_hairs[i].Id == id)
                    return i;
            }
            return 0;
        }

        public int FindHairColorIndex(string id)
        {
            for (int i = 0; i < _hairColors.Length; i++)
            {
                if (_hairColors[i].Id == id)
                    return i;
            }
            return 0;
        }

        // Thời gian chạy hết 1 lượt hoạt ảnh (giây).
        public float GetDuration(CharacterAnim anim)
        {
            Clip clip = Get(anim);
            return clip.FrameCount / clip.Fps;
        }

        // Thông số shader VeQue/HairTint cho kiểu tóc + màu đang chọn.
        public static void ApplyTint(MaterialPropertyBlock block, HairSet hair, HairColorOption color)
        {
            block.SetColor(HairTintIds.Color, color != null ? color.Color : Color.white);
            block.SetFloat(HairTintIds.Amount, color == null || color.Original ? 0f : 1f);
            block.SetFloat(HairTintIds.MinLum, hair != null ? hair.MinLum : 0f);
            block.SetFloat(HairTintIds.MaxLum, hair != null ? hair.MaxLum : 1f);
        }

        public static void ApplyTint(Material material, HairSet hair, HairColorOption color)
        {
            material.SetColor(HairTintIds.Color, color != null ? color.Color : Color.white);
            material.SetFloat(HairTintIds.Amount, color == null || color.Original ? 0f : 1f);
            material.SetFloat(HairTintIds.MinLum, hair != null ? hair.MinLum : 0f);
            material.SetFloat(HairTintIds.MaxLum, hair != null ? hair.MaxLum : 1f);
        }

        private static class HairTintIds
        {
            public static readonly int Color = Shader.PropertyToID("_HairColor");
            public static readonly int Amount = Shader.PropertyToID("_Amount");
            public static readonly int MinLum = Shader.PropertyToID("_MinLum");
            public static readonly int MaxLum = Shader.PropertyToID("_MaxLum");
        }
    }
}
