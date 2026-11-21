using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Fishing;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Giỏ cá (duyệt 13/11/2026), dùng chung cho bảng Giỏ ở vùng câu và tab Giỏ cá trong Kho (nông trại).
    // - Hàng trên: số cá / sức chứa, nút "Bán hết (tổng xu)" → hộp xác nhận.
    // - Khung chi tiết cá đang chọn: tên, độ hiếm, cân nặng, giá, khung giờ bắt; nút Bán (giá), Thả.
    // - Lưới ô vuông tới 25 ô, cuộn dọc: cá (nền theo độ hiếm), ô trống, ô khóa (ô kế tiếp hiện giá → xác nhận mở ô).
    // - Đợt 2 Phần 5: rác nằm sau cá (chiếm ô chung), chạm → bán cho cô đồng nát (không có Thả); nút "Tự bán rác: Bật/Tắt".
    // Tự vẽ lại khi bật và khi tiến trình đổi trong lúc đang bật.
    public sealed class BasketView : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private TMP_Text _capacity;
        [SerializeField] private Button _sellAll;
        [SerializeField] private TMP_Text _sellAllLabel;
        [Tooltip("Bật / tắt tự bán rác cho cô đồng nát (Đợt 2 Phần 5).")]
        [SerializeField] private Button _autoSell;
        [SerializeField] private TMP_Text _autoSellLabel;
        [SerializeField] private Color _junkColor = new Color(0.55f, 0.48f, 0.38f);
        [Tooltip("Ô theo thứ tự (đủ cho giỏ lớn nhất); ô thừa bị ẩn.")]
        [SerializeField] private FishCell[] _cells = Array.Empty<FishCell>();
        [Header("Chi tiết")]
        [SerializeField] private GameObject _detail;
        [SerializeField] private GameObject _hint;
        [SerializeField] private Image _detailIcon;
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private TMP_Text _detailInfo;
        [SerializeField] private Button _sell;
        [SerializeField] private TMP_Text _sellLabel;
        [SerializeField] private Button _release;
        [Header("Hộp xác nhận (mở ô / bán hết)")]
        [SerializeField] private GameObject _confirmRoot;
        [SerializeField] private TMP_Text _confirmText;
        [SerializeField] private Button _confirmOk;
        [SerializeField] private TMP_Text _confirmOkLabel;
        [SerializeField] private Button _confirmCancel;
        [SerializeField] private Toast _toast;
        [Tooltip("Màu độ hiếm: Thường, Khá, Hiếm, Quý, Huyền thoại.")]
        [SerializeField] private Color[] _rarityColors =
        {
            new Color(0.45f, 0.42f, 0.38f),
            new Color(0.25f, 0.55f, 0.20f),
            new Color(0.15f, 0.40f, 0.80f),
            new Color(0.55f, 0.25f, 0.75f),
            new Color(0.85f, 0.55f, 0.05f),
        };

        private int _selected = -1;
        private Action _pendingConfirm;
        private bool _listening;
        private bool _wired;

        private void Awake()
        {
            _release.onClick.AddListener(ReleaseSelected);
            _sell.onClick.AddListener(SellSelected);
            _sellAll.onClick.AddListener(AskSellAll);
            if (_autoSell != null)
                _autoSell.onClick.AddListener(ToggleAutoSell);
            _confirmOk.onClick.AddListener(ConfirmOk);
            _confirmCancel.onClick.AddListener(HideConfirm);
            _wired = true;
        }

        private void OnEnable()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_game == null || _game.Farm == null)
                return;
            _game.Farm.Changed += Rebuild;
            _listening = true;
            _selected = -1;
            HideConfirm();
            Rebuild();
        }

        private void OnDisable()
        {
            if (_listening && _game != null && _game.Farm != null)
                _game.Farm.Changed -= Rebuild;
            _listening = false;
        }

        private void Rebuild()
        {
            if (!_wired)
                return;
            Farm farm = _game.Farm;
            List<CaughtFish> basket = farm.State.Fishing.Basket;
            List<CaughtJunk> junk = farm.State.Fishing.JunkBasket;
            int used = farm.State.Fishing.BasketUsed;
            int capacity = farm.BasketCapacity;
            if (_selected >= used)
                _selected = -1;
            _capacity.text = $"Giỏ {used}/{capacity}";
            // "Bán hết" bỏ qua cá shiny (bán lẻ từng con), gồm cả rác.
            bool canSellAll = farm.GetSellAllCount() > 0;
            _sellAll.interactable = canSellAll;
            _sellAllLabel.text = canSellAll ? $"Bán hết {UiFormat.Coins(farm.GetBasketValue())}" : "Bán hết";
            if (_autoSellLabel != null)
                _autoSellLabel.text = UiFormat.AutoSellJunk(farm.State.Fishing.AutoSellJunk);

            int maxSlots = Mathf.Max(farm.FishingDefinition.BasketMaxSlots, used);
            string nextPrice = farm.CanUnlockBasketSlot ? $"{UiFormat.Coins(farm.NextBasketSlotPrice)} xu" : "";
            for (int i = 0; i < _cells.Length; i++)
            {
                FishCell cell = _cells[i];
                bool shown = i < maxSlots;
                cell.gameObject.SetActive(shown);
                if (!shown)
                    continue;
                if (i >= used)
                {
                    if (i < capacity)
                        cell.SetEmpty();
                    else
                        cell.SetLocked(i == capacity, nextPrice, AskUnlock);
                    continue;
                }
                if (i >= basket.Count)
                {
                    // Rác sau cá: nền màu rác, chữ = giá cô đồng nát trả.
                    CaughtJunk item = junk[i - basket.Count];
                    int junkIndex = i;
                    TryGetJunk(item.JunkId, out JunkDefinition junkArt);
                    cell.SetFish(junkArt != null ? junkArt.Icon : null, $"{UiFormat.Coins(farm.GetJunkPrice(item))} xu", _junkColor,
                        i == _selected, () => Select(junkIndex));
                    continue;
                }
                CaughtFish fish = basket[i];
                int index = i;
                TryGetFish(fish.FishId, out FishDefinition art, out FishDef def);
                cell.SetFish(art != null ? art.Icon : null, UiFormat.Weight(fish.Grams), RarityColor(def), i == _selected,
                    () => Select(index), Shiny(fish.Shiny), ShinySeed(fish));
            }
            ShowDetail(basket, junk);
        }

        private void ShowDetail(List<CaughtFish> basket, List<CaughtJunk> junk)
        {
            bool has = _selected >= 0 && _selected < basket.Count + junk.Count;
            _detail.SetActive(has);
            if (_hint != null)
                _hint.SetActive(!has);
            if (!has)
                return;

            if (_selected >= basket.Count)
            {
                ShowJunkDetail(junk[_selected - basket.Count]);
                return;
            }
            _release.gameObject.SetActive(true);
            CaughtFish fish = basket[_selected];
            TryGetFish(fish.FishId, out FishDefinition art, out FishDef def);
            _detailIcon.sprite = art != null ? art.Icon : null;
            _detailIcon.enabled = _detailIcon.sprite != null;
            ShinyPalette.Entry shiny = Shiny(fish.Shiny);
            ShinyLook.Apply(_detailIcon, shiny, ShinySeed(fish));
            _detailName.text = art != null ? art.DisplayName : fish.FishId;
            string rarity = def != null
                ? $"<color=#{ColorUtility.ToHtmlStringRGB(RarityColor(def))}>{UiFormat.RarityName(def.Rarity)}</color> · "
                : "";
            string shinyLine = shiny != null ? UiFormat.ShinyDetail(shiny.DisplayName) + "\n" : "";
            // 21/11/2026: bỏ dòng "Bắt lúc <khung giờ>" – giờ ra cá không hiển thị cho người chơi (vẫn lưu trong save).
            _detailInfo.text = $"{shinyLine}{rarity}{UiFormat.Weight(fish.Grams)}";
            _sellLabel.text = $"Bán {UiFormat.Coins(_game.Farm.GetFishPrice(fish))}";
        }

        // Rác: không có nút Thả (không vứt lại xuống sông), chỉ bán cho cô đồng nát.
        private void ShowJunkDetail(CaughtJunk item)
        {
            TryGetJunk(item.JunkId, out JunkDefinition art);
            _detailIcon.sprite = art != null ? art.Icon : null;
            _detailIcon.enabled = _detailIcon.sprite != null;
            ShinyLook.Apply(_detailIcon, null, 0);
            _detailName.text = art != null ? art.DisplayName : item.JunkId;
            int price = _game.Farm.GetJunkPrice(item);
            _detailInfo.text = $"<color=#{ColorUtility.ToHtmlStringRGB(_junkColor)}>Đồ vớt được</color> · Cô đồng nát trả {UiFormat.Coins(price)} xu";
            _sellLabel.text = $"Bán {UiFormat.Coins(price)}";
            _release.gameObject.SetActive(false);
        }

        private void Select(int index)
        {
            _selected = index;
            Rebuild();
        }

        // ---------- Bán / thả ----------

        private void SellSelected()
        {
            if (_selected < 0)
                return;
            int index = _selected;
            // Bỏ chọn trước: lệnh phát Farm.Changed → Rebuild ngay.
            _selected = -1;
            int fishCount = _game.Farm.State.Fishing.Basket.Count;
            if (index >= fishCount)
            {
                if (_game.Farm.SellJunk(index - fishCount, out long junkEarned) == FarmResult.Ok)
                    ShowToast(UiFormat.JunkSold(junkEarned));
                else
                    Rebuild();
                return;
            }
            if (_game.Farm.SellFish(index, out long earned) == FarmResult.Ok)
                ShowToast($"+{UiFormat.Coins(earned)} xu");
            else
                Rebuild();
        }

        private void ToggleAutoSell()
        {
            Farm farm = _game.Farm;
            bool on = !farm.State.Fishing.AutoSellJunk;
            farm.SetAutoSellJunk(on);
            ShowToast(on ? "Vớt được rác sẽ bán ngay cho cô đồng nát" : "Rác vớt được sẽ cho vào giỏ");
        }

        private void ReleaseSelected()
        {
            if (_selected < 0 || _selected >= _game.Farm.State.Fishing.Basket.Count)
                return;
            int index = _selected;
            _selected = -1;
            if (_game.Farm.ReleaseFish(index) != FarmResult.Ok)
                Rebuild();
        }

        private void AskSellAll()
        {
            Farm farm = _game.Farm;
            int count = farm.GetSellAllCount();
            if (count == 0)
                return;
            int kept = farm.State.Fishing.BasketUsed - count;
            string keptNote = kept > 0 ? $"\n(Giữ lại {kept} con cá đột biến / cho đơn)" : "";
            ShowConfirm($"Bán hết {count} món trong giỏ?\nĐược {UiFormat.Coins(farm.GetBasketValue())} xu{keptNote}", "Bán", true, () =>
            {
                _selected = -1;
                if (_game.Farm.SellAllFish(out long earned) == FarmResult.Ok)
                    ShowToast($"+{UiFormat.Coins(earned)} xu");
            });
        }

        // ---------- Mở ô ----------

        private void AskUnlock()
        {
            Farm farm = _game.Farm;
            if (!farm.CanUnlockBasketSlot)
                return;
            long price = farm.NextBasketSlotPrice;
            ShowConfirm($"Mở thêm 1 ô giỏ (ô {farm.BasketCapacity + 1})\nGiá: {UiFormat.Coins(price)} xu", "Mở", farm.State.Coins >= price, () =>
            {
                if (_game.Farm.UnlockBasketSlot() == FarmResult.NotEnoughCoins)
                    ShowToast("Không đủ xu");
            });
        }

        // ---------- Hộp xác nhận ----------

        private void ShowConfirm(string text, string okText, bool canConfirm, Action onConfirm)
        {
            _confirmText.text = text;
            _confirmOkLabel.text = okText;
            _confirmOk.interactable = canConfirm;
            _pendingConfirm = onConfirm;
            _confirmRoot.SetActive(true);
        }

        private void HideConfirm()
        {
            _pendingConfirm = null;
            _confirmRoot.SetActive(false);
        }

        private void ConfirmOk()
        {
            Action action = _pendingConfirm;
            HideConfirm();
            action?.Invoke();
        }

        // ---------- Hỗ trợ ----------

        private void ShowToast(string text)
        {
            if (_toast != null)
                _toast.Show(text);
        }

        private ShinyPalette.Entry Shiny(ShinyKind kind)
        {
            FishingDefinition data = _game.Definition.Fishing;
            return data != null ? data.GetShiny(kind) : null;
        }

        // Vị trí đốm lấp lánh theo cân nặng (giống bảng cá vừa câu) để con cá giữ nguyên đốm.
        private static int ShinySeed(CaughtFish fish) => fish.Grams;

        private bool TryGetFish(string id, out FishDefinition art, out FishDef def)
        {
            art = null;
            FishingDefinition data = _game.Definition.Fishing;
            if (data != null)
                data.TryGetFish(id, out art);
            return _game.Farm.FishingDefinition.TryGetFish(id, out def);
        }

        private bool TryGetJunk(string id, out JunkDefinition art)
        {
            art = null;
            FishingDefinition data = _game.Definition.Fishing;
            return data != null && data.TryGetJunk(id, out art);
        }

        private Color RarityColor(FishDef def)
        {
            int r = def != null ? (int)def.Rarity : 0;
            return r < _rarityColors.Length ? _rarityColors[r] : Color.gray;
        }
    }
}
