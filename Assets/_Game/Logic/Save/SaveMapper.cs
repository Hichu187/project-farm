using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Chuyển FarmState <-> SaveData. Save đối chiếu với data hiện tại theo định danh ổn định,
    // nên đổi data sau này không làm hỏng save: ô không còn thì bỏ qua, cây không còn thì ô về trống.
    public static class SaveMapper
    {
        public static SaveData ToSave(FarmState state, FarmDef def, long lastSeenAt)
        {
            var zones = new SaveZone[state.Zones.Length];
            for (int i = 0; i < zones.Length; i++)
                zones[i] = ToSaveZone(state.Zones[i], def.Zones[i]);

            return new SaveData
            {
                version = SaveData.CurrentVersion,
                coins = state.Coins,
                level = state.Level,
                xp = state.Xp,
                lastSeenAt = lastSeenAt,
                seeds = ToSaveCounts(state.Seeds),
                inventory = ToSaveCounts(state.Inventory),
                zones = zones,
                orders = ToSaveOrders(state.Orders),
                randomState = state.RandomState,
            };
        }

        public static SaveLoadResult TryFromSave(SaveData data, FarmDef def, out FarmState state)
        {
            state = null;
            if (data == null || data.version < 1)
                return SaveLoadResult.Invalid;
            if (data.version > SaveData.CurrentVersion)
                return SaveLoadResult.NewerVersion;
            if (data.coins < 0 || data.level < 0 || data.xp < 0 || data.zones == null || data.zones.Length == 0)
                return SaveLoadResult.Invalid;

            var seeds = new Dictionary<string, int>();
            var inventory = new Dictionary<string, int>();
            if (!TryReadCounts(data.seeds, seeds) || !TryReadCounts(data.inventory, inventory))
                return SaveLoadResult.Invalid;

            var zones = new ZoneState[def.Zones.Count];
            for (int i = 0; i < zones.Length; i++)
                zones[i] = ZoneState.CreateLocked(def.Zones[i]);

            foreach (SaveZone saved in data.zones)
            {
                if (saved == null || saved.openCount < 0)
                    return SaveLoadResult.Invalid;
                if (def.TryGetZoneIndex(saved.zoneId, out int index))
                    RestoreZone(saved, def, def.Zones[index], zones[index]);
            }

            ReadLevel(data, def.Levels, out int level, out long xp);
            state = new FarmState
            {
                Coins = data.coins,
                Level = level,
                Xp = xp,
                Seeds = seeds,
                Inventory = inventory,
                Zones = zones,
                // Số ô, đơn có cây không còn: Farm chỉnh khi nạp.
                Orders = FromSaveOrders(data.orders),
                RandomState = (uint)data.randomState,
            };
            return SaveLoadResult.Ok;
        }

        private static SaveOrder[] ToSaveOrders(OrderState[] orders)
        {
            if (orders == null)
                return Array.Empty<SaveOrder>();

            var result = new SaveOrder[orders.Length];
            for (int i = 0; i < orders.Length; i++)
            {
                OrderState order = orders[i] ?? new OrderState();
                OrderItem[] items = order.Items ?? Array.Empty<OrderItem>();
                var savedItems = new SaveCount[items.Length];
                for (int j = 0; j < items.Length; j++)
                    savedItems[j] = new SaveCount { id = items[j].CropId, count = items[j].Count };

                result[i] = new SaveOrder
                {
                    customer = order.Customer,
                    items = savedItems,
                    rewardCoins = order.RewardCoins,
                    rewardXp = order.RewardXp,
                    availableAt = order.AvailableAt,
                };
            }
            return result;
        }

        // Đơn hỏng (món null, số lượng ≤ 0) thì để ô trống, không làm hỏng cả save.
        private static OrderState[] FromSaveOrders(SaveOrder[] saved)
        {
            if (saved == null)
                return Array.Empty<OrderState>();

            var result = new OrderState[saved.Length];
            for (int i = 0; i < saved.Length; i++)
            {
                var order = new OrderState();
                result[i] = order;
                SaveOrder s = saved[i];
                if (s == null)
                    continue;

                order.AvailableAt = s.availableAt;
                if (s.items == null || s.items.Length == 0)
                    continue;

                var items = new OrderItem[s.items.Length];
                bool valid = true;
                for (int j = 0; j < items.Length && valid; j++)
                {
                    SaveCount item = s.items[j];
                    valid = item != null && !string.IsNullOrEmpty(item.id) && item.count > 0;
                    if (valid)
                        items[j] = new OrderItem { CropId = item.id, Count = item.count };
                }
                if (!valid)
                    continue;

                order.Customer = s.customer;
                order.Items = items;
                order.RewardCoins = s.rewardCoins;
                order.RewardXp = s.rewardXp;
            }
            return result;
        }

        // Cấp ngoài bảng (data đổi, bảng cấp ngắn lại) thì kẹp về cấp tối đa; XP không vượt ngưỡng lên cấp.
        private static void ReadLevel(SaveData data, LevelDef levels, out int level, out long xp)
        {
            level = data.level < 1 ? 1 : data.level;
            if (level > levels.MaxLevel)
                level = levels.MaxLevel;

            int need = levels.GetXpToNext(level);
            xp = need == 0 ? 0 : Math.Min(data.xp, need - 1L);
        }

        private static SaveZone ToSaveZone(ZoneState zone, ZoneDef def)
        {
            var plots = new List<SavePlot>();
            for (int i = 0; i < zone.Plots.Length; i++)
            {
                PlotState plot = zone.Plots[i];
                if (plot.Status != PlotStatus.Growing)
                    continue;

                PlotId id = def.PlotIdAt(i);
                plots.Add(new SavePlot
                {
                    bed = id.BedIndex,
                    col = id.Col,
                    row = id.Row,
                    crop = plot.CropId,
                    plantedAt = plot.PlantedAt,
                    watered = plot.Watered,
                });
            }

            return new SaveZone
            {
                zoneId = zone.ZoneId,
                openCount = zone.OpenCount,
                plots = plots.ToArray(),
            };
        }

        private static void RestoreZone(SaveZone saved, FarmDef farm, ZoneDef def, ZoneState zone)
        {
            int openCount = saved.openCount < def.PlotCount ? saved.openCount : def.PlotCount;
            while (zone.OpenCount < openCount)
                zone.OpenNext();

            if (saved.plots == null)
                return;

            foreach (SavePlot plot in saved.plots)
            {
                if (plot == null
                    || !def.TryGetPlotIndex(plot.bed, plot.col, plot.row, out int index)
                    || index >= zone.OpenCount
                    || !farm.TryGetCrop(plot.crop, out _))
                    continue;

                PlotState target = zone.Plots[index];
                target.Status = PlotStatus.Growing;
                target.CropId = plot.crop;
                target.PlantedAt = plot.plantedAt;
                target.Watered = plot.watered;
            }
        }

        private static SaveCount[] ToSaveCounts(Dictionary<string, int> counts)
        {
            var result = new SaveCount[counts.Count];
            int i = 0;
            foreach (KeyValuePair<string, int> pair in counts)
                result[i++] = new SaveCount { id = pair.Key, count = pair.Value };
            return result;
        }

        // Giữ cả cây chưa bật trong data, để khi bật lại người chơi không mất hạt và nông sản.
        private static bool TryReadCounts(SaveCount[] saved, Dictionary<string, int> counts)
        {
            if (saved == null)
                return true;

            foreach (SaveCount item in saved)
            {
                if (item == null || string.IsNullOrEmpty(item.id) || item.count < 0)
                    return false;
                if (item.count > 0)
                    counts[item.id] = item.count;
            }
            return true;
        }
    }
}
