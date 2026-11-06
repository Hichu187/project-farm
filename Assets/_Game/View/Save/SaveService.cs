using System;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Save
{
    public interface ISaveStore
    {
        string Read(string key);
        void Write(string key, string value);
        void Flush();
    }

    public sealed class PlayerPrefsStore : ISaveStore
    {
        public string Read(string key) => PlayerPrefs.GetString(key, null);
        public void Write(string key, string value) => PlayerPrefs.SetString(key, value);
        public void Flush() => PlayerPrefs.Save();
    }

    // Đọc, ghi save và xử lý mã sao lưu. Giữ thêm bản lưu liền trước để phòng bản chính bị hỏng.
    public sealed class SaveService
    {
        public const string MainKey = "veque.save";
        public const string PreviousKey = "veque.save.prev";

        private readonly ISaveStore _store;
        private readonly FarmDef _def;
        private readonly IClock _systemClock;

        private FarmState _state;
        private string _lastGoodJson;
        private bool _suspended;

        public SaveService(ISaveStore store, FarmDef def, IClock systemClock)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _def = def ?? throw new ArgumentNullException(nameof(def));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public IClock Clock { get; private set; }

        // Save do bản game mới hơn tạo (bản cũ còn trong cache): không ghi để khỏi mất tiến trình đó.
        public bool SavingBlocked { get; private set; }

        public FarmState Load()
        {
            long lastSeenAt = 0;
            if (!TryLoad(MainKey, ref lastSeenAt) && !SavingBlocked)
                TryLoad(PreviousKey, ref lastSeenAt);

            _state ??= FarmState.CreateNew(_def);
            Clock = new MonotonicClock(_systemClock, lastSeenAt);
            return _state;
        }

        public void Save()
        {
            if (_suspended || SavingBlocked)
                return;
            Write(ToJson());
        }

        public string ExportCode() => BackupCode.Encode(ToJson());

        // Thành công thì ngừng ghi, để trạng thái cũ không đè lên save vừa nhập trước khi tải lại scene.
        public SaveLoadResult Import(string code)
        {
            if (!BackupCode.TryDecode(code, out string json) || !TryParse(json, out SaveData data))
                return SaveLoadResult.Invalid;

            SaveLoadResult result = SaveMapper.TryFromSave(data, _def, out _);
            if (result != SaveLoadResult.Ok)
                return result;

            Write(json);
            _suspended = true;
            return SaveLoadResult.Ok;
        }

        private bool TryLoad(string key, ref long lastSeenAt)
        {
            string json = _store.Read(key);
            if (string.IsNullOrEmpty(json))
                return false;

            SaveLoadResult result = TryParse(json, out SaveData data)
                ? SaveMapper.TryFromSave(data, _def, out _state)
                : SaveLoadResult.Invalid;

            switch (result)
            {
                case SaveLoadResult.Ok:
                    lastSeenAt = data.lastSeenAt;
                    _lastGoodJson = json;
                    return true;
                case SaveLoadResult.NewerVersion:
                    SavingBlocked = true;
                    Debug.LogWarning($"Save '{key}' do bản game mới hơn tạo, tạm không ghi đè.");
                    return false;
                default:
                    Debug.LogWarning($"Save '{key}' bị hỏng, bỏ qua.");
                    return false;
            }
        }

        private void Write(string json)
        {
            if (_lastGoodJson != null)
                _store.Write(PreviousKey, _lastGoodJson);
            _store.Write(MainKey, json);
            _store.Flush();
            _lastGoodJson = json;
        }

        private string ToJson() => JsonUtility.ToJson(SaveMapper.ToSave(_state, _def, Clock.UtcNowSeconds));

        private static bool TryParse(string json, out SaveData data)
        {
            try
            {
                data = JsonUtility.FromJson<SaveData>(json);
                return data != null;
            }
            catch (ArgumentException)
            {
                data = null;
                return false;
            }
        }
    }
}
