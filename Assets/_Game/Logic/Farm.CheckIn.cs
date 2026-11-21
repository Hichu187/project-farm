using System;

namespace VeQue.Logic
{
    // Điểm danh (Đợt 2 Phần 10). View gọi TryCheckIn cùng nhịp RefreshMail (lúc mở game, định kỳ, quay lại tab).
    public sealed partial class Farm
    {
        // Vừa điểm danh (thư quà đã gửi). Phát trước Changed của lệnh đó.
        public event Action<CheckInResult> CheckedIn;

        // Ngày sẽ nhận kế tiếp (1..7) và vòng hiện tại – popup / lịch 7 ô đọc.
        public int CheckInNextDay => State.CheckIn.NextDay;
        public int CheckInCycle => State.CheckIn.Cycle;

        public bool CanCheckIn
        {
            get
            {
                CheckInDef def = Definition.CheckIn;
                if (!def.IsEnabled)
                    return false;
                CheckInState state = State.CheckIn;
                if (!state.HasClaimed)
                    return true;
                if (Calendar.DayKey <= state.LastClaimDayKey)
                    return false;
                return _clock.UtcNowSeconds - state.LastClaimUtc >= def.MinHoursBetween * 3600L;
            }
        }

        // Đủ điều kiện thì gửi thư "Điểm danh ngày N/7" và chuyển sang ngày kế (qua 7 → 1, tăng vòng). Trả true nếu đã nhận.
        public bool TryCheckIn()
        {
            if (!CanCheckIn)
                return false;
            CheckInDef def = Definition.CheckIn;
            CheckInState state = State.CheckIn;
            int day = state.NextDay;
            int cycle = state.Cycle;
            string id = CheckInDef.MailId(cycle, day);
            // Id đã gửi (hiếm: save cũ lệch) thì bỏ thư nhưng vẫn qua ngày để không kẹt chuỗi.
            AddMail(id, def.Sender, def.FormatTitle(day), def.FormatBody(day), def.GetRewards(day, cycle), def.ExpireDays);
            CleanupMail(Calendar.DayKey);

            state.LastClaimDayKey = Calendar.DayKey;
            state.LastClaimUtc = _clock.UtcNowSeconds;
            if (day >= CheckInDef.DayCount)
            {
                state.NextDay = 1;
                state.Cycle = cycle + 1;
            }
            else
            {
                state.NextDay = day + 1;
            }

            CheckedIn?.Invoke(new CheckInResult(day, cycle, id));
            NotifyChanged();
            return true;
        }

        // Nạp save: ngày trong khoảng 1..7, vòng ≥ 0.
        private void NormalizeCheckIn()
        {
            CheckInState state = State.CheckIn ??= new CheckInState();
            if (state.NextDay < 1 || state.NextDay > CheckInDef.DayCount)
                state.NextDay = 1;
            if (state.Cycle < 0)
                state.Cycle = 0;
        }
    }
}
