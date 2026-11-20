using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.View.Map
{
    // Ngày đêm (Đợt 2 Phần 13): gắn vào camera thế giới. Tạo một quad phủ khung nhìn (shader VeQue/WorldLight, hòa màu nhân)
    // ở sortingOrder 280: mọi thứ vẽ dưới 280 bị tint theo giờ, icon / dấu trên 280 và UI giữ nguyên.
    // Màu trời theo GIỜ GAME (GameDayClock, 1 ngày = DayNightProfile.DayLengthMinutes phút thật – chốt 21/11/2026),
    // không theo giờ thật. Ban ngày (không tint, không tối) thì tắt quad.
    // Darkness: độ tối thêm cho chế độ soi đèn (Phần 16) – không phụ thuộc cài đặt Tắt.
    [RequireComponent(typeof(Camera))]
    public sealed class DayNightController : MonoBehaviour
    {
        private const int MaxLights = 8;
        private const float PixelsPerUnit = 16f;
        // Dư mỗi cạnh để không hở mép khi camera lệch nửa pixel.
        private const float Margin = 1f;

        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int DarknessId = Shader.PropertyToID("_Darkness");
        private static readonly int LightCountId = Shader.PropertyToID("_LightCount");
        private static readonly int LightsId = Shader.PropertyToID("_Lights");
        private static readonly int LightColorsId = Shader.PropertyToID("_LightColors");

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private DayNightProfile _profile;
        [Tooltip("Material dùng shader VeQue/WorldLight.")]
        [SerializeField] private Material _material;
        [Tooltip("Dưới mức này bị tint (cây, nhân vật…); icon / dấu trên cây dùng order cao hơn.")]
        [SerializeField] private int _sortingOrder = 280;
        [Tooltip("Mức Nhẹ: giữ bao nhiêu phần tint (0 = như tắt, 1 = như đầy đủ).")]
        [Range(0f, 1f)] [SerializeField] private float _lightModeStrength = 0.5f;

        private readonly Vector4[] _lights = new Vector4[MaxLights];
        private readonly Vector4[] _lightColors = new Vector4[MaxLights];
        private readonly WorldLight[] _picked = new WorldLight[MaxLights];
        private readonly float[] _pickedDistance = new float[MaxLights];

        private Camera _camera;
        private MeshRenderer _renderer;
        private Material _instance;
        private IReadOnlyList<DayNightKey> _keys;
        private float _minLuminance = 1f;
        private GameDayClock _dayClock = new GameDayClock();

        // Độ tối thêm (0–1) cho soi đèn; 0 = chỉ màu trời.
        public float Darkness { get; set; }

        // Ép giờ trời (0–24) – vd. trời tối quanh giao thừa (Phần 12b); < 0 = theo giờ game.
        public float ForcedHour { get; set; } = -1f;

        // Giờ game hiện tại (0–24) dùng cho màu trời và đồng hồ HUD (đã tính ForcedHour).
        public float GameHour => ForcedHour >= 0f ? ForcedHour % 24f
            : _game != null ? _dayClock.HourOf(_game.NowSeconds) : 12f;

        // 0 = ban ngày, 1 = tối nhất trong profile (đèn / đom đóm sáng theo hệ số này). Tính lại mỗi frame.
        public float NightAmount { get; private set; }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            _keys = _profile != null ? _profile.ToKeys() : null;
            if (_profile != null)
                _dayClock = _profile.CreateDayClock();
            if (_keys != null)
            {
                foreach (DayNightKey key in _keys)
                    _minLuminance = Mathf.Min(_minLuminance, key.Luminance);
            }
            CreateQuad();
        }

        private void OnDestroy()
        {
            if (_instance != null)
                Destroy(_instance);
        }

        private void LateUpdate()
        {
            if (_renderer == null || _game == null || _game.Farm == null)
                return;

            DayNightKey sky = DayNightCurve.Evaluate(_keys, GameHour);
            float luminance = sky.Luminance;
            float onBelow = _profile != null ? _profile.LightsOnBelow : 0.92f;
            NightAmount = Mathf.Clamp01((onBelow - luminance) / Mathf.Max(0.01f, onBelow - _minLuminance));

            DayNightMode mode = DayNightSetting.Mode;
            float strength = mode == DayNightMode.Off ? 0f : mode == DayNightMode.Light ? _lightModeStrength : 1f;
            var tint = new Color(Mathf.Lerp(1f, sky.R, strength), Mathf.Lerp(1f, sky.G, strength), Mathf.Lerp(1f, sky.B, strength), 1f);
            float darkness = Mathf.Clamp01(Darkness);
            bool tinted = tint.r < 0.999f || tint.g < 0.999f || tint.b < 0.999f;
            bool active = tinted || darkness > 0f;
            if (_renderer.enabled != active)
                _renderer.enabled = active;
            if (!active)
                return;

            FitToCamera();
            int count = PickLights(strength);
            _instance.SetColor(TintId, tint);
            _instance.SetFloat(DarknessId, darkness);
            _instance.SetFloat(LightCountId, count);
            _instance.SetVectorArray(LightsId, _lights);
            _instance.SetVectorArray(LightColorsId, _lightColors);
        }

        // Tối đa 8 vòng sáng gần tâm camera nhất. Đèn chỉ-ban-đêm sáng theo NightAmount × mức cài đặt.
        private int PickLights(float settingStrength)
        {
            Vector2 center = _camera.transform.position;
            int count = 0;
            IReadOnlyList<WorldLight> all = WorldLight.Active;
            for (int i = 0; i < all.Count; i++)
            {
                WorldLight light = all[i];
                float intensity = light.Intensity * (light.NightOnly ? NightAmount * settingStrength : 1f);
                if (intensity <= 0.001f)
                    continue;
                float distance = ((Vector2)light.transform.position - center).sqrMagnitude;
                int slot = count < MaxLights ? count++ : -1;
                if (slot < 0)
                {
                    // Thay vòng xa nhất nếu vòng này gần hơn.
                    int farthest = 0;
                    for (int k = 1; k < MaxLights; k++)
                    {
                        if (_pickedDistance[k] > _pickedDistance[farthest])
                            farthest = k;
                    }
                    if (distance >= _pickedDistance[farthest])
                        continue;
                    slot = farthest;
                }
                _picked[slot] = light;
                _pickedDistance[slot] = distance;
                Vector2 p = light.transform.position;
                _lights[slot] = new Vector4(p.x, p.y, light.RadiusPixels / PixelsPerUnit, Mathf.Max(0.0001f, light.SoftPixels / PixelsPerUnit));
                Color c = light.Color;
                _lightColors[slot] = new Vector4(c.r, c.g, c.b, intensity);
            }
            for (int i = count; i < MaxLights; i++)
            {
                _picked[i] = null;
                _lights[i] = Vector4.zero;
                _lightColors[i] = Vector4.zero;
            }
            return count;
        }

        private void FitToCamera()
        {
            float height = _camera.orthographicSize * 2f + Margin * 2f;
            float width = _camera.orthographicSize * 2f * _camera.aspect + Margin * 2f;
            Transform quad = _renderer.transform;
            quad.localPosition = new Vector3(0f, 0f, _camera.nearClipPlane + 0.5f);
            quad.localScale = new Vector3(width, height, 1f);
        }

        private void CreateQuad()
        {
            if (_material == null)
            {
                Debug.LogWarning("DayNightController: chưa gán material VeQue/WorldLight – tắt ngày đêm.", this);
                enabled = false;
                return;
            }
            var go = new GameObject("DayNightQuad");
            go.transform.SetParent(transform, false);
            var mesh = new Mesh { name = "DayNightQuad" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f),
            };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(1f, 1f, 1f));
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            _renderer = go.AddComponent<MeshRenderer>();
            _instance = new Material(_material);
            _renderer.sharedMaterial = _instance;
            _renderer.sortingOrder = _sortingOrder;
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.enabled = false;
        }
    }
}
