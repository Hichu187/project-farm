using System;
using AOT;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace VeQue.View.Save
{
    // Cầu nối sang trình duyệt (Plugins/WebGL/VeQueSave.jslib): sự kiện ẩn tab và màn sao lưu HTML.
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
#endif

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
