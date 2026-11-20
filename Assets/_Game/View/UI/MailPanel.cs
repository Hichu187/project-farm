using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.View.UI
{
    // Bảng Hộp thư (Đợt 2 Phần 3): danh sách thư (mới nhất trên cùng) + "Nhận tất cả"; chạm 1 thư → xem chi tiết
    // (người gửi, tiêu đề, nội dung, quà có icon + số lượng, nút "Nhận"). Mở thư = đánh dấu đã đọc.
    // Nhận quà qua GameBootstrap (lưu ngay). Báo kết quả bằng Message (toast).
    public sealed class MailPanel : MonoBehaviour
    {
        private const int RewardSlots = 4;

        [SerializeField] private GameBootstrap _game;
        [SerializeField] private BottomSheet _sheet;
        [SerializeField] private RectTransform _content;
        [SerializeField] private MailRow _rowTemplate;
        [SerializeField] private TMP_Text _emptyLabel;
        [SerializeField] private Button _claimAll;

        [Header("Chi tiết thư")]
        [SerializeField] private GameObject _detail;
        [SerializeField] private Button _back;
        [SerializeField] private TMP_Text _detailSender;
        [SerializeField] private TMP_Text _detailTitle;
        [SerializeField] private TMP_Text _detailBody;
        [SerializeField] private GameObject[] _rewardRoots = Array.Empty<GameObject>();
        [SerializeField] private Image[] _rewardIcons = Array.Empty<Image>();
        [SerializeField] private TMP_Text[] _rewardLabels = Array.Empty<TMP_Text>();
        [SerializeField] private Button _claim;
        [SerializeField] private TMP_Text _claimLabel;
        [SerializeField] private Sprite _coinIcon;

        private readonly List<MailRow> _rows = new List<MailRow>();
        private string _openId;

        public event Action<string> Message;

        public bool IsOpen => _sheet.IsOpen;

        public void Open()
        {
            _openId = null;
            Refresh();
            _sheet.Open();
        }

        public void Close()
        {
            _sheet.Close();
        }

        private void Start()
        {
            if (_game == null)
                _game = FindFirstObjectByType<GameBootstrap>();
            _rowTemplate.gameObject.SetActive(false);
            _claimAll.onClick.AddListener(ClaimAll);
            _claim.onClick.AddListener(ClaimOpen);
            _back.onClick.AddListener(() =>
            {
                _openId = null;
                Refresh();
            });
            _game.Farm.Changed += HandleChanged;
            _detail.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_game != null && _game.Farm != null)
                _game.Farm.Changed -= HandleChanged;
        }

        private void HandleChanged()
        {
            if (IsOpen)
                Refresh();
        }

        private void Refresh()
        {
            Farm farm = _game.Farm;
            List<Mail> mails = farm.State.Mailbox.Mails;
            if (_openId != null && farm.State.Mailbox.TryGet(_openId, out Mail open))
            {
                ShowDetail(open);
                return;
            }
            _openId = null;
            _detail.SetActive(false);

            // Mới nhất trên cùng (thư thêm sau nằm cuối danh sách).
            int shown = 0;
            bool anyClaimable = false;
            for (int i = mails.Count - 1; i >= 0; i--)
            {
                Mail mail = mails[i];
                MailRow row = RowAt(shown++);
                string id = mail.Id;
                row.Show(mail.Sender, mail.Title, RewardSummary(mail), Status(mail), mail.NeedsAttention, () => OpenMail(id));
                anyClaimable |= mail.HasRewards && !mail.Claimed;
            }
            for (int i = shown; i < _rows.Count; i++)
                _rows[i].gameObject.SetActive(false);
            _emptyLabel.gameObject.SetActive(shown == 0);
            _claimAll.gameObject.SetActive(true);
            _claimAll.interactable = anyClaimable;
        }

        private MailRow RowAt(int index)
        {
            while (_rows.Count <= index)
                _rows.Add(Instantiate(_rowTemplate, _content));
            MailRow row = _rows[index];
            row.gameObject.SetActive(true);
            return row;
        }

        private void OpenMail(string id)
        {
            _openId = id;
            // Đọc thư phát Farm.Changed → Refresh hiện chi tiết.
            if (_game.Farm.ReadMail(id) == FarmResult.Ok)
                Refresh();
        }

        private void ShowDetail(Mail mail)
        {
            _detail.SetActive(true);
            _claimAll.gameObject.SetActive(false);
            _detailSender.text = string.IsNullOrEmpty(mail.Sender) ? "" : $"Từ: {mail.Sender}";
            _detailTitle.text = mail.Title;
            _detailBody.text = mail.Body;

            int count = mail.HasRewards ? Mathf.Min(mail.Rewards.Count, _rewardRoots.Length) : 0;
            for (int i = 0; i < _rewardRoots.Length; i++)
            {
                bool used = i < count;
                _rewardRoots[i].SetActive(used);
                if (!used)
                    continue;
                Reward reward = mail.Rewards[i];
                Sprite icon = RewardIcon(reward);
                _rewardIcons[i].sprite = icon;
                _rewardIcons[i].enabled = icon != null;
                _rewardLabels[i].text = RewardLabel(reward);
            }

            bool claimable = mail.HasRewards && !mail.Claimed;
            _claim.gameObject.SetActive(mail.HasRewards);
            _claim.interactable = claimable;
            _claimLabel.text = mail.Claimed ? "Đã nhận" : "Nhận";
        }

        private void ClaimOpen()
        {
            if (_openId == null)
                return;
            FarmResult result = _game.ClaimMail(_openId, out IReadOnlyList<Reward> granted);
            if (result == FarmResult.Ok)
                Message?.Invoke(GrantedText(granted));
            else if (result == FarmResult.MailExpired)
                Message?.Invoke("Thư đã hết hạn");
        }

        private void ClaimAll()
        {
            if (_game.ClaimAllMail(out List<Reward> granted) == FarmResult.Ok)
                Message?.Invoke(GrantedText(granted));
        }

        private static string Status(Mail mail)
        {
            if (mail.HasRewards)
                return mail.Claimed ? "Đã nhận" : "Có quà";
            return mail.Read ? "Đã đọc" : "Mới";
        }

        private string RewardSummary(Mail mail)
        {
            if (!mail.HasRewards)
                return "";
            var parts = new List<string>(mail.Rewards.Count);
            foreach (Reward reward in mail.Rewards)
                parts.Add(RewardLabel(reward));
            return string.Join(", ", parts);
        }

        // "+50 xu, +5 hạt Rau muống" – gộp các phần quà cùng loại + id.
        private string GrantedText(IEnumerable<Reward> rewards)
        {
            var totals = new List<Reward>();
            foreach (Reward reward in rewards)
            {
                Reward same = totals.Find(r => r.Kind == reward.Kind && r.Id == reward.Id);
                if (same != null)
                    same.Amount += reward.Amount;
                else
                    totals.Add(new Reward(reward.Kind, reward.Id, reward.Amount));
            }
            var parts = new List<string>(totals.Count);
            foreach (Reward reward in totals)
                parts.Add("+" + RewardLabel(reward));
            return parts.Count > 0 ? string.Join(", ", parts) : "Đã nhận quà";
        }

        private string RewardLabel(Reward reward)
        {
            switch (reward.Kind)
            {
                case RewardKind.Coins: return $"{UiFormat.Coins(reward.Amount)} xu";
                case RewardKind.Xp: return $"{reward.Amount} XP";
                case RewardKind.Seeds: return $"{reward.Amount} hạt {CropName(reward.Id)}";
                case RewardKind.Produce: return $"{reward.Amount} {CropName(reward.Id)}";
                case RewardKind.Bait: return $"{reward.Amount} {BaitName(reward.Id)}";
                default: return reward.Amount.ToString();
            }
        }

        private Sprite RewardIcon(Reward reward)
        {
            switch (reward.Kind)
            {
                case RewardKind.Coins:
                    return _coinIcon;
                case RewardKind.Seeds:
                    return _game.TryGetCrop(reward.Id, out CropDefinition seedCrop) ? seedCrop.SeedSprite : null;
                case RewardKind.Produce:
                    return _game.TryGetCrop(reward.Id, out CropDefinition crop) ? crop.ProduceIcon : null;
                case RewardKind.Bait:
                    FishingDefinition fishing = _game.Definition.Fishing;
                    return fishing != null && fishing.TryGetBait(reward.Id, out BaitDefinition bait) ? bait.Icon : null;
                default:
                    return null;
            }
        }

        private string CropName(string id) => _game.TryGetCrop(id, out CropDefinition crop) ? crop.DisplayName : id;

        private string BaitName(string id)
        {
            FishingDefinition fishing = _game.Definition.Fishing;
            return fishing != null && fishing.TryGetBait(id, out BaitDefinition bait) ? bait.DisplayName : id;
        }
    }
}
