using System;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Chọn hạt. Mở từ một ô trống thì gieo luôn vào ô đó; mở từ nút Đổi thì chỉ đổi hạt đang chọn.
    public sealed class SeedPanel : CropListPanel
    {
        [SerializeField] private FarmInteraction _interaction;

        private bool _hasPendingPlot;
        private PlotId _pendingPlot;

        public event Action BuyRequested;

        public void OpenForPlot(PlotId plot)
        {
            _hasPendingPlot = true;
            _pendingPlot = plot;
            Open();
        }

        public void OpenForChange()
        {
            _hasPendingPlot = false;
            Open();
        }

        protected override void SetupRow(CropRow row, CropDefinition crop, CropDef def)
        {
            row.SetCrop(crop.SeedSprite, crop.DisplayName);
            row.SetLabels("Chọn", "Mua");
            row.Primary.onClick.AddListener(() => Select(crop));
            row.Secondary.onClick.AddListener(() =>
            {
                Close();
                BuyRequested?.Invoke();
            });
        }

        protected override void RefreshRow(CropRow row, CropDefinition crop, CropDef def)
        {
            int seeds = Farm.State.GetSeeds(def.Id);
            row.SetInfo($"x{seeds} · {UiFormat.GrowTime(def.GrowSeconds)}");
            row.Primary.interactable = seeds > 0;
            row.Secondary.gameObject.SetActive(seeds == 0);
        }

        private void Select(CropDefinition crop)
        {
            _interaction.SelectCrop(crop);
            if (_hasPendingPlot)
                _interaction.PlantSelected(_pendingPlot);
            _hasPendingPlot = false;
            Close();
        }
    }
}
