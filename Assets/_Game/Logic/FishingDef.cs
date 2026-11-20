using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    public enum FishRarity
    {
        Common = 0,     // Thường
        Uncommon = 1,   // Khá
        Rare = 2,       // Hiếm
        Epic = 3,       // Quý
        Legendary = 4,  // Huyền thoại
    }

    // Cá đột biến lấp lánh (Đợt 2 Phần 18). Số cố định vì lưu trong save: không đổi số, kiểu mới thêm số mới.
    public enum ShinyKind
    {
        None = 0,
        Gold = 1,        // Hoàng kim
        Aurora = 2,      // Cực quang (bậc hiếm)
        Ice = 3,         // Băng tinh
        Nebula = 4,      // Tinh vân
        Ruby = 5,        // Hồng ngọc
        Onyx = 6,        // Hư không
        Silver = 7,      // Bạc
        DarkAurora = 8,  // Tinh dạ (bậc hiếm)
    }

    // Phao rơi gần / vừa / xa bờ (theo lực ném). Cá có thể sống ở nhiều độ sâu.
    [Flags]
    public enum FishDepth
    {
        None = 0,
        Near = 1,
        Mid = 2,
        Far = 4,
        All = Near | Mid | Far,
    }

    // Một loài cá (MVP-Plan Phần 10 mục 4, 4b). Cân nặng tính bằng gam.
    public sealed class FishDef
    {
        public string Id { get; }
        public FishRarity Rarity { get; }
        public int MinGrams { get; }
        public int MaxGrams { get; }
        public int BasePrice { get; }
        public int BaseXp { get; }
        // Hệ số theo 7 khung giờ (DaySlot): 0 = không có, 1 = bình thường, > 1 = ăn mạnh.
        public IReadOnlyList<float> SlotFactors { get; }
        public FishDepth Depths { get; }
        public IReadOnlyList<string> PreferredBaits { get; }

        // Độ khó kéo cá (1 lượt, chốt 13/11/2026): chuỗi MinArrows..MaxArrows mũi tên, nhập trong PullSeconds giây.
        public int MinArrows { get; }
        public int MaxArrows { get; }
        public float PullSeconds { get; }
        // Khung giật câu sau khi phao chìm (giây).
        public float HookWindowSeconds { get; }
        public int SlipPercent { get; }
        // Có mũi tên đỏ (bấm hướng ngược lại).
        public bool RedArrows { get; }

        public FishDef(string id, FishRarity rarity, int minGrams, int maxGrams, int basePrice, int baseXp,
            IReadOnlyList<float> slotFactors, FishDepth depths, IReadOnlyList<string> preferredBaits,
            int minArrows, int maxArrows, float pullSeconds,
            float hookWindowSeconds, int slipPercent, bool redArrows = false)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Cá cần id.", nameof(id));
            if (minGrams <= 0 || maxGrams < minGrams)
                throw new ArgumentOutOfRangeException(nameof(maxGrams), $"Cân nặng cá '{id}' không hợp lệ.");
            if (basePrice < 0 || baseXp < 0)
                throw new ArgumentOutOfRangeException(nameof(basePrice));
            if (slotFactors == null || slotFactors.Count != DaySlots.Count)
                throw new ArgumentException($"Cá '{id}' cần đủ {DaySlots.Count} hệ số khung giờ.", nameof(slotFactors));
            foreach (float factor in slotFactors)
            {
                if (factor < 0f)
                    throw new ArgumentOutOfRangeException(nameof(slotFactors));
            }
            if (depths == FishDepth.None)
                throw new ArgumentException($"Cá '{id}' cần ít nhất 1 độ sâu.", nameof(depths));
            if (minArrows < 1 || maxArrows < minArrows)
                throw new ArgumentOutOfRangeException(nameof(minArrows), $"Độ khó cá '{id}' không hợp lệ.");
            if (pullSeconds <= 0f || hookWindowSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(pullSeconds), $"Thời gian của cá '{id}' không hợp lệ.");
            if (slipPercent < 0 || slipPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(slipPercent));

            Id = id;
            Rarity = rarity;
            MinGrams = minGrams;
            MaxGrams = maxGrams;
            BasePrice = basePrice;
            BaseXp = baseXp;
            SlotFactors = new List<float>(slotFactors).AsReadOnly();
            Depths = depths;
            PreferredBaits = new List<string>(preferredBaits ?? Array.Empty<string>()).AsReadOnly();
            MinArrows = minArrows;
            MaxArrows = maxArrows;
            PullSeconds = pullSeconds;
            HookWindowSeconds = hookWindowSeconds;
            SlipPercent = slipPercent;
            RedArrows = redArrows;
        }

        public float GetSlotFactor(DaySlot slot) => SlotFactors[(int)slot];

        public bool Prefers(string baitId)
        {
            foreach (string bait in PreferredBaits)
            {
                if (bait == baitId)
                    return true;
            }
            return false;
        }
    }

    // Cần câu (mục 5). Rank càng cao: vùng Perfect rộng hơn, thanh lực chậm hơn, thêm thời gian kéo cá, ít trượt hơn.
    public sealed class RodDef
    {
        public string Id { get; }
        public int Rank { get; }
        // Nửa độ rộng vùng Perfect trên thanh lực (0..1).
        public float PerfectHalfWidth { get; }
        // Thời gian thanh lực chạy 0 → 100% → 0 (giây) – View dùng.
        public float BarPeriodSeconds { get; }
        // Thêm thời gian nhập chuỗi mũi tên khi kéo cá (% của FishDef.PullSeconds).
        public int BonusTimePercent { get; }
        public int SlipReductionPercent { get; }
        public int RareBonusPercent { get; }
        public int MaxDurability { get; }
        public long Price { get; }
        public int UnlockLevel { get; }

        public RodDef(string id, int rank, float perfectHalfWidth, float barPeriodSeconds, int bonusTimePercent,
            int slipReductionPercent, int rareBonusPercent, int maxDurability, long price, int unlockLevel)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Cần câu cần id.", nameof(id));
            if (perfectHalfWidth <= 0f || perfectHalfWidth >= 0.5f || barPeriodSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(perfectHalfWidth));
            if (bonusTimePercent < 0 || maxDurability <= 0)
                throw new ArgumentOutOfRangeException(nameof(bonusTimePercent));
            if (slipReductionPercent < 0 || rareBonusPercent < 0 || price < 0 || unlockLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(price));

            Id = id;
            Rank = rank;
            PerfectHalfWidth = perfectHalfWidth;
            BarPeriodSeconds = barPeriodSeconds;
            BonusTimePercent = bonusTimePercent;
            SlipReductionPercent = slipReductionPercent;
            RareBonusPercent = rareBonusPercent;
            MaxDurability = maxDurability;
            Price = price;
            UnlockLevel = unlockLevel;
        }
    }

    // Mồi (mục 6): mỗi lần ném tốn 1; loài ưa mồi này cắn nhanh, ra nhiều hơn.
    public sealed class BaitDef
    {
        public string Id { get; }
        public long Price { get; }
        // false = không bán ở Cửa hàng (chỉ đào được / tặng).
        public bool Buyable { get; }
        public int RareBonusPercent { get; }
        // Cấp tối thiểu để mua (13/11/2026). Mồi đang có thì dùng được ở mọi cấp.
        public int UnlockLevel { get; }

        public BaitDef(string id, long price, int rareBonusPercent = 0, bool buyable = true, int unlockLevel = 1)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Mồi cần id.", nameof(id));
            if (price < 0 || rareBonusPercent < 0)
                throw new ArgumentOutOfRangeException(nameof(price));
            if (unlockLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(unlockLevel));
            Id = id;
            Price = price;
            Buyable = buyable;
            RareBonusPercent = rareBonusPercent;
            UnlockLevel = unlockLevel;
        }
    }

    // Các con số chung của câu cá. Giá trị mặc định là đề xuất 12/11/2026, chỉnh trong data.
    // [Serializable] để FishingDefinition (ScriptableObject) giữ và chỉnh được trong Inspector.
    [Serializable]
    public sealed class FishingTuning
    {
        // Ném: lực 0..1; Perfect quanh PerfectCenter ± RodDef.PerfectHalfWidth; Tốt rộng gấp GoodWidthMultiplier.
        public float PerfectCenter = 0.9f;
        public float GoodWidthMultiplier = 2.5f;
        // Lực dưới mức này là ném hỏng: phao sát bờ, chỉ có cá Thường.
        public float WeakBelow = 0.25f;
        public float NearBelow = 0.45f;
        public float MidBelow = 0.75f;

        // Trọng số chọn cá theo độ hiếm (Thường → Huyền thoại).
        public int[] RarityWeights = { 60, 25, 10, 4, 1 };
        public int PreferredBaitMultiplier = 3;
        // Thưởng tỉ lệ cá từ Hiếm trở lên (%), cộng dồn với cần và mồi.
        public int PerfectRareBonusPercent = 50;
        public int GoodRareBonusPercent = 15;

        // Chờ cắn (giây); mồi ưa thích / ném Perfect nhân thêm hệ số < 1. (13/11/2026: 3–15 → 2–8 cho nhịp nhanh hơn.)
        public float BiteMinSeconds = 2f;
        public float BiteMaxSeconds = 8f;
        public float PreferredBiteFactor = 0.7f;
        public float PerfectBiteFactor = 0.7f;
        public int MaxNibbles = 3;
        // Khoảng cách tối thiểu giữa các lần rỉa và tới lúc cắn (giây).
        public float NibbleGapSeconds = 0.8f;

        // Kéo cá 1 lượt (chốt 13/11/2026): nhập đúng hết chuỗi trong thời gian → câu được. Xếp hạng theo thời gian đã dùng:
        // ≤ PerfectTimePercent% = Perfect, ≤ GoodTimePercent% = Tốt, còn lại = Thường.
        // Perfect: +PerfectXpBonusPercent% XP. Cân nặng = nhẹ nhất + (nặng nhất − nhẹ nhất) × u^số mũ (số mũ lớn = lệch nhẹ).
        public int PerfectTimePercent = 40;
        public int GoodTimePercent = 70;
        public int PerfectXpBonusPercent = 50;
        public float PerfectWeightExponent = 1f;
        public float GoodWeightExponent = 2f;
        public float NormalWeightExponent = 3f;
        public int RedArrowPercent = 25;
        // Chuỗi mũi tên ít nhất bao nhiêu (sau khi ném Perfect bớt 1). 14/11/2026: tối thiểu 4.
        public int MinPullArrows = 4;
        // Móc được cá: hiện chuỗi mũi tên PullPreviewSeconds giây rồi mới tính giờ (bấm đúng mũi tên đầu thì tính luôn).
        // Thời gian xem trước không tính vào xếp hạng (View đo giờ từ lúc bắt đầu tính).
        public float PullPreviewSeconds = 1.2f;
        // Đợt 2 Phần 6 (20/11/2026): thời gian kéo = (giây kéo của cá + số mũi tên × PullSecondsPerArrow) × (1 + thưởng cần),
        // để chuỗi dài bị reset gần cuối vẫn kịp làm lại trên điện thoại.
        public float PullSecondsPerArrow = 0.45f;

        // Độ bền: mỗi lượt câu trừ 1 lúc ném (chốt 13/11/2026). Cần chỉ hỏng khi hết độ bền (không còn gãy khi kéo).
        public int CastDurabilityCost = 1;
        // Sửa cần (Đợt 2 Phần 1, 20/11/2026): về đầy độ bền, độ bền tối đa giữ nguyên.
        // Giá sửa = giá cần × RepairPricePercent% × phần độ bền còn thiếu, làm tròn lên, tối thiểu 1 xu.
        public int RepairPricePercent = 30;
        // Cảnh báo "sắp hỏng" khi độ bền còn ≤ LowDurabilityPercent% độ bền tối đa.
        public int LowDurabilityPercent = 10;

        // Giá / XP theo cân nặng: nhẹ nhất × WeightValueMinPercent%, nặng nhất × WeightValueMaxPercent%.
        public int WeightValueMinPercent = 60;
        public int WeightValueMaxPercent = 140;

        // Cá shiny (Đợt 2 Phần 18, chốt 20/11/2026): mỗi lần câu được cá có ShinyChancePer10000 / 10000 ra shiny.
        // Ra shiny thì tung bậc (ShinyRareTierPercent% là bậc hiếm) rồi chọn đều 1 kiểu trong bậc.
        // Giá = giá theo cân nặng × hệ số của bậc. XP không đổi.
        public int ShinyChancePer10000 = 100;
        public float ShinyRareTierPercent = 3.2f;
        public ShinyKind[] ShinyCommonKinds =
        {
            ShinyKind.Gold, ShinyKind.Ice, ShinyKind.Nebula, ShinyKind.Ruby, ShinyKind.Onyx, ShinyKind.Silver,
        };
        public ShinyKind[] ShinyRareKinds = { ShinyKind.Aurora, ShinyKind.DarkAurora };
        public int ShinyCommonPriceMultiplier = 3;
        public int ShinyRarePriceMultiplier = 10;

        public bool IsRareShiny(ShinyKind kind) => kind != ShinyKind.None && Array.IndexOf(ShinyRareKinds, kind) >= 0;

        // Hệ số giá: không shiny = 1.
        public int ShinyPriceMultiplier(ShinyKind kind)
        {
            if (kind == ShinyKind.None)
                return 1;
            return IsRareShiny(kind) ? ShinyRarePriceMultiplier : ShinyCommonPriceMultiplier;
        }

        internal void Validate()
        {
            if (ShinyChancePer10000 < 0 || ShinyChancePer10000 > 10000)
                throw new ArgumentException("Tỉ lệ cá shiny không hợp lệ.");
            if (ShinyRareTierPercent < 0f || ShinyRareTierPercent > 100f)
                throw new ArgumentException("Tỉ lệ bậc shiny hiếm không hợp lệ.");
            if (ShinyCommonPriceMultiplier < 1 || ShinyRarePriceMultiplier < 1)
                throw new ArgumentException("Hệ số giá cá shiny không hợp lệ.");
            ValidateShinyKinds(ShinyCommonKinds, ShinyRareKinds);
            ValidateShinyKinds(ShinyRareKinds, ShinyCommonKinds);
            if (RarityWeights == null || RarityWeights.Length != 5)
                throw new ArgumentException("Cần 5 trọng số độ hiếm.");
            if (BiteMinSeconds <= 0f || BiteMaxSeconds < BiteMinSeconds)
                throw new ArgumentException("Thời gian chờ cắn không hợp lệ.");
            if (PerfectTimePercent <= 0 || GoodTimePercent < PerfectTimePercent || GoodTimePercent > 100 || PerfectXpBonusPercent < 0)
                throw new ArgumentException("Xếp hạng kéo cá không hợp lệ.");
            if (MinPullArrows < 1)
                throw new ArgumentException("Số mũi tên tối thiểu không hợp lệ.");
            if (PullPreviewSeconds < 0f || PullSecondsPerArrow < 0f)
                throw new ArgumentException("Thời gian xem trước chuỗi mũi tên không hợp lệ.");
            if (PerfectWeightExponent <= 0f || GoodWeightExponent <= 0f || NormalWeightExponent <= 0f)
                throw new ArgumentException("Số mũ cân nặng không hợp lệ.");
            if (CastDurabilityCost < 0)
                throw new ArgumentException("Mức trừ độ bền không hợp lệ.");
            if (RepairPricePercent < 0 || LowDurabilityPercent < 0 || LowDurabilityPercent > 100)
                throw new ArgumentException("Thông số sửa cần không hợp lệ.");
        }

        // Mỗi bậc có ít nhất 1 kiểu; không None, không kiểu lạ, không trùng trong bậc hay với bậc kia.
        private static void ValidateShinyKinds(ShinyKind[] kinds, ShinyKind[] other)
        {
            if (kinds == null || kinds.Length == 0)
                throw new ArgumentException("Mỗi bậc cá shiny cần ít nhất 1 kiểu.");
            for (int i = 0; i < kinds.Length; i++)
            {
                ShinyKind kind = kinds[i];
                if (kind == ShinyKind.None || !Enum.IsDefined(typeof(ShinyKind), kind))
                    throw new ArgumentException($"Kiểu cá shiny không hợp lệ: {kind}.");
                if (Array.IndexOf(kinds, kind) != i || (other != null && Array.IndexOf(other, kind) >= 0))
                    throw new ArgumentException($"Trùng kiểu cá shiny: {kind}.");
            }
        }

        // Giá sửa cần về đầy: 0 khi đã đầy, còn lại ít nhất 1 xu.
        public long RepairPrice(long rodPrice, int durability, int maxDurability)
        {
            if (maxDurability <= 0)
                return 0;
            long missing = Math.Max(0, Math.Min(maxDurability, maxDurability - durability));
            if (missing == 0)
                return 0;
            long numerator = Math.Max(0, rodPrice) * RepairPricePercent * missing;
            long denominator = 100L * maxDurability;
            return Math.Max(1, (numerator + denominator - 1) / denominator);
        }

        // Độ bền còn ≤ LowDurabilityPercent% (gồm cả khi đã hỏng).
        public bool IsLowDurability(int durability, int maxDurability)
            => (long)durability * 100 <= (long)maxDurability * LowDurabilityPercent;

    }

    // Toàn bộ data câu cá (Phần 10). FishingDef.None = chưa có câu cá.
    public sealed class FishingDef
    {
        public static readonly FishingDef None = new FishingDef(
            Array.Empty<FishDef>(), Array.Empty<RodDef>(), Array.Empty<BaitDef>(), null, null, 0);

        private readonly Dictionary<string, FishDef> _fish = new Dictionary<string, FishDef>();
        private readonly Dictionary<string, RodDef> _rods = new Dictionary<string, RodDef>();
        private readonly Dictionary<string, BaitDef> _baits = new Dictionary<string, BaitDef>();

        public IReadOnlyList<FishDef> Fish { get; }
        public IReadOnlyList<RodDef> Rods { get; }
        public IReadOnlyList<BaitDef> Baits { get; }
        public int UnlockLevel { get; }
        // Tặng khi mở câu cá (1 lần).
        public string StarterRodId { get; }
        public string StarterBaitId { get; }
        public int StarterBaitCount { get; }
        // Giỏ cá (chốt 13/11/2026): mở sẵn BasketStartSlots ô, mua lần lượt từng ô; BasketSlotPrices[k] = giá ô thứ
        // BasketStartSlots + k + 1. Tối đa BasketMaxSlots ô.
        public int BasketStartSlots { get; }
        public IReadOnlyList<long> BasketSlotPrices { get; }
        public int BasketMaxSlots => BasketStartSlots + BasketSlotPrices.Count;
        // Đào được mồi khi thu hoạch.
        public string WormBaitId { get; }
        public int WormChancePercent { get; }
        public int WormMin { get; }
        public int WormMax { get; }
        public FishingTuning Tuning { get; }

        public bool IsEnabled => Fish.Count > 0 && Rods.Count > 0 && Baits.Count > 0;

        public FishingDef(IEnumerable<FishDef> fish, IEnumerable<RodDef> rods, IEnumerable<BaitDef> baits,
            string starterRodId, string starterBaitId, int starterBaitCount, int unlockLevel = 1,
            int basketStartSlots = 8, IReadOnlyList<long> basketSlotPrices = null,
            string wormBaitId = null, int wormChancePercent = 0, int wormMin = 1, int wormMax = 1,
            FishingTuning tuning = null)
        {
            Fish = Index(fish, _fish, f => f.Id, "cá");
            Rods = Index(rods, _rods, r => r.Id, "cần");
            Baits = Index(baits, _baits, b => b.Id, "mồi");

            if (unlockLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(unlockLevel));
            if (starterRodId != null && !_rods.ContainsKey(starterRodId))
                throw new ArgumentException($"Cần tặng không tồn tại '{starterRodId}'.", nameof(starterRodId));
            if (starterBaitId != null && !_baits.ContainsKey(starterBaitId))
                throw new ArgumentException($"Mồi tặng không tồn tại '{starterBaitId}'.", nameof(starterBaitId));
            if (wormBaitId != null && !_baits.ContainsKey(wormBaitId))
                throw new ArgumentException($"Mồi đào được không tồn tại '{wormBaitId}'.", nameof(wormBaitId));
            if (starterBaitCount < 0 || wormChancePercent < 0 || wormChancePercent > 100 || wormMin < 0 || wormMax < wormMin)
                throw new ArgumentOutOfRangeException(nameof(wormChancePercent));

            if (basketStartSlots < 1)
                throw new ArgumentOutOfRangeException(nameof(basketStartSlots), "Giỏ phải có ít nhất 1 ô mở sẵn.");
            var prices = new List<long>(basketSlotPrices ?? Array.Empty<long>());
            foreach (long price in prices)
            {
                if (price < 0)
                    throw new ArgumentOutOfRangeException(nameof(basketSlotPrices));
            }

            UnlockLevel = unlockLevel;
            StarterRodId = starterRodId;
            StarterBaitId = starterBaitId;
            StarterBaitCount = starterBaitCount;
            BasketStartSlots = basketStartSlots;
            BasketSlotPrices = prices.AsReadOnly();
            WormBaitId = wormBaitId;
            WormChancePercent = wormChancePercent;
            WormMin = wormMin;
            WormMax = wormMax;
            Tuning = tuning ?? new FishingTuning();
            Tuning.Validate();
        }

        public bool TryGetFish(string id, out FishDef fish) => TryGet(_fish, id, out fish);
        public bool TryGetRod(string id, out RodDef rod) => TryGet(_rods, id, out rod);
        public bool TryGetBait(string id, out BaitDef bait) => TryGet(_baits, id, out bait);

        private static bool TryGet<T>(Dictionary<string, T> map, string id, out T value) where T : class
        {
            if (id == null)
            {
                value = null;
                return false;
            }
            return map.TryGetValue(id, out value);
        }

        private static IReadOnlyList<T> Index<T>(IEnumerable<T> items, Dictionary<string, T> map, Func<T, string> getId, string label)
            where T : class
        {
            var list = new List<T>();
            if (items == null)
                return list.AsReadOnly();
            foreach (T item in items)
            {
                if (item == null)
                    throw new ArgumentException($"Có {label} null.");
                string id = getId(item);
                if (map.ContainsKey(id))
                    throw new ArgumentException($"Trùng id {label} '{id}'.");
                map.Add(id, item);
                list.Add(item);
            }
            return list.AsReadOnly();
        }
    }
}
