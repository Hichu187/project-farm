using System;
using AOT;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace VeQue.View.Save
{
    // Cầu nối sang trình duyệt (Plugins/WebGL/VeQueSave.jslib): sự kiện ẩn tab, màn sao lưu HTML, rung máy.
    public static class WebBridge
    {
        private delegate int ImportCallback(string code);

        public static event Action PageHidden;

        // Trả về (int)SaveLoadResult để màn sao lưu báo lỗi.
        public static Func<string, int> ImportHandler { get; set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void VeQue_RegisterPageHidden(Action callback);

        [DllImport("__Internal")]
        private static extern void VeQue_ShowBackup(string code, ImportCallback callback);

        [DllImport("__Internal")]
        private static extern int VeQue_GetUtcOffsetMinutes();

        [DllImport("__Internal")]
        private static extern void VeQue_Vibrate(int milliseconds);
#endif

        // Rung máy (cá cắn câu). Chỉ có tác dụng trên trình duyệt Android; nơi khác bỏ qua.
        public static void Vibrate(int milliseconds)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            VeQue_Vibrate(milliseconds);
#endif
        }

        // Độ lệch múi giờ so với UTC (giây, dương = phía đông). Trên WebGL lấy từ trình duyệt vì múi giờ
        // trong C# có thể sai; nơi khác lấy từ máy.
        public static int GetUtcOffsetSeconds()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return VeQue_GetUtcOffsetMinutes() * 60;
#else
            return (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
#endif
        }

        public static void RegisterPageEvents()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            VeQue_RegisterPageHidden(HandlePageHidden);
#endif
        }

        // True nếu đã mở màn sao lưu HTML. Ngoài WebGL không có HTML nên chép mã vào clipboard và trả về false.
        public static bool ShowBackup(string code)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Để ô HTML nhận được phím; game không dùng bàn phím.
            WebGLInput.captureAllKeyboardInput = false;
            VeQue_ShowBackup(code, HandleImport);
            return true;
#else
            GUIUtility.systemCopyBuffer = code;
            return false;
#endif
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void HandlePageHidden() => PageHidden?.Invoke();

        [MonoPInvokeCallback(typeof(ImportCallback))]
        private static int HandleImport(string code)
        {
            return ImportHandler != null ? ImportHandler(code) : 1;
        }
    }
}
