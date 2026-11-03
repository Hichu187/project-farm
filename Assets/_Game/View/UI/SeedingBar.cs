using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.View.CameraControl;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Thanh "Đang gieo": chỉ hiện khi đang zoom vào khu và đã chọn hạt.
    public sealed class SeedingBar : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmInteraction _interaction;
        [SerializeField] private CameraController _camera;
        [SerializeField] private SeedPanel _seedPanel;
        [SerializeField] private GameObject _bar;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _change;

        private CameraController.Mode _lastMode;

        private void Start()
        {
            _change.onClick.AddListener(_seedPanel.OpenForChange);
            _interaction.SelectionChanged += Refresh;
            _game.Farm.Changed += Refresh;
            _lastMode = _camera.CurrentMode;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_interaction != null)
                _interaction.SelectionChanged -= Refresh;
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= Refresh;
        }

        private void Update()
        {
            if (_camera.CurrentMode != _lastMode)
            {
                _lastMode = _camera.CurrentMode;
                Refresh();
            }
        }

        private void Refresh()
        {
            bool show = _camera.CurrentMode == CameraController.Mode.Focused && _interaction.SelectedCrop != null;
            _bar.SetActive(show);
            if (show)
                _text.text = $"Đang gieo: {_interaction.SelectedCrop.DisplayName} x{_game.Farm.State.GetSeeds(_interaction.SelectedCrop.Id)}";
        }
    }
}
