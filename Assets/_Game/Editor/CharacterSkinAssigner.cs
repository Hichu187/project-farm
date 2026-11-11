using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VeQue.View.Character;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Gán sprite nhân vật. Tạo (nếu chưa có) Assets/_Game/Data/CharacterSkin.asset rồi gán khung
    // từ 2 sheet Sprout Lands, tìm sprite theo vị trí ô 48×48 trên sheet nên không phụ thuộc tên sprite.
    // Thứ tự hàng (đã xem sheet): xuống, lên, trái, phải.
    // - Spritesheet: cột 0–1 đứng, cột 2–3 đi.
    // - Actions: hàng 0–3 cuốc, hàng 8–11 bình tưới (hàng 4–7 rìu, chưa dùng); mỗi hàng 2 khung.
    // Chạy lại sẽ ghi đè khung (giữ Fps / Loop đã chỉnh).
    public static class CharacterSkinAssigner
    {
        private const string SheetFolder = "Assets/Art/Sprout Lands - Sprites - Basic pack/Characters/";
        private const string BaseSheet = SheetFolder + "Basic Charakter Spritesheet.png";
        private const string ActionSheet = SheetFolder + "Basic Charakter Actions.png";
        private const string SkinPath = DefaultDataGenerator.DataFolder + "/CharacterSkin.asset";
        private const int CellSize = 48;
        private const float ExpectedPpu = 16f;

        private static readonly string[] FacingFields = { "Down", "Up", "Left", "Right" };
        private const int HoeFirstRow = 0;
        private const int WaterFirstRow = 8;

        [MenuItem("Về Quê/Gán sprite nhân vật")]
        private static void Assign()
        {
            var errors = new List<string>();
            Dictionary<Vector2Int, Sprite> baseSheet = LoadSheet(BaseSheet, errors);
            Dictionary<Vector2Int, Sprite> actions = LoadSheet(ActionSheet, errors);

            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                    Require(baseSheet, column, row, BaseSheet, errors);
                for (int column = 0; column < 2; column++)
                {
                    Require(actions, column, HoeFirstRow + row, ActionSheet, errors);
                    Require(actions, column, WaterFirstRow + row, ActionSheet, errors);
                }
            }

            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog("Gán sprite nhân vật", "Chưa gán gì vì:\n" + string.Join("\n", errors), "OK");
                return;
            }

            var skin = AssetDatabase.LoadAssetAtPath<CharacterSkin>(SkinPath);
            bool created = skin == null;
            if (created)
            {
                skin = ScriptableObject.CreateInstance<CharacterSkin>();
                AssetDatabase.CreateAsset(skin, SkinPath);
            }

            var so = new SerializedObject(skin);
            for (int facing = 0; facing < 4; facing++)
            {
                SetFrames(so, "_idle", facing, baseSheet, facing, 0, 1);
                SetFrames(so, "_walk", facing, baseSheet, facing, 2, 3);
                SetFrames(so, "_hoe", facing, actions, HoeFirstRow + facing, 0, 1);
                SetFrames(so, "_water", facing, actions, WaterFirstRow + facing, 0, 1);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skin);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(skin);

            EditorUtility.DisplayDialog("Gán sprite nhân vật",
                (created ? "Đã tạo " : "Đã cập nhật ") + SkinPath + ": đứng, đi, cuốc, tưới × 4 hướng.", "OK");
        }

        private static void SetFrames(SerializedObject so, string clip, int facing,
            Dictionary<Vector2Int, Sprite> sheet, int row, params int[] columns)
        {
            SerializedProperty array = so.FindProperty(clip + "." + FacingFields[facing]);
            array.arraySize = columns.Length;
            for (int i = 0; i < columns.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = sheet[new Vector2Int(columns[i], row)];
        }

        private static void Require(Dictionary<Vector2Int, Sprite> sheet, int column, int row, string path, List<string> errors)
        {
            if (sheet.Count > 0 && !sheet.ContainsKey(new Vector2Int(column, row)))
                errors.Add($"{path}: thiếu sprite ở cột {column}, hàng {row} (cần cắt lưới 48×48).");
        }

        // Khóa là (cột, hàng) tính từ góc trên-trái; rect của sprite tính từ góc dưới-trái.
        private static Dictionary<Vector2Int, Sprite> LoadSheet(string path, List<string> errors)
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

                Rect rect = sprite.rect;
                int column = Mathf.FloorToInt(rect.center.x) / CellSize;
                int row = (sprite.texture.height - Mathf.FloorToInt(rect.center.y) - 1) / CellSize;
                result[new Vector2Int(column, row)] = sprite;
            }

            if (result.Count == 0 && errors.Count == 0)
                errors.Add($"{path}: chưa cắt sprite (hoặc không tìm thấy file).");
            return result;
        }
    }
}
