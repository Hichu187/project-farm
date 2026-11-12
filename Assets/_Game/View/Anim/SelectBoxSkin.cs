using UnityEngine;

namespace VeQue.View.Anim
{
    // Bộ 4 góc khung chọn (Sunnyside UI/selectbox_tl/tr/bl/br) và thông số hoạt ảnh dùng chung.
    // Tạo / gán bằng menu Về Quê → Khung chọn: tạo asset + gán vào scene.
    [CreateAssetMenu(menuName = "Về Quê/Select Box Skin", fileName = "SelectBoxSkin")]
    public sealed class SelectBoxSkin : ScriptableObject
    {
        public const int TopLeft = 0;
        public const int TopRight = 1;
        public const int BottomLeft = 2;
        public const int BottomRight = 3;

        [SerializeField] private Sprite _topLeft;
        [SerializeField] private Sprite _topRight;
        [SerializeField] private Sprite _bottomLeft;
        [SerializeField] private Sprite _bottomRight;

        [Tooltip("Mép ngoài của góc nằm ngoài vùng chọn bao nhiêu pixel.")]
        [SerializeField] private int _outsetPixels = 2;

        [Header("Thở (điểm đến)")]
        [SerializeField] private float _breathePeriod = 0.8f;
        [SerializeField] private int _breathePixels = 1;

        [Header("Co giãn (ô đang làm)")]
        [Tooltip("Thời gian một nhịp giãn ra rồi co về (giây).")]
        [SerializeField] private float _scalePeriod = 0.6f;
        [Tooltip("Góc giãn ra tối đa bao nhiêu pixel (đi từng bước 1 pixel).")]
        [SerializeField] private int _scalePixels = 2;

        [Header("Thu vào (chọn khu)")]
        [SerializeField] private float _snapDuration = 0.15f;
        [Tooltip("Lúc bắt đầu, góc nằm ngoài vị trí cuối bao nhiêu pixel.")]
        [SerializeField] private int _snapPixels = 4;

        [Header("Mờ dần")]
        [SerializeField] private float _fadeDuration = 0.3f;

        public int OutsetPixels => _outsetPixels;
        public float ScalePeriod => _scalePeriod;
        public int ScalePixels => _scalePixels;
        public float BreathePeriod => _breathePeriod;
        public int BreathePixels => _breathePixels;
        public float SnapDuration => _snapDuration;
        public int SnapPixels => _snapPixels;
        public float FadeDuration => _fadeDuration;

        public bool IsValid => _topLeft != null && _topRight != null && _bottomLeft != null && _bottomRight != null;

        public Sprite GetCorner(int corner)
        {
            switch (corner)
            {
                case TopLeft: return _topLeft;
                case TopRight: return _topRight;
                case BottomLeft: return _bottomLeft;
                default: return _bottomRight;
            }
        }
    }
}
