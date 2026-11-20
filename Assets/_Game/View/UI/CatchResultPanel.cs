using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Fishing;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Bảng cá vừa câu được (duyệt 13/11/2026): tên, hình, cân nặng, độ hiếm (màu), giá; dòng nhỏ hạng kéo + XP.
    // Mở khi hình cá bay tới nhân vật; nền tối chặn chạm phía sau, OK để đóng.
    public sealed class CatchResultPanel : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FishingController _controller;
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _rarity;
        [SerializeField] private TMP_Text _weight;
        [SerializeField] private TMP_Text _price;
        [SerializeField] private TMP_Text _detail;
        [Tooltip("Dòng \"Loài mới!\" / \"Kỷ lục mới! (cũ …)\"; ẩn khi không phá kỷ lục.")]
        [SerializeField] private TMP_Text _record;
        [Tooltip("Dòng \"Đột biến: <kiểu>!\" (+ \"Lần đầu!\"); ẩn khi cá thường.")]
        [SerializeField] private TMP_Text _shiny;
        [SerializeField] private Button _ok;
        [Tooltip("Màu chữ độ hiếm: Thường, Khá, Hiếm, Quý, Huyền thoại.")]
        [SerializeField] private Color[] _rarityColors =
        {
            new Color(0.45f, 0.42f, 0.38f),
            new Color(0.25f, 0.55f, 0.20f),
            new Color(0.15f, 0.40f, 0.80f),
            new Color(0.55f, 0.25f, 0.75f),
            new Color(0.85f, 0.55f, 0.05f),
        };

        public bool IsOpen => _root.activeSelf;

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_controller == null)
                _controller = FindFirstObjectByType<FishingController>();
            _root.SetActive(false);
            if (_controller == null)
            {
                enabled = false;
                return;
            }
            _controller.FishCaught += Show;
            _ok.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            if (_controller != null)
                _controller.FishCaught -= Show;
        }

        private void Show(FishingSession session)
        {
            FishingDefinition data = _game.Definition.Fishing;
            FishDefinition fish = null;
            if (data != null)
                data.TryGetFish(session.Fish.Id, out fish);

            FishRarity rarity = session.Fish.Rarity;
            _name.text = fish != null ? fish.DisplayName : session.Fish.Id;
            _icon.sprite = fish != null ? fish.Icon : null;
            _icon.enabled = _icon.sprite != null;
            ShinyPalette.Entry shiny = data != null ? data.GetShiny(session.Shiny) : null;
            ShinyLook.Apply(_icon, shiny, session.Grams);
            if (_shiny != null)
            {
                _shiny.gameObject.SetActive(shiny != null);
                if (shiny != null)
                {
                    _shiny.text = UiFormat.ShinyCatch(shiny.DisplayName, session.IsNewShiny);
                    _shiny.color = shiny.SparkleColor;
                }
            }
            _rarity.text = UiFormat.RarityName(rarity);
            _rarity.color = (int)rarity < _rarityColors.Length ? _rarityColors[(int)rarity] : Color.black;
            _weight.text = UiFormat.Weight(session.Grams);
            _price.text = $"{UiFormat.Coins(session.Price)} xu";
            _detail.text = UiFormat.CatchDetail(session.Grade, session.Xp);
            if (_record != null)
            {
                string record = UiFormat.CatchRecord(session.IsNewRecord, session.PreviousRecordGrams);
                _record.text = record;
                _record.gameObject.SetActive(record.Length > 0);
            }
            _root.SetActive(true);
        }

        private void Close()
        {
            _root.SetActive(false);
        }
    }
}
