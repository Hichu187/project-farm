using UnityEngine;

namespace VeQue.View.UI
{
    // Bảng Giỏ cá ở vùng câu: BottomSheet chứa BasketView (vẽ lại khi bảng mở). Kho ở nông trại dùng cùng BasketView.
    public sealed class BasketPanel : MonoBehaviour
    {
        [SerializeField] private BottomSheet _sheet;

        public void Open() => _sheet.Open();
    }
}
