using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.View.UI
{
    // Cửa hàng: mua hạt. Nút mờ khi không đủ xu.
    public sealed class ShopPanel : CropListPanel
    {
        private const int BulkAmount = 10;

        protected override void SetupRow(CropRow row, CropDefinition crop, CropDef def)
        {
            row.SetCrop(crop.SeedSprite, crop.DisplayName);
            row.SetLabels("Mua 1", $"Mua {BulkAmount}");
            row.Primary.onClick.AddListener(() => Farm.BuySeeds(def.Id, 1));
            row.Secondary.onClick.AddListener(() => Farm.BuySeeds(def.Id, BulkAmount));
        }

        protected override void RefreshRow(CropRow row, CropDefinition crop, CropDef def)
        {
            if (!Farm.IsCropUnlocked(def.Id))
            {
                row.SetInfo($"{UiFormat.GrowTime(def.GrowSeconds)} · Mở ở cấp {def.UnlockLevel}");
                row.Primary.interactable = false;
                row.Secondary.interactable = false;
                return;
            }

            long coins = Farm.State.Coins;
            row.SetInfo($"{UiFormat.GrowTime(def.GrowSeconds)} · {UiFormat.Coins(def.BuyPrice)} xu · có {Farm.State.GetSeeds(def.Id)} hạt");
            row.Primary.interactable = coins >= def.BuyPrice;
            row.Secondary.interactable = coins >= (long)def.BuyPrice * BulkAmount;
        }
    }
}
