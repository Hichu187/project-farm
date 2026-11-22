using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VeQue.View.Map;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Dựng scene → Trang trí Tết (Đợt 2 Phần 12a): sinh art tạm (ô vuông) cây nêu + 2 câu đối ở
    // Assets/_Game/Art/Tet_tam/, tạo object "TetDecor" gần cổng nông trại (người dùng kéo lại cho đúng chỗ).
    // Chạy lại: giữ vị trí các món đã kéo, chỉ gán lại sprite tạm nếu đang trống. Thay art thật: kéo sprite vào SpriteRenderer.
    public static class TetDecorBuilder
    {
        private const string Folder = "Assets/_Game/Art/Tet_tam";
        private const string CayNeuPath = Folder + "/cay_neu.png";
        private const string CauDoiPath = Folder + "/cau_doi.png";
        private const string RootName = "TetDecor";

        private static readonly Color32 Outline = new Color32(0x2b, 0x2b, 0x3a, 0xff);
        private static readonly Color32 Bamboo = new Color32(0x9a, 0x7a, 0x3c, 0xff);
        private static readonly Color32 Red = new Color32(0xc8, 0x2c, 0x2c, 0xff);
        private static readonly Color32 Gold = new Color32(0xf0, 0xc0, 0x40, 0xff);

        [MenuItem("Về Quê/Dựng scene/Trang trí Tết", priority = 106)]
        private static void Build()
        {
            Sprite cayNeu = Square(CayNeuPath, 16, 48, Bamboo);
            Sprite cauDoi = Square(CauDoiPath, 8, 32, Red);

            var game = Object.FindFirstObjectByType<GameBootstrap>();
            Vector2Int gate = FarmGate();
            GameObject root = GameObject.Find(RootName);
            bool created = root == null;
            if (created)
            {
                root = new GameObject(RootName);
                Undo.RegisterCreatedObjectUndo(root, "Trang trí Tết");
            }
            if (!root.TryGetComponent(out TetDecor decor))
                decor = Undo.AddComponent<TetDecor>(root);

            // Vị trí gợi ý quanh cổng (tâm đáy ô); đã có thì giữ chỗ người dùng kéo.
            GameObject neu = EnsureItem(root.transform, "CayNeu", cayNeu, new Vector3(gate.x + 2.5f, gate.y + 2f, 0f));
            GameObject doiTren = EnsureItem(root.transform, "CauDoiTren", cauDoi, new Vector3(gate.x + 1.5f, gate.y + 2f, 0f));
            GameObject doiDuoi = EnsureItem(root.transform, "CauDoiDuoi", cauDoi, new Vector3(gate.x + 1.5f, gate.y - 1f, 0f));

            var so = new SerializedObject(decor);
            so.FindProperty("_game").objectReferenceValue = game;
            SerializedProperty items = so.FindProperty("_items");
            items.arraySize = 3;
            items.GetArrayElementAtIndex(0).objectReferenceValue = neu;
            items.GetArrayElementAtIndex(1).objectReferenceValue = doiTren;
            items.GetArrayElementAtIndex(2).objectReferenceValue = doiDuoi;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            EditorUtility.DisplayDialog("Trang trí Tết",
                (created ? "Đã tạo" : "Đã cập nhật") + " TetDecor gần cổng nông trại (ô cổng " + gate + ").\n" +
                "Kéo CayNeu / CauDoiTren / CauDoiDuoi tới đúng chỗ rồi Ctrl+S. Chỉ hiện từ 23 tháng Chạp tới mùng 10 Tết.", "OK");
        }

        private static GameObject EnsureItem(Transform parent, string name, Sprite sprite, Vector3 position)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                var current = existing.GetComponent<SpriteRenderer>();
                if (current != null && current.sprite == null)
                    current.sprite = sprite;
                return existing.gameObject;
            }
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Trang trí Tết");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = SortingOrders.ByCellY(Mathf.FloorToInt(position.y));
            go.SetActive(false);
            return go;
        }

        // Ô cổng của vùng nông trại; không có thì (1, 12).
        private static Vector2Int FarmGate()
        {
            foreach (PlayRegion region in Object.FindObjectsByType<PlayRegion>(FindObjectsSortMode.None))
            {
                if (region.Kind == PlayRegion.RegionKind.Farm)
                    return new Vector2Int(region.GateCell.X, region.GateCell.Y);
            }
            return new Vector2Int(1, 12);
        }

        // Ô vuông màu có viền 1 px, pivot đáy giữa, PPU 16, Point, không nén (art tạm – có thì giữ).
        private static Sprite Square(string path, int width, int height, Color32 fill)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
                return existing;
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Game/Art", "Tet_tam");

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool edge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    // Dải vàng giữa (chữ câu đối / dải vải cây nêu) để nhìn ra món nào.
                    bool stripe = !edge && x == width / 2 && y % 4 != 0;
                    texture.SetPixel(x, y, edge ? Outline : stripe ? Gold : fill);
                }
            }
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
