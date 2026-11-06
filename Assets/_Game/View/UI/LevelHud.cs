using TMPro;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // "Cấp N" + thanh XP trên HUD. Thanh là RectTransform kéo giãn theo anchorMax.x.
    public sealed class LevelHud : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _game;
        [SerializeField] private TMP_Text _level;
        [SerializeField] private RectTransform _fill;
        [SerializeField] private TMP_Text _xp;

        private void Start()
        {
            _game.Farm.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= Refresh;
        }

        private void Refresh()
        {
            Farm farm = _game.Farm;
            int need = farm.GetXpToNext();
            _level.text = $"Cấp {farm.State.Level}";

            float ratio = need > 0 ? Mathf.Clamp01((float)farm.State.Xp / need) : 1f;
            _fill.anchorMax = new Vector2(ratio, 1f);
            _xp.text = need > 0 ? $"{UiFormat.Coins(farm.State.Xp)}/{UiFormat.Coins(need)}" : "tối đa";
        }
    }
}
