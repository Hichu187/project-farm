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
            _game.Farm.OrderDelivered += OnOrderDelivered;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
            {
                _game.Farm.Changed -= Refresh;
                _game.Farm.OrderDelivered -= OnOrderDelivered;
            }
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

            string customer = NpcName(order);
            // Thưởng lúc giao, đã nhân sự kiện (OrderBonus).
            _game.Farm.GetOrderReward(order, out long rewardCoins, out long rewardXp);
            string reward = UiFormat.Reward(rewardCoins, rewardXp);
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
                farm.GetOrderReward(order, out long coins, out long xp);
                card.ShowOrder(NpcName(order), UiFormat.Reward(coins, xp), farm.CanDeliver(i), badge);
                ShowSpeaker(card, order);
                int shown = Math.Min(order.Items.Length, card.ItemSlots);
                for (int j = 0; j < shown; j++)
                {
                    OrderItem item = order.Items[j];
                    int have = farm.GetOrderItemHave(order, item);
                    ItemLook(order, item, out Sprite icon, out string name);
                    // Hai dòng: tên dài vẫn đủ chỗ cho số lượng (font pixel rộng).
                    card.SetItem(j, icon, $"{name}\n{Math.Min(have, item.Count)}/{item.Count}", have >= item.Count);
                }
                card.HideItemsFrom(shown);
            }
        }

        // Người đặt + câu đặt đơn (điền món đầu: số lượng, tên viết thường, món theo seed của đơn).
        private void ShowSpeaker(OrderCard card, OrderState order)
        {
            OrderDialogue dialogue = _game.Farm.Definition.Orders.Dialogue;
            if (!dialogue.TryGetNpc(order.NpcId, out NpcDef npc))
            {
                card.SetSpeaker(null, null, null, null, Color.white);
                return;
            }
            string speech = null;
            if (dialogue.TryGetLine(order.Line, out OrderLineDef line) && order.Items.Length > 0)
            {
                OrderItem first = order.Items[0];
                ItemLook(order, first, out _, out string itemName);
                string dish = order.IsFish ? null : dialogue.PickDish(first.Id, order.Seed);
                speech = OrderDialogue.Format(line.Text, npc.Title, first.Count, itemName.ToLowerInvariant(), dish);
            }
            card.SetSpeaker(npc.DisplayName, speech, _game.Definition.GetNpcPortrait(npc.Id), Initial(npc.DisplayName), NpcColor(npc.Id));
        }

        // Giao xong: bong bóng câu cảm ơn 2 giây trên thẻ của ô đó.
        private void OnOrderDelivered(OrderDelivery delivery)
        {
            if (delivery.Slot < 0 || delivery.Slot >= _cards.Length)
                return;
            OrderDialogue dialogue = _game.Farm.Definition.Orders.Dialogue;
            if (!dialogue.TryGetLine(delivery.ThanksLine, out OrderLineDef line))
                return;
            string title = dialogue.TryGetNpc(delivery.NpcId, out NpcDef npc) ? npc.Title : "";
            string text = OrderDialogue.Format(line.Text, title, 0, "", null);
            string who = npc != null ? $"{npc.DisplayName}: " : "";
            _cards[delivery.Slot].ShowBubble(who + text, ThanksSeconds);
        }

        private const float ThanksSeconds = 2f;

        // Chữ cái đầu của tên gọi (từ cuối): "Bà Lụa" → "L", "Cu Tí" → "T".
        private static string Initial(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return "";
            string trimmed = displayName.Trim();
            int space = trimmed.LastIndexOf(' ');
            string word = space >= 0 ? trimmed.Substring(space + 1) : trimmed;
            return word.Length > 0 ? word.Substring(0, 1).ToUpperInvariant() : "";
        }

        private static readonly Color[] NpcColors =
        {
            new Color(0.95f, 0.72f, 0.55f),
            new Color(0.62f, 0.80f, 0.55f),
            new Color(0.58f, 0.75f, 0.92f),
            new Color(0.92f, 0.62f, 0.70f),
            new Color(0.80f, 0.70f, 0.92f),
            new Color(0.95f, 0.85f, 0.50f),
        };

        // Màu khung tròn cố định theo id người.
        private static Color NpcColor(string npcId)
        {
            int hash = 17;
            foreach (char c in npcId)
                hash = hash * 31 + c;
            return NpcColors[(hash & int.MaxValue) % NpcColors.Length];
        }

        // Tên người đặt (null nếu đơn không gắn người).
        private string NpcName(OrderState order)
            => _game.Farm.Definition.Orders.Dialogue.TryGetNpc(order.NpcId, out NpcDef npc) ? npc.DisplayName : null;

        // Icon + tên món: nông sản (Kho) hoặc cá (giỏ).
        private void ItemLook(OrderState order, OrderItem item, out Sprite icon, out string name)
        {
            icon = null;
            name = item.Id;
            if (order.IsFish)
            {
                FishingDefinition fishing = _game.Definition.Fishing;
                if (fishing != null && fishing.TryGetFish(item.Id, out FishDefinition fish))
                {
                    icon = fish.Icon;
                    name = fish.DisplayName;
                }
            }
            else if (_game.TryGetCrop(item.Id, out CropDefinition crop))
            {
                icon = crop.ProduceIcon;
                name = crop.DisplayName;
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
