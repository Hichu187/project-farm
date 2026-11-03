using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using VeQue.View.CameraControl;
using VeQue.View.Map;
using VeQue.View.UI;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Tạo UI: dựng toàn bộ GUI dưới Canvas của scene đang mở và nối sẵn tham chiếu.
    // Chạy lại sẽ xóa "GameUI" cũ rồi dựng mới, nên phần chỉnh tay trên GameUI sẽ mất.
    // TODO: thay màu phẳng bằng khung, nút của UI Pack sau khi đặt Border 9-slice cho sprite.
    public static class UiBuilder
    {
        private const string RootName = "GameUI";
        private const float HudHeight = 52f;
        private const float NavHeight = 64f;
        private const float BarHeight = 48f;
        private const float SheetHeight = 440f;
        private const float RowHeight = 72f;
        private const float ButtonHeight = 44f;

        private static readonly Vector2 ReferenceResolution = new Vector2(390f, 844f);
        private static readonly Color Cream = new Color(0.96f, 0.87f, 0.70f);
        private static readonly Color CreamDark = new Color(0.90f, 0.78f, 0.58f);
        private static readonly Color Wood = new Color(0.60f, 0.40f, 0.24f);
        private static readonly Color Green = new Color(0.45f, 0.62f, 0.28f);
        private static readonly Color TextDark = new Color(0.27f, 0.16f, 0.09f);
        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.35f);

        [MenuItem("Về Quê/Tạo UI")]
        private static void Build()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            var game = Object.FindFirstObjectByType<GameBootstrap>();
            var interaction = Object.FindFirstObjectByType<FarmInteraction>();
            var touch = Object.FindFirstObjectByType<CameraTouchInput>();
            var camera = Object.FindFirstObjectByType<CameraController>();
            var view = Object.FindFirstObjectByType<FarmView>();
            if (canvas == null || game == null || interaction == null || touch == null || camera == null || view == null)
            {
                EditorUtility.DisplayDialog("Tạo UI",
                    "Scene đang mở cần có Canvas, GameBootstrap, FarmInteraction, FarmView, CameraTouchInput và CameraController.", "OK");
                return;
            }

            Transform old = canvas.transform.Find(RootName);
            if (old != null)
            {
                if (!EditorUtility.DisplayDialog("Tạo UI", "Xóa GameUI cũ rồi dựng lại? Phần chỉnh tay sẽ mất (Ctrl+Z để hoàn tác).", "Dựng lại", "Hủy"))
                    return;
                Undo.DestroyObjectImmediate(old.gameObject);
            }

            SetupScaler(canvas);

            RectTransform root = Stretch(RootName, canvas.transform);
            var ui = root.gameObject.AddComponent<GameUI>();
            RectTransform safe = Stretch("SafeArea", root);
            safe.gameObject.AddComponent<SafeArea>();

            // Dựng trước để nằm dưới HUD và các bảng.
            CropProgressPopup popup = BuildCropProgress(safe, game, interaction, view, camera);
            BuildHud(safe, out TMP_Text coins, out Button backup);
            SeedingBar bar = BuildSeedingBar(safe);
            SeedPanel seedPanel = BuildPanel<SeedPanel>(safe, "SeedPanel", "Chọn hạt", game);
            StoragePanel storagePanel = BuildPanel<StoragePanel>(safe, "StoragePanel", "Kho", game);
            ShopPanel shopPanel = BuildPanel<ShopPanel>(safe, "ShopPanel", "Cửa hàng", game);
            BuildNav(safe, out Button farmTab, out Button storageTab, out Button shopTab);
            Toast toast = BuildToast(safe);
            UnlockDialog dialog = BuildDialog(safe, game, interaction);

            Wire(seedPanel, ("_interaction", interaction));
            Wire(storagePanel, ("_emptyLabel", BuildEmptyLabel(storagePanel)));
            Wire(bar, ("_game", game), ("_interaction", interaction), ("_camera", camera), ("_seedPanel", seedPanel));
            Wire(ui,
                ("_game", game), ("_interaction", interaction), ("_touch", touch),
                ("_coins", coins), ("_backup", backup),
                ("_farmTab", farmTab), ("_storageTab", storageTab), ("_shopTab", shopTab),
                ("_toast", toast), ("_seedPanel", seedPanel), ("_storagePanel", storagePanel), ("_shopPanel", shopPanel),
                ("_unlockDialog", dialog), ("_progressPopup", popup));

            Undo.RegisterCreatedObjectUndo(root.gameObject, "Tạo UI");
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Selection.activeGameObject = root.gameObject;
            EditorUtility.DisplayDialog("Tạo UI", "Đã dựng GUI. Nhấn Ctrl+S để lưu scene.", "OK");
        }

        private static void SetupScaler(Canvas canvas)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = Undo.AddComponent<CanvasScaler>(canvas.gameObject);

            Undo.RecordObject(scaler, "Tạo UI");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static CropProgressPopup BuildCropProgress(RectTransform parent, GameBootstrap game, FarmInteraction interaction, FarmView view, CameraController camera)
        {
            RectTransform area = Stretch("CropProgress", parent);
            var popup = area.gameObject.AddComponent<CropProgressPopup>();

            RectTransform box = Node("Box", area, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(96f, 42f));
            AddImage(box, new Color(0.15f, 0.10f, 0.06f, 0.85f), false);

            RectTransform sliderNode = Stretch("Slider", box);
            Inset(sliderNode, 6f, 24f, 6f, 6f);
            var slider = sliderNode.gameObject.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            RectTransform background = Stretch("Background", sliderNode);
            AddImage(background, CreamDark, false);
            RectTransform fillArea = Stretch("FillArea", sliderNode);
            RectTransform fill = Stretch("Fill", fillArea);
            AddImage(fill, Green, false);
            slider.fillRect = fill;

            TMP_Text text = AddLabel(box, "Text", "", 15f, TextAlignmentOptions.Center, Color.white);
            Inset(text.rectTransform, 2f, 2f, 2f, 20f);

            box.gameObject.SetActive(false);
            Wire(popup, ("_game", game), ("_interaction", interaction), ("_view", view),
                ("_cameraController", camera), ("_camera", camera.GetComponent<UnityEngine.Camera>()),
                ("_area", area), ("_box", box), ("_slider", slider), ("_text", text));
            return popup;
        }

        private static void BuildHud(RectTransform parent, out TMP_Text coins, out Button backup)
        {
            RectTransform hud = TopBand("Hud", parent, 0f, HudHeight);

            RectTransform coinBox = Node("Coins", hud, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(160f, ButtonHeight));
            AddImage(coinBox, Cream, false);
            coins = AddLabel(coinBox, "Text", "Xu 0", 18f, TextAlignmentOptions.Center, TextDark);

            RectTransform backupNode = Node("Backup", hud, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(100f, ButtonHeight));
            backup = MakeButton(backupNode, "Sao lưu", 16f, Wood, Color.white, out _);
        }

        private static SeedingBar BuildSeedingBar(RectTransform parent)
        {
            RectTransform holder = Node("SeedingBar", parent, Vector2.zero, new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, NavHeight), new Vector2(0f, BarHeight));
            var bar = holder.gameObject.AddComponent<SeedingBar>();

            RectTransform box = Stretch("Box", holder);
            AddImage(box, Cream, true);
            TMP_Text text = AddLabel(box, "Text", "", 16f, TextAlignmentOptions.MidlineLeft, TextDark);
            Inset(text.rectTransform, 12f, 0f, 88f, 0f);

            RectTransform changeNode = Node("Change", box, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(72f, ButtonHeight - 4f));
            Button change = MakeButton(changeNode, "Đổi", 16f, Wood, Color.white, out _);

            box.gameObject.SetActive(false);
            Wire(bar, ("_bar", box.gameObject), ("_text", text), ("_change", change));
            return bar;
        }

        private static T BuildPanel<T>(RectTransform parent, string name, string title, GameBootstrap game) where T : CropListPanel
        {
            RectTransform root = Stretch(name, parent);
            var sheetScript = root.gameObject.AddComponent<BottomSheet>();
            var panel = root.gameObject.AddComponent<T>();

            RectTransform backdrop = Stretch("Backdrop", root);
            backdrop.offsetMin = new Vector2(-400f, -400f);
            backdrop.offsetMax = new Vector2(400f, 400f);
            AddImage(backdrop, Dim, true);
            Button backdropButton = backdrop.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;

            RectTransform sheet = Node("Sheet", root, Vector2.zero, new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, NavHeight), new Vector2(0f, SheetHeight));
            AddImage(sheet, Cream, true);

            TMP_Text titleText = AddLabel(sheet, "Title", title, 22f, TextAlignmentOptions.MidlineLeft, TextDark);
            titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleText.rectTransform.pivot = new Vector2(0.5f, 1f);
            titleText.rectTransform.anchoredPosition = Vector2.zero;
            titleText.rectTransform.sizeDelta = new Vector2(-24f, 52f);

            RectTransform closeNode = Node("Close", sheet, Vector2.one, Vector2.one, Vector2.one, new Vector2(-6f, -4f), new Vector2(ButtonHeight, ButtonHeight));
            Button close = MakeButton(closeNode, "X", 18f, Wood, Color.white, out _);

            BuildList(sheet, out RectTransform content);
            CropRow template = BuildRowTemplate(content);

            backdrop.gameObject.SetActive(false);
            sheet.gameObject.SetActive(false);

            Wire(sheetScript, ("_backdrop", backdrop.gameObject), ("_sheet", sheet), ("_backdropButton", backdropButton), ("_closeButton", close));
            Wire(panel, ("_game", game), ("_sheet", sheetScript), ("_rowTemplate", template), ("_content", content));
            return panel;
        }

        private static void BuildList(RectTransform sheet, out RectTransform content)
        {
            RectTransform scrollNode = Stretch("Scroll", sheet);
            Inset(scrollNode, 8f, 8f, 8f, 56f);
            var scroll = scrollNode.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            RectTransform viewport = Stretch("Viewport", scrollNode);
            AddImage(viewport, new Color(1f, 1f, 1f, 0f), true);
            viewport.gameObject.AddComponent<RectMask2D>();

            content = Node("Content", viewport, new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
        }

        private static CropRow BuildRowTemplate(RectTransform content)
        {
            RectTransform row = Stretch("RowTemplate", content);
            AddImage(row, CreamDark, true);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = RowHeight;
            var script = row.gameObject.AddComponent<CropRow>();

            RectTransform iconNode = Node("Icon", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(48f, 48f));
            Image icon = AddImage(iconNode, Color.white, false);
            icon.preserveAspect = true;

            TMP_Text name = AddLabel(row, "Name", "", 17f, TextAlignmentOptions.BottomLeft, TextDark);
            Inset(name.rectTransform, 62f, RowHeight * 0.5f, 158f, 2f);
            TMP_Text info = AddLabel(row, "Info", "", 13f, TextAlignmentOptions.TopLeft, TextDark);
            Inset(info.rectTransform, 62f, 2f, 158f, RowHeight * 0.5f);

            RectTransform primaryNode = Node("Primary", row, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(70f, ButtonHeight));
            Button primary = MakeButton(primaryNode, "", 14f, Green, Color.white, out TMP_Text primaryLabel);
            RectTransform secondaryNode = Node("Secondary", row, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-80f, 0f), new Vector2(70f, ButtonHeight));
            Button secondary = MakeButton(secondaryNode, "", 14f, Wood, Color.white, out TMP_Text secondaryLabel);

            Wire(script, ("_icon", icon), ("_name", name), ("_info", info),
                ("_primary", primary), ("_primaryLabel", primaryLabel),
                ("_secondary", secondary), ("_secondaryLabel", secondaryLabel));
            row.gameObject.SetActive(false);
            return script;
        }

        private static GameObject BuildEmptyLabel(StoragePanel panel)
        {
            Transform sheet = panel.transform.Find("Sheet");
            TMP_Text label = AddLabel((RectTransform)sheet, "Empty", "Kho trống. Thu hoạch để có nông sản.", 16f, TextAlignmentOptions.Center, TextDark);
            Inset(label.rectTransform, 24f, 24f, 24f, 56f);
            return label.gameObject;
        }

        private static void BuildNav(RectTransform parent, out Button farm, out Button storage, out Button shop)
        {
            RectTransform nav = Node("Nav", parent, Vector2.zero, new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, NavHeight));
            AddImage(nav, Wood, true);
            var layout = nav.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            farm = MakeTab(nav, "Nông trại");
            storage = MakeTab(nav, "Kho");
            shop = MakeTab(nav, "Cửa hàng");
        }

        private static Button MakeTab(RectTransform nav, string label)
        {
            RectTransform node = Stretch(label, nav);
            return MakeButton(node, label, 16f, Cream, TextDark, out _);
        }

        private static Toast BuildToast(RectTransform parent)
        {
            RectTransform holder = TopBand("Toast", parent, HudHeight + 8f, 44f);
            var toast = holder.gameObject.AddComponent<Toast>();

            RectTransform box = Node("Box", holder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(340f, 44f));
            AddImage(box, new Color(0.15f, 0.10f, 0.06f, 0.85f), false);
            TMP_Text text = AddLabel(box, "Text", "", 16f, TextAlignmentOptions.Center, Color.white);
            box.gameObject.SetActive(false);

            Wire(toast, ("_box", box.gameObject), ("_text", text));
            return toast;
        }

        private static UnlockDialog BuildDialog(RectTransform parent, GameBootstrap game, FarmInteraction interaction)
        {
            RectTransform holder = Stretch("UnlockDialog", parent);
            var dialog = holder.gameObject.AddComponent<UnlockDialog>();

            RectTransform dim = Stretch("Root", holder);
            dim.offsetMin = new Vector2(-400f, -400f);
            dim.offsetMax = new Vector2(400f, 400f);
            AddImage(dim, Dim, true);

            RectTransform box = Node("Box", dim, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 180f));
            AddImage(box, Cream, true);

            TMP_Text title = AddLabel(box, "Title", "Mở ô đất này?", 20f, TextAlignmentOptions.Center, TextDark);
            Inset(title.rectTransform, 12f, 112f, 12f, 12f);
            TMP_Text price = AddLabel(box, "Price", "", 18f, TextAlignmentOptions.Center, TextDark);
            Inset(price.rectTransform, 12f, 68f, 12f, 68f);

            RectTransform cancelNode = Node("Cancel", box, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(16f, 14f), new Vector2(124f, ButtonHeight));
            Button cancel = MakeButton(cancelNode, "Hủy", 16f, Wood, Color.white, out _);
            RectTransform confirmNode = Node("Confirm", box, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 14f), new Vector2(124f, ButtonHeight));
            Button confirm = MakeButton(confirmNode, "Mở", 16f, Green, Color.white, out _);

            dim.gameObject.SetActive(false);
            Wire(dialog, ("_game", game), ("_interaction", interaction), ("_root", dim.gameObject),
                ("_priceText", price), ("_confirm", confirm), ("_cancel", cancel));
            return dialog;
        }

        // ---------- Helpers ----------

        private static RectTransform Stretch(string name, Transform parent)
        {
            RectTransform rect = Create(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform TopBand(string name, Transform parent, float top, float height)
        {
            return Node(name, parent, new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, -top), new Vector2(0f, height));
        }

        private static RectTransform Node(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            RectTransform rect = Create(name, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static RectTransform Create(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        // Cách các cạnh của cha, kiểu CSS: trái, dưới, phải, trên.
        private static void Inset(RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static Image AddImage(RectTransform rect, Color color, bool raycastTarget)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static TMP_Text AddLabel(RectTransform parent, string name, string text, float size, TextAlignmentOptions alignment, Color color)
        {
            RectTransform rect = Stretch(name, parent);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        private static Button MakeButton(RectTransform node, string text, float size, Color background, Color textColor, out TMP_Text label)
        {
            Image image = AddImage(node, background, true);
            var button = node.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            label = AddLabel(node, "Label", text, size, TextAlignmentOptions.Center, textColor);
            return button;
        }

        private static void Wire(Object target, params (string Field, Object Value)[] fields)
        {
            var so = new SerializedObject(target);
            foreach ((string field, Object value) in fields)
            {
                SerializedProperty property = so.FindProperty(field);
                if (property == null)
                    throw new System.ArgumentException($"{target.GetType().Name} không có trường {field}.");
                property.objectReferenceValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
