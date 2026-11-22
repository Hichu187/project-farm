using System;
using System.Collections.Generic;
using UnityEngine;

namespace VeQue.View.Map
{
    // Trang trí Tết ở cổng (Đợt 2 Phần 12a): cây nêu + câu đối, bật khi sự kiện có Overlay "tet_trang_tri" đang chạy
    // (Đón Tết 23 tháng Chạp → Tết Nguyên Đán tới mùng 10). Xét mỗi giây. Sprite tạm là ô vuông (người dùng thay art sau);
    // mỗi món là 1 SpriteRenderer con, pivot đáy, sắp theo trục Y như cây / nhân vật (sortingOrder đặt lúc dựng).
    public sealed class TetDecor : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private string _overlayId = "tet_trang_tri";
        [Tooltip("Các món trang trí (con của object này) – bật / tắt cùng lúc.")]
        [SerializeField] private GameObject[] _items = Array.Empty<GameObject>();

        private readonly List<string> _overlays = new List<string>();
        private float _nextCheck;
        private int _shown = -1;

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            // Thứ tự vẽ theo ô Y hiện tại (người dùng kéo vị trí trong scene thì vẫn đúng).
            foreach (GameObject item in _items)
            {
                if (item != null && item.TryGetComponent(out SpriteRenderer renderer))
                    renderer.sortingOrder = SortingOrders.ByCellY(Mathf.FloorToInt(item.transform.position.y));
            }
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
            _game.Farm.GetOverlays(_overlays);
            int on = _overlays.Contains(_overlayId) ? 1 : 0;
            if (on == _shown)
                return;
            _shown = on;
            foreach (GameObject item in _items)
            {
                if (item != null)
                    item.SetActive(on == 1);
            }
        }
    }
}
