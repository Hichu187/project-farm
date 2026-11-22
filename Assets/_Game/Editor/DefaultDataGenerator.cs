using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VeQue.Logic;
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
        private const int OrderSlots = 5;
        private const int OrderUnlockLevel = 2;
        private const int OrderCoinPercent = 160;
        private const int OrderXpPercent = 200;
        private const int OrderDiscardMinutes = 5;
        private const int OrderHarvestCap = 6;
        private const int OrderRefillMinutes = 10;
        private const int OrderUrgentRefillMinutes = 30;
        private const int OrderUrgentChancePercent = 20;
        private const int OrderUrgentRewardPercent = 200;
        private const int OrderUrgentExpireMinutes = 30;
        private const int OrderDailyLimit = 15;
        // Đơn hàng có thoại (Đợt 2 Phần 11): người làng, câu thoại, món nằm trong 3 file data, sửa trực tiếp file.
        private const string OrdersFolder = DataFolder + "/Orders";
        private const string NpcsPath = OrdersFolder + "/npcs.json";
        private const string OrderLinesPath = OrdersFolder + "/order_lines.csv";
        private const string DishesPath = OrdersFolder + "/dishes.json";
        // Bảng âm lịch (Đợt 2 Phần 2), sinh bằng Tools/lunar_table.py.
        private const string LunarTablePath = DataFolder + "/Calendar/lunar_2026_2040.json";
        // Thư theo build (Đợt 2 Phần 3).
        private const string BuildMailsPath = DataFolder + "/Mail/build_mails.json";
        private const string CheckInPath = DataFolder + "/Mail/check_in.json";
        private const string EventsPath = DataFolder + "/Events/events.json";
        // Đơn cá: 10% ở cấp mở câu cá, +1%/cấp, tối đa 20% (chốt 20/11/2026).
        private const int FishOrderMinPercent = 10;
        private const int FishOrderMaxPercent = 20;

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
            // Cây sự kiện (Đợt 2 Phần 7–8): chỉ mua hạt khi sự kiện UnlockCrop đang chạy.
            public readonly bool EventOnly;
            public readonly CropGroup Group;
            public readonly int SeedPrice;
            public readonly int SellPrice;
            public readonly int Xp;

            public CropRow(string id, string name, CropGroup group, int minutes, int yield, int unlockLevel, bool enabled = true,
                bool eventOnly = false)
            {
                EventOnly = eventOnly;
                Id = id;
                Name = name;
                Minutes = minutes;
                Yield = yield;
                UnlockLevel = unlockLevel;
                Enabled = enabled;
                Group = group;

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

        private const CropGroup RauLa = CropGroup.LeafyGreen;
        private const CropGroup Qua = CropGroup.Fruit;
        private const CropGroup CuHat = CropGroup.RootGrain;

        // Bộ 25 cây thuần Việt, tên giọng Bắc (Đợt 2 Phần 4, Docs/thiet-ke-dot-2.md mục 1).
        // (id, tên, nhóm giá, phút lớn, sản lượng, cấp mở, bật). Giá và XP tính theo công thức ở CropRow.
        // Thứ tự theo thời gian lớn ngoài đời; cây sau không chín nhanh hơn cây trước.
        private static readonly CropRow[] Crops =
        {
            new CropRow("rau_muong", "Rau muống", RauLa, 2, 2, 1),
            new CropRow("hanh_la", "Hành lá", RauLa, 5, 2, 2),
            new CropRow("cai_xanh", "Cải xanh", RauLa, 15, 2, 2),
            new CropRow("rau_day", "Rau đay", RauLa, 20, 2, 4),
            new CropRow("mong_toi", "Mồng tơi", RauLa, 30, 2, 3),
            new CropRow("dua_chuot", "Dưa chuột", Qua, 30, 3, 5),
            new CropRow("muop", "Mướp", Qua, 45, 3, 6),
            new CropRow("dau_dua", "Đậu đũa", Qua, 60, 4, 3),
            new CropRow("bau", "Bầu", Qua, 90, 1, 7),
            new CropRow("muop_dang", "Mướp đắng", Qua, 90, 3, 9),
            new CropRow("su_hao", "Su hào", CuHat, 120, 1, 8),
            new CropRow("ngo_nep", "Ngô nếp", CuHat, 150, 2, 6),
            new CropRow("dua_hau", "Dưa hấu", Qua, 180, 1, 11),
            new CropRow("dau_xanh", "Đậu xanh", CuHat, 180, 4, 8),
            new CropRow("ca_phao", "Cà pháo", Qua, 210, 5, 10),
            new CropRow("ca_chua", "Cà chua", Qua, 240, 3, 5),
            new CropRow("lac", "Lạc", CuHat, 300, 5, 10),
            new CropRow("ot", "Ớt", Qua, 360, 5, 12),
            new CropRow("bi_ngo", "Bí ngô", CuHat, 360, 1, 7),
            new CropRow("lua", "Lúa", CuHat, 480, 2, 4),
            new CropRow("khoai_lang", "Khoai lang", CuHat, 600, 3, 9),
            new CropRow("khoai_so", "Khoai sọ", CuHat, 720, 3, 13),
            // Cây sự kiện (eventOnly). 22/11/2026 Phần 8: bật hoa cúc, hướng dương (Chợ đầu tháng – mùng 1 âm),
            // 4 giờ, sản lượng 2, cấp 1, giá / XP theo công thức (người dùng chốt). Hoa đào vẫn tắt, chờ Tết (12a).
            new CropRow("hoa_dao", "Hoa đào", Qua, 240, 1, 1, false, eventOnly: true),
            new CropRow("hoa_cuc", "Hoa cúc", Qua, 240, 2, 1, eventOnly: true),
            new CropRow("huong_duong", "Hướng dương", Qua, 240, 2, 1, eventOnly: true),
        };

        // Art cây: crop_{id}_{seed|stage1|stage2|stage3|icon|packet}.png.
        // Ruộng: seed → stage1 → stage2 → stage3 (stage3 chỉ hiện khi chín); hạt (UI): packet; nông sản: icon.
        private const string CropArtFolder = "Assets/_Game/Art/Crops";
        private static readonly string[] CropStageParts = { "seed", "stage1", "stage2", "stage3" };

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

        [MenuItem("Về Quê/Data/Tạo data mặc định", priority = 200)]
        private static void Generate()
        {
            if (AssetDatabase.LoadAssetAtPath<FarmDefinition>(FarmPath) != null
                && !EditorUtility.DisplayDialog("Tạo data mặc định",
                    "Data đã có. Ghi đè số liệu bằng bảng mặc định (cả câu cá)? Sprite cây lấy lại từ Art/Crops; icon câu cá và origin luống đã có được giữ nguyên.", "Ghi đè", "Hủy"))
                return;

            EnsureFolder(DataFolder);
            EnsureFolder(CropsFolder);
            EnsureFolder(ZonesFolder);

            var crops = new CropDefinition[Crops.Length];
            var cropPaths = new HashSet<string>();
            CropDefinition startSeedCrop = null;
            CropDefinition rescueCrop = null;
            for (int i = 0; i < Crops.Length; i++)
            {
                CropRow row = Crops[i];
                string cropPath = $"{CropsFolder}/{i + 1:00}_{row.Id}.asset";
                cropPaths.Add(cropPath);
                crops[i] = LoadOrCreate<CropDefinition>(cropPath);
                var so = new SerializedObject(crops[i]);
                so.FindProperty("_id").stringValue = row.Id;
                so.FindProperty("_displayName").stringValue = row.Name;
                so.FindProperty("_enabled").boolValue = row.Enabled;
                so.FindProperty("_eventOnly").boolValue = row.EventOnly;
                so.FindProperty("_growMinutes").intValue = row.Minutes;
                so.FindProperty("_buyPrice").intValue = row.SeedPrice;
                so.FindProperty("_sellPrice").intValue = row.SellPrice;
                so.FindProperty("_harvestYield").intValue = row.Yield;
                so.FindProperty("_xp").intValue = row.Xp;
                so.FindProperty("_unlockLevel").intValue = row.UnlockLevel;
                so.FindProperty("_group").enumValueIndex = (int)row.Group;
                so.ApplyModifiedPropertiesWithoutUndo();

                if (row.Id == StartSeedCropId)
                    startSeedCrop = crops[i];
                if (row.Id == RescueCropId)
                    rescueCrop = crops[i];
            }

            int removed = DeleteStaleCrops(cropPaths);
            int missingArt = AssignCropArt(crops);

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
            farmSo.FindProperty("_orderRefillMinutes").intValue = OrderRefillMinutes;
            farmSo.FindProperty("_orderUrgentRefillMinutes").intValue = OrderUrgentRefillMinutes;
            farmSo.FindProperty("_orderUrgentChancePercent").intValue = OrderUrgentChancePercent;
            farmSo.FindProperty("_orderUrgentRewardPercent").intValue = OrderUrgentRewardPercent;
            farmSo.FindProperty("_orderUrgentExpireMinutes").intValue = OrderUrgentExpireMinutes;
            farmSo.FindProperty("_orderDailyLimit").intValue = OrderDailyLimit;
            farmSo.FindProperty("_npcsJson").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(NpcsPath);
            farmSo.FindProperty("_orderLinesCsv").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(OrderLinesPath);
            farmSo.FindProperty("_dishesJson").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(DishesPath);
            farmSo.FindProperty("_fishOrderMinPercent").intValue = FishOrderMinPercent;
            farmSo.FindProperty("_fishOrderMaxPercent").intValue = FishOrderMaxPercent;
            farmSo.FindProperty("_lunarTable").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(LunarTablePath);
            farmSo.FindProperty("_buildMails").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(BuildMailsPath);
            farmSo.FindProperty("_checkIn").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(CheckInPath);
            farmSo.FindProperty("_events").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(EventsPath);
            // Giá chợ (Đợt 2 Phần 9, duyệt 22/11/2026).
            farmSo.FindProperty("_marketEnabled").boolValue = true;
            farmSo.FindProperty("_marketDailyMin").floatValue = 0.85f;
            farmSo.FindProperty("_marketDailyMax").floatValue = 1.15f;
            farmSo.FindProperty("_marketHotMult").floatValue = 1.4f;
            farmSo.FindProperty("_marketColdMult").floatValue = 0.7f;
            farmSo.FindProperty("_marketLeafyMorningPercent").intValue = 10;
            farmSo.FindProperty("_marketLeafyNightPercent").intValue = 10;
            farmSo.ApplyModifiedPropertiesWithoutUndo();
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(NpcsPath) == null)
                Debug.LogWarning($"Về Quê: thiếu {NpcsPath} – đơn hàng sẽ không có người đặt / thoại.");

            FishingDataGenerator.Generate(farm);

            AssetDatabase.SaveAssets();
            Selection.activeObject = farm;
            Debug.Log($"Về Quê: đã tạo {crops.Length} cây ({removed} asset cây cũ đã xóa, {missingArt} cây thiếu art), "
                + $"{zones.Length} khu → {FarmPath}", farm);
        }

        // Xóa asset cây không còn trong bảng (bộ cây cũ, hoặc cây đổi số thứ tự).
        private static int DeleteStaleCrops(HashSet<string> keep)
        {
            int removed = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:CropDefinition", new[] { CropsFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (keep.Contains(path))
                    continue;
                if (AssetDatabase.DeleteAsset(path))
                    removed++;
                else
                    Debug.LogWarning($"Về Quê: không xóa được {path}.");
            }
            return removed;
        }

        // Gán art từ CropArtFolder theo tên file. Cây đủ 6 file thì ghi đè sprite cũ; thiếu file thì giữ nguyên và báo.
        private static int AssignCropArt(CropDefinition[] crops)
        {
            int missing = 0;
            foreach (CropDefinition crop in crops)
            {
                var stages = new Sprite[CropStageParts.Length];
                var absent = new List<string>();
                for (int i = 0; i < CropStageParts.Length; i++)
                    stages[i] = LoadCropSprite(crop.Id, CropStageParts[i], absent);
                Sprite packet = LoadCropSprite(crop.Id, "packet", absent);
                Sprite icon = LoadCropSprite(crop.Id, "icon", absent);

                if (absent.Count > 0)
                {
                    missing++;
                    Debug.LogWarning($"Về Quê: cây '{crop.Id}' thiếu art ({string.Join(", ", absent)}) trong {CropArtFolder}, giữ sprite cũ.", crop);
                    continue;
                }

                var so = new SerializedObject(crop);
                so.FindProperty("_seedSprite").objectReferenceValue = packet;
                so.FindProperty("_produceIcon").objectReferenceValue = icon;
                SerializedProperty stageProp = so.FindProperty("_stageSprites");
                stageProp.arraySize = stages.Length;
                for (int i = 0; i < stages.Length; i++)
                    stageProp.GetArrayElementAtIndex(i).objectReferenceValue = stages[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            return missing;
        }

        private static Sprite LoadCropSprite(string cropId, string part, List<string> absent)
        {
            string file = $"crop_{cropId}_{part}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{CropArtFolder}/{file}");
            if (sprite == null)
                absent.Add(file);
            return sprite;
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
