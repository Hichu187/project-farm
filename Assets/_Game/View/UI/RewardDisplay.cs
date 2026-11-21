using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Chữ + icon của một phần quà (hộp thư, điểm danh). Tách từ MailPanel (Đợt 2 Phần 10).
    public static class RewardDisplay
    {
        public static string Label(GameBootstrap game, Reward reward)
        {
            switch (reward.Kind)
            {
                case RewardKind.Coins: return $"{UiFormat.Coins(reward.Amount)} xu";
                case RewardKind.Xp: return $"{reward.Amount} XP";
                case RewardKind.Seeds: return $"{reward.Amount} hạt {CropName(game, reward.Id)}";
                case RewardKind.Produce: return $"{reward.Amount} {CropName(game, reward.Id)}";
                case RewardKind.Bait: return $"{reward.Amount} {BaitName(game, reward.Id)}";
                case RewardKind.Rod: return RodName(game, reward.Id);
                default: return reward.Amount.ToString();
            }
        }

        // Ngắn cho ô lịch: "50 xu", "5", "Cần nứa".
        public static string ShortLabel(GameBootstrap game, Reward reward)
        {
            switch (reward.Kind)
            {
                case RewardKind.Coins: return $"{UiFormat.Coins(reward.Amount)} xu";
                case RewardKind.Xp: return $"{reward.Amount} XP";
                case RewardKind.Rod: return RodName(game, reward.Id);
                default: return $"×{reward.Amount}";
            }
        }

        public static Sprite Icon(GameBootstrap game, Reward reward, Sprite coinIcon)
        {
            FishingDefinition fishing = game.Definition.Fishing;
            switch (reward.Kind)
            {
                case RewardKind.Coins:
                    return coinIcon;
                case RewardKind.Seeds:
                    return game.TryGetCrop(reward.Id, out CropDefinition seedCrop) ? seedCrop.SeedSprite : null;
                case RewardKind.Produce:
                    return game.TryGetCrop(reward.Id, out CropDefinition crop) ? crop.ProduceIcon : null;
                case RewardKind.Bait:
                    return fishing != null && fishing.TryGetBait(reward.Id, out BaitDefinition bait) ? bait.Icon : null;
                case RewardKind.Rod:
                    return fishing != null && fishing.TryGetRod(reward.Id, out RodDefinition rod) ? rod.Icon : null;
                default:
                    return null;
            }
        }

        private static string CropName(GameBootstrap game, string id)
            => game.TryGetCrop(id, out CropDefinition crop) ? crop.DisplayName : id;

        private static string BaitName(GameBootstrap game, string id)
        {
            FishingDefinition fishing = game.Definition.Fishing;
            return fishing != null && fishing.TryGetBait(id, out BaitDefinition bait) ? bait.DisplayName : id;
        }

        private static string RodName(GameBootstrap game, string id)
        {
            FishingDefinition fishing = game.Definition.Fishing;
            return fishing != null && fishing.TryGetRod(id, out RodDefinition rod) ? rod.DisplayName : id;
        }
    }
}
