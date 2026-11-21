using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    // Giá chợ theo ngày (Đợt 2 Phần 9): hệ số ngày, hút hàng / rớt giá, rau lá theo giờ, nhân với sự kiện,
    // không phụ thuộc múi giờ máy, đơn theo giá gốc, "Bán hết" giữ phần cho đơn, lịch sử 7 ngày.
    public class MarketTests
    {
        private const string Zone1 = "khu1";
        private const string RauMuong = "rau_muong";  // rau lá, giá gốc 100
        private const string CaiXanh = "cai_xanh";    // rau lá, giá gốc 100
        private const string BiDo = "bi_do";          // quả, giá gốc 100
        private const string Khoai = "khoai";         // củ, giá gốc 100
        private const string HoaCuc = "hoa_cuc";      // eventOnly

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock { UtcNowSeconds = VietnamTime(2026, 12, 1, 14) };
        }

        private static long VietnamTime(int year, int month, int day, int hour = 0)
            => (long)GameCalendar.DayKeyFromDate(year, month, day) * 86400 + hour * 3600L
               + 1767225600L - GameCalendar.OffsetSeconds;

        // ---------- Hệ số ngày ----------

        [Test]
        public void DailyMult_AverageAboutOne_InRange()
        {
            var market = new MarketService(new MarketDef(), Crops());
            double sum = 0;
            int count = 0;
            for (int d = 0; d < 365; d++)
            {
                string hot = market.GetHotCrop(d);
                string cold = market.GetColdCrop(d);
                foreach (CropDef crop in Crops())
                {
                    if (crop.EventOnly)
                        continue;
                    float mult = market.GetDailyMult(crop.Id, d);
                    float raw = crop.Id == hot ? mult / 1.4f : crop.Id == cold ? mult / 0.7f : mult;
                    Assert.That(raw, Is.InRange(0.85f, 1.15f), $"{crop.Id} ngày {d}");
                    sum += raw;
                    count++;
                }
            }
            Assert.AreEqual(1.0, sum / count, 0.02);
        }

        [Test]
        public void EachDay_OneHotOneCold_Different_NotEventOnly()
        {
            var market = new MarketService(new MarketDef(), Crops());
            var hots = new HashSet<string>();
            for (int d = 0; d < 365; d++)
            {
                string hot = market.GetHotCrop(d);
                string cold = market.GetColdCrop(d);
                Assert.IsNotNull(hot);
                Assert.IsNotNull(cold);
                Assert.AreNotEqual(hot, cold, $"Ngày {d}");
                Assert.AreNotEqual(HoaCuc, hot);
                Assert.AreNotEqual(HoaCuc, cold);
                hots.Add(hot);
            }
            Assert.AreEqual(4, hots.Count, "Cây nào cũng có ngày hút hàng");
        }

        [Test]
        public void SameDay_SamePrice_AnyOrder()
        {
            var a = new MarketService(new MarketDef(), Crops());
            var b = new MarketService(new MarketDef(), Crops());
            var first = new float[60];
            for (int d = 0; d < 60; d++)
                first[d] = a.GetDailyMult(BiDo, d);
            for (int d = 59; d >= 0; d--)
                Assert.AreEqual(first[d], b.GetDailyMult(BiDo, d));
        }

        // ---------- Trong ngày ----------

        [Test]
        public void Intraday_OnlyLeafy()
        {
            var market = new MarketService(new MarketDef(), Crops());
            CropDef leafy = Crop(RauMuong);
            CropDef fruit = Crop(BiDo);
            Assert.AreEqual(1.1f, market.GetIntradayMult(leafy, DaySlot.Dawn), 1e-5);
            Assert.AreEqual(1.1f, market.GetIntradayMult(leafy, DaySlot.Morning), 1e-5);
            Assert.AreEqual(1f, market.GetIntradayMult(leafy, DaySlot.Afternoon), 1e-5);
            Assert.AreEqual(0.9f, market.GetIntradayMult(leafy, DaySlot.Night), 1e-5);
            Assert.AreEqual(1f, market.GetIntradayMult(fruit, DaySlot.Morning));
            Assert.AreEqual(1f, market.GetIntradayMult(fruit, DaySlot.Night));
        }

        [Test]
        public void SellPrice_LeafyChangesWithHour_FruitDoesNot()
        {
            Farm farm = CreateFarm();
            int day = farm.Calendar.DayKey;
            float leafyDaily = farm.Market.GetDailyMult(RauMuong, day);
            float fruitDaily = farm.Market.GetDailyMult(BiDo, day);

            _clock.UtcNowSeconds = VietnamTime(2026, 12, 1, 8);
            Assert.AreEqual(MarketService.Round(100 * leafyDaily * 1.1), farm.GetSellPrice(RauMuong));
            Assert.AreEqual(MarketService.Round(100 * fruitDaily), farm.GetSellPrice(BiDo));
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 1, 14);
            Assert.AreEqual(MarketService.Round(100 * leafyDaily), farm.GetSellPrice(RauMuong));
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 1, 23);
            Assert.AreEqual(MarketService.Round(100 * leafyDaily * 0.9), farm.GetSellPrice(RauMuong));
            Assert.AreEqual(MarketService.Round(100 * fruitDaily), farm.GetSellPrice(BiDo));
        }

        [Test]
        public void Price_IndependentOfMachineTimeZone()
        {
            // Giá theo DayKey + khung giờ VN (GameCalendar UTC+7 cố định): cùng thời điểm UTC → cùng giá,
            // dù máy ở múi giờ nào (Farm không đọc múi giờ máy).
            Farm a = CreateFarm();
            Farm b = CreateFarm();
            foreach (string id in new[] { RauMuong, CaiXanh, BiDo, Khoai })
                Assert.AreEqual(a.GetSellPrice(id), b.GetSellPrice(id));
            // 16:59 UTC = 23:59 VN ngày 1/12; 17:00 UTC = 0:00 VN ngày 2/12 → đổi ngày đúng mốc giờ VN.
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 2, 0) - 1;
            Assert.AreEqual(GameCalendar.DayKeyFromDate(2026, 12, 1), a.Calendar.DayKey);
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 2, 0);
            Assert.AreEqual(GameCalendar.DayKeyFromDate(2026, 12, 2), a.Calendar.DayKey);
        }

        [Test]
        public void EventMult_StacksWithMarket()
        {
            var e = new EventDef("cho", "Chợ", "", new EventTrigger(EventTriggerKind.Solar, 1, 12), new[]
            {
                new EventEffect(EventEffectType.CropPriceMult, EffectTarget.Parse("crop:bi_do"), mult: 2f),
            });
            Farm farm = CreateFarm(e);
            float daily = farm.Market.GetDailyMult(BiDo, farm.Calendar.DayKey);
            Assert.AreEqual(MarketService.Round(100 * daily * 2.0), farm.GetSellPrice(BiDo));
        }

        [Test]
        public void Sell_UsesMarketPrice()
        {
            Farm farm = CreateFarm();
            farm.State.Inventory[BiDo] = 3;
            long coins = farm.State.Coins;
            int price = farm.GetSellPrice(BiDo);
            Assert.AreEqual(FarmResult.Ok, farm.Sell(BiDo, 3));
            Assert.AreEqual(coins + 3L * price, farm.State.Coins);
        }

        [Test]
        public void Disabled_BasePrice()
        {
            Farm farm = CreateFarm(market: MarketDef.None);
            Assert.AreEqual(100, farm.GetSellPrice(RauMuong));
            Assert.IsNull(farm.TodayHotCrop);
            Assert.AreEqual(0, farm.GetPriceChangePercent(RauMuong));
        }

        // ---------- So với hôm qua, lịch sử ----------

        [Test]
        public void History7Days_MatchesDailyPrice_AndChange()
        {
            Farm farm = CreateFarm();
            int today = farm.Calendar.DayKey;
            var history = new int[7];
            farm.GetPriceHistory(Khoai, history);
            for (int i = 0; i < 7; i++)
                Assert.AreEqual(farm.Market.GetDailyPrice(Crop(Khoai), today - 6 + i), history[i]);
            int expected = (int)System.Math.Round((history[6] - history[5]) * 100.0 / history[5]);
            Assert.AreEqual(expected, farm.GetPriceChangePercent(Khoai));
        }

        // ---------- Đơn ----------

        [Test]
        public void Orders_UseBasePrice_NotMarket()
        {
            // Thưởng đơn sinh ra từ giá gốc: 2 farm cùng seed, 1 có giá chợ, 1 không → đơn giống hệt.
            Farm withMarket = CreateFarm(withOrders: true);
            Farm plain = CreateFarm(withOrders: true, market: MarketDef.None);
            for (int i = 0; i < withMarket.State.Orders.Length; i++)
            {
                Assert.AreEqual(plain.State.Orders[i].RewardCoins, withMarket.State.Orders[i].RewardCoins);
                Assert.AreEqual(plain.State.Orders[i].RewardXp, withMarket.State.Orders[i].RewardXp);
            }
        }

        [Test]
        public void SellAll_KeepsReservedForOrders()
        {
            Farm farm = CreateFarm(withOrders: true);
            OrderState order = farm.State.Orders[0];
            order.Kind = OrderKind.Crops;
            order.Items = new[] { new OrderItem { Id = BiDo, Count = 3 } };
            order.ExpiresAt = 0;
            for (int i = 1; i < farm.State.Orders.Length; i++)
                farm.State.Orders[i].Items = new[] { new OrderItem { Id = Khoai, Count = 1 } };
            farm.State.Inventory[BiDo] = 10;

            Assert.AreEqual(3, farm.GetReservedForOrders(BiDo));
            Assert.AreEqual(7, farm.GetSellableCount(BiDo));
            farm.State.Inventory[BiDo] = 2;
            Assert.AreEqual(0, farm.GetSellableCount(BiDo), "Có ít hơn số cần cho đơn: không bán");
            Assert.AreEqual(0, farm.GetReservedForOrders(CaiXanh));
        }

        // ---------- Hỗ trợ ----------

        private static CropDef[] Crops() => new[]
        {
            new CropDef(RauMuong, 60, 10, 100, 2),
            new CropDef(CaiXanh, 600, 10, 100, 2),
            new CropDef(BiDo, 3600, 10, 100, 2, group: CropGroup.Fruit),
            new CropDef(Khoai, 3600, 10, 100, 2, group: CropGroup.RootGrain),
            new CropDef(HoaCuc, 3600, 10, 100, 1, eventOnly: true),
        };

        private static CropDef Crop(string id)
        {
            foreach (CropDef crop in Crops())
            {
                if (crop.Id == id)
                    return crop;
            }
            return null;
        }

        private Farm CreateFarm(EventDef e = null, bool withOrders = false, MarketDef market = null)
        {
            var zones = new[] { new ZoneDef(Zone1, new[] { new BedDef(2, 3) }, 6, new int[0]) };
            var levels = new LevelDef(new[] { 3, 20, 50, 100, 200, 300, 400, 500, 600 }, 0, 0);
            OrderDef orders = withOrders ? new OrderDef(3, 1, 150, 100, 0, null) : null;
            var events = e != null ? new EventsDef(new[] { e }, 0) : null;
            var def = new FarmDef(zones, Crops(), 0, null, 0, levels, null, orders, null,
                events: events, market: market ?? new MarketDef());
            FarmState state = FarmState.CreateNew(def);
            state.RandomState = 4242;
            return new Farm(def, state, _clock);
        }
    }
}
