using TMPro;
using UnityEngine;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Nút "Thư" trên HUD (Đợt 2 Phần 3): chấm đỏ kèm số thư chưa đọc / còn quà chưa nhận; 0 thì ẩn chấm.
    // Xét lại khi tiến trình đổi và mỗi giây (thư hết hạn / phát thư theo ngày).
    public sealed class MailButton : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private GameObject _badge;
        [SerializeField] private TMP_Text _badgeCount;

        private int _shown = -1;
        private float _nextCheck;

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= Refresh;
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
            int count = _game.Farm.MailAttentionCount;
            if (count == _shown)
                return;
            _shown = count;
            _badge.SetActive(count > 0);
            _badgeCount.text = count > 9 ? "9+" : count.ToString();
        }
    }
}
