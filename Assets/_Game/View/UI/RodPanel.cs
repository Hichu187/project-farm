using UnityEngine;

namespace VeQue.View.UI
{
    // Bảng Cần ở vùng câu: BottomSheet chứa RodListView (danh sách tự vẽ lại khi bảng mở).
    public sealed class RodPanel : MonoBehaviour
    {
        [SerializeField] private BottomSheet _sheet;

        public void Open() => _sheet.Open();
    }
}
