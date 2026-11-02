using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using VeQue.View.Data;

namespace VeQue.EditorTools
{
    // Cửa sổ Về Quê → Tạo map: nhập kích thước, gán tile rồi vẽ từng lớp Tilemap trong scene đang mở.
    public sealed class MapPaintWindow : EditorWindow
    {
        private const string SettingsPath = DefaultDataGenerator.DataFolder + "/MapPaintSettings.asset";

        [SerializeField] private Tilemap _ground;
        [SerializeField] private Tilemap _decor;
        [SerializeField] private Tilemap _plots;
        [SerializeField] private bool _showGround = true;
        [SerializeField] private bool _showDecor = true;
        [SerializeField] private bool _showPlots = true;
        [SerializeField] private int _addWeight = 1;

        private MapPaintSettings _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scroll;
        private Action _pendingAction;

        [MenuItem("Về Quê/Tạo map")]
        private static void Open()
        {
            GetWindow<MapPaintWindow>("Tạo map");
        }

        private void OnEnable()
        {
            _settings = LoadOrCreateSettings();
            _serializedSettings = new SerializedObject(_settings);
            FindLayers();
        }

        private void OnGUI()
        {
            _serializedSettings.Update();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawProperties("_mapSize");
            if (GUILayout.Button("Tìm lại Tilemap theo tên"))
                FindLayers();

            EditorGUILayout.Space();
            DrawGround();
            EditorGUILayout.Space();
            DrawDecor();
            EditorGUILayout.Space();
            DrawPlots();

            EditorGUILayout.EndScrollView();

            // Chạy sau khi đóng layout vì hộp thoại xác nhận chặn giữa chừng sẽ làm lệch layout của OnGUI.
            if (_pendingAction != null)
            {
                Action action = _pendingAction;
                _pendingAction = null;
                action();
                GUIUtility.ExitGUI();
            }
        }

        private void DrawGround()
        {
            _showGround = EditorGUILayout.Foldout(_showGround, "Ground (cỏ + sân đất)", true);
            if (!_showGround)
                return;

            _ground = (Tilemap)EditorGUILayout.ObjectField("Tilemap", _ground, typeof(Tilemap), true);
            DrawGrassPicker();
            DrawProperties("_grassTiles", "_grassSeed", "_paintYard", "_yardRect", "_yardTiles");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Vẽ Ground"))
                _pendingAction = () => Run(MapPainter.ValidateGround(_settings, _ground),
                    "Xóa sạch lớp Ground rồi vẽ lại cỏ và sân đất. Phần vẽ tay trên lớp này sẽ mất (Ctrl+Z để hoàn tác).",
                    "Vẽ", () => MapPainter.PaintGround(_settings, _ground));
            if (GUILayout.Button("Xóa Ground"))
                _pendingAction = () => RunClear(_ground, "Ground");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrassPicker()
        {
            EditorGUILayout.HelpBox("Chọn nhiều tile (Shift/Ctrl + click) hoặc cả thư mục trong Project, rồi bấm Thêm. Tile đã có trong danh sách chỉ được cập nhật Weight.", MessageType.None);
            _addWeight = Mathf.Max(1, EditorGUILayout.IntField("Weight khi thêm", _addWeight));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Thêm tile đang chọn"))
                _pendingAction = AddSelectedGrass;
            if (GUILayout.Button("Xóa danh sách cỏ"))
                _pendingAction = () => Run(null, "Xóa toàn bộ danh sách tile cỏ? Tile đã vẽ trên map không bị ảnh hưởng.",
                    "Xóa", ClearGrass);
            EditorGUILayout.EndHorizontal();
        }

        private void AddSelectedGrass()
        {
            TileBase[] selected = Selection.GetFiltered<TileBase>(SelectionMode.DeepAssets);
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Tạo map", "Chưa chọn tile hoặc thư mục có tile trong Project.", "OK");
                return;
            }

            _serializedSettings.Update();
            SerializedProperty list = _serializedSettings.FindProperty("_grassTiles");
            foreach (TileBase tile in selected)
            {
                int index = FindGrass(list, tile);
                if (index < 0)
                {
                    index = list.arraySize;
                    list.arraySize++;
                }

                SerializedProperty entry = list.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("Tile").objectReferenceValue = tile;
                entry.FindPropertyRelative("Weight").intValue = _addWeight;
            }
            _serializedSettings.ApplyModifiedProperties();
        }

        private void ClearGrass()
        {
            _serializedSettings.Update();
            _serializedSettings.FindProperty("_grassTiles").arraySize = 0;
            _serializedSettings.ApplyModifiedProperties();
        }

        private static int FindGrass(SerializedProperty list, TileBase tile)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("Tile").objectReferenceValue == tile)
                    return i;
            }
            return -1;
        }

        private void DrawDecor()
        {
            _showDecor = EditorGUILayout.Foldout(_showDecor, "Decor (hàng rào)", true);
            if (!_showDecor)
                return;

            _decor = (Tilemap)EditorGUILayout.ObjectField("Tilemap", _decor, typeof(Tilemap), true);
            DrawProperties("_fenceTiles", "_fenceOffset", "_gateEdge", "_gateStart", "_gateSize");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Vẽ hàng rào"))
                _pendingAction = () => Run(MapPainter.ValidateFence(_settings, _decor),
                    "Xóa các ô đang dùng Fence Tiles trên Decor rồi vẽ lại hàng rào theo offset. Phần vẽ tay bằng tile khác được giữ, trừ ô trùng vị trí hàng rào mới (Ctrl+Z để hoàn tác).",
                    "Vẽ", () => MapPainter.PaintFence(_settings, _decor));
            if (GUILayout.Button("Xóa Decor"))
                _pendingAction = () => RunClear(_decor, "Decor");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPlots()
        {
            _showPlots = EditorGUILayout.Foldout(_showPlots, "Plots (ô đất)", true);
            if (!_showPlots)
                return;

            _plots = (Tilemap)EditorGUILayout.ObjectField("Tilemap", _plots, typeof(Tilemap), true);
            DrawProperties("_farm", "_plotTile");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Vẽ Plots"))
                _pendingAction = () => Run(MapPainter.ValidatePlots(_settings, _plots),
                    "Xóa sạch lớp Plots rồi vẽ lại ô đất theo Beds. Phần vẽ tay trên lớp này sẽ mất (Ctrl+Z để hoàn tác).",
                    "Vẽ", () => MapPainter.PaintPlots(_settings, _plots));
            if (GUILayout.Button("Xóa Plots"))
                _pendingAction = () => RunClear(_plots, "Plots");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProperties(params string[] names)
        {
            foreach (string propertyName in names)
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty(propertyName), true);
            _serializedSettings.ApplyModifiedProperties();
        }

        private void RunClear(Tilemap tilemap, string layerName)
        {
            Run(MapPainter.ValidateLayer(tilemap, layerName),
                $"Xóa toàn bộ tile trên lớp {layerName}? (Ctrl+Z để hoàn tác)",
                "Xóa", () => MapPainter.ClearLayer(tilemap));
        }

        private static void Run(string error, string confirmMessage, string confirmButton, Action action)
        {
            if (error != null)
            {
                EditorUtility.DisplayDialog("Tạo map", error, "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Tạo map", confirmMessage, confirmButton, "Hủy"))
                action();
        }

        private void FindLayers()
        {
            if (_ground == null)
                _ground = FindTilemap("Ground");
            if (_decor == null)
                _decor = FindTilemap("Decor");
            if (_plots == null)
                _plots = FindTilemap("Plots");
        }

        private static Tilemap FindTilemap(string objectName)
        {
            GameObject found = GameObject.Find(objectName);
            return found != null ? found.GetComponent<Tilemap>() : null;
        }

        private static MapPaintSettings LoadOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MapPaintSettings>(SettingsPath);
            if (settings != null)
                return settings;

            settings = CreateInstance<MapPaintSettings>();
            settings.Farm = AssetDatabase.LoadAssetAtPath<FarmDefinition>(DefaultDataGenerator.FarmPath);
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
