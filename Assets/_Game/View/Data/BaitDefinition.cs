using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Mồi câu (MVP-Plan Phần 10 mục 6). Mỗi lần ném tốn 1.
    [CreateAssetMenu(menuName = "Về Quê/Bait Definition", fileName = "Bait")]
    public sealed class BaitDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [Min(0)] [SerializeField] private int _price;
        [Tooltip("Tắt: không bán ở Cửa hàng (chỉ đào được / được tặng).")]
        [SerializeField] private bool _buyable = true;
        [Tooltip("Thưởng tỉ lệ cá Hiếm trở lên (%).")]
        [Min(0)] [SerializeField] private int _rareBonusPercent;
        [Tooltip("Cấp tối thiểu để mua.")]
        [Min(1)] [SerializeField] private int _unlockLevel = 1;
        [Tooltip("Tỉ lệ dính rác (%) trước hệ số kiểu ném (Tốt ×0,6, Perfect ×0,3). 0 = không bao giờ dính rác.")]
        [Range(0, 100)] [SerializeField] private int _junkChancePercent;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;

        public BaitDef ToDef() => new BaitDef(_id, _price, _rareBonusPercent, _buyable, _unlockLevel, _junkChancePercent);
    }
}
