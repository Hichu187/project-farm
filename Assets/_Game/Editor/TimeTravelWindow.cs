using UnityEditor;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Map;

namespace VeQue.EditorTools
{
    // Tua nhanh thời gian khi đang Play để test cây lớn, cấp độ. Chỉ tua tới; muốn về giờ thật thì dùng Về Quê → Xóa save.
    public sealed class TimeTravelWindow : EditorWindow
    {
        private static readonly (string Label, long Seconds)[] Steps =
        {
            ("+1 phút", 60),
            ("+10 phút", 600),
            ("+1 giờ", 3600),
            ("+4 giờ", 4 * 3600),
            ("+1 ngày", 24 * 3600),
        };

        private GameBootstrap _game;

        [MenuItem("Về Quê/Tua thời gian")]
        private static void Open()
        {
            GetWindow<TimeTravelWindow>("Tua thời gian");
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
                Repaint();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Bấm Play để tua thời gian.", MessageType.Info);
                return;
            }

            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            if (_game == null || _game.Farm == null)
            {
                EditorGUILayout.HelpBox("Không thấy GameBootstrap trong scene.", MessageType.Warning);
                return;
            }

            Farm farm = _game.Farm;
            int need = farm.GetXpToNext();
            EditorGUILayout.LabelField("Đã tua", FormatDuration(_game.TimeOffsetSeconds));
            EditorGUILayout.LabelField("Cấp", need > 0 ? $"{farm.State.Level} ({farm.State.Xp}/{need} XP)" : $"{farm.State.Level} (tối đa)");
            EditorGUILayout.LabelField("Xu", farm.State.Coins.ToString("N0"));

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach ((string label, long seconds) in Steps)
                {
                    if (GUILayout.Button(label))
                        _game.AdvanceTime(seconds);
                }
            }

            EditorGUILayout.HelpBox("Chỉ tua tới. Về giờ thật: Về Quê → Xóa save (xóa cả khoảng đã tua).", MessageType.None);

            DrawOrders(farm);
        }

        private void DrawOrders(Farm farm)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Đơn hàng", EditorStyles.boldLabel);
            if (farm.OrderSlotCount == 0)
            {
                EditorGUILayout.LabelField("Data không có đơn hàng.");
                return;
            }
            if (!farm.OrdersUnlocked)
            {
                EditorGUILayout.LabelField($"Mở ở cấp {farm.Definition.Orders.UnlockLevel}.");
                return;
            }

            for (int slot = 0; slot < farm.OrderSlotCount; slot++)
            {
                farm.TryGetOrder(slot, out OrderState order);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    if (!order.HasOrder)
                    {
                        long wait = System.Math.Max(0, order.AvailableAt - _game.NowSeconds);
                        EditorGUILayout.LabelField($"Ô {slot + 1}: có đơn mới sau {FormatClock(wait)}");
                        continue;
                    }

                    EditorGUILayout.LabelField($"Ô {slot + 1} · {order.Customer}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(DescribeItems(order));
                    EditorGUILayout.LabelField($"Thưởng: +{order.RewardCoins:N0} xu, +{order.RewardXp:N0} XP");
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(!farm.CanDeliver(slot)))
                        {
                            if (GUILayout.Button("Giao"))
                                _game.DeliverOrder(slot);
                        }
                        if (GUILayout.Button("Bỏ"))
                            farm.DiscardOrder(slot);
                    }
                }
            }
        }

        private string DescribeItems(OrderState order)
        {
            var parts = new string[order.Items.Length];
            for (int i = 0; i < order.Items.Length; i++)
            {
                OrderItem item = order.Items[i];
                string name = _game.TryGetCrop(item.CropId, out var crop) ? crop.DisplayName : item.CropId;
                parts[i] = $"{name} {_game.Farm.State.GetItems(item.CropId)}/{item.Count}";
            }
            return string.Join(" · ", parts);
        }

        private static string FormatClock(long seconds) => $"{seconds / 60}:{seconds % 60:00}";

        private static string FormatDuration(long seconds)
        {
            long days = seconds / 86400;
            long hours = seconds % 86400 / 3600;
            long minutes = seconds % 3600 / 60;
            return days > 0 ? $"{days} ngày {hours} giờ {minutes} phút" : $"{hours} giờ {minutes} phút";
        }
    }
}
