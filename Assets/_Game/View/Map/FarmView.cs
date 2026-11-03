using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.View.Map
{
    // Vẽ ô đất và cây theo Farm. Ô đất đổi theo sự kiện Changed; cây đổi thêm theo định kỳ vì lớn theo thời gian.
    public sealed class FarmView : MonoBehaviour
    {
        private const int CropBaseOrder = 100;
        private const float SpritePixelsPerUnit = 16f;

        private sealed class PlotVisual
        {
            public PlotId Id;
            public Vector3Int Cell;
            public SpriteRenderer Crop;
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

        [Header("Màu theo trạng thái")]
        [SerializeField] private Color _lockedColor = new Color(0.45f, 0.45f, 0.45f);
        [SerializeField] private Color _nextUnlockColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color _openColor = Color.white;
        [SerializeField] private Color _ripeBlinkColor = new Color(1f, 1f, 0.55f);

        private readonly List<PlotVisual> _visuals = new List<PlotVisual>();
        private Farm _farm;
        private bool _blinkOn;

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

                top = visual.Crop.transform.position;
                top.y = visual.Crop.bounds.max.y;
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

            _farm.Changed += RefreshAll;
            RefreshAll();
            InvokeRepeating(nameof(RefreshPeriodic), _refreshInterval, _refreshInterval);
        }

        private void OnDestroy()
        {
            if (_farm != null)
                _farm.Changed -= RefreshAll;
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
                crop.transform.position = _plots.CellToWorld(cell)
                    + new Vector3(0.5f, _cropOffsetPixels / SpritePixelsPerUnit, 0f);
                // Ô thấp hơn trên map vẽ đè lên ô phía trên.
                crop.sortingOrder = CropBaseOrder - cell.y;

                _visuals.Add(new PlotVisual { Id = id, Cell = cell, Crop = crop });
            }
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

            if (tile != null)
                _plots.SetTile(visual.Cell, tile);
            _plots.SetTileFlags(visual.Cell, TileFlags.None);
            _plots.SetColor(visual.Cell, color);
        }

        private void RefreshCrops()
        {
            for (int i = 0; i < _visuals.Count; i++)
            {
                PlotVisual visual = _visuals[i];
                if (!_farm.TryGetPlotState(visual.Id, out PlotState state)
                    || state.Status != PlotStatus.Growing
                    || !_game.TryGetCrop(state.CropId, out CropDefinition crop))
                {
                    visual.Crop.sprite = null;
                    continue;
                }

                visual.Crop.sprite = crop.GetStageSprite(_farm.GetStage(visual.Id), state.Watered);
                visual.Crop.color = _blinkOn && _farm.IsRipe(visual.Id) ? _ripeBlinkColor : Color.white;
            }
        }
    }
}
