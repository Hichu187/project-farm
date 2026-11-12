using System;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Anim;
using VeQue.View.CameraControl;
using VeQue.View.Map;

namespace VeQue.View.Character
{
    // Nhân vật ở nông trại:
    // - Chạm đất ở toàn cảnh → đi tới (chạm chỗ không đi được thì tới ô gần nhất tới được), có dấu điểm đến.
    //   Đang có hàng việc thì chạm đất cũng hủy hàng việc. Chạm vùng "sắp có" chỉ hiện thông báo (GameUI lo).
    // - Hàng việc (GameBootstrap.Tasks): lấy việc kế tiếp → tìm chỗ đứng sát ô → đi tới → quay mặt về ô →
    //   hoạt ảnh làm việc / tưới → FarmInteraction.Execute (Farm kiểm tra lại) → ô sáng lên → việc kế tiếp.
    //   Làm đúng thứ tự được chọn. Tốc độ đi tăng theo số việc còn lại (có trần).
    // - Chạm nhân vật hoặc nút Dừng → hủy phần còn lại. Tab bị ẩn → làm ngay hết việc còn lại (rồi GameBootstrap lưu).
    // Logic zoom / kéo camera giữ nguyên; camera không đi theo nhân vật.
    // Chạm cổng / chỗ câu: RegionTravel xử lý trước (đi câu, về nhà, đi tới chỗ câu) qua WalkTo / PlaceAt / Face.
    public sealed class FarmCharacterController : MonoBehaviour, ITapInterceptor
    {
        private enum State
        {
            Idle,
            Walking,
            WalkingToTask,
            Working,
        }

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmInteraction _interaction;
        [SerializeField] private FarmView _farmView;
        [SerializeField] private CameraTouchInput _touch;
        [SerializeField] private Camera _camera;
        [SerializeField] private CharacterMotor _motor;
        [Tooltip("Chỗ nhân vật đứng khi vào game (lấy ô đi được gần nhất).")]
        [SerializeField] private Transform _spawnPoint;
        [Tooltip("Đi câu / về nhà, chỗ câu. Để trống thì tự tìm; không có thì chỉ có nông trại.")]
        [SerializeField] private RegionTravel _travel;

        [Header("Làm việc")]
        [Tooltip("Thời gian tối thiểu cho một việc (giây), khi hoạt ảnh ngắn hơn.")]
        [SerializeField] private float _minWorkSeconds = 0.25f;
        [Tooltip("Mỗi việc còn chờ tăng tốc độ đi thêm bấy nhiêu (0.1 = +10%).")]
        [SerializeField] private float _speedPerPendingTask = 0.1f;
        [SerializeField] private float _maxSpeedMultiplier = 2f;
        [Tooltip("Vùng chạm vào nhân vật để dừng hàng việc (ô, tính từ chân).")]
        [SerializeField] private Vector2 _tapSize = new Vector2(1f, 1.25f);

        [Header("Dấu điểm đến: khung 4 góc thở quanh ô đích")]
        [SerializeField] private bool _showTargetMarker = true;
        [Tooltip("Sunnyside UI/selectbox_*. Gán bằng menu Về Quê → Khung chọn: tạo asset + gán vào scene.")]
        [SerializeField] private SelectBoxSkin _selectBoxSkin;
        [SerializeField] private Color _targetBoxColor = new Color(1f, 1f, 1f, 0.8f);
        // Trên cây và nhân vật, dưới icon gợi ý tưới (cùng mức khung chọn ô).
        [SerializeField] private int _targetBoxSortingOrder = 290;

        private readonly GridPath _finder = new GridPath();
        private SelectBox _marker;
        private State _state;
        private FarmTask _task;
        private GridCell _taskPlotCell;
        private float _workEnd;
        // Gọi khi tới nơi của lần WalkTo (đi ra cổng, tới chỗ câu); chạm đất đi chỗ khác thì bỏ.
        private Action _arrivedCallback;

        public bool IsBusy => _state == State.WalkingToTask || _state == State.Working;
        // Transform của thân nhân vật (vị trí chân) – camera đi theo cái này.
        public Transform Body => _motor.transform;

        public CharacterView View => _motor.View;

        public Facing Facing => _motor.Facing;

        // Đang câu cá: bỏ qua chạm đất (không đi, không qua cổng). Kéo camera vẫn được.
        public bool InputLocked { get; set; }
        // Đang đi tới đích của WalkTo (chưa tới, chưa bị đổi hướng).
        public bool HasPendingArrival => _arrivedCallback != null;

        // Xóa phần còn lại của hàng việc (kể cả việc đang đi tới / đang làm dở), trả hạt giữ chỗ.
        public void CancelTasks()
        {
            _game.Tasks.Cancel();
            if (IsBusy)
            {
                _motor.Stop();
                _state = State.Idle;
            }
        }

        // Đi tới ô đi được gần target nhất rồi gọi onArrived. false nếu không có đường.
        public bool WalkTo(GridCell target, Action onArrived)
        {
            WalkGridSource source = _motor.GridSource;
            if (!_finder.FindNearestReachable(source.Grid, _motor.Cell, target, out GridCell reach))
                return false;

            _motor.SpeedMultiplier = 1f;
            if (!_motor.TryWalkTo(reach))
                return false;

            _state = State.Walking;
            _arrivedCallback = onArrived;
            ShowMarker(reach);
            return true;
        }

        // Đặt ngay nhân vật vào ô đi được gần cell nhất (chuyển cảnh sang vùng khác).
        public void PlaceAt(GridCell cell)
        {
            WalkGrid grid = _motor.GridSource.Grid;
            GridCell target = _finder.FindNearestReachable(grid, cell, cell, out GridCell found) ? found : cell;
            _motor.PlaceAt(target);
            _arrivedCallback = null;
            _state = State.Idle;
            HideMarker();
            _motor.View.Play(CharacterAnim.Idle, _motor.Facing);
        }

        public void Face(Facing facing)
        {
            _motor.View.Play(CharacterAnim.Idle, facing);
        }

        public bool TryHandleTap(Vector2 world)
        {
            if (_game.Tasks.IsEmpty)
                return false;

            Vector3 feet = _motor.transform.position;
            var rect = new Rect(feet.x - _tapSize.x * 0.5f, feet.y - 0.25f, _tapSize.x, _tapSize.y);
            if (!rect.Contains(world))
                return false;

            CancelTasks();
            return true;
        }

        private void Awake()
        {
            // Đợt A chưa có các tham chiếu này: chưa gán thì tự tìm trong scene (chỉ lúc khởi động).
            if (_camera == null)
                _camera = Camera.main;
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_interaction == null)
                _interaction = FindFirstObjectByType<FarmInteraction>();
            if (_farmView == null)
                _farmView = FindFirstObjectByType<FarmView>();
            if (_travel == null)
                _travel = FindFirstObjectByType<RegionTravel>();
            if (_showTargetMarker)
                CreateMarker();
        }

        private void Start()
        {
            WalkGridSource source = _motor.GridSource;
            Vector3 spawn = _spawnPoint != null ? _spawnPoint.position : _motor.transform.position;
            GridCell wanted = source.WorldToCell(spawn);
            GridCell cell = _finder.FindNearestReachable(source.Grid, wanted, wanted, out GridCell found) ? found : wanted;
            if (!source.Grid.IsWalkable(cell))
                Debug.LogWarning($"FarmCharacterController: không có ô đi được quanh điểm xuất phát {wanted}.", this);
            _motor.PlaceAt(cell);

            // Farm có từ Awake của GameBootstrap nên đăng ký ở Start.
            _game.Farm.Changed += ApplyLook;
            ApplyLook();
        }

        // Ngoại hình theo màn tạo nhân vật; SetLook bỏ qua nếu không đổi nên gọi mỗi lần Farm đổi cũng rẻ.
        private void ApplyLook()
        {
            CharacterLook look = _game.Farm.State.Character;
            _motor.View.SetLook(look.HairStyle, look.HairColor);
        }

        private void OnEnable()
        {
            _touch.TappedGround += HandleTapGround;
            _touch.TapInterceptor = this;
            _motor.Arrived += HandleArrived;
            _game.PageHiding += FinishAllNow;
        }

        private void OnDisable()
        {
            _touch.TappedGround -= HandleTapGround;
            if (ReferenceEquals(_touch.TapInterceptor, this))
                _touch.TapInterceptor = null;
            _motor.Arrived -= HandleArrived;
            _game.PageHiding -= FinishAllNow;
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= ApplyLook;
            if (_marker != null)
                Destroy(_marker.gameObject);
        }

        private void Update()
        {
            if (_state == State.Working && Time.time >= _workEnd)
                FinishCurrentTask();

            // Có việc mới thì bỏ việc đi dạo để làm việc.
            if ((_state == State.Idle || _state == State.Walking) && _game.Tasks.PendingCount > 0)
                StartNextTask();
        }

        // ---------- Hàng việc ----------

        private void StartNextTask()
        {
            FarmTaskQueue tasks = _game.Tasks;
            while (tasks.TryBeginNext(out FarmTask task))
            {
                if (TryWalkToTask(task))
                    return;

                // Không tới được ô này (không nên xảy ra nếu Blockers đúng): bỏ qua việc, sang việc kế.
                Debug.LogWarning($"FarmCharacterController: không tới được ô {task.Plot}, bỏ qua.", this);
                tasks.CompleteCurrent();
            }

            if (_state != State.Walking)
                _state = State.Idle;
        }

        private bool TryWalkToTask(FarmTask task)
        {
            if (!_farmView.TryGetPlotCell(task.Plot, out Vector3Int plotCell))
                return false;

            var target = new GridCell(plotCell.x, plotCell.y);
            WalkGridSource source = _motor.GridSource;
            if (!_finder.FindStandCell(source.Grid, target, _motor.Cell, out GridCell stand))
                return false;
            if (!_motor.TryWalkTo(stand))
                return false;

            _task = task;
            _taskPlotCell = target;
            _state = State.WalkingToTask;
            _arrivedCallback = null;
            HideMarker();

            float multiplier = 1f + _speedPerPendingTask * _game.Tasks.PendingCount;
            _motor.SpeedMultiplier = Mathf.Min(_maxSpeedMultiplier, multiplier);

            // Nhân vật đang ở ngoài màn hình → hiện ngay ở chỗ đường đi bắt đầu vào khung hình.
            Rect view = GetCameraRect();
            if (!view.Contains(_motor.transform.position))
                _motor.SkipToFirstCellInside(view);
            return true;
        }

        private void HandleArrived()
        {
            if (_state == State.Walking)
            {
                _state = State.Idle;
                HideMarker();
                Action callback = _arrivedCallback;
                _arrivedCallback = null;
                callback?.Invoke();
                return;
            }
            if (_state != State.WalkingToTask)
                return;

            Facing facing = CharacterMotor.FacingTowards(_motor.Cell, _taskPlotCell);
            CharacterAnim anim = _task.Kind == FarmTaskKind.Water ? CharacterAnim.Water : CharacterAnim.Work;
            _motor.View.Play(anim, facing, true);
            _workEnd = Time.time + Mathf.Max(_minWorkSeconds, _motor.View.GetDuration(anim));
            _state = State.Working;
        }

        private void FinishCurrentTask()
        {
            FarmTask task = _task;
            _state = State.Idle;
            _motor.View.Play(CharacterAnim.Idle, _motor.Facing);

            FarmResult result = _interaction.Execute(task);
            if (result == FarmResult.Ok)
                _farmView.Flash(task.Plot);
            else
                _interaction.Report(result);
            _game.Tasks.CompleteCurrent();
        }

        // Tab bị ẩn / app tạm dừng: làm ngay mọi việc còn lại theo thứ tự (Farm vẫn kiểm tra từng việc).
        private void FinishAllNow()
        {
            FarmTaskQueue tasks = _game.Tasks;
            if (tasks.IsEmpty)
                return;

            if (tasks.HasCurrent)
            {
                _interaction.Execute(tasks.Current);
                tasks.CompleteCurrent();
            }
            while (tasks.TryBeginNext(out FarmTask task))
            {
                _interaction.Execute(task);
                tasks.CompleteCurrent();
            }

            _motor.Stop();
            _state = State.Idle;
        }

        // ---------- Đi tới chỗ chạm ----------

        private void HandleTapGround(Vector2 world)
        {
            if (InputLocked)
                return;

            // Cổng, chỗ câu, đang chuyển cảnh: RegionTravel lo.
            if (_travel != null && _travel.HandleGroundTap(world))
                return;

            WalkGridSource source = _motor.GridSource;
            if (source.IsInBlockedArea(world))
                return;

            if (!_game.Tasks.IsEmpty)
                CancelTasks();

            GridCell tapped = source.WorldToCell(world);
            if (!_finder.FindNearestReachable(source.Grid, _motor.Cell, tapped, out GridCell target))
                return;

            _motor.SpeedMultiplier = 1f;
            // Tới nơi thì HandleArrived ẩn dấu, kể cả khi đích là ô đang đứng.
            if (_motor.TryWalkTo(target))
            {
                _state = State.Walking;
                _arrivedCallback = null;
                ShowMarker(target);
            }
        }

        private Rect GetCameraRect()
        {
            Vector3 min = _camera.ViewportToWorldPoint(Vector3.zero);
            Vector3 max = _camera.ViewportToWorldPoint(Vector3.one);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        // ---------- Dấu điểm đến ----------

        private void ShowMarker(GridCell cell)
        {
            if (_marker == null)
                return;
            Vector3 min = _motor.GridSource.CellToWorld(cell);
            _marker.Show(new Rect(min.x, min.y, 1f, 1f), SelectBox.Motion.Breathe, _targetBoxColor);
        }

        private void HideMarker()
        {
            if (_marker != null)
                _marker.Hide();
        }

        private void CreateMarker()
        {
            if (_selectBoxSkin == null || !_selectBoxSkin.IsValid)
            {
                Debug.LogWarning("FarmCharacterController: chưa gán SelectBoxSkin đủ 4 góc, không hiện dấu điểm đến.", this);
                return;
            }

            // Không làm con của nhân vật (dấu đứng yên ở ô đích).
            _marker = SelectBox.Create(_selectBoxSkin, null, "CharacterTargetMarker", _targetBoxSortingOrder);
        }
    }
}
