using UnityEngine;

namespace VeQue.View.UI
{
    // Bảng Mồi ở vùng câu: BottomSheet chứa BaitListView (danh sách tự vẽ lại khi bảng mở).
    public sealed class BaitPanel : MonoBehaviour
    {
        [SerializeField] private BottomSheet _sheet;

        public void Open() => _sheet.Open();
    }
}
