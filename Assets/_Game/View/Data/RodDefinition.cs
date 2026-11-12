using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Cần câu (MVP-Plan Phần 10 mục 5). Rank cao: vùng Perfect rộng, thanh lực chậm, thêm thời gian kéo cá, ít trượt, bền hơn.
    [CreateAssetMenu(menuName = "Về Quê/Rod Definition", fileName = "Rod")]
    public sealed class RodDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [Min(1)] [SerializeField] private int _rank = 1;
        [Tooltip("Nửa độ rộng vùng Perfect trên thanh lực (0..0,5).")]
        [Range(0.01f, 0.49f)] [SerializeField] private float _perfectHalfWidth = 0.03f;
        [Tooltip("Thanh lực chạy 0 → 100% → 0 hết bao nhiêu giây.")]
        [Min(0.1f)] [SerializeField] private float _barPeriodSeconds = 1.2f;
        [Tooltip("Thêm thời gian nhập chuỗi mũi tên khi kéo cá (%).")]
        [Min(0)] [SerializeField] private int _bonusTimePercent;
        [Range(0, 100)] [SerializeField] private int _slipReductionPercent;
        [Tooltip("Thưởng tỉ lệ cá Hiếm trở lên (%).")]
        [Min(0)] [SerializeField] private int _rareBonusPercent;
        [Min(1)] [SerializeField] private int _maxDurability = 30;
        [Min(0)] [SerializeField] private int _price;
        [Min(1)] [SerializeField] private int _unlockLevel = 1;
        [Min(0)] [SerializeField] private int _repairPrice;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;

        public RodDef ToDef() => new RodDef(_id, _rank, _perfectHalfWidth, _barPeriodSeconds, _bonusTimePercent,
            _slipReductionPercent, _rareBonusPercent, _maxDurability, _price, _unlockLevel, _repairPrice);
    }
}
