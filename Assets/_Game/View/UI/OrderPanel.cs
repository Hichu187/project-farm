using System;
using TMPro;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Bảng Đơn hàng: dòng "Còn N đơn hôm nay" + mỗi ô đơn một thẻ. Làm mới khi Farm đổi, và mỗi giây khi đang mở
    // (đếm ngược ô trống, hạn đơn khẩn cấp).
    public sealed class OrderPanel : MonoBehaviour
    {
        private const float CountdownInterval = 1f;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private BottomSheet _sheet;
        [SerializeField] private OrderCard[] _cards = Array.Empty<OrderCard>();
        [Tooltip("Dòng đầu bảng: số đơn còn giao được hôm nay. Để trống thì không hiện.")]
        [SerializeField] private TMP_Text _dailyInfo;

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
            else if (result == FarmResult.OrderExpired)
                Message?.Invoke("Đơn khẩn cấp đã hết hạn");
        }

        private void Refresh()
        {
            Farm farm = _game.Farm;
            RefreshDailyInfo(farm);
            bool canReceive = farm.CanReceiveOrderToday;
            for (int i = 0; i < _cards.Length; i++)
            {
                OrderCard card = _cards[i];
                bool used = farm.TryGetOrder(i, out OrderState order);
                card.gameObject.SetActive(used);
                if (!used)
                    continue;

                if (!order.HasOrder)
                {
                    if (!canReceive)
                    {
                        card.ShowWaiting("Hết đơn hôm nay, có đơn mới sau 0 giờ", false);
                        continue;
                    }
                    long wait = Math.Max(0L, order.AvailableAt - _game.NowSeconds);
                    string next = order.IsUrgent ? "Đơn khẩn cấp" : "Có đơn mới";
                    card.ShowWaiting($"{next} sau {UiFormat.Countdown(wait)}", order.IsUrgent);
                    continue;
                }

                string badge = null;
                if (order.IsUrgent)
                    badge = farm.TryGetOrderTimeLeft(i, out long left) ? $"KHẨN CẤP · còn {UiFormat.Countdown(left)}" : "KHẨN CẤP";
                card.ShowOrder(order.Customer, UiFormat.Reward(order.RewardCoins, order.RewardXp), farm.CanDeliver(i), badge);
                int shown = Math.Min(order.Items.Length, card.ItemSlots);
                for (int j = 0; j < shown; j++)
                {
                    OrderItem item = order.Items[j];
                    int have = farm.State.GetItems(item.CropId);
                    Sprite icon = _game.TryGetCrop(item.CropId, out CropDefinition crop) ? crop.ProduceIcon : null;
                    string name = crop != null ? crop.DisplayName : item.CropId;
                    // Hai dòng: tên dài vẫn đủ chỗ cho số lượng (font pixel rộng).
                    card.SetItem(j, icon, $"{name}\n{Math.Min(have, item.Count)}/{item.Count}", have >= item.Count);
                }
                card.HideItemsFrom(shown);
            }
        }

        private void RefreshDailyInfo(Farm farm)
        {
            if (_dailyInfo == null)
                return;

            bool show = farm.HasDailyOrderLimit && farm.OrdersUnlocked;
            if (_dailyInfo.gameObject.activeSelf != show)
                _dailyInfo.gameObject.SetActive(show);
            if (!show)
                return;

            int left = farm.OrdersLeftToday;
            _dailyInfo.text = left > 0
                ? $"Còn {left} đơn hôm nay"
                : $"Hết đơn hôm nay · đơn mới sau {UiFormat.Countdown(farm.SecondsUntilNextDay)}";
        }
    }
}
