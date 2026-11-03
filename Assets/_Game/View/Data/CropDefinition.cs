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
        [Tooltip("Số phần tử = số giai đoạn của cây.")]
        [SerializeField] private Sprite[] _stageSprites = new Sprite[CropDef.DefaultStageCount];
        [Tooltip("Bản đất ướt, dùng khi cây đã tưới. Để trống thì dùng Stage Sprites.")]
        [SerializeField] private Sprite[] _wateredStageSprites = new Sprite[CropDef.DefaultStageCount];
        [SerializeField] private Sprite _produceIcon;

        public string Id => _id;
        public string DisplayName => _displayName;
        public bool Enabled => _enabled;
        public Sprite SeedSprite => _seedSprite;
        public Sprite ProduceIcon => _produceIcon;

        public Sprite GetStageSprite(int stage, bool watered = false)
        {
            if (watered && _wateredStageSprites != null && stage >= 0 && stage < _wateredStageSprites.Length
                && _wateredStageSprites[stage] != null)
                return _wateredStageSprites[stage];

            if (_stageSprites == null || stage < 0 || stage >= _stageSprites.Length)
                return null;
            return _stageSprites[stage];
        }

        public CropDef ToDef()
        {
            int stageCount = _stageSprites == null ? 1 : Mathf.Max(1, _stageSprites.Length);
            return new CropDef(_id, _growMinutes * 60L, _buyPrice, _sellPrice, _harvestYield, stageCount);
        }

        private void OnValidate()
        {
            int stageCount = _stageSprites == null ? 0 : _stageSprites.Length;
            if (_wateredStageSprites == null || _wateredStageSprites.Length != stageCount)
                System.Array.Resize(ref _wateredStageSprites, stageCount);
        }
    }
}
