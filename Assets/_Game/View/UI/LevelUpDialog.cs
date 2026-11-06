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
    // Hộp "Lên cấp N!" với thưởng và mở khóa. Lên liền nhiều cấp thì hiện lần lượt.
    public sealed class LevelUpDialog : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private Button _ok;

        private readonly Queue<int> _pending = new Queue<int>();
        private readonly StringBuilder _text = new StringBuilder();

        private void Start()
        {
            _ok.onClick.AddListener(ShowNext);
            _game.Farm.LeveledUp += Enqueue;
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
                _game.Farm.LeveledUp -= Enqueue;
        }

        private void Enqueue(int level)
        {
            _pending.Enqueue(level);
            if (!_root.activeSelf)
                ShowNext();
        }

        private void ShowNext()
        {
            if (_pending.Count == 0)
            {
                _root.SetActive(false);
                return;
            }

            LevelUnlocks unlocks = _game.Farm.GetUnlocksAtLevel(_pending.Dequeue());
            _title.text = $"Lên cấp {unlocks.Level}!";

            _text.Clear();
            if (unlocks.Coins > 0)
                _text.Append('+').Append(UiFormat.Coins(unlocks.Coins)).Append(" xu\n");
            foreach (string cropId in unlocks.CropIds)
            {
                // Cây đang tắt (chưa có art) không có trong game, không báo.
                if (!_game.TryGetCrop(cropId, out CropDefinition crop))
                    continue;
                _text.Append("Mở ").Append(crop.DisplayName);
                if (unlocks.SeedsPerCrop > 0)
                    _text.Append(" (+").Append(unlocks.SeedsPerCrop).Append(" hạt)");
                _text.Append('\n');
            }
            if (unlocks.OrdersUnlocked)
                _text.Append("Mở Đơn hàng\n");
            foreach (int zone in unlocks.ZoneIndices)
                _text.Append("Đủ cấp mở ").Append(UiFormat.ZoneName(zone)).Append('\n');

            _body.text = _text.ToString().TrimEnd('\n');
            _root.SetActive(true);
        }
    }
}
