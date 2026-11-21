using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    // Điểm danh 7 ngày (Đợt 2 Phần 10): chuỗi cộng dồn, 18 giờ thật giữa 2 lần nhận, thư không trùng id,
    // xu tăng theo vòng, quà cần câu, save.
    public class CheckInTests
    {
        private const string Zone1 = "khu1";
        private const string RauMuong = "rau_muong";
        private const string Giun = "giun";
        private const string CanTre = "can_tre";
        private const string CanNua = "can_nua";

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock { UtcNowSeconds = VietnamTime(2026, 12, 1, 8) };
        }

        // Giờ VN → giây UTC.
        private static long VietnamTime(int year, int month, int day, int hour = 0)
            => ((long)GameCalendar.DayKeyFromDate(year, month, day) * 86400) + hour * 3600L
               + 1767225600L - GameCalendar.OffsetSeconds; // 1767225600 = 01/01/2026 00:00 UTC

        // ---------- Điều kiện nhận ----------

        [Test]
        public void FirstOpen_ClaimsDay1_SendsMail()
        {
            Farm farm = CreateFarm();
            var results = new List<CheckInResult>();
            farm.CheckedIn += results.Add;

            Assert.IsTrue(farm.CanCheckIn);
            Assert.IsTrue(farm.TryCheckIn());

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, results[0].Day);
            Assert.AreEqual(0, results[0].Cycle);
            Assert.IsTrue(farm.State.Mailbox.TryGet("diem_danh_1_1", out Mail mail));
            Assert.AreEqual("Điểm danh ngày 1/7", mail.Title);
            Assert.AreEqual(10, mail.Rewards[0].Amount);
            Assert.AreEqual(2, farm.CheckInNextDay);
            // Cùng ngày: không nhận lại.
            Assert.IsFalse(farm.TryCheckIn());
            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void Claim23h_Next7hNotYet_Next17hOk()
        {
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 1, 23);
            Farm farm = CreateFarm();
            Assert.IsTrue(farm.TryCheckIn());

            _clock.UtcNowSeconds = VietnamTime(2026, 12, 2, 7);
            Assert.IsFalse(farm.CanCheckIn, "Sang ngày nhưng mới 8 giờ");
            Assert.IsFalse(farm.TryCheckIn());

            _clock.UtcNowSeconds = VietnamTime(2026, 12, 2, 17);
            Assert.IsTrue(farm.TryCheckIn(), "Đủ 18 giờ");
            Assert.AreEqual(3, farm.CheckInNextDay);
        }

        [Test]
        public void SkipDays_KeepsChain()
        {
            Farm farm = CreateFarm();
            Assert.IsTrue(farm.TryCheckIn());
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 2, 8);
            Assert.IsTrue(farm.TryCheckIn());

            // Bỏ 3 ngày (3, 4, 5/12), quay lại 6/12: vẫn là ngày 3 của chuỗi.
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 6, 8);
            var results = new List<CheckInResult>();
            farm.CheckedIn += results.Add;
            Assert.IsTrue(farm.TryCheckIn());
            Assert.AreEqual(3, results[0].Day);
            Assert.IsTrue(farm.State.Mailbox.TryGet("diem_danh_1_3", out _));
        }

        [Test]
        public void ClockForwardTwoDays_OnlyOneClaim()
        {
            Farm farm = CreateFarm();
            Assert.IsTrue(farm.TryCheckIn());

            // Chỉnh giờ tiến 2 ngày: nhận được 1 lần, gọi tiếp không nhận thêm.
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 3, 8);
            Assert.IsTrue(farm.TryCheckIn());
            Assert.IsFalse(farm.TryCheckIn());
            Assert.AreEqual(3, farm.CheckInNextDay);
            // Hôm sau nhưng chưa đủ 18 giờ kể từ lần nhận trước (chỉ +16 giờ): chưa được.
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 4, 0);
            Assert.IsFalse(farm.TryCheckIn());
        }

        [Test]
        public void ClockBack_NoClaim()
        {
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 5, 8);
            Farm farm = CreateFarm();
            Assert.IsTrue(farm.TryCheckIn());
            // Lùi giờ máy về trước: ngày không lùi (GameCalendar) → không nhận.
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 3, 8);
            Assert.IsFalse(farm.TryCheckIn());
        }

        // ---------- Chu kỳ, id thư, quà ----------

        [Test]
        public void FullCycle_WrapsTo1_UniqueMailIds_CoinsGrow()
        {
            Farm farm = CreateFarm();
            // Đọc xu ngay lúc nhận (thư cũ hết hạn sau 7 ngày sẽ bị dọn).
            var coins = new Dictionary<string, long>();
            farm.CheckedIn += r =>
            {
                Assert.IsFalse(coins.ContainsKey(r.MailId), r.MailId);
                coins[r.MailId] = CoinsOf(farm, r.MailId);
            };

            for (int i = 0; i < 15; i++)
            {
                Assert.IsTrue(farm.TryCheckIn(), $"Lần {i + 1}");
                _clock.Advance(86400);
            }

            Assert.AreEqual(15, coins.Count);
            Assert.AreEqual(2, farm.CheckInCycle);
            Assert.AreEqual(2, farm.CheckInNextDay);
            // Ngày 1 vòng 1: 10 xu; vòng 2: +10%; vòng 3: +20%.
            Assert.AreEqual(10, coins["diem_danh_1_1"]);
            Assert.AreEqual(11, coins["diem_danh_2_1"]);
            Assert.AreEqual(12, coins["diem_danh_3_1"]);
        }

        [Test]
        public void CoinBonus_Capped()
        {
            CheckInDef def = CreateCheckIn();
            // 10%/vòng, tối đa 100% → vòng 50 vẫn gấp đôi.
            Assert.AreEqual(20, def.GetRewards(1, 50)[0].Amount);
            // Mồi không nhân.
            Assert.AreEqual(5, def.GetRewards(2, 50)[0].Amount);
        }

        [Test]
        public void Day7_Rod_NewThenRepair()
        {
            Farm farm = CreateFarm();
            for (int i = 0; i < 7; i++)
            {
                Assert.IsTrue(farm.TryCheckIn());
                _clock.Advance(86400);
            }
            Assert.IsTrue(farm.State.Mailbox.TryGet("diem_danh_1_7", out Mail day7));
            Assert.AreEqual(RewardKind.Rod, day7.Rewards[1].Kind);

            Assert.IsFalse(farm.State.Fishing.TryGetRod(CanNua, out _));
            Assert.AreEqual(FarmResult.Ok, farm.ClaimMail("diem_danh_1_7", out _));
            Assert.IsTrue(farm.State.Fishing.TryGetRod(CanNua, out RodState rod));
            Assert.AreEqual(40, rod.Durability);
            // Vẫn cầm cần tre (đã có cần) – cần mới không tự đổi.
            Assert.AreEqual(CanTre, farm.State.Fishing.EquippedRodId);

            // Vòng 2 ngày 7: đã có cần nứa → sửa đầy, không thêm cây trùng.
            rod.Durability = 3;
            for (int i = 0; i < 7; i++)
            {
                Assert.IsTrue(farm.TryCheckIn());
                _clock.Advance(86400);
            }
            Assert.AreEqual(FarmResult.Ok, farm.ClaimMail("diem_danh_2_7", out _));
            Assert.AreEqual(40, rod.Durability);
            Assert.AreEqual(2, farm.State.Fishing.Rods.Count);
        }

        [Test]
        public void Disabled_NeverClaims()
        {
            Farm farm = CreateFarm(withCheckIn: false);
            Assert.IsFalse(farm.CanCheckIn);
            Assert.IsFalse(farm.TryCheckIn());
        }

        [Test]
        public void InvalidRodReward_Throws()
        {
            var days = Days();
            days[6] = new[] { new Reward(RewardKind.Rod, "can_khong_co", 1) };
            Assert.Throws<System.ArgumentException>(() => CreateDef(new CheckInDef(days)));
            days[6] = new[] { new Reward(RewardKind.Rod, CanNua, 2) };
            Assert.Throws<System.ArgumentException>(() => CreateDef(new CheckInDef(days)));
        }

        [Test]
        public void WrongDayCount_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => new CheckInDef(new IEnumerable<Reward>[6]));
        }

        // ---------- Save ----------

        [Test]
        public void Save_RoundTrip_KeepsChainAndCooldown()
        {
            Farm farm = CreateFarm();
            Assert.IsTrue(farm.TryCheckIn());
            _clock.Advance(86400);
            Assert.IsTrue(farm.TryCheckIn());

            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loadedState));
            var loaded = new Farm(farm.Definition, loadedState, _clock);

            Assert.AreEqual(3, loaded.CheckInNextDay);
            Assert.AreEqual(farm.State.CheckIn.LastClaimDayKey, loaded.State.CheckIn.LastClaimDayKey);
            Assert.AreEqual(farm.State.CheckIn.LastClaimUtc, loaded.State.CheckIn.LastClaimUtc);
            Assert.IsFalse(loaded.TryCheckIn(), "Cùng ngày sau khi nạp lại");
        }

        [Test]
        public void Load_BadDay_Normalized()
        {
            Farm farm = CreateFarm();
            farm.State.CheckIn.NextDay = 9;
            farm.State.CheckIn.Cycle = -2;
            var reloaded = new Farm(farm.Definition, farm.State, _clock);
            Assert.AreEqual(1, reloaded.CheckInNextDay);
            Assert.AreEqual(0, reloaded.CheckInCycle);
        }

        // ---------- Hỗ trợ ----------

        private static long CoinsOf(Farm farm, string mailId)
        {
            Assert.IsTrue(farm.State.Mailbox.TryGet(mailId, out Mail mail), mailId);
            foreach (Reward reward in mail.Rewards)
            {
                if (reward.Kind == RewardKind.Coins)
                    return reward.Amount;
            }
            return 0;
        }

        private Farm CreateFarm(bool withCheckIn = true)
        {
            FarmDef def = CreateDef(withCheckIn ? CreateCheckIn() : null);
            return new Farm(def, FarmState.CreateNew(def), _clock);
        }

        private static IEnumerable<Reward>[] Days() => new IEnumerable<Reward>[]
        {
            new[] { new Reward(RewardKind.Coins, null, 10) },
            new[] { new Reward(RewardKind.Bait, Giun, 5) },
            new[] { new Reward(RewardKind.Coins, null, 20) },
            new[] { new Reward(RewardKind.Seeds, RauMuong, 10) },
            new[] { new Reward(RewardKind.Coins, null, 30) },
            new[] { new Reward(RewardKind.Coins, null, 40) },
            new[] { new Reward(RewardKind.Coins, null, 300), new Reward(RewardKind.Rod, CanNua, 1) },
        };

        private static CheckInDef CreateCheckIn() => new CheckInDef(Days(), 18, 10, 100, "Trưởng thôn");

        private static FarmDef CreateDef(CheckInDef checkIn)
        {
            var zones = new[] { new ZoneDef(Zone1, new[] { new BedDef(2, 3) }, 2, new[] { 10, 10, 10, 10 }) };
            var crops = new[] { new CropDef(RauMuong, 60, 5, 5, 2, xp: 1) };
            var levels = new LevelDef(new[] { 3, 20, 50, 100, 200, 300, 400, 500, 600 }, 0, 0);
            var fish = new[]
            {
                new FishDef("ca_ro", FishRarity.Common, 80, 300, 10, 1, new float[] { 1, 1, 1, 1, 1, 1, 1 }, FishDepth.All, null,
                    3, 4, 4f, 0.9f, 0),
            };
            var rods = new[]
            {
                new RodDef(CanTre, 1, 0.03f, 1.2f, 0, 0, 0, 30, 0, 1),
                new RodDef(CanNua, 2, 0.035f, 1.25f, 5, 2, 5, 40, 120, 2),
            };
            var fishing = new FishingDef(fish, rods, new[] { new BaitDef(Giun, 2) }, CanTre, Giun, 10);
            return new FarmDef(zones, crops, 0, null, 0, levels, null, null, fishing, checkIn: checkIn);
        }
    }
}
