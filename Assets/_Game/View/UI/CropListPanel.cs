using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Khung chung của Kho, Cửa hàng, bảng chọn hạt: mỗi cây đang bật một dòng, làm mới khi Farm đổi.
    public abstract class CropListPanel : MonoBehaviour
    {
        private sealed class Entry
        {
            public CropRow Row;
            public CropDefinition Crop;
            public CropDef Def;
        }

        [SerializeField] protected GameBootstrap _game;
        [SerializeField] private BottomSheet _sheet;
        [SerializeField] private CropRow _rowTemplate;
        [SerializeField] private Transform _content;

        private readonly List<Entry> _entries = new List<Entry>();

        protected Farm Farm => _game.Farm;

        public bool IsOpen => _sheet.IsOpen;

        public void Open()
        {
            Refresh();
            _sheet.Open();
        }

        public void Close()
        {
            _sheet.Close();
        }

        protected abstract void SetupRow(CropRow row, CropDefinition crop, CropDef def);
        protected abstract void RefreshRow(CropRow row, CropDefinition crop, CropDef def);

        protected virtual void Refresh()
        {
            for (int i = 0; i < _entries.Count; i++)
                RefreshRow(_entries[i].Row, _entries[i].Crop, _entries[i].Def);
        }

        private void Start()
        {
            foreach (CropDefinition crop in _game.Definition.Crops)
            {
                if (crop == null || !crop.Enabled || !Farm.Definition.TryGetCrop(crop.Id, out CropDef def))
                    continue;

                CropRow row = Instantiate(_rowTemplate, _content);
                row.gameObject.name = $"Row {crop.Id}";
                row.gameObject.SetActive(true);
                SetupRow(row, crop, def);
                _entries.Add(new Entry { Row = row, Crop = crop, Def = def });
            }

            Farm.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= Refresh;
        }
    }
}
