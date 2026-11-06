using UnityEditor;
using UnityEngine;
using VeQue.View.Map;
using VeQue.View.Save;

namespace VeQue.EditorTools
{
    public static class SaveMenu
    {
        // Save trong Editor còn lại qua các lần Play; dùng menu này để chơi lại từ đầu.
        [MenuItem("Về Quê/Xóa save")]
        private static void DeleteSave()
        {
            if (!EditorUtility.DisplayDialog("Xóa save", "Xóa tiến trình đã lưu và khoảng thời gian đã tua trong Editor?", "Xóa", "Hủy"))
                return;

            PlayerPrefs.DeleteKey(SaveService.MainKey);
            PlayerPrefs.DeleteKey(SaveService.PreviousKey);
            PlayerPrefs.DeleteKey(GameBootstrap.TimeOffsetKey);
            PlayerPrefs.Save();
        }
    }
}
