using System;
using UnityEngine;

namespace VeQue.View.Character
{
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
        Hoe = 2,
        Water = 3,
    }

    // Bộ khung hình của nhân vật (bản đơn giản, 1 lớp): mỗi hoạt ảnh có khung cho 4 hướng.
    // Gán bằng menu Về Quê → Gán sprite nhân vật.
    [CreateAssetMenu(menuName = "Về Quê/Character Skin", fileName = "CharacterSkin")]
    public sealed class CharacterSkin : ScriptableObject
    {
        [Serializable]
        public sealed class Clip
        {
            [Min(0.1f)] public float Fps = 6f;
            // Hoạt ảnh lặp (đứng, đi) hay chạy 1 lần (cuốc, tưới).
            public bool Loop = true;
            public Sprite[] Down = Array.Empty<Sprite>();
            public Sprite[] Up = Array.Empty<Sprite>();
            public Sprite[] Left = Array.Empty<Sprite>();
            public Sprite[] Right = Array.Empty<Sprite>();

            public Sprite[] Get(Facing facing)
            {
                switch (facing)
                {
                    case Facing.Up: return Up;
                    case Facing.Left: return Left;
                    case Facing.Right: return Right;
                    default: return Down;
                }
            }
        }

        [SerializeField] private Clip _idle = new Clip { Fps = 2f };
        [SerializeField] private Clip _walk = new Clip { Fps = 8f };
        [SerializeField] private Clip _hoe = new Clip { Fps = 6f, Loop = false };
        [SerializeField] private Clip _water = new Clip { Fps = 6f, Loop = false };

        public Clip Get(CharacterAnim anim)
        {
            switch (anim)
            {
                case CharacterAnim.Walk: return _walk;
                case CharacterAnim.Hoe: return _hoe;
                case CharacterAnim.Water: return _water;
                default: return _idle;
            }
        }

        // Thời gian chạy hết 1 lượt hoạt ảnh theo hướng này (giây).
        public float GetDuration(CharacterAnim anim, Facing facing)
        {
            Clip clip = Get(anim);
            return clip.Get(facing).Length / clip.Fps;
        }
    }
}
