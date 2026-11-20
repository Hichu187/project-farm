using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.UI;

namespace VeQue.Tests
{
    // Đợt 2 Phần 2: GameCalendar (giờ VN cố định, không lùi ngày) + bảng âm lịch.
    public class CalendarTests
    {
        private const string LunarPath = "Assets/_Game/Data/Calendar/lunar_2026_2040.json";

        private static LunarTable LoadTable()
        {
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(LunarPath);
            Assert.IsNotNull(json, $"Không thấy {LunarPath}.");
            return LunarTableLoader.Load(json);
        }

        // DayKey của một ngày dương (giờ VN).
        private static int Key(int year, int month, int day)
            => (int)(new DateTime(year, month, day) - new DateTime(2026, 1, 1)).TotalDays;

        // Giờ Unix của hh:mm giờ VN.
        private static long VietnamTime(int year, int month, int day, int hour = 0, int minute = 0)
            => new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.FromHours(7)).ToUnixTimeSeconds();

        private static void AssertLunar(LunarTable table, int dayKey, int day, int month, bool leap, int year)
        {
            Assert.IsTrue(table.TryGet(dayKey, out LunarDate date), $"DayKey {dayKey}");
            Assert.AreEqual(day, date.Day, "ngày");
            Assert.AreEqual(month, date.Month, "tháng");
            Assert.AreEqual(leap, date.IsLeapMonth, "nhuận");
            Assert.AreEqual(year, date.Year, "năm");
        }

        // ---------- Bảng âm lịch ----------

        [Test]
        public void Lunar_TetBinhNgo_2026()
        {
            LunarTable table = LoadTable();
            AssertLunar(table, Key(2026, 2, 17), 1, 1, false, 2026);
            // Hôm trước là ngày cuối tháng Chạp năm Ất Tỵ.
            Assert.IsTrue(table.TryGet(Key(2026, 2, 16), out LunarDate eve));
            Assert.AreEqual(12, eve.Month);
            Assert.AreEqual(2025, eve.Year);
            Assert.That(eve.Day, Is.EqualTo(29).Or.EqualTo(30));
        }

        [Test]
        public void Lunar_TetDinhMui_2027()
        {
            AssertLunar(LoadTable(), Key(2027, 2, 6), 1, 1, false, 2027);
        }

        [Test]
        public void Lunar_LeapMonths()
        {
            LunarTable table = LoadTable();
            // Tháng 5 nhuận năm 2028 bắt đầu 23/06/2028; tháng 11 nhuận năm 2033 bắt đầu 22/12/2033.
            AssertLunar(table, Key(2028, 6, 23), 1, 5, true, 2028);
            AssertLunar(table, Key(2028, 6, 22), 30, 5, false, 2028);
            AssertLunar(table, Key(2033, 12, 22), 1, 11, true, 2033);
        }

        [Test]
        public void Lunar_RangeCoversToEnd2040_OutsideFalse()
        {
            LunarTable table = LoadTable();
            Assert.IsTrue(table.TryGet(Key(2026, 1, 1), out _));
            Assert.IsTrue(table.TryGet(Key(2040, 12, 31), out _));
            Assert.IsFalse(table.TryGet(table.FirstDayKey - 1, out _));
            Assert.IsFalse(table.TryGet(table.EndDayKey, out _));
            Assert.IsFalse(LunarTable.Empty.TryGet(0, out _));
        }

        [Test]
        public void LunarTable_NotContiguous_Throws()
        {
            var months = new[] { new LunarMonth(0, 30, 1, false, 2026), new LunarMonth(31, 29, 2, false, 2026) };
            Assert.Throws<ArgumentException>(() => new LunarTable(months));
        }

        [TestCase(1, 11, false, "Mùng 1 tháng 11")]
        [TestCase(10, 1, false, "Mùng 10 tháng Giêng")]
        [TestCase(15, 12, false, "Rằm tháng Chạp")]
        [TestCase(20, 5, true, "20 tháng 5 nhuận")]
        public void UiFormat_LunarDay(int day, int month, bool leap, string expected)
        {
            Assert.AreEqual(expected, UiFormat.LunarDay(new LunarDate(day, month, leap, 2026)));
        }

        // ---------- GameCalendar ----------

        [Test]
        public void DayKey_ChangesAtVietnamMidnight()
        {
            var clock = new FakeClock { UtcNowSeconds = VietnamTime(2026, 11, 20) - 1 };
            var calendar = new GameCalendar(clock, LunarTable.Empty);
            Assert.AreEqual(Key(2026, 11, 19), calendar.DayKey);
            clock.Advance(1);
            Assert.AreEqual(Key(2026, 11, 20), calendar.DayKey);
            // 0 giờ VN = 17:00 UTC hôm trước.
            Assert.AreEqual(17 * 3600, VietnamTime(2026, 11, 20) % 86400);
            Assert.AreEqual(0, GameCalendar.DayKeyOf(VietnamTime(2026, 1, 1)));
            Assert.AreEqual(-1, GameCalendar.DayKeyOf(VietnamTime(2026, 1, 1) - 1));
        }

        [Test]
        public void SecondsUntilNextDay_ToVietnamMidnight()
        {
            var clock = new FakeClock { UtcNowSeconds = VietnamTime(2026, 11, 20, 23, 0) };
            var calendar = new GameCalendar(clock, LunarTable.Empty);
            Assert.AreEqual(3600, calendar.SecondsUntilNextDay);
        }

        [Test]
        public void TimeWindow_UsesVietnamTime()
        {
            var clock = new FakeClock { UtcNowSeconds = VietnamTime(2026, 11, 20, 5, 0) };
            var calendar = new GameCalendar(clock, LunarTable.Empty);
            Assert.AreEqual(DaySlot.Dawn, calendar.TimeWindow);
            clock.UtcNowSeconds = VietnamTime(2026, 11, 20, 4, 59);
            Assert.AreEqual(DaySlot.Night, calendar.TimeWindow);
        }

        [Test]
        public void DayKey_NeverGoesBack_AndSurvivesSave()
        {
            var clock = new FakeClock { UtcNowSeconds = VietnamTime(2026, 11, 20, 10, 0) };
            var state = new FarmState();
            var calendar = new GameCalendar(clock, LunarTable.Empty, state);
            int today = calendar.DayKey;

            clock.Advance(-3 * 86400);
            Assert.AreEqual(today, calendar.DayKey, "Chỉnh giờ lùi 3 ngày: ngày giữ nguyên");
            Assert.AreEqual(today, state.LastSeenDayKey);

            // Mở lại game với giờ đã lùi: vẫn giữ ngày đã thấy.
            var reopened = new GameCalendar(clock, LunarTable.Empty, state);
            Assert.AreEqual(today, reopened.DayKey);

            clock.Advance(4 * 86400);
            Assert.AreEqual(today + 1, reopened.DayKey);
        }

        [Test]
        public void DaySeed_StablePerDayAndSalt()
        {
            int day = Key(2026, 11, 20);
            Assert.AreEqual(GameCalendar.DaySeed(day, "gia_cho"), GameCalendar.DaySeed(day, "gia_cho"));
            Assert.AreNotEqual(GameCalendar.DaySeed(day, "gia_cho"), GameCalendar.DaySeed(day, "su_kien"));
            Assert.AreNotEqual(GameCalendar.DaySeed(day, "gia_cho"), GameCalendar.DaySeed(day + 1, "gia_cho"));
            // Giá trị cố định: đổi thuật toán băm là đổi giá chợ / sự kiện của mọi người chơi → test này phải đỏ.
            Assert.AreEqual(ExpectedSeed(day, "gia_cho"), GameCalendar.DaySeed(day, "gia_cho"));

            var clock = new FakeClock { UtcNowSeconds = VietnamTime(2026, 11, 20, 12, 0) };
            Assert.AreEqual(GameCalendar.DaySeed(day, "gia_cho"), new GameCalendar(clock, LunarTable.Empty).DaySeed("gia_cho"));
        }

        [Test]
        public void Farm_CalendarLunarToday()
        {
            var clock = new FakeClock { UtcNowSeconds = VietnamTime(2027, 2, 6, 9, 0) };
            var zones = new[] { new ZoneDef("khu1", new[] { new BedDef(2, 3) }, 2, new[] { 10, 10, 10, 10 }) };
            var crops = new[] { new CropDef("rau_muong", 60, 5, 5, 2, xp: 1) };
            var def = new FarmDef(zones, crops, 0, lunar: LoadTable());
            var farm = new Farm(def, FarmState.CreateNew(def), clock);

            Assert.IsTrue(farm.Calendar.TryGetLunar(out LunarDate date));
            Assert.AreEqual(1, date.Day);
            Assert.AreEqual(1, date.Month);
            Assert.AreEqual(2027, date.Year);
        }

        // FNV-1a trên các ký tự salt, rồi trộn DayKey, rồi murmur3 fmix32 (cùng công thức GameCalendar) – viết lại độc lập.
        private static uint ExpectedSeed(int dayKey, string salt)
        {
            uint hash = 2166136261u;
            foreach (char c in salt)
            {
                hash ^= c;
                hash *= 16777619u;
            }
            hash ^= (uint)dayKey;
            hash *= 16777619u;
            uint h = hash ^ 0x9E3779B9u;
            h ^= h >> 16;
            h *= 0x85EBCA6Bu;
            h ^= h >> 13;
            h *= 0xC2B2AE35u;
            h ^= h >> 16;
            return h == 0 ? 1u : h;
        }
    }
}
