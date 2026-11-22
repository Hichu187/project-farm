using System.Collections.Generic;
using System.Text;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Mô tả sự kiện bằng lời cho người chơi (Đợt 2 Phần 8): tra tên cây / cá / người làng từ data View rồi ghép câu
    // bằng UiFormat.EventEffectLine. Dùng cho bảng chi tiết sự kiện (icon HUD).
    public static class EventText
    {
        // Các dòng hiệu ứng (bỏ Overlay, gộp dòng trùng), mỗi dòng một "• …".
        public static string Effects(GameBootstrap game, EventDef e)
        {
            var lines = new List<string>();
            foreach (EventEffect effect in e.Effects)
            {
                string line = UiFormat.EventEffectLine(effect, TargetName(game, e, effect));
                if (!string.IsNullOrEmpty(line) && !lines.Contains(line))
                    lines.Add(line);
            }
            var text = new StringBuilder();
            foreach (string line in lines)
            {
                if (text.Length > 0)
                    text.Append('\n');
                text.Append("• ").Append(line);
            }
            return text.ToString();
        }

        private static string TargetName(GameBootstrap game, EventDef e, EventEffect effect)
        {
            if (effect.Type == EventEffectType.UnlockCrop)
                return CropName(game, effect.Id);
            EffectTarget target = effect.Target;
            switch (target.Kind)
            {
                case EffectTargetKind.All:
                    switch (effect.Type)
                    {
                        case EventEffectType.CropPriceMult: return "Nông sản";
                        case EventEffectType.FishWeightMult:
                        case EventEffectType.FishSizeMult: return "Cá";
                        default: return "Đơn hàng";
                    }
                case EffectTargetKind.Crop:
                    return CropName(game, target.Id);
                case EffectTargetKind.Group:
                    return GroupName(target.Id);
                case EffectTargetKind.RandomCrop:
                    var names = new List<string>();
                    foreach (string id in game.Farm.RandomCrops(e, target.Count))
                        names.Add(CropName(game, id));
                    return string.Join(", ", names);
                case EffectTargetKind.Fish:
                    FishingDefinition fishing = game.Definition.Fishing;
                    return fishing != null && fishing.TryGetFish(target.Id, out FishDefinition fish) ? fish.DisplayName : target.Id;
                case EffectTargetKind.Rarity:
                    return System.Enum.TryParse(target.Id, out FishRarity rarity)
                        ? "Cá " + UiFormat.RarityName(rarity).ToLowerInvariant()
                        : "Cá";
                case EffectTargetKind.Npc:
                    return game.Farm.Definition.Orders.Dialogue.TryGetNpc(target.Id, out NpcDef npc)
                        ? $"Đơn của {npc.DisplayName}"
                        : "Đơn hàng";
                case EffectTargetKind.FishOrders:
                    return "Đơn cá";
                default:
                    return "";
            }
        }

        private static string CropName(GameBootstrap game, string id)
            => game.TryGetCrop(id, out CropDefinition crop) ? crop.DisplayName : id;

        private static string GroupName(string group)
        {
            switch (group)
            {
                case nameof(CropGroup.LeafyGreen): return "Rau lá";
                case nameof(CropGroup.Fruit): return "Nhóm quả";
                case nameof(CropGroup.RootGrain): return "Củ, hạt";
                default: return group;
            }
        }
    }
}
