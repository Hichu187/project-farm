using System;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Bảng Đơn hàng: mỗi ô đơn một thẻ. Làm mới khi Farm đổi, và mỗi giây khi đang mở (đếm ngược ô trống).
    public sealed class OrderPanel : MonoBehaviour
    {
        private const float CountdownInterval = 1f;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private BottomSheet _sheet;
        [SerializeField] private OrderCard[] _cards = Array.Empty<OrderCard>();

        private float _nextCountdown;

        public event Action<string> Message;

        public bool IsOpen => _sheet.IsOpen;

        public void Open()
        {
            Refresh();
            _sheet.Open();
        }

        public void Close()
        {
            _sheet.Close();
        }

        private void Start()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                int slot = i;
                _cards[i].Deliver.onClick.AddListener(() => Deliver(slot));
                _cards[i].Discard.onClick.AddListener(() => _game.Farm.DiscardOrder(slot));
            }

            _game.Farm.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= Refresh;
        }

        private void Update()
        {
            if (!IsOpen || Time.unscaledTime < _nextCountdown)
                return;
            _nextCountdown = Time.unscaledTime + CountdownInterval;
            Refresh();
        }

        private void Deliver(int slot)
        {
            if (!_game.Farm.TryGetOrder(slot, out OrderState order) || !order.HasOrder)
                return;

            string customer = order.Customer;
            string reward = UiFormat.Reward(order.RewardCoins, order.RewardXp);
            FarmResult result = _game.DeliverOrder(slot);
            if (result == FarmResult.Ok)
                Message?.Invoke(string.IsNullOrEmpty(customer) ? reward : $"{customer}: {reward}");
            else if (result == FarmResult.NotEnoughItems)
                Message?.Invoke("Chưa đủ hàng");
        }

        private void Refresh()
        {
            Farm farm = _game.Farm;
            for (int i = 0; i < _cards.Length; i++)
            {
                OrderCard card = _cards[i];
                bool used = farm.TryGetOrder(i, out OrderState order);
                card.gameObject.SetActive(used);
                if (!used)
                    continue;

                if (!order.HasOrder)
                {
                    long wait = Math.Max(0L, order.AvailableAt - _game.NowSeconds);
                    card.ShowWaiting($"Có đơn mới sau {UiFormat.Countdown(wait)}");
                    continue;
                }

                card.ShowOrder(order.Customer, UiFormat.Reward(order.RewardCoins, order.RewardXp), farm.CanDeliver(i));
                int shown = Math.Min(order.Items.Length, card.ItemSlots);
                for (int j = 0; j < shown; j++)
                {
                    OrderItem item = order.Items[j];
                    int have = farm.State.GetItems(item.CropId);
                    Sprite icon = _game.TryGetCrop(item.CropId, out CropDefinition crop) ? crop.ProduceIcon : null;
                    string name = crop != null ? crop.DisplayName : item.CropId;
                    card.SetItem(j, icon, $"{name} {Math.Min(have, item.Count)}/{item.Count}", have >= item.Count);
                }
                card.HideItemsFrom(shown);
            }
        }
    }
}
