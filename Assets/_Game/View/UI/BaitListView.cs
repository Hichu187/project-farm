using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Fishing;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Danh sách mọi mồi (duyệt 13/11/2026) – dùng ở bảng Mồi (vùng câu) và tab Cửa hàng → Đồ câu (nông trại).
    // Nút: Dùng / Đang dùng (khi đang có), Mua 1, Mua 10 (bán ở Cửa hàng, đủ cấp; mờ khi thiếu xu).
    // Chưa đủ cấp (và chưa có con nào): dòng mờ, nút "Cấp N". Giun không bán: ghi "đào được khi thu hoạch".
    public sealed class BaitListView : MonoBehaviour
    {
        private const int BulkAmount = 10;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FishingController _controller;
        [SerializeField] private RectTransform _content;
        [SerializeField] private FishingItemRow _rowTemplate;
        [SerializeField] private Sprite _fallbackIcon;
        [SerializeField] private Toast _toast;

        private readonly List<FishingItemRow> _rows = new List<FishingItemRow>();
        private bool _subscribed;

        private void OnEnable()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_controller == null)
                _controller = FindFirstObjectByType<FishingController>();
            if (_game == null || _game.Farm == null)
                return;
            _game.Farm.Changed += Refresh;
            if (_controller != null)
                _controller.Changed += Refresh;
            _subscribed = true;
            Refresh();
        }

        private void OnDisable()
        {
            if (!_subscribed)
                return;
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= Refresh;
            if (_controller != null)
                _controller.Changed -= Refresh;
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

            foreach (BaitDef bait in farm.FishingDefinition.Baits)
            {
                BaitDefinition art = null;
                if (data != null)
                    data.TryGetBait(bait.Id, out art);
                string name = art != null ? art.DisplayName : bait.Id;
                Sprite icon = art != null && art.Icon != null ? art.Icon : _fallbackIcon;
                int count = fishing.GetBaits(bait.Id);
                string id = bait.Id;
                FishingItemRow row = Row(shown++);

                bool locked = bait.Buyable && farm.State.Level < bait.UnlockLevel;
                if (locked && count <= 0)
                {
                    row.Set(icon, name, $"Mở ở cấp {bait.UnlockLevel}", FishingItemRow.Look.Locked);
                    row.SetButton(0, $"Cấp {bait.UnlockLevel}", null);
                    continue;
                }

                bool selected = _controller != null && _controller.BaitId == bait.Id && count > 0;
                string effect = bait.RareBonusPercent > 0 ? $" · +{bait.RareBonusPercent}% cá hiếm" : "";
                string price = !bait.Buyable ? " · đào được khi thu hoạch" : locked ? "" : $" · {UiFormat.Coins(bait.Price)} xu/con";
                row.Set(icon, name, $"Còn {count}{effect}{price}", selected ? FishingItemRow.Look.Selected : FishingItemRow.Look.Normal);

                int button = 0;
                if (count > 0)
                    row.SetButton(button++, selected ? "Đang dùng" : "Dùng", selected || _controller == null ? null : (System.Action)(() => _controller.SelectBait(id)));
                if (bait.Buyable && !locked)
                {
                    row.SetButton(button++, "Mua 1", () => Buy(id, 1), coins >= bait.Price);
                    row.SetButton(button, $"Mua {BulkAmount}", () => Buy(id, BulkAmount), coins >= bait.Price * BulkAmount);
                }
            }
            for (int i = shown; i < _rows.Count; i++)
                _rows[i].gameObject.SetActive(false);
        }

        private void Buy(string baitId, int amount)
        {
            FarmResult result = _game.Farm.BuyBait(baitId, amount);
            if (result != FarmResult.Ok && _toast != null)
                _toast.Show(UiFormat.ShopBlocked(result));
        }

        private FishingItemRow Row(int index)
        {
            while (_rows.Count <= index)
                _rows.Add(Instantiate(_rowTemplate, _content));
            FishingItemRow row = _rows[index];
            row.gameObject.SetActive(true);
            return row;
        }
    }
}
