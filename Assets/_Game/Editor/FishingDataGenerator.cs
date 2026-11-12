using System.Reflection;
using UnityEditor;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.EditorTools
{
    // Data câu cá mặc định (MVP-Plan Phần 10 giai đoạn 4, duyệt 12/11/2026): 13 cá, 5 cần, 5 mồi.
    // Chạy lại được: chỉ ghi số liệu; icon đã gán được giữ. Con số chung (FishingTuning) về mặc định.
    public static class FishingDataGenerator
    {
        public const string FishingPath = DefaultDataGenerator.DataFolder + "/FishingDefinition.asset";
        private const string FishingFolder = DefaultDataGenerator.DataFolder + "/Fishing";
        private const string FishFolder = FishingFolder + "/Fish";
        private const string RodsFolder = FishingFolder + "/Rods";
        private const string BaitsFolder = FishingFolder + "/Baits";

        private const int UnlockLevel = 1;
        private const string StarterRodId = "can_tre";
        private const string StarterBaitId = "giun";
        private const int StarterBaitCount = 10;
        private static readonly int[] BasketCapacities = { 8, 10, 12, 16, 20 };
        private static readonly long[] BasketUpgradePrices = { 0, 300, 1000, 3000, 8000 };
        private const string WormBaitId = "giun";
        private const int WormChancePercent = 10;
        private const int WormMin = 1;
        private const int WormMax = 2;

        // Độ khó + XP theo độ hiếm (Thường → Huyền thoại).
        private static readonly int[] MinArrows = { 3, 4, 5, 6, 8 };
        private static readonly int[] MaxArrows = { 4, 5, 7, 8, 10 };
        private static readonly float[] PullSeconds = { 4f, 4f, 3.5f, 3.2f, 3f };
        private static readonly float[] HookWindows = { 0.9f, 0.8f, 0.6f, 0.5f, 0.4f };
        private static readonly int[] SlipPercents = { 5, 8, 12, 18, 25 };
        private static readonly int[] Xps = { 1, 3, 8, 20, 50 };

        // Hệ số 7 khung: Bình minh, Sáng, Trưa, Chiều, Hoàng hôn, Tối, Khuya.
        private static readonly float[] AllDay = { 1, 1, 1, 1, 1, 1, 1 };

        private readonly struct FishRow
        {
            public readonly string Id;
            public readonly string Name;
            public readonly FishRarity Rarity;
            public readonly int MinGrams;
            public readonly int MaxGrams;
            public readonly int Price;
            public readonly float[] Slots;
            public readonly FishDepth Depths;
            public readonly string[] Baits;

            public FishRow(string id, string name, FishRarity rarity, int minGrams, int maxGrams, int price,
                float[] slots, FishDepth depths, params string[] baits)
            {
                Id = id;
                Name = name;
                Rarity = rarity;
                MinGrams = minGrams;
                MaxGrams = maxGrams;
                Price = price;
                Slots = slots;
                Depths = depths;
                Baits = baits;
            }
        }

        private static readonly FishRow[] Fish =
        {
            new FishRow("ca_ro", "Cá rô", FishRarity.Common, 80, 400, 10, AllDay, FishDepth.All, "giun"),
            new FishRow("ca_diec", "Cá diếc", FishRarity.Common, 100, 600, 12, AllDay, FishDepth.Near | FishDepth.Mid, "giun", "cam"),
            new FishRow("tep", "Tép", FishRarity.Common, 5, 30, 8, new float[] { 1, 1, 1, 1, 1, 2, 1 }, FishDepth.Near, "cam"),
            new FishRow("ca_chep", "Cá chép", FishRarity.Uncommon, 500, 4000, 40, new float[] { 1, 2, 0, 1, 0, 0, 0 }, FishDepth.Mid | FishDepth.Far, "cam"),
            new FishRow("ca_tre", "Cá trê", FishRarity.Uncommon, 300, 2500, 45, new float[] { 0, 0, 0, 0, 0, 1, 1 }, FishDepth.Near | FishDepth.Mid, "giun"),
            new FishRow("ca_me", "Cá mè", FishRarity.Uncommon, 500, 5000, 35, new float[] { 0, 1, 1, 1, 0, 0, 0 }, FishDepth.Mid | FishDepth.Far, "cam"),
            new FishRow("ca_loc", "Cá lóc", FishRarity.Rare, 400, 3000, 120, new float[] { 2, 1, 0, 0, 2, 0, 0 }, FishDepth.Mid | FishDepth.Far, "moi_tep", "moi_gia"),
            new FishRow("luon", "Lươn", FishRarity.Rare, 200, 1200, 110, new float[] { 0, 0, 0, 0, 0, 1, 1 }, FishDepth.Near, "giun"),
            new FishRow("tom_cang", "Tôm càng xanh", FishRarity.Rare, 50, 400, 130, new float[] { 0, 0, 0, 0, 1, 2, 0 }, FishDepth.Near | FishDepth.Mid, "moi_tep"),
            new FishRow("ca_bong_tuong", "Cá bống tượng", FishRarity.Epic, 300, 1500, 320, new float[] { 0, 0, 0, 1, 1, 0, 0 }, FishDepth.Mid | FishDepth.Far, "moi_tep"),
            new FishRow("ca_chep_do", "Cá chép đỏ", FishRarity.Epic, 500, 3000, 380, new float[] { 0, 1, 0, 0, 0, 0, 0 }, FishDepth.Far, "moi_thom"),
            new FishRow("ca_chep_vang", "Cá chép vàng", FishRarity.Legendary, 1000, 6000, 900, new float[] { 1, 0, 0, 0, 0, 0, 0 }, FishDepth.Far, "moi_thom"),
            new FishRow("ca_tram_den", "Cá trắm đen khổng lồ", FishRarity.Legendary, 5000, 30000, 1000, new float[] { 0, 0, 0, 0, 0, 0, 1 }, FishDepth.Far, "moi_thom", "moi_gia"),
        };

        private readonly struct RodRow
        {
            public readonly string Id;
            public readonly string Name;
            public readonly int Rank;
            public readonly float PerfectHalfWidth;
            public readonly float BarPeriod;
            public readonly int BonusTime;
            public readonly int SlipReduction;
            public readonly int RareBonus;
            public readonly int Durability;
            public readonly int Price;
            public readonly int UnlockLevel;
            public readonly int RepairPrice;

            public RodRow(string id, string name, int rank, float perfectHalfWidth, float barPeriod, int bonusTime,
                int slipReduction, int rareBonus, int durability, int price, int unlockLevel, int repairPrice)
            {
                Id = id;
                Name = name;
                Rank = rank;
                PerfectHalfWidth = perfectHalfWidth;
                BarPeriod = barPeriod;
                BonusTime = bonusTime;
                SlipReduction = slipReduction;
                RareBonus = rareBonus;
                Durability = durability;
                Price = price;
                UnlockLevel = unlockLevel;
                RepairPrice = repairPrice;
            }
        }

        private static readonly RodRow[] Rods =
        {
            new RodRow("can_tre", "Cần tre", 1, 0.03f, 1.2f, 0, 0, 0, 30, 0, 1, 20),
            new RodRow("can_truc", "Cần trúc", 2, 0.04f, 1.3f, 10, 3, 10, 50, 300, 3, 120),
            new RodRow("can_thuy_tinh", "Cần sợi thủy tinh", 3, 0.05f, 1.4f, 20, 6, 20, 80, 1500, 5, 600),
            new RodRow("can_carbon", "Cần carbon", 4, 0.06f, 1.6f, 30, 10, 30, 120, 6000, 9, 2400),
            new RodRow("can_vang", "Cần vàng", 5, 0.08f, 1.8f, 50, 15, 50, 200, 20000, 12, 8000),
        };

        private readonly struct BaitRow
        {
            public readonly string Id;
            public readonly string Name;
            public readonly int Price;
            public readonly bool Buyable;
            public readonly int RareBonus;

            public BaitRow(string id, string name, int price, bool buyable, int rareBonus)
            {
                Id = id;
                Name = name;
                Price = price;
                Buyable = buyable;
                RareBonus = rareBonus;
            }
        }

        private static readonly BaitRow[] Baits =
        {
            new BaitRow("giun", "Giun", 2, false, 0),
            new BaitRow("cam", "Cám", 3, true, 0),
            new BaitRow("moi_tep", "Mồi tép", 5, true, 0),
            new BaitRow("moi_gia", "Mồi giả", 8, true, 20),
            new BaitRow("moi_thom", "Mồi thơm", 25, true, 100),
        };

        [MenuItem("Về Quê/Tạo data câu cá")]
        private static void GenerateMenu()
        {
            FarmDefinition farm = AssetDatabase.LoadAssetAtPath<FarmDefinition>(DefaultDataGenerator.FarmPath);
            if (farm == null)
            {
                EditorUtility.DisplayDialog("Tạo data câu cá", "Chưa có FarmDefinition. Chạy Về Quê → Tạo data mặc định trước.", "OK");
                return;
            }
            FishingDefinition fishing = Generate(farm);
            AssetDatabase.SaveAssets();
            Selection.activeObject = fishing;
        }

        // Tạo / cập nhật data câu cá và gán vào FarmDefinition. Không gọi SaveAssets.
        public static FishingDefinition Generate(FarmDefinition farm)
        {
            EnsureFolder(FishingFolder);
            EnsureFolder(FishFolder);
            EnsureFolder(RodsFolder);
            EnsureFolder(BaitsFolder);

            var baits = new BaitDefinition[Baits.Length];
            for (int i = 0; i < Baits.Length; i++)
            {
                BaitRow row = Baits[i];
                baits[i] = LoadOrCreate<BaitDefinition>($"{BaitsFolder}/{i + 1:00}_{row.Id}.asset");
                var so = new SerializedObject(baits[i]);
                so.FindProperty("_id").stringValue = row.Id;
                so.FindProperty("_displayName").stringValue = row.Name;
                so.FindProperty("_price").intValue = row.Price;
                so.FindProperty("_buyable").boolValue = row.Buyable;
                so.FindProperty("_rareBonusPercent").intValue = row.RareBonus;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var rods = new RodDefinition[Rods.Length];
            for (int i = 0; i < Rods.Length; i++)
            {
                RodRow row = Rods[i];
                rods[i] = LoadOrCreate<RodDefinition>($"{RodsFolder}/{i + 1:00}_{row.Id}.asset");
                var so = new SerializedObject(rods[i]);
                so.FindProperty("_id").stringValue = row.Id;
                so.FindProperty("_displayName").stringValue = row.Name;
                so.FindProperty("_rank").intValue = row.Rank;
                so.FindProperty("_perfectHalfWidth").floatValue = row.PerfectHalfWidth;
                so.FindProperty("_barPeriodSeconds").floatValue = row.BarPeriod;
                so.FindProperty("_bonusTimePercent").intValue = row.BonusTime;
                so.FindProperty("_slipReductionPercent").intValue = row.SlipReduction;
                so.FindProperty("_rareBonusPercent").intValue = row.RareBonus;
                so.FindProperty("_maxDurability").intValue = row.Durability;
                so.FindProperty("_price").intValue = row.Price;
                so.FindProperty("_unlockLevel").intValue = row.UnlockLevel;
                so.FindProperty("_repairPrice").intValue = row.RepairPrice;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var fish = new FishDefinition[Fish.Length];
            for (int i = 0; i < Fish.Length; i++)
            {
                FishRow row = Fish[i];
                int r = (int)row.Rarity;
                fish[i] = LoadOrCreate<FishDefinition>($"{FishFolder}/{i + 1:00}_{row.Id}.asset");
                var so = new SerializedObject(fish[i]);
                so.FindProperty("_id").stringValue = row.Id;
                so.FindProperty("_displayName").stringValue = row.Name;
                so.FindProperty("_rarity").enumValueIndex = r;
                so.FindProperty("_minGrams").intValue = row.MinGrams;
                so.FindProperty("_maxGrams").intValue = row.MaxGrams;
                so.FindProperty("_basePrice").intValue = row.Price;
                so.FindProperty("_baseXp").intValue = Xps[r];
                SerializedProperty slots = so.FindProperty("_slotFactors");
                slots.arraySize = row.Slots.Length;
                for (int s = 0; s < row.Slots.Length; s++)
                    slots.GetArrayElementAtIndex(s).floatValue = row.Slots[s];
                so.FindProperty("_depths").intValue = (int)row.Depths;
                SerializedProperty preferred = so.FindProperty("_preferredBaits");
                preferred.arraySize = row.Baits.Length;
                for (int b = 0; b < row.Baits.Length; b++)
                    preferred.GetArrayElementAtIndex(b).objectReferenceValue = FindBait(baits, row.Baits[b]);
                so.FindProperty("_minArrows").intValue = MinArrows[r];
                so.FindProperty("_maxArrows").intValue = MaxArrows[r];
                so.FindProperty("_pullSeconds").floatValue = PullSeconds[r];
                so.FindProperty("_hookWindowSeconds").floatValue = HookWindows[r];
                so.FindProperty("_slipPercent").intValue = SlipPercents[r];
                so.FindProperty("_redArrows").boolValue = row.Rarity == FishRarity.Legendary;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            FishingDefinition fishing = LoadOrCreate<FishingDefinition>(FishingPath);
            // Con số chung về mặc định của FishingTuning (một chỗ duy nhất giữ các số này).
            typeof(FishingDefinition).GetField("_tuning", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(fishing, new FishingTuning());
            EditorUtility.SetDirty(fishing);

            var fishingSo = new SerializedObject(fishing);
            SetObjectArray(fishingSo.FindProperty("_fish"), fish);
            SetObjectArray(fishingSo.FindProperty("_rods"), rods);
            SetObjectArray(fishingSo.FindProperty("_baits"), baits);
            fishingSo.FindProperty("_unlockLevel").intValue = UnlockLevel;
            fishingSo.FindProperty("_starterRod").objectReferenceValue = FindRod(rods, StarterRodId);
            fishingSo.FindProperty("_starterBait").objectReferenceValue = FindBait(baits, StarterBaitId);
            fishingSo.FindProperty("_starterBaitCount").intValue = StarterBaitCount;
            SerializedProperty capacities = fishingSo.FindProperty("_basketCapacities");
            capacities.arraySize = BasketCapacities.Length;
            for (int i = 0; i < BasketCapacities.Length; i++)
                capacities.GetArrayElementAtIndex(i).intValue = BasketCapacities[i];
            SerializedProperty prices = fishingSo.FindProperty("_basketUpgradePrices");
            prices.arraySize = BasketUpgradePrices.Length;
            for (int i = 0; i < BasketUpgradePrices.Length; i++)
                prices.GetArrayElementAtIndex(i).longValue = BasketUpgradePrices[i];
            fishingSo.FindProperty("_wormBait").objectReferenceValue = FindBait(baits, WormBaitId);
            fishingSo.FindProperty("_wormChancePercent").intValue = WormChancePercent;
            fishingSo.FindProperty("_wormMin").intValue = WormMin;
            fishingSo.FindProperty("_wormMax").intValue = WormMax;
            fishingSo.ApplyModifiedPropertiesWithoutUndo();

            var farmSo = new SerializedObject(farm);
            farmSo.FindProperty("_fishing").objectReferenceValue = fishing;
            farmSo.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log($"Về Quê: đã tạo {fish.Length} cá, {rods.Length} cần, {baits.Length} mồi → {FishingPath}", fishing);
            return fishing;
        }

        private static BaitDefinition FindBait(BaitDefinition[] baits, string id)
        {
            foreach (BaitDefinition bait in baits)
            {
                if (bait.Id == id)
                    return bait;
            }
            throw new System.InvalidOperationException($"Không có mồi '{id}' trong bảng mồi.");
        }

        private static RodDefinition FindRod(RodDefinition[] rods, string id)
        {
            foreach (RodDefinition rod in rods)
            {
                if (rod.Id == id)
                    return rod;
            }
            throw new System.InvalidOperationException($"Không có cần '{id}' trong bảng cần.");
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
