using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    public enum CastQuality
    {
        Weak = 0,
        Normal = 1,
        Good = 2,
        Perfect = 3,
    }

    public enum FishingPhase
    {
        // Phao đã xuống nước, chờ cá cắn và người chơi giật.
        Waiting,
        // Đã móc được cá, đang nhập chuỗi mũi tên (1 lượt).
        Pulling,
        // Xong (có Outcome); Farm.CompleteFishing áp kết quả.
        Finished,
    }

    public enum FishingOutcome
    {
        None,
        Caught,
        // Giật lúc cá chưa cắn: cá sợ bỏ đi, mất mồi.
        Scared,
        // Giật trễ / không giật: cá ăn mất mồi.
        BaitStolen,
        // Giật đúng lúc nhưng trượt lưỡi.
        Slipped,
        // Kéo cá bấm sai mũi tên: đứt dây, cá thoát; có thể gãy cần.
        LineSnapped,
        // Kéo cá hết giờ: cá thoát (chỉ mất mồi).
        Escaped,
    }

    public enum Arrow
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    // Xếp hạng kéo cá theo tốc độ nhập (càng nhanh càng hoàn hảo).
    public enum PullGrade
    {
        Normal = 0,
        Good = 1,
        Perfect = 2,
    }

    // Đề kéo cá: chuỗi mũi tên (mũi tên đỏ = bấm hướng ngược lại) và thời gian nhập (đã cộng thưởng của cần).
    public sealed class PullRound
    {
        public IReadOnlyList<Arrow> Arrows { get; }
        public IReadOnlyList<bool> Red { get; }
        public float TimeLimitSeconds { get; }

        internal PullRound(Arrow[] arrows, bool[] red, float timeLimit)
        {
            Arrows = arrows;
            Red = red;
            TimeLimitSeconds = timeLimit;
        }

        public int Count => Arrows.Count;

        // Hướng phải bấm cho mũi tên thứ i (đỏ thì ngược lại).
        public Arrow Expected(int i)
        {
            Arrow arrow = Arrows[i];
            if (!Red[i])
                return arrow;
            switch (arrow)
            {
                case Arrow.Up: return Arrow.Down;
                case Arrow.Down: return Arrow.Up;
                case Arrow.Left: return Arrow.Right;
                default: return Arrow.Left;
            }
        }
    }

    // Một lần câu (MVP-Plan Phần 10 mục 1–3). Phần cần phản xạ thời gian thực (thanh lực, giật, nhập mũi tên)
    // do View chạy; View báo kết quả, phiên tính phần còn lại. Ngẫu nhiên riêng (seed từ Farm) nên lặp lại được.
    // Tạo bằng Farm.Cast (đã trừ mồi + độ bền); xong thì Farm.CompleteFishing áp kết quả vào tiến trình.
    // Kéo cá 1 lượt (chốt 13/11/2026): nhập đúng hết chuỗi trước khi hết giờ là câu được; nhanh thì hạng cao hơn.
    public sealed class FishingSession
    {
        private readonly FishingTuning _tuning;
        private readonly List<float> _nibbles = new List<float>();
        private uint _random;

        public FishDef Fish { get; }
        public RodDef Rod { get; }
        public BaitDef Bait { get; }
        public DaySlot Slot { get; }
        public float Power { get; }
        public CastQuality Quality { get; }
        public FishDepth Depth { get; }

        // Lịch phao (giây tính từ lúc phao chạm nước): các lần rỉa, lúc cắn, khung giật.
        public IReadOnlyList<float> NibbleTimes => _nibbles;
        public float BiteTime { get; }
        public float HookWindowSeconds { get; }
        public float HookDeadline => BiteTime + HookWindowSeconds;

        public FishingPhase Phase { get; private set; }
        public FishingOutcome Outcome { get; private set; }

        // Đề kéo cá (có sau khi giật trúng).
        public PullRound Pull { get; private set; }

        // Kết quả khi Caught.
        public PullGrade Grade { get; private set; }
        public int Grams { get; private set; }
        public int Price { get; private set; }
        public int Xp { get; private set; }

        internal bool Applied { get; set; }

        internal FishingSession(FishingDef def, RodDef rod, BaitDef bait, DaySlot slot, float power, uint seed)
        {
            _tuning = def.Tuning;
            _random = seed == 0 ? 1u : seed;
            Rod = rod;
            Bait = bait;
            Slot = slot;
            Power = Clamp01(power);
            Quality = Classify(Power, rod, _tuning);
            Depth = DepthOf(Power, Quality, _tuning);
            Fish = PickFish(def);
            Phase = FishingPhase.Waiting;

            float delay = Lerp(_tuning.BiteMinSeconds, _tuning.BiteMaxSeconds, NextFloat());
            if (Fish.Prefers(bait.Id))
                delay *= _tuning.PreferredBiteFactor;
            if (Quality == CastQuality.Perfect)
                delay *= _tuning.PerfectBiteFactor;
            BiteTime = Math.Max(delay, _tuning.NibbleGapSeconds);
            HookWindowSeconds = Fish.HookWindowSeconds;

            // Rỉa nhử trước khi cắn, cách nhau và cách lúc cắn ít nhất NibbleGapSeconds.
            int nibbles = SeededRandom.Range(ref _random, 0, _tuning.MaxNibbles + 1);
            float latest = BiteTime - _tuning.NibbleGapSeconds;
            for (int i = 0; i < nibbles && latest >= _tuning.NibbleGapSeconds; i++)
            {
                float time = Lerp(_tuning.NibbleGapSeconds, latest, NextFloat());
                if (IsFarFromNibbles(time))
                    _nibbles.Add(time);
            }
            _nibbles.Sort();
        }

        public static CastQuality Classify(float power, RodDef rod, FishingTuning tuning)
        {
            float distance = Math.Abs(power - tuning.PerfectCenter);
            if (distance <= rod.PerfectHalfWidth)
                return CastQuality.Perfect;
            if (distance <= rod.PerfectHalfWidth * tuning.GoodWidthMultiplier)
                return CastQuality.Good;
            return power < tuning.WeakBelow ? CastQuality.Weak : CastQuality.Normal;
        }

        // Xếp hạng theo phần thời gian đã dùng.
        public static PullGrade GradeFor(float secondsUsed, float timeLimitSeconds, FishingTuning tuning)
        {
            float percent = timeLimitSeconds > 0f ? secondsUsed / timeLimitSeconds * 100f : 100f;
            if (percent <= tuning.PerfectTimePercent)
                return PullGrade.Perfect;
            return percent <= tuning.GoodTimePercent ? PullGrade.Good : PullGrade.Normal;
        }

        // Giật câu ở giây thứ t (tính từ lúc phao chạm nước). Trúng thì sang Pulling và có đề kéo cá (Pull).
        public FishingOutcome Strike(float t)
        {
            RequirePhase(FishingPhase.Waiting);
            if (t < BiteTime)
                return Finish(FishingOutcome.Scared);
            if (t > HookDeadline)
                return Finish(FishingOutcome.BaitStolen);

            int slip = Math.Max(0, Fish.SlipPercent - Rod.SlipReductionPercent);
            if (SeededRandom.Range(ref _random, 0, 100) < slip)
                return Finish(FishingOutcome.Slipped);

            Phase = FishingPhase.Pulling;
            Pull = MakePull();
            return FishingOutcome.None;
        }

        // Hết khung giật mà người chơi không giật.
        public FishingOutcome Timeout()
        {
            RequirePhase(FishingPhase.Waiting);
            return Finish(FishingOutcome.BaitStolen);
        }

        // Nhập đúng hết chuỗi sau secondsUsed giây. Quá thời gian thì cá thoát.
        public FishingOutcome CompletePull(float secondsUsed)
        {
            RequirePhase(FishingPhase.Pulling);
            if (secondsUsed > Pull.TimeLimitSeconds)
                return Finish(FishingOutcome.Escaped);

            Grade = GradeFor(Math.Max(0f, secondsUsed), Pull.TimeLimitSeconds, _tuning);
            RollCatch(Grade);
            return Finish(FishingOutcome.Caught);
        }

        // Bấm sai mũi tên: đứt dây.
        public FishingOutcome WrongArrow()
        {
            RequirePhase(FishingPhase.Pulling);
            return Finish(FishingOutcome.LineSnapped);
        }

        // Hết giờ nhập: cá thoát.
        public FishingOutcome PullTimeout()
        {
            RequirePhase(FishingPhase.Pulling);
            return Finish(FishingOutcome.Escaped);
        }

        // Người chơi bỏ cuộc (về nhà, đóng game…) khi chưa xong: coi như cá ăn mất mồi.
        public FishingOutcome Abandon()
        {
            if (Phase == FishingPhase.Finished)
                return Outcome;
            return Finish(FishingOutcome.BaitStolen);
        }

        // Chuỗi mũi tên: độ dài theo cá (ném Perfect bớt 1), thời gian theo cá + thưởng của cần.
        private PullRound MakePull()
        {
            int count = SeededRandom.Range(ref _random, Fish.MinArrows, Fish.MaxArrows + 1);
            if (Quality == CastQuality.Perfect)
                count = Math.Max(1, count - 1);

            var arrows = new Arrow[count];
            var red = new bool[count];
            for (int i = 0; i < count; i++)
            {
                arrows[i] = (Arrow)SeededRandom.Range(ref _random, 0, 4);
                red[i] = Fish.RedArrows && SeededRandom.Range(ref _random, 0, 100) < _tuning.RedArrowPercent;
            }
            float time = Fish.PullSeconds * (1f + Rod.BonusTimePercent / 100f);
            return new PullRound(arrows, red, time);
        }

        // Cân nặng theo hạng (Perfect lệch về nặng hơn); giá, XP theo cân nặng; Perfect thêm XP.
        private void RollCatch(PullGrade grade)
        {
            float exponent = grade == PullGrade.Perfect ? _tuning.PerfectWeightExponent
                : grade == PullGrade.Good ? _tuning.GoodWeightExponent
                : _tuning.NormalWeightExponent;
            float u = (float)Math.Pow(NextFloat(), exponent);
            int span = Fish.MaxGrams - Fish.MinGrams;
            Grams = Fish.MinGrams + (int)Math.Round(span * u);
            float ratio = span == 0 ? 0.5f : (Grams - Fish.MinGrams) / (float)span;
            float percent = Lerp(_tuning.WeightValueMinPercent, _tuning.WeightValueMaxPercent, ratio);
            Price = Math.Max(1, (int)Math.Round(Fish.BasePrice * percent / 100f));
            int xpPercent = grade == PullGrade.Perfect ? 100 + _tuning.PerfectXpBonusPercent : 100;
            Xp = Math.Max(1, (int)Math.Round(Fish.BaseXp * percent / 100f * xpPercent / 100f));
        }

        // Trọng số: độ hiếm × khung giờ × mồi ưa thích; chỉ cá có độ sâu khớp; Hiếm trở lên nhân thưởng
        // (ném Perfect / Tốt, cần, mồi). Ném hỏng chỉ ra cá Thường. Không có cá nào hợp thì lấy cá Thường bất kỳ.
        private FishDef PickFish(FishingDef def)
        {
            int rareBonus = Rod.RareBonusPercent + Bait.RareBonusPercent
                + (Quality == CastQuality.Perfect ? _tuning.PerfectRareBonusPercent
                    : Quality == CastQuality.Good ? _tuning.GoodRareBonusPercent : 0);

            var weights = new List<(FishDef Fish, float Weight)>();
            float total = 0f;
            foreach (FishDef fish in def.Fish)
            {
                if ((fish.Depths & Depth) == 0)
                    continue;
                if (Quality == CastQuality.Weak && fish.Rarity != FishRarity.Common)
                    continue;
                float weight = _tuning.RarityWeights[(int)fish.Rarity] * fish.GetSlotFactor(Slot);
                if (fish.Prefers(Bait.Id))
                    weight *= _tuning.PreferredBaitMultiplier;
                if (fish.Rarity >= FishRarity.Rare)
                    weight *= 1f + rareBonus / 100f;
                if (weight <= 0f)
                    continue;
                weights.Add((fish, weight));
                total += weight;
            }

            if (weights.Count == 0)
            {
                foreach (FishDef fish in def.Fish)
                {
                    if (fish.Rarity == FishRarity.Common)
                        return fish;
                }
                return def.Fish[0];
            }

            float pick = NextFloat() * total;
            foreach ((FishDef fish, float weight) in weights)
            {
                if (pick < weight)
                    return fish;
                pick -= weight;
            }
            return weights[weights.Count - 1].Fish;
        }

        private static FishDepth DepthOf(float power, CastQuality quality, FishingTuning tuning)
        {
            if (quality == CastQuality.Weak || power < tuning.NearBelow)
                return FishDepth.Near;
            return power < tuning.MidBelow ? FishDepth.Mid : FishDepth.Far;
        }

        private bool IsFarFromNibbles(float time)
        {
            foreach (float other in _nibbles)
            {
                if (Math.Abs(other - time) < _tuning.NibbleGapSeconds)
                    return false;
            }
            return true;
        }

        private FishingOutcome Finish(FishingOutcome outcome)
        {
            Outcome = outcome;
            Phase = FishingPhase.Finished;
            return outcome;
        }

        private void RequirePhase(FishingPhase phase)
        {
            if (Phase != phase)
                throw new InvalidOperationException($"Phiên câu đang ở {Phase}, cần {phase}.");
        }

        // [0, 1).
        private float NextFloat() => (SeededRandom.Next(ref _random) >> 8) / 16777216f;

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
