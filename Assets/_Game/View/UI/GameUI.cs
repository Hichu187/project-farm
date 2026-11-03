using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.CameraControl;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Điều phối HUD, thanh tab và các bảng. Mỗi lúc chỉ mở một bảng.
    public sealed class GameUI : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmInteraction _interaction;
        [SerializeField] private CameraTouchInput _touch;
        [SerializeField] private TMP_Text _coins;
        [SerializeField] private Button _backup;
        [SerializeField] private Button _farmTab;
        [SerializeField] private Button _storageTab;
        [SerializeField] private Button _shopTab;
        [SerializeField] private Toast _toast;
        [SerializeField] private SeedPanel _seedPanel;
        [SerializeField] private StoragePanel _storagePanel;
        [SerializeField] private ShopPanel _shopPanel;
        [SerializeField] private UnlockDialog _unlockDialog;
        [SerializeField] private CropProgressPopup _progressPopup;
        [Tooltip("Gán các vùng ao, chuồng, nhà kho, đất mở rộng sau khi vẽ xong Decor.")]
        [SerializeField] private ComingSoonArea[] _comingSoonAreas = System.Array.Empty<ComingSoonArea>();

        private void Start()
        {
            // TODO(Phần 7): nút sao lưu mở màn xuất/nhập mã.
            _backup.onClick.AddListener(() => _toast.Show("Sao lưu: sắp có"));
            _farmTab.onClick.AddListener(CloseAll);
            _storageTab.onClick.AddListener(() => OpenOnly(_storagePanel));
            _shopTab.onClick.AddListener(() => OpenOnly(_shopPanel));

            _seedPanel.BuyRequested += OpenShop;
            _interaction.SeedRequested += OpenSeedPanel;
            _interaction.UnlockRequested += _unlockDialog.Show;
            _interaction.Message += _toast.Show;
            _touch.TappedOutsideAreas += HandleTapOutside;
            _game.Farm.Changed += RefreshCoins;
            RefreshCoins();
        }

        private void OnDestroy()
        {
            if (_seedPanel != null)
                _seedPanel.BuyRequested -= OpenShop;
            if (_interaction != null)
            {
                _interaction.SeedRequested -= OpenSeedPanel;
                _interaction.UnlockRequested -= _unlockDialog.Show;
                _interaction.Message -= _toast.Show;
            }
            if (_touch != null)
                _touch.TappedOutsideAreas -= HandleTapOutside;
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= RefreshCoins;
        }

        private void RefreshCoins()
        {
            _coins.text = $"Xu {UiFormat.Coins(_game.Farm.State.Coins)}";
        }

        private void OpenSeedPanel(PlotId plot)
        {
            CloseAll();
            _seedPanel.OpenForPlot(plot);
        }

        private void OpenShop()
        {
            OpenOnly(_shopPanel);
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
            _seedPanel.Close();
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
