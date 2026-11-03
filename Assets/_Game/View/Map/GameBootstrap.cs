using System.Collections.Generic;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.View.Map
{
    // Dựng Farm từ data lúc khởi động. Script khác lấy Farm từ Start trở đi.
    // TODO(Phần 7): đọc state đã lưu thay vì luôn tạo mới.
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private FarmDefinition _definition;

        private readonly Dictionary<string, CropDefinition> _crops = new Dictionary<string, CropDefinition>();

        public FarmDefinition Definition => _definition;
        public Farm Farm { get; private set; }

        public bool TryGetCrop(string id, out CropDefinition crop)
        {
            if (id == null)
            {
                crop = null;
                return false;
            }
            return _crops.TryGetValue(id, out crop);
        }

        private void Awake()
        {
            FarmDef def = _definition.ToDef();
            Farm = new Farm(def, FarmState.CreateNew(def), new SystemClock());

            foreach (CropDefinition crop in _definition.Crops)
            {
                if (crop != null && crop.Enabled)
                    _crops[crop.Id] = crop;
            }
        }
    }
}
