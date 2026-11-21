using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Bảng Album (Đợt 2 Phần 5, duyệt 21/11/2026): tab "Cá" (lưới loài, xếp theo độ hiếm) và "Đồ vớt được" (rác).
    // Chạm ô → trang chi tiết phủ lên lưới (nút Quay lại). Chưa có: bóng đen + "???"; cá chưa câu vẫn hiện độ hiếm,
    // độ sâu và gợi ý mờ; mồi ưa thích chỉ hiện khi đã câu được. Không hiện khung giờ (chỉ dev biết).
    public sealed class AlbumPanel : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private BottomSheet _sheet;
        [SerializeField] private TMP_Text _progress;

        [Header("Tab")]
        [SerializeField] private Button _fishTab;
        [SerializeField] private Button _junkTab;
        [SerializeField] private GameObject _fishPage;
        [SerializeField] private GameObject _junkPage;
        [SerializeField] private AlbumCell[] _fishCells = Array.Empty<AlbumCell>();
        [SerializeField] private AlbumCell[] _junkCells = Array.Empty<AlbumCell>();
        [SerializeField] private Color _tabActive = new Color(0.45f, 0.62f, 0.28f);
        [SerializeField] private Color _tabInactive = new Color(0.60f, 0.40f, 0.24f);

        [Header("Chi tiết")]
        [SerializeField] private GameObject _detail;
        [SerializeField] private Button _back;
        [SerializeField] private Image _detailIcon;
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private TMP_Text _detailRarity;
        [SerializeField] private TMP_Text _detailInfo;
        [SerializeField] private Color _silhouette = new Color(0.12f, 0.10f, 0.14f, 0.85f);
        [Tooltip("Màu chữ gợi ý (mờ).")]
        [SerializeField] private Color _hintColor = new Color(0.45f, 0.38f, 0.30f);

        [Tooltip("Thường → Huyền thoại.")]
        [SerializeField] private Color[] _rarityColors =
        {
            new Color(0.45f, 0.42f, 0.38f),
            new Color(0.25f, 0.55f, 0.20f),
            new Color(0.15f, 0.40f, 0.80f),
            new Color(0.55f, 0.25f, 0.75f),
            new Color(0.85f, 0.55f, 0.05f),
        };
        [SerializeField] private Color _junkColor = new Color(0.55f, 0.48f, 0.38f);

        private readonly List<FishDefinition> _fishOrder = new List<FishDefinition>();
        private readonly StringBuilder _text = new StringBuilder();
        private bool _junkTabShown;
        // Ô đang xem chi tiết: -1 = không; cá / rác theo tab.
        private int _detailIndex = -1;
        private bool _detailIsJunk;

        public bool IsOpen => _sheet.IsOpen;

        public void Open()
        {
            _junkTabShown = false;
            _detailIndex = -1;
            Refresh();
            _sheet.Open();
        }

        public void Close()
        {
            _sheet.Close();
        }

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            _fishTab.onClick.AddListener(() => ShowTab(false));
            _junkTab.onClick.AddListener(() => ShowTab(true));
            _back.onClick.AddListener(CloseDetail);
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed += HandleChanged;
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= HandleChanged;
        }

        private void HandleChanged()
        {
            if (IsOpen)
                Refresh();
        }

        private void ShowTab(bool junk)
        {
            _junkTabShown = junk;
            _detailIndex = -1;
            Refresh();
        }

        private void CloseDetail()
        {
            _detailIndex = -1;
            Refresh();
        }

        private void Refresh()
        {
            if (_game == null || _game.Farm == null || _game.Definition == null || _game.Definition.Fishing == null)
                return;
            FishingDefinition data = _game.Definition.Fishing;
            Farm farm = _game.Farm;

            _fishPage.SetActive(!_junkTabShown);
            _junkPage.SetActive(_junkTabShown);
            SetTabColor(_fishTab, !_junkTabShown);
            SetTabColor(_junkTab, _junkTabShown);

            BuildFishOrder(data);
            if (_junkTabShown)
            {
                FillJunk(data, farm);
                _progress.text = UiFormat.JunkProgress(farm.State.Fishing.JunkDex.Count, data.Junk.Count);
            }
            else
            {
                FillFish(farm);
                _progress.text = UiFormat.AlbumProgress(farm.DexSpeciesCount, _fishOrder.Count, NextSpeciesMilestone(farm));
            }

            _detail.SetActive(_detailIndex >= 0);
            if (_detailIndex < 0)
                return;
            if (_detailIsJunk)
                ShowJunkDetail(data, farm);
            else
                ShowFishDetail(data, farm);
        }

        // Cá theo độ hiếm (Thường trước), cùng độ hiếm giữ thứ tự data.
        private void BuildFishOrder(FishingDefinition data)
        {
            _fishOrder.Clear();
            for (int r = 0; r <= (int)FishRarity.Legendary; r++)
            {
                foreach (FishDefinition fish in data.Fish)
                {
                    if (fish != null && (int)fish.Rarity == r)
                        _fishOrder.Add(fish);
                }
            }
        }

        private void FillFish(Farm farm)
        {
            for (int i = 0; i < _fishCells.Length; i++)
            {
                if (i >= _fishOrder.Count)
                {
                    _fishCells[i].Hide();
                    continue;
                }
                FishDefinition fish = _fishOrder[i];
                int index = i;
                Action open = () => OpenDetail(index, false);
                if (farm.State.Fishing.HasCaught(fish.Id))
                    _fishCells[i].SetKnown(fish.Icon, fish.DisplayName, RarityColor(fish.Rarity), open);
                else
                    _fishCells[i].SetUnknown(fish.Icon, open);
            }
        }

        private void FillJunk(FishingDefinition data, Farm farm)
        {
            IReadOnlyList<JunkDefinition> junk = data.Junk;
            for (int i = 0; i < _junkCells.Length; i++)
            {
                if (i >= junk.Count || junk[i] == null)
                {
                    _junkCells[i].Hide();
                    continue;
                }
                int index = i;
                Action open = () => OpenDetail(index, true);
                if (farm.State.Fishing.JunkDex.ContainsKey(junk[i].Id))
                    _junkCells[i].SetKnown(junk[i].Icon, junk[i].DisplayName, _junkColor, open);
                else
                    _junkCells[i].SetUnknown(junk[i].Icon, open);
            }
        }

        private void OpenDetail(int index, bool junk)
        {
            _detailIndex = index;
            _detailIsJunk = junk;
            Refresh();
        }

        private void ShowFishDetail(FishingDefinition data, Farm farm)
        {
            if (_detailIndex >= _fishOrder.Count)
            {
                _detail.SetActive(false);
                return;
            }
            FishDefinition fish = _fishOrder[_detailIndex];
            bool caught = farm.State.Fishing.FishDex.TryGetValue(fish.Id, out FishDexEntry entry);
            SetDetailIcon(fish.Icon, caught);
            _detailName.text = caught ? fish.DisplayName : "???";
            _detailRarity.text = UiFormat.RarityName(fish.Rarity);
            _detailRarity.color = RarityColor(fish.Rarity);

            _text.Clear();
            _text.Append("Độ sâu: ").Append(UiFormat.DepthList(fish.Depths)).Append('\n');
            _text.Append("Mồi ưa thích: ");
            if (caught)
                AppendBaits(fish);
            else
                _text.Append("???");
            _text.Append('\n');
            if (caught)
            {
                _text.Append("Đã câu: ").Append(entry.Count).Append(" lần\n");
                _text.Append("Kỷ lục: ").Append(UiFormat.Weight(entry.MaxGrams)).Append('\n');
                _text.Append("Lần đầu: ").Append(UiFormat.DayKeyDate(entry.FirstDayKey));
            }
            else if (!string.IsNullOrEmpty(fish.Hint))
            {
                _text.Append("\n<color=#").Append(ColorUtility.ToHtmlStringRGB(_hintColor)).Append('>')
                    .Append(fish.Hint).Append("</color>");
            }
            _detailInfo.text = _text.ToString();
        }

        private void AppendBaits(FishDefinition fish)
        {
            int written = 0;
            foreach (BaitDefinition bait in fish.PreferredBaits)
            {
                if (bait == null)
                    continue;
                if (written++ > 0)
                    _text.Append(", ");
                _text.Append(bait.DisplayName);
            }
            if (written == 0)
                _text.Append("không kén");
        }

        private void ShowJunkDetail(FishingDefinition data, Farm farm)
        {
            IReadOnlyList<JunkDefinition> list = data.Junk;
            if (_detailIndex >= list.Count || list[_detailIndex] == null)
            {
                _detail.SetActive(false);
                return;
            }
            JunkDefinition junk = list[_detailIndex];
            bool found = farm.State.Fishing.JunkDex.TryGetValue(junk.Id, out JunkDexEntry entry);
            SetDetailIcon(junk.Icon, found);
            _detailName.text = found ? junk.DisplayName : "???";
            _detailRarity.text = "Đồ vớt được";
            _detailRarity.color = _junkColor;

            _text.Clear();
            if (found)
            {
                farm.FishingDefinition.TryGetJunk(junk.Id, out JunkDef def);
                _text.Append("Cô đồng nát trả: ").Append(UiFormat.Coins(def != null ? def.SellPrice : 0)).Append(" xu\n");
                _text.Append("Đã vớt: ").Append(entry.Count).Append(" lần\n");
                _text.Append("Lần đầu: ").Append(UiFormat.DayKeyDate(entry.FirstDayKey)).Append("\n\n");
                _text.Append('"').Append(junk.FlavorText).Append('"');
            }
            else
            {
                _text.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(_hintColor)).Append(">Chưa vớt được món này.</color>");
            }
            _detailInfo.text = _text.ToString();
        }

        private void SetDetailIcon(Sprite icon, bool known)
        {
            _detailIcon.sprite = icon;
            _detailIcon.enabled = icon != null;
            _detailIcon.color = known ? Color.white : _silhouette;
        }

        // Mốc số loài kế tiếp chưa đạt; 0 = hết mốc.
        private static int NextSpeciesMilestone(Farm farm)
        {
            int caught = farm.DexSpeciesCount;
            int next = 0;
            foreach (DexMilestoneDef milestone in farm.FishingDefinition.DexMilestones)
            {
                if (milestone.Kind != DexMilestoneKind.SpeciesCount || milestone.Count <= caught)
                    continue;
                if (next == 0 || milestone.Count < next)
                    next = milestone.Count;
            }
            return next;
        }

        private Color RarityColor(FishRarity rarity)
        {
            int r = (int)rarity;
            return r < _rarityColors.Length ? _rarityColors[r] : Color.gray;
        }

        private void SetTabColor(Button tab, bool active)
        {
            if (tab.targetGraphic != null)
                tab.targetGraphic.color = active ? _tabActive : _tabInactive;
        }
    }
}
