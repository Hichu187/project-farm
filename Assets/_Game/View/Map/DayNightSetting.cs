using System;
using UnityEngine;

namespace VeQue.View.Map
{
    public enum DayNightMode
    {
        Full = 0,
        Light = 1,
        Off = 2,
    }

    // Cài đặt ngày đêm của máy (Đợt 2 Phần 13): Đầy đủ / Nhẹ (nửa độ tint) / Tắt. PlayerPrefs, không nằm trong save.
    // "Tắt" chỉ tắt màu trời ban đêm; chế độ soi đèn (Phần 16) vẫn tối vì đó là luật chơi.
    public static class DayNightSetting
    {
        private const string Key = "VeQue.DayNightMode";

        private static bool _loaded;
        private static DayNightMode _mode;

        public static event Action Changed;

        public static DayNightMode Mode
        {
            get
            {
                if (!_loaded)
                {
                    _loaded = true;
                    try
                    {
                        int value = PlayerPrefs.GetInt(Key, 0);
                        _mode = value >= 0 && value <= 2 ? (DayNightMode)value : DayNightMode.Full;
                    }
                    catch (PlayerPrefsException)
                    {
                        _mode = DayNightMode.Full;
                    }
                }
                return _mode;
            }
            set
            {
                if (_loaded && _mode == value)
                    return;
                _loaded = true;
                _mode = value;
                try
                {
                    PlayerPrefs.SetInt(Key, (int)value);
                    PlayerPrefs.Save();
                }
                catch (PlayerPrefsException)
                {
                    // Không lưu được thì chỉ dùng trong phiên này.
                }
                Changed?.Invoke();
            }
        }

        // Đầy đủ → Nhẹ → Tắt → Đầy đủ.
        public static DayNightMode Cycle()
        {
            Mode = (DayNightMode)(((int)Mode + 1) % 3);
            return Mode;
        }

        public static string Label(DayNightMode mode)
        {
            switch (mode)
            {
                case DayNightMode.Light: return "Ngày đêm: Nhẹ";
                case DayNightMode.Off: return "Ngày đêm: Tắt";
                default: return "Ngày đêm: Đầy đủ";
            }
        }
    }
}
