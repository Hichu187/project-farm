using UnityEngine;
using UnityEngine.UI;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Nút "Điểm danh" cột trái HUD (22/11/2026, dưới Album, cả hai vùng): mở popup điểm danh; chấm đỏ khi hôm nay
    // còn quà chưa bấm Nhận. Xét mỗi giây (điều kiện đổi theo giờ, không có Farm.Changed).
    public sealed class CheckInButton : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private Button _button;
        [SerializeField] private CheckInPopup _popup;
        [SerializeField] private GameObject _badge;

        private float _nextCheck;

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            _button.onClick.AddListener(() => _popup.ShowCalendar());
            bool hasCheckIn = _game != null && _game.Farm != null && _game.Farm.Definition.CheckIn.IsEnabled;
            gameObject.SetActive(hasCheckIn);
            Refresh();
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            if (now < _nextCheck)
                return;
            _nextCheck = now + 1f;
            Refresh();
        }

        private void Refresh()
        {
            if (_game == null || _game.Farm == null)
                return;
            bool claimable = _game.Farm.CanCheckIn;
            if (_badge.activeSelf != claimable)
                _badge.SetActive(claimable);
        }
    }
}
