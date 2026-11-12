using UnityEngine;
using UnityEngine.UI;
using VeQue.View.Character;

namespace VeQue.View.UI
{
    // Xem trước nhân vật trên UI (màn tạo nhân vật): 3 Image chồng nhau (thân, tóc, dụng cụ) chạy hoạt ảnh đứng.
    // Tóc dùng bản sao riêng của material VeQue/HairTint để phủ màu. Kích thước Image do UiBuilder đặt (khung 96×64 × 4).
    public sealed class CharacterPreview : MonoBehaviour
    {
        [SerializeField] private CharacterSkin _skin;
        [SerializeField] private Image _body;
        [SerializeField] private Image _hair;
        [SerializeField] private Image _tools;

        private Material _hairMaterial;
        private int _hairIndex;
        private int _hairColorIndex;
        private int _shownFrame = -1;

        public CharacterSkin Skin => _skin;

        public void SetLook(int hairIndex, int hairColorIndex)
        {
            _hairIndex = hairIndex;
            _hairColorIndex = hairColorIndex;
            if (_skin == null)
                return;

            EnsureMaterial();
            if (_hairMaterial != null)
                CharacterSkin.ApplyTint(_hairMaterial, _skin.GetHair(_hairIndex), _skin.GetHairColor(_hairColorIndex));
            _shownFrame = -1;
            UpdateFrame();
        }

        private void OnEnable()
        {
            _shownFrame = -1;
            UpdateFrame();
        }

        private void OnDestroy()
        {
            if (_hairMaterial != null)
                Destroy(_hairMaterial);
        }

        private void Update() => UpdateFrame();

        private void EnsureMaterial()
        {
            if (_hairMaterial != null || _skin.HairMaterial == null)
                return;
            _hairMaterial = new Material(_skin.HairMaterial) { name = "HairTint (preview)" };
            _hair.material = _hairMaterial;
        }

        private void UpdateFrame()
        {
            if (_skin == null)
                return;

            CharacterSkin.Clip clip = _skin.Get(CharacterAnim.Idle);
            int count = clip.FrameCount;
            if (count == 0)
                return;

            int frame = Mathf.FloorToInt(Time.unscaledTime * clip.Fps) % count;
            if (frame == _shownFrame)
                return;
            _shownFrame = frame;

            CharacterSkin.HairSet hair = _skin.GetHair(_hairIndex);
            SetSprite(_body, clip.Body, frame);
            SetSprite(_hair, hair != null ? hair.Idle : null, frame);
            SetSprite(_tools, clip.Tools, frame);
        }

        private static void SetSprite(Image image, Sprite[] frames, int index)
        {
            Sprite sprite = frames != null && index < frames.Length ? frames[index] : null;
            image.sprite = sprite;
            image.enabled = sprite != null;
        }
    }
}
