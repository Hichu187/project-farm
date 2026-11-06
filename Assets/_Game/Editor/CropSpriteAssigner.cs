using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Gán sprite 4 cây. Tìm sprite theo vị trí trên sheet nên không phụ thuộc tên sprite
    // và dùng được cho cả cắt theo lưới 16×16 lẫn cắt tự động.
    // Cây đã có sprite giai đoạn thì bỏ qua để giữ phần gán tay. Chạy lại nhiều lần không sao.
    public static class CropSpriteAssigner
    {
        private const string SheetFolder = "Assets/Art/Sprout Sorry pack/Early Access/Plant update 2/";
        private const string PlantSheet = SheetFolder + "Farming Plants v2.png";
        private const string ItemSheet = SheetFolder + "Farming Plants items v2.png";
        private const string CropFolder = DefaultDataGenerator.DataFolder + "/Crops/";
        private const int CellSize = 16;
        private const float ExpectedPpu = 16f;

        // Cột trên sheet cây ứng với từng giai đoạn; số phần tử là số giai đoạn của cây.
        private static readonly int[] StageColumns = { 1, 2, 3, 4, 5 };

        // Hàng và cột đếm từ 0, từ góc trên-trái của sheet.
        private readonly struct CropArt
        {
            public readonly string Asset;
            public readonly int PlantRow;
            public readonly int ItemRow;
            public readonly int SeedColumn;
            public readonly int ProduceColumn;

            public CropArt(string asset, int plantRow, int itemRow, int seedColumn, int produceColumn)
            {
                Asset = asset;
                PlantRow = plantRow;
                ItemRow = itemRow;
                SeedColumn = seedColumn;
                ProduceColumn = produceColumn;
            }
        }

        private static readonly CropArt[] Crops =
        {
            new CropArt("01_rau_muong", 32, 13, 2, 3),
            new CropArt("04_cai_xanh", 7, 6, 0, 1),
            new CropArt("08_ca_chua", 4, 3, 0, 1),
            new CropArt("13_bi_do", 9, 8, 0, 1),
        };

        [MenuItem("Về Quê/Gán sprite 4 cây")]
        private static void Assign()
        {
            var errors = new List<string>();
            var targets = new List<CropArt>();
            var skipped = new List<string>();
            foreach (CropArt art in Crops)
            {
                string path = CropFolder + art.Asset + ".asset";
                Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null)
                    errors.Add($"Không thấy {path}.");
                else if (HasStageSprite(new SerializedObject(asset)))
                    skipped.Add(art.Asset);
                else
                    targets.Add(art);
            }

            Dictionary<Vector2Int, Sprite> plants = LoadSheet(PlantSheet, true, errors);
            Dictionary<Vector2Int, Sprite> items = LoadSheet(ItemSheet, false, errors);

            foreach (CropArt art in targets)
            {
                foreach (int column in StageColumns)
                    Require(plants, column, art.PlantRow, PlantSheet, errors);
                Require(items, art.SeedColumn, art.ItemRow, ItemSheet, errors);
                Require(items, art.ProduceColumn, art.ItemRow, ItemSheet, errors);
            }

            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog("Gán sprite 4 cây", "Chưa gán gì vì:\n" + string.Join("\n", errors), "OK");
                return;
            }

            foreach (CropArt art in targets)
            {
                var so = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(CropFolder + art.Asset + ".asset"));
                so.FindProperty("_seedSprite").objectReferenceValue = items[new Vector2Int(art.SeedColumn, art.ItemRow)];
                so.FindProperty("_produceIcon").objectReferenceValue = items[new Vector2Int(art.ProduceColumn, art.ItemRow)];
                AssignStages(so.FindProperty("_stageSprites"), plants, art.PlantRow);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            AssetDatabase.SaveAssets();
            string message = $"Đã gán sprite cho {targets.Count} cây.";
            if (skipped.Count > 0)
                message += "\nBỏ qua vì đã có sprite: " + string.Join(", ", skipped) + ".";
            EditorUtility.DisplayDialog("Gán sprite 4 cây", message, "OK");
        }

        private static bool HasStageSprite(SerializedObject crop)
        {
            SerializedProperty stages = crop.FindProperty("_stageSprites");
            for (int i = 0; i < stages.arraySize; i++)
            {
                if (stages.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    return true;
            }
            return false;
        }

        private static void AssignStages(SerializedProperty array, Dictionary<Vector2Int, Sprite> sheet, int row)
        {
            array.arraySize = StageColumns.Length;
            for (int i = 0; i < StageColumns.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = sheet[new Vector2Int(StageColumns[i], row)];
        }

        private static void Require(Dictionary<Vector2Int, Sprite> sheet, int column, int row, string path, List<string> errors)
        {
            if (sheet.Count > 0 && !sheet.ContainsKey(new Vector2Int(column, row)))
                errors.Add($"{path}: thiếu sprite ở cột {column}, hàng {row}.");
        }

        // Khóa là (cột, hàng) tính từ góc trên-trái. Sprite thuộc ô chứa giữa cạnh dưới của nó,
        // vì cây cao có thể nhô lên hàng trên; rect của sprite tính từ góc dưới-trái.
        private static Dictionary<Vector2Int, Sprite> LoadSheet(string path, bool requireBottomPivot, List<string> errors)
        {
            var result = new Dictionary<Vector2Int, Sprite>();
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (!(asset is Sprite sprite))
                    continue;

                if (!Mathf.Approximately(sprite.pixelsPerUnit, ExpectedPpu))
                {
                    errors.Add($"{path}: Pixels Per Unit phải là {ExpectedPpu}.");
                    return result;
                }
                if (requireBottomPivot && !Mathf.Approximately(sprite.pivot.y, 0f))
                {
                    errors.Add($"{path}: Pivot của sprite phải là Bottom.");
                    return result;
                }

                Rect rect = sprite.rect;
                int column = Mathf.FloorToInt(rect.center.x) / CellSize;
                int row = (sprite.texture.height - Mathf.RoundToInt(rect.y) - 1) / CellSize;
                result[new Vector2Int(column, row)] = sprite;
            }

            if (result.Count == 0 && errors.Count == 0)
                errors.Add($"{path}: chưa cắt sprite.");
            return result;
        }
    }
}
