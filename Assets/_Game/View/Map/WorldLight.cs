using System.Collections.Generic;
using UnityEngine;

namespace VeQue.View.Map
{
    // Một vòng sáng trong thế giới (Đợt 2 Phần 13): đèn dầu trong nhà, đom đóm, sau này đèn pin soi đèn.
    // Tự đăng ký khi bật; DayNightController lấy tối đa 8 vòng gần tâm camera nhất. Bán kính tính bằng pixel (PPU 16).
    public sealed class WorldLight : MonoBehaviour
    {
        private static readonly List<WorldLight> ActiveLights = new List<WorldLight>();

        [Tooltip("Bán kính vòng sáng (pixel).")]
        [Min(1f)] [SerializeField] private float _radiusPixels = 24f;
        [Tooltip("Độ mềm rìa (pixel) – phần mờ dần ở mép.")]
        [Min(0f)] [SerializeField] private float _softPixels = 10f;
        [SerializeField] private Color _color = new Color(1f, 0.86f, 0.6f);
        [Range(0f, 1f)] [SerializeField] private float _intensity = 1f;
        [Tooltip("Chỉ sáng ban đêm (đèn dầu, đom đóm). Tắt để luôn sáng (đèn pin soi đèn).")]
        [SerializeField] private bool _nightOnly = true;

        public static IReadOnlyList<WorldLight> Active => ActiveLights;

        public float RadiusPixels
        {
            get => _radiusPixels;
            set => _radiusPixels = Mathf.Max(1f, value);
        }

        public float SoftPixels
        {
            get => _softPixels;
            set => _softPixels = Mathf.Max(0f, value);
        }

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        public float Intensity
        {
            get => _intensity;
            set => _intensity = Mathf.Clamp01(value);
        }

        public bool NightOnly
        {
            get => _nightOnly;
            set => _nightOnly = value;
        }

        private void OnEnable()
        {
            if (!ActiveLights.Contains(this))
                ActiveLights.Add(this);
        }

        private void OnDisable()
        {
            ActiveLights.Remove(this);
        }
    }
}
