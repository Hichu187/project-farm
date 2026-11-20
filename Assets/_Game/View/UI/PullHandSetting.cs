using System;
using UnityEngine;

namespace VeQue.View.UI
{
    // Cài đặt của máy (Đợt 2 Phần 6): chữ thập kéo cá ở góc dưới phải (mặc định) hay dưới trái.
    // Lưu trong PlayerPrefs, không nằm trong save game (không đổi version save).
    public static class PullHandSetting
    {
        private const string Key = "VeQue.PullLeftHand";

        private static bool _loaded;
        private static bool _leftHanded;

        public static event Action Changed;

        public static bool LeftHanded
        {
            get
            {
                if (!_loaded)
                {
                    _loaded = true;
                    try
                    {
                        _leftHanded = PlayerPrefs.GetInt(Key, 0) == 1;
                    }
                    catch (PlayerPrefsException)
                    {
                        _leftHanded = false;
                    }
                }
                return _leftHanded;
            }
            set
            {
                if (_loaded && _leftHanded == value)
                    return;
                _loaded = true;
                _leftHanded = value;
                try
                {
                    PlayerPrefs.SetInt(Key, value ? 1 : 0);
                    PlayerPrefs.Save();
                }
                catch (PlayerPrefsException)
                {
                    // Trình duyệt chặn lưu: vẫn dùng được trong phiên này.
                }
                Changed?.Invoke();
            }
        }

        public static void Toggle() => LeftHanded = !LeftHanded;
    }
}
