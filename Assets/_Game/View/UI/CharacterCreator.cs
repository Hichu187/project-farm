using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Character;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Màn tạo nhân vật (lớp phủ toàn màn hình): tên, kiểu tóc, màu tóc, xem trước nhân vật.
    // Chưa tạo nhân vật (game mới / save cũ) thì tự mở khi vào game và không đóng được cho tới khi bấm Bắt đầu.
    // Nút "Nhân vật" trên HUD mở lại để sửa (có nút Đóng).
    public sealed class CharacterCreator : MonoBehaviour
    {
        private const string FirstTitle = "Nhân vật của bạn";
        private const string EditTitle = "Sửa nhân vật";
        private const string StartText = "Bắt đầu";
        private const string SaveText = "Lưu";

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private CharacterPreview _preview;
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_Text _error;
        [SerializeField] private Button _hairPrev;
        [SerializeField] private Button _hairNext;
        [SerializeField] private TMP_Text _hairLabel;
        [SerializeField] private Button _colorPrev;
        [SerializeField] private Button _colorNext;
        [SerializeField] private TMP_Text _colorLabel;
        [SerializeField] private Image _colorSwatch;
        [SerializeField] private Button _confirm;
        [SerializeField] private TMP_Text _confirmLabel;
        [SerializeField] private Button _close;
        [Tooltip("Nút \"Nhân vật\" trên HUD.")]
        [SerializeField] private Button _openButton;

        private int _hairIndex;
        private int _colorIndex;

        // Báo khi mở / đóng (UI khác đóng bảng đang mở).
        public event Action Opened;
        public bool IsOpen => _panel.activeSelf;

        public void Open(bool firstTime)
        {
            CharacterLook look = _game.Farm.State.Character;
            CharacterSkin skin = _preview.Skin;
            _hairIndex = skin != null ? skin.FindHairIndex(look.HairStyle) : 0;
            _colorIndex = skin != null ? skin.FindHairColorIndex(look.HairColor) : 0;
            _nameInput.text = look.Name ?? "";

            _title.text = firstTime ? FirstTitle : EditTitle;
            _confirmLabel.text = firstTime ? StartText : SaveText;
            _close.gameObject.SetActive(!firstTime);
            _error.text = "";

            _panel.SetActive(true);
            RefreshLook();
            Opened?.Invoke();
        }

        public void Close() => _panel.SetActive(false);

        private void Start()
        {
            _hairPrev.onClick.AddListener(() => StepHair(-1));
            _hairNext.onClick.AddListener(() => StepHair(1));
            _colorPrev.onClick.AddListener(() => StepColor(-1));
            _colorNext.onClick.AddListener(() => StepColor(1));
            _confirm.onClick.AddListener(Confirm);
            _close.onClick.AddListener(Close);
            if (_openButton != null)
                _openButton.onClick.AddListener(() => Open(false));
            _nameInput.characterLimit = CharacterLook.MaxNameLength;
            _nameInput.onValueChanged.AddListener(_ => _error.text = "");

            if (_game.Farm.State.Character.Created)
                Close();
            else
                Open(true);
        }

        private void StepHair(int delta)
        {
            CharacterSkin skin = _preview.Skin;
            if (skin == null || skin.HairCount == 0)
                return;
            _hairIndex = Wrap(_hairIndex + delta, skin.HairCount);
            RefreshLook();
        }

        private void StepColor(int delta)
        {
            CharacterSkin skin = _preview.Skin;
            if (skin == null || skin.HairColorCount == 0)
                return;
            _colorIndex = Wrap(_colorIndex + delta, skin.HairColorCount);
            RefreshLook();
        }

        private void RefreshLook()
        {
            _preview.SetLook(_hairIndex, _colorIndex);
            CharacterSkin skin = _preview.Skin;
            if (skin == null)
                return;

            CharacterSkin.HairSet hair = skin.GetHair(_hairIndex);
            CharacterSkin.HairColorOption color = skin.GetHairColor(_colorIndex);
            _hairLabel.text = hair != null ? hair.DisplayName : "";
            _colorLabel.text = color != null ? color.DisplayName : "";
            // Ô màu: màu gốc thì để trong suốt (chỉ thấy khung).
            _colorSwatch.color = color == null || color.Original ? new Color(1f, 1f, 1f, 0f) : color.Color;
        }

        private void Confirm()
        {
            CharacterSkin skin = _preview.Skin;
            string hairId = skin != null && skin.HairCount > 0 ? skin.GetHair(_hairIndex).Id : null;
            string colorId = skin != null && skin.HairColorCount > 0 ? skin.GetHairColor(_colorIndex).Id : null;

            FarmResult result = _game.Farm.SetCharacter(_nameInput.text, hairId, colorId);
            if (result != FarmResult.Ok)
            {
                _error.text = $"Nhập tên từ 1 đến {CharacterLook.MaxNameLength} ký tự";
                return;
            }
            Close();
        }

        private static int Wrap(int value, int count) => ((value % count) + count) % count;
    }
}
