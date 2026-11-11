using System;
using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Character
{
    // Cho nhân vật đi theo đường A* (GridPath) trên WalkGridSource. Vị trí thật giữ dạng số thực,
    // vị trí hiển thị làm tròn theo pixel. Chân nhân vật (pivot sprite) đặt giữa ô theo chiều ngang,
    // cao hơn mép dưới ô _footOffsetPixels pixel.
    [RequireComponent(typeof(CharacterView))]
    public sealed class CharacterMotor : MonoBehaviour
    {
        private const float PixelsPerUnit = 16f;

        [SerializeField] private WalkGridSource _grid;
        [Tooltip("Tốc độ đi (ô / giây).")]
        [SerializeField, Min(0.1f)] private float _speed = 4f;
        [SerializeField] private int _footOffsetPixels = 4;

        private readonly GridPath _finder = new GridPath();
        private readonly List<GridCell> _path = new List<GridCell>();
        private CharacterView _view;
        private Vector3 _position;
        private int _next;

        // Báo khi đi tới ô cuối của đường đi.
        public event Action Arrived;

        public bool IsMoving { get; private set; }
        // Hệ số nhân tốc độ (đợt sau: tăng khi hàng việc dài).
        public float SpeedMultiplier { get; set; } = 1f;
        public WalkGridSource GridSource => _grid;
        public GridCell Cell => _grid.WorldToCell(_position);
        public Facing Facing => _view.Facing;
        public CharacterView View => _view;

        // Ô đích của lần đi hiện tại.
        public GridCell Destination => _path.Count > 0 ? _path[_path.Count - 1] : Cell;

        // Đặt ngay nhân vật vào ô (không đi bộ).
        public void PlaceAt(GridCell cell)
        {
            Stop();
            _position = CellToFeet(cell);
            ApplyPosition();
        }

        // Tìm đường từ ô hiện tại tới target rồi đi. Đang đi thì đổi đích. false nếu không có đường.
        public bool TryWalkTo(GridCell target)
        {
            if (!_finder.FindPath(_grid.Grid, Cell, target, _path))
                return false;

            // Điểm đầu của đường là ô đang đứng: đi về giữa ô đó trước rồi mới rẽ (tránh đi chéo xuyên góc).
            _next = 0;
            IsMoving = true;
            return true;
        }

        // Đang đi: đặt ngay nhân vật vào ô đầu tiên của đường đi nằm trong view (toạ độ world) rồi đi tiếp từ đó.
        // Dùng khi nhân vật ở ngoài màn hình mà người chơi đang nhìn khu khác. Không có ô nào trong view thì tới thẳng đích.
        public void SkipToFirstCellInside(Rect view)
        {
            if (!IsMoving || _path.Count == 0)
                return;

            int index = _path.Count - 1;
            for (int i = _next; i < _path.Count; i++)
            {
                if (view.Contains(CellToFeet(_path[i])))
                {
                    index = i;
                    break;
                }
            }

            _position = CellToFeet(_path[index]);
            _next = index + 1;
            ApplyPosition();
        }

        public void Stop()
        {
            _path.Clear();
            IsMoving = false;
            _view.Play(CharacterAnim.Idle, _view.Facing);
        }

        public void Face(Facing facing) => _view.Play(_view.Anim, facing);

        // Hướng nhìn từ ô hiện tại về ô target (dùng khi tới chỗ làm việc).
        public static Facing FacingTowards(GridCell from, GridCell to)
        {
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;
            if (Mathf.Abs(dx) > Mathf.Abs(dy))
                return dx < 0 ? Facing.Left : Facing.Right;
            return dy > 0 ? Facing.Up : Facing.Down;
        }

        public Vector3 CellToFeet(GridCell cell)
        {
            return _grid.CellToWorld(cell) + new Vector3(0.5f, _footOffsetPixels / PixelsPerUnit, 0f);
        }

        private void Awake()
        {
            _view = GetComponent<CharacterView>();
            _position = transform.position;
        }

        private void Update()
        {
            if (!IsMoving)
                return;

            float step = _speed * SpeedMultiplier * Time.deltaTime;
            while (step > 0f && _next < _path.Count)
            {
                Vector3 target = CellToFeet(_path[_next]);
                Vector3 delta = target - _position;
                float distance = delta.magnitude;
                if (distance > 0.0001f)
                    _view.Play(CharacterAnim.Walk, ToFacing(delta));

                if (distance <= step)
                {
                    _position = target;
                    step -= distance;
                    _next++;
                }
                else
                {
                    _position += delta / distance * step;
                    step = 0f;
                }
            }

            ApplyPosition();

            if (_next >= _path.Count)
            {
                IsMoving = false;
                _view.Play(CharacterAnim.Idle, _view.Facing);
                Arrived?.Invoke();
            }
        }

        private void ApplyPosition()
        {
            transform.position = new Vector3(
                Mathf.Round(_position.x * PixelsPerUnit) / PixelsPerUnit,
                Mathf.Round(_position.y * PixelsPerUnit) / PixelsPerUnit,
                transform.position.z);
        }

        private static Facing ToFacing(Vector3 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x < 0f ? Facing.Left : Facing.Right;
            return delta.y > 0f ? Facing.Up : Facing.Down;
        }
    }
}
