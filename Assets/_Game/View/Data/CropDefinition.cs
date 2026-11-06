using UnityEngine;
using VeQue.Logic;

namespace VeQue.View.Data
{
    [CreateAssetMenu(menuName = "Về Quê/Crop Definition", fileName = "Crop")]
    public sealed class CropDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [Tooltip("Tắt: cây không có trong game (chưa có art).")]
        [SerializeField] private bool _enabled = true;
        [Min(1)] [SerializeField] private int _growMinutes = 2;
        [Min(0)] [SerializeField] private int _buyPrice;
        [Min(0)] [SerializeField] private int _sellPrice;
        [Min(1)] [SerializeField] private int _harvestYield = 1;
        [Tooltip("XP mỗi lần thu hoạch.")]
        [Min(0)] [SerializeField] private int _xp;
        [Tooltip("Cấp cần để gieo và mua hạt.")]
        [Min(1)] [SerializeField] private int _unlockLevel = 1;

        [Header("Art")]
        [SerializeField] private Sprite _seedSprite;
        [Tooltip("Số phần tử = số giai đoạn của cây.")]
        [SerializeField] private Sprite[] _stageSprites = new Sprite[CropDef.DefaultStageCount];
        [SerializeField] private Sprite _produceIcon;

        public string Id => _id;
        public string DisplayName => _displayName;
        public bool Enabled => _enabled;
        public int UnlockLevel => _unlockLevel;
        public Sprite SeedSprite => _seedSprite;
        public Sprite ProduceIcon => _produceIcon;

        public Sprite GetStageSprite(int stage)
        {
            if (_stageSprites == null || stage < 0 || stage >= _stageSprites.Length)
                return null;
            return _stageSprites[stage];
        }

        public CropDef ToDef()
        {
            int stageCount = _stageSprites == null ? 1 : Mathf.Max(1, _stageSprites.Length);
            return new CropDef(_id, _growMinutes * 60L, _buyPrice, _sellPrice, _harvestYield, stageCount, _xp, _unlockLevel);
        }
    }
}
