using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using VeQue.View.CameraControl;
using VeQue.View.Fishing;
using VeQue.View.Map;
using VeQue.View.UI;

namespace VeQue.EditorTools
{
    // Menu Về Quê → Tạo UI: dựng toàn bộ GUI dưới Canvas của scene đang mở và nối sẵn tham chiếu.
    // Chạy lại sẽ xóa "GameUI" cũ rồi dựng mới, nên phần chỉnh tay trên GameUI sẽ mất.
    // Khung, nút, tab dùng sprite Sunnyside đã ghép (menu Về Quê → UI Sunnyside: ghép 9-slice); chưa ghép thì dùng màu phẳng.
    public static class UiBuilder
    {
        private const string RootName = "GameUI";
        private const float HudHeight = 52f;
        private const float NavHeight = 64f;
        private const float SheetHeight = 440f;
        private const float RowHeight = 72f;
        private const float ButtonHeight = 44f;

        private static readonly Vector2 ReferenceResolution = new Vector2(390f, 844f);
        private static readonly Color Cream = new Color(0.96f, 0.87f, 0.70f);
        private static readonly Color CreamDark = new Color(0.90f, 0.78f, 0.58f);
        private static readonly Color Wood = new Color(0.60f, 0.40f, 0.24f);
        private static readonly Color Green = new Color(0.45f, 0.62f, 0.28f);
        private static readonly Color TextDark = new Color(0.27f, 0.16f, 0.09f);
        private static readonly Color UrgentText = new Color(0.70f, 0.15f, 0.10f);
        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.35f);
        // Nhuộm thẻ đơn khẩn cấp (khung giấy nhân màu này thành hồng cam nhạt).
        private static readonly Color UrgentTint = new Color(1f, 0.80f, 0.70f);

        // 1 pixel art = bấy nhiêu đơn vị UI (Canvas 390 ngang; màn 1170 px → 1 đơn vị = 3 px màn hình).
        private const float UiPixelScale = 2f;

        // Sắc độ khung (SunnysideUiImporter), từ sáng tới đậm: Paper → Cream → Sand → Tan → Brown → Dark.
        // Nút: Green, Brown, Red, Blue. Nhãn bo hai đầu: LabelPaper, LabelCream, LabelTan.
        private enum Skin
        {
            Paper,
            Cream,
            Sand,
            Tan,
            Brown,
            Dark,
            Gray,
            Green,
            Red,
            Blue,
            LabelPaper,
            LabelCream,
            LabelTan,
        }

        private enum Icon
        {
            None,
            Confirm,
            Cancel,
            Sandtimer,
            Stopwatch,
        }

        private static readonly System.Collections.Generic.Dictionary<Icon, Sprite> Icons =
            new System.Collections.Generic.Dictionary<Icon, Sprite>();
        private static Sprite _bar;
        // Font pixel tiếng Việt (menu Về Quê → Font: tạo font pixel tiếng Việt); null = font mặc định của TMP.
        private static TMP_FontAsset _pixelFont;
        // Material viền pixel cho chữ trắng trên nền màu (nút, thanh XP); null = không viền.
        private static Material _outlineMaterial;

        // Sprite đã ghép theo Skin; thiếu thì dựng bằng màu phẳng.
        private static readonly System.Collections.Generic.Dictionary<Skin, Sprite> Skins =
            new System.Collections.Generic.Dictionary<Skin, Sprite>();

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
            LoadSkin();
            _pixelFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PixelFontBuilder.AssetPath);
            _outlineMaterial = _pixelFont != null ? AssetDatabase.LoadAssetAtPath<Material>(PixelFontBuilder.OutlineMaterialPath) : null;
            if (_pixelFont == null)
                Debug.LogWarning($"Tạo UI: chưa có {PixelFontBuilder.AssetPath}, dùng font mặc định. Chạy Về Quê → Font: tạo font pixel tiếng Việt rồi Tạo UI lại.");

            RectTransform root = Stretch(RootName, canvas.transform);
            var ui = root.gameObject.AddComponent<GameUI>();
            RectTransform safe = Stretch("SafeArea", root);
            safe.gameObject.AddComponent<SafeArea>();

            // Dựng trước để nằm dưới HUD và các bảng.
            FarmGesture gesture = EnsureGesture(game, interaction, view, touch, camera);
            CropProgressPopup popup = BuildCropProgress(safe, game, interaction, view, camera);
            SeedPicker seedPicker = BuildSeedPicker(safe, game, interaction, gesture, view, camera);
            BuildHud(safe, game, out TMP_Text coins, out Button backup);
            // Trước các bảng để bảng mở ra che nút.
            BuildTaskStopButton(safe, game);
            OrderPanel orderPanel = BuildOrderPanel(safe, game);
            StoragePanel storagePanel = BuildPanel<StoragePanel>(safe, "StoragePanel", "Kho", game);
            ShopPanel shopPanel = BuildPanel<ShopPanel>(safe, "ShopPanel", "Cửa hàng", game);
            BuildNav(safe, out Button farmTab, out Button ordersTab, out TMP_Text ordersTabLabel, out Button storageTab, out Button shopTab);
            BuildHarvestFlyer(safe, game, interaction, view, camera.GetComponent<Camera>(), (RectTransform)storageTab.transform);
            Toast toast = BuildToast(safe);
            UnlockDialog dialog = BuildDialog(safe, game, interaction);
            BuildLevelUpDialog(safe, game);
            // Sau cùng để nằm trên mọi thứ (lần đầu vào game che cả HUD).
            Button characterButton = BuildCharacterButton(safe);
            // Vùng chơi: Đi câu dưới nút Nhân vật; ở vùng câu: Về nhà ở chỗ nút Nhân vật + tên vùng.
            Button goFishing = BuildRegionButton(safe, "GoFishing", "Đi câu", 1);
            Button goHome = BuildRegionButton(safe, "GoHome", "Về nhà", 0);
            TMP_Text regionLabel = BuildRegionLabel(safe, out GameObject regionPanel);
            BuildFishing(safe, game, toast);
            CharacterCreator creator = BuildCharacterCreator(safe, game, characterButton);
            // Sau cùng: màn tối chuyển cảnh phủ lên mọi thứ.
            ScreenFader fader = BuildFader(safe);
            var travel = Object.FindFirstObjectByType<RegionTravel>();
            if (travel != null)
                Wire(travel, ("_fader", fader));

            Wire(storagePanel, ("_emptyLabel", BuildEmptyLabel(storagePanel)));
            Wire(ui,
                ("_game", game), ("_interaction", interaction), ("_touch", touch),
                ("_coins", coins), ("_backup", backup),
                ("_farmTab", farmTab), ("_ordersTab", ordersTab), ("_ordersTabLabel", ordersTabLabel),
                ("_storageTab", storageTab), ("_shopTab", shopTab),
                ("_toast", toast), ("_seedPicker", seedPicker), ("_gesture", gesture), ("_orderPanel", orderPanel),
                ("_storagePanel", storagePanel), ("_shopPanel", shopPanel),
                ("_unlockDialog", dialog), ("_progressPopup", popup), ("_characterCreator", creator),
                ("_travel", travel), ("_goFishing", goFishing), ("_goHome", goHome), ("_regionLabel", regionLabel), ("_regionPanel", regionPanel));
            var uiSo = new SerializedObject(ui);
            SetArray(uiSo.FindProperty("_farmOnly"), new Object[]
            {
                farmTab.transform.parent.gameObject, characterButton.gameObject, goFishing.gameObject,
            });
            uiSo.ApplyModifiedPropertiesWithoutUndo();

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

            // Đặt khung và chữ đúng ô pixel màn hình (khung 9-slice, font pixel).
            Undo.RecordObject(canvas, "Tạo UI");
            canvas.pixelPerfect = true;
        }

        private static CropProgressPopup BuildCropProgress(RectTransform parent, GameBootstrap game, FarmInteraction interaction, FarmView view, CameraController camera)
        {
            RectTransform area = Stretch("CropProgress", parent);
            var popup = area.gameObject.AddComponent<CropProgressPopup>();

            RectTransform box = Node("Box", area, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(112f, 50f));
            AddImage(box, new Color(0.15f, 0.10f, 0.06f, 0.85f), false);

            RectTransform sliderNode = Node("Slider", box, new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f),
                new Vector2(0f, -6f), new Vector2(-12f, 0f));
            var slider = sliderNode.gameObject.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            RectTransform background = Stretch("Background", sliderNode);
            RectTransform fillArea = BuildBar(background, out float barHeight);
            sliderNode.sizeDelta = new Vector2(-12f, barHeight);
            RectTransform fill = Stretch("Fill", fillArea);
            AddImage(fill, BarFillColor, false);
            slider.fillRect = fill;

            // Đồng hồ bấm giờ bên trái chữ thời gian còn lại.
            TMP_Text text = AddLabel(box, "Text", "", 15f, TextAlignmentOptions.Center, Color.white);
            Inset(text.rectTransform, 2f, 2f, 2f, 10f + barHeight);
            Image clock = AddIcon(box, Icon.Stopwatch, 0f, 6f);
            if (clock != null)
            {
                RectTransform clockRect = clock.rectTransform;
                clockRect.anchorMin = clockRect.anchorMax = clockRect.pivot = new Vector2(0f, 0f);
                clockRect.anchoredPosition = new Vector2(6f, 2f);
                text.rectTransform.offsetMin = new Vector2(8f + clockRect.sizeDelta.x, 2f);
            }

            box.gameObject.SetActive(false);
            Wire(popup, ("_game", game), ("_interaction", interaction), ("_view", view),
                ("_cameraController", camera), ("_camera", camera.GetComponent<UnityEngine.Camera>()),
                ("_area", area), ("_box", box), ("_slider", slider), ("_text", text));
            return popup;
        }

        // FarmGesture nằm cùng GameObject với FarmInteraction; chưa có thì thêm và nối tham chiếu.
        private static FarmGesture EnsureGesture(GameBootstrap game, FarmInteraction interaction, FarmView view,
            CameraTouchInput touch, CameraController camera)
        {
            var gesture = Object.FindFirstObjectByType<FarmGesture>();
            if (gesture == null)
                gesture = Undo.AddComponent<FarmGesture>(interaction.gameObject);
            Undo.RecordObject(gesture, "Tạo UI");
            Wire(gesture, ("_game", game), ("_interaction", interaction), ("_view", view), ("_touch", touch),
                ("_camera", camera.GetComponent<UnityEngine.Camera>()));
            return gesture;
        }

        private const float SeedItemWidth = 54f;
        private const float SeedItemHeight = 62f;

        // Thanh nhỏ bám trên ô (pivot giữa cạnh dưới), tự co theo số icon; icon hạt đi theo ngón tay khi kéo.
        private static SeedPicker BuildSeedPicker(RectTransform parent, GameBootstrap game, FarmInteraction interaction,
            FarmGesture gesture, FarmView view, CameraController camera)
        {
            RectTransform area = Stretch("SeedPicker", parent);
            var picker = area.gameObject.AddComponent<SeedPicker>();

            RectTransform box = Node("Box", area, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(SeedItemWidth, SeedItemHeight + 8f));
            AddImage(box, new Color(0.15f, 0.10f, 0.06f, 0.85f), true);
            CanvasGroup boxGroup = box.gameObject.AddComponent<CanvasGroup>();
            var layout = box.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.spacing = 4f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            var fitter = box.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RectTransform item = Create("ItemTemplate", box);
            item.sizeDelta = new Vector2(SeedItemWidth, SeedItemHeight);
            Frame(item, Skin.Paper, Cream, true);
            CanvasGroup itemGroup = item.gameObject.AddComponent<CanvasGroup>();
            var itemScript = item.gameObject.AddComponent<SeedPickerItem>();
            RectTransform iconNode = Node("Icon", item, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(36f, 36f));
            Image icon = AddImage(iconNode, Color.white, false);
            icon.preserveAspect = true;
            TMP_Text count = AddLabel(item, "Count", "x0", 14f, TextAlignmentOptions.Bottom, TextDark);
            Inset(count.rectTransform, 2f, 3f, 2f, 40f);
            Wire(itemScript, ("_icon", icon), ("_count", count), ("_group", itemGroup));
            item.gameObject.SetActive(false);

            RectTransform dragNode = Node("DragIcon", area, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44f, 44f));
            Image dragIcon = AddImage(dragNode, Color.white, false);
            dragIcon.preserveAspect = true;
            dragNode.gameObject.SetActive(false);

            box.gameObject.SetActive(false);
            Wire(picker, ("_game", game), ("_interaction", interaction), ("_gesture", gesture), ("_view", view),
                ("_cameraController", camera), ("_camera", camera.GetComponent<UnityEngine.Camera>()),
                ("_area", area), ("_box", box), ("_boxGroup", boxGroup), ("_content", box),
                ("_itemTemplate", itemScript), ("_dragIcon", dragIcon));
            return picker;
        }

        private static void BuildHud(RectTransform parent, GameBootstrap game, out TMP_Text coins, out Button backup)
        {
            RectTransform hud = TopBand("Hud", parent, 0f, HudHeight);

            RectTransform coinBox = Node("Coins", hud, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(130f, ButtonHeight));
            Frame(coinBox, Skin.LabelCream, Cream, false);
            coins = AddLabel(coinBox, "Text", "Xu 0", 18f, TextAlignmentOptions.Center, TextDark);

            // Cấp + thanh XP giữa xu và nút sao lưu.
            RectTransform levelBox = Node("Level", hud, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(150f, 0f), new Vector2(130f, ButtonHeight));
            Frame(levelBox, Skin.LabelCream, Cream, false);
            var levelHud = levelBox.gameObject.AddComponent<LevelHud>();
            RectTransform barBack = Node("Bar", levelBox, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 6f), new Vector2(-16f, 0f));
            RectTransform track = BuildBar(barBack, out float barHeight);
            barBack.sizeDelta = new Vector2(-16f, barHeight);
            TMP_Text level = AddLabel(levelBox, "Text", "Cấp 1", 15f, TextAlignmentOptions.Center, TextDark);
            Inset(level.rectTransform, 4f, 6f + barHeight, 4f, 2f);
            RectTransform fill = Stretch("Fill", track);
            fill.anchorMax = new Vector2(0f, 1f);
            AddImage(fill, BarFillColor, false);
            // Chữ XP nằm trên rãnh tối / phần xanh nên dùng màu trắng.
            TMP_Text xp = AddLabel(barBack, "Xp", "0/3", 10f, TextAlignmentOptions.Center, _bar != null ? Color.white : TextDark);
            if (_bar != null)
                Outlined(xp);
            Wire(levelHud, ("_game", game), ("_level", level), ("_fill", fill), ("_xp", xp));

            RectTransform backupNode = Node("Backup", hud, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(90f, ButtonHeight));
            backup = MakeButton(backupNode, "Sao lưu", 16f, Wood, Color.white, out _);
        }

        // Nút "Dừng (N)" góc dưới-phải, trên thanh tab; chỉ hiện khi nhân vật còn việc.
        private static void BuildTaskStopButton(RectTransform parent, GameBootstrap game)
        {
            RectTransform holder = Stretch("TaskStop", parent);
            var stop = holder.gameObject.AddComponent<TaskStopButton>();

            RectTransform node = Node("Button", holder, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-12f, NavHeight + 12f), new Vector2(110f, ButtonHeight));
            Button button = MakeButton(node, "Dừng", 16f, Wood, Color.white, out TMP_Text label);
            node.gameObject.SetActive(false);

            // Nhân vật có thể chưa đặt vào scene; khi đó TaskStopButton tự tìm lúc chạy.
            var character = Object.FindFirstObjectByType<VeQue.View.Character.FarmCharacterController>();
            Wire(stop, ("_game", game), ("_character", character), ("_button", button), ("_label", label));
        }

        private const string CharacterSkinPath = "Assets/_Game/Data/CharacterSkin.asset";
        // Khung nhân vật 96×64 phóng 8 lần; nhân vật ở giữa khung nên ô xem trước cắt lấy phần giữa.
        private const float PreviewScale = 8f;

        // Nút "Nhân vật" dưới HUD, góc trái.
        // Nút cột trái dưới HUD (cùng cột nút Nhân vật); row 0 = chỗ nút Nhân vật, 1 = ngay dưới.
        private static Button BuildRegionButton(RectTransform parent, string name, string text, int row)
        {
            RectTransform node = Node(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(12f, -(HudHeight + 6f + row * 42f)), new Vector2(100f, 36f));
            return MakeButton(node, text, 15f, Wood, Color.white, out _);
        }

        // Tên vùng (vd. "Bến sông") giữa, ngay dưới HUD; chỉ hiện ở vùng câu (GameUI bật / tắt panel).
        private static TMP_Text BuildRegionLabel(RectTransform parent, out GameObject panel)
        {
            RectTransform band = TopBand("RegionLabel", parent, HudHeight + 6f, 36f);
            RectTransform box = Node("Box", band, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(150f, 36f));
            Frame(box, Skin.LabelCream, Cream, false);
            TMP_Text label = AddLabel(box, "Text", "", 18f, TextAlignmentOptions.Center, TextDark);
            SingleLine(label);
            band.gameObject.SetActive(false);
            panel = band.gameObject;
            return label;
        }

        // ---------- Câu cá (Phần 10 giai đoạn 4, Đợt C) ----------

        // HUD vùng câu + FishingController (trên GameObject của RegionTravel, kèm phao và dấu "!").
        // Bố cục tạm (duyệt 13/11/2026): khung giờ dưới thông báo; dưới cùng: cần / giỏ, mồi, thanh lực, nút Ném / Giật, Thu cần.
        private static void BuildFishing(RectTransform parent, GameBootstrap game, Toast toast)
        {
            var travel = Object.FindFirstObjectByType<RegionTravel>();
            if (travel == null)
            {
                Debug.LogWarning("Tạo UI: chưa có vùng câu (RegionTravel), bỏ qua HUD câu cá. Chạy Về Quê → Vùng câu: tạo tạm rồi Tạo UI lại.");
                return;
            }
            FishingController controller = EnsureFishingController(travel, game);

            RectTransform holder = Stretch("FishingHud", parent);
            var hud = holder.gameObject.AddComponent<FishingHud>();
            RectTransform root = Stretch("Root", holder);

            // Khung giờ, ngay dưới thông báo.
            RectTransform slotBand = TopBand("Slot", root, HudHeight + 98f, 30f);
            RectTransform slotBox = Node("Box", slotBand, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(320f, 30f));
            Frame(slotBox, Skin.LabelPaper, Cream, false);
            TMP_Text slot = AddLabel(slotBox, "Text", "", 16f, TextAlignmentOptions.Center, TextDark);
            SingleLine(slot);

            // Cần + độ bền (trái), giỏ (phải).
            RectTransform info = Node("Info", root, Vector2.zero, new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 196f), new Vector2(-24f, 36f));
            Frame(info, Skin.Paper, Cream, false);
            TMP_Text rod = AddLabel(info, "Rod", "", 16f, TextAlignmentOptions.Left, TextDark);
            Inset(rod.rectTransform, 10f, 0f, 90f, 0f);
            SingleLine(rod);
            TMP_Text basket = AddLabel(info, "Basket", "", 16f, TextAlignmentOptions.Right, TextDark);
            Inset(basket.rectTransform, 10f, 0f, 10f, 0f);
            SingleLine(basket);

            // Chỉ hiện khi đứng ở chỗ câu.
            RectTransform controls = Stretch("SpotControls", root);

            RectTransform baitNode = Node("Bait", controls, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(12f, 150f), new Vector2(230f, 38f));
            Button baitButton = MakeButton(baitNode, "", 16f, Cream, TextDark, out TMP_Text baitLabel);

            // Thanh lực: rãnh tối, vùng Tốt (vàng), vùng Perfect (xanh), con trỏ trắng. HUD đặt anchor theo cần.
            RectTransform bar = Node("PowerBar", controls, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 110f), new Vector2(300f, 24f));
            AddImage(bar, new Color(0.15f, 0.10f, 0.06f, 0.9f), false);
            RectTransform track = Stretch("Track", bar);
            Inset(track, 3f, 3f, 3f, 3f);
            RectTransform good = Stretch("Good", track);
            AddImage(good, new Color(0.95f, 0.78f, 0.30f), false);
            RectTransform perfect = Stretch("Perfect", track);
            AddImage(perfect, new Color(0.45f, 0.80f, 0.30f), false);
            RectTransform cursor = Node("Cursor", track, Vector2.zero, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4f, 6f));
            AddImage(cursor, Color.white, false);
            bar.gameObject.SetActive(false);

            // Nút lớn Ném (giữ, nhả) / Giật (chạm xuống là giật).
            RectTransform actionNode = Node("Action", controls, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 24f), new Vector2(170f, 72f));
            Image actionImage = Frame(actionNode, Skin.Green, Green, true);
            var action = actionNode.gameObject.AddComponent<HoldButton>();
            action.targetGraphic = actionImage;
            TMP_Text actionLabel = AddLabel(actionNode, "Label", "Ném", 24f, TextAlignmentOptions.Center, Color.white);
            SingleLine(actionLabel);
            Outlined(actionLabel);

            // Góc phải dưới cùng (không chạm nút mũi tên phải của bảng kéo cá).
            RectTransform reelNode = Node("Reel", controls, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-12f, 12f), new Vector2(84f, 44f));
            Button reel = MakeButton(reelNode, "Thu cần", 16f, Wood, Color.white, out _);

            BuildPullPanel(root, controller);

            Wire(hud,
                ("_game", game), ("_travel", travel), ("_controller", controller), ("_root", root.gameObject),
                ("_slotLabel", slot), ("_rodLabel", rod), ("_basketLabel", basket), ("_info", info.gameObject),
                ("_spotControls", controls.gameObject), ("_baitButton", baitButton), ("_baitLabel", baitLabel),
                ("_actionButton", action), ("_actionLabel", actionLabel), ("_reelButton", reel),
                ("_powerBar", bar.gameObject), ("_goodZone", good), ("_perfectZone", perfect), ("_cursor", cursor),
                ("_toast", toast));
        }

        // Bảng kéo cá (Đợt D, 1 lượt): thanh thời gian + chuỗi mũi tên phía trên, 4 nút mũi tên chữ thập ở giữa dưới.
        private const int MaxPullArrows = 10;
        private const float PullButtonSize = 64f;
        private const float PullPadCenterY = 110f;
        private const float PullPadStep = 70f;

        private static void BuildPullPanel(RectTransform parent, FishingController controller)
        {
            RectTransform holder = Stretch("FishingPull", parent);
            var panel = holder.gameObject.AddComponent<FishingPullPanel>();
            RectTransform root = Stretch("Root", holder);

            var sprites = new Sprite[4];
            string[] names = { SunnysideUiImporter.ArrowUp, SunnysideUiImporter.ArrowDown, SunnysideUiImporter.ArrowLeft, SunnysideUiImporter.ArrowRight };
            for (int i = 0; i < names.Length; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(SunnysideUiImporter.IconPath(names[i]));
                if (sprites[i] == null)
                    Debug.LogWarning($"Tạo UI: thiếu {SunnysideUiImporter.IconPath(names[i])}. Chạy Về Quê → UI Sunnyside: ghép 9-slice rồi Tạo UI lại.");
            }

            // Thanh thời gian (đầy → rỗng).
            RectTransform timeBar = Node("TimeBar", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 238f), new Vector2(300f, 16f));
            AddImage(timeBar, new Color(0.15f, 0.10f, 0.06f, 0.9f), false);
            RectTransform timeTrack = Stretch("Track", timeBar);
            Inset(timeTrack, 3f, 3f, 3f, 3f);
            RectTransform timeFill = Stretch("Fill", timeTrack);
            Image timeFillImage = AddImage(timeFill, Color.white, false);

            // Chuỗi mũi tên trên nền giấy, tự căn giữa theo số mũi tên đang hiện.
            RectTransform row = Node("Arrows", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 262f), new Vector2(MaxPullArrows * 30f + 16f, 40f));
            Frame(row, Skin.Paper, Cream, false);
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 4f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var slots = new Image[MaxPullArrows];
            for (int i = 0; i < MaxPullArrows; i++)
            {
                RectTransform slotNode = Create($"Arrow{i + 1}", row);
                slotNode.sizeDelta = new Vector2(11f, 13f) * UiPixelScale;
                slots[i] = AddImage(slotNode, Color.white, false);
                slots[i].sprite = sprites[0];
                slots[i].preserveAspect = true;
            }

            // Chữ thập: Lên, Xuống, Trái, Phải (thứ tự theo enum Arrow).
            Vector2[] offsets =
            {
                new Vector2(0f, PullPadStep), new Vector2(0f, -PullPadStep), new Vector2(-PullPadStep, 0f), new Vector2(PullPadStep, 0f),
            };
            var buttons = new HoldButton[4];
            for (int i = 0; i < 4; i++)
            {
                RectTransform node = Node($"Button{(VeQue.Logic.Arrow)i}", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0.5f), new Vector2(0f, PullPadCenterY) + offsets[i], new Vector2(PullButtonSize, PullButtonSize));
                Image background = Frame(node, Skin.Brown, Wood, true);
                buttons[i] = node.gameObject.AddComponent<HoldButton>();
                buttons[i].targetGraphic = background;
                if (sprites[i] != null)
                {
                    RectTransform iconNode = Node("Icon", node, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        Vector2.zero, sprites[i].rect.size * 3f);
                    AddImage(iconNode, Color.white, false).sprite = sprites[i];
                }
            }

            root.gameObject.SetActive(false);
            Wire(panel, ("_controller", controller), ("_root", root.gameObject), ("_timeFill", timeFill), ("_timeFillImage", timeFillImage));
            var so = new SerializedObject(panel);
            SetArray(so.FindProperty("_slots"), slots);
            SetArray(so.FindProperty("_buttons"), buttons);
            SetArray(so.FindProperty("_arrowSprites"), sprites);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // FishingController trên GameObject của RegionTravel ("Regions"); phao và dấu "!" là con (dùng lại nếu đã có).
        private static FishingController EnsureFishingController(RegionTravel travel, GameBootstrap game)
        {
            GameObject owner = travel.gameObject;
            var controller = owner.GetComponent<FishingController>();
            if (controller == null)
                controller = Undo.AddComponent<FishingController>(owner);

            var bobberSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FishingPlaceholderArt.BobberPath);
            var alertSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FishingPlaceholderArt.AlertPath);
            if (bobberSprite == null || alertSprite == null)
                Debug.LogWarning("Tạo UI: chưa có phao / dấu \"!\". Chạy Về Quê → Câu cá: tạo art tạm rồi Tạo UI lại.");

            SpriteRenderer bobber = EnsureWorldSprite(owner.transform, "Bobber", bobberSprite, 300);
            SpriteRenderer alert = EnsureWorldSprite(owner.transform, "BiteAlert", alertSprite, 310);
            var character = Object.FindFirstObjectByType<VeQue.View.Character.FarmCharacterController>();
            Wire(controller, ("_game", game), ("_travel", travel), ("_character", character), ("_bobber", bobber), ("_alert", alert));
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static SpriteRenderer EnsureWorldSprite(Transform parent, string name, Sprite sprite, int sortingOrder)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                var go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, "Tạo UI");
                go.transform.SetParent(parent, false);
                child = go.transform;
            }
            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = Undo.AddComponent<SpriteRenderer>(child.gameObject);
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            child.gameObject.SetActive(false);
            EditorUtility.SetDirty(renderer);
            return renderer;
        }

        // Phủ đen cả màn hình (kể cả ngoài safe area); trong suốt và không chặn chạm khi không chuyển cảnh.
        private static ScreenFader BuildFader(RectTransform parent)
        {
            RectTransform node = Stretch("ScreenFader", parent);
            node.offsetMin = new Vector2(-400f, -400f);
            node.offsetMax = new Vector2(400f, 400f);
            AddImage(node, Color.black, true);
            var group = node.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            var fader = node.gameObject.AddComponent<ScreenFader>();
            Wire(fader, ("_group", group));
            return fader;
        }

        private static Button BuildCharacterButton(RectTransform parent)
        {
            RectTransform node = Node("CharacterButton", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(12f, -(HudHeight + 6f)), new Vector2(100f, 36f));
            return MakeButton(node, "Nhân vật", 15f, Wood, Color.white, out _);
        }

        private static CharacterCreator BuildCharacterCreator(RectTransform parent, GameBootstrap game, Button openButton)
        {
            RectTransform holder = Stretch("CharacterCreator", parent);
            var creator = holder.gameObject.AddComponent<CharacterCreator>();

            // Che cả vùng ngoài safe area để không chạm được map phía sau.
            RectTransform panel = Stretch("Panel", holder);
            panel.offsetMin = new Vector2(-400f, -400f);
            panel.offsetMax = new Vector2(400f, 400f);
            AddImage(panel, new Color(0f, 0f, 0f, 0.6f), true);

            RectTransform box = Node("Box", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360f, 600f));
            Frame(box, Skin.Cream, Cream, true);

            TMP_Text title = AddLabel(BoxRow(box, "Title", 16f, 340f, 36f), "Text", "Nhân vật của bạn", 22f, TextAlignmentOptions.Center, TextDark);

            RectTransform closeNode = Node("Close", box, Vector2.one, Vector2.one, Vector2.one, new Vector2(-8f, -8f), new Vector2(64f, 36f));
            Button close = MakeButton(closeNode, IconOr("Đóng", Icon.Cancel), 15f, Wood, Color.white, out _, Icon.Cancel);

            // Xem trước: khung cắt 220×220, 3 Image 768×512 (96×64 × 8) chồng nhau.
            RectTransform window = BoxRow(box, "Preview", 60f, 220f, 220f);
            Frame(window, Skin.Sand, CreamDark, false);
            window.gameObject.AddComponent<RectMask2D>();
            var preview = window.gameObject.AddComponent<CharacterPreview>();
            Image body = PreviewLayer(window, "Body");
            Image hair = PreviewLayer(window, "Hair");
            Image tools = PreviewLayer(window, "Tools");
            var skin = AssetDatabase.LoadAssetAtPath<VeQue.View.Character.CharacterSkin>(CharacterSkinPath);
            if (skin == null)
                Debug.LogWarning($"Tạo UI: chưa có {CharacterSkinPath}; chạy menu Nhân vật Sunnyside rồi Tạo UI lại.");
            Wire(preview, ("_skin", skin), ("_body", body), ("_hair", hair), ("_tools", tools));

            TMP_InputField nameInput = BuildNameInput(BoxRow(box, "Name", 296f, 300f, 48f));

            RectTransform hairRow = BoxRow(box, "HairRow", 356f, 300f, 48f);
            Button hairPrev = MakeButton(SideNode(hairRow, "Prev", true), "<", 22f, Wood, Color.white, out _);
            Button hairNext = MakeButton(SideNode(hairRow, "Next", false), ">", 22f, Wood, Color.white, out _);
            TMP_Text hairLabel = AddLabel(hairRow, "Label", "Tóc ngắn", 18f, TextAlignmentOptions.Center, TextDark);
            Inset(hairLabel.rectTransform, 56f, 0f, 56f, 0f);

            RectTransform colorRow = BoxRow(box, "ColorRow", 414f, 300f, 48f);
            Button colorPrev = MakeButton(SideNode(colorRow, "Prev", true), "<", 22f, Wood, Color.white, out _);
            Button colorNext = MakeButton(SideNode(colorRow, "Next", false), ">", 22f, Wood, Color.white, out _);
            RectTransform swatchFrame = Node("Swatch", colorRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-58f, 0f), new Vector2(30f, 30f));
            AddImage(swatchFrame, TextDark, false);
            RectTransform swatchFill = Stretch("Fill", swatchFrame);
            Inset(swatchFill, 3f, 3f, 3f, 3f);
            Image swatch = AddImage(swatchFill, Color.white, false);
            TMP_Text colorLabel = AddLabel(colorRow, "Label", "Màu gốc", 18f, TextAlignmentOptions.Center, TextDark);
            Inset(colorLabel.rectTransform, 96f, 0f, 56f, 0f);

            TMP_Text error = AddLabel(BoxRow(box, "Error", 470f, 320f, 28f), "Text", "", 15f, TextAlignmentOptions.Center, new Color(0.75f, 0.15f, 0.1f));

            RectTransform confirmNode = BoxRow(box, "Confirm", 508f, 200f, 48f);
            Button confirm = MakeButton(confirmNode, "Bắt đầu", 18f, Green, Color.white, out TMP_Text confirmLabel, Icon.Confirm);

            Wire(creator, ("_game", game), ("_preview", preview), ("_panel", panel.gameObject), ("_title", title),
                ("_nameInput", nameInput), ("_error", error),
                ("_hairPrev", hairPrev), ("_hairNext", hairNext), ("_hairLabel", hairLabel),
                ("_colorPrev", colorPrev), ("_colorNext", colorNext), ("_colorLabel", colorLabel), ("_colorSwatch", swatch),
                ("_confirm", confirm), ("_confirmLabel", confirmLabel), ("_close", close), ("_openButton", openButton));
            return creator;
        }

        // Hàng trong hộp, căn giữa, đo từ mép trên của hộp.
        private static RectTransform BoxRow(RectTransform box, string name, float top, float width, float height)
        {
            return Node(name, box, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -top), new Vector2(width, height));
        }

        // Nút vuông ở mép trái / phải của một hàng.
        private static RectTransform SideNode(RectTransform row, string name, bool left)
        {
            Vector2 anchor = left ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            return Node(name, row, anchor, anchor, anchor, Vector2.zero, new Vector2(48f, 48f));
        }

        private static Image PreviewLayer(RectTransform window, string name)
        {
            RectTransform node = Node(name, window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(96f * PreviewScale, 64f * PreviewScale));
            Image image = AddImage(node, Color.white, false);
            image.enabled = false;
            return image;
        }

        private static TMP_InputField BuildNameInput(RectTransform node)
        {
            Image background = Frame(node, Skin.Paper, Color.white, true);
            var input = node.gameObject.AddComponent<TMP_InputField>();

            RectTransform area = Stretch("Text Area", node);
            Inset(area, 12f, 6f, 12f, 6f);
            area.gameObject.AddComponent<RectMask2D>();

            TMP_Text placeholder = AddLabel(area, "Placeholder", "Tên nhân vật", 18f, TextAlignmentOptions.MidlineLeft, new Color(0.27f, 0.16f, 0.09f, 0.45f));
            placeholder.fontStyle = FontStyles.Italic;
            TMP_Text text = AddLabel(area, "Text", "", 18f, TextAlignmentOptions.MidlineLeft, TextDark);
            text.richText = false;

            input.targetGraphic = background;
            input.textViewport = area;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.richText = false;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = VeQue.Logic.CharacterLook.MaxNameLength;
            return input;
        }

        private static T BuildPanel<T>(RectTransform parent, string name, string title, GameBootstrap game) where T : CropListPanel
        {
            RectTransform root = Stretch(name, parent);
            BottomSheet sheetScript = BuildSheet(root, title, out RectTransform sheet);
            var panel = root.gameObject.AddComponent<T>();

            BuildList(sheet, out RectTransform content);
            CropRow template = BuildRowTemplate(content);

            Wire(panel, ("_game", game), ("_sheet", sheetScript), ("_rowTemplate", template), ("_content", content));
            return panel;
        }

        private const int OrderCards = 5;
        private const int OrderItemSlots = 3;
        private const float OrderCardHeight = 116f;

        private static OrderPanel BuildOrderPanel(RectTransform parent, GameBootstrap game)
        {
            RectTransform root = Stretch("OrderPanel", parent);
            BottomSheet sheetScript = BuildSheet(root, "Đơn hàng", out RectTransform sheet);
            var panel = root.gameObject.AddComponent<OrderPanel>();

            BuildList(sheet, out RectTransform content);
            TMP_Text dailyInfo = AddLabel(content, "DailyInfo", "", 15f, TextAlignmentOptions.Center, TextDark);
            dailyInfo.gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;
            var cards = new OrderCard[OrderCards];
            for (int i = 0; i < OrderCards; i++)
                cards[i] = BuildOrderCard(content, i);

            var so = new SerializedObject(panel);
            so.FindProperty("_game").objectReferenceValue = game;
            so.FindProperty("_sheet").objectReferenceValue = sheetScript;
            SetArray(so.FindProperty("_cards"), cards);
            so.FindProperty("_dailyInfo").objectReferenceValue = dailyInfo;
            so.ApplyModifiedPropertiesWithoutUndo();
            return panel;
        }

        // Hàng trên: người đặt (trái). Hàng giữa: tối đa 3 món. Hàng dưới: thưởng (trái), Bỏ + Giao (phải).
        private static OrderCard BuildOrderCard(RectTransform content, int index)
        {
            RectTransform card = Stretch($"Order{index + 1}", content);
            Image background = Frame(card, Skin.Paper, CreamDark, true);
            card.gameObject.AddComponent<LayoutElement>().preferredHeight = OrderCardHeight;
            var script = card.gameObject.AddComponent<OrderCard>();

            RectTransform body = Stretch("Content", card);
            TMP_Text customer = AddLabel(body, "Customer", "", 17f, TextAlignmentOptions.MidlineLeft, TextDark);
            customer.rectTransform.anchorMin = new Vector2(0f, 1f);
            customer.rectTransform.anchorMax = Vector2.one;
            customer.rectTransform.pivot = new Vector2(0.5f, 1f);
            customer.rectTransform.anchoredPosition = new Vector2(0f, -4f);
            customer.rectTransform.sizeDelta = new Vector2(-20f, 26f);

            // Cùng hàng với người đặt, canh phải: nhãn khẩn cấp + đếm ngược hết hạn.
            TMP_Text badge = AddLabel(body, "Badge", "", 14f, TextAlignmentOptions.MidlineRight, UrgentText);
            badge.fontStyle = FontStyles.Bold;
            badge.rectTransform.anchorMin = new Vector2(0f, 1f);
            badge.rectTransform.anchorMax = Vector2.one;
            badge.rectTransform.pivot = new Vector2(0.5f, 1f);
            badge.rectTransform.anchoredPosition = new Vector2(0f, -4f);
            badge.rectTransform.sizeDelta = new Vector2(-20f, 26f);
            badge.gameObject.SetActive(false);

            var roots = new GameObject[OrderItemSlots];
            var icons = new Image[OrderItemSlots];
            var labels = new TMP_Text[OrderItemSlots];
            for (int i = 0; i < OrderItemSlots; i++)
            {
                RectTransform item = Node($"Item{i + 1}", body, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f + i * 116f, -32f), new Vector2(112f, 30f));
                RectTransform iconNode = Node("Icon", item, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(28f, 28f));
                icons[i] = AddImage(iconNode, Color.white, false);
                icons[i].preserveAspect = true;
                labels[i] = AddLabel(item, "Label", "", 13f, TextAlignmentOptions.MidlineLeft, TextDark);
                Inset(labels[i].rectTransform, 32f, 0f, 0f, 0f);
                // "Tên\nđang có/cần": không tự xuống dòng (cắt "…" sẽ mất luôn dòng số lượng).
                // 4 cây MVP vừa 80 đơn vị ở cỡ nhỏ; tên dài hơn sẽ lấn sang món bên cạnh.
                labels[i].textWrappingMode = TextWrappingModes.NoWrap;
                roots[i] = item.gameObject;
            }

            TMP_Text reward = AddLabel(body, "Reward", "", 14f, TextAlignmentOptions.MidlineLeft, TextDark);
            Inset(reward.rectTransform, 10f, 6f, 172f, OrderCardHeight - 50f);
            SingleLine(reward);

            RectTransform deliverNode = Node("Deliver", body, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-6f, 6f), new Vector2(78f, ButtonHeight));
            Button deliver = MakeButton(deliverNode, "Giao", 15f, Green, Color.white, out _, Icon.Confirm);
            RectTransform discardNode = Node("Discard", body, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-88f, 6f), new Vector2(78f, ButtonHeight));
            Button discard = MakeButton(discardNode, "Bỏ", 15f, Wood, Color.white, out _, Icon.Cancel);

            TMP_Text waiting = AddLabel(card, "Waiting", "", 15f, TextAlignmentOptions.Center, TextDark);
            // Con của dòng chờ nên ẩn / hiện cùng.
            AddIcon(waiting.rectTransform, Icon.Sandtimer, 0f, 14f);
            waiting.gameObject.SetActive(false);

            var so = new SerializedObject(script);
            so.FindProperty("_content").objectReferenceValue = body.gameObject;
            so.FindProperty("_customer").objectReferenceValue = customer;
            so.FindProperty("_reward").objectReferenceValue = reward;
            SetArray(so.FindProperty("_itemRoots"), roots);
            SetArray(so.FindProperty("_itemIcons"), icons);
            SetArray(so.FindProperty("_itemLabels"), labels);
            so.FindProperty("_deliver").objectReferenceValue = deliver;
            so.FindProperty("_discard").objectReferenceValue = discard;
            so.FindProperty("_waiting").objectReferenceValue = waiting;
            so.FindProperty("_background").objectReferenceValue = background;
            so.FindProperty("_badge").objectReferenceValue = badge;
            bool framed = background.sprite != null;
            so.FindProperty("_normalColor").colorValue = framed ? Color.white : CreamDark;
            if (framed)
                so.FindProperty("_urgentColor").colorValue = UrgentTint;
            so.ApplyModifiedPropertiesWithoutUndo();
            return script;
        }

        // Nền tối chặn chạm + tấm trượt có tiêu đề, nút X. Trả về BottomSheet đã nối tham chiếu.
        private static BottomSheet BuildSheet(RectTransform root, string title, out RectTransform sheet)
        {
            var sheetScript = root.gameObject.AddComponent<BottomSheet>();

            RectTransform backdrop = Stretch("Backdrop", root);
            backdrop.offsetMin = new Vector2(-400f, -400f);
            backdrop.offsetMax = new Vector2(400f, 400f);
            AddImage(backdrop, Dim, true);
            Button backdropButton = backdrop.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;

            sheet = Node("Sheet", root, Vector2.zero, new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, NavHeight), new Vector2(0f, SheetHeight));
            Frame(sheet, Skin.Cream, Cream, true);

            TMP_Text titleText = AddLabel(sheet, "Title", title, 22f, TextAlignmentOptions.MidlineLeft, TextDark);
            titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleText.rectTransform.pivot = new Vector2(0.5f, 1f);
            titleText.rectTransform.anchoredPosition = Vector2.zero;
            titleText.rectTransform.sizeDelta = new Vector2(-24f, 52f);

            RectTransform closeNode = Node("Close", sheet, Vector2.one, Vector2.one, Vector2.one, new Vector2(-6f, -4f), new Vector2(ButtonHeight, ButtonHeight));
            Button close = MakeButton(closeNode, IconOr("X", Icon.Cancel), 18f, Wood, Color.white, out _, Icon.Cancel);

            backdrop.gameObject.SetActive(false);
            sheet.gameObject.SetActive(false);

            Wire(sheetScript, ("_backdrop", backdrop.gameObject), ("_sheet", sheet), ("_backdropButton", backdropButton), ("_closeButton", close));
            return sheetScript;
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
            Frame(row, Skin.Paper, CreamDark, true);
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

        private static void BuildNav(RectTransform parent, out Button farm, out Button orders, out TMP_Text ordersLabel,
            out Button storage, out Button shop)
        {
            RectTransform nav = Node("Nav", parent, Vector2.zero, new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, NavHeight));
            Frame(nav, Skin.Sand, Wood, true);
            var layout = nav.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 6, 6);
            layout.spacing = 3f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            farm = MakeTab(nav, "Nông trại", out _);
            orders = MakeTab(nav, "Đơn hàng", out ordersLabel);
            storage = MakeTab(nav, "Kho", out _);
            shop = MakeTab(nav, "Cửa hàng", out _);
        }

        // 4 tab trên bề ngang 390: chữ 15 để "Đơn hàng (3)" vừa một dòng.
        private static Button MakeTab(RectTransform nav, string label, out TMP_Text text)
        {
            RectTransform node = Stretch(label, nav);
            Button button = MakeButton(node, label, 15f, Cream, TextDark, out text);
            // Font pixel giữ cỡ cố định (tự co sẽ ra cỡ lẻ, mờ nét); tab hẹp nên dùng cỡ nhỏ.
            if (_pixelFont == null)
            {
                text.enableAutoSizing = true;
                text.fontSizeMin = 11f;
                text.fontSizeMax = 15f;
            }
            else
            {
                text.fontSize = PixelFontSizeFor(2);
                // Chữ được tràn nhẹ lên viền khung nhãn (khung bo 4 pixel mỗi bên).
                Inset(text.rectTransform, -2f, 0f, -2f, 0f);
            }
            return button;
        }

        // Dưới nhãn tên vùng (HudHeight + 6, cao 36) để ở vùng câu không bị che.
        private static Toast BuildToast(RectTransform parent)
        {
            RectTransform holder = TopBand("Toast", parent, HudHeight + 48f, 44f);
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
            Frame(box, Skin.Cream, Cream, true);

            TMP_Text title = AddLabel(box, "Title", "Mở ô đất này?", 20f, TextAlignmentOptions.Center, TextDark);
            Inset(title.rectTransform, 12f, 112f, 12f, 12f);
            TMP_Text price = AddLabel(box, "Price", "", 18f, TextAlignmentOptions.Center, TextDark);
            Inset(price.rectTransform, 12f, 68f, 12f, 68f);

            RectTransform cancelNode = Node("Cancel", box, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(16f, 14f), new Vector2(124f, ButtonHeight));
            Button cancel = MakeButton(cancelNode, "Hủy", 16f, Wood, Color.white, out _, Icon.Cancel);
            RectTransform confirmNode = Node("Confirm", box, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 14f), new Vector2(124f, ButtonHeight));
            Button confirm = MakeButton(confirmNode, "Mở", 16f, Green, Color.white, out _, Icon.Confirm);

            dim.gameObject.SetActive(false);
            Wire(dialog, ("_game", game), ("_interaction", interaction), ("_root", dim.gameObject),
                ("_priceText", price), ("_confirm", confirm), ("_cancel", cancel));
            return dialog;
        }

        private const int FlyingIcons = 9;
        private const float FlyingIconSize = 36f;

        // Lớp phủ không chặn chạm chứa icon bay; nhãn "+N [icon]" gắn trên tab Kho để nảy cùng tab.
        private static HarvestFlyer BuildHarvestFlyer(RectTransform parent, GameBootstrap game, FarmInteraction interaction,
            FarmView view, Camera mapCamera, RectTransform storageTab)
        {
            RectTransform layer = Stretch("HarvestFly", parent);
            var flyer = layer.gameObject.AddComponent<HarvestFlyer>();

            var icons = new Image[FlyingIcons];
            for (int i = 0; i < FlyingIcons; i++)
            {
                RectTransform node = Node($"Icon{i + 1}", layer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(FlyingIconSize, FlyingIconSize));
                icons[i] = AddImage(node, Color.white, false);
                icons[i].preserveAspect = true;
                node.gameObject.SetActive(false);
            }

            RectTransform badge = Node("HarvestBadge", storageTab, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(76f, 30f));
            AddImage(badge, new Color(0.15f, 0.10f, 0.06f, 0.85f), false);
            badge.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;
            TMP_Text text = AddLabel(badge, "Text", "+0", 16f, TextAlignmentOptions.MidlineRight, Color.white);
            Inset(text.rectTransform, 4f, 0f, 36f, 0f);
            RectTransform iconNode = Node("Icon", badge, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(24f, 24f));
            Image badgeIcon = AddImage(iconNode, Color.white, false);
            badgeIcon.preserveAspect = true;
            badge.gameObject.SetActive(false);

            var so = new SerializedObject(flyer);
            so.FindProperty("_interaction").objectReferenceValue = interaction;
            so.FindProperty("_game").objectReferenceValue = game;
            so.FindProperty("_view").objectReferenceValue = view;
            so.FindProperty("_camera").objectReferenceValue = mapCamera;
            so.FindProperty("_layer").objectReferenceValue = layer;
            SetArray(so.FindProperty("_icons"), icons);
            so.FindProperty("_target").objectReferenceValue = storageTab;
            so.FindProperty("_badge").objectReferenceValue = badge.gameObject;
            so.FindProperty("_badgeIcon").objectReferenceValue = badgeIcon;
            so.FindProperty("_badgeText").objectReferenceValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();
            return flyer;
        }

        private static LevelUpDialog BuildLevelUpDialog(RectTransform parent, GameBootstrap game)
        {
            RectTransform holder = Stretch("LevelUpDialog", parent);
            var dialog = holder.gameObject.AddComponent<LevelUpDialog>();

            RectTransform dim = Stretch("Root", holder);
            dim.offsetMin = new Vector2(-400f, -400f);
            dim.offsetMax = new Vector2(400f, 400f);
            AddImage(dim, Dim, true);

            RectTransform box = Node("Box", dim, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 260f));
            Frame(box, Skin.Cream, Cream, true);

            TMP_Text title = AddLabel(box, "Title", "Lên cấp!", 24f, TextAlignmentOptions.Center, TextDark);
            Inset(title.rectTransform, 12f, 200f, 12f, 12f);
            TMP_Text body = AddLabel(box, "Body", "", 16f, TextAlignmentOptions.Center, TextDark);
            Inset(body.rectTransform, 16f, 70f, 16f, 64f);

            RectTransform okNode = Node("Ok", box, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(160f, ButtonHeight));
            Button ok = MakeButton(okNode, "Tuyệt", 18f, Green, Color.white, out _, Icon.Confirm);

            dim.gameObject.SetActive(false);
            Wire(dialog, ("_game", game), ("_root", dim.gameObject), ("_title", title), ("_body", body), ("_ok", ok));
            return dialog;
        }

        // ---------- Helpers ----------

        private static void SetArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

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
            label.font = _pixelFont != null ? _pixelFont : TMP_Settings.defaultFontAsset;
            label.text = text;
            label.fontSize = _pixelFont != null ? PixelFontSize(size) : size;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        // Có sprite: tab (nền Cream) dùng nhãn giấy; nút xanh / nâu dùng khung xanh / nâu.
        private static Button MakeButton(RectTransform node, string text, float size, Color background, Color textColor, out TMP_Text label,
            Icon icon = Icon.None)
        {
            Image image;
            if (background == Cream)
                image = Frame(node, Skin.LabelPaper, background, true);
            else if (background == Green)
                image = Frame(node, Skin.Green, background, true);
            else if (background == Wood)
                image = Frame(node, Skin.Brown, background, true);
            else
                image = AddImage(node, background, true);
            var button = node.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            label = AddLabel(node, "Label", text, size, TextAlignmentOptions.Center, textColor);
            SingleLine(label);
            // Chữ trắng trên nút xanh / nâu: thêm viền tối cho dễ đọc.
            if (textColor == Color.white)
                Outlined(label);

            // Icon bên trái chữ; nút không chữ thì icon nằm giữa.
            Image iconImage = AddIcon(node, icon, string.IsNullOrEmpty(text) ? 0.5f : 0f, ButtonIconPadding);
            if (iconImage != null && !string.IsNullOrEmpty(text))
                Inset(label.rectTransform, ButtonIconPadding + iconImage.rectTransform.sizeDelta.x, 0f, 4f, 0f);
            return button;
        }

        private const float ButtonIconPadding = 6f;

        private static void Outlined(TMP_Text label)
        {
            if (_outlineMaterial != null)
                label.fontSharedMaterial = _outlineMaterial;
        }

        // Một dòng; dài quá khung thì cắt bằng "…".
        private static void SingleLine(TMP_Text label)
        {
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
        }

        // Nút chỉ có icon: chưa có sprite icon thì dùng chữ thay.
        private static string IconOr(string fallback, Icon icon) => Icons.ContainsKey(icon) ? "" : fallback;

        // Icon kích thước gốc × UiPixelScale, căn giữa theo chiều dọc; anchorX 0 = sát trái (cách offset), 0.5 = giữa.
        // Không có sprite thì không thêm gì và trả về null.
        private static Image AddIcon(RectTransform parent, Icon icon, float anchorX, float offset)
        {
            if (icon == Icon.None || !Icons.TryGetValue(icon, out Sprite sprite))
                return null;

            Vector2 size = sprite.rect.size * UiPixelScale;
            var anchor = new Vector2(anchorX, 0.5f);
            RectTransform node = Node("Icon", parent, anchor, anchor, anchor,
                new Vector2(anchorX < 0.5f ? offset : 0f, 0f), size);
            Image image = AddImage(node, Color.white, false);
            image.sprite = sprite;
            return image;
        }

        // Khung thanh (greenbar) + vùng rãnh để đặt phần đầy. Cao đúng 7 pixel art (không giãn dọc).
        // Chưa có sprite: nền màu phẳng, rãnh = cả khung.
        private static RectTransform BuildBar(RectTransform bar, out float height)
        {
            height = _bar != null ? _bar.rect.height * UiPixelScale : 12f;
            if (_bar == null)
            {
                AddImage(bar, CreamDark, false);
                return Stretch("Track", bar);
            }

            Image image = AddImage(bar, Color.white, false);
            image.sprite = _bar;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 100f / (_bar.pixelsPerUnit * UiPixelScale);

            RectTransform track = Stretch("Track", bar);
            Vector4 inset = SunnysideUiImporter.BarTrackInset * UiPixelScale;
            Inset(track, inset.x, inset.y, inset.z, inset.w);
            return track;
        }

        private static Color BarFillColor => _bar != null ? SunnysideUiImporter.BarFill : Green;

        private static void LoadSkin()
        {
            Skins.Clear();
            var missing = new System.Collections.Generic.List<string>();
            foreach (Skin skin in System.Enum.GetValues(typeof(Skin)))
            {
                string path = SkinPath(skin);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                    Skins[skin] = sprite;
                else
                    missing.Add(path);
            }

            Icons.Clear();
            foreach (Icon icon in System.Enum.GetValues(typeof(Icon)))
            {
                if (icon == Icon.None)
                    continue;
                string path = SunnysideUiImporter.IconPath(icon.ToString().ToLowerInvariant());
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                    Icons[icon] = sprite;
                else
                    missing.Add(path);
            }
            _bar = AssetDatabase.LoadAssetAtPath<Sprite>(SunnysideUiImporter.BarPath);
            if (_bar == null)
                missing.Add(SunnysideUiImporter.BarPath);

            // Thiếu sprite nào thì chỗ dùng sprite đó dựng bằng màu phẳng / không có icon; các chỗ khác vẫn dùng khung.
            if (missing.Count > 0)
                Debug.LogWarning("Tạo UI: thiếu sprite UI Sunnyside, các chỗ đó dùng màu phẳng. Chạy Về Quê → UI Sunnyside: ghép 9-slice rồi Tạo UI lại.\n"
                    + string.Join("\n", missing));
        }

        private static string SkinPath(Skin skin)
        {
            switch (skin)
            {
                case Skin.LabelPaper: return SunnysideUiImporter.LabelPath("paper");
                case Skin.LabelCream: return SunnysideUiImporter.LabelPath("cream");
                case Skin.LabelTan: return SunnysideUiImporter.LabelPath("tan");
                default: return SunnysideUiImporter.BoxPath(skin.ToString().ToLowerInvariant());
            }
        }

        // Khung 9-slice giữ màu gốc của sprite; chưa có sprite thì nền màu phẳng.
        private static Image Frame(RectTransform rect, Skin skin, Color flat, bool raycastTarget)
        {
            if (!Skins.TryGetValue(skin, out Sprite sprite))
                return AddImage(rect, flat, raycastTarget);

            Image image = AddImage(rect, Color.white, raycastTarget);
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            // Image: pixelsPerUnit = sprite PPU / Canvas.referencePixelsPerUnit × multiplier; muốn 1 pixel art = UiPixelScale đơn vị.
            image.pixelsPerUnitMultiplier = 100f / (sprite.pixelsPerUnit * UiPixelScale);
            return image;
        }

        // Cỡ chữ để 1 pixel thiết kế (= 1 texel atlas) = N pixel màn hình khi Canvas ×3 (màn 1170): sắc nét.
        private static float PixelFontSizeFor(int screenPixels) => screenPixels * PixelFontBuilder.PointSize / 3f;

        // FatPix rộng nên chỉ 2 bậc: chữ thường → nhỏ (2 px / pixel, cỡ 16), tiêu đề (cỡ cũ ≥ 20) → vừa (3 px, cỡ 24).
        // Đã đo độ rộng các chuỗi dài nhất với khung chứa (12/11/2026).
        private static float PixelFontSize(float size) => size < 20f ? PixelFontSizeFor(2) : PixelFontSizeFor(3);

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
