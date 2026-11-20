using System;
using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    // Đợt 2 Phần 3: hộp thư – gửi một lần theo id, nhận quà một lần, lưu / nạp, thư theo build, dọn thư, các loại quà.
    public class MailTests
    {
        private const string Zone1 = "khu1";
        private const string RauMuong = "rau_muong";
        private const string CaiXanh = "cai_xanh";
        private const string Giun = "giun";
        private const string Can = "can_tre";
        private static readonly int[] XpToNext = { 10, 20, 50 };
        private const int CoinsPerLevel = 7;

        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock { UtcNowSeconds = VietnamTime(2026, 12, 1, 9) };
        }

        private static long VietnamTime(int year, int month, int day, int hour = 0)
            => new DateTimeOffset(year, month, day, hour, 0, 0, TimeSpan.FromHours(7)).ToUnixTimeSeconds();

        private static int Day(int year, int month, int day) => GameCalendar.DayKeyFromDate(year, month, day);

        // ---------- Gửi / nhận ----------

        [Test]
        public void Claim_Twice_RewardOnce()
        {
            Farm farm = CreateFarm();
            Assert.IsTrue(farm.SendMail("qua_1", "Mẹ", "Quà", "…", new[] { new Reward(RewardKind.Coins, null, 50) }));
            long coins = farm.State.Coins;

            Assert.AreEqual(FarmResult.Ok, farm.ClaimMail("qua_1", out IReadOnlyList<Reward> granted));
            Assert.AreEqual(1, granted.Count);
            Assert.AreEqual(FarmResult.MailAlreadyClaimed, farm.ClaimMail("qua_1", out _));
            Assert.AreEqual(coins + 50, farm.State.Coins);
            Assert.IsTrue(farm.State.Mailbox.Mails[0].Read);
        }

        [Test]
        public void SendMail_SameIdTwice_Ignored()
        {
            Farm farm = CreateFarm();
            Assert.IsTrue(farm.SendMail("a", "", "", "", null));
            Assert.IsFalse(farm.SendMail("a", "", "", "", null));
            Assert.AreEqual(1, farm.State.Mailbox.Mails.Count);
        }

        [Test]
        public void ClaimThenReload_CannotClaimAgain()
        {
            Farm farm = CreateFarm();
            farm.SendMail("qua_1", "Mẹ", "Quà", "", new[] { new Reward(RewardKind.Coins, null, 50) });
            farm.ClaimMail("qua_1", out _);
            long coins = farm.State.Coins;

            Farm loaded = RoundTrip(farm);

            Assert.AreEqual(coins, loaded.State.Coins);
            Assert.AreEqual(FarmResult.MailAlreadyClaimed, loaded.ClaimMail("qua_1", out _));
            Assert.AreEqual(coins, loaded.State.Coins);
            Assert.IsFalse(loaded.SendMail("qua_1", "", "", "", null), "Id đã gửi vẫn nhớ sau khi nạp");
        }

        [Test]
        public void Reload_KeepsMailContent()
        {
            Farm farm = CreateFarm();
            farm.SendMail("thu", "Mẹ", "Tiêu đề", "Nội dung", new[]
            {
                new Reward(RewardKind.Seeds, RauMuong, 5), new Reward(RewardKind.Bait, Giun, 3),
            }, expireDays: 10);
            farm.ReadMail("thu");

            Mail mail = RoundTrip(farm).State.Mailbox.Mails[0];

            Assert.AreEqual("Mẹ", mail.Sender);
            Assert.AreEqual("Tiêu đề", mail.Title);
            Assert.AreEqual("Nội dung", mail.Body);
            Assert.AreEqual(2, mail.Rewards.Count);
            Assert.AreEqual(RewardKind.Bait, mail.Rewards[1].Kind);
            Assert.AreEqual(Giun, mail.Rewards[1].Id);
            Assert.AreEqual(3, mail.Rewards[1].Amount);
            Assert.AreEqual(Day(2026, 12, 1) + 10, mail.ExpiresDayKey);
            Assert.IsTrue(mail.Read);
            Assert.IsFalse(mail.Claimed);
        }

        [Test]
        public void ClaimAll_GrantsEveryUnclaimed()
        {
            Farm farm = CreateFarm();
            farm.SendMail("a", "", "", "", new[] { new Reward(RewardKind.Coins, null, 10) });
            farm.SendMail("b", "", "", "", new[] { new Reward(RewardKind.Coins, null, 20) });
            farm.SendMail("c", "", "", "", null);
            farm.ClaimMail("a", out _);
            long coins = farm.State.Coins;

            Assert.AreEqual(FarmResult.Ok, farm.ClaimAllMail(out List<Reward> granted));
            Assert.AreEqual(1, granted.Count);
            Assert.AreEqual(coins + 20, farm.State.Coins);
            Assert.AreEqual(FarmResult.NoMailToClaim, farm.ClaimAllMail(out _));
        }

        [Test]
        public void AttentionCount_UnreadOrUnclaimed()
        {
            Farm farm = CreateFarm();
            farm.SendMail("chu", "", "", "", null);
            farm.SendMail("qua", "", "", "", new[] { new Reward(RewardKind.Coins, null, 10) });
            Assert.AreEqual(2, farm.MailAttentionCount);
            farm.ReadMail("chu");
            farm.ReadMail("qua");
            Assert.AreEqual(1, farm.MailAttentionCount, "Đã đọc nhưng chưa nhận quà vẫn cần chú ý");
            farm.ClaimMail("qua", out _);
            Assert.AreEqual(0, farm.MailAttentionCount);
        }

        // ---------- Quà ----------

        [Test]
        public void Rewards_EveryKindGranted()
        {
            Farm farm = CreateFarm();
            farm.SendMail("q", "", "", "", new[]
            {
                new Reward(RewardKind.Coins, null, 30),
                new Reward(RewardKind.Seeds, CaiXanh, 4),
                new Reward(RewardKind.Produce, RauMuong, 6),
                new Reward(RewardKind.Bait, Giun, 2),
                new Reward(RewardKind.Xp, null, 3),
            });
            long coins = farm.State.Coins;
            int bait = farm.State.Fishing.GetBaits(Giun);

            farm.ClaimMail("q", out _);

            Assert.AreEqual(coins + 30, farm.State.Coins);
            Assert.AreEqual(4, farm.State.GetSeeds(CaiXanh));
            Assert.AreEqual(6, farm.State.GetItems(RauMuong));
            Assert.AreEqual(bait + 2, farm.State.Fishing.GetBaits(Giun));
            Assert.AreEqual(3, farm.State.Xp);
        }

        [Test]
        public void XpReward_LevelUpGivesLevelReward()
        {
            Farm farm = CreateFarm();
            var levels = new List<int>();
            farm.LeveledUp += levels.Add;
            farm.SendMail("xp", "", "", "", new[] { new Reward(RewardKind.Xp, null, 12) });
            long coins = farm.State.Coins;

            farm.ClaimMail("xp", out _);

            CollectionAssert.AreEqual(new[] { 2 }, levels);
            Assert.AreEqual(2, farm.State.Level);
            Assert.AreEqual(2, farm.State.Xp);
            Assert.AreEqual(coins + CoinsPerLevel * 2, farm.State.Coins);
        }

        [Test]
        public void InvalidReward_Throws()
        {
            Farm farm = CreateFarm();
            Assert.Throws<ArgumentException>(() => farm.SendMail("x", "", "", "", new[] { new Reward(RewardKind.Seeds, "khong_co", 1) }));
            Assert.Throws<ArgumentException>(() => farm.SendMail("y", "", "", "", new[] { new Reward(RewardKind.Bait, "khong_co", 1) }));
            Assert.Throws<ArgumentException>(() => farm.SendMail("z", "", "", "", new[] { new Reward(RewardKind.Coins, null, 0) }));
            CollectionAssert.IsEmpty(farm.State.Mailbox.Mails);
            Assert.Throws<ArgumentException>(() => CreateDef(new[]
            {
                new BuildMailDef("bad", 0, 9999, "", "", "", new[] { new Reward(RewardKind.Produce, "khong_co", 1) }),
            }));
        }

        // ---------- Thư theo build ----------

        [Test]
        public void BuildMail_SentOnceInRange_EvenAfterCleanup()
        {
            var mails = new[]
            {
                new BuildMailDef("tet", Day(2026, 12, 1), Day(2026, 12, 10), "Nhà phát triển", "Chúc mừng", "",
                    new[] { new Reward(RewardKind.Coins, null, 100) }),
            };
            Farm farm = CreateFarm(mails);
            Assert.IsTrue(farm.State.Mailbox.TryGet("tet", out _));

            // Người chơi xóa / thư bị dọn mất mà chưa nhận: hôm sau không gửi lại.
            farm.State.Mailbox.Mails.Clear();
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 2, 9);
            farm.RefreshMail();
            CollectionAssert.IsEmpty(farm.State.Mailbox.Mails);

            // Nạp lại save cũng không gửi lại.
            CollectionAssert.IsEmpty(RoundTrip(farm).State.Mailbox.Mails);
        }

        [Test]
        public void BuildMail_OutOfRange_NotSent_DayBoundaryVietnam()
        {
            var mails = new[] { new BuildMailDef("sau", Day(2026, 12, 5), Day(2026, 12, 5), "", "", "", null) };
            // 4/12 23:00 giờ VN: chưa tới ngày.
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 4, 23);
            Farm farm = CreateFarm(mails);
            CollectionAssert.IsEmpty(farm.State.Mailbox.Mails);

            // 5/12 0:00 giờ VN (= 4/12 17:00 UTC): đủ ngày.
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 5, 0);
            Assert.IsTrue(farm.RefreshMail());
            Assert.IsTrue(farm.State.Mailbox.TryGet("sau", out _));

            // 6/12: hết hạn phát; người chưa vào ngày 5 không nhận.
            _clock.UtcNowSeconds = VietnamTime(2026, 12, 6, 9);
            Farm late = CreateFarm(mails);
            CollectionAssert.IsEmpty(late.State.Mailbox.Mails);
        }

        [Test]
        public void TryParseDate()
        {
            Assert.IsTrue(GameCalendar.TryParseDate("2027-02-06", out int day));
            Assert.AreEqual(Day(2027, 2, 6), day);
            Assert.IsFalse(GameCalendar.TryParseDate("2027-02-30", out _));
            Assert.IsFalse(GameCalendar.TryParseDate("06/02/2027", out _));
        }

        // ---------- Dọn thư ----------

        [Test]
        public void Cleanup_ExpiredAndOldClaimed()
        {
            Farm farm = CreateFarm();
            farm.SendMail("het_han", "", "", "", new[] { new Reward(RewardKind.Coins, null, 5) }, expireDays: 2);
            farm.SendMail("da_nhan", "", "", "", new[] { new Reward(RewardKind.Coins, null, 5) });
            farm.ClaimMail("da_nhan", out _);

            _clock.UtcNowSeconds = VietnamTime(2026, 12, 3, 9);
            farm.RefreshMail();
            Assert.IsTrue(farm.State.Mailbox.TryGet("het_han", out _), "Ngày hết hạn vẫn còn");

            _clock.UtcNowSeconds = VietnamTime(2026, 12, 4, 9);
            farm.RefreshMail();
            Assert.IsFalse(farm.State.Mailbox.TryGet("het_han", out _), "Quá hạn thì xóa");
            Assert.AreEqual(FarmResult.UnknownMail, farm.ClaimMail("het_han", out _));
            Assert.IsTrue(farm.State.Mailbox.TryGet("da_nhan", out _));

            _clock.UtcNowSeconds = VietnamTime(2026, 12, 8, 9);
            farm.RefreshMail();
            Assert.IsFalse(farm.State.Mailbox.TryGet("da_nhan", out _), "Đã nhận 7 ngày thì xóa");
        }

        [Test]
        public void Cleanup_Over50_RemovesOldestClaimedFirst()
        {
            Farm farm = CreateFarm();
            farm.SendMail("cu_chua_nhan", "", "", "", new[] { new Reward(RewardKind.Coins, null, 1) });
            farm.SendMail("cu_da_nhan", "", "", "", new[] { new Reward(RewardKind.Coins, null, 1) });
            farm.ClaimMail("cu_da_nhan", out _);
            for (int i = 0; i < Farm.MaxMails - 1; i++)
                farm.SendMail($"m{i}", "", "", "", new[] { new Reward(RewardKind.Coins, null, 1) });

            Assert.AreEqual(Farm.MaxMails, farm.State.Mailbox.Mails.Count);
            Assert.IsFalse(farm.State.Mailbox.TryGet("cu_da_nhan", out _));
            Assert.IsTrue(farm.State.Mailbox.TryGet("cu_chua_nhan", out _));

            farm.SendMail("them", "", "", "", new[] { new Reward(RewardKind.Coins, null, 1) });
            Assert.AreEqual(Farm.MaxMails, farm.State.Mailbox.Mails.Count);
            Assert.IsFalse(farm.State.Mailbox.TryGet("cu_chua_nhan", out _), "Hết thư đã nhận thì xóa thư cũ nhất");
        }

        // ---------- Dựng data ----------

        private Farm CreateFarm(IEnumerable<BuildMailDef> buildMails = null)
        {
            FarmDef def = CreateDef(buildMails);
            return new Farm(def, FarmState.CreateNew(def), _clock);
        }

        private static FarmDef CreateDef(IEnumerable<BuildMailDef> buildMails)
        {
            var zones = new[] { new ZoneDef(Zone1, new[] { new BedDef(2, 3) }, 2, new[] { 10, 10, 10, 10 }) };
            var crops = new[]
            {
                new CropDef(RauMuong, 60, 5, 5, 2, xp: 1),
                new CropDef(CaiXanh, 60, 10, 20, 2, xp: 4, unlockLevel: 2),
            };
            var levels = new LevelDef(XpToNext, CoinsPerLevel, 0);
            float[] allDay = { 1, 1, 1, 1, 1, 1, 1 };
            var fishing = new FishingDef(
                new[] { new FishDef("ca_ro", FishRarity.Common, 80, 300, 10, 1, allDay, FishDepth.All, null, 3, 4, 4f, 0.9f, 0) },
                new[] { new RodDef(Can, 1, 0.03f, 1.2f, 0, 0, 0, 30, 0, 1) },
                new[] { new BaitDef(Giun, 2) }, Can, Giun, 10);
            return new FarmDef(zones, crops, 100, null, 0, levels, null, null, fishing, null, buildMails);
        }

        private Farm RoundTrip(Farm farm)
        {
            SaveData data = SaveMapper.ToSave(farm.State, farm.Definition, _clock.UtcNowSeconds);
            Assert.AreEqual(SaveLoadResult.Ok, SaveMapper.TryFromSave(data, farm.Definition, out FarmState loaded));
            return new Farm(farm.Definition, loaded, _clock);
        }
    }
}
