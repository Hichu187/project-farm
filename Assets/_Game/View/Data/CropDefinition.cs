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

        [Header("Art")]
        [SerializeField] private Sprite _seedSprite;
        [SerializeField] private Sprite[] _stageSprites = new Sprite[CropDef.StageCount];
        [SerializeField] private Sprite _produceIcon;

        public string Id => _id;
        public string DisplayName => _displayName;
        public bool Enabled => _enabled;
        public Sprite SeedSprite => _seedSprite;
        public Sprite ProduceIcon => _produceIcon;

        public Sprite GetStageSprite(int stage)
        {
            if (_stageSprites == null || stage < 0 || stage >= _stageSprites.Length)
                return null;
            return _stageSprites[stage];
        }

        public CropDef ToDef() => new CropDef(_id, _growMinutes * 60L, _buyPrice, _sellPrice, _harvestYield);

        private void OnValidate()
        {
            if (_stageSprites == null || _stageSprites.Length != CropDef.StageCount)
                System.Array.Resize(ref _stageSprites, CropDef.StageCount);
        }
    }
}
