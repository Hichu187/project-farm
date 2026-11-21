using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Hộp thư + phát quà (Đợt 2 Phần 3). Ngày tính theo GameCalendar.DayKey (ngày thật, giờ VN).
    // Nhận quà là một bước trọn vẹn: cộng quà + đánh dấu đã nhận trong cùng lệnh; View lưu ngay sau đó (GameBootstrap).
    public sealed partial class Farm
    {
        public const int MaxMails = 50;
        // Thư đã nhận tự xóa sau bấy nhiêu ngày.
        public const int ClaimedMailKeepDays = 7;

        // Số thư cần chú ý (chưa đọc, hoặc có quà chưa nhận) – chấm đỏ trên nút Thư.
        public int MailAttentionCount
        {
            get
            {
                int count = 0;
                foreach (Mail mail in State.Mailbox.Mails)
                {
                    if (mail.NeedsAttention)
                        count++;
                }
                return count;
            }
        }

        // Gửi thư (id chưa từng gửi). Quà sai (cây / mồi không có, số lượng ≤ 0) → ArgumentException.
        // expireDays 0 = không hết hạn. Trả false nếu id đã gửi trước đây (kể cả thư đã bị dọn).
        public bool SendMail(string id, string sender, string title, string body, IEnumerable<Reward> rewards, int expireDays = 0)
        {
            if (!AddMail(id, sender, title, body, rewards, expireDays))
                return false;
            CleanupMail(Calendar.DayKey);
            NotifyChanged();
            return true;
        }

        public FarmResult ReadMail(string id)
        {
            if (!State.Mailbox.TryGet(id, out Mail mail))
                return FarmResult.UnknownMail;
            if (mail.Read)
                return FarmResult.Ok;
            mail.Read = true;
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Nhận quà 1 thư. Thư không có quà: chỉ đánh dấu đã đọc.
        public FarmResult ClaimMail(string id, out IReadOnlyList<Reward> granted)
        {
            granted = Array.Empty<Reward>();
            if (!State.Mailbox.TryGet(id, out Mail mail))
                return FarmResult.UnknownMail;
            int today = Calendar.DayKey;
            if (mail.IsExpired(today))
                return FarmResult.MailExpired;
            if (mail.Claimed)
                return FarmResult.MailAlreadyClaimed;
            if (!mail.HasRewards)
                return FarmResult.NoMailToClaim;

            ClaimInternal(mail, today);
            granted = mail.Rewards.AsReadOnly();
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Nhận hết các thư còn quà (chưa hết hạn). Không có thư nào → NoMailToClaim.
        public FarmResult ClaimAllMail(out List<Reward> granted)
        {
            granted = new List<Reward>();
            int today = Calendar.DayKey;
            foreach (Mail mail in State.Mailbox.Mails)
            {
                if (mail.Claimed || !mail.HasRewards || mail.IsExpired(today))
                    continue;
                ClaimInternal(mail, today);
                granted.AddRange(mail.Rewards);
            }
            if (granted.Count == 0)
                return FarmResult.NoMailToClaim;
            NotifyChanged();
            return FarmResult.Ok;
        }

        // Sang ngày mới (hoặc lần đầu): dọn thư, phát thư theo build đang trong hạn. View gọi định kỳ.
        // Trả về true (và báo Changed) nếu có gì đổi.
        public bool RefreshMail()
        {
            if (!RefreshMailInternal())
                return false;
            NotifyChanged();
            return true;
        }

        // Cộng quà (dùng chung cho mọi hệ thống). Quà sai → ArgumentException, không cộng phần nào.
        public void GrantRewards(IEnumerable<Reward> rewards)
        {
            if (rewards == null)
                return;
            var list = new List<Reward>(rewards);
            foreach (Reward reward in list)
            {
                if (!Definition.IsValidReward(reward, out string error))
                    throw new ArgumentException(error, nameof(rewards));
            }
            long xp = 0;
            foreach (Reward reward in list)
            {
                switch (reward.Kind)
                {
                    case RewardKind.Coins:
                        State.Coins += reward.Amount;
                        break;
                    case RewardKind.Xp:
                        xp += reward.Amount;
                        break;
                    case RewardKind.Seeds:
                        FarmState.AddCount(State.Seeds, reward.Id, (int)reward.Amount);
                        break;
                    case RewardKind.Produce:
                        FarmState.AddCount(State.Inventory, reward.Id, (int)reward.Amount);
                        break;
                    case RewardKind.Bait:
                        FarmState.AddCount(State.Fishing.Baits, reward.Id, (int)reward.Amount);
                        break;
                    case RewardKind.Rod:
                        GrantRod(reward.Id);
                        break;
                }
            }
            // XP sau cùng: lên cấp vẫn trao thưởng cấp như bình thường.
            AddXp(xp);
        }

        // Quà cần câu: chưa có thì thêm (đầy độ bền, chưa cầm cần nào thì cầm luôn); đã có thì sửa đầy độ bền.
        private void GrantRod(string rodId)
        {
            Definition.Fishing.TryGetRod(rodId, out RodDef def);
            FishingState fishing = State.Fishing;
            if (fishing.TryGetRod(rodId, out RodState owned))
            {
                owned.Durability = def.MaxDurability;
                return;
            }
            fishing.Rods.Add(new RodState { Id = rodId, Durability = def.MaxDurability });
            fishing.EquippedRodId ??= rodId;
        }

        private void ClaimInternal(Mail mail, int today)
        {
            GrantRewards(mail.Rewards);
            mail.Claimed = true;
            mail.Read = true;
            mail.ClaimedDayKey = today;
        }

        private bool AddMail(string id, string sender, string title, string body, IEnumerable<Reward> rewards, int expireDays)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Thư cần id.", nameof(id));
            if (expireDays < 0)
                throw new ArgumentOutOfRangeException(nameof(expireDays));
            MailboxState box = State.Mailbox;
            if (box.SentIds.Contains(id))
                return false;
            var list = new List<Reward>();
            if (rewards != null)
            {
                foreach (Reward reward in rewards)
                {
                    if (!Definition.IsValidReward(reward, out string error))
                        throw new ArgumentException($"Thư '{id}': {error}", nameof(rewards));
                    list.Add(new Reward(reward.Kind, reward.Id, reward.Amount));
                }
            }
            int today = Calendar.DayKey;
            box.Mails.Add(new Mail
            {
                Id = id,
                Sender = sender ?? "",
                Title = title ?? "",
                Body = body ?? "",
                Rewards = list,
                CreatedDayKey = today,
                ExpiresDayKey = expireDays > 0 ? today + expireDays : Mail.NoExpiry,
            });
            box.SentIds.Add(id);
            return true;
        }

        private bool RefreshMailInternal()
        {
            MailboxState box = State.Mailbox;
            int today = Calendar.DayKey;
            if (box.LastRefreshDayKey == today)
                return false;
            box.LastRefreshDayKey = today;
            foreach (BuildMailDef def in Definition.BuildMails)
            {
                if (today >= def.StartDayKey && today <= def.EndDayKey)
                    AddMail(def.Id, def.Sender, def.Title, def.Body, def.Rewards, def.ExpireDays);
            }
            CleanupMail(today);
            return true;
        }

        // Thư quá hạn: xóa (quà chưa nhận mất). Thư đã nhận ≥ 7 ngày: xóa. Quá 50 thư: xóa thư cũ nhất, ưu tiên thư đã nhận.
        private void CleanupMail(int today)
        {
            List<Mail> mails = State.Mailbox.Mails;
            mails.RemoveAll(m => m.IsExpired(today) || (m.Claimed && today - m.ClaimedDayKey >= ClaimedMailKeepDays));
            while (mails.Count > MaxMails)
            {
                int victim = -1;
                for (int i = 0; i < mails.Count && victim < 0; i++)
                {
                    if (mails[i].Claimed || !mails[i].HasRewards)
                        victim = i;
                }
                mails.RemoveAt(victim >= 0 ? victim : 0);
            }
        }

        // Nạp save: hộp thư hợp lệ.
        private void NormalizeMail()
        {
            MailboxState box = State.Mailbox ??= new MailboxState();
            box.Mails ??= new List<Mail>();
            box.SentIds ??= new HashSet<string>();
            box.Mails.RemoveAll(m => m == null || string.IsNullOrEmpty(m.Id));
            foreach (Mail mail in box.Mails)
            {
                mail.Rewards ??= new List<Reward>();
                // Quà không còn hợp lệ với data hiện tại (cây / mồi bị bỏ) thì bỏ phần quà đó.
                mail.Rewards.RemoveAll(r => !Definition.IsValidReward(r, out _));
                box.SentIds.Add(mail.Id);
            }
        }
    }
}
