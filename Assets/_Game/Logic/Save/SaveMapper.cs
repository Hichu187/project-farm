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
                character = ToSaveCharacter(state.Character),
                orderDay = state.OrderDay,
                deliveredToday = state.DeliveredToday,
                lastSeenDayKey = state.LastSeenDayKey,
                mailbox = ToSaveMailbox(state.Mailbox),
                checkIn = ToSaveCheckIn(state.CheckIn),
                fishing = ToSaveFishing(state.Fishing),
                orderLines = ToSaveCounts(state.LastOrderLines),
                lastThanks = state.LastThanksLine,
            };
        }

        public static SaveLoadResult TryFromSave(SaveData data, FarmDef def, out FarmState state)
        {
            state = null;
            if (data == null || data.version < 1)
                return SaveLoadResult.Invalid;
            if (data.version > SaveData.CurrentVersion)
                return SaveLoadResult.NewerVersion;
            if (data.version < SaveData.MinSupportedVersion)
                return SaveLoadResult.Outdated;
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
            // Chỉ số câu (≥ 0, kể cả 0) – không dùng ReadCountsLenient vì hàm đó bỏ số 0.
            var orderLines = new Dictionary<string, int>();
            if (data.orderLines != null)
            {
                foreach (SaveCount item in data.orderLines)
                {
                    if (item != null && !string.IsNullOrEmpty(item.id) && item.count >= 0)
                        orderLines[item.id] = item.count;
                }
            }
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
                Character = FromSaveCharacter(data.character),
                OrderDay = data.orderDay > 0 ? data.orderDay : 0,
                DeliveredToday = data.deliveredToday > 0 ? data.deliveredToday : 0,
                LastSeenDayKey = data.lastSeenDayKey,
                Mailbox = FromSaveMailbox(data.mailbox),
                CheckIn = FromSaveCheckIn(data.checkIn),
                Fishing = FromSaveFishing(data.fishing),
                LastOrderLines = orderLines,
                LastThanksLine = data.lastThanks >= 0 ? data.lastThanks : -1,
            };
            return SaveLoadResult.Ok;
        }

        private static SaveFishing ToSaveFishing(FishingState fishing)
        {
            fishing = fishing ?? new FishingState();
            var rods = new List<SaveRod>();
            foreach (RodState rod in fishing.Rods)
            {
                if (rod != null)
                    rods.Add(new SaveRod { id = rod.Id, durability = rod.Durability });
            }
            var basket = new List<SaveFish>();
            foreach (CaughtFish fish in fishing.Basket)
            {
                if (fish != null)
                    basket.Add(new SaveFish
                    {
                        id = fish.FishId,
                        grams = fish.Grams,
                        caughtAt = fish.CaughtAt,
                        slot = (int)fish.Slot,
                        shiny = (int)fish.Shiny,
                    });
            }
            var shinyDex = new List<SaveShiny>();
            foreach (ShinyRecord record in fishing.ShinyDex)
            {
                if (record != null)
                    shinyDex.Add(new SaveShiny { fishId = record.FishId, kind = (int)record.Kind, firstCaughtAt = record.FirstCaughtAt });
            }
            // Album / rác: sắp theo id để file lưu ổn định.
            var dex = new List<SaveDexFish>();
            foreach (KeyValuePair<string, FishDexEntry> pair in fishing.FishDex)
            {
                if (pair.Value != null)
                    dex.Add(new SaveDexFish { id = pair.Key, count = pair.Value.Count, maxGrams = pair.Value.MaxGrams, firstDay = pair.Value.FirstDayKey });
            }
            dex.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
            var junkDex = new List<SaveDexJunk>();
            foreach (KeyValuePair<string, JunkDexEntry> pair in fishing.JunkDex)
            {
                if (pair.Value != null)
                    junkDex.Add(new SaveDexJunk { id = pair.Key, count = pair.Value.Count, firstDay = pair.Value.FirstDayKey });
            }
            junkDex.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
            var junk = new List<SaveJunk>();
            foreach (CaughtJunk item in fishing.JunkBasket)
            {
                if (item != null)
                    junk.Add(new SaveJunk { id = item.JunkId, caughtAt = item.CaughtAt });
            }
            return new SaveFishing
            {
                rods = rods.ToArray(),
                equippedRod = fishing.EquippedRodId,
                baits = ToSaveCounts(fishing.Baits),
                basket = basket.ToArray(),
                basketSlots = fishing.BasketSlots,
                starterGranted = fishing.StarterGranted,
                shinyDex = shinyDex.ToArray(),
                dex = dex.ToArray(),
                junk = junk.ToArray(),
                junkDex = junkDex.ToArray(),
                autoSellJunk = fishing.AutoSellJunk,
            };
        }

        // Không có fishing: trạng thái trống, Farm tặng cần + mồi khi mở câu cá.
        // Mục hỏng (id rỗng, số âm) thì bỏ qua, không làm hỏng cả save.
        private static FishingState FromSaveFishing(SaveFishing saved)
        {
            var fishing = new FishingState();
            if (saved == null)
                return fishing;

            if (saved.rods != null)
            {
                foreach (SaveRod rod in saved.rods)
                {
                    if (rod != null && !string.IsNullOrEmpty(rod.id) && !fishing.TryGetRod(rod.id, out _))
                        fishing.Rods.Add(new RodState
                        {
                            Id = rod.id,
                            Durability = Math.Max(0, rod.durability),
                        });
                }
            }
            if (saved.basket != null)
            {
                foreach (SaveFish fish in saved.basket)
                {
                    if (fish == null || string.IsNullOrEmpty(fish.id) || fish.grams <= 0)
                        continue;
                    int slot = fish.slot >= 0 && fish.slot < DaySlots.Count ? fish.slot : 0;
                    fishing.Basket.Add(new CaughtFish
                    {
                        FishId = fish.id,
                        Grams = fish.grams,
                        CaughtAt = fish.caughtAt,
                        Slot = (DaySlot)slot,
                        Shiny = ToShinyKind(fish.shiny),
                    });
                }
            }
            if (saved.shinyDex != null)
            {
                foreach (SaveShiny record in saved.shinyDex)
                {
                    ShinyKind kind = record != null ? ToShinyKind(record.kind) : ShinyKind.None;
                    if (kind == ShinyKind.None || string.IsNullOrEmpty(record.fishId) || fishing.HasShiny(record.fishId, kind))
                        continue;
                    fishing.ShinyDex.Add(new ShinyRecord { FishId = record.fishId, Kind = kind, FirstCaughtAt = record.firstCaughtAt });
                }
            }
            if (saved.dex != null)
            {
                foreach (SaveDexFish entry in saved.dex)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.id) || entry.count <= 0 || entry.maxGrams <= 0)
                        continue;
                    fishing.FishDex[entry.id] = new FishDexEntry { Count = entry.count, MaxGrams = entry.maxGrams, FirstDayKey = entry.firstDay };
                }
            }
            if (saved.junkDex != null)
            {
                foreach (SaveDexJunk entry in saved.junkDex)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.id) || entry.count <= 0)
                        continue;
                    fishing.JunkDex[entry.id] = new JunkDexEntry { Count = entry.count, FirstDayKey = entry.firstDay };
                }
            }
            if (saved.junk != null)
            {
                foreach (SaveJunk item in saved.junk)
                {
                    if (item != null && !string.IsNullOrEmpty(item.id))
                        fishing.JunkBasket.Add(new CaughtJunk { JunkId = item.id, CaughtAt = item.caughtAt });
                }
            }
            fishing.AutoSellJunk = saved.autoSellJunk;
            ReadCountsLenient(saved.baits, fishing.Baits);
            fishing.EquippedRodId = string.IsNullOrEmpty(saved.equippedRod) ? null : saved.equippedRod;
            // Farm chuẩn hóa về khoảng hợp lệ của data hiện tại.
            fishing.BasketSlots = saved.basketSlots;
            fishing.StarterGranted = saved.starterGranted;
            return fishing;
        }

        private static SaveCheckIn ToSaveCheckIn(CheckInState state)
        {
            state = state ?? new CheckInState();
            return new SaveCheckIn
            {
                nextDay = state.NextDay,
                cycle = state.Cycle,
                lastDay = state.LastClaimDayKey,
                lastUtc = state.LastClaimUtc,
            };
        }

        // Không có: chưa điểm danh lần nào. Ngày / vòng sai thì Farm chuẩn hóa lúc nạp.
        private static CheckInState FromSaveCheckIn(SaveCheckIn saved)
        {
            if (saved == null)
                return new CheckInState();
            return new CheckInState
            {
                NextDay = saved.nextDay,
                Cycle = saved.cycle,
                LastClaimDayKey = saved.lastDay,
                LastClaimUtc = saved.lastUtc,
            };
        }

        private static SaveMailbox ToSaveMailbox(MailboxState box)
        {
            box = box ?? new MailboxState();
            var mails = new List<SaveMail>();
            foreach (Mail mail in box.Mails)
            {
                if (mail == null)
                    continue;
                var rewards = new List<SaveReward>();
                foreach (Reward reward in mail.Rewards ?? new List<Reward>())
                {
                    if (reward != null)
                        rewards.Add(new SaveReward { kind = (int)reward.Kind, id = reward.Id, amount = reward.Amount });
                }
                mails.Add(new SaveMail
                {
                    id = mail.Id,
                    sender = mail.Sender,
                    title = mail.Title,
                    body = mail.Body,
                    rewards = rewards.ToArray(),
                    createdDayKey = mail.CreatedDayKey,
                    expiresDayKey = mail.ExpiresDayKey,
                    read = mail.Read,
                    claimed = mail.Claimed,
                    claimedDayKey = mail.ClaimedDayKey,
                });
            }
            var sent = new List<string>(box.SentIds);
            sent.Sort(StringComparer.Ordinal);
            return new SaveMailbox { mails = mails.ToArray(), sentIds = sent.ToArray(), lastRefreshDayKey = box.LastRefreshDayKey };
        }

        // Thư hỏng (không id) bỏ qua; quà loại lạ bỏ qua; Farm bỏ tiếp quà trỏ tới cây / mồi không còn.
        private static MailboxState FromSaveMailbox(SaveMailbox saved)
        {
            var box = new MailboxState();
            if (saved == null)
                return box;
            if (saved.sentIds != null)
            {
                foreach (string id in saved.sentIds)
                {
                    if (!string.IsNullOrEmpty(id))
                        box.SentIds.Add(id);
                }
            }
            if (saved.mails != null)
            {
                foreach (SaveMail s in saved.mails)
                {
                    if (s == null || string.IsNullOrEmpty(s.id) || box.TryGet(s.id, out _))
                        continue;
                    var mail = new Mail
                    {
                        Id = s.id,
                        Sender = s.sender ?? "",
                        Title = s.title ?? "",
                        Body = s.body ?? "",
                        CreatedDayKey = s.createdDayKey,
                        ExpiresDayKey = s.expiresDayKey,
                        Read = s.read,
                        Claimed = s.claimed,
                        ClaimedDayKey = s.claimedDayKey,
                    };
                    if (s.rewards != null)
                    {
                        foreach (SaveReward r in s.rewards)
                        {
                            if (r != null && Enum.IsDefined(typeof(RewardKind), r.kind))
                                mail.Rewards.Add(new Reward((RewardKind)r.kind, string.IsNullOrEmpty(r.id) ? null : r.id, r.amount));
                        }
                    }
                    box.Mails.Add(mail);
                    box.SentIds.Add(mail.Id);
                }
            }
            box.LastRefreshDayKey = saved.lastRefreshDayKey;
            return box;
        }

        // Kiểu shiny lạ (bản sau thêm, hoặc save hỏng) → None.
        private static ShinyKind ToShinyKind(int value)
            => Enum.IsDefined(typeof(ShinyKind), value) ? (ShinyKind)value : ShinyKind.None;

        private static void ReadCountsLenient(SaveCount[] saved, Dictionary<string, int> counts)
        {
            if (saved == null)
                return;
            foreach (SaveCount item in saved)
            {
                if (item != null && !string.IsNullOrEmpty(item.id) && item.count > 0)
                    counts[item.id] = item.count;
            }
        }

        private static SaveCharacter ToSaveCharacter(CharacterLook look)
        {
            look = look ?? new CharacterLook();
            return new SaveCharacter
            {
                created = look.Created,
                name = look.Name ?? "",
                hairStyle = look.HairStyle,
                hairColor = look.HairColor,
            };
        }

        // Save cũ (không có character) hoặc tên hỏng: coi như chưa tạo nhân vật, người chơi tạo lại.
        private static CharacterLook FromSaveCharacter(SaveCharacter saved)
        {
            var look = new CharacterLook();
            if (saved == null)
                return look;

            if (!string.IsNullOrEmpty(saved.hairStyle))
                look.HairStyle = saved.hairStyle;
            if (!string.IsNullOrEmpty(saved.hairColor))
                look.HairColor = saved.hairColor;
            if (saved.created && CharacterLook.TryNormalizeName(saved.name, out string name))
            {
                look.Name = name;
                look.Created = true;
            }
            return look;
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
                    savedItems[j] = new SaveCount { id = items[j].Id, count = items[j].Count };

                result[i] = new SaveOrder
                {
                    kind = (int)order.Kind,
                    npc = order.NpcId,
                    line = order.Line,
                    seed = order.Seed,
                    items = savedItems,
                    rewardCoins = order.RewardCoins,
                    rewardXp = order.RewardXp,
                    availableAt = order.AvailableAt,
                    rank = (int)order.Rank,
                    expiresAt = order.ExpiresAt,
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
                // Hạng lạ (save sửa tay / bản sau) coi như đơn thường.
                order.Rank = s.rank == (int)OrderRank.Urgent ? OrderRank.Urgent : OrderRank.Normal;
                if (s.items == null || s.items.Length == 0)
                    continue;

                var items = new OrderItem[s.items.Length];
                bool valid = true;
                for (int j = 0; j < items.Length && valid; j++)
                {
                    SaveCount item = s.items[j];
                    valid = item != null && !string.IsNullOrEmpty(item.id) && item.count > 0;
                    if (valid)
                        items[j] = new OrderItem { Id = item.id, Count = item.count };
                }
                if (!valid)
                    continue;

                // Loại lạ coi như đơn nông sản (Farm bỏ đơn nếu id không phải cây). Người / câu lạ Farm bỏ khi nạp.
                order.Kind = s.kind == (int)OrderKind.Fish ? OrderKind.Fish : OrderKind.Crops;
                order.NpcId = string.IsNullOrEmpty(s.npc) ? null : s.npc;
                order.Line = s.line >= 0 ? s.line : -1;
                order.Seed = (uint)s.seed;
                order.Items = items;
                order.RewardCoins = s.rewardCoins;
                order.RewardXp = s.rewardXp;
                order.ExpiresAt = s.expiresAt > 0 ? s.expiresAt : 0;
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
                    growSeconds = plot.GrowSeconds,
                    bonusYield = plot.BonusYield,
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
                // Chốt lúc gieo (Đợt 2 Phần 7); số âm coi như không có.
                target.GrowSeconds = Math.Max(0L, plot.growSeconds);
                target.BonusYield = Math.Max(0, plot.bonusYield);
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
