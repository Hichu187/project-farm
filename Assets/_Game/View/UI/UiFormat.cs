using System.Globalization;
using VeQue.Logic;

namespace VeQue.View.UI
{
    // Trạng thái popup điểm danh: nhận được (nút Nhận), vừa / đã nhận hôm nay, chưa tới lúc nhận.
    public enum CheckInView
    {
        Ready,
        Claimed,
        Waiting,
    }

    public static class UiFormat
    {
        private const long SecondsPerMinute = 60;
        private const long SecondsPerHour = 3600;

        // Không dùng CultureInfo "vi-VN" vì có thể không có sẵn trên WebGL.
        public static string Coins(long coins)
        {
            return coins.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', '.');
        }

        public static string Countdown(long seconds)
        {
            long hours = seconds / SecondsPerHour;
            long minutes = seconds % SecondsPerHour / SecondsPerMinute;
            long rest = seconds % SecondsPerMinute;
            return hours > 0 ? $"{hours}:{minutes:00}:{rest:00}" : $"{minutes}:{rest:00}";
        }

        public static string Reward(long coins, long xp) => $"+{Coins(coins)} xu · +{Coins(xp)} XP";

        // Khu theo thứ tự mở khóa, đếm từ 1.
        public static string ZoneName(int index) => $"Khu {index + 1}";

        public static string GrowTime(long seconds)
        {
            if (seconds >= SecondsPerHour && seconds % SecondsPerHour == 0)
                return $"{seconds / SecondsPerHour} giờ";
            return $"{seconds / SecondsPerMinute} phút";
        }

        // ---------- Câu cá ----------

        private static readonly string[] SlotNames = { "Bình minh", "Sáng", "Trưa", "Chiều", "Hoàng hôn", "Tối", "Khuya" };

        public static string SlotName(DaySlot slot) => SlotNames[(int)slot];

        // Đồng hồ chung trên HUD (giờ địa phương theo độ lệch múi giờ đã lưu): "15:42". Tên khung giờ hiện dòng dưới (SlotName).
        // Ngày âm lịch: "Mùng 1 tháng 11", "Rằm tháng Chạp", "20 tháng Giêng", tháng nhuận thêm " nhuận".
        public static string LunarDay(LunarDate date)
        {
            string day = date.Day <= 10 ? $"Mùng {date.Day}" : date.Day == 15 ? "Rằm" : date.Day.ToString();
            string month = date.Month == 1 ? "Giêng" : date.Month == 12 ? "Chạp" : date.Month.ToString();
            return $"{day} tháng {month}{(date.IsLeapMonth ? " nhuận" : "")}";
        }

        public static string Clock(long utcSeconds, int utcOffsetSeconds)
        {
            long local = utcSeconds + utcOffsetSeconds;
            long ofDay = ((local % 86400) + 86400) % 86400;
            long hour = ofDay / 3600;
            long minute = ofDay % 3600 / 60;
            return $"{hour:00}:{minute:00}";
        }

        // "Hoàng hôn 17–19h · còn 1:24".
        public static string SlotStatus(DaySlot slot, long secondsLeft) =>
            $"{SlotName(slot)} {DaySlots.StartHour(slot)}–{DaySlots.EndHour(slot)}h · còn {Countdown(secondsLeft)}";

        // Chữ nổi trên đầu nhân vật sau khi ném (chốt 13/11/2026).
        public static string CastQualityName(CastQuality quality)
        {
            switch (quality)
            {
                case CastQuality.Perfect: return "Hoàn hảo!";
                case CastQuality.Good: return "Tuyệt!";
                case CastQuality.Normal: return "Tốt";
                default: return "Yếu";
            }
        }

        public static string RarityName(FishRarity rarity)
        {
            switch (rarity)
            {
                case FishRarity.Uncommon: return "Khá";
                case FishRarity.Rare: return "Hiếm";
                case FishRarity.Epic: return "Quý";
                case FishRarity.Legendary: return "Huyền thoại";
                default: return "Thường";
            }
        }

        public static string DepthName(FishDepth depth)
        {
            switch (depth)
            {
                case FishDepth.Far: return "Xa";
                case FishDepth.Mid: return "Vừa";
                default: return "Gần";
            }
        }

        // Album: các độ sâu cá sống, "Gần, Vừa, Xa".
        public static string DepthList(FishDepth depths)
        {
            var parts = new System.Collections.Generic.List<string>(3);
            if ((depths & FishDepth.Near) != 0)
                parts.Add("Gần");
            if ((depths & FishDepth.Mid) != 0)
                parts.Add("Vừa");
            if ((depths & FishDepth.Far) != 0)
                parts.Add("Xa");
            return parts.Count == 0 ? "?" : string.Join(", ", parts);
        }

        // DayKey (GameCalendar, 0 = 01/01/2026) → "21/11/2026".
        public static string DayKeyDate(int dayKey)
            => new System.DateTime(2026, 1, 1).AddDays(dayKey).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        // "7/20 loài · Mốc kế: 10 loài"; hết mốc: "20/20 loài · Đủ cả rồi!" (đủ loài) hoặc chỉ "x/y loài".
        public static string AlbumProgress(int caught, int total, int nextMilestone)
        {
            string head = $"{caught}/{total} loài";
            if (nextMilestone > 0)
                return $"{head} · Mốc kế: {nextMilestone} loài";
            return caught >= total && total > 0 ? $"{head} · Đủ cả rồi!" : head;
        }

        public static string JunkProgress(int found, int total) => $"Đã vớt {found}/{total} món";

        // Bảng kết quả khi vớt rác: tự bán thì "Đã bán +5 xu", không thì giá cô đồng nát trả.
        public static string JunkPrice(int price, bool autoSold)
            => autoSold ? $"Đã bán +{Coins(price)} xu" : $"{Coins(price)} xu";

        // Nút bật / tắt tự bán rác trong giỏ.
        public static string AutoSellJunk(bool on) => on ? "Tự bán rác: Bật" : "Tự bán rác: Tắt";

        public static string JunkSold(long earned) => $"Cô đồng nát trả +{Coins(earned)} xu";

        // ---------- Giá chợ (Đợt 2 Phần 9) ----------

        // % so với hôm qua: "+8%" xanh, "-5%" đỏ, 0 thì rỗng. Dùng dấu '-' thường (font pixel có thể thiếu '−').
        public static string PriceChange(int percent)
        {
            if (percent > 0)
                return $"<color=#3c8a2e>+{percent}%</color>";
            if (percent < 0)
                return $"<color=#c0392b>-{-percent}%</color>";
            return "";
        }

        // Nhãn sau tên: " Hút hàng" (cam) / " Rớt giá" (xám), cỡ nhỏ hơn tên.
        public static string MarketTag(bool hot, bool cold)
        {
            if (hot)
                return " <size=75%><color=#d9822b>Hút hàng</color></size>";
            if (cold)
                return " <size=75%><color=#7a7a7a>Rớt giá</color></size>";
            return "";
        }

        // Dòng Kho: "x5 · 12 xu/cái +8%".
        public static string StorageInfo(int items, int price, int changePercent)
        {
            string change = PriceChange(changePercent);
            return change.Length > 0
                ? $"x{items} · {Coins(price)} xu/cái {change}"
                : $"x{items} · {Coins(price)} xu/cái";
        }

        // Bán hết có giữ phần cần cho đơn.
        public static string SoldKeeping(long earned, int kept)
            => kept > 0 ? $"+{Coins(earned)} xu · giữ {kept} cho đơn" : $"+{Coins(earned)} xu";

        public static string KeptForOrders(int kept) => $"Đang giữ {kept} cho đơn hàng";

        public static string PriceChartTitle(string cropName) => $"{cropName} · giá 7 ngày";

        // DayKey → "T2".."T7", "CN".
        public static string WeekdayShort(int dayKey)
        {
            System.DayOfWeek day = new System.DateTime(2026, 1, 1).AddDays(dayKey).DayOfWeek;
            return day == System.DayOfWeek.Sunday ? "CN" : $"T{(int)day + 1}";
        }

        // Popup điểm danh (Đợt 2 Phần 10; 22/11: người chơi tự bấm Nhận).
        public static string CheckInSubtitle(int day, CheckInView view)
        {
            switch (view)
            {
                case CheckInView.Ready: return $"Ngày {day}/7 · Bấm Nhận để lấy quà";
                case CheckInView.Claimed: return $"Ngày {day}/7 · Quà đã gửi vào hộp thư";
                default: return $"Mai ghé lại nhận quà ngày {day}/7 nhé!";
            }
        }

        // Nhãn trên ô: ô hôm nay "Hôm nay" (nhận được / đã nhận) hoặc "Kế tiếp" (chưa tới lúc); ô trước "Đã nhận"; còn lại "Ngày N".
        public static string CheckInDayLabel(int day, bool done, bool today, CheckInView view)
        {
            if (today)
                return view == CheckInView.Waiting ? "Kế tiếp" : "Hôm nay";
            return done ? "Đã nhận" : $"Ngày {day}";
        }

        public static string AlbumMilestoneToast(int count)
            => count > 1 ? $"Album đạt {count} mốc! Xem hộp thư" : "Album đạt mốc! Xem hộp thư";

        // Dưới 1 kg: "350 g"; từ 1 kg: "1,25 kg" (tối đa 2 số lẻ, dấu phẩy).
        public static string Weight(int grams)
        {
            if (grams < 1000)
                return $"{grams} g";
            return (grams / 1000.0).ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',') + " kg";
        }

        public static string PullGradeName(PullGrade grade)
        {
            switch (grade)
            {
                case PullGrade.Perfect: return "Perfect!";
                case PullGrade.Good: return "Tốt";
                default: return "Thường";
            }
        }

        // Bảng cá vừa câu: "Perfect! · +2 XP".
        public static string CatchDetail(PullGrade grade, int xp) => $"{PullGradeName(grade)} · +{xp} XP";

        // Dòng kỷ lục trong bảng cá vừa câu (Đợt E): lần đầu câu loài → "Loài mới!"; nặng hơn kỷ lục cũ → "Kỷ lục mới! (cũ …)";
        // không phá kỷ lục → "" (ẩn dòng).
        // Cá shiny (Đợt 2 Phần 18): dòng trên bảng cá vừa câu.
        public static string ShinyCatch(string kindName, bool firstTime)
            => firstTime ? $"Đột biến: {kindName}! Lần đầu!" : $"Đột biến: {kindName}!";

        // Cá shiny: dòng trong chi tiết cá ở giỏ.
        public static string ShinyDetail(string kindName) => $"Đột biến: {kindName}";

        public static string CatchRecord(bool isNewRecord, int previousRecordGrams)
        {
            if (!isNewRecord)
                return "";
            return previousRecordGrams <= 0 ? "Loài mới!" : $"Kỷ lục mới! (cũ {Weight(previousRecordGrams)})";
        }

        // Kết quả không ra cá: chữ ngắn nổi trên đầu nhân vật (chốt 13/11/2026). Cá ra thì bảng cá vừa câu.
        // junk: lượt móc phải rác (Đợt 2 Phần 5) – hết giờ kéo thì "Rác trôi mất!".
        public static string FishingOutcomeText(FishingOutcome outcome, bool junk = false)
        {
            switch (outcome)
            {
                case FishingOutcome.Scared: return "Sớm quá!";
                case FishingOutcome.BaitStolen: return "Mất mồi!";
                case FishingOutcome.Slipped: return "Trượt lưỡi!";
                case FishingOutcome.Escaped: return junk ? "Rác trôi mất!" : "Cá thoát!";
                default: return "";
            }
        }

        // Lý do không mua / sửa được đồ câu (Farm.BuyRod / BuyBait / RepairRod).
        public static string ShopBlocked(FarmResult result)
        {
            switch (result)
            {
                case FarmResult.NotEnoughCoins: return "Không đủ xu";
                case FarmResult.LevelTooLow: return "Chưa đủ cấp";
                case FarmResult.AlreadyOwned: return "Đã có cần này";
                case FarmResult.RodNotDamaged: return "Cần còn đầy độ bền";
                case FarmResult.FishingLocked: return "Chưa mở câu cá";
                default: return "Không mua được";
            }
        }

        // Lý do không ném được (Farm.CanCast).
        public static string CastBlocked(FarmResult result)
        {
            switch (result)
            {
                case FarmResult.FishingLocked: return "Chưa mở câu cá";
                case FarmResult.NoRod: return "Chưa có cần câu";
                case FarmResult.RodBroken: return "Cần hỏng rồi, cần sửa hoặc mua cần mới";
                case FarmResult.NotEnoughBait: return "Hết mồi rồi";
                case FarmResult.BasketFull: return "Giỏ cá đầy rồi";
                default: return "Không ném được";
            }
        }
    }
}
