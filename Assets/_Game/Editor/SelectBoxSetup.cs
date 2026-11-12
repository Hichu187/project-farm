using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VeQue.View.Anim;
using VeQue.View.CameraControl;
using VeQue.View.Character;
using VeQue.View.Map;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Khung chọn: tạo asset + gán vào scene.
    // Tạo (hoặc cập nhật) SelectBoxSkin từ Sunnyside UI/selectbox_*, gán vào FarmView và FarmCharacterController;
    // thêm ZoneSelectHighlight lên camera nếu chưa có. UnlockDialog tự tìm FarmView nên không cần gán.
    // Không sửa cấu hình import: chỉ cảnh báo nếu sprite chưa PPU 16 / Point / Compression None. Chạy lại nhiều lần không sao.
    public static class SelectBoxSetup
    {
        private const string UiFolder = "Assets/Art/Sunnyside_World_ASSET_PACK_V2.1/Sunnyside_World_Assets/UI/";
        private const string SkinPath = DefaultDataGenerator.DataFolder + "/SelectBoxSkin.asset";
        private const string Title = "Khung chọn";

        private static readonly string[] CornerFiles = { "selectbox_tl", "selectbox_tr", "selectbox_bl", "selectbox_br" };
        private static readonly string[] CornerFields = { "_topLeft", "_topRight", "_bottomLeft", "_bottomRight" };

        [MenuItem("Về Quê/Khung chọn: tạo asset + gán vào scene")]
        private static void Setup()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var sprites = new Sprite[CornerFiles.Length];
            for (int i = 0; i < CornerFiles.Length; i++)
            {
                string path = UiFolder + CornerFiles[i] + ".png";
                sprites[i] = LoadFirstSprite(path);
                if (sprites[i] == null)
                    errors.Add($"Không thấy sprite trong {path}.");
                else
                    CheckImport(path, warnings);
            }

            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog(Title, "Chưa làm gì vì:\n" + string.Join("\n", errors), "OK");
                return;
            }

            SelectBoxSkin skin = AssetDatabase.LoadAssetAtPath<SelectBoxSkin>(SkinPath);
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<SelectBoxSkin>();
                AssetDatabase.CreateAsset(skin, SkinPath);
            }

            var skinSo = new SerializedObject(skin);
            for (int i = 0; i < CornerFields.Length; i++)
                skinSo.FindProperty(CornerFields[i]).objectReferenceValue = sprites[i];
            skinSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skin);
            AssetDatabase.SaveAssets();

            var done = new List<string>();
            AssignSkin(Object.FindFirstObjectByType<FarmView>(), skin, "FarmView", done, warnings);
            AssignSkin(Object.FindFirstObjectByType<FarmCharacterController>(), skin, "FarmCharacterController", done, warnings);

            CameraController camera = Object.FindFirstObjectByType<CameraController>();
            if (camera == null)
            {
                warnings.Add("Không thấy CameraController trong scene, chưa thêm ZoneSelectHighlight.");
            }
            else
            {
                ZoneSelectHighlight highlight = camera.GetComponent<ZoneSelectHighlight>();
                if (highlight == null)
                    highlight = Undo.AddComponent<ZoneSelectHighlight>(camera.gameObject);
                var so = new SerializedObject(highlight);
                so.FindProperty("_controller").objectReferenceValue = camera;
                so.FindProperty("_skin").objectReferenceValue = skin;
                so.ApplyModifiedProperties();
                done.Add("ZoneSelectHighlight (trên camera)");
            }

            EditorSceneManager.MarkAllScenesDirty();
            string message = $"Đã tạo / cập nhật {SkinPath}.\nĐã gán: " + string.Join(", ", done) + ".\nNhớ lưu scene (Ctrl+S).";
            if (warnings.Count > 0)
                message += "\n\nLưu ý:\n" + string.Join("\n", warnings);
            EditorUtility.DisplayDialog(Title, message, "OK");
        }

        private static void AssignSkin(Object target, SelectBoxSkin skin, string label, List<string> done, List<string> warnings)
        {
            if (target == null)
            {
                warnings.Add($"Không thấy {label} trong scene.");
                return;
            }

            var so = new SerializedObject(target);
            so.FindProperty("_selectBoxSkin").objectReferenceValue = skin;
            so.ApplyModifiedProperties();
            done.Add(label);
        }

        private static Sprite LoadFirstSprite(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                    return sprite;
            }
            return null;
        }

        private static void CheckImport(string path, List<string> warnings)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                return;

            string name = System.IO.Path.GetFileName(path);
            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 16f))
                warnings.Add($"{name}: Pixels Per Unit nên là 16.");
            if (importer.filterMode != FilterMode.Point)
                warnings.Add($"{name}: Filter Mode nên là Point.");
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                warnings.Add($"{name}: Compression nên là None.");
            if (importer.mipmapEnabled)
                warnings.Add($"{name}: nên tắt Generate Mip Maps.");
        }
    }
}
