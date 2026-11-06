using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    // Đơn hàng làng quê: sinh đơn, giao, bỏ, làm mới, lưu; và một vòng chơi trọn vẹn.
    public class OrderTests
    {
        private const string Zone1 = "khu1";
        private const string Zone2 = "khu2";
        private const string RauMuong = "rau_muong";
        private const string CaiXanh = "cai_xanh";
        private const string BiDo = "bi_do";
        private const long GrowSeconds = 60;
        private const int Slots = 3;
        private const int OrdersLevel = 2;
        private const int BiDoLevel = 4;
        private const int Zone2Level = 3;
        private const int CoinPercent = 160;
        private const int XpPercent = 200;
        private const long Cooldown = 300;
        private const int HarvestCap = 6;
        private static readonly string[] Names = { "Bà Tư", "Chú Ba" };
        private static readonly int[] XpToNext = { 3, 20, 50 };

        private static readonly PlotId Plot0 = new PlotId(Zone1, 0, 0, 0);

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
        }

        // ---------- Mở đơn hàng ----------

        [Test]
        public void Level1_OrdersLockedAndEmpty()
        {
            Farm farm = CreateFarm();

            Assert.IsFalse(farm.OrdersUnlocked);
            Assert.AreEqual(Slots, farm.OrderSlotCount);
            for (int i = 0; i < Slots; i++)
            {
                Assert.IsTrue(farm.TryGetOrder(i, out OrderState order));
                Assert.IsFalse(order.HasOrder);
            }
            Assert.AreEqual(FarmResult.OrdersLocked, farm.DeliverOrder(0));
            Assert.AreEqual(FarmResult.OrdersLocked, farm.DiscardOrder(0));
        }

        [Test]
        public void ReachOrdersLevel_FillsAllSlots()
        {
            Farm farm = CreateFarm();
            farm.State.Xp = XpToNext[0] - 1;

            PlantAndHarvest(farm, RauMuong);

            Assert.AreEqual(OrdersLevel, farm.State.Level);
            Assert.IsTrue(farm.OrdersUnlocked);
            AssertAllSlotsFilled(farm);
        }

        [Test]
        public void LoadAtOrdersLevel_FillsAllSlots()
        {
            Farm farm = CreateFarm(level: OrdersLevel);

            AssertAllSlotsFilled(farm);
        }

        [Test]
        public void NoOrderDef_NoSlots()
        {
            FarmDef def = CreateDef(withOrders: false);
            FarmState state = FarmState.CreateNew(def);
            state.Level = OrdersLevel;
            var farm = new Farm(def, state, _clock);

            Assert.AreEqual(0, farm.OrderSlotCount);
            Assert.IsFalse(farm.OrdersUnlocked);
        }

        [TestCase(-1)]
        [TestCase(Slots)]
        public void UnknownSlot_ReturnsUnknownOrder(int slot)
        {
            Farm farm = CreateFarm(level: OrdersLevel);

            Assert.IsFalse(farm.TryGetOrder(slot, out _));
            Assert.AreEqual(FarmResult.UnknownOrder, farm.DeliverOrder(slot));
            Assert.AreEqual(FarmResult.UnknownOrder, farm.DiscardOrder(slot));
        }

        // ---------- Nội dung đơn ----------

        [TestCase(2)]
        [TestCase(4)]
        [TestCase(7)]
        public void GeneratedOrders_FollowRulesForLevel(int level)
        {
            Farm farm = CreateFarm(level: level);
            OrderDef def = farm.Definition.Orders;

            for (int round = 0; round < 60; round++)
            {
                Assert.IsTrue(farm.TryGetOrder(0, out OrderState order));
                Assert.IsTrue(order.HasOrder);
                Assert.LessOrEqual(order.Items.Length, def.GetMaxItemTypes(level));

                long coins = 0;
                long xp = 0;
                var seen = new HashSet<string>();
                foreach (OrderItem item in order.Items)
                {
                    Assert.IsTrue(seen.Add(item.CropId), "Một đơn không lặp loại cây");
                    Assert.IsTrue(farm.IsCropUnlocked(item.CropId), item.CropId);
                    Assert.IsTrue(farm.Definition.TryGetCrop(item.CropId, out CropDef crop));
                    Assert.AreEqual(0, item.Count % crop.HarvestYield);
                    int harvests = item.Count / crop.HarvestYield;
                    Assert.That(harvests, Is.InRange(1, def.GetMaxHarvests(level)));
                    coins += (long)item.Count * crop.SellPrice;
                    xp += (long)harvests * crop.Xp;
                }

                long expectedCoins = System.Math.Max(5L, (long)System.Math.Round(coins * CoinPercent / 100.0 / 5.0,
                    System.MidpointRounding.AwayFromZero) * 5);
                Assert.AreEqual(expectedCoins, order.RewardCoins);
                Assert.AreEqual(System.Math.Max(1L, xp * XpPercent / 100), order.RewardXp);
                CollectionAssert.Contains(Names, order.Customer);

                Assert.AreEqual(FarmResult.Ok, farm.DiscardOrder(0));
                _clock.Advance(Cooldown);
                Assert.IsTrue(farm.RefreshOrders());
            }
        }

        [Test]
        public void MaxHarvests_GrowsWithLevelUpToCap()
        {
            OrderDef def = CreateDef().Orders;

            Assert.AreEqual(3, def.GetMaxHarvests(2));
            Assert.AreEqual(4, def.GetMaxHarvests(4));
            Assert.AreEqual(HarvestCap, def.GetMaxHarvests(15));
        }

        [Test]
        public void SameRandomState_SameOrders()
        {
            Farm a = CreateFarm(level: OrdersLevel, randomState: 12345);
            Farm b = CreateFarm(level: OrdersLevel, randomState: 12345);

            for (int i = 0; i < Slots; i++)
                AssertSameOrder(a.State.Orders[i], b.State.Orders[i]);
        }

        [Test]
        public void ZeroRandomState_IsSeeded()
        {
            Farm farm = CreateFarm();

            Assert.AreNotEqual(0u, farm.State.RandomState);
        }

        // ---------- Giao đơn ----------

        [Test]
        public void Deliver_NotEnoughItems_ChangesNothing()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            long coins = farm.State.Coins;
            OrderItem[] items = farm.State.Orders[0].Items;

            Assert.IsFalse(farm.CanDeliver(0));
            Assert.AreEqual(FarmResult.NotEnoughItems, farm.DeliverOrder(0));

            Assert.AreEqual(coins, farm.State.Coins);
            Assert.AreSame(items, farm.State.Orders[0].Items);
        }

        [Test]
        public void Deliver_EnoughItems_PaysRemovesItemsAndRefills()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            OrderState order = farm.State.Orders[0];
            long reward = order.RewardCoins;
            long rewardXp = order.RewardXp;
            OrderItem[] items = order.Items;
            foreach (OrderItem item in items)
                farm.State.Inventory[item.CropId] = item.Count + 1;
            long coins = farm.State.Coins;
            int changed = 0;
            int leveled = 0;
            farm.Changed += () => changed++;
            farm.LeveledUp += _ => leveled++;

            Assert.IsTrue(farm.CanDeliver(0));
            Assert.AreEqual(FarmResult.Ok, farm.DeliverOrder(0));

            // Thưởng XP có thể làm lên cấp 3 (thưởng thêm 10 × 3 xu); thưởng tối đa ở cấp 2 không đủ lên 2 cấp.
            long levelCoins = leveled > 0 ? 10 * (OrdersLevel + 1) : 0;
            Assert.AreEqual(coins + reward + levelCoins, farm.State.Coins);
            foreach (OrderItem item in items)
                Assert.AreEqual(1, farm.State.GetItems(item.CropId));
            Assert.AreEqual(rewardXp, farm.State.Xp + (leveled > 0 ? XpToNext[1] : 0));
            Assert.IsTrue(farm.State.Orders[0].HasOrder);
            Assert.AreNotSame(items, farm.State.Orders[0].Items);
            Assert.AreEqual(1, changed);
        }

        [Test]
        public void Deliver_EnoughXp_LevelsUp()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            OrderState order = farm.State.Orders[0];
            farm.State.Xp = System.Math.Max(0, XpToNext[1] - order.RewardXp);
            foreach (OrderItem item in order.Items)
                farm.State.Inventory[item.CropId] = item.Count;
            int leveled = 0;
            farm.LeveledUp += level => leveled = level;

            Assert.AreEqual(FarmResult.Ok, farm.DeliverOrder(0));

            Assert.AreEqual(OrdersLevel + 1, leveled);
        }

        // ---------- Bỏ đơn, làm mới ----------

        [Test]
        public void Discard_EmptiesSlotUntilCooldown()
        {
            Farm farm = CreateFarm(level: OrdersLevel);

            Assert.AreEqual(FarmResult.Ok, farm.DiscardOrder(1));

            OrderState order = farm.State.Orders[1];
            Assert.IsFalse(order.HasOrder);
            Assert.AreEqual(_clock.UtcNowSeconds + Cooldown, order.AvailableAt);
            Assert.AreEqual(FarmResult.OrderNotReady, farm.DeliverOrder(1));
            Assert.AreEqual(FarmResult.OrderNotReady, farm.DiscardOrder(1));
            Assert.IsTrue(farm.State.Orders[0].HasOrder);
        }

        [Test]
        public void Refresh_BeforeCooldown_DoesNothing()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            farm.DiscardOrder(0);
            int changed = 0;
            farm.Changed += () => changed++;

            _clock.Advance(Cooldown - 1);

            Assert.IsFalse(farm.RefreshOrders());
            Assert.IsFalse(farm.State.Orders[0].HasOrder);
            Assert.AreEqual(0, changed);
        }

        [Test]
        public void Refresh_AfterCooldown_FillsSlotAndRaisesChanged()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            farm.DiscardOrder(0);
            int changed = 0;
            farm.Changed += () => changed++;

            _clock.Advance(Cooldown);

            Assert.IsTrue(farm.RefreshOrders());
            Assert.IsTrue(farm.State.Orders[0].HasOrder);
            Assert.AreEqual(1, changed);
            Assert.IsFalse(farm.RefreshOrders());
        }

        [Test]
        public void OtherCommand_AfterCooldown_FillsSlot()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            farm.DiscardOrder(0);
            _clock.Advance(Cooldown);

            Assert.AreEqual(FarmResult.Ok, farm.BuySeeds(RauMuong, 1));

            Assert.IsTrue(farm.State.Orders[0].HasOrder);
        }

        // ---------- Save ----------

        [Test]
        public void Save_CurrentVersionIs2()
        {
            Assert.AreEqual(2, SaveData.CurrentVersion);
        }

        [Test]
        public void Save_RoundTrip_KeepsOrdersAndRandomState()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            farm.DiscardOrder(2);

            Farm loaded = RoundTrip(farm);

            Assert.AreEqual(farm.State.RandomState, loaded.State.RandomState);
            for (int i = 0; i < Slots; i++)
                AssertSameOrder(farm.State.Orders[i], loaded.State.Orders[i]);
        }

        [Test]
        public void Save_RoundTrip_NextOrderIsSame()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            Farm loaded = RoundTrip(farm);

            farm.DiscardOrder(0);
            loaded.DiscardOrder(0);
            _clock.Advance(Cooldown);
            farm.RefreshOrders();
            loaded.RefreshOrders();

            AssertSameOrder(farm.State.Orders[0], loaded.State.Orders[0]);
        }

        [Test]
        public void Save_Version1WithoutOrders_LoadsAndGeneratesOrders()
        {
            FarmDef def = CreateDef();
            FarmState state = FarmState.CreateNew(def);
            state.Level = OrdersLevel;
            SaveData data = SaveMapper.ToSave(state, def, _clock.UtcNowSeconds);
            data.version = 1;
            data.orders = null;
            data.randomState = 0;

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, def, out FarmState loaded));
            var farm = new Farm(def, loaded, _clock);

            AssertAllSlotsFilled(farm);
        }

        [Test]
        public void Save_OrderWithUnknownCrop_IsReplaced()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.orders[0].items = new[] { new SaveCount { id = "khong_co", count = 2 } };

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));
            var reloaded = new Farm(farm.Definition, loaded, _clock);

            Assert.IsTrue(reloaded.State.Orders[0].HasOrder);
            Assert.AreNotEqual("khong_co", reloaded.State.Orders[0].Items[0].CropId);
        }

        [Test]
        public void Save_FewerSlotsThanData_AddsSlots()
        {
            Farm farm = CreateFarm(level: OrdersLevel);
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            data.orders = new[] { data.orders[0] };

            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));
            var reloaded = new Farm(farm.Definition, loaded, _clock);

            Assert.AreEqual(Slots, reloaded.OrderSlotCount);
            AssertSameOrder(farm.State.Orders[0], reloaded.State.Orders[0]);
            AssertAllSlotsFilled(reloaded);
        }

        // ---------- Vòng chơi trọn vẹn ----------

        // Gieo → thu hoạch → lên cấp 2 có đơn → trồng đủ hàng → giao đơn tới cấp 3 → mở hết Khu 1 → Khu 2 có ô miễn phí.
        [Test]
        public void FullLoop_PlantHarvestDeliverLevelUpUnlockZone()
        {
            Farm farm = CreateFarm();

            while (farm.State.Level < OrdersLevel)
                PlantAndHarvest(farm, RauMuong);
            AssertAllSlotsFilled(farm);

            int delivered = 0;
            while (farm.State.Level < Zone2Level && delivered < 50)
            {
                foreach (OrderItem item in farm.State.Orders[0].Items)
                {
                    while (farm.State.GetItems(item.CropId) < item.Count)
                        PlantAndHarvest(farm, item.CropId);
                }
                long coins = farm.State.Coins;
                Assert.AreEqual(FarmResult.Ok, farm.DeliverOrder(0));
                Assert.Greater(farm.State.Coins, coins);
                delivered++;
            }
            Assert.AreEqual(Zone2Level, farm.State.Level);

            while (!farm.State.Zones[0].IsFull)
            {
                if (farm.UnlockNextPlot(Zone1) == FarmResult.NotEnoughCoins)
                    PlantAndHarvestAndSellAll(farm);
            }

            Assert.AreEqual(1, farm.State.Zones[1].OpenCount);
            Assert.IsTrue(farm.IsZoneAvailable(Zone2));
        }

        // ---------- Hỗ trợ ----------

        private void PlantAndHarvest(Farm farm, string cropId)
        {
            if (farm.State.GetSeeds(cropId) == 0)
                Assert.AreEqual(FarmResult.Ok, farm.BuySeeds(cropId, 1), $"Mua hạt {cropId}");
            Assert.AreEqual(FarmResult.Ok, farm.Plant(Plot0, cropId));
            _clock.Advance(GrowSeconds);
            Assert.AreEqual(FarmResult.Ok, farm.Harvest(Plot0));
        }

        private void PlantAndHarvestAndSellAll(Farm farm)
        {
            PlantAndHarvest(farm, RauMuong);
            Assert.AreEqual(FarmResult.Ok, farm.Sell(RauMuong, farm.State.GetItems(RauMuong)));
        }

        private Farm RoundTrip(Farm farm)
        {
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));
            return new Farm(farm.Definition, loaded, _clock);
        }

        private static void AssertAllSlotsFilled(Farm farm)
        {
            for (int i = 0; i < farm.OrderSlotCount; i++)
                Assert.IsTrue(farm.State.Orders[i].HasOrder, $"Ô đơn {i}");
        }

        private static void AssertSameOrder(OrderState expected, OrderState actual)
        {
            Assert.AreEqual(expected.Customer, actual.Customer);
            Assert.AreEqual(expected.RewardCoins, actual.RewardCoins);
            Assert.AreEqual(expected.RewardXp, actual.RewardXp);
            Assert.AreEqual(expected.AvailableAt, actual.AvailableAt);
            Assert.AreEqual(expected.Items.Length, actual.Items.Length);
            for (int i = 0; i < expected.Items.Length; i++)
            {
                Assert.AreEqual(expected.Items[i].CropId, actual.Items[i].CropId);
                Assert.AreEqual(expected.Items[i].Count, actual.Items[i].Count);
            }
        }

        private Farm CreateFarm(int level = 1, uint randomState = 0)
        {
            FarmDef def = CreateDef();
            FarmState state = FarmState.CreateNew(def);
            state.Level = level;
            state.RandomState = randomState;
            return new Farm(def, state, _clock);
        }

        private static FarmDef CreateDef(bool withOrders = true)
        {
            var zones = new[]
            {
                new ZoneDef(Zone1, new[] { new BedDef(2, 3) }, 2, new[] { 10, 10, 10, 10 }),
                new ZoneDef(Zone2, new[] { new BedDef(2, 3) }, 1, new[] { 10, 10, 10, 10, 10 }, Zone2Level),
            };
            var crops = new[]
            {
                new CropDef(RauMuong, GrowSeconds, 5, 5, 2, xp: 1),
                new CropDef(CaiXanh, GrowSeconds, 10, 20, 2, xp: 4, unlockLevel: OrdersLevel),
                new CropDef(BiDo, GrowSeconds, 30, 100, 1, xp: 10, unlockLevel: BiDoLevel),
            };
            var levels = new LevelDef(XpToNext, 10, 2);
            OrderDef orders = withOrders
                ? new OrderDef(Slots, OrdersLevel, CoinPercent, XpPercent, Cooldown, Names, HarvestCap)
                : null;
            var seeds = new Dictionary<string, int> { { RauMuong, 5 } };
            return new FarmDef(zones, crops, 100, seeds, 0, levels, RauMuong, orders);
        }
    }
}
