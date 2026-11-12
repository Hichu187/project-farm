using System;
using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Một loài cá (MVP-Plan Phần 10 mục 4, 4b). Cân nặng tính bằng gam.
    [CreateAssetMenu(menuName = "Về Quê/Fish Definition", fileName = "Fish")]
    public sealed class FishDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private FishRarity _rarity;
        [Min(1)] [SerializeField] private int _minGrams = 100;
        [Min(1)] [SerializeField] private int _maxGrams = 500;
        [Tooltip("Giá ở cân nặng trung bình; nhẹ nhất / nặng nhất nhân hệ số trong FishingTuning.")]
        [Min(0)] [SerializeField] private int _basePrice = 10;
        [Min(0)] [SerializeField] private int _baseXp = 1;

        [Header("Xuất hiện")]
        [Tooltip("7 khung: Bình minh, Sáng, Trưa, Chiều, Hoàng hôn, Tối, Khuya. 0 = không có, 1 = bình thường, > 1 = ăn mạnh.")]
        [SerializeField] private float[] _slotFactors = { 1, 1, 1, 1, 1, 1, 1 };
        [SerializeField] private FishDepth _depths = FishDepth.All;
        [SerializeField] private BaitDefinition[] _preferredBaits = Array.Empty<BaitDefinition>();

        [Header("Độ khó (kéo cá 1 lượt)")]
        [Min(1)] [SerializeField] private int _minArrows = 3;
        [Min(1)] [SerializeField] private int _maxArrows = 4;
        [Tooltip("Thời gian nhập chuỗi mũi tên (giây), trước khi cộng thưởng của cần.")]
        [Min(0.5f)] [SerializeField] private float _pullSeconds = 4f;
        [Tooltip("Khung giật câu sau khi phao chìm (giây).")]
        [Min(0.1f)] [SerializeField] private float _hookWindowSeconds = 0.9f;
        [Range(0, 100)] [SerializeField] private int _slipPercent = 5;
        [Tooltip("Có mũi tên khác màu: bấm hướng ngược lại.")]
        [SerializeField] private bool _redArrows;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public FishRarity Rarity => _rarity;

        public FishDef ToDef()
        {
            var baits = new string[_preferredBaits?.Length ?? 0];
            for (int i = 0; i < baits.Length; i++)
            {
                if (_preferredBaits[i] == null)
                    throw new InvalidOperationException($"Cá '{_id}': mồi ưa thích thứ {i} chưa gán.");
                baits[i] = _preferredBaits[i].Id;
            }
            return new FishDef(_id, _rarity, _minGrams, _maxGrams, _basePrice, _baseXp, _slotFactors, _depths, baits,
                _minArrows, _maxArrows, _pullSeconds, _hookWindowSeconds, _slipPercent, _redArrows);
        }

        private void OnValidate()
        {
            if (_slotFactors == null || _slotFactors.Length != DaySlots.Count)
                Array.Resize(ref _slotFactors, DaySlots.Count);
            if (_maxGrams < _minGrams)
                _maxGrams = _minGrams;
            if (_maxArrows < _minArrows)
                _maxArrows = _minArrows;
        }
    }
}
