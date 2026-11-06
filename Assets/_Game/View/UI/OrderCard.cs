using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VeQue.View.UI
{
    // Thẻ một ô đơn: người đặt, tối đa 3 món (icon + đang có/cần), thưởng, nút Giao/Bỏ; hoặc dòng chờ đơn mới.
    public sealed class OrderCard : MonoBehaviour
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private TMP_Text _customer;
        [SerializeField] private TMP_Text _reward;
        [SerializeField] private GameObject[] _itemRoots;
        [SerializeField] private Image[] _itemIcons;
        [SerializeField] private TMP_Text[] _itemLabels;
        [SerializeField] private Button _deliver;
        [SerializeField] private Button _discard;
        [SerializeField] private TMP_Text _waiting;
        [SerializeField] private Color _doneColor = new Color(0.25f, 0.50f, 0.15f);
        [SerializeField] private Color _missingColor = new Color(0.27f, 0.16f, 0.09f);

        public Button Deliver => _deliver;
        public Button Discard => _discard;
        public int ItemSlots => _itemRoots.Length;

        public void ShowOrder(string customer, string reward, bool canDeliver)
        {
            _content.SetActive(true);
            _waiting.gameObject.SetActive(false);
            _customer.text = customer;
            _reward.text = reward;
            _deliver.interactable = canDeliver;
        }

        public void SetItem(int index, Sprite icon, string label, bool done)
        {
            _itemRoots[index].SetActive(true);
            _itemIcons[index].sprite = icon;
            _itemIcons[index].enabled = icon != null;
            _itemLabels[index].text = label;
            _itemLabels[index].color = done ? _doneColor : _missingColor;
        }

        public void HideItemsFrom(int index)
        {
            for (int i = index; i < _itemRoots.Length; i++)
                _itemRoots[i].SetActive(false);
        }

        public void ShowWaiting(string text)
        {
            _content.SetActive(false);
            _waiting.gameObject.SetActive(true);
            _waiting.text = text;
        }
    }
}
