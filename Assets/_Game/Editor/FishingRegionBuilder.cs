using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using VeQue.View.CameraControl;
using VeQue.View.Character;
using VeQue.View.Map;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Vùng câu: tạo tạm (Phần 10 giai đoạn 2).
    // Dựng vùng câu tạm bằng tile Sunnyside (cỏ ss_1_1, đất ss_3_1, nước ss_4_1) để thử di chuyển trước khi có art:
    // - Tilemap riêng dưới Grid: FishingGround (đi được), FishingWater (chặn) – không đụng Ground của nông trại,
    //   nên giới hạn camera nông trại (lấy từ Ground) không đổi.
    // - Vùng 24×26 ô ở x = -60 (xa nông trại, camera nông trại không thấy; chuyển vùng bằng màn tối).
    //   Hồ bên trái, 3 bến (đất) nhô ra hồ, lối đất từ cổng bên phải.
    // - GameObject "Regions": PlayRegion nông trại (cổng trái Map D) + vùng câu (3 FishingSpot), RegionTravel;
    //   thêm CameraFollow lên camera; gán Tilemap mới vào WalkGridSource; gán RegionTravel cho nhân vật.
    // Chạy lại sẽ xóa và dựng lại toàn bộ phần trên. Sau đó chạy Về Quê → Tạo UI (nút Đi câu / Về nhà, màn tối).
    public static class FishingRegionBuilder
    {
        private const string Title = "Vùng câu";
        private const string TileFolder = "Assets/_Game/Art/Tiles/Fishing";
        private const string RegionsName = "Regions";
        private const string GroundName = "FishingGround";
        private const string WaterName = "FishingWater";

        // Vùng câu: góc dưới-trái, kích thước (ô).
        private static readonly Vector2Int Origin = new Vector2Int(-60, 0);
        private static readonly Vector2Int Size = new Vector2Int(24, 26);
        // Hồ: cột 0..11, hàng 3..22 (tương đối với Origin).
        private static readonly RectInt Lake = new RectInt(0, 3, 12, 20);
        // Hàng của 3 bến; bến dài 4 ô từ mép phải hồ vào trong.
        private static readonly int[] DockRows = { 7, 12, 17 };
        private const int DockLength = 4;
        // Cổng vùng câu: mép phải, hàng 12–13 (lối đất từ đây vào hồ).
        private static readonly Vector2Int FishingGate = new Vector2Int(23, 12);
        // Cổng trái Map D: ô (0, 8) cao 2 ô (MVP-Plan Phần 3: [0, 16, 1, 2] → originY = 26 − 16 − 2).
        private static readonly Vector2Int FarmGate = new Vector2Int(0, 8);

        [MenuItem("Về Quê/Vùng câu: tạo tạm")]
        private static void Build()
        {
            var walk = Object.FindFirstObjectByType<WalkGridSource>();
            var character = Object.FindFirstObjectByType<FarmCharacterController>();
            var camera = Object.FindFirstObjectByType<CameraController>();
            if (walk == null || character == null || camera == null)
            {
                EditorUtility.DisplayDialog(Title, "Scene cần có WalkGridSource, FarmCharacterController và CameraController.", "OK");
                return;
            }

            var walkSo = new SerializedObject(walk);
            var ground = walkSo.FindProperty("_ground").objectReferenceValue as Tilemap;
            if (ground == null || ground.layoutGrid == null)
            {
                EditorUtility.DisplayDialog(Title, "WalkGridSource chưa gán Tilemap Ground.", "OK");
                return;
            }

            Dictionary<string, Sprite> sprites = LoadTilesetSprites();
            if (!sprites.TryGetValue("ss_1_1", out Sprite grassSprite)
                || !sprites.TryGetValue("ss_3_1", out Sprite dirtSprite)
                || !sprites.TryGetValue("ss_4_1", out Sprite waterSprite))
            {
                EditorUtility.DisplayDialog(Title,
                    "Chưa có sprite tileset Sunnyside (ss_1_1, ss_3_1, ss_4_1). Chạy Về Quê → Sunnyside: import tileset 16px trước.", "OK");
                return;
            }

            TileBase grass = EnsureTile("fishing_grass", grassSprite);
            TileBase dirt = EnsureTile("fishing_dirt", dirtSprite);
            TileBase water = EnsureTile("fishing_water", waterSprite);

            Transform gridTransform = ground.layoutGrid.transform;
            DestroyChild(gridTransform, GroundName);
            DestroyChild(gridTransform, WaterName);
            Tilemap fishingGround = CreateTilemap(gridTransform, GroundName, ground, 0);
            Tilemap fishingWater = CreateTilemap(gridTransform, WaterName, ground, 1);

            PaintRegion(fishingGround, fishingWater, grass, dirt, water);

            // Regions + chỗ câu.
            GameObject old = GameObject.Find(RegionsName);
            if (old != null)
                Undo.DestroyObjectImmediate(old);
            var regionsGo = new GameObject(RegionsName);
            Undo.RegisterCreatedObjectUndo(regionsGo, Title);

            PlayRegion farm = CreateRegion(regionsGo.transform, "FarmRegion", PlayRegion.RegionKind.Farm, "Nông trại",
                ground.cellBounds.min, (Vector2Int)ground.cellBounds.size, FarmGate);
            PlayRegion fishing = CreateRegion(regionsGo.transform, "FishingRegion", PlayRegion.RegionKind.Fishing, "Bến sông",
                (Vector3Int)Origin, Size, Origin + FishingGate);

            Sprite rod = AssetDatabase.LoadAssetAtPath<Sprite>(SunnysideUiImporter.IconPath("rod"));
            var spots = new List<FishingSpot>();
            for (int i = 0; i < DockRows.Length; i++)
                spots.Add(CreateSpot(fishing.transform, i, DockRows[i], rod));
            Wire(fishing, "_spots", spots.ToArray());

            var travel = regionsGo.AddComponent<RegionTravel>();
            CameraFollow follow = camera.GetComponent<CameraFollow>();
            if (follow == null)
                follow = Undo.AddComponent<CameraFollow>(camera.gameObject);
            var followSo = new SerializedObject(follow);
            followSo.FindProperty("_controller").objectReferenceValue = camera;
            followSo.ApplyModifiedProperties();

            var travelSo = new SerializedObject(travel);
            travelSo.FindProperty("_game").objectReferenceValue = Object.FindFirstObjectByType<GameBootstrap>();
            travelSo.FindProperty("_character").objectReferenceValue = character;
            travelSo.FindProperty("_camera").objectReferenceValue = camera;
            travelSo.FindProperty("_follow").objectReferenceValue = follow;
            travelSo.FindProperty("_farm").objectReferenceValue = farm;
            travelSo.FindProperty("_fishing").objectReferenceValue = fishing;
            travelSo.ApplyModifiedPropertiesWithoutUndo();

            SetArray(walkSo.FindProperty("_extraGround"), new Object[] { fishingGround });
            SetArray(walkSo.FindProperty("_extraBlockers"), new Object[] { fishingWater });
            walkSo.ApplyModifiedProperties();

            var characterSo = new SerializedObject(character);
            characterSo.FindProperty("_travel").objectReferenceValue = travel;
            characterSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(regionsGo.scene);
            string note = rod == null ? "\nChưa có icon cần câu: chạy Về Quê → UI Sunnyside: ghép 9-slice rồi chạy lại menu này." : "";
            EditorUtility.DisplayDialog(Title,
                $"Đã dựng vùng câu tạm tại x = {Origin.x} ({Size.x}×{Size.y} ô), 3 chỗ câu.{note}\n"
                + "Tiếp theo: Về Quê → Tạo UI (nút Đi câu / Về nhà, màn tối), rồi Ctrl+S.\n"
                + "Cổng nông trại đặt ở ô (0, 8); nếu cổng trên map khác chỗ, sửa Gate Cell của FarmRegion.", "OK");
            Selection.activeGameObject = regionsGo;
        }

        private static void PaintRegion(Tilemap ground, Tilemap water, TileBase grass, TileBase dirt, TileBase waterTile)
        {
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    var cell = new Vector3Int(Origin.x + x, Origin.y + y, 0);
                    bool inLake = Lake.Contains(new Vector2Int(x, y));
                    ground.SetTile(cell, inLake ? null : grass);
                    water.SetTile(cell, inLake ? waterTile : null);
                }
            }

            // Bến: đất nhô vào hồ (đi được), bỏ nước ở đó.
            foreach (int row in DockRows)
            {
                for (int i = 0; i < DockLength; i++)
                {
                    var cell = new Vector3Int(Origin.x + Lake.xMax - 1 - i, Origin.y + row, 0);
                    water.SetTile(cell, null);
                    ground.SetTile(cell, dirt);
                }
            }

            // Lối đất từ cổng tới mép hồ (2 hàng).
            for (int x = Lake.xMax; x < Size.x; x++)
            {
                for (int y = FishingGate.y; y < FishingGate.y + 2; y++)
                    ground.SetTile(new Vector3Int(Origin.x + x, Origin.y + y, 0), dirt);
            }
        }

        private static PlayRegion CreateRegion(Transform parent, string name, PlayRegion.RegionKind kind, string displayName,
            Vector3Int origin, Vector2Int size, Vector2Int gate)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(origin.x, origin.y, 0f);
            var region = go.AddComponent<PlayRegion>();
            var so = new SerializedObject(region);
            so.FindProperty("_kind").enumValueIndex = (int)kind;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_sizeInCells").vector2IntValue = size;
            so.FindProperty("_gateCell").vector2IntValue = gate;
            so.FindProperty("_gateSize").vector2IntValue = new Vector2Int(1, 2);
            so.ApplyModifiedPropertiesWithoutUndo();
            return region;
        }

        // Chỗ câu ở đầu bến (ô ngoài cùng trong hồ), quay mặt sang trái ra nước; vùng chạm gồm đầu bến + 2 ô nước.
        private static FishingSpot CreateSpot(Transform parent, int index, int row, Sprite icon)
        {
            int standX = Origin.x + Lake.xMax - DockLength;
            int standY = Origin.y + row;
            var go = new GameObject($"FishingSpot{index + 1}");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(standX - 2, standY - 1, 0f);
            var spot = go.AddComponent<FishingSpot>();

            SpriteRenderer iconRenderer = null;
            if (icon != null)
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(go.transform, false);
                // Icon trên ô nước ngay trước đầu bến.
                iconGo.transform.position = new Vector3(standX - 0.5f, standY + 1.5f, 0f);
                iconRenderer = iconGo.AddComponent<SpriteRenderer>();
                iconRenderer.sprite = icon;
                iconRenderer.sortingOrder = 300;
            }

            var so = new SerializedObject(spot);
            so.FindProperty("_tapSize").vector2IntValue = new Vector2Int(3, 3);
            so.FindProperty("_standCell").vector2IntValue = new Vector2Int(standX, standY);
            so.FindProperty("_facing").enumValueIndex = (int)Facing.Left;
            so.FindProperty("_icon").objectReferenceValue = iconRenderer;
            so.ApplyModifiedPropertiesWithoutUndo();
            return spot;
        }

        private static Tilemap CreateTilemap(Transform grid, string name, Tilemap like, int sortingOffset)
        {
            var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            Undo.RegisterCreatedObjectUndo(go, Title);
            go.transform.SetParent(grid, false);
            var likeRenderer = like.GetComponent<TilemapRenderer>();
            var renderer = go.GetComponent<TilemapRenderer>();
            if (likeRenderer != null)
            {
                renderer.sortingLayerID = likeRenderer.sortingLayerID;
                renderer.sortingOrder = likeRenderer.sortingOrder + sortingOffset;
                renderer.sharedMaterial = likeRenderer.sharedMaterial;
            }
            return go.GetComponent<Tilemap>();
        }

        private static void DestroyChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
                Undo.DestroyObjectImmediate(child.gameObject);
        }

        private static Dictionary<string, Sprite> LoadTilesetSprites()
        {
            var result = new Dictionary<string, Sprite>();
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(SunnysideTilesetImporter.TilesetPath))
            {
                if (asset is Sprite sprite)
                    result[sprite.name] = sprite;
            }
            return result;
        }

        // Tile asset dùng lại nếu đã có (giữ GUID), chỉ gán lại sprite.
        private static TileBase EnsureTile(string name, Sprite sprite)
        {
            System.IO.Directory.CreateDirectory(TileFolder);
            string path = $"{TileFolder}/{name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            AssetDatabase.SaveAssets();
            return tile;
        }

        private static void Wire(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            SetArray(so.FindProperty(field), values);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
