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

        // Cấp độ (MVP-Plan Phần 6b). XP lên cấp kế tiếp = 3 × cấp^3,2, cấp 1 → 15.
        private static readonly int[] XpToNext =
        {
            3, 28, 101, 253, 517, 927, 1519, 2328, 3394, 4755, 6450, 8521, 11009, 13955,
        };
        private const int CoinsPerLevel = 20;
        private const int UnlockSeedCount = 3;
        private const string RescueCropId = "rau_muong";

        // Đơn hàng (MVP-Plan Phần 6b).
        private const int OrderSlots = 3;
        private const int OrderUnlockLevel = 2;
        private const int OrderCoinPercent = 160;
        private const int OrderXpPercent = 200;
        private const int OrderDiscardMinutes = 5;
        private const int OrderHarvestCap = 6;
        private static readonly string[] CustomerNames =
        {
            "Bà Tư", "Chú Ba", "Cô Năm", "Bác Hai", "Dì Sáu", "Ông Bảy", "Chị Út", "Anh Tám",
        };

        // Công thức cân bằng cây (MVP-Plan Phần 6b), T = phút lớn:
        //   lãi mỗi lần thu = 5 × (T/2)^0,8; thu/giá hạt = 2 + 1,5 × ln(T/2) / ln(120) (2,0 ở 2 phút → 3,5 ở 4 giờ);
        //   giá hạt = lãi / (thu/giá hạt − 1); giá bán = (lãi + giá hạt) / sản lượng; XP = (T/2)^0,65.
        private readonly struct CropRow
        {
            public readonly string Id;
            public readonly string Name;
            public readonly int Minutes;
            public readonly int Yield;
            public readonly int UnlockLevel;
            public readonly bool Enabled;
            public readonly int SeedPrice;
            public readonly int SellPrice;
            public readonly int Xp;

            public CropRow(string id, string name, int minutes, int yield, int unlockLevel, bool enabled)
            {
                Id = id;
                Name = name;
                Minutes = minutes;
                Yield = yield;
                UnlockLevel = unlockLevel;
                Enabled = enabled;

                double t = minutes / 2.0;
                double profit = 5.0 * System.Math.Pow(t, 0.8);
                double returnPerSeed = 2.0 + 1.5 * System.Math.Log(t) / System.Math.Log(120.0);
                SeedPrice = System.Math.Max(1, RoundHalfUp(profit / (returnPerSeed - 1.0)));
                SellPrice = System.Math.Max(1, RoundHalfUp((profit + SeedPrice) / yield));
                Xp = System.Math.Max(1, RoundHalfUp(System.Math.Pow(t, 0.65)));
            }
        }

        private static int RoundHalfUp(double value) => (int)System.Math.Floor(value + 0.5);

        private readonly struct ZoneRow
        {
            public readonly string Id;
            public readonly Vector3Int[] BedOrigins;
            public readonly int StartOpen;
            public readonly int RequiredLevel;
            public readonly int[] Prices;

            public ZoneRow(string id, Vector3Int[] bedOrigins, int startOpen, int requiredLevel, int[] prices)
            {
                Id = id;
                BedOrigins = bedOrigins;
                StartOpen = startOpen;
                RequiredLevel = requiredLevel;
                Prices = prices;
            }
        }

        // (id, tên, phút lớn, sản lượng, cấp mở, bật). Giá và XP tính theo công thức ở CropRow.
        // MVP chỉ bật 4 cây: rau muống (cấp 1), cải xanh (2), cà chua (4), bí đỏ (6). Cấp mở của cây đang tắt là tạm.
        private static readonly CropRow[] Crops =
        {
            new CropRow("rau_muong", "Rau muống", 2, 2, 1, true),
            new CropRow("hanh_la", "Hành lá", 5, 2, 1, false),
            new CropRow("xa_lach", "Xà lách", 10, 1, 2, false),
            new CropRow("cai_xanh", "Cải xanh", 15, 2, 2, true),
            new CropRow("rau_den", "Rau dền", 20, 2, 3, false),
            new CropRow("mong_toi", "Mồng tơi", 30, 2, 3, false),
            new CropRow("cu_cai_trang", "Củ cải trắng", 45, 1, 4, false),
            new CropRow("ca_chua", "Cà chua", 60, 3, 4, true),
            new CropRow("dua_leo", "Dưa leo", 90, 3, 5, false),
            new CropRow("dau_dua", "Đậu đũa", 120, 4, 5, false),
            new CropRow("ca_rot", "Cà rốt", 150, 2, 6, false),
            new CropRow("ot", "Ớt", 180, 5, 6, false),
            new CropRow("bi_do", "Bí đỏ", 240, 1, 6, true),
            new CropRow("khoai_lang", "Khoai lang", 300, 3, 7, false),
            new CropRow("ca_tim", "Cà tím", 360, 3, 8, false),
            new CropRow("bap", "Bắp", 480, 2, 9, false),
            new CropRow("kho_qua", "Khổ qua", 600, 4, 10, false),
            new CropRow("dau_phong", "Đậu phộng", 720, 5, 11, false),
            new CropRow("dua_hau", "Dưa hấu", 960, 1, 12, false),
            new CropRow("khoai_mon", "Khoai môn", 1200, 2, 13, false),
            new CropRow("bi_dao", "Bí đao", 1440, 2, 14, false),
            new CropRow("gung", "Gừng", 1800, 3, 15, false),
            new CropRow("nghe", "Nghệ", 2160, 3, 16, false),
            new CropRow("dau_tay", "Dâu tây", 2520, 5, 17, false),
            new CropRow("dua_luoi", "Dưa lưới", 2880, 2, 18, false),
        };

        // Mỗi khu 2 luống 6×2 = 24 ô; origin là vị trí tham chiếu của Map D (luống trên trước).
        // Giá mở ô ≈ 40 × 1.102^(n−1), làm tròn. Khu 1 mở sẵn 6 ô (18 giá); Khu 2, 3 mở miễn phí 1 ô (23 giá).
        // Khu 3 = bảng gốc × 3,5 làm tròn 5 (MVP-Plan Phần 6b). Khu 2 cần cấp 5, Khu 3 cần cấp 9.
        private static readonly ZoneRow[] Zones =
        {
            new ZoneRow("khu1", new[] { new Vector3Int(13, 14, 0), new Vector3Int(13, 11, 0) }, 6, 1, new[]
            {
                40, 45, 50, 55, 60, 65, 70, 80, 85, 95, 105, 115, 130, 140, 155, 170, 190, 210,
            }),
            new ZoneRow("khu2", new[] { new Vector3Int(21, 14, 0), new Vector3Int(21, 11, 0) }, 1, 5, new[]
            {
                230, 255, 280, 310, 340, 375, 410, 455, 500, 550, 605, 670, 740, 815, 895, 985, 1100, 1200, 1300, 1450,
                1600, 1750, 1950,
            }),
            new ZoneRow("khu3", new[] { new Vector3Int(13, 6, 0), new Vector3Int(13, 3, 0) }, 1, 9, new[]
            {
                7525, 8225, 9100, 9975, 11025, 12250, 13475, 14875, 16275, 18025, 19775, 21875, 24150, 26600, 29225,
                32200, 35700, 39200, 43050, 47600, 52500, 57750, 63700,
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
            CropDefinition rescueCrop = null;
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
                so.FindProperty("_xp").intValue = row.Xp;
                so.FindProperty("_unlockLevel").intValue = row.UnlockLevel;
                so.ApplyModifiedPropertiesWithoutUndo();

                if (row.Id == StartSeedCropId)
                    startSeedCrop = crops[i];
                if (row.Id == RescueCropId)
                    rescueCrop = crops[i];
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
                so.FindProperty("_requiredLevel").intValue = row.RequiredLevel;
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
            SerializedProperty xpToNext = farmSo.FindProperty("_xpToNext");
            xpToNext.arraySize = XpToNext.Length;
            for (int i = 0; i < XpToNext.Length; i++)
                xpToNext.GetArrayElementAtIndex(i).intValue = XpToNext[i];
            farmSo.FindProperty("_coinsPerLevel").intValue = CoinsPerLevel;
            farmSo.FindProperty("_unlockSeedCount").intValue = UnlockSeedCount;
            farmSo.FindProperty("_rescueCrop").objectReferenceValue = rescueCrop;
            farmSo.FindProperty("_orderSlots").intValue = OrderSlots;
            farmSo.FindProperty("_orderUnlockLevel").intValue = OrderUnlockLevel;
            farmSo.FindProperty("_orderCoinPercent").intValue = OrderCoinPercent;
            farmSo.FindProperty("_orderXpPercent").intValue = OrderXpPercent;
            farmSo.FindProperty("_orderDiscardMinutes").intValue = OrderDiscardMinutes;
            farmSo.FindProperty("_orderHarvestCap").intValue = OrderHarvestCap;
            SerializedProperty names = farmSo.FindProperty("_customerNames");
            names.arraySize = CustomerNames.Length;
            for (int i = 0; i < CustomerNames.Length; i++)
                names.GetArrayElementAtIndex(i).stringValue = CustomerNames[i];
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
