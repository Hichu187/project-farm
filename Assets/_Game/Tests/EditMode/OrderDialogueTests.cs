using System;
using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    // Đợt 2 Phần 11: đơn hàng có thoại (người đặt, câu, món, cảm ơn) và đơn cá.
    public class OrderDialogueTests
    {
        private const string Zone1 = "khu1";
        private const string RauMuong = "rau_muong";
        private const string CaiXanh = "cai_xanh";
        private const long GrowSeconds = 60;
        private const int Slots = 3;
        private const int CoinPercent = 160;
        private const int XpPercent = 200;

        private const string BaLua = "ba_lua";
        private const string CoHanh = "co_hanh";
        private const string CuTi = "cu_ti";

        private const string Ro = "ca_ro";        // Thường, mọi giờ → đặt được.
        private const string Diec = "ca_diec";    // Khá, 3 khung giờ → đặt được.
        private const string Loc = "ca_loc";      // Hiếm → không đặt.
        private const string Tre = "ca_tre";      // Thường nhưng chỉ 2 khung giờ → không đặt.
        private const string Can = "can_tre";
        private const string Giun = "giun";

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock { UtcNowSeconds = 1798761600 + 8 * 3600 };
        }

        // ---------- CSV, điền chỗ trống ----------

        [Test]
        public void ParseCsv_QuotesCommentsBlankLines()
        {
            string csv = "﻿npcId,kind,text,cropTag\r\n"
                + "ba_lua,Request,\"{title} cần {qty} {crop}, cháu nhé!\",\r\n"
                + "\r\n"
                + "# ghi chú\r\n"
                + ",thanks,\"Cảm ơn \"\"cháu\"\"!\"\n"
                + "co_hanh,URGENT,Gấp lắm,rau_muong";

            List<OrderLineDef> lines = OrderDialogue.ParseLinesCsv(csv);

            Assert.AreEqual(3, lines.Count);
            Assert.AreEqual(BaLua, lines[0].NpcId);
            Assert.AreEqual(OrderLineKind.Request, lines[0].Kind);
            Assert.AreEqual("{title} cần {qty} {crop}, cháu nhé!", lines[0].Text);
            Assert.IsNull(lines[0].CropTag);
            Assert.IsNull(lines[1].NpcId);
            Assert.AreEqual(OrderLineKind.Thanks, lines[1].Kind);
            Assert.AreEqual("Cảm ơn \"cháu\"!", lines[1].Text);
            Assert.AreEqual(OrderLineKind.Urgent, lines[2].Kind);
            Assert.AreEqual(RauMuong, lines[2].CropTag);
        }

        [Test]
        public void ParseCsv_UnknownKind_Throws()
        {
            Assert.Throws<FormatException>(() => OrderDialogue.ParseLinesCsv("npcId,kind,text\nba_lua,Hoi,Chào"));
        }

        [Test]
        public void Format_FillsPlaceholders()
        {
            Assert.AreEqual("Bà cần 4 rau muống về nấu canh cua, cháu nhé!",
                OrderDialogue.Format("{title} cần {qty} {crop} về nấu {dish}, cháu nhé!", "bà", 4, "rau muống", "canh cua"));
            // Giữa câu giữ chữ thường; đầu câu là tên cây thì viết hoa.
            Assert.AreEqual("Cho em 2 cá rô nhé!", OrderDialogue.Format("Cho {title} {qty} {crop} nhé!", "em", 2, "cá rô", null));
            Assert.AreEqual("Rau muống tươi quá!", OrderDialogue.Format("{crop} tươi quá!", "bà", 1, "rau muống", null));
            // Sau . ! ? cũng viết hoa; "1.5" không bị ảnh hưởng vì sau dấu chấm là số.
            Assert.AreEqual("Khách đông quá! Cô cần 2 cà chua. Nhanh nhé?", OrderDialogue.Format(
                "khách đông quá! {title} cần {qty} {crop}. nhanh nhé?", "cô", 2, "cà chua", null));
            // Không có món: {dish} lấy tên cây.
            Assert.AreEqual("Nấu rau muống", OrderDialogue.Format("Nấu {dish}", "", 1, "rau muống", null));
        }

        [Test]
        public void Dialogue_LineOfUnknownNpc_Throws()
        {
            var lines = new[] { new OrderLineDef("khong_co", OrderLineKind.Request, "Chào") };
            Assert.Throws<ArgumentException>(() => new OrderDialogue(new[] { Npc(BaLua) }, lines, null));
        }

        // ---------- Người đặt, câu thoại ----------

        [Test]
        public void Orders_HaveNpcAndRequestLine()
        {
            Farm farm = CreateFarm(Dialogue());
            foreach (OrderState order in farm.State.Orders)
            {
                Assert.IsTrue(order.HasOrder);
                Assert.IsTrue(farm.Definition.Orders.Dialogue.TryGetNpc(order.NpcId, out NpcDef npc));
                Assert.IsTrue(farm.Definition.Orders.Dialogue.TryGetLine(order.Line, out OrderLineDef line));
                Assert.AreEqual(OrderLineKind.Request, line.Kind);
                Assert.That(line.NpcId == null || line.NpcId == npc.Id);
            }
        }

        [Test]
        public void SameNpc_NeverRepeatsLineBackToBack()
        {
            // 1 người, 3 câu: 200 đơn liên tiếp không có 2 câu liền nhau trùng.
            var npcs = new[] { Npc(BaLua) };
            var lines = new[]
            {
                new OrderLineDef(BaLua, OrderLineKind.Request, "Câu 1"),
                new OrderLineDef(BaLua, OrderLineKind.Request, "Câu 2"),
                new OrderLineDef(BaLua, OrderLineKind.Request, "Câu 3"),
            };
            Farm farm = CreateFarm(new OrderDialogue(npcs, lines, null), slots: 1);
            int last = farm.State.Orders[0].Line;
            var seen = new HashSet<int> { last };
            for (int i = 0; i < 200; i++)
            {
                Assert.AreEqual(FarmResult.Ok, farm.DiscardOrder(0));
                _clock.Advance(1);
                farm.RefreshOrders();
                int line = farm.State.Orders[0].Line;
                Assert.AreNotEqual(last, line, $"Đơn {i}");
                seen.Add(line);
                last = line;
            }
            Assert.AreEqual(3, seen.Count);
        }

        [Test]
        public void DishLine_OnlyWhenCropHasDish()
        {
            var npcs = new[] { Npc(BaLua) };
            var lines = new[] { new OrderLineDef(BaLua, OrderLineKind.Request, "Nấu {dish}") };
            var dishes = new Dictionary<string, IReadOnlyList<string>> { { CaiXanh, new[] { "canh cải" } } };
            Farm farm = CreateFarm(new OrderDialogue(npcs, lines, dishes), slots: 1);
            for (int i = 0; i < 50; i++)
            {
                OrderState order = farm.State.Orders[0];
                bool hasDish = order.Items[0].Id == CaiXanh;
                Assert.AreEqual(hasDish ? 0 : -1, order.Line, order.Items[0].Id);
                farm.DiscardOrder(0);
                _clock.Advance(1);
                farm.RefreshOrders();
            }
        }

        [Test]
        public void UrgentOrder_UsesUrgentLine()
        {
            var npcs = new[] { Npc(BaLua) };
            var lines = new[]
            {
                new OrderLineDef(BaLua, OrderLineKind.Request, "Thường"),
                new OrderLineDef(BaLua, OrderLineKind.Urgent, "Gấp"),
            };
            Farm farm = CreateFarm(new OrderDialogue(npcs, lines, null), urgentChance: 100);
            foreach (OrderState order in farm.State.Orders)
            {
                Assert.IsTrue(order.IsUrgent);
                Assert.AreEqual(1, order.Line);
            }
        }

        [Test]
        public void PreferredCrop_NpcChosenMoreOften()
        {
            // Cô Hạnh hay đặt rau muống: đơn chỉ có rau muống (cấp 1) → Cô Hạnh ×3 so với Bà Lụa.
            var npcs = new[] { Npc(BaLua), new NpcDef(CoHanh, "Cô Hạnh", "Cô", new[] { RauMuong }, 1) };
            Farm farm = CreateFarm(new OrderDialogue(npcs, null, null), slots: 1, level: 1);
            int hanh = 0;
            const int Count = 600;
            for (int i = 0; i < Count; i++)
            {
                Assert.AreEqual(RauMuong, farm.State.Orders[0].Items[0].Id);
                if (farm.State.Orders[0].NpcId == CoHanh)
                    hanh++;
                farm.DiscardOrder(0);
                _clock.Advance(1);
                farm.RefreshOrders();
            }
            Assert.AreEqual(0.75, hanh / (double)Count, 0.06);
        }

        [Test]
        public void Reload_KeepsNpcLineAndSeed()
        {
            Farm farm = CreateFarm(Dialogue());
            Farm loaded = RoundTrip(farm);
            for (int i = 0; i < Slots; i++)
            {
                OrderState a = farm.State.Orders[i];
                OrderState b = loaded.State.Orders[i];
                Assert.AreEqual(a.NpcId, b.NpcId);
                Assert.AreEqual(a.Line, b.Line);
                Assert.AreEqual(a.Seed, b.Seed);
                Assert.AreEqual(a.Kind, b.Kind);
            }
            CollectionAssert.AreEquivalent(farm.State.LastOrderLines, loaded.State.LastOrderLines);
        }

        [Test]
        public void Deliver_RaisesThanks_NoBackToBackRepeat()
        {
            var npcs = new[] { Npc(BaLua) };
            var lines = new[]
            {
                new OrderLineDef(BaLua, OrderLineKind.Request, "Đặt"),
                new OrderLineDef(null, OrderLineKind.Thanks, "Cảm ơn 1"),
                new OrderLineDef(null, OrderLineKind.Thanks, "Cảm ơn 2"),
            };
            Farm farm = CreateFarm(new OrderDialogue(npcs, lines, null), slots: 1);
            var thanks = new List<OrderDelivery>();
            farm.OrderDelivered += d => thanks.Add(d);
            for (int i = 0; i < 20; i++)
            {
                OrderState order = farm.State.Orders[0];
                foreach (OrderItem item in order.Items)
                    farm.State.Inventory[item.Id] = item.Count;
                Assert.AreEqual(FarmResult.Ok, farm.DeliverOrder(0));
                _clock.Advance(1);
                farm.RefreshOrders();
            }
            Assert.AreEqual(20, thanks.Count);
            for (int i = 0; i < thanks.Count; i++)
            {
                Assert.AreEqual(BaLua, thanks[i].NpcId);
                Assert.That(thanks[i].ThanksLine, Is.EqualTo(1).Or.EqualTo(2));
                if (i > 0)
                    Assert.AreNotEqual(thanks[i - 1].ThanksLine, thanks[i].ThanksLine);
            }
        }

        // ---------- Đơn cá ----------

        [Test]
        public void FishPool_LowRarityAndEnoughSlots()
        {
            Farm farm = CreateFarm(Dialogue(), fishPercent: 100);
            var ids = new List<string>();
            foreach (FishDef fish in farm.GetFishOrderPool())
                ids.Add(fish.Id);
            CollectionAssert.AreEquivalent(new[] { Ro, Diec }, ids);
        }

        // Phần 5: album rỗng → không có loài nào đặt được, không ra đơn cá, chuỗi đơn giống khi tắt đơn cá.
        [Test]
        public void FishPool_OnlyCaughtSpecies()
        {
            Farm empty = CreateFarm(Dialogue(), fishPercent: 100, dexAll: false);
            CollectionAssert.IsEmpty(empty.GetFishOrderPool());
            Farm plain = CreateFarm(Dialogue(), fishPercent: 0, dexAll: false);
            for (int i = 0; i < Slots; i++)
            {
                Assert.IsFalse(empty.State.Orders[i].IsFish);
                Assert.AreEqual(plain.State.Orders[i].Items[0].Id, empty.State.Orders[i].Items[0].Id);
            }

            empty.State.Fishing.FishDex[Diec] = new FishDexEntry { Count = 1, MaxGrams = 100 };
            var ids = new List<string>();
            foreach (FishDef fish in empty.GetFishOrderPool())
                ids.Add(fish.Id);
            CollectionAssert.AreEquivalent(new[] { Diec }, ids);
        }

        [Test]
        public void FishOrders_ValidSpeciesCountAndNpc()
        {
            Farm farm = CreateFarm(Dialogue(), fishPercent: 100, slots: 1, level: 7);
            for (int i = 0; i < 100; i++)
            {
                OrderState order = farm.State.Orders[0];
                Assert.IsTrue(order.IsFish);
                Assert.IsFalse(order.IsUrgent);
                Assert.AreEqual(1, order.Items.Length);
                Assert.That(order.Items[0].Id, Is.EqualTo(Ro).Or.EqualTo(Diec));
                // Cấp 7, mở câu cá cấp 1: tối đa 1 + 6 / 3 = 3 con.
                Assert.That(order.Items[0].Count, Is.InRange(1, 3));
                Assert.AreEqual(CuTi, order.NpcId, "Chỉ Cu Tí đặt đơn cá");
                farm.Definition.Orders.Dialogue.TryGetLine(order.Line, out OrderLineDef line);
                Assert.AreEqual(OrderLineKind.Fish, line.Kind);
                farm.DiscardOrder(0);
                _clock.Advance(1);
                farm.RefreshOrders();
            }
        }

        [Test]
        public void FishOrders_NeverUrgent()
        {
            Farm farm = CreateFarm(Dialogue(), fishPercent: 100, urgentChance: 100);
            foreach (OrderState order in farm.State.Orders)
            {
                Assert.IsTrue(order.IsUrgent);
                Assert.IsFalse(order.IsFish);
            }
        }

        [Test]
        public void FishOrders_NotBeforeFishingUnlocked_AndSameSequence()
        {
            // Câu cá mở ở cấp 5, người chơi cấp 1: không có đơn cá, chuỗi đơn giống hệt khi không có đơn cá.
            Farm withFish = CreateFarm(Dialogue(), fishPercent: 100, fishingLevel: 5);
            Farm plain = CreateFarm(Dialogue(), fishPercent: 0, fishingLevel: 5);
            for (int i = 0; i < Slots; i++)
            {
                Assert.IsFalse(withFish.State.Orders[i].IsFish);
                Assert.AreEqual(plain.State.Orders[i].Items[0].Id, withFish.State.Orders[i].Items[0].Id);
                Assert.AreEqual(plain.State.Orders[i].NpcId, withFish.State.Orders[i].NpcId);
            }
        }

        [Test]
        public void FishOrderPercent_RisesPerLevel_Capped()
        {
            var def = new OrderDef(1, 1, 100, 100, 0, null, fishOrderMinPercent: 10, fishOrderMaxPercent: 20);
            Assert.AreEqual(10, def.GetFishOrderPercent(0));
            Assert.AreEqual(15, def.GetFishOrderPercent(5));
            Assert.AreEqual(20, def.GetFishOrderPercent(10));
            Assert.AreEqual(20, def.GetFishOrderPercent(30));
            Assert.AreEqual(1, def.GetFishOrderMaxCount(0));
            Assert.AreEqual(2, def.GetFishOrderMaxCount(3));
            Assert.AreEqual(3, def.GetFishOrderMaxCount(20));
        }

        [Test]
        public void FishOrder_RewardFromBasePrice()
        {
            Farm farm = CreateFarm(Dialogue(), fishPercent: 100, slots: 1, level: 7);
            OrderState order = farm.State.Orders[0];
            farm.Definition.Fishing.TryGetFish(order.Items[0].Id, out FishDef fish);
            long coins = (long)order.Items[0].Count * fish.BasePrice;
            long xp = (long)order.Items[0].Count * fish.BaseXp;
            Assert.AreEqual(Math.Max(5L, (coins * CoinPercent * 100 + 25000) / 50000 * 5), order.RewardCoins);
            Assert.AreEqual(Math.Max(1L, (xp * XpPercent * 100 + 5000) / 10000), order.RewardXp);
        }

        [Test]
        public void FishOrder_Deliver_NeedsNormalFish_TakesLightest()
        {
            Farm farm = CreateFarm(Dialogue(), fishPercent: 100, slots: 1);
            OrderState order = farm.State.Orders[0];
            string id = order.Items[0].Id;
            Assert.AreEqual(1, order.Items[0].Count, "Cấp 1: 1 con");
            List<CaughtFish> basket = farm.State.Fishing.Basket;

            // Chỉ có cá shiny: không giao được.
            basket.Add(new CaughtFish { FishId = id, Grams = 100, Shiny = ShinyKind.Gold });
            Assert.IsFalse(farm.CanDeliver(0));
            Assert.AreEqual(FarmResult.NotEnoughItems, farm.DeliverOrder(0));

            basket.Add(new CaughtFish { FishId = id, Grams = 900 });
            basket.Add(new CaughtFish { FishId = id, Grams = 300 });
            Assert.AreEqual(2, farm.GetOrderItemHave(order, order.Items[0]));
            long coins = farm.State.Coins;
            long reward = order.RewardCoins;

            Assert.AreEqual(FarmResult.Ok, farm.DeliverOrder(0));

            Assert.AreEqual(coins + reward, farm.State.Coins);
            Assert.AreEqual(2, basket.Count);
            Assert.AreEqual(ShinyKind.Gold, basket[0].Shiny);
            Assert.AreEqual(900, basket[1].Grams, "Giữ con nặng, giao con nhẹ");
        }

        [Test]
        public void SellAll_KeepsFishNeededForOrders()
        {
            Farm farm = CreateFarm(Dialogue(), fishPercent: 100, slots: 1);
            string id = farm.State.Orders[0].Items[0].Id;
            string other = id == Ro ? Diec : Ro;
            List<CaughtFish> basket = farm.State.Fishing.Basket;
            basket.Add(new CaughtFish { FishId = id, Grams = 500 });
            basket.Add(new CaughtFish { FishId = id, Grams = 200 });
            basket.Add(new CaughtFish { FishId = other, Grams = 200 });

            Assert.AreEqual(2, farm.GetSellAllCount());
            Assert.IsTrue(farm.IsKeptOnSellAll(1));
            Assert.AreEqual(FarmResult.Ok, farm.SellAllFish(out _));

            Assert.AreEqual(1, basket.Count);
            Assert.AreEqual(id, basket[0].FishId);
            Assert.AreEqual(200, basket[0].Grams);
            Assert.IsTrue(farm.CanDeliver(0));
        }

        [Test]
        public void FishOrder_SurvivesReload()
        {
            Farm farm = CreateFarm(Dialogue(), fishPercent: 100);
            Farm loaded = RoundTrip(farm);
            for (int i = 0; i < Slots; i++)
            {
                Assert.IsTrue(loaded.State.Orders[i].IsFish);
                Assert.AreEqual(farm.State.Orders[i].Items[0].Id, loaded.State.Orders[i].Items[0].Id);
            }
        }

        // ---------- Dựng data ----------

        private static NpcDef Npc(string id) => new NpcDef(id, id, "Bà", null, 1);

        private static OrderDialogue Dialogue()
        {
            var npcs = new[]
            {
                new NpcDef(BaLua, "Bà Lụa", "Bà", new[] { CaiXanh }, 2),
                new NpcDef(CoHanh, "Cô Hạnh", "Cô", new[] { RauMuong }, 2),
                new NpcDef(CuTi, "Cu Tí", "Cháu", null, 1, fishWeight: 5),
            };
            var lines = new List<OrderLineDef>();
            foreach (NpcDef npc in npcs)
            {
                for (int i = 0; i < 4; i++)
                    lines.Add(new OrderLineDef(npc.Id, OrderLineKind.Request, $"{npc.Id} đặt {i}: {{qty}} {{crop}}"));
            }
            lines.Add(new OrderLineDef(CuTi, OrderLineKind.Fish, "Cho cháu {qty} con {crop} nhé!"));
            lines.Add(new OrderLineDef(CuTi, OrderLineKind.Fish, "Cháu cần {qty} {crop}!"));
            lines.Add(new OrderLineDef(null, OrderLineKind.Thanks, "Cảm ơn cháu!"));
            return new OrderDialogue(npcs, lines, null);
        }

        private Farm CreateFarm(OrderDialogue dialogue, int slots = Slots, int level = 2, int urgentChance = 0,
            int fishPercent = 0, int fishingLevel = 1, bool dexAll = true)
        {
            var zones = new[] { new ZoneDef(Zone1, new[] { new BedDef(2, 3) }, 2, new[] { 10, 10, 10, 10 }) };
            var crops = new[]
            {
                new CropDef(RauMuong, GrowSeconds, 5, 5, 2, xp: 1),
                new CropDef(CaiXanh, GrowSeconds, 10, 20, 2, xp: 4, unlockLevel: 2),
            };
            var levels = new LevelDef(new[] { 3, 20, 50, 100, 200, 300, 400, 500, 600 }, 0, 0);
            var orders = new OrderDef(slots, 1, CoinPercent, XpPercent, 0, dialogue, 6,
                0, 0, urgentChance, 200, 0, 0, fishPercent, fishPercent);

            float[] allDay = { 1, 1, 1, 1, 1, 1, 1 };
            float[] threeSlots = { 1, 1, 1, 0, 0, 0, 0 };
            float[] twoSlots = { 0, 0, 0, 0, 1, 1, 0 };
            var fish = new[]
            {
                new FishDef(Ro, FishRarity.Common, 80, 300, 10, 1, allDay, FishDepth.All, null, 3, 4, 4f, 0.9f, 0),
                new FishDef(Diec, FishRarity.Uncommon, 100, 400, 20, 2, threeSlots, FishDepth.All, null, 3, 4, 4f, 0.9f, 0),
                new FishDef(Loc, FishRarity.Rare, 500, 2500, 80, 8, allDay, FishDepth.All, null, 5, 7, 3.5f, 0.6f, 15),
                new FishDef(Tre, FishRarity.Common, 100, 500, 15, 1, twoSlots, FishDepth.All, null, 3, 4, 4f, 0.9f, 0),
            };
            var rods = new[] { new RodDef(Can, 1, 0.03f, 1.2f, 0, 0, 0, 30, 0, 1) };
            var baits = new[] { new BaitDef(Giun, 2) };
            var fishing = new FishingDef(fish, rods, baits, Can, Giun, 10, fishingLevel);

            var def = new FarmDef(zones, crops, 100, new Dictionary<string, int> { { RauMuong, 5 } }, 0, levels, RauMuong,
                orders, fishing);
            FarmState state = FarmState.CreateNew(def);
            state.Level = level;
            state.RandomState = 4242;
            // Đơn cá chỉ đặt loài đã có trong album (Phần 5): mặc định album đủ loài để giữ hành vi test cũ.
            if (dexAll)
            {
                foreach (FishDef f in fish)
                    state.Fishing.FishDex[f.Id] = new FishDexEntry { Count = 1, MaxGrams = f.MinGrams };
            }
            return new Farm(def, state, _clock);
        }

        private Farm RoundTrip(Farm farm)
        {
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));
            return new Farm(farm.Definition, loaded, _clock);
        }
    }
}
