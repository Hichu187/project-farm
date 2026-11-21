using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VeQue.Logic;
using VeQue.View.Data;

namespace VeQue.EditorTools
{
    // Data câu cá mặc định (MVP-Plan Phần 10 giai đoạn 4, duyệt 12/11/2026): 20 cá (thêm 7 loài 20/11/2026), 10 cần, 10 mồi,
    // 7 rác + mốc album (Đợt 2 Phần 5, 21/11/2026).
    // Chạy lại được: ghi số liệu và gán icon cá / cần / mồi theo file trong Art/Fishing. Con số chung (FishingTuning) về mặc định.
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
        // Giỏ (duyệt 13/11/2026): mở sẵn 8 ô, mua lần lượt tới 25 ô; giá ô thứ k (k = 1..17) = 100 × 1,3^(k−1), tròn 10 xu.
        private const int BasketStartSlots = 8;
        private const int BasketMaxSlots = 25;
        public static long BasketSlotPrice(int k) => (long)System.Math.Round(100.0 * System.Math.Pow(1.3, k - 1) / 10.0) * 10;
        private const string WormBaitId = "giun";
        private const int WormChancePercent = 10;
        private const int WormMin = 1;
        private const int WormMax = 2;

        // Độ khó + XP theo độ hiếm (Thường → Huyền thoại).
        // 13/11/2026: Huyền thoại 8–10 → 7–9 mũi tên; khung giật rộng hơn (0,9…0,4 → 1,6…0,8 giây) cho dễ để ý.
        // 14/11/2026: mọi hạng +1 mũi tên (tối thiểu 4, xem FishingTuning.MinPullArrows).
        private static readonly int[] MinArrows = { 4, 5, 6, 7, 8 };
        private static readonly int[] MaxArrows = { 5, 6, 8, 9, 10 };
        private static readonly float[] PullSeconds = { 4f, 4f, 3.5f, 3.2f, 3f };
        private static readonly float[] HookWindows = { 1.6f, 1.4f, 1.2f, 1.0f, 0.8f };
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
            // 14/11/2026: thêm mồi ưa thích cho 5 mồi mới (ruồi, dế, nhái, nhái hơi, thìa vàng).
            new FishRow("ca_ro", "Cá rô", FishRarity.Common, 80, 400, 10, AllDay, FishDepth.All, "giun", "moi_ruoi", "moi_de"),
            new FishRow("ca_diec", "Cá diếc", FishRarity.Common, 100, 600, 12, AllDay, FishDepth.Near | FishDepth.Mid, "giun", "cam"),
            new FishRow("tep", "Tép", FishRarity.Common, 5, 30, 8, new float[] { 1, 1, 1, 1, 1, 2, 1 }, FishDepth.Near, "cam"),
            new FishRow("ca_chep", "Cá chép", FishRarity.Uncommon, 500, 4000, 40, new float[] { 1, 2, 0, 1, 0, 0, 0 }, FishDepth.Mid | FishDepth.Far, "cam"),
            new FishRow("ca_tre", "Cá trê", FishRarity.Uncommon, 300, 2500, 45, new float[] { 0, 0, 0, 0, 0, 1, 1 }, FishDepth.Near | FishDepth.Mid, "giun", "moi_de"),
            new FishRow("ca_me", "Cá mè", FishRarity.Uncommon, 500, 5000, 35, new float[] { 0, 1, 1, 1, 0, 0, 0 }, FishDepth.Mid | FishDepth.Far, "cam", "moi_ruoi"),
            new FishRow("ca_loc", "Cá lóc", FishRarity.Rare, 400, 3000, 120, new float[] { 2, 1, 0, 0, 2, 0, 0 }, FishDepth.Mid | FishDepth.Far,
                "moi_tep", "moi_gia", "moi_de", "moi_nhai", "moi_nhai_hoi"),
            new FishRow("luon", "Lươn", FishRarity.Rare, 200, 1200, 110, new float[] { 0, 0, 0, 0, 0, 1, 1 }, FishDepth.Near, "giun"),
            new FishRow("tom_cang", "Tôm càng xanh", FishRarity.Rare, 50, 400, 130, new float[] { 0, 0, 0, 0, 1, 2, 0 }, FishDepth.Near | FishDepth.Mid, "moi_tep"),
            new FishRow("ca_bong_tuong", "Cá bống tượng", FishRarity.Epic, 300, 1500, 320, new float[] { 0, 0, 0, 1, 1, 0, 0 }, FishDepth.Mid | FishDepth.Far,
                "moi_tep", "moi_nhai", "moi_nhai_hoi"),
            new FishRow("ca_chep_do", "Cá chép đỏ", FishRarity.Epic, 500, 3000, 380, new float[] { 0, 1, 0, 0, 0, 0, 0 }, FishDepth.Far, "moi_thom", "moi_thia_vang"),
            new FishRow("ca_chep_vang", "Cá chép vàng", FishRarity.Legendary, 1000, 6000, 900, new float[] { 1, 0, 0, 0, 0, 0, 0 }, FishDepth.Far, "moi_thom", "moi_thia_vang"),
            new FishRow("ca_tram_den", "Cá trắm đen khổng lồ", FishRarity.Legendary, 5000, 30000, 1000, new float[] { 0, 0, 0, 0, 0, 0, 1 }, FishDepth.Far,
                "moi_thom", "moi_gia", "moi_nhai_hoi", "moi_thia_vang"),
            // 20/11/2026: thêm 7 loài đã có art (thứ tự asset 14–20, 13 loài cũ giữ nguyên tên file).
            new FishRow("ca_ro_phi", "Cá rô phi", FishRarity.Common, 200, 1200, 14, AllDay, FishDepth.Near | FishDepth.Mid, "giun", "cam"),
            new FishRow("ca_chach", "Cá chạch", FishRarity.Common, 20, 150, 10, new float[] { 1, 1, 1, 1, 1, 2, 1 }, FishDepth.Near, "giun"),
            new FishRow("ca_troi", "Cá trôi", FishRarity.Uncommon, 300, 3000, 38, new float[] { 1, 1, 1, 1, 0, 0, 0 }, FishDepth.Mid | FishDepth.Far, "cam", "moi_ruoi"),
            new FishRow("ca_that_lat", "Cá thát lát", FishRarity.Uncommon, 200, 1500, 50, new float[] { 1, 0, 0, 0, 1, 1, 0 }, FishDepth.Near | FishDepth.Mid,
                "giun", "moi_tep"),
            new FishRow("ca_lang", "Cá lăng", FishRarity.Rare, 500, 5000, 140, new float[] { 0, 0, 0, 0, 1, 1, 2 }, FishDepth.Far, "moi_tep", "moi_nhai", "moi_gia"),
            new FishRow("ca_anh_vu", "Cá anh vũ", FishRarity.Epic, 800, 4000, 450, new float[] { 1, 1, 0, 0, 0, 0, 0 }, FishDepth.Far, "moi_thom", "moi_gia"),
            new FishRow("ca_ho", "Cá hô", FishRarity.Legendary, 10000, 60000, 1100, new float[] { 0, 0, 1, 0, 0, 0, 0 }, FishDepth.Far, "moi_thom", "moi_thia_vang"),
        };

        // Gợi ý album khi chưa câu được (Đợt 2 Phần 5, nháp – người dùng sửa trong asset cá). KHÔNG nhắc giờ:
        // chỉ độ sâu, mồi (nói mơ hồ), tính cách cá. FarmDefinitionTests kiểm không có từ chỉ giờ.
        private static readonly Dictionary<string, string> Hints = new Dictionary<string, string>
        {
            { "ca_ro", "Loài quen mặt ở mọi khúc sông, mồi gì cũng đớp." },
            { "ca_diec", "Hay lởn vởn gần bờ, mê mùi cám." },
            { "tep", "Bé tí, chỉ quanh quẩn sát bờ. Rắc chút cám là kéo đến." },
            { "ca_chep", "Ăn đáy ở chỗ nước vừa và xa bờ. Cám thơm là dụ được." },
            { "ca_tre", "Thích chỗ bùn gần bờ, khoái giun với côn trùng." },
            { "ca_me", "Bơi thành đàn ở giữa sông, hay đớp mồi nổi." },
            { "ca_loc", "Kẻ săn mồi hung dữ, thích mồi sống còn động đậy." },
            { "luon", "Trốn trong hang bùn sát bờ, chỉ chịu mồi giun." },
            { "tom_cang", "Càng dài, rình gần bờ. Mồi tép là thứ khó cưỡng." },
            { "ca_bong_tuong", "Nằm im dưới đáy rồi đớp bất ngờ. Ưa mồi sống." },
            { "ca_chep_do", "Hiếm gặp, chỉ ở chỗ nước sâu xa bờ. Phải có mồi thật xịn." },
            { "ca_chep_vang", "Truyền thuyết của làng: ở tít ngoài xa, chỉ màng tới mồi quý nhất." },
            { "ca_tram_den", "Bóng đen khổng lồ ngoài nước sâu, kéo được là phải dai sức." },
            { "ca_ro_phi", "Dễ tính, ăn khỏe, hay ra gần bờ." },
            { "ca_chach", "Trơn tuột, rúc bùn sát mép nước, khoái giun." },
            { "ca_troi", "Hiền lành, ăn tạp ở khúc nước vừa." },
            { "ca_that_lat", "Thân dẹt như lưỡi dao, hay ẩn ven bờ cỏ." },
            { "ca_lang", "Có râu dài, sống ở chỗ nước sâu, ưa mồi tanh." },
            { "ca_anh_vu", "Miệng như cái vòi, quý như vàng. Nghe nói chỉ ăn mồi thơm hảo hạng." },
            { "ca_ho", "Cá khổng lồ hiếm có, sức kéo như trâu. Chỉ ở chỗ sâu nhất." },
        };

        // Rác (Đợt 2 Phần 5, nháp): id, tên, giá cô đồng nát trả, trọng số, câu đùa, màu ô vuông tạm.
        private readonly struct JunkRow
        {
            public readonly string Id;
            public readonly string Name;
            public readonly int Price;
            public readonly int Weight;
            public readonly string Flavor;
            public readonly Color32 Color;

            public JunkRow(string id, string name, int price, int weight, string flavor, uint rgb)
            {
                Id = id;
                Name = name;
                Price = price;
                Weight = weight;
                Flavor = flavor;
                Color = new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 0xff);
            }
        }

        // Thứ tự = số trong tên file art (junk_NN_<id>.png). 21/11/2026: thêm 3 món theo art người dùng; 9 món thường
        // cùng trọng số (người dùng chốt), hũ tiền xu cổ vẫn hiếm.
        private const int JunkWeight = 10;
        private const int JarWeight = 2;

        private static readonly JunkRow[] Junk =
        {
            new JunkRow("dep_to_ong", "Dép tổ ong", 2, JunkWeight, "Ai vứt dép ở đây vậy trời!", 0x4a8fe0),
            new JunkRow("non_la_rach", "Nón lá rách", 2, JunkWeight, "Nón ai bay xuống sông từ hồi nào không biết.", 0xe0cc8a),
            new JunkRow("lon_bia", "Lon bia", 3, JunkWeight, "Uống xong vứt luôn xuống sông. Ý thức đâu hết rồi!", 0xd84440),
            new JunkRow("chai_nhua", "Chai nhựa", 2, JunkWeight, "Lại một cái chai nhựa. Khổ thân con sông.", 0x9ad8e8),
            new JunkRow("ung_cao_su", "Ủng cao su", 3, JunkWeight, "Một chiếc ủng... chiếc kia đâu rồi?", 0x3e6b3a),
            new JunkRow("bao_tai", "Bao tải", 2, JunkWeight, "Tưởng cá to, hóa ra cái bao tải.", 0x9a7048),
            new JunkRow("tui_ni_long", "Túi ni lông", 1, JunkWeight, "Túi ni lông… cá ăn phải thì khổ.", 0x2b2b3a),
            new JunkRow("canh_cay_muc", "Cành cây mục", 1, JunkWeight, "Cành cây mục, đem về nhóm bếp được không nhỉ?", 0x6b4a2a),
            new JunkRow("sam_xe_dap", "Săm xe đạp", 3, JunkWeight, "Săm xe đạp thủng, chắc của thằng Tí.", 0x3a3a44),
            new JunkRow("hu_tien_co", "Hũ tiền xu cổ", 150, JarWeight, "Trời ơi, hũ tiền xu cổ! Hôm nay đúng là ngày may mắn.", 0xf0c040),
        };

        private static string JunkIconPath(int index, string id) => $"{FishingSpriteImporter.JunkFolder}/junk_{index + 1:00}_{id}.png";

        private const string JunkFolder = FishingFolder + "/Junk";
        public const string DexMilestonesPath = FishingFolder + "/dex_milestones.json";

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

            public RodRow(string id, string name, int rank, float perfectHalfWidth, float barPeriod, int bonusTime,
                int slipReduction, int rareBonus, int durability, int price, int unlockLevel)
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
            }
        }

        // 14/11/2026: 10 bậc (thêm nứa, mây, composite, titan, rồng; id cũ giữ nguyên cho save). Icon: Art/Fishing/rods/rod_NN_<id>.png.
        private static readonly RodRow[] Rods =
        {
            new RodRow("can_tre", "Cần tre", 1, 0.030f, 1.20f, 0, 0, 0, 30, 0, 1),
            new RodRow("can_nua", "Cần nứa", 2, 0.035f, 1.25f, 5, 2, 5, 40, 120, 2),
            new RodRow("can_truc", "Cần trúc", 3, 0.040f, 1.30f, 10, 3, 10, 50, 300, 3),
            new RodRow("can_may", "Cần mây", 4, 0.045f, 1.35f, 15, 5, 15, 65, 700, 4),
            new RodRow("can_thuy_tinh", "Cần sợi thủy tinh", 5, 0.050f, 1.40f, 20, 6, 20, 80, 1500, 5),
            new RodRow("can_composite", "Cần composite", 6, 0.055f, 1.50f, 25, 8, 25, 100, 3000, 7),
            new RodRow("can_carbon", "Cần carbon", 7, 0.060f, 1.60f, 30, 10, 30, 120, 6000, 9),
            new RodRow("can_titan", "Cần titan", 8, 0.070f, 1.70f, 40, 12, 40, 160, 11000, 11),
            new RodRow("can_vang", "Cần vàng", 9, 0.080f, 1.80f, 50, 15, 50, 200, 20000, 13),
            new RodRow("can_rong", "Cần rồng", 10, 0.090f, 1.90f, 60, 18, 70, 250, 40000, 15),
        };

        private readonly struct BaitRow
        {
            public readonly string Id;
            public readonly string Name;
            public readonly int Price;
            public readonly bool Buyable;
            public readonly int RareBonus;
            public readonly int UnlockLevel;
            // File icon trong Art/Fishing/bait (tên file khác id ở các mồi cũ).
            public readonly string IconFile;
            // Tỉ lệ dính rác (%) – Đợt 2 Phần 5 (thiet-ke-dot-2 mục 3).
            public readonly int JunkChance;

            public BaitRow(string id, string name, int price, bool buyable, int rareBonus, int unlockLevel, string iconFile, int junkChance)
            {
                JunkChance = junkChance;
                UnlockLevel = unlockLevel;
                IconFile = iconFile;
                Id = id;
                Name = name;
                Price = price;
                Buyable = buyable;
                RareBonus = rareBonus;
            }
        }

        // Cấp mở mua (duyệt 13/11/2026, mở rộng 10 mồi 14/11/2026): cám 1, ruồi 2, mồi tép 3, dế 4, nhái 5, mồi giả cá 6,
        // mồi thơm 9, nhái hơi 11, thìa vàng 13; giun không bán. Id cũ giữ nguyên cho save.
        private static readonly BaitRow[] Baits =
        {
            // Cột cuối: tỉ lệ rác (giun, cám 12; ruồi, tép 9; dế, nhái 6; mồi giả 3; mồi cao cấp 0).
            new BaitRow("giun", "Giun", 2, false, 0, 1, "bait_01_giun_dat", 12),
            new BaitRow("cam", "Cám", 3, true, 0, 1, "bait_02_cam", 12),
            new BaitRow("moi_ruoi", "Ruồi", 4, true, 0, 2, "bait_04_ruoi", 9),
            new BaitRow("moi_tep", "Mồi tép", 6, true, 0, 3, "bait_06_tep", 9),
            new BaitRow("moi_de", "Dế", 6, true, 5, 4, "bait_05_de", 6),
            new BaitRow("moi_nhai", "Nhái", 10, true, 10, 5, "bait_07_nhai", 6),
            new BaitRow("moi_gia", "Mồi giả cá", 12, true, 20, 6, "bait_08_moi_gia_ca", 3),
            new BaitRow("moi_thom", "Mồi thơm", 25, true, 100, 9, "bait_03_bot", 0),
            new BaitRow("moi_nhai_hoi", "Nhái hơi", 30, true, 60, 11, "bait_09_nhai_hoi", 0),
            new BaitRow("moi_thia_vang", "Thìa vàng", 50, true, 150, 13, "bait_10_thia_vang", 0),
        };

        private static string RodIconPath(int index, string id) => $"{FishingSpriteImporter.RodFolder}/rod_{index + 1:00}_{id}.png";
        private static string BaitIconPath(string file) => $"{FishingSpriteImporter.BaitFolder}/{file}.png";

        // Icon cá: file fish_NN_<tên>.png trong Art/Fishing/fish có tên quy ra đúng id (số thứ tự file khác thứ tự data).
        private static string FishIconPath(string id)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { FishingSpriteImporter.IconFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (FishingSpriteImporter.IdFromFile(System.IO.Path.GetFileNameWithoutExtension(path)) == id)
                    return path;
            }
            return $"{FishingSpriteImporter.IconFolder}/fish_??_{id}.png";
        }

        [MenuItem("Về Quê/Data/Tạo data câu cá", priority = 201)]
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
                baits[i] = LoadOrCreateById<BaitDefinition>(BaitsFolder, i, row.Id);
                var so = new SerializedObject(baits[i]);
                so.FindProperty("_id").stringValue = row.Id;
                so.FindProperty("_icon").objectReferenceValue = LoadIcon(BaitIconPath(row.IconFile));
                so.FindProperty("_displayName").stringValue = row.Name;
                so.FindProperty("_price").intValue = row.Price;
                so.FindProperty("_buyable").boolValue = row.Buyable;
                so.FindProperty("_rareBonusPercent").intValue = row.RareBonus;
                so.FindProperty("_unlockLevel").intValue = row.UnlockLevel;
                so.FindProperty("_junkChancePercent").intValue = row.JunkChance;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var rods = new RodDefinition[Rods.Length];
            for (int i = 0; i < Rods.Length; i++)
            {
                RodRow row = Rods[i];
                rods[i] = LoadOrCreateById<RodDefinition>(RodsFolder, i, row.Id);
                var so = new SerializedObject(rods[i]);
                so.FindProperty("_id").stringValue = row.Id;
                so.FindProperty("_icon").objectReferenceValue = LoadIcon(RodIconPath(i, row.Id));
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
                so.FindProperty("_icon").objectReferenceValue = LoadIcon(FishIconPath(row.Id));
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
                // Gợi ý: chỉ điền khi đang trống (người dùng sửa trong asset thì giữ).
                SerializedProperty hint = so.FindProperty("_hint");
                if (string.IsNullOrEmpty(hint.stringValue) && Hints.TryGetValue(row.Id, out string hintText))
                    hint.stringValue = hintText;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EnsureFolder(JunkFolder);
            var junk = new JunkDefinition[Junk.Length];
            for (int i = 0; i < Junk.Length; i++)
            {
                JunkRow row = Junk[i];
                junk[i] = LoadOrCreateById<JunkDefinition>(JunkFolder, i, row.Id);
                var so = new SerializedObject(junk[i]);
                so.FindProperty("_id").stringValue = row.Id;
                so.FindProperty("_displayName").stringValue = row.Name;
                so.FindProperty("_sellPrice").intValue = row.Price;
                so.FindProperty("_weight").intValue = row.Weight;
                so.FindProperty("_flavorText").stringValue = row.Flavor;
                // Icon: có art junk/junk_NN_<id>.png thì dùng (tự sửa import về Single, PPU 32); chưa có thì giữ icon
                // đang gán nếu là art khác, còn trống / ô vuông tạm thì gán ô vuông tạm.
                SerializedProperty icon = so.FindProperty("_icon");
                string artPath = JunkIconPath(i, row.Id);
                if (System.IO.File.Exists(artPath))
                    icon.objectReferenceValue = LoadIcon(artPath);
                else if (icon.objectReferenceValue == null
                    || FishingPlaceholderArt.IsPlaceholderJunkIcon(AssetDatabase.GetAssetPath(icon.objectReferenceValue)))
                    icon.objectReferenceValue = FishingPlaceholderArt.JunkSquare(row.Id, row.Color);
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
            SetObjectArray(fishingSo.FindProperty("_junk"), junk);
            fishingSo.FindProperty("_dexMilestones").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(DexMilestonesPath);
            fishingSo.FindProperty("_unlockLevel").intValue = UnlockLevel;
            fishingSo.FindProperty("_starterRod").objectReferenceValue = FindRod(rods, StarterRodId);
            fishingSo.FindProperty("_starterBait").objectReferenceValue = FindBait(baits, StarterBaitId);
            fishingSo.FindProperty("_starterBaitCount").intValue = StarterBaitCount;
            fishingSo.FindProperty("_basketStartSlots").intValue = BasketStartSlots;
            SerializedProperty prices = fishingSo.FindProperty("_basketSlotPrices");
            prices.arraySize = BasketMaxSlots - BasketStartSlots;
            for (int i = 0; i < prices.arraySize; i++)
                prices.GetArrayElementAtIndex(i).longValue = BasketSlotPrice(i + 1);
            fishingSo.FindProperty("_wormBait").objectReferenceValue = FindBait(baits, WormBaitId);
            fishingSo.FindProperty("_wormChancePercent").intValue = WormChancePercent;
            fishingSo.FindProperty("_wormMin").intValue = WormMin;
            fishingSo.FindProperty("_wormMax").intValue = WormMax;
            fishingSo.ApplyModifiedPropertiesWithoutUndo();
            // Bảng cá shiny (nếu đã chạy menu Cá shiny: tạo material).
            ShinyMaterialGenerator.AssignToFishing();

            var farmSo = new SerializedObject(farm);
            farmSo.FindProperty("_fishing").objectReferenceValue = fishing;
            farmSo.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log($"Về Quê: đã tạo {fish.Length} cá, {rods.Length} cần, {baits.Length} mồi, {junk.Length} rác → {FishingPath}", fishing);
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

        // Asset "NN_<id>.asset" theo thứ tự trong bảng. Thứ tự đổi (chèn cần / mồi mới) thì đổi tên file cũ cùng id
        // (MoveAsset giữ GUID → FishingDefinition, mồi ưa thích của cá không mất tham chiếu).
        private static T LoadOrCreateById<T>(string folder, int index, string id) where T : ScriptableObject
        {
            string path = $"{folder}/{index + 1:00}_{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<T>(path) == null)
            {
                foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder }))
                {
                    string old = AssetDatabase.GUIDToAssetPath(guid);
                    string name = System.IO.Path.GetFileNameWithoutExtension(old);
                    int underscore = name.IndexOf('_');
                    if (underscore < 0 || name.Substring(underscore + 1) != id)
                        continue;
                    string error = AssetDatabase.MoveAsset(old, path);
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogError($"Về Quê: không đổi tên {old} → {path}: {error}");
                    break;
                }
            }
            return LoadOrCreate<T>(path);
        }

        // Icon 32×32 (đưa về Single trước nếu chưa); thiếu file thì cảnh báo, trả null.
        private static Sprite LoadIcon(string path)
        {
            if (!FishingSpriteImporter.EnsureSingleIcon(path))
            {
                Debug.LogWarning($"Về Quê: thiếu icon {path}");
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
