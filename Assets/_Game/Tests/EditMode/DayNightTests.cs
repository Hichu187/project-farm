using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.Tests
{
    // Đợt 2 Phần 13: màu trời theo giờ (nội suy, vòng qua nửa đêm) + profile thật không tối quá 30%.
    public class DayNightTests
    {
        private const string ProfilePath = "Assets/_Game/Data/DayNightProfile.asset";

        private static readonly DayNightKey[] Keys =
        {
            new DayNightKey(0f, 0.6f, 0.6f, 0.8f),
            new DayNightKey(6f, 1f, 0.8f, 0.8f),
            new DayNightKey(12f, 1f, 1f, 1f),
            new DayNightKey(20f, 0.8f, 0.8f, 1f),
        };

        [Test]
        public void Evaluate_OnKey_ReturnsKey()
        {
            DayNightKey k = DayNightCurve.Evaluate(Keys, 12f);
            Assert.AreEqual(1f, k.R, 1e-5f);
            Assert.AreEqual(1f, k.G, 1e-5f);
        }

        [Test]
        public void Evaluate_Between_Interpolates()
        {
            // 9h: giữa 6h (G 0,8) và 12h (G 1) → 0,9.
            Assert.AreEqual(0.9f, DayNightCurve.Evaluate(Keys, 9f).G, 1e-5f);
        }

        [Test]
        public void Evaluate_WrapsMidnight()
        {
            // 22h: giữa 20h (R 0,8) và 0h hôm sau (R 0,6) → 0,7.
            Assert.AreEqual(0.7f, DayNightCurve.Evaluate(Keys, 22f).R, 1e-5f);
            // Mốc đầu không ở 0h: 1h nằm giữa 22h hôm trước và 4h.
            var keys = new[] { new DayNightKey(4f, 0f, 0f, 0f), new DayNightKey(22f, 1f, 1f, 1f) };
            Assert.AreEqual(0.5f, DayNightCurve.Evaluate(keys, 1f).R, 1e-5f);
            Assert.AreEqual(0.5f, DayNightCurve.Evaluate(keys, 25f).R, 1e-5f);
        }

        [Test]
        public void Evaluate_NoKeys_White()
        {
            Assert.AreEqual(1f, DayNightCurve.Evaluate(Array.Empty<DayNightKey>(), 3f).B);
        }

        [Test]
        public void Validate_RejectsBadKeys()
        {
            Assert.Throws<ArgumentException>(() => DayNightCurve.Validate(new[] { new DayNightKey(24f, 1f, 1f, 1f) }));
            Assert.Throws<ArgumentException>(() => DayNightCurve.Validate(new[]
            {
                new DayNightKey(5f, 1f, 1f, 1f), new DayNightKey(5f, 1f, 1f, 1f),
            }));
            Assert.Throws<ArgumentException>(() => DayNightCurve.Validate(new[] { new DayNightKey(1f, 1.2f, 1f, 1f) }));
        }

        [Test]
        public void HourOfDay_IsVietnamTime()
        {
            // 31/12/2026 17:00 UTC = 0:00 giờ VN; 17:30 UTC = 0:30.
            Assert.AreEqual(0f, DayNightCurve.HourOfDay(1798736400), 1e-4f);
            Assert.AreEqual(0.5f, DayNightCurve.HourOfDay(1798736400 + 1800), 1e-4f);
            Assert.AreEqual(23.5f, DayNightCurve.HourOfDay(1798736400 - 1800), 1e-4f);
        }

        // ---------- Giờ trong game (chốt 21/11/2026: 1 ngày game = 30 phút thật, chỉ hiển thị) ----------

        [Test]
        public void GameDayClock_ThirtyMinuteDay()
        {
            var clock = new GameDayClock(1800);
            Assert.AreEqual(0f, clock.HourOf(1800L * 1000), 1e-4f);
            // 7,5 phút thật = 6 giờ game; 75 giây thật = 1 giờ game.
            Assert.AreEqual(6f, clock.HourOf(1800L * 1000 + 450), 1e-4f);
            Assert.AreEqual(1f, clock.HourOf(1800L * 1000 + 75), 1e-4f);
            // 75 giây trước nửa đêm game = 23 giờ.
            Assert.AreEqual(23f, clock.HourOf(1800L * 1000 - 75), 1e-4f);
        }

        [Test]
        public void GameDayClock_PhaseAndLength()
        {
            // Lệch pha 450 giây: lúc giờ Unix chia hết 1800 thì đã 6 giờ game.
            Assert.AreEqual(6f, new GameDayClock(1800, 450).HourOf(1800L * 5), 1e-4f);
            // Ngày 60 phút: 15 phút thật = 6 giờ game.
            Assert.AreEqual(6f, new GameDayClock(3600).HourOf(3600L * 3 + 900), 1e-4f);
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameDayClock(30));
        }

        [TestCase(0f, "00:00")]
        [TestCase(6.5f, "06:30")]
        [TestCase(23.99f, "23:59")]
        [TestCase(13.25f, "13:15")]
        public void GameDayClock_Format(float hour, string expected)
        {
            Assert.AreEqual(expected, GameDayClock.Format(hour));
        }

        [TestCase(5.99f, false)]
        [TestCase(6f, true)]
        [TestCase(17.99f, true)]
        [TestCase(18f, false)]
        public void GameDayClock_Daytime(float hour, bool expected)
        {
            Assert.AreEqual(expected, GameDayClock.IsDaytime(hour));
        }

        [Test]
        public void Profile_DefaultData_NeverDarkerThan30Percent()
        {
            var profile = AssetDatabase.LoadAssetAtPath<DayNightProfile>(ProfilePath);
            Assert.IsNotNull(profile, $"Không thấy {ProfilePath}. Chạy menu Về Quê → Ngày đêm: tạo trong scene.");
            IReadOnlyList<DayNightKey> keys = profile.ToKeys();
            Assert.GreaterOrEqual(keys.Count, 2);
            for (float hour = 0f; hour < 24f; hour += 0.25f)
                Assert.GreaterOrEqual(DayNightCurve.Evaluate(keys, hour).Luminance, 0.7f, $"{hour}h");
        }
    }
}
