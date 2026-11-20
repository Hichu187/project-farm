using System;
using UnityEngine;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Hàng nút chuyển trang trong một bảng (duyệt 13/11/2026): Cửa hàng "Hạt giống | Cần câu | Mồi câu",
    // Kho "Nông sản | Giỏ cá". Gắn trong Sheet của bảng: mỗi lần mở (Sheet bật lại) về trang đầu.
    public sealed class ShopTabs : MonoBehaviour
    {
        [Tooltip("Nút theo thứ tự, cùng thứ tự với Views.")]
        [SerializeField] private Button[] _tabs = Array.Empty<Button>();
        [SerializeField] private GameObject[] _views = Array.Empty<GameObject>();
        [SerializeField] private Color _active = new Color(0.45f, 0.62f, 0.28f);
        [SerializeField] private Color _inactive = new Color(0.60f, 0.40f, 0.24f);

        private void Awake()
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                int index = i;
                _tabs[i].onClick.AddListener(() => Show(index));
            }
        }

        private void OnEnable()
        {
            Show(0);
        }

        private void Show(int index)
        {
            for (int i = 0; i < _views.Length; i++)
            {
                if (_views[i] != null)
                    _views[i].SetActive(i == index);
            }
            for (int i = 0; i < _tabs.Length; i++)
            {
                if (_tabs[i].targetGraphic != null)
                    _tabs[i].targetGraphic.color = i == index ? _active : _inactive;
            }
        }
    }
}
