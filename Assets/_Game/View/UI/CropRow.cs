using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Một dòng cây trong Kho, Cửa hàng, bảng chọn hạt: ảnh, tên, thông tin và 2 nút.
    public sealed class CropRow : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _info;
        [SerializeField] private Button _primary;
        [SerializeField] private TMP_Text _primaryLabel;
        [SerializeField] private Button _secondary;
        [SerializeField] private TMP_Text _secondaryLabel;

        public Button Primary => _primary;
        public Button Secondary => _secondary;

        public void SetCrop(Sprite icon, string displayName)
        {
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _name.text = displayName;
        }

        public void SetLabels(string primary, string secondary)
        {
            _primaryLabel.text = primary;
            _secondaryLabel.text = secondary;
        }

        public void SetInfo(string info)
        {
            _info.text = info;
        }
    }
}
