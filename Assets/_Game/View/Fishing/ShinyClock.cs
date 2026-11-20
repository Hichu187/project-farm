using UnityEngine;

namespace VeQue.View.Fishing
{
    // Đồng hồ chung cho shader VeQue/ShinyIcon: ghi global _ShinyTime từ Time.unscaledTime (vẫn chạy khi game tạm dừng).
    // Tự tạo khi vào game, không cần đặt vào scene. Lấy dư 304 giây (= 200 vòng 1,52 giây) để số float không mất chính xác.
    public sealed class ShinyClock : MonoBehaviour
    {
        private const float Wrap = 304f;
        private static readonly int TimeId = Shader.PropertyToID("_ShinyTime");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindFirstObjectByType<ShinyClock>() != null)
                return;
            var go = new GameObject(nameof(ShinyClock)) { hideFlags = HideFlags.HideInHierarchy };
            DontDestroyOnLoad(go);
            go.AddComponent<ShinyClock>();
        }

        private void Update()
        {
            Shader.SetGlobalFloat(TimeId, Time.unscaledTime % Wrap);
        }
    }
}
