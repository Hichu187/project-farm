using UnityEditor;
using UnityEngine;
using VeQue.View.Data;

namespace VeQue.EditorTools
{
    // Tạo/cập nhật data mặc định từ bảng số đã chốt. Chạy lại được: chỉ ghi số liệu, sprite và origin luống đã có được giữ.
    public static class DefaultDataGenerator
    {
        public const string DataFolder = "Assets/_Game/Data";
        public const string FarmPath = DataFolder + "/FarmDefinition.asset";
        private const string CropsFolder = DataFolder + "/Crops";
        private const string ZonesFolder = DataFolder + "/Zones";

        private const long StartCoins = 60;
        private const string StartSeedCropId = "rau_muong";
        private const int StartSeedCount = 5;
        private const int BedWidth = 6;
        private const int BedHeight = 2;

        private readonly struct CropRow
        {
            public readonly string Id;
            public readonly string Name;
            public readonly int Minutes;
            public readonly int SeedPrice;
            public readonly int Yield;
            public readonly int SellPrice;
            public readonly bool Enabled;

            public CropRow(string id, string name, int minutes, int seedPrice, int yield, int sellPrice, bool enabled)
            {
                Id = id;
                Name = name;
                Minutes = minutes;
                SeedPrice = seedPrice;
                Yield = yield;
                SellPrice = sellPrice;
                Enabled = enabled;
            }
        }

        private readonly struct ZoneRow
        {
            public readonly string Id;
            public readonly Vector3Int[] BedOrigins;
            public readonly int StartOpen;
            public readonly int[] Prices;

            public ZoneRow(string id, Vector3Int[] bedOrigins, int startOpen, int[] prices)
            {
                Id = id;
                BedOrigins = bedOrigins;
                StartOpen = startOpen;
                Prices = prices;
            }
        }

        // MVP chỉ bật 4 cây: rau muống, cải xanh, cà chua, bí đỏ.
        private static readonly CropRow[] Crops =
        {
            new CropRow("rau_muong", "Rau muống", 2, 5, 2, 5, true),
            new CropRow("hanh_la", "Hành lá", 5, 8, 2, 9, false),
            new CropRow("xa_lach", "Xà lách", 10, 12, 1, 25, false),
            new CropRow("cai_xanh", "Cải xanh", 15, 15, 2, 16, true),
            new CropRow("rau_den", "Rau dền", 20, 20, 2, 20, false),
            new CropRow("mong_toi", "Mồng tơi", 30, 25, 2, 25, false),
            new CropRow("cu_cai_trang", "Củ cải trắng", 45, 30, 1, 62, false),
            new CropRow("ca_chua", "Cà chua", 60, 35, 3, 25, true),
            new CropRow("dua_leo", "Dưa leo", 90, 45, 3, 32, false),
            new CropRow("dau_dua", "Đậu đũa", 120, 55, 4, 28, false),
            new CropRow("ca_rot", "Cà rốt", 150, 65, 2, 66, false),
            new CropRow("ot", "Ớt", 180, 70, 5, 29, false),
            new CropRow("bi_do", "Bí đỏ", 240, 90, 1, 180, true),
            new CropRow("khoai_lang", "Khoai lang", 300, 100, 3, 67, false),
            new CropRow("ca_tim", "Cà tím", 360, 110, 3, 75, false),
            new CropRow("bap", "Bắp", 480, 130, 2, 132, false),
            new CropRow("kho_qua", "Khổ qua", 600, 150, 4, 76, false),
            new CropRow("dau_phong", "Đậu phộng", 720, 170, 5, 68, false),
            new CropRow("dua_hau", "Dưa hấu", 960, 200, 1, 405, false),
            new CropRow("khoai_mon", "Khoai môn", 1200, 230, 2, 232, false),
            new CropRow("bi_dao", "Bí đao", 1440, 260, 2, 260, false),
            new CropRow("gung", "Gừng", 1800, 300, 3, 199, false),
            new CropRow("nghe", "Nghệ", 2160, 330, 3, 220, false),
            new CropRow("dau_tay", "Dâu tây", 2520, 360, 5, 145, false),
            new CropRow("dua_luoi", "Dưa lưới", 2880, 390, 2, 392, false),
        };

        // Mỗi khu 2 luống 6×2 = 24 ô; origin là vị trí tham chiếu của Map D (luống trên trước).
        // Giá mở ô ≈ 40 × 1.102^(n−1), làm tròn. Khu 1 mở sẵn 6 ô (18 giá); Khu 2, 3 mở miễn phí 1 ô (23 giá).
        private static readonly ZoneRow[] Zones =
        {
            new ZoneRow("khu1", new[] { new Vector3Int(13, 14, 0), new Vector3Int(13, 11, 0) }, 6, new[]
            {
                40, 45, 50, 55, 60, 65, 70, 80, 85, 95, 105, 115, 130, 140, 155, 170, 190, 210,
            }),
            new ZoneRow("khu2", new[] { new Vector3Int(21, 14, 0), new Vector3Int(21, 11, 0) }, 1, new[]
            {
                230, 255, 280, 310, 340, 375, 410, 455, 500, 550, 605, 670, 740, 815, 895, 985, 1100, 1200, 1300, 1450,
                1600, 1750, 1950,
            }),
            new ZoneRow("khu3", new[] { new Vector3Int(13, 6, 0), new Vector3Int(13, 3, 0) }, 1, new[]
            {
                2150, 2350, 2600, 2850, 3150, 3500, 3850, 4250, 4650, 5150, 5650, 6250, 6900, 7600, 8350, 9200, 10200,
                11200, 12300, 13600, 15000, 16500, 18200,
            }),
        };

        [MenuItem("Về Quê/Tạo data mặc định")]
        private static void Generate()
        {
            if (AssetDatabase.LoadAssetAtPath<FarmDefinition>(FarmPath) != null
                && !EditorUtility.DisplayDialog("Tạo data mặc định",
                    "Data đã có. Ghi đè số liệu bằng bảng mặc định? Sprite đã gán và origin luống đã có được giữ nguyên.", "Ghi đè", "Hủy"))
                return;

            EnsureFolder(DataFolder);
            EnsureFolder(CropsFolder);
            EnsureFolder(ZonesFolder);

            var crops = new CropDefinition[Crops.Length];
            CropDefinition startSeedCrop = null;
            for (int i = 0; i < Crops.Length; i++)
            {
                CropRow row = Crops[i];
                crops[i] = LoadOrCreate<CropDefinition>($"{CropsFolder}/{i + 1:00}_{row.Id}.asset");
                var so = new SerializedObject(crops[i]);
                so.FindProperty("_id").stringValue = row.Id;
                so.FindProperty("_displayName").stringValue = row.Name;
                so.FindProperty("_enabled").boolValue = row.Enabled;
                so.FindProperty("_growMinutes").intValue = row.Minutes;
                so.FindProperty("_buyPrice").intValue = row.SeedPrice;
                so.FindProperty("_sellPrice").intValue = row.SellPrice;
                so.FindProperty("_harvestYield").intValue = row.Yield;
                so.ApplyModifiedPropertiesWithoutUndo();

                if (row.Id == StartSeedCropId)
                    startSeedCrop = crops[i];
            }

            var zones = new ZoneDefinition[Zones.Length];
            for (int i = 0; i < Zones.Length; i++)
            {
                ZoneRow row = Zones[i];
                zones[i] = LoadOrCreate<ZoneDefinition>($"{ZonesFolder}/{row.Id}.asset");
                var so = new SerializedObject(zones[i]);
                so.FindProperty("_id").stringValue = row.Id;
                SerializedProperty beds = so.FindProperty("_beds");
                int existingBeds = beds.arraySize;
                beds.arraySize = row.BedOrigins.Length;
                for (int b = 0; b < row.BedOrigins.Length; b++)
                {
                    SerializedProperty bed = beds.GetArrayElementAtIndex(b);
                    if (b >= existingBeds)
                        bed.FindPropertyRelative("Origin").vector3IntValue = row.BedOrigins[b];
                    bed.FindPropertyRelative("Width").intValue = BedWidth;
                    bed.FindPropertyRelative("Height").intValue = BedHeight;
                }
                so.FindProperty("_startOpenCount").intValue = row.StartOpen;
                SerializedProperty prices = so.FindProperty("_unlockPrices");
                prices.arraySize = row.Prices.Length;
                for (int p = 0; p < row.Prices.Length; p++)
                    prices.GetArrayElementAtIndex(p).intValue = row.Prices[p];
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            FarmDefinition farm = LoadOrCreate<FarmDefinition>(FarmPath);
            var farmSo = new SerializedObject(farm);
            SetObjectArray(farmSo.FindProperty("_zones"), zones);
            SetObjectArray(farmSo.FindProperty("_crops"), crops);
            farmSo.FindProperty("_startCoins").longValue = StartCoins;
            SerializedProperty seeds = farmSo.FindProperty("_startSeeds");
            seeds.arraySize = 1;
            seeds.GetArrayElementAtIndex(0).FindPropertyRelative("Crop").objectReferenceValue = startSeedCrop;
            seeds.GetArrayElementAtIndex(0).FindPropertyRelative("Count").intValue = StartSeedCount;
            farmSo.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            Selection.activeObject = farm;
            Debug.Log($"Về Quê: đã tạo {crops.Length} cây, {zones.Length} khu → {FarmPath}", farm);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetObjectArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
