using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using VeQue.Logic;
using VeQue.View.Anim;
using VeQue.View.Data;

namespace VeQue.View.Map
{
    // Vẽ ô đất và cây theo Farm. Ô đất đổi theo sự kiện Changed; cây đổi thêm theo định kỳ vì lớn theo thời gian.
    // Hoạt ảnh tự viết, chỉ dùng sprite giai đoạn có sẵn: gieo nảy lên, lên giai đoạn nhún, chín nhún 1 pixel,
    // thu hoạch bật lên rồi mờ, tưới có giọt nước. Di chuyển làm tròn theo pixel; phóng/thu chỉ dùng thoáng qua.
    public sealed class FarmView : MonoBehaviour
    {
        private const int CropBaseOrder = 100;
        private const float SpritePixelsPerUnit = 16f;
        private const int GhostPoolSize = 8;
        private const int DropPoolSize = 36;
        private const int FlashPoolSize = 16;
        // Vẽ trên ô đất, dưới mọi cây (cây dùng CropBaseOrder - cell.y).
        private const int FlashSortingOrder = 50;

        private enum CropAnim
        {
            None,
            Plant,
            Stage,
        }

        private sealed class PlotVisual
        {
            public PlotId Id;
            public Vector3Int Cell;
            public SpriteRenderer Crop;
            public Vector3 BasePosition;
            public TileBase PaintedTile;

            // Trạng thái lần làm mới trước, để biết vừa gieo / lên giai đoạn / thu hoạch / tưới.
            public bool HadCrop;
            public string LastCropId;
            public int LastStage;
            public bool LastWatered;
            public bool Ripe;
            public float RipePhase;

            public CropAnim Anim;
            public float AnimStart;
            public bool Moved;
        }

        // Hiệu ứng tạm từ pool: cây vừa thu hoạch (ô đã trống, bản sao bật lên rồi mờ) hoặc giọt nước.
        // Start < 0 = rảnh; Start > now = chờ tới lượt.
        private sealed class Effect
        {
            public SpriteRenderer Renderer;
            public Vector3 BasePosition;
            public float Start = -1f;
        }

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private ZoneAnchor[] _zones = System.Array.Empty<ZoneAnchor>();
        [SerializeField] private Tilemap _plots;
        [SerializeField] private float _refreshInterval = 0.5f;
        [Tooltip("Nâng gốc cây lên khỏi cạnh dưới ô, tính bằng pixel của sprite.")]
        [SerializeField] private int _cropOffsetPixels = 5;

        [Header("Tile theo trạng thái (để trống thì giữ tile đã vẽ)")]
        [SerializeField] private TileBase _lockedTile;
        [SerializeField] private TileBase _nextUnlockTile;
        [SerializeField] private TileBase _openTile;
        [Tooltip("Đất ướt, dùng từ lúc tưới tới khi thu hoạch.")]
        [SerializeField] private TileBase _wateredTile;

        [Header("Màu theo trạng thái")]
        [SerializeField] private Color _lockedColor = new Color(0.45f, 0.45f, 0.45f);
        [SerializeField] private Color _nextUnlockColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color _openColor = Color.white;
        [SerializeField] private Color _ripeBlinkColor = new Color(1f, 1f, 0.55f);

        [Header("Gió")]
        [Tooltip("Material dùng shader VeQue/CropSway để cây đung đưa. Để trống = không có gió (material sprite mặc định).")]
        [SerializeField] private Material _cropMaterial;

        [Header("Hoạt ảnh: gieo")]
        [SerializeField] private bool _animatePlant = true;
        [SerializeField] private float _plantDuration = 0.25f;
        [Tooltip("Độ lố khi nảy lên (1,7 ≈ lố 10%).")]
        [SerializeField] private float _plantOvershoot = Ease.DefaultOvershoot;

        [Header("Hoạt ảnh: lên giai đoạn")]
        [SerializeField] private bool _animateStage = true;
        [SerializeField] private float _stageDuration = 0.3f;
        [Tooltip("Mức ép dẹt / vươn cao (0,15 = 15%).")]
        [SerializeField] private float _stageSquash = 0.15f;

        [Header("Hoạt ảnh: chín")]
        [Tooltip("Nhún lên xuống theo pixel. Tắt thì dùng nhấp nháy màu như cũ.")]
        [SerializeField] private bool _ripeHop = true;
        [SerializeField] private float _ripeHopPeriod = 1.5f;
        [Tooltip("Thời gian nhấc lên trong mỗi nhịp.")]
        [SerializeField] private float _ripeHopUpTime = 0.25f;
        [SerializeField] private int _ripeHopPixels = 1;

        [Header("Hoạt ảnh: thu hoạch")]
        [SerializeField] private bool _animateHarvest = true;
        [SerializeField] private float _harvestDuration = 0.35f;
        [SerializeField] private int _harvestRisePixels = 4;

        [Header("Hoạt ảnh: tưới")]
        [SerializeField] private bool _animateWater = true;
        [SerializeField] private int _dropsPerWater = 6;
        [SerializeField] private float _dropDuration = 0.4f;
        [Tooltip("Giọt bắt đầu cao hơn mặt đất bao nhiêu pixel và rơi xuống mặt đất.")]
        [SerializeField] private int _dropFallPixels = 10;
        [SerializeField] private Color _dropColor = new Color(0.45f, 0.70f, 1f);

        [Header("Hoạt ảnh: ô vừa thao tác khi kéo")]
        [SerializeField] private bool _flashOnGesture = true;
        [SerializeField] private float _flashDuration = 0.25f;
        [SerializeField] private Color _flashColor = new Color(1f, 1f, 0.75f, 0.45f);

        private readonly List<PlotVisual> _visuals = new List<PlotVisual>();
        private readonly List<Effect> _ghosts = new List<Effect>();
        private readonly List<Effect> _drops = new List<Effect>();
        private readonly List<Effect> _flashes = new List<Effect>();
        private Farm _farm;
        private bool _blinkOn;
        private bool _initialized;
        private Sprite _pixelSprite;

        public bool TryGetPlotAt(Vector2 world, out PlotId id)
        {
            Vector3Int cell = _plots.WorldToCell(world);
            for (int i = 0; i < _zones.Length; i++)
            {
                if (_zones[i].Zone.TryGetPlotId(cell, out id))
                    return true;
            }

            id = default;
            return false;
        }

        // Tâm ô đất (toạ độ world); false nếu ô không thuộc khu nào đang vẽ.
        public bool TryGetPlotCenter(PlotId id, out Vector3 center)
        {
            for (int i = 0; i < _visuals.Count; i++)
            {
                if (_visuals[i].Id != id)
                    continue;
                center = _plots.GetCellCenterWorld(_visuals[i].Cell);
                return true;
            }

            center = default;
            return false;
        }

        // Ô sáng lên thoáng qua: báo ô vừa được gieo / tưới / thu hoạch khi kéo qua.
        public void Flash(PlotId id)
        {
            if (!_flashOnGesture || !TryGetPlotCenter(id, out Vector3 center))
                return;

            Effect flash = FindFree(_flashes);
            flash.Start = Time.unscaledTime;
            flash.BasePosition = center;
            flash.Renderer.transform.position = center;
            flash.Renderer.color = _flashColor;
            flash.Renderer.gameObject.SetActive(true);
        }

        // Điểm giữa cạnh trên của sprite cây (toạ độ world); false nếu ô không có cây.
        public bool TryGetCropTop(PlotId id, out Vector3 top)
        {
            for (int i = 0; i < _visuals.Count; i++)
            {
                PlotVisual visual = _visuals[i];
                if (visual.Id != id)
                    continue;

                if (visual.Crop.sprite == null)
                    break;

                top = visual.BasePosition;
                top.y += visual.Crop.sprite.bounds.max.y;
                return true;
            }

            top = default;
            return false;
        }

        private void Start()
        {
            _farm = _game.Farm;

            for (int i = 0; i < _zones.Length; i++)
                CreateVisuals(_zones[i].Zone);
            CreatePools();

            _farm.Changed += RefreshAll;
            RefreshAll();
            _initialized = true;
            InvokeRepeating(nameof(RefreshPeriodic), _refreshInterval, _refreshInterval);
        }

        private void OnDestroy()
        {
            if (_farm != null)
                _farm.Changed -= RefreshAll;
            if (_pixelSprite != null)
            {
                Destroy(_pixelSprite.texture);
                Destroy(_pixelSprite);
            }
        }

        private void CreateVisuals(ZoneDefinition zone)
        {
            if (!_farm.Definition.TryGetZoneIndex(zone.Id, out int zoneIndex))
            {
                Debug.LogWarning($"FarmView: khu '{zone.Id}' không có trong FarmDefinition.", this);
                return;
            }

            ZoneDef def = _farm.Definition.Zones[zoneIndex];
            for (int i = 0; i < def.PlotCount; i++)
            {
                PlotId id = def.PlotIdAt(i);
                Vector3Int cell = zone.GetCell(id);

                var crop = new GameObject($"Crop {id}").AddComponent<SpriteRenderer>();
                crop.transform.SetParent(transform, false);
                // Sprite cây có pivot Bottom nên lấy giữa cạnh dưới của ô rồi nâng lên để hạt nằm giữa ô.
                Vector3 position = _plots.CellToWorld(cell) + new Vector3(0.5f, _cropOffsetPixels / SpritePixelsPerUnit, 0f);
                crop.transform.position = position;
                // Ô thấp hơn trên map vẽ đè lên ô phía trên.
                crop.sortingOrder = CropBaseOrder - cell.y;
                if (_cropMaterial != null)
                    crop.sharedMaterial = _cropMaterial;

                _visuals.Add(new PlotVisual
                {
                    Id = id,
                    Cell = cell,
                    Crop = crop,
                    BasePosition = position,
                    PaintedTile = _plots.GetTile(cell),
                    // Lệch nhịp nhún giữa các ô để cả luống không nhún cùng lúc.
                    RipePhase = Mathf.Repeat((cell.x * 0.37f + cell.y * 0.61f), 1f),
                });
            }
        }

        // Tạo sẵn để không cấp phát khi đang chơi.
        private void CreatePools()
        {
            for (int i = 0; i < GhostPoolSize; i++)
            {
                SpriteRenderer renderer = CreatePooledRenderer($"HarvestGhost {i}");
                _ghosts.Add(new Effect { Renderer = renderer });
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _pixelSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), SpritePixelsPerUnit);
            for (int i = 0; i < DropPoolSize; i++)
            {
                SpriteRenderer renderer = CreatePooledRenderer($"WaterDrop {i}");
                renderer.sprite = _pixelSprite;
                _drops.Add(new Effect { Renderer = renderer });
            }
            for (int i = 0; i < FlashPoolSize; i++)
            {
                SpriteRenderer renderer = CreatePooledRenderer($"PlotFlash {i}");
                renderer.sprite = _pixelSprite;
                renderer.sortingOrder = FlashSortingOrder;
                // Sprite 1 pixel → phóng 16 lần cho phủ đúng 1 ô.
                renderer.transform.localScale = Vector3.one * SpritePixelsPerUnit;
                _flashes.Add(new Effect { Renderer = renderer });
            }
        }

        private SpriteRenderer CreatePooledRenderer(string name)
        {
            var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(transform, false);
            renderer.gameObject.SetActive(false);
            return renderer;
        }

        private void RefreshAll()
        {
            for (int i = 0; i < _visuals.Count; i++)
                RefreshPlot(_visuals[i]);
            RefreshCrops();
        }

        private void RefreshPeriodic()
        {
            _blinkOn = !_blinkOn;
            RefreshCrops();
        }

        private void RefreshPlot(PlotVisual visual)
        {
            if (!_farm.TryGetPlotState(visual.Id, out PlotState state))
                return;

            TileBase tile = _openTile;
            Color color = _openColor;
            if (state.Status == PlotStatus.Locked)
            {
                bool isNext = _farm.TryGetNextUnlock(visual.Id.ZoneId, out PlotId next, out _) && next == visual.Id;
                tile = isNext ? _nextUnlockTile : _lockedTile;
                color = isNext ? _nextUnlockColor : _lockedColor;
            }
            else if (state.Status == PlotStatus.Growing && state.Watered && _wateredTile != null)
            {
                tile = _wateredTile;
            }

            // Không gán tile riêng thì trả về tile đã vẽ, để đất ướt không còn lại sau khi thu hoạch.
            if (tile == null)
                tile = visual.PaintedTile;
            if (tile != null)
                _plots.SetTile(visual.Cell, tile);
            _plots.SetTileFlags(visual.Cell, TileFlags.None);
            _plots.SetColor(visual.Cell, color);
        }

        private void RefreshCrops()
        {
            float now = Time.unscaledTime;
            for (int i = 0; i < _visuals.Count; i++)
            {
                PlotVisual visual = _visuals[i];
                bool hasCrop = _farm.TryGetPlotState(visual.Id, out PlotState state)
                    && state.Status == PlotStatus.Growing
                    && _game.TryGetCrop(state.CropId, out _);

                if (!hasCrop)
                {
                    if (visual.HadCrop && _initialized && _animateHarvest)
                        SpawnGhost(visual, now);
                    visual.Crop.sprite = null;
                    visual.HadCrop = false;
                    visual.LastCropId = null;
                    visual.Ripe = false;
                    visual.Anim = CropAnim.None;
                    continue;
                }

                _game.TryGetCrop(state.CropId, out CropDefinition crop);
                int stage = _farm.GetStage(visual.Id);
                bool sameCrop = visual.HadCrop && visual.LastCropId == state.CropId;
                if (_initialized)
                {
                    if (!sameCrop && _animatePlant)
                        StartAnim(visual, CropAnim.Plant, now);
                    else if (sameCrop && stage > visual.LastStage && _animateStage)
                        StartAnim(visual, CropAnim.Stage, now);

                    if (state.Watered && (!sameCrop || !visual.LastWatered) && _animateWater)
                        SpawnDrops(visual, now);
                }

                visual.Crop.sprite = crop.GetStageSprite(stage);
                visual.Ripe = _farm.IsRipe(visual.Id);
                visual.Crop.color = !_ripeHop && _blinkOn && visual.Ripe ? _ripeBlinkColor : Color.white;
                visual.HadCrop = true;
                visual.LastCropId = state.CropId;
                visual.LastStage = stage;
                visual.LastWatered = state.Watered;
            }
        }

        private static void StartAnim(PlotVisual visual, CropAnim anim, float now)
        {
            visual.Anim = anim;
            visual.AnimStart = now;
        }

        private void SpawnGhost(PlotVisual visual, float now)
        {
            if (visual.Crop.sprite == null)
                return;

            Effect ghost = FindFree(_ghosts);
            ghost.Start = now;
            ghost.BasePosition = visual.BasePosition;
            ghost.Renderer.sprite = visual.Crop.sprite;
            ghost.Renderer.sortingOrder = visual.Crop.sortingOrder;
            ghost.Renderer.color = Color.white;
            ghost.Renderer.transform.position = visual.BasePosition;
            ghost.Renderer.gameObject.SetActive(true);
        }

        // Giọt ở các cột pixel khác nhau trên ô, xuất phát lệch giờ một chút.
        private void SpawnDrops(PlotVisual visual, float now)
        {
            Vector3 ground = _plots.CellToWorld(visual.Cell) + new Vector3(0f, 0.5f, 0f);
            for (int i = 0; i < _dropsPerWater; i++)
            {
                Effect drop = FindFree(_drops);
                int column = 2 + Random.Range(0, 12);
                drop.BasePosition = ground + new Vector3((column + 0.5f) / SpritePixelsPerUnit, 0f, 0f);
                drop.Start = now + Random.Range(0f, _dropDuration * 0.4f);
                drop.Renderer.sortingOrder = visual.Crop.sortingOrder + 1;
                drop.Renderer.color = _dropColor;
                drop.Renderer.gameObject.SetActive(false);
            }
        }

        // Lấy phần tử rảnh; hết chỗ thì dùng lại cái bắt đầu sớm nhất.
        private static Effect FindFree(List<Effect> pool)
        {
            Effect oldest = pool[0];
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].Start < 0f)
                    return pool[i];
                if (pool[i].Start < oldest.Start)
                    oldest = pool[i];
            }
            return oldest;
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            for (int i = 0; i < _visuals.Count; i++)
                AnimateCrop(_visuals[i], now);
            for (int i = 0; i < _ghosts.Count; i++)
                AnimateGhost(_ghosts[i], now);
            for (int i = 0; i < _drops.Count; i++)
                AnimateDrop(_drops[i], now);
            for (int i = 0; i < _flashes.Count; i++)
                AnimateFlash(_flashes[i], now);
        }

        private void AnimateFlash(Effect flash, float now)
        {
            if (flash.Start < 0f)
                return;

            float t = (now - flash.Start) / _flashDuration;
            if (t >= 1f)
            {
                flash.Start = -1f;
                flash.Renderer.gameObject.SetActive(false);
                return;
            }

            Color color = _flashColor;
            color.a *= 1f - Ease.OutQuad(t);
            flash.Renderer.color = color;
        }

        private void AnimateCrop(PlotVisual visual, float now)
        {
            Vector3 scale = Vector3.one;
            float offsetPixels = 0f;

            if (visual.Anim == CropAnim.Plant)
            {
                float t = (now - visual.AnimStart) / _plantDuration;
                if (t >= 1f)
                    visual.Anim = CropAnim.None;
                else
                    scale = Vector3.one * Ease.OutBack(t, _plantOvershoot);
            }
            else if (visual.Anim == CropAnim.Stage)
            {
                float t = (now - visual.AnimStart) / _stageDuration;
                if (t >= 1f)
                {
                    visual.Anim = CropAnim.None;
                }
                else
                {
                    float wave = Ease.DampedWave(t) * _stageSquash;
                    scale = new Vector3(1f - wave * 0.5f, 1f + wave, 1f);
                }
            }

            if (visual.Anim == CropAnim.None && visual.Ripe && _ripeHop && _ripeHopPeriod > 0f)
            {
                float phase = Mathf.Repeat(now / _ripeHopPeriod + visual.RipePhase, 1f);
                if (phase * _ripeHopPeriod < _ripeHopUpTime)
                    offsetPixels = _ripeHopPixels;
            }

            bool moved = visual.Anim != CropAnim.None || offsetPixels != 0f;
            if (!moved && !visual.Moved)
                return;

            Transform t2 = visual.Crop.transform;
            t2.position = visual.BasePosition + new Vector3(0f, offsetPixels / SpritePixelsPerUnit, 0f);
            t2.localScale = scale;
            visual.Moved = moved;
        }

        private void AnimateGhost(Effect ghost, float now)
        {
            if (ghost.Start < 0f)
                return;

            float t = (now - ghost.Start) / _harvestDuration;
            if (t >= 1f)
            {
                ghost.Start = -1f;
                ghost.Renderer.gameObject.SetActive(false);
                return;
            }

            float rise = Ease.SnapToPixel(Ease.OutQuad(t) * _harvestRisePixels / SpritePixelsPerUnit, SpritePixelsPerUnit);
            ghost.Renderer.transform.position = ghost.BasePosition + new Vector3(0f, rise, 0f);
            Color color = Color.white;
            color.a = 1f - Ease.InQuad(t);
            ghost.Renderer.color = color;
        }

        private void AnimateDrop(Effect drop, float now)
        {
            if (drop.Start < 0f)
                return;

            float t = (now - drop.Start) / _dropDuration;
            if (t < 0f)
                return;
            if (t >= 1f)
            {
                drop.Start = -1f;
                drop.Renderer.gameObject.SetActive(false);
                return;
            }

            if (!drop.Renderer.gameObject.activeSelf)
                drop.Renderer.gameObject.SetActive(true);
            float height = Ease.SnapToPixel((1f - Ease.InQuad(t)) * _dropFallPixels / SpritePixelsPerUnit, SpritePixelsPerUnit);
            drop.Renderer.transform.position = drop.BasePosition + new Vector3(0f, height, 0f);
            Color color = _dropColor;
            color.a = t > 0.7f ? (1f - t) / 0.3f : 1f;
            drop.Renderer.color = color;
        }
    }
}
