using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Xác nhận mở ô. Nút Mở mờ khi không đủ xu. Lúc mở bảng, ô đang mua có khung chọn.
    public sealed class UnlockDialog : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmInteraction _interaction;
        [SerializeField] private FarmView _farmView;
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Button _confirm;
        [SerializeField] private Button _cancel;

        private string _zoneId;

        public void Show(PlotId plot, int price)
        {
            _zoneId = plot.ZoneId;
            _priceText.text = $"Giá: {UiFormat.Coins(price)} xu";
            _confirm.interactable = _game.Farm.State.Coins >= price;
            _root.SetActive(true);
            if (_farmView != null)
                _farmView.ShowPlotSelection(plot);
        }

        private void Awake()
        {
            // Scene cũ chưa gán thì tự tìm (chỉ lúc khởi động).
            if (_farmView == null)
                _farmView = FindFirstObjectByType<FarmView>();
            _confirm.onClick.AddListener(Confirm);
            _cancel.onClick.AddListener(Hide);
        }

        private void Confirm()
        {
            _interaction.ConfirmUnlock(_zoneId);
            Hide();
        }

        public void Hide()
        {
            _root.SetActive(false);
            if (_farmView != null)
                _farmView.HidePlotSelection();
        }
    }
}
