using System;
using UnityEngine;
using VeQue.View.CameraControl;
using VeQue.View.Character;
using VeQue.View.UI;

namespace VeQue.View.Map
{
    // Đi câu / về nhà (Phần 10 giai đoạn 2). Hai vùng cùng scene, cùng Grid nhưng không nối nhau: chuyển bằng
    // màn tối (chuyển cảnh). Các bước: (hủy hàng việc, báo) → nhân vật đi tới cổng của vùng đang ở → tối dần →
    // đặt nhân vật ở cổng vùng mới, đổi giới hạn / chế độ camera → sáng dần.
    // Đang đi ra cổng mà bấm lần nữa thì bỏ qua đoạn đi bộ. Chạm đất chỗ khác thì hủy chuyến đi.
    // Ở vùng câu: chạm chỗ câu → nhân vật đi tới ô đứng, quay mặt ra nước, báo SpotReached.
    // Không lưu vùng: luôn vào game ở nông trại.
    public sealed class RegionTravel : MonoBehaviour
    {
        private enum Phase
        {
            None,
            WalkingToGate,
            Fading,
        }

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmCharacterController _character;
        [SerializeField] private CameraController _camera;
        [SerializeField] private CameraFollow _follow;
        [SerializeField] private ScreenFader _fader;
        [SerializeField] private PlayRegion _farm;
        [SerializeField] private PlayRegion _fishing;
        [SerializeField] private float _fadeDuration = 0.25f;

        private Phase _phase;
        private PlayRegion _target;

        public PlayRegion Current { get; private set; }
        public bool IsFishing => Current != null && Current == _fishing;
        public bool HasFishingRegion => _fishing != null;

        // Đã sang vùng mới (sau khi tối hẳn, trước khi sáng lại).
        public event Action<PlayRegion> RegionChanged;
        // Bắt đầu đi sang vùng khác (để đóng bảng UI).
        public event Action TravelStarted;
        public event Action<string> Message;
        public event Action<FishingSpot> SpotReached;

        public void GoFishing() => Travel(_fishing);

        public void GoHome() => Travel(_farm);

        // Gọi từ FarmCharacterController khi chạm đất. true = đã xử lý (cổng, chỗ câu, đang chuyển cảnh).
        public bool HandleGroundTap(Vector2 world)
        {
            if (_phase == Phase.Fading)
                return true;
            if (Current == null)
                return false;

            if (Current.GateContains(world))
            {
                Travel(Current == _farm ? _fishing : _farm);
                return true;
            }

            if (Current == _fishing && Current.TryGetSpotAt(world, out FishingSpot spot))
            {
                CancelTrip();
                _character.WalkTo(spot.StandCell, () =>
                {
                    _character.Face(spot.Facing);
                    SpotReached?.Invoke(spot);
                });
                return true;
            }

            // Chạm chỗ khác: hủy chuyến đi (nhân vật đi tới chỗ chạm như thường).
            CancelTrip();
            return false;
        }

        private void Awake()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_character == null)
                _character = FindFirstObjectByType<FarmCharacterController>();
            if (_camera == null)
                _camera = FindFirstObjectByType<CameraController>();
            if (_follow == null && _camera != null)
                _follow = _camera.GetComponent<CameraFollow>();
            if (_fader == null)
                _fader = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
            Current = _farm;
        }

        private void Update()
        {
            // Đường ra cổng bị thay (vd. hàng việc mới, chạm đất): coi như hủy chuyến đi.
            if (_phase == Phase.WalkingToGate && !_character.HasPendingArrival)
                CancelTrip();
        }

        private void Travel(PlayRegion target)
        {
            if (target == null || Current == null || target == Current || _phase == Phase.Fading)
                return;

            // Bấm lần nữa khi đang đi ra cổng: bỏ qua đoạn đi bộ.
            if (_phase == Phase.WalkingToGate && _target == target)
            {
                BeginFade();
                return;
            }

            int tasks = _game.Tasks.Count;
            if (tasks > 0)
            {
                _character.CancelTasks();
                Message?.Invoke(target == _fishing ? $"Đã hủy {tasks} việc để đi câu" : $"Đã hủy {tasks} việc");
            }

            _target = target;
            _phase = Phase.WalkingToGate;
            TravelStarted?.Invoke();
            if (_camera.CurrentMode == CameraController.Mode.Focused)
                _camera.ShowOverview();

            if (!_character.WalkTo(Current.GateCell, BeginFade))
                BeginFade();
        }

        private void CancelTrip()
        {
            if (_phase != Phase.WalkingToGate)
                return;
            _phase = Phase.None;
            _target = null;
        }

        private void BeginFade()
        {
            if (_target == null)
                return;
            _phase = Phase.Fading;
            if (_fader != null)
                _fader.FadeOut(_fadeDuration, Arrive);
            else
                Arrive();
        }

        private void Arrive()
        {
            PlayRegion target = _target;
            _target = null;
            Current = target;
            _character.PlaceAt(target.GateCell);

            if (target == _fishing)
            {
                _camera.UseRegionBounds(target.CameraRect);
                if (_follow != null)
                {
                    _follow.Target = _character.Body;
                    _follow.Following = true;
                    _follow.Snap();
                }
            }
            else
            {
                if (_follow != null)
                    _follow.Following = false;
                _camera.UseMapBounds();
                _camera.JumpTo(_character.Body.position);
            }

            RegionChanged?.Invoke(target);
            if (_fader != null)
                _fader.FadeIn(_fadeDuration, () => _phase = Phase.None);
            else
                _phase = Phase.None;
        }
    }
}
