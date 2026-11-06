using System;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.View.UI
{
    // Kho: chỉ hiện nông sản đang có, bán 1 hoặc bán hết.
    public sealed class StoragePanel : CropListPanel
    {
        [SerializeField] private GameObject _emptyLabel;

        private int _visibleRows;

        public event Action<string> Message;

        protected override void SetupRow(CropRow row, CropDefinition crop, CropDef def)
        {
            row.SetCrop(crop.ProduceIcon, crop.DisplayName);
            row.SetLabels("Bán 1", "Bán hết");
            row.Primary.onClick.AddListener(() => Sell(def, 1));
            row.Secondary.onClick.AddListener(() => Sell(def, Farm.State.GetItems(def.Id)));
        }

        private void Sell(CropDef def, int amount)
        {
            if (Farm.Sell(def.Id, amount) == FarmResult.Ok)
                Message?.Invoke($"+{UiFormat.Coins((long)def.SellPrice * amount)} xu");
        }

        protected override void RefreshRow(CropRow row, CropDefinition crop, CropDef def)
        {
            int items = Farm.State.GetItems(def.Id);
            row.gameObject.SetActive(items > 0);
            row.SetInfo($"x{items} · {UiFormat.Coins(def.SellPrice)} xu/cái");
            if (items > 0)
                _visibleRows++;
        }

        protected override void Refresh()
        {
            _visibleRows = 0;
            base.Refresh();
            _emptyLabel.SetActive(_visibleRows == 0);
        }
    }
}
