using UnityEngine;
using VeQue.View.Map;

namespace VeQue.View.Character
{
    // Vẽ nhân vật (bản đơn giản: 1 SpriteRenderer) theo CharacterSkin: chọn khung theo hoạt ảnh + hướng,
    // thứ tự vẽ theo trục Y dùng chung với cây. Vị trí do CharacterMotor đặt (đã làm tròn theo pixel).
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CharacterView : MonoBehaviour
    {
        [SerializeField] private CharacterSkin _skin;

        private SpriteRenderer _renderer;
        private CharacterAnim _anim;
        private Facing _facing;
        private float _startTime;
        private bool _finished;

        public CharacterAnim Anim => _anim;
        public Facing Facing => _facing;
        public CharacterSkin Skin => _skin;
        // Hoạt ảnh chạy 1 lần (cuốc, tưới) đã tới khung cuối.
        public bool Finished => _finished;

        public void Play(CharacterAnim anim, Facing facing, bool restart = false)
        {
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
            UpdateFrame();
        }

        public float GetDuration(CharacterAnim anim, Facing facing)
        {
            return _skin != null ? _skin.GetDuration(anim, facing) : 0f;
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_skin == null)
                Debug.LogWarning("CharacterView: chưa gán CharacterSkin.", this);
        }

        private void OnEnable()
        {
            _startTime = Time.time;
            UpdateFrame();
        }

        private void LateUpdate()
        {
            UpdateFrame();
            _renderer.sortingOrder = SortingOrders.ByCellY(Mathf.FloorToInt(transform.position.y));
        }

        private void UpdateFrame()
        {
            if (_skin == null || _renderer == null)
                return;

            CharacterSkin.Clip clip = _skin.Get(_anim);
            Sprite[] frames = clip.Get(_facing);
            if (frames.Length == 0)
                return;

            int frame = Mathf.FloorToInt((Time.time - _startTime) * clip.Fps);
            if (clip.Loop)
            {
                frame %= frames.Length;
            }
            else if (frame >= frames.Length)
            {
                frame = frames.Length - 1;
                _finished = true;
            }
            _renderer.sprite = frames[frame];
        }
    }
}
