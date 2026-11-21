using System;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.View.UI
{
    // Kho: chỉ hiện nông sản đang có, bán 1 hoặc bán hết.
    // Đợt 2 Phần 9 (giá chợ): giá hôm nay + % so với hôm qua, nhãn "Hút hàng" / "Rớt giá" sau tên; chạm icon → biểu đồ
    // 7 ngày; "Bán hết" giữ lại phần đang cần cho đơn hàng (bán 1 vẫn bán được).
    public sealed class StoragePanel : CropListPanel
    {
        [SerializeField] private GameObject _emptyLabel;
        [SerializeField] private PriceChartPopup _priceChart;

        private int _visibleRows;

        public event Action<string> Message;

        protected override void SetupRow(CropRow row, CropDefinition crop, CropDef def)
        {
            row.SetCrop(crop.ProduceIcon, crop.DisplayName);
            row.SetLabels("Bán 1", "Bán hết");
            row.Primary.onClick.AddListener(() => Sell(def, 1, 0));
            row.Secondary.onClick.AddListener(() => SellAll(def));
            if (row.IconButton != null && _priceChart != null)
                row.IconButton.onClick.AddListener(() => _priceChart.Show(crop));
        }

        private void SellAll(CropDef def)
        {
            int amount = Farm.GetSellableCount(def.Id);
            int kept = Math.Min(Farm.State.GetItems(def.Id), Farm.GetReservedForOrders(def.Id));
            if (amount <= 0)
            {
                if (kept > 0)
                    Message?.Invoke(UiFormat.KeptForOrders(kept));
                return;
            }
            Sell(def, amount, kept);
        }

        private void Sell(CropDef def, int amount, int kept)
        {
            // Giá trước khi bán (cùng lệnh Sell dùng).
            long earned = (long)Farm.GetSellPrice(def.Id) * amount;
            if (Farm.Sell(def.Id, amount) == FarmResult.Ok)
                Message?.Invoke(UiFormat.SoldKeeping(earned, kept));
        }

        protected override void RefreshRow(CropRow row, CropDefinition crop, CropDef def)
        {
            int items = Farm.State.GetItems(def.Id);
            row.gameObject.SetActive(items > 0);
            if (items <= 0)
                return;
            _visibleRows++;
            row.SetName(crop.DisplayName + UiFormat.MarketTag(def.Id == Farm.TodayHotCrop, def.Id == Farm.TodayColdCrop));
            row.SetInfo(UiFormat.StorageInfo(items, Farm.GetSellPrice(def.Id), Farm.GetPriceChangePercent(def.Id)));
        }

        protected override void Refresh()
        {
            _visibleRows = 0;
            base.Refresh();
            _emptyLabel.SetActive(_visibleRows == 0);
        }
    }
}
