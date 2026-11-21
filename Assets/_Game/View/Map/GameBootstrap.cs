using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VeQue.Logic;
using VeQue.View.Data;
using VeQue.View.Save;

namespace VeQue.View.Map
{
    // Dựng Farm từ data và save lúc khởi động, rồi tự lưu khi có thay đổi. Script khác lấy Farm từ Start trở đi.
    public sealed class GameBootstrap : MonoBehaviour
    {
        // Gộp nhiều lệnh liên tiếp (gieo nhiều ô) vào một lần ghi.
        private const float SaveDelaySeconds = 1f;
        private const float NoPendingSave = -1f;
        // Khoảng tua thời gian trong Editor, lưu cùng save để thoát Play không làm thời gian đứng yên.
        public const string TimeOffsetKey = "veque.debug.timeOffset";

        [SerializeField] private FarmDefinition _definition;

        private readonly Dictionary<string, CropDefinition> _crops = new Dictionary<string, CropDefinition>();
        private OffsetClock _clock;
        private SaveService _save;
        private float _saveAt = NoPendingSave;
        // Đơn bị bỏ có thể hết thời gian chờ khi người chơi không làm gì, nên kiểm tra mỗi giây.
        private const float OrderRefreshInterval = 1f;
        private float _nextOrderRefresh;

        public FarmDefinition Definition => _definition;
        public Farm Farm { get; private set; }
        // Hàng việc của nhân vật (gieo / tưới / thu hoạch). Không lưu vào save.
        public FarmTaskQueue Tasks { get; } = new FarmTaskQueue();

        // Báo ngay trước khi lưu lúc tab bị ẩn / app tạm dừng / thoát: nhân vật làm hết việc còn lại ở đây.
        public event System.Action PageHiding;
        public bool SavingBlocked => _save.SavingBlocked;
        public bool OldSaveDiscarded => _save.OldSaveDiscarded;

        public long TimeOffsetSeconds => _clock.OffsetSeconds;
        // Giờ của game (đã tính khoảng tua và chống lùi giờ).
        public long NowSeconds => _save.Clock.UtcNowSeconds;

        public string ExportBackupCode() => _save.ExportCode();

        public bool TryGetCrop(string id, out CropDefinition crop)
        {
            if (id == null)
            {
                crop = null;
                return false;
            }
            return _crops.TryGetValue(id, out crop);
        }

        private void Awake()
        {
            FarmDef def = _definition.ToDef();
            _clock = new OffsetClock(new SystemClock(), LoadTimeOffset());
            _save = new SaveService(new PlayerPrefsStore(), def, _clock);
            FarmState state = _save.Load();
            // Ngày, khung giờ, giới hạn đơn: giờ VN cố định (GameCalendar, Đợt 2 Phần 2) – không đọc múi giờ máy.
            Farm = new Farm(def, state, _save.Clock);
            Farm.Changed += ScheduleSave;
            Farm.LeveledUp += LogLevelUp;
            Farm.Rescued += LogRescued;

            foreach (CropDefinition crop in _definition.Crops)
            {
                if (crop != null && crop.Enabled)
                    _crops[crop.Id] = crop;
            }

            WebBridge.PageHidden += HandlePageHidden;
            WebBridge.ImportHandler = HandleImport;
            WebBridge.RegisterPageEvents();
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            if (now >= _nextOrderRefresh)
            {
                _nextOrderRefresh = now + OrderRefreshInterval;
                Farm.RefreshOrders();
                // Sang ngày mới: dọn thư, phát thư theo build (Đợt 2 Phần 3).
                Farm.RefreshMail();
                // Điểm danh (Đợt 2 Phần 10): 22/11 người chơi tự bấm Nhận trong popup (CheckInPopup tự mở khi đủ điều kiện).
                // Quà sự kiện hôm nay (Đợt 2 Phần 7): mỗi (sự kiện, ngày) gửi 1 thư.
                Farm.TryEventMail();
            }

            if (_saveAt != NoPendingSave && now >= _saveAt)
                FlushSave();
        }

        // Nhận quà trong thư: cộng quà + đánh dấu đã nhận rồi LƯU NGAY (tải lại trang giữa chừng không nhận trùng).
        public FarmResult ClaimMail(string id, out IReadOnlyList<Reward> granted)
        {
            FarmResult result = Farm.ClaimMail(id, out granted);
            if (result == FarmResult.Ok)
                FlushSave();
            return result;
        }

        public FarmResult ClaimAllMail(out List<Reward> granted)
        {
            FarmResult result = Farm.ClaimAllMail(out granted);
            if (result == FarmResult.Ok)
                FlushSave();
            return result;
        }

        // Giao đơn qua đây để có log; UI (Đợt 4) cũng gọi hàm này.
        public FarmResult DeliverOrder(int slot)
        {
            Farm.TryGetOrder(slot, out OrderState order);
            string customer = order?.NpcId;
            long coins = 0, xp = 0;
            if (order != null)
                Farm.GetOrderReward(order, out coins, out xp);

            FarmResult result = Farm.DeliverOrder(slot);
            if (result == FarmResult.Ok)
                Debug.Log($"Giao đơn {customer}: +{coins} xu, +{xp} XP", this);
            return result;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                HandlePageHidden();
        }

        private void OnApplicationQuit() => HandlePageHidden();

        private void HandlePageHidden()
        {
            PageHiding?.Invoke();
            FlushSave();
        }

        private void OnDestroy()
        {
            if (Farm != null)
            {
                Farm.Changed -= ScheduleSave;
                Farm.LeveledUp -= LogLevelUp;
                Farm.Rescued -= LogRescued;
            }

            WebBridge.PageHidden -= HandlePageHidden;
            // Scene mới có thể đã gán handler của nó trước khi bản cũ bị hủy.
            if (WebBridge.ImportHandler == (System.Func<string, int>)HandleImport)
                WebBridge.ImportHandler = null;
        }

        private void ScheduleSave()
        {
            if (_saveAt == NoPendingSave)
                _saveAt = Time.unscaledTime + SaveDelaySeconds;
        }

        private void FlushSave()
        {
            if (_saveAt == NoPendingSave)
                return;

            _saveAt = NoPendingSave;
            try
            {
                _save.Save();
            }
            catch (PlayerPrefsException exception)
            {
                Debug.LogError($"Không ghi được save: {exception.Message}", this);
            }
        }

        // Tải lại scene để mọi màn hình dựng lại từ save vừa nhập.
        private int HandleImport(string code)
        {
            SaveLoadResult result = _save.Import(code);
            if (result == SaveLoadResult.Ok)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return (int)result;
        }

        private void LogLevelUp(int level)
        {
            var unlocked = new List<string>();
            foreach (CropDefinition crop in _crops.Values)
            {
                if (crop.UnlockLevel == level)
                    unlocked.Add(crop.DisplayName);
            }

            int coins = Farm.Definition.Levels.CoinsPerLevel * level;
            string crops = unlocked.Count > 0
                ? $", mở {string.Join(", ", unlocked)} (+{Farm.Definition.Levels.UnlockSeedCount} hạt mỗi cây)"
                : string.Empty;
            Debug.Log($"Lên cấp {level}: +{coins} xu{crops}. XP {Farm.State.Xp}/{Farm.GetXpToNext()}", this);
        }

        private void LogRescued(string cropId)
        {
            string name = TryGetCrop(cropId, out CropDefinition crop) ? crop.DisplayName : cropId;
            Debug.Log($"Hết xu, hạt, nông sản: tặng 1 hạt {name}", this);
        }

        // Trong bản build không tua thời gian.
        private static long LoadTimeOffset()
        {
#if UNITY_EDITOR
            return long.TryParse(PlayerPrefs.GetString(TimeOffsetKey, "0"), out long offset) && offset > 0 ? offset : 0;
#else
            return 0;
#endif
        }

#if UNITY_EDITOR
        // Gọi từ cửa sổ Về Quê → Tua thời gian khi đang Play.
        public void AdvanceTime(long seconds)
        {
            _clock.Advance(seconds);
            PlayerPrefs.SetString(TimeOffsetKey, _clock.OffsetSeconds.ToString());
            PlayerPrefs.Save();
            Debug.Log($"Tua thời gian +{seconds / 60} phút (tổng đã tua {_clock.OffsetSeconds / 60} phút)", this);
        }

        // Trong Editor không có màn sao lưu HTML. Chạy khi đang Play, từ menu ba chấm của component.
        [ContextMenu("Nhập mã từ clipboard")]
        private void ImportFromClipboard()
        {
            if (!Application.isPlaying)
                return;

            Debug.Log($"Nhập mã: {(SaveLoadResult)HandleImport(GUIUtility.systemCopyBuffer)}", this);
        }
#endif
    }
}
