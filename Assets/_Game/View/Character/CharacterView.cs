using UnityEngine;
using UnityEngine.Rendering;
using VeQue.View.Map;

namespace VeQue.View.Character
{
    // Vẽ nhân vật Sunnyside: 3 SpriteRenderer đồng bộ khung (thân trên GameObject này, tóc và dụng cụ là con),
    // gom bằng SortingGroup để cả nhân vật sắp theo trục Y cùng cây. Art nhìn sang phải: quay trái thì lật;
    // đi lên / xuống giữ hướng ngang gần nhất. Tóc theo kiểu người chơi chọn, màu phủ bằng shader VeQue/HairTint.
    // Vị trí do CharacterMotor đặt (đã làm tròn theo pixel).
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CharacterView : MonoBehaviour
    {
        [SerializeField] private CharacterSkin _skin;
        [Tooltip("Lớp tóc; để trống thì tự tạo con \"Hair\".")]
        [SerializeField] private SpriteRenderer _hair;
        [Tooltip("Lớp dụng cụ; để trống thì tự tạo con \"Tools\".")]
        [SerializeField] private SpriteRenderer _tools;

        private readonly SpriteRenderer[] _layers = new SpriteRenderer[CharacterSkin.LayerCount];
        private MaterialPropertyBlock _hairBlock;
        private SortingGroup _group;
        private CharacterAnim _anim;
        private Facing _facing;
        private bool _flipped;
        private float _startTime;
        private bool _finished;
        private int _shownFrame = -1;
        private int _hairIndex;
        private int _hairColorIndex;

        public CharacterAnim Anim => _anim;
        public Facing Facing => _facing;
        public CharacterSkin Skin => _skin;
        // Hoạt ảnh chạy 1 lần (gieo, tưới) đã tới khung cuối.
        public bool Finished => _finished;

        public void Play(CharacterAnim anim, Facing facing, bool restart = false)
        {
            // Lên / xuống: giữ hướng ngang đang có.
            if (facing == Facing.Left)
                _flipped = true;
            else if (facing == Facing.Right)
                _flipped = false;

            if (!restart && anim == _anim && facing == _facing)
                return;

            // Đổi hướng giữa lúc đang đi thì giữ nhịp bước chân.
            bool keepPhase = !restart && anim == _anim;
            _anim = anim;
            _facing = facing;
            if (!keepPhase)
            {
                _startTime = Time.time;
                _finished = false;
            }
            _shownFrame = -1;
            UpdateFrame();
        }

        // Kiểu tóc + màu tóc theo id (CharacterLook). Id không còn thì về mục đầu tiên.
        public void SetLook(string hairStyle, string hairColor)
        {
            if (_skin == null)
                return;

            int hair = _skin.FindHairIndex(hairStyle);
            int color = _skin.FindHairColorIndex(hairColor);
            if (hair == _hairIndex && color == _hairColorIndex && _hairBlock != null)
                return;

            _hairIndex = hair;
            _hairColorIndex = color;
            ApplyHairTint();
            _shownFrame = -1;
            UpdateFrame();
        }

        public float GetDuration(CharacterAnim anim) => _skin != null ? _skin.GetDuration(anim) : 0f;

        private void Awake()
        {
            _layers[CharacterSkin.BodyLayer] = GetComponent<SpriteRenderer>();
            _layers[CharacterSkin.HairLayer] = _hair != null ? _hair : CreateLayer("Hair");
            _layers[CharacterSkin.ToolsLayer] = _tools != null ? _tools : CreateLayer("Tools");
            for (int i = 0; i < _layers.Length; i++)
                _layers[i].sortingOrder = i;

            _group = GetComponent<SortingGroup>();
            if (_group == null)
                _group = gameObject.AddComponent<SortingGroup>();

            if (_skin == null)
            {
                Debug.LogWarning("CharacterView: chưa gán CharacterSkin.", this);
                return;
            }
            if (_skin.HairMaterial != null)
                _layers[CharacterSkin.HairLayer].sharedMaterial = _skin.HairMaterial;
            ApplyHairTint();
        }

        private void OnEnable()
        {
            _startTime = Time.time;
            _shownFrame = -1;
            UpdateFrame();
        }

        private void LateUpdate()
        {
            UpdateFrame();
            _group.sortingOrder = SortingOrders.ByCellY(Mathf.FloorToInt(transform.position.y));
        }

        private SpriteRenderer CreateLayer(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.AddComponent<SpriteRenderer>();
        }

        private void ApplyHairTint()
        {
            SpriteRenderer hair = _layers[CharacterSkin.HairLayer];
            if (_skin == null || hair == null)
                return;

            if (_hairBlock == null)
                _hairBlock = new MaterialPropertyBlock();
            hair.GetPropertyBlock(_hairBlock);
            CharacterSkin.ApplyTint(_hairBlock, _skin.GetHair(_hairIndex), _skin.GetHairColor(_hairColorIndex));
            hair.SetPropertyBlock(_hairBlock);
        }

        private void UpdateFrame()
        {
            if (_skin == null || _layers[0] == null)
                return;

            CharacterSkin.Clip clip = _skin.Get(_anim);
            int count = clip.FrameCount;
            if (count == 0)
                return;

            int frame = Mathf.FloorToInt((Time.time - _startTime) * clip.Fps);
            if (clip.Loop)
            {
                frame %= count;
            }
            else if (frame >= count)
            {
                frame = count - 1;
                _finished = true;
            }

            for (int i = 0; i < _layers.Length; i++)
            {
                SpriteRenderer layer = _layers[i];
                if (layer.flipX != _flipped)
                    layer.flipX = _flipped;
            }

            if (frame == _shownFrame)
                return;
            _shownFrame = frame;

            CharacterSkin.HairSet hairSet = _skin.GetHair(_hairIndex);
            Sprite[] hair = hairSet != null ? hairSet.Get(_anim) : null;
            _layers[CharacterSkin.BodyLayer].sprite = Frame(clip.Body, frame);
            _layers[CharacterSkin.HairLayer].sprite = Frame(hair, frame);
            _layers[CharacterSkin.ToolsLayer].sprite = Frame(clip.Tools, frame);
        }

        private static Sprite Frame(Sprite[] frames, int index) =>
            frames != null && index < frames.Length ? frames[index] : null;
    }
}
