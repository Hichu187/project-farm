using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Toàn bộ data của nông trại: các khu theo thứ tự mở khóa, các loại cây, vốn ban đầu.
    public sealed class FarmDef
    {
        private readonly Dictionary<string, CropDef> _crops = new Dictionary<string, CropDef>();
        private readonly Dictionary<string, int> _zoneIndices = new Dictionary<string, int>();

        public IReadOnlyList<ZoneDef> Zones { get; }
        public IReadOnlyList<CropDef> Crops { get; }
        public long StartCoins { get; }
        public IReadOnlyDictionary<string, int> StartSeeds { get; }
        // Tưới giảm bao nhiêu % tổng thời gian lớn của cây.
        public int WaterReducePercent { get; }
        public LevelDef Levels { get; }
        // Cây tặng 1 hạt khi người chơi bị kẹt (hết xu, hạt, nông sản, cây); null = không cứu.
        public string RescueCropId { get; }
        public OrderDef Orders { get; }
        public FishingDef Fishing { get; }
        // Bảng âm lịch cho GameCalendar (Đợt 2 Phần 2); Empty = không có.
        public LunarTable Lunar { get; }
        // Thư theo bản build (Đợt 2 Phần 3).
        public IReadOnlyList<BuildMailDef> BuildMails { get; }

        public FarmDef(IEnumerable<ZoneDef> zones, IEnumerable<CropDef> crops, long startCoins,
            IReadOnlyDictionary<string, int> startSeeds = null, int waterReducePercent = 0,
            LevelDef levels = null, string rescueCropId = null, OrderDef orders = null, FishingDef fishing = null,
            LunarTable lunar = null, IEnumerable<BuildMailDef> buildMails = null)
        {
            if (zones == null)
                throw new ArgumentNullException(nameof(zones));
            if (crops == null)
                throw new ArgumentNullException(nameof(crops));
            if (startCoins < 0)
                throw new ArgumentOutOfRangeException(nameof(startCoins));
            if (waterReducePercent < 0 || waterReducePercent > 100)
                throw new ArgumentOutOfRangeException(nameof(waterReducePercent));

            var zoneList = new List<ZoneDef>(zones);
            if (zoneList.Count == 0)
                throw new ArgumentException("Cần ít nhất 1 khu.", nameof(zones));
            for (int i = 0; i < zoneList.Count; i++)
            {
                ZoneDef zone = zoneList[i] ?? throw new ArgumentException("Có khu null.", nameof(zones));
                if (_zoneIndices.ContainsKey(zone.Id))
                    throw new ArgumentException($"Trùng zone id '{zone.Id}'.", nameof(zones));
                _zoneIndices.Add(zone.Id, i);
            }

            var cropList = new List<CropDef>(crops);
            foreach (CropDef crop in cropList)
            {
                if (crop == null)
                    throw new ArgumentException("Có cây null.", nameof(crops));
                if (_crops.ContainsKey(crop.Id))
                    throw new ArgumentException($"Trùng crop id '{crop.Id}'.", nameof(crops));
                _crops.Add(crop.Id, crop);
            }

            var seeds = new Dictionary<string, int>();
            if (startSeeds != null)
            {
                foreach (KeyValuePair<string, int> pair in startSeeds)
                {
                    if (!_crops.ContainsKey(pair.Key))
                        throw new ArgumentException($"Hạt ban đầu của cây không tồn tại '{pair.Key}'.", nameof(startSeeds));
                    if (pair.Value < 0)
                        throw new ArgumentOutOfRangeException(nameof(startSeeds));
                    if (pair.Value > 0)
                        seeds.Add(pair.Key, pair.Value);
                }
            }

            if (rescueCropId != null)
            {
                if (!_crops.TryGetValue(rescueCropId, out CropDef rescue))
                    throw new ArgumentException($"Cây cứu kẹt không tồn tại '{rescueCropId}'.", nameof(rescueCropId));
                if (rescue.UnlockLevel != 1)
                    throw new ArgumentException($"Cây cứu kẹt '{rescueCropId}' phải mở từ cấp 1.", nameof(rescueCropId));
            }

            Zones = zoneList.AsReadOnly();
            Crops = cropList.AsReadOnly();
            StartCoins = startCoins;
            StartSeeds = seeds;
            WaterReducePercent = waterReducePercent;
            Levels = levels ?? LevelDef.None;
            RescueCropId = rescueCropId;
            Orders = orders ?? OrderDef.None;
            Fishing = fishing ?? FishingDef.None;
            Lunar = lunar ?? LunarTable.Empty;

            // Quà trong thư theo build phải trỏ tới cây / mồi có thật (sai data thì báo ngay lúc nạp).
            var mails = new List<BuildMailDef>();
            var mailIds = new HashSet<string>();
            if (buildMails != null)
            {
                foreach (BuildMailDef mail in buildMails)
                {
                    if (mail == null)
                        throw new ArgumentException("Có thư null.", nameof(buildMails));
                    if (!mailIds.Add(mail.Id))
                        throw new ArgumentException($"Trùng id thư '{mail.Id}'.", nameof(buildMails));
                    foreach (Reward reward in mail.Rewards)
                    {
                        if (!IsValidReward(reward, out string error))
                            throw new ArgumentException($"Thư '{mail.Id}': {error}", nameof(buildMails));
                    }
                    mails.Add(mail);
                }
            }
            BuildMails = mails.AsReadOnly();
        }

        // Quà hợp lệ: số lượng > 0, loại biết, id trỏ tới cây (hạt / nông sản) hoặc mồi có thật.
        public bool IsValidReward(Reward reward, out string error)
        {
            error = null;
            if (reward == null)
                error = "quà null.";
            else if (reward.Amount <= 0)
                error = "số lượng quà phải > 0.";
            else if (!Enum.IsDefined(typeof(RewardKind), reward.Kind))
                error = $"loại quà lạ {(int)reward.Kind}.";
            else if ((reward.Kind == RewardKind.Seeds || reward.Kind == RewardKind.Produce) && !_crops.ContainsKey(reward.Id ?? ""))
                error = $"cây '{reward.Id}' không có.";
            else if (reward.Kind == RewardKind.Bait && !Fishing.TryGetBait(reward.Id, out _))
                error = $"mồi '{reward.Id}' không có.";
            else if ((reward.Kind == RewardKind.Seeds || reward.Kind == RewardKind.Produce || reward.Kind == RewardKind.Bait)
                && reward.Amount > int.MaxValue)
                error = "số lượng quà quá lớn.";
            return error == null;
        }

        public bool TryGetCrop(string id, out CropDef crop)
        {
            if (id == null)
            {
                crop = null;
                return false;
            }
            return _crops.TryGetValue(id, out crop);
        }

        public bool TryGetZoneIndex(string id, out int index)
        {
            if (id == null)
            {
                index = -1;
                return false;
            }
            return _zoneIndices.TryGetValue(id, out index);
        }
    }
}
