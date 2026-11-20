using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VeQue.View.Data;
using VeQue.View.Map;

namespace VeQue.EditorTools
{
    // Ngày đêm (Đợt 2 Phần 13): menu tạo material VeQue/WorldLight, DayNightProfile (màu nháp – chỉnh sau khi chụp
    // màn hình), gắn DayNightController vào Main Camera và tạo vùng đom đóm. Chạy lại thì cập nhật, không tạo trùng.
    // Profile đã có thì giữ màu người dùng đã chỉnh (chỉ điền khi trống).
    public static class DayNightSetup
    {
        private const string MaterialPath = "Assets/_Game/Art/WorldLight.mat";
        private const string ProfilePath = DefaultDataGenerator.DataFolder + "/DayNightProfile.asset";
        private const string ShaderName = "VeQue/WorldLight";
        private const string FirefliesName = "Fireflies";

        // Màu nháp (hệ số nhân). Đêm: độ sáng ≈ 0,71 (luật không tối quá 30%).
        private static readonly (float Hour, string Hex)[] DefaultKeys =
        {
            (0f, "a8b3eb"),    // Khuya
            (4.5f, "a8b3eb"),  // Khuya → rạng sáng
            (6f, "fad6cc"),    // Bình minh hồng cam
            (7.5f, "ffffff"),  // Sáng
            (12f, "ffffff"),   // Trưa
            (15.5f, "fffaed"), // Chiều vàng nhẹ
            (17.5f, "ffdbb3"), // Hoàng hôn cam
            (19f, "d1c2e6"),   // Chạng vạng tím
            (21f, "a8b3eb"),   // Tối
        };

        [MenuItem("Về Quê/Dựng scene/Ngày đêm", priority = 102)]
        public static void Setup()
        {
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog("Ngày đêm", $"Không tìm thấy shader {ShaderName}.", "OK");
                return;
            }
            Camera camera = Camera.main;
            if (camera == null)
            {
                EditorUtility.DisplayDialog("Ngày đêm", "Scene đang mở cần có Main Camera (tag MainCamera).", "OK");
                return;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "WorldLight" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.shader = shader;

            DayNightProfile profile = AssetDatabase.LoadAssetAtPath<DayNightProfile>(ProfilePath);
            if (profile == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ProfilePath));
                profile = ScriptableObject.CreateInstance<DayNightProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }
            var profileSo = new SerializedObject(profile);
            SerializedProperty keys = profileSo.FindProperty("_keys");
            if (keys.arraySize == 0)
            {
                keys.arraySize = DefaultKeys.Length;
                for (int i = 0; i < DefaultKeys.Length; i++)
                {
                    SerializedProperty key = keys.GetArrayElementAtIndex(i);
                    key.FindPropertyRelative("Hour").floatValue = DefaultKeys[i].Hour;
                    ColorUtility.TryParseHtmlString("#" + DefaultKeys[i].Hex, out Color color);
                    key.FindPropertyRelative("Color").colorValue = color;
                }
                profileSo.ApplyModifiedPropertiesWithoutUndo();
            }

            DayNightController controller = camera.GetComponent<DayNightController>();
            if (controller == null)
                controller = Undo.AddComponent<DayNightController>(camera.gameObject);
            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("_game").objectReferenceValue = Object.FindFirstObjectByType<GameBootstrap>();
            controllerSo.FindProperty("_profile").objectReferenceValue = profile;
            controllerSo.FindProperty("_material").objectReferenceValue = material;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            // Đom đóm: tạo 1 vùng ở giữa map nông trại (44×26) nếu chưa có; người dùng kéo / chỉnh Size cho đúng chỗ.
            Fireflies fireflies = Object.FindFirstObjectByType<Fireflies>();
            if (fireflies == null)
            {
                var go = new GameObject(FirefliesName);
                Undo.RegisterCreatedObjectUndo(go, "Tạo đom đóm");
                go.transform.position = new Vector3(22f, 13f, 0f);
                fireflies = go.AddComponent<Fireflies>();
            }
            var firefliesSo = new SerializedObject(fireflies);
            firefliesSo.FindProperty("_dayNight").objectReferenceValue = controller;
            firefliesSo.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(material);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
            Selection.activeObject = profile;
            Debug.Log("Ngày đêm: đã gắn DayNightController vào Main Camera, tạo WorldLight.mat, DayNightProfile, vùng đom đóm. "
                + "Thêm đèn dầu: tạo object con ở ô cửa sổ nhà → Add Component → World Light. Nhớ Ctrl+S.");
        }
    }
}
