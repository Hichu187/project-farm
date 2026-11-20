using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Danh sách mọi cần câu (duyệt 13/11/2026) – dùng ở bảng Cần (vùng câu) và tab Cửa hàng → Đồ câu (nông trại).
    // Đang có: Dùng / Đang dùng; chưa đầy độ bền: thêm Sửa (giá theo phần thiếu, Đợt 2 Phần 1); ≤ 10%: "Sắp hỏng".
    // Chưa có: Mua (giá) – mờ khi thiếu xu; chưa đủ cấp: dòng mờ, nút "Cấp N".
    // Tự vẽ lại khi bật (bảng mở) và khi tiến trình đổi trong lúc đang bật.
    public sealed class RodListView : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private RectTransform _content;
        [SerializeField] private FishingItemRow _rowTemplate;
        [Tooltip("Icon khi cần chưa có icon riêng trong data.")]
        [SerializeField] private Sprite _fallbackIcon;
        [SerializeField] private Toast _toast;

        private readonly List<FishingItemRow> _rows = new List<FishingItemRow>();
        private bool _subscribed;

        private void OnEnable()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_game == null || _game.Farm == null)
                return;
            _game.Farm.Changed += Refresh;
            _subscribed = true;
            Refresh();
        }

        private void OnDisable()
        {
            if (_subscribed && _game != null && _game.Farm != null)
                _game.Farm.Changed -= Refresh;
            _subscribed = false;
        }

        public void Refresh()
        {
            if (_rowTemplate.gameObject.activeSelf)
                _rowTemplate.gameObject.SetActive(false);
            Farm farm = _game.Farm;
            FishingState fishing = farm.State.Fishing;
            FishingDefinition data = _game.Definition.Fishing;
            long coins = farm.State.Coins;
            int shown = 0;

            foreach (RodDef def in farm.FishingDefinition.Rods)
            {
                RodDefinition art = null;
                if (data != null)
                    data.TryGetRod(def.Id, out art);
                string name = art != null ? art.DisplayName : def.Id;
                Sprite icon = art != null && art.Icon != null ? art.Icon : _fallbackIcon;
                string id = def.Id;
                FishingItemRow row = Row(shown++);

                if (fishing.TryGetRod(def.Id, out RodState rod))
                {
                    bool equipped = fishing.EquippedRodId == def.Id;
                    string state = rod.IsBroken ? " · Hỏng" : farm.IsRodLow(def.Id) ? " · Sắp hỏng" : equipped ? " · Đang dùng" : "";
                    row.Set(icon, name, $"Rank {def.Rank} · Độ bền {rod.Durability}/{farm.GetRodMaxDurability(def.Id)}{state}",
                        equipped && !rod.IsBroken ? FishingItemRow.Look.Selected : FishingItemRow.Look.Normal);

                    if (equipped)
                        row.SetButton(0, "Đang dùng", null);
                    else
                        row.SetButton(0, "Dùng", () => farm.EquipRod(id));
                    long repairPrice = farm.GetRodRepairPrice(def.Id);
                    if (repairPrice > 0)
                        row.SetButton(1, $"Sửa {UiFormat.Coins(repairPrice)}", () => Repair(id), coins >= repairPrice);
                }
                else if (farm.State.Level < def.UnlockLevel)
                {
                    row.Set(icon, name, $"Rank {def.Rank} · Độ bền {def.MaxDurability} · Mở ở cấp {def.UnlockLevel}", FishingItemRow.Look.Locked);
                    row.SetButton(0, $"Cấp {def.UnlockLevel}", null);
                }
                else
                {
                    row.Set(icon, name, $"Rank {def.Rank} · Độ bền {def.MaxDurability}", FishingItemRow.Look.Normal);
                    row.SetButton(0, $"Mua {UiFormat.Coins(def.Price)}", () => Buy(id), coins >= def.Price);
                }
            }
            for (int i = shown; i < _rows.Count; i++)
                _rows[i].gameObject.SetActive(false);
        }

        private void Buy(string rodId)
        {
            FarmResult result = _game.Farm.BuyRod(rodId);
            if (result == FarmResult.Ok)
                ShowToast("Đã mua cần");
            else
                ShowToast(UiFormat.ShopBlocked(result));
        }

        private void Repair(string rodId)
        {
            FarmResult result = _game.Farm.RepairRod(rodId);
            if (result == FarmResult.Ok)
                ShowToast("Đã sửa cần");
            else
                ShowToast(UiFormat.ShopBlocked(result));
        }

        private FishingItemRow Row(int index)
        {
            while (_rows.Count <= index)
                _rows.Add(Instantiate(_rowTemplate, _content));
            FishingItemRow row = _rows[index];
            row.gameObject.SetActive(true);
            return row;
        }

        private void ShowToast(string text)
        {
            if (_toast != null && !string.IsNullOrEmpty(text))
                _toast.Show(text);
        }
    }
}
