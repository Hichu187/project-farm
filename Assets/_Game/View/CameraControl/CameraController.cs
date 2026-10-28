using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

namespace VeQue.View.CameraControl
{
    // Di chuyển, zoom và giới hạn camera.
    // Zoom mượt: tạm tắt PixelPerfectCamera và nội suy orthographicSize, tới nơi bật lại ở bậc zoom nguyên.
    // Zoom theo nấc: đi qua từng bậc zoom nguyên, luôn sắc nét.
    // Toàn cảnh: giới hạn trong mép map. Đang zoom: giới hạn trong khu đang xem (và mép map).
    [RequireComponent(typeof(UnityEngine.Camera), typeof(PixelPerfectCamera))]
    public sealed class CameraController : MonoBehaviour
    {
        public enum Mode { Overview, Focused }

        [SerializeField] private Vector2Int _overviewRef = new Vector2Int(208, 370);
        [SerializeField] private Vector2Int _focusRef = new Vector2Int(90, 160);
        [SerializeField] private Tilemap _mapTilemap;
        [Tooltip("Nới (dương) hoặc thu (âm) mép map theo từng cạnh, tính bằng ô.")]
        [SerializeField] private EdgePadding _mapPadding;
        [SerializeField] private float _transitionDuration = 0.35f;
        [Tooltip("Bật: zoom mượt (pixel hơi lệch nét trong lúc chuyển). Tắt: zoom theo từng bậc nguyên.")]
        [SerializeField] private bool _smoothZoom = true;

        private UnityEngine.Camera _camera;
        private PixelPerfectCamera _pixelPerfect;
        private Rect _mapRect;
        private bool _hasMapRect;
        private int _zoom;
        private float _orthoSize;
        private bool _isTransitioning;
        private float _transitionElapsed;
        private Vector2 _fromPosition;
        private Vector2 _toPosition;
        private float _fromOrthoSize;
        private float _toOrthoSize;
        private int _toZoom;

        public Mode CurrentMode { get; private set; }
        public FocusArea Current { get; private set; }
        public bool IsTransitioning => _isTransitioning;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _pixelPerfect = GetComponent<PixelPerfectCamera>();
            _hasMapRect = TryGetMapRect(out _mapRect);
            CurrentMode = Mode.Overview;
            ApplyZoom(GetZoom(_overviewRef));
            SetPosition(transform.position);
        }

        private void OnDisable()
        {
            if (_isTransitioning)
                EndTransition();
        }

        private void Update()
        {
            if (_isTransitioning)
            {
                StepTransition();
                return;
            }

            // Tính lại mỗi frame để đúng cả khi xoay màn hình / đổi kích thước cửa sổ.
            int targetZoom = GetZoom(CurrentMode == Mode.Focused ? _focusRef : _overviewRef);
            if (targetZoom != _zoom)
                ApplyZoom(targetZoom);
            SetPosition(transform.position);
        }

        // screenDelta: số pixel ngón tay đã kéo trên màn hình.
        public void Pan(Vector2 screenDelta)
        {
            if (_isTransitioning)
                return;

            float unitsPerScreenPixel = 1f / (_zoom * _pixelPerfect.assetsPPU);
            SetPosition((Vector2)transform.position - screenDelta * unitsPerScreenPixel);
        }

        public void FocusOn(FocusArea area)
        {
            if (area == null)
                return;

            CurrentMode = Mode.Focused;
            Current = area;
            int focusZoom = GetZoom(_focusRef);
            BeginTransition(Clamp(area.Center, OrthoSizeFor(focusZoom), true), focusZoom);
        }

        public void ShowOverview()
        {
            CurrentMode = Mode.Overview;
            Current = null;
            int overviewZoom = GetZoom(_overviewRef);
            BeginTransition(Clamp(transform.position, OrthoSizeFor(overviewZoom), false), overviewZoom);
        }

        private void BeginTransition(Vector2 toPosition, int toZoom)
        {
            _fromPosition = transform.position;
            _toPosition = toPosition;
            _fromOrthoSize = _orthoSize;
            _toOrthoSize = OrthoSizeFor(toZoom);
            _toZoom = toZoom;
            _transitionElapsed = 0f;
            _isTransitioning = true;

            if (_smoothZoom)
            {
                // Tắt PixelPerfectCamera sẽ trả orthographicSize về giá trị cũ, nên gán lại ngay sau đó.
                _pixelPerfect.enabled = false;
                _camera.orthographicSize = _orthoSize;
            }
        }

        private void StepTransition()
        {
            _transitionElapsed += Time.unscaledDeltaTime;
            float t = _transitionDuration > 0f ? Mathf.Clamp01(_transitionElapsed / _transitionDuration) : 1f;
            float eased = t * t * (3f - 2f * t);

            if (t >= 1f)
            {
                EndTransition();
                return;
            }

            if (_smoothZoom)
            {
                // Nội suy theo 1/size để tốc độ phóng to và thu nhỏ cảm giác đều nhau.
                _orthoSize = 1f / Mathf.Lerp(1f / _fromOrthoSize, 1f / _toOrthoSize, eased);
                _camera.orthographicSize = _orthoSize;
            }
            else
            {
                float fromZoom = OrthoSizeToZoom(_fromOrthoSize);
                ApplyZoom(Mathf.RoundToInt(Mathf.Lerp(fromZoom, _toZoom, eased)));
            }

            // Trong lúc chuyển chỉ giữ trong mép map, tránh giật khi vùng nhìn còn lớn hơn khu.
            Vector2 position = Clamp(Vector2.Lerp(_fromPosition, _toPosition, eased), _orthoSize, false);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        private void EndTransition()
        {
            _isTransitioning = false;
            ApplyZoom(_toZoom);
            _pixelPerfect.enabled = true;
            SetPosition(_toPosition);
        }

        // Với Crop Frame None, vùng nhìn chỉ phụ thuộc zoom, nên đặt Ref = màn hình / zoom để ép đúng bậc zoom mong muốn.
        private void ApplyZoom(int zoom)
        {
            _zoom = Mathf.Max(1, zoom);
            _orthoSize = OrthoSizeFor(_zoom);
            _pixelPerfect.refResolutionX = Mathf.Max(1, Screen.width / _zoom);
            _pixelPerfect.refResolutionY = Mathf.Max(1, Screen.height / _zoom);
        }

        private void SetPosition(Vector2 position)
        {
            Vector2 clamped = Clamp(position, _orthoSize, true);
            transform.position = new Vector3(clamped.x, clamped.y, transform.position.z);
        }

        private Vector2 Clamp(Vector2 position, float orthoSize, bool includeFocusArea)
        {
            Vector2 halfView = new Vector2(orthoSize * Screen.width / Screen.height, orthoSize);

            if (includeFocusArea && CurrentMode == Mode.Focused && Current != null)
                position = ClampToRect(position, halfView, Current.PaddedRect);

            if (_hasMapRect)
                position = ClampToRect(position, halfView, _mapRect);
            return position;
        }

        private static Vector2 ClampToRect(Vector2 position, Vector2 halfView, Rect rect)
        {
            position.x = ClampAxis(position.x, halfView.x, rect.xMin, rect.xMax);
            position.y = ClampAxis(position.y, halfView.y, rect.yMin, rect.yMax);
            return position;
        }

        // Vùng nhìn lớn hơn vùng giới hạn thì đặt giữa.
        private static float ClampAxis(float value, float halfView, float min, float max)
        {
            if (max - min <= halfView * 2f)
                return (min + max) * 0.5f;
            return Mathf.Clamp(value, min + halfView, max - halfView);
        }

        // Cùng công thức với PixelPerfectCamera khi Crop Frame None.
        private float OrthoSizeFor(int zoom) => Screen.height * 0.5f / (zoom * _pixelPerfect.assetsPPU);

        private float OrthoSizeToZoom(float orthoSize) => Screen.height * 0.5f / (orthoSize * _pixelPerfect.assetsPPU);

        private static int GetZoom(Vector2Int refResolution)
        {
            int horizontal = Screen.width / Mathf.Max(1, refResolution.x);
            int vertical = Screen.height / Mathf.Max(1, refResolution.y);
            return Mathf.Max(1, Mathf.Min(horizontal, vertical));
        }

        private bool TryGetMapRect(out Rect rect)
        {
            rect = default;
            if (_mapTilemap == null)
            {
                Debug.LogWarning("CameraController: chưa gán Map Tilemap, camera không bị giới hạn.", this);
                return false;
            }

            _mapTilemap.CompressBounds();
            return TryReadMapRect(out rect);
        }

        private bool TryReadMapRect(out Rect rect)
        {
            rect = default;
            BoundsInt cells = _mapTilemap.cellBounds;
            if (cells.size.x <= 0 || cells.size.y <= 0)
                return false;

            Vector3 min = _mapTilemap.CellToWorld(cells.min);
            Vector3 max = _mapTilemap.CellToWorld(cells.max);
            rect = _mapPadding.Apply(Rect.MinMaxRect(min.x, min.y, max.x, max.y));
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (_mapTilemap == null || !TryReadMapRect(out Rect rect))
                return;

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(rect.center, rect.size);
        }
    }
}
