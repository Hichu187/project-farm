using UnityEngine;

namespace VeQue.EditorTools
{
    // Bộ art UI cho menu Tạo UI (22/11/2026): người dùng kéo sprite vào từng ô, Tạo UI dựng lại GUI bằng các sprite này.
    // Ô nào để trống thì dùng art Sunnyside cũ (Assets/_Game/Art/UI, menu Import art → UI 9-slice).
    // Asset: Assets/_Game/Data/UiSkin.asset (Tạo UI tự tạo nếu chưa có). Chỉ dùng trong Editor (class nằm ở assembly Editor);
    // sprite gán vào Image trong scene nên vẫn vào build như thường.
    //
    // Setup mỗi sprite: Texture Type Sprite (2D and UI), Single, PPU 16, Point, Compression None, tắt mipmap,
    // Mesh Type Full Rect. Khung / nút: đặt Border trong Sprite Editor (9-slice). Sprite phải nằm trong 1 Sprite Atlas (padding ≥ 2).
    [CreateAssetMenu(menuName = "Về Quê/UI Skin", fileName = "UiSkin")]
    public sealed class UiSkinDefinition : ScriptableObject
    {
        [Header("Tỉ lệ")]
        [Tooltip("1 pixel art = bao nhiêu đơn vị UI cho khung / nút (mặc định 2). Viền sprite dày quá thì giảm (1 hoặc 1,5).")]
        [Min(0.5f)] [SerializeField] private float _frameScale = 2f;
        [Tooltip("1 pixel art = bao nhiêu đơn vị UI cho icon (mặc định 2).")]
        [Min(0.5f)] [SerializeField] private float _iconScale = 2f;

        [Header("Khung 9-slice (cần Border)")]
        [Tooltip("Nền bảng trượt, hộp thoại, popup (Cream).")]
        [SerializeField] private Sprite _panel;
        [Tooltip("Thẻ, dòng danh sách, ô lưới, khung chi tiết (Paper).")]
        [SerializeField] private Sprite _card;
        [Tooltip("Thanh tab dưới cùng, khung phụ (Sand).")]
        [SerializeField] private Sprite _subPanel;
        [Tooltip("Nhãn trên HUD: xu, cấp, đồng hồ, tên vùng, loa xã (LabelCream).")]
        [SerializeField] private Sprite _hudLabel;

        [Header("Nút (cần Border; ô \"nhấn\" để trống = không đổi hình khi bấm)")]
        [Tooltip("Nút xanh: Nhận, Bán, Đồng ý, Ném…")]
        [SerializeField] private Sprite _buttonGreen;
        [SerializeField] private Sprite _buttonGreenPressed;
        [Tooltip("Nút nâu: Đóng, Hủy, Quay lại, nút cột trái HUD, chữ thập kéo cá…")]
        [SerializeField] private Sprite _buttonBrown;
        [SerializeField] private Sprite _buttonBrownPressed;
        [Tooltip("Nút tab nền kem (thanh tab dưới, tab trong bảng).")]
        [SerializeField] private Sprite _buttonTab;
        [SerializeField] private Sprite _buttonTabPressed;

        [Header("Thanh tiến độ (XP, thời gian kéo cá)")]
        [Tooltip("Rãnh (cần Border). Để trống = thanh Sunnyside.")]
        [SerializeField] private Sprite _barTrack;
        [Tooltip("Phần đầy (cần Border). Để trống = màu xanh phẳng.")]
        [SerializeField] private Sprite _barFill;
        [Tooltip("Lề trong rãnh tới phần đầy, tính bằng pixel art: trái, dưới, phải, trên.")]
        [SerializeField] private Vector4 _barInset = new Vector4(2, 2, 2, 2);

        [Header("Icon (Simple, giữ tỉ lệ)")]
        [SerializeField] private Sprite _iconConfirm;
        [SerializeField] private Sprite _iconCancel;
        [SerializeField] private Sprite _iconSandtimer;
        [SerializeField] private Sprite _iconStopwatch;
        [Tooltip("Đồng xu: quà xu trong thư, lịch điểm danh.")]
        [SerializeField] private Sprite _iconCoin;
        [Tooltip("Nút Thư trên HUD (có thì hiện trái chữ).")]
        [SerializeField] private Sprite _iconMail;
        [Tooltip("Nút Điểm danh trên HUD.")]
        [SerializeField] private Sprite _iconCheckIn;
        [Tooltip("Nút Album trên HUD.")]
        [SerializeField] private Sprite _iconAlbum;

        public float FrameScale => _frameScale;
        public float IconScale => _iconScale;
        public Sprite Panel => _panel;
        public Sprite Card => _card;
        public Sprite SubPanel => _subPanel;
        public Sprite HudLabel => _hudLabel;
        public Sprite ButtonGreen => _buttonGreen;
        public Sprite ButtonGreenPressed => _buttonGreenPressed;
        public Sprite ButtonBrown => _buttonBrown;
        public Sprite ButtonBrownPressed => _buttonBrownPressed;
        public Sprite ButtonTab => _buttonTab;
        public Sprite ButtonTabPressed => _buttonTabPressed;
        public Sprite BarTrack => _barTrack;
        public Sprite BarFill => _barFill;
        public Vector4 BarInset => _barInset;
        public Sprite IconConfirm => _iconConfirm;
        public Sprite IconCancel => _iconCancel;
        public Sprite IconSandtimer => _iconSandtimer;
        public Sprite IconStopwatch => _iconStopwatch;
        public Sprite IconCoin => _iconCoin;
        public Sprite IconMail => _iconMail;
        public Sprite IconCheckIn => _iconCheckIn;
        public Sprite IconAlbum => _iconAlbum;
    }
}
