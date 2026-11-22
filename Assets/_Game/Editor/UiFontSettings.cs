using TMPro;
using UnityEditor;
using UnityEngine;

namespace VeQue.EditorTools
{
    // Font cho lệnh Về Quê → Tạo UI (và bảng xem thử cá shiny). Chỉ dùng trong Editor, game không tham chiếu nên asset không vào build.
    // Đổi font: chọn asset (menu Về Quê → Font UI), kéo font mới vào Font, rồi chạy Tạo UI lại.
    public sealed class UiFontSettings : ScriptableObject
    {
        public const string AssetPath = "Assets/_Game/Data/UiFontSettings.asset";

        [Tooltip("Để trống = font mặc định của TMP (LiberationSans SDF).")]
        [SerializeField] private TMP_FontAsset _font;

        [Tooltip("Material viền cho chữ trắng trên nền màu (nút, thanh XP); phải cùng atlas với Font. Để trống = không viền.")]
        [SerializeField] private Material _outlineMaterial;

        [Tooltip("Bật cho font pixel: cỡ chữ cố định theo bội số pixel (2 bậc), tắt tự co chữ để nét không bị mờ.")]
        [SerializeField] private bool _pixelFont;

        [Tooltip("Chỉ dùng khi Pixel Font bật: Point Size của font asset (số pixel thiết kế × hệ số khi tạo font).")]
        [SerializeField, Min(1)] private int _pixelPointSize = PixelFontBuilder.PointSize;

        public TMP_FontAsset Font => _font;
        public Material OutlineMaterial => _font != null ? _outlineMaterial : null;
        public bool PixelFont => _font != null && _pixelFont;
        public int PixelPointSize => _pixelPointSize;

        // Font thực dùng: chưa gán thì lấy font mặc định của TMP.
        public TMP_FontAsset FontOrDefault => _font != null ? _font : TMP_Settings.defaultFontAsset;

        public static UiFontSettings LoadOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<UiFontSettings>(AssetPath);
            if (settings != null)
                return settings;
            settings = CreateInstance<UiFontSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Đã tạo {AssetPath} (font mặc định của TMP).");
            return settings;
        }

        [MenuItem("Về Quê/Font UI/Mở cài đặt", priority = 306)]
        private static void Select()
        {
            UiFontSettings settings = LoadOrCreate();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        // Đổi font cho mọi chữ TMP trong scene đang mở (kể cả object ẩn); giữ nguyên bố cục và cỡ chữ.
        // Chữ đang dùng material viền (khác material gốc của font) → Outline Material mới; trống thì về material gốc.
        [MenuItem("Về Quê/Font UI/Áp vào scene đang mở", priority = 307)]
        private static void ApplyToScene()
        {
            UiFontSettings settings = LoadOrCreate();
            TMP_FontAsset font = settings.FontOrDefault;
            if (font == null)
            {
                EditorUtility.DisplayDialog("Font UI", "Không tìm thấy font mặc định của TMP (TMP Settings).", "OK");
                return;
            }

            TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int changed = 0, outlined = 0;
            foreach (TMP_Text text in texts)
            {
                if (EditorUtility.IsPersistent(text))
                    continue;
                Material oldDefault = text.font != null ? text.font.material : null;
                bool wasOutlined = text.fontSharedMaterial != null && text.fontSharedMaterial != oldDefault;
                Material material = wasOutlined && settings.OutlineMaterial != null ? settings.OutlineMaterial : font.material;
                if (text.font == font && text.fontSharedMaterial == material)
                    continue;

                Undo.RecordObject(text, "Đổi font UI");
                text.font = font;
                text.fontSharedMaterial = material;
                EditorUtility.SetDirty(text);
                changed++;
                if (wasOutlined)
                    outlined++;
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            EditorUtility.DisplayDialog("Font UI",
                $"Đã đổi {changed}/{texts.Length} chữ sang {font.name} ({outlined} chữ từng có viền).\nKiểm tra rồi Ctrl+S; Ctrl+Z để hoàn tác.", "OK");
        }
    }
}
