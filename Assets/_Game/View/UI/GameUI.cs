using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.CameraControl;
using VeQue.View.Data;
using VeQue.View.Map;
using VeQue.View.Save;

namespace VeQue.View.UI
{
    // Điều phối HUD, thanh tab và các bảng. Mỗi lúc chỉ mở một bảng.
    // HUD theo vùng: ở nông trại có thanh tab, nút Nhân vật, nút Đi câu; ở vùng câu ẩn chúng, hiện tên vùng + nút Về nhà.
    public sealed class GameUI : MonoBehaviour
    {
        private const string OrdersTabText = "Đơn hàng";

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmInteraction _interaction;
        [SerializeField] private CameraTouchInput _touch;
        [SerializeField] private TMP_Text _coins;
        [SerializeField] private Button _backup;
        [SerializeField] private Button _farmTab;
        [SerializeField] private Button _ordersTab;
        [SerializeField] private TMP_Text _ordersTabLabel;
        [SerializeField] private Button _storageTab;
        [SerializeField] private Button _shopTab;
        [SerializeField] private Toast _toast;
        [SerializeField] private SeedPicker _seedPicker;
        [SerializeField] private FarmGesture _gesture;
        [SerializeField] private OrderPanel _orderPanel;
        [SerializeField] private StoragePanel _storagePanel;
        [SerializeField] private ShopPanel _shopPanel;
        [SerializeField] private UnlockDialog _unlockDialog;
        [SerializeField] private CropProgressPopup _progressPopup;
        [SerializeField] private CharacterCreator _characterCreator;
        [Header("Vùng chơi")]
        [Tooltip("Để trống thì tự tìm; không có thì ẩn nút Đi câu.")]
        [SerializeField] private RegionTravel _travel;
        [SerializeField] private Button _goFishing;
        [SerializeField] private Button _goHome;
        [SerializeField] private TMP_Text _regionLabel;
        [Tooltip("Khung tên vùng (cha của _regionLabel), chỉ hiện ở vùng câu.")]
        [SerializeField] private GameObject _regionPanel;
        [Tooltip("Chỉ hiện ở nông trại (thanh tab, nút Nhân vật, nút Đi câu).")]
        [SerializeField] private GameObject[] _farmOnly = System.Array.Empty<GameObject>();
        [Tooltip("Gán các vùng ao, chuồng, nhà kho, đất mở rộng sau khi vẽ xong Decor.")]
        [SerializeField] private ComingSoonArea[] _comingSoonAreas = System.Array.Empty<ComingSoonArea>();

        private const string OutdatedMessage = "Có bản game mới, hãy tải lại trang";

        // Số khu đã có ô mở, để báo "Đã mở Khu N!" khi tăng.
        private int _openZones;
        private int _deliverableOrders = -1;

        private void Start()
        {
            _backup.onClick.AddListener(OpenBackup);
            _farmTab.onClick.AddListener(CloseAll);
            _ordersTab.onClick.AddListener(OpenOrders);
            _storageTab.onClick.AddListener(() => OpenOnly(_storagePanel));
            _shopTab.onClick.AddListener(() => OpenOnly(_shopPanel));

            _seedPicker.ShopRequested += OpenShop;
            _seedPicker.Message += _toast.Show;
            _gesture.Message += _toast.Show;
            _interaction.Tapped += _seedPicker.Hide;
            _orderPanel.Message += _toast.Show;
            _storagePanel.Message += _toast.Show;
            _interaction.SeedRequested += OpenSeedPicker;
            _interaction.UnlockRequested += _unlockDialog.Show;
            _interaction.Message += _toast.Show;
            _touch.TappedOutsideAreas += HandleTapOutside;
            _game.Farm.Changed += HandleFarmChanged;
            _game.Farm.Rescued += HandleRescued;
            if (_characterCreator != null)
                _characterCreator.Opened += CloseAll;

            _openZones = CountOpenZones();
            HandleFarmChanged();

            if (_travel == null)
                _travel = FindFirstObjectByType<RegionTravel>();
            if (_travel != null)
            {
                _travel.RegionChanged += HandleRegionChanged;
                _travel.TravelStarted += CloseAll;
                _travel.Message += _toast.Show;
            }
            if (_goFishing != null)
                _goFishing.onClick.AddListener(GoFishing);
            if (_goHome != null)
                _goHome.onClick.AddListener(GoHome);
            ApplyRegionHud(false);

            if (_game.SavingBlocked)
                _toast.Show(OutdatedMessage);
        }

        private void OnDestroy()
        {
            if (_seedPicker != null && _toast != null)
            {
                _seedPicker.ShopRequested -= OpenShop;
                _seedPicker.Message -= _toast.Show;
            }
            if (_gesture != null && _toast != null)
                _gesture.Message -= _toast.Show;
            if (_orderPanel != null && _toast != null)
                _orderPanel.Message -= _toast.Show;
            if (_storagePanel != null && _toast != null)
                _storagePanel.Message -= _toast.Show;
            if (_interaction != null)
            {
                _interaction.SeedRequested -= OpenSeedPicker;
                if (_seedPicker != null)
                    _interaction.Tapped -= _seedPicker.Hide;
                _interaction.UnlockRequested -= _unlockDialog.Show;
                _interaction.Message -= _toast.Show;
            }
            if (_touch != null)
                _touch.TappedOutsideAreas -= HandleTapOutside;
            if (_game != null && _game.Farm != null)
            {
                _game.Farm.Changed -= HandleFarmChanged;
                _game.Farm.Rescued -= HandleRescued;
            }
            if (_characterCreator != null)
                _characterCreator.Opened -= CloseAll;
            if (_travel != null)
            {
                _travel.RegionChanged -= HandleRegionChanged;
                _travel.TravelStarted -= CloseAll;
                if (_toast != null)
                    _travel.Message -= _toast.Show;
            }
        }

        private void GoFishing()
        {
            if (_travel != null)
                _travel.GoFishing();
        }

        private void GoHome()
        {
            if (_travel != null)
                _travel.GoHome();
        }

        private void HandleRegionChanged(PlayRegion region)
        {
            CloseAll();
            ApplyRegionHud(_travel.IsFishing);
            if (_regionLabel != null)
                _regionLabel.text = region.DisplayName;
        }

        private void ApplyRegionHud(bool fishing)
        {
            for (int i = 0; i < _farmOnly.Length; i++)
            {
                if (_farmOnly[i] != null)
                    _farmOnly[i].SetActive(!fishing);
            }
            // Không có vùng câu trong scene thì không hiện nút Đi câu.
            if (_goFishing != null && (_travel == null || !_travel.HasFishingRegion))
                _goFishing.gameObject.SetActive(false);
            if (_goHome != null)
                _goHome.gameObject.SetActive(fishing);
            if (_regionPanel != null)
                _regionPanel.SetActive(fishing);
        }

        private void HandleFarmChanged()
        {
            Farm farm = _game.Farm;
            _coins.text = $"Xu {UiFormat.Coins(farm.State.Coins)}";
            RefreshOrdersTab(farm);

            int openZones = CountOpenZones();
            if (openZones > _openZones)
                _toast.Show($"Đã mở {UiFormat.ZoneName(openZones - 1)}!");
            _openZones = openZones;
        }

        private void RefreshOrdersTab(Farm farm)
        {
            int deliverable = 0;
            if (farm.OrdersUnlocked)
            {
                for (int i = 0; i < farm.OrderSlotCount; i++)
                {
                    if (farm.CanDeliver(i))
                        deliverable++;
                }
            }

            if (deliverable == _deliverableOrders)
                return;
            _deliverableOrders = deliverable;
            _ordersTabLabel.text = deliverable > 0 ? $"{OrdersTabText} ({deliverable})" : OrdersTabText;
        }

        // Khu mở theo thứ tự, nên số khu có ô mở = chỉ số khu mới nhất + 1.
        private int CountOpenZones()
        {
            int count = 0;
            foreach (ZoneState zone in _game.Farm.State.Zones)
            {
                if (zone.OpenCount > 0)
                    count++;
            }
            return count;
        }

        private void HandleRescued(string cropId)
        {
            string name = _game.TryGetCrop(cropId, out CropDefinition crop) ? crop.DisplayName : cropId;
            _toast.Show($"Được tặng 1 hạt {name}");
        }

        private void OpenBackup()
        {
            if (_game.SavingBlocked)
            {
                _toast.Show(OutdatedMessage);
                return;
            }

            CloseAll();
            if (!WebBridge.ShowBackup(_game.ExportBackupCode()))
                _toast.Show("Đã chép mã vào clipboard");
        }

        private void OpenSeedPicker(PlotId plot)
        {
            CloseAll();
            _seedPicker.Show(plot);
        }

        private void OpenShop()
        {
            OpenOnly(_shopPanel);
        }

        private void OpenOrders()
        {
            Farm farm = _game.Farm;
            if (!farm.OrdersUnlocked)
            {
                _toast.Show(farm.OrderSlotCount > 0
                    ? $"Đơn hàng mở ở cấp {farm.Definition.Orders.UnlockLevel}"
                    : "Đơn hàng: sắp có");
                return;
            }
            if (_orderPanel.IsOpen)
                return;

            CloseAll();
            _orderPanel.Open();
        }

        private void OpenOnly(CropListPanel panel)
        {
            if (panel.IsOpen)
                return;

            CloseAll();
            panel.Open();
        }

        private void CloseAll()
        {
            _progressPopup.Hide();
            _unlockDialog.Hide();
            _seedPicker.Hide();
            _orderPanel.Close();
            _storagePanel.Close();
            _shopPanel.Close();
        }

        private void HandleTapOutside(Vector2 world)
        {
            for (int i = 0; i < _comingSoonAreas.Length; i++)
            {
                ComingSoonArea area = _comingSoonAreas[i];
                if (area != null && area.Contains(world))
                {
                    _toast.Show($"{area.Label}: sắp có");
                    return;
                }
            }
        }
    }
}
