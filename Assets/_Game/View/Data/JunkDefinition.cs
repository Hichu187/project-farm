using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    // Một món rác vớt được khi câu (Đợt 2 Phần 5). Bán cho cô đồng nát; album tab "Đồ vớt được".
    [CreateAssetMenu(menuName = "Về Quê/Junk Definition", fileName = "Junk")]
    public sealed class JunkDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [Tooltip("Giá cô đồng nát trả (xu).")]
        [Min(0)] [SerializeField] private int _sellPrice = 2;
        [Tooltip("Câu đùa trên bảng kết quả.")]
        [TextArea] [SerializeField] private string _flavorText;
        [Tooltip("Trọng số khi phiên câu ra rác (món hiếm đặt nhỏ).")]
        [Min(1)] [SerializeField] private int _weight = 10;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public string FlavorText => _flavorText;

        public JunkDef ToDef() => new JunkDef(_id, _sellPrice, _weight);
    }
}
