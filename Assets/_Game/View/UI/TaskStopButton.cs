using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.View.Character;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Nút "Dừng (N)": chỉ hiện khi hàng việc của nhân vật còn việc; bấm thì hủy phần còn lại.
    // Gắn trên GameObject cha luôn bật; _button là con, bật / tắt theo hàng việc.
    public sealed class TaskStopButton : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private FarmCharacterController _character;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        private int _shownCount = -1;

        private void Start()
        {
            if (_character == null)
                _character = FindFirstObjectByType<FarmCharacterController>();
            _button.onClick.AddListener(Stop);
            _game.Tasks.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_game != null)
                _game.Tasks.Changed -= Refresh;
        }

        private void Stop()
        {
            if (_character != null)
                _character.CancelTasks();
            else
                _game.Tasks.Cancel();
        }

        private void Refresh()
        {
            int count = _game.Tasks.Count;
            if (count == _shownCount)
                return;

            _shownCount = count;
            _button.gameObject.SetActive(count > 0);
            if (count > 0)
                _label.text = $"Dừng ({count})";
        }
    }
}
