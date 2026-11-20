using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Màu trời theo giờ (Đợt 2 Phần 13). Chỉnh trong Inspector sau khi chụp màn hình ở 7 khung giờ (Tua thời gian).
    // Màu là hệ số nhân (trắng = giữ nguyên). Luật: đêm ở nông trại không tối quá 30% (độ sáng ≥ 0,7).
    [CreateAssetMenu(menuName = "Về Quê/Day Night Profile", fileName = "DayNightProfile")]
    public sealed class DayNightProfile : ScriptableObject
    {
        [Serializable]
        public sealed class Key
        {
            [Range(0f, 23.99f)] public float Hour;
            public Color Color = Color.white;
        }

        [Tooltip("Sắp theo giờ tăng dần (giờ VN).")]
        [SerializeField] private Key[] _keys = Array.Empty<Key>();

        [Tooltip("Đèn / đom đóm bật khi trời tối hơn mức này (độ sáng màu trời).")]
        [Range(0f, 1f)] [SerializeField] private float _lightsOnBelow = 0.92f;

        [Header("Giờ trong game (chỉ hiển thị – chốt 21/11/2026)")]
        [Tooltip("1 ngày game dài bao nhiêu phút ngoài đời. Chỉ ảnh hưởng màu trời và đồng hồ HUD; logic dùng giờ thật.")]
        [Min(1f)] [SerializeField] private float _dayLengthMinutes = 30f;
        [Tooltip("Lệch pha (phút thật) để chọn lúc nào trời sáng.")]
        [SerializeField] private float _phaseMinutes;

        public float LightsOnBelow => _lightsOnBelow;

        public GameDayClock CreateDayClock()
            => new GameDayClock(Mathf.Max(GameDayClock.MinDayLengthSeconds, Mathf.RoundToInt(_dayLengthMinutes * 60f)),
                Mathf.RoundToInt(_phaseMinutes * 60f));

        public IReadOnlyList<DayNightKey> ToKeys()
        {
            var keys = new List<DayNightKey>(_keys.Length);
            foreach (Key key in _keys)
            {
                if (key != null)
                    keys.Add(new DayNightKey(key.Hour, key.Color.r, key.Color.g, key.Color.b));
            }
            keys.Sort((a, b) => a.Hour.CompareTo(b.Hour));
            DayNightCurve.Validate(keys);
            return keys;
        }
    }
}
