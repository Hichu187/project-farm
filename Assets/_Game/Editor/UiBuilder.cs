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
        // Dòng "Kỷ lục mới!" / "Loài mới!" trong bảng cá vừa câu (có viền chữ).
        private static readonly Color RecordGold = new Color(0.98f, 0.78f, 0.20f);
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

        [MenuItem("Về Quê/Dựng scene/Tạo UI", priority = 101)]
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
            MailPanel mailPanel = BuildMailPanel(safe, game);
            StoragePanel storagePanel = BuildPanel<StoragePanel>(safe, "StoragePanel", "Kho", game);
            ShopPanel shopPanel = BuildPanel<ShopPanel>(safe, "ShopPanel", "Cửa hàng", game);
            BuildNav(safe, out Button ordersTab, out TMP_Text ordersTabLabel, out Button storageTab, out Button shopTab);
            BuildHarvestFlyer(safe, game, interaction, view, camera.GetComponent<Camera>(), (RectTransform)storageTab.transform);
            Toast toast = BuildToast(safe);
            BuildGameClock(safe, game);
            UnlockDialog dialog = BuildDialog(safe, game, interaction);
            BuildLevelUpDialog(safe, game);
            // Sau cùng để nằm trên mọi thứ (lần đầu vào game che cả HUD).
            Button characterButton = BuildCharacterButton(safe);
            // Vùng chơi: Đi câu dưới nút Nhân vật; ở vùng câu: Về nhà ở chỗ nút Nhân vật + tên vùng.
            Button goFishing = BuildRegionButton(safe, "GoFishing", "Đi câu", 1);
            Button goHome = BuildRegionButton(safe, "GoHome", "Về nhà", 0);
            // Đợt 2 Phần 3: nút Thư dưới Đi câu (hiện ở cả hai vùng).
            Button mailButton = BuildMailButton(safe, game);
            TMP_Text regionLabel = BuildRegionLabel(safe, out GameObject regionPanel);
            BuildFishing(safe, game, toast);
            // Sau BuildFishing (cần FishingController) và BuildToast.
            BuildShopTabs(shopPanel, game, toast);
            CharacterCreator creator = BuildCharacterCreator(safe, game, characterButton);
            // Sau cùng: màn tối chuyển cảnh phủ lên mọi thứ.
            ScreenFader fader = BuildFader(safe);
            var travel = Object.FindFirstObjectByType<RegionTravel>();
            if (travel != null)
                Wire(travel, ("_fader", fader));

            Wire(storagePanel, ("_emptyLabel", BuildEmptyLabel(storagePanel)));
            // Sau nhãn Empty (đưa vào trang Nông sản).
            BuildStorageTabs(storagePanel, game, toast);
            Wire(ui,
                ("_game", game), ("_interaction", interaction), ("_touch", touch),
                ("_coins", coins), ("_backup", backup),
                ("_ordersTab", ordersTab), ("_ordersTabLabel", ordersTabLabel),
                ("_storageTab", storageTab), ("_shopTab", shopTab),
                ("_toast", toast), ("_seedPicker", seedPicker), ("_gesture", gesture), ("_orderPanel", orderPanel),
                ("_storagePanel", storagePanel), ("_shopPanel", shopPanel),
                ("_unlockDialog", dialog), ("_progressPopup", popup), ("_characterCreator", creator),
                ("_travel", travel), ("_goFishing", goFishing), ("_goHome", goHome), ("_regionLabel", regionLabel), ("_regionPanel", regionPanel),
                ("_mailPanel", mailPanel), ("_mailButton", mailButton));
            var uiSo = new SerializedObject(ui);
            SetArray(uiSo.FindProperty("_farmOnly"), new Object[]
            {
                ordersTab.transform.parent.gameObject, characterButton.gameObject, goFishing.gameObject,
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

        private const float SeedBoxPadding = 4f;

        // Thanh nhỏ bám trên ô (pivot giữa cạnh dưới). SeedPicker đặt bề ngang theo số icon, tối đa ~5,5 icon;
        // nhiều hơn thì cuộn ngang (ScrollRect). Icon hạt đi theo ngón tay khi kéo gieo.
        private static SeedPicker BuildSeedPicker(RectTransform parent, GameBootstrap game, FarmInteraction interaction,
            FarmGesture gesture, FarmView view, CameraController camera)
        {
            RectTransform area = Stretch("SeedPicker", parent);
            var picker = area.gameObject.AddComponent<SeedPicker>();

            RectTransform box = Node("Box", area, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero,
                new Vector2(SeedItemWidth + SeedBoxPadding * 2f, SeedItemHeight + SeedBoxPadding * 2f));
            AddImage(box, new Color(0.15f, 0.10f, 0.06f, 0.85f), true);
            CanvasGroup boxGroup = box.gameObject.AddComponent<CanvasGroup>();

            RectTransform viewport = Stretch("Viewport", box);
            Inset(viewport, SeedBoxPadding, SeedBoxPadding, SeedBoxPadding, SeedBoxPadding);
            viewport.gameObject.AddComponent<RectMask2D>();

            // Content neo mép trái, cao bằng viewport; SeedPicker.ResizeBox đặt bề ngang theo số icon.
            RectTransform content = Node("Content", viewport, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(SeedItemWidth, 0f));
            var layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var scroll = box.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            scroll.scrollSensitivity = 20f;

            RectTransform item = Create("ItemTemplate", content);
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
                ("_area", area), ("_box", box), ("_boxGroup", boxGroup), ("_content", content), ("_scroll", scroll),
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
        // Bố cục (duyệt 13/11/2026): cột 3 nút icon Cần / Giỏ / Mồi bên phải; dưới cùng: thanh lực, nút Ném / Giật, Thu cần.
        // Khung giờ chuyển sang đồng hồ chung (BuildGameClock).
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

            // Cột nút icon bên phải (trên nút Thu cần): Cần (trên), Mồi, Giỏ (dưới). Số nhỏ ở góc dưới-phải.
            Sprite rodIcon = LoadUiIcon(SunnysideUiImporter.RodIcon);
            Sprite basketIcon = LoadUiIcon(SunnysideUiImporter.BasketIcon);
            Sprite baitIcon = LoadUiIcon(SunnysideUiImporter.BaitIcon);
            RectTransform items = Stretch("ItemButtons", root);
            // Icon Cần / Mồi đổi theo cần đang dùng / mồi đang chọn (FishingHud); icon Sunnyside ở đây là dự phòng.
            Button rodButton = BuildIconButton(items, "Rod", rodIcon, 2, out TMP_Text rodBadge, out Image rodIconImage);
            // 14/11/2026: đổi chỗ Giỏ và Mồi → Cần (trên), Mồi, Giỏ (dưới).
            Button basketButton = BuildIconButton(items, "Basket", basketIcon, 0, out TMP_Text basketBadge, out _);
            Button baitButton = BuildIconButton(items, "Bait", baitIcon, 1, out TMP_Text baitBadge, out Image baitIconImage);

            // Chỉ hiện khi đứng ở chỗ câu.
            RectTransform controls = Stretch("SpotControls", root);

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

            // Góc trái dưới cùng (Đợt 2 Phần 6): đổi chữ thập kéo cá sang tay trái / phải; chỉ hiện khi chưa câu.
            RectTransform handNode = Node("Hand", controls, Vector2.zero, Vector2.zero, Vector2.zero,
                new Vector2(12f, 12f), new Vector2(84f, 44f));
            Button hand = MakeButton(handNode, "Tay phải", 16f, Wood, Color.white, out TMP_Text handLabel);

            BuildPullPanel(root, controller);
            WorldPopText popText = BuildWorldPopText(root);

            // Bảng Cần / Giỏ / Mồi và bảng cá vừa câu: trên HUD câu để nền tối che các nút.
            RodPanel rodPanel = BuildRodPanel(parent, game, rodIcon, toast);
            BasketPanel basketPanel = BuildBasketPanel(parent, game, toast);
            BaitPanel baitPanel = BuildBaitPanel(parent, game, controller, baitIcon, toast);

            Wire(hud,
                ("_game", game), ("_travel", travel), ("_controller", controller), ("_root", root.gameObject),
                ("_itemButtons", items.gameObject),
                ("_rodButton", rodButton), ("_rodBadge", rodBadge), ("_basketButton", basketButton), ("_basketBadge", basketBadge),
                ("_baitButton", baitButton), ("_baitBadge", baitBadge),
                ("_rodIcon", rodIconImage), ("_baitIcon", baitIconImage),
                ("_rodPanel", rodPanel), ("_basketPanel", basketPanel), ("_baitPanel", baitPanel),
                ("_spotControls", controls.gameObject),
                ("_actionButton", action), ("_actionLabel", actionLabel), ("_reelButton", reel),
                ("_handButton", hand), ("_handLabel", handLabel),
                ("_powerBar", bar.gameObject), ("_goodZone", good), ("_perfectZone", perfect), ("_cursor", cursor),
                ("_toast", toast), ("_popText", popText));

            BuildCatchResultPanel(parent, game, controller);
        }

        private const float IconButtonSize = 56f;
        private const float IconButtonBottom = 72f;
        private const float IconButtonStep = 64f;

        private static Sprite LoadUiIcon(string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SunnysideUiImporter.IconPath(name));
            if (sprite == null)
                Debug.LogWarning($"Tạo UI: thiếu {SunnysideUiImporter.IconPath(name)}. Chạy Về Quê → UI Sunnyside: ghép 9-slice rồi Tạo UI lại.");
            return sprite;
        }

        // Nút vuông chỉ có icon (pixel ×2) + số nhỏ góc dưới-phải; row 0 = dưới cùng của cột.
        private static Button BuildIconButton(RectTransform parent, string name, Sprite icon, int row, out TMP_Text badge,
            out Image iconImage)
        {
            RectTransform node = Node(name, parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-12f, IconButtonBottom + row * IconButtonStep), new Vector2(IconButtonSize, IconButtonSize));
            Image background = Frame(node, Skin.Cream, Cream, true);
            var button = node.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            // Luôn có ô icon (kể cả khi thiếu icon Sunnyside) để FishingHud đặt icon cần / mồi lúc chạy.
            Vector2 size = icon != null ? icon.rect.size * UiPixelScale : new Vector2(16f, 16f) * UiPixelScale;
            RectTransform iconNode = Node("Icon", node, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 4f), size);
            iconImage = AddImage(iconNode, Color.white, false);
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
            if (icon == null)
                AddLabel(node, "Label", name, 12f, TextAlignmentOptions.Center, TextDark);
            badge = AddLabel(node, "Badge", "", 12f, TextAlignmentOptions.BottomRight, Color.white);
            Inset(badge.rectTransform, 2f, 2f, 4f, 2f);
            SingleLine(badge);
            Outlined(badge);
            badge.raycastTarget = false;
            return button;
        }

        // Đồng hồ chung góc trên phải, dưới nút Sao lưu: icon mặt trời / mặt trăng + giờ game hh:mm (21/11/2026).
        private static void BuildGameClock(RectTransform parent, GameBootstrap game)
        {
            RectTransform node = Node("GameClock", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-12f, -(HudHeight + 4f)), new Vector2(104f, 40f));
            Frame(node, Skin.LabelCream, Cream, true); // vẫn chặn chạm để không lọt xuống map
            // 21/11/2026: icon đổi lúc chạy thành mặt trời / mặt trăng theo giờ game (GameClock).
            Image clockIcon = AddIcon(node, Icon.Stopwatch, 0f, 8f);
            TMP_Text time = AddLabel(node, "Time", "00:00", 16f, TextAlignmentOptions.Center, TextDark);
            Inset(time.rectTransform, 30f, 2f, 4f, 2f);
            SingleLine(time);
            // 21/11/2026: bỏ dòng âm lịch dưới đồng hồ – người chơi tự xem lịch ngoài đời.
            // Đợt 2 Phần 13: chạm đồng hồ đổi ngày đêm Đầy đủ / Nhẹ / Tắt.
            Button clockButton = node.gameObject.AddComponent<Button>();
            clockButton.transition = Selectable.Transition.None;
            var clock = node.gameObject.AddComponent<GameClock>();
            Wire(clock, ("_game", game), ("_time", time), ("_button", clockButton), ("_icon", clockIcon));
        }

        private const int ItemRowButtons = 3;
        private const float ItemRowHeight = 84f;

        // Dòng mẫu danh sách Cần / Mồi: icon trái, tên + dòng phụ ở trên, tối đa 3 nút ở góc dưới-phải (trái → phải).
        private static FishingItemRow BuildItemRowTemplate(RectTransform content)
        {
            RectTransform row = Stretch("RowTemplate", content);
            Image background = Frame(row, Skin.Paper, CreamDark, true);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = ItemRowHeight;
            var script = row.gameObject.AddComponent<FishingItemRow>();

            RectTransform iconNode = Node("Icon", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(10f, 0f), new Vector2(40f, 40f));
            Image icon = AddImage(iconNode, Color.white, false);
            icon.preserveAspect = true;

            TMP_Text title = AddLabel(row, "Title", "", 17f, TextAlignmentOptions.BottomLeft, TextDark);
            Inset(title.rectTransform, 60f, 58f, 8f, 4f);
            SingleLine(title);
            TMP_Text subtitle = AddLabel(row, "Subtitle", "", 12f, TextAlignmentOptions.TopLeft, TextDark);
            Inset(subtitle.rectTransform, 60f, 42f, 8f, 28f);
            SingleLine(subtitle);

            // Hàng nút dưới-phải: nút ẩn thì các nút còn lại dồn sang phải.
            RectTransform buttonsNode = Node("Buttons", row, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-8f, 6f), new Vector2(-68f, 34f));
            var layout = buttonsNode.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = 6f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var buttons = new Button[ItemRowButtons];
            var labels = new TMP_Text[ItemRowButtons];
            for (int i = 0; i < ItemRowButtons; i++)
            {
                RectTransform node = Create($"Button{i + 1}", buttonsNode);
                node.sizeDelta = new Vector2(92f, 34f);
                buttons[i] = MakeButton(node, "", 13f, i == 0 ? Green : Wood, Color.white, out labels[i]);
                SingleLine(labels[i]);
                node.gameObject.SetActive(false);
            }

            Wire(script, ("_background", background), ("_icon", icon), ("_title", title), ("_subtitle", subtitle));
            var so = new SerializedObject(script);
            SetArray(so.FindProperty("_buttons"), buttons);
            SetArray(so.FindProperty("_labels"), labels);
            so.ApplyModifiedPropertiesWithoutUndo();
            return script;
        }

        // Khối dọc lồng trong danh sách cuộn (để 2 danh sách Cần / Mồi nằm chung 1 trang).
        private static RectTransform BuildStack(RectTransform parent, string name)
        {
            RectTransform stack = Stretch(name, parent);
            var layout = stack.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            return stack;
        }

        private static RodListView AddRodList(RectTransform container, GameBootstrap game, Sprite fallbackIcon, Toast toast)
        {
            FishingItemRow template = BuildItemRowTemplate(container);
            template.gameObject.SetActive(false);
            var list = container.gameObject.AddComponent<RodListView>();
            Wire(list, ("_game", game), ("_content", container), ("_rowTemplate", template), ("_fallbackIcon", fallbackIcon), ("_toast", toast));
            return list;
        }

        private static BaitListView AddBaitList(RectTransform container, GameBootstrap game, FishingController controller,
            Sprite fallbackIcon, Toast toast)
        {
            FishingItemRow template = BuildItemRowTemplate(container);
            template.gameObject.SetActive(false);
            var list = container.gameObject.AddComponent<BaitListView>();
            Wire(list, ("_game", game), ("_controller", controller), ("_content", container), ("_rowTemplate", template),
                ("_fallbackIcon", fallbackIcon), ("_toast", toast));
            return list;
        }

        private static RodPanel BuildRodPanel(RectTransform parent, GameBootstrap game, Sprite fallbackIcon, Toast toast)
        {
            RectTransform root = Stretch("RodPanel", parent);
            BottomSheet sheetScript = BuildSheet(root, "Cần câu", out RectTransform sheet);
            var panel = root.gameObject.AddComponent<RodPanel>();
            BuildList(sheet, out RectTransform content);
            AddRodList(content, game, fallbackIcon, toast);
            Wire(panel, ("_sheet", sheetScript));
            return panel;
        }

        private static BaitPanel BuildBaitPanel(RectTransform parent, GameBootstrap game, FishingController controller,
            Sprite fallbackIcon, Toast toast)
        {
            RectTransform root = Stretch("BaitPanel", parent);
            BottomSheet sheetScript = BuildSheet(root, "Mồi câu", out RectTransform sheet);
            var panel = root.gameObject.AddComponent<BaitPanel>();
            BuildList(sheet, out RectTransform content);
            AddBaitList(content, game, controller, fallbackIcon, toast);
            Wire(panel, ("_sheet", sheetScript));
            return panel;
        }

        private const float TabRowTop = 50f;
        private const float TabRowHeight = 36f;
        // Trang dưới hàng tab: chừa tiêu đề (52) + hàng tab.
        private const float TabbedPageTop = TabRowTop + TabRowHeight + 8f;

        // Hàng nút chuyển trang ngay dưới tiêu đề bảng, chia đều bề ngang. Nút đầu xanh (trang đang mở), còn lại nâu.
        private static Button[] BuildTabRow(RectTransform sheet, params string[] names)
        {
            RectTransform row = Node("Tabs", sheet, new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f),
                new Vector2(0f, -TabRowTop), new Vector2(-24f, TabRowHeight));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            var buttons = new Button[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                RectTransform node = Create(names[i], row);
                buttons[i] = MakeButton(node, names[i], 14f, i == 0 ? Green : Wood, Color.white, out TMP_Text label);
                SingleLine(label);
            }
            return buttons;
        }

        // Trang trống phủ phần dưới hàng tab của bảng.
        private static RectTransform BuildTabPage(RectTransform sheet, string name)
        {
            RectTransform page = Stretch(name, sheet);
            Inset(page, 0f, 0f, 0f, TabbedPageTop);
            return page;
        }

        private static ShopTabs WireTabs(RectTransform sheet, Button[] tabs, params GameObject[] views)
        {
            var script = tabs[0].transform.parent.gameObject.AddComponent<ShopTabs>();
            var so = new SerializedObject(script);
            SetArray(so.FindProperty("_tabs"), tabs);
            SetArray(so.FindProperty("_views"), views);
            so.ApplyModifiedPropertiesWithoutUndo();
            return script;
        }

        // Đưa danh sách sẵn có của bảng (Scroll, nhãn Empty…) vào một trang dưới hàng tab.
        private static RectTransform WrapIntoPage(RectTransform sheet, string pageName, params string[] childNames)
        {
            RectTransform page = BuildTabPage(sheet, pageName);
            foreach (string childName in childNames)
            {
                Transform child = sheet.Find(childName);
                if (child == null)
                    continue;
                var rect = (RectTransform)child;
                // Trẻ con đang neo theo Sheet với lề trên 56 (dưới tiêu đề): sang trang thì lề trên còn 0.
                rect.SetParent(page, false);
                rect.offsetMax = new Vector2(rect.offsetMax.x, 0f);
            }
            return page;
        }

        // Cửa hàng: "Hạt giống | Cần câu | Mồi câu" (cần / mồi dùng chung list view với bảng ở vùng câu).
        private static void BuildShopTabs(ShopPanel shop, GameBootstrap game, Toast toast)
        {
            var sheet = shop.transform.Find("Sheet") as RectTransform;
            if (sheet == null || sheet.Find("Scroll") == null)
            {
                Debug.LogWarning("Tạo UI: không thấy Sheet / Scroll của Cửa hàng, bỏ qua tab đồ câu.");
                return;
            }
            var controller = Object.FindFirstObjectByType<FishingController>();
            Button[] tabs = BuildTabRow(sheet, "Hạt giống", "Cần câu", "Mồi câu");
            RectTransform seedsPage = WrapIntoPage(sheet, "SeedsPage", "Scroll");

            RectTransform rodPage = BuildTabPage(sheet, "RodPage");
            BuildList(rodPage, out RectTransform rodContent);
            ((RectTransform)rodContent.parent.parent).offsetMax = new Vector2(-8f, 0f);
            AddRodList(rodContent, game, LoadUiIcon(SunnysideUiImporter.RodIcon), toast);
            rodPage.gameObject.SetActive(false);

            RectTransform baitPage = BuildTabPage(sheet, "BaitPage");
            BuildList(baitPage, out RectTransform baitContent);
            ((RectTransform)baitContent.parent.parent).offsetMax = new Vector2(-8f, 0f);
            AddBaitList(baitContent, game, controller, LoadUiIcon(SunnysideUiImporter.BaitIcon), toast);
            baitPage.gameObject.SetActive(false);

            WireTabs(sheet, tabs, seedsPage.gameObject, rodPage.gameObject, baitPage.gameObject);
        }

        // Kho: "Nông sản | Giỏ cá" – giỏ dùng chung BasketView với bảng Giỏ ở vùng câu (bán, thả, mở ô).
        private static void BuildStorageTabs(StoragePanel storage, GameBootstrap game, Toast toast)
        {
            var sheet = storage.transform.Find("Sheet") as RectTransform;
            if (sheet == null || sheet.Find("Scroll") == null)
            {
                Debug.LogWarning("Tạo UI: không thấy Sheet / Scroll của Kho, bỏ qua tab Giỏ cá.");
                return;
            }
            Button[] tabs = BuildTabRow(sheet, "Nông sản", "Giỏ cá");
            RectTransform cropsPage = WrapIntoPage(sheet, "CropsPage", "Scroll", "Empty");
            RectTransform fishPage = BuildTabPage(sheet, "FishPage");
            BuildBasketView(fishPage, game, toast);
            fishPage.gameObject.SetActive(false);
            WireTabs(sheet, tabs, cropsPage.gameObject, fishPage.gameObject);
        }

        private const int MaxBasketCells = 25;
        private const float BasketCellSize = 64f;

        // Bảng Giỏ ở vùng câu: BottomSheet + BasketView dưới tiêu đề.
        private static BasketPanel BuildBasketPanel(RectTransform parent, GameBootstrap game, Toast toast)
        {
            RectTransform root = Stretch("BasketPanel", parent);
            BottomSheet sheetScript = BuildSheet(root, "Giỏ cá", out RectTransform sheet);
            var panel = root.gameObject.AddComponent<BasketPanel>();
            RectTransform page = Stretch("Basket", sheet);
            Inset(page, 0f, 0f, 0f, 52f);
            BuildBasketView(page, game, toast);
            Wire(panel, ("_sheet", sheetScript));
            return panel;
        }

        // Giỏ cá dùng chung: hàng trên (số cá, Bán hết), khung chi tiết (Bán, Thả), lưới 5 cột ô vuông tới 25 ô cuộn dọc,
        // hộp xác nhận (mở ô / bán hết) phủ trên cùng.
        private static BasketView BuildBasketView(RectTransform page, GameBootstrap game, Toast toast)
        {
            var view = page.gameObject.AddComponent<BasketView>();

            RectTransform top = Node("Top", page, new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f),
                new Vector2(0f, -2f), new Vector2(-16f, 36f));
            TMP_Text capacity = AddLabel(top, "Capacity", "Giỏ 0/8", 17f, TextAlignmentOptions.MidlineLeft, TextDark);
            Inset(capacity.rectTransform, 4f, 0f, 150f, 0f);
            SingleLine(capacity);
            RectTransform sellAllNode = Node("SellAll", top, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                Vector2.zero, new Vector2(140f, 34f));
            Button sellAll = MakeButton(sellAllNode, "Bán hết", 14f, Green, Color.white, out TMP_Text sellAllLabel);
            SingleLine(sellAllLabel);

            // Khung chi tiết cá đang chọn: icon, tên, 2 dòng thông tin; Bán (trên) / Thả (dưới) bên phải.
            RectTransform detailArea = Node("DetailArea", page, new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f),
                new Vector2(0f, -42f), new Vector2(-16f, 80f));
            TMP_Text hint = AddLabel(detailArea, "Hint", "Chạm vào cá để xem", 15f, TextAlignmentOptions.Center, TextDark);
            RectTransform detail = Stretch("Detail", detailArea);
            Frame(detail, Skin.Paper, CreamDark, false);
            RectTransform detailIconNode = Node("Icon", detail, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(10f, 0f), new Vector2(48f, 48f));
            Image detailIcon = AddImage(detailIconNode, Color.white, false);
            detailIcon.preserveAspect = true;
            TMP_Text detailName = AddLabel(detail, "Name", "", 17f, TextAlignmentOptions.BottomLeft, TextDark);
            Inset(detailName.rectTransform, 68f, 46f, 100f, 6f);
            SingleLine(detailName);
            TMP_Text detailInfo = AddLabel(detail, "Info", "", 13f, TextAlignmentOptions.TopLeft, TextDark);
            Inset(detailInfo.rectTransform, 68f, 4f, 100f, 36f);
            detailInfo.richText = true;
            RectTransform sellNode = Node("Sell", detail, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-8f, -6f), new Vector2(86f, 32f));
            Button sell = MakeButton(sellNode, "Bán", 14f, Green, Color.white, out TMP_Text sellLabel);
            SingleLine(sellLabel);
            RectTransform releaseNode = Node("Release", detail, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-8f, 6f), new Vector2(86f, 32f));
            Button release = MakeButton(releaseNode, "Thả", 14f, Wood, Color.white, out _);
            detail.gameObject.SetActive(false);

            // Lưới cuộn dưới khung chi tiết.
            RectTransform scrollNode = Stretch("Grid", page);
            Inset(scrollNode, 8f, 8f, 8f, 130f);
            var scroll = scrollNode.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            RectTransform viewport = Stretch("Viewport", scrollNode);
            AddImage(viewport, new Color(1f, 1f, 1f, 0f), true);
            viewport.gameObject.AddComponent<RectMask2D>();
            RectTransform content = Node("Content", viewport, new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(BasketCellSize, BasketCellSize);
            grid.spacing = new Vector2(8f, 8f);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;

            var lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FishingPlaceholderArt.LockPath);
            if (lockSprite == null)
                Debug.LogWarning("Tạo UI: chưa có icon ổ khóa giỏ. Chạy Về Quê → Câu cá: tạo art tạm rồi Tạo UI lại.");
            var cells = new FishCell[MaxBasketCells];
            for (int i = 0; i < MaxBasketCells; i++)
                cells[i] = BuildFishCell(content, i, lockSprite);

            // Hộp xác nhận: nền tối phủ cả màn hình, hộp giữa.
            RectTransform confirmRoot = Stretch("Confirm", page);
            confirmRoot.offsetMin = new Vector2(-400f, -400f);
            confirmRoot.offsetMax = new Vector2(400f, 400f);
            AddImage(confirmRoot, Dim, true);
            RectTransform confirmBox = Node("Box", confirmRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(290f, 170f));
            Frame(confirmBox, Skin.Cream, Cream, true);
            TMP_Text confirmText = AddLabel(confirmBox, "Text", "", 17f, TextAlignmentOptions.Center, TextDark);
            Inset(confirmText.rectTransform, 12f, 66f, 12f, 12f);
            RectTransform cancelNode = Node("Cancel", confirmBox, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1f, 0f),
                new Vector2(-6f, 14f), new Vector2(110f, ButtonHeight));
            Button confirmCancel = MakeButton(cancelNode, "Hủy", 17f, Wood, Color.white, out _, Icon.Cancel);
            RectTransform okNode = Node("Ok", confirmBox, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f),
                new Vector2(6f, 14f), new Vector2(110f, ButtonHeight));
            Button confirmOk = MakeButton(okNode, "Đồng ý", 17f, Green, Color.white, out TMP_Text confirmOkLabel, Icon.Confirm);
            confirmRoot.gameObject.SetActive(false);

            Wire(view, ("_game", game), ("_capacity", capacity), ("_sellAll", sellAll), ("_sellAllLabel", sellAllLabel),
                ("_detail", detail.gameObject), ("_hint", hint.gameObject), ("_detailIcon", detailIcon), ("_detailName", detailName),
                ("_detailInfo", detailInfo), ("_sell", sell), ("_sellLabel", sellLabel), ("_release", release),
                ("_confirmRoot", confirmRoot.gameObject), ("_confirmText", confirmText), ("_confirmOk", confirmOk),
                ("_confirmOkLabel", confirmOkLabel), ("_confirmCancel", confirmCancel), ("_toast", toast));
            var so = new SerializedObject(view);
            SetArray(so.FindProperty("_cells"), cells);
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        // Ô vuông: icon cá (hoặc ổ khóa) phía trên, chữ nhỏ ở đáy (cân nặng / giá mở ô).
        private static FishCell BuildFishCell(RectTransform content, int index, Sprite lockSprite)
        {
            RectTransform cell = Create($"Cell{index + 1}", content);
            Image background = Frame(cell, Skin.Paper, CreamDark, true);
            var button = cell.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            RectTransform iconNode = Node("Icon", cell, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -6f), new Vector2(36f, 36f));
            Image icon = AddImage(iconNode, Color.white, false);
            icon.preserveAspect = true;
            Image lockImage = null;
            RectTransform lockNode = Node("Lock", cell, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -10f), lockSprite != null ? lockSprite.rect.size * UiPixelScale : new Vector2(20f, 24f));
            lockImage = AddImage(lockNode, Color.white, false);
            lockImage.sprite = lockSprite;
            lockImage.enabled = false;
            TMP_Text weight = AddLabel(cell, "Weight", "", 11f, TextAlignmentOptions.Bottom, TextDark);
            Inset(weight.rectTransform, 2f, 5f, 2f, 42f);
            SingleLine(weight);
            var script = cell.gameObject.AddComponent<FishCell>();
            Wire(script, ("_background", background), ("_icon", icon), ("_weight", weight), ("_lock", lockImage), ("_button", button));
            return script;
        }

        // Chữ nổi trên đầu nhân vật (đánh giá ném, hụt cá): hộp neo giữa vùng, vị trí đặt theo camera mỗi khung hình.
        private static WorldPopText BuildWorldPopText(RectTransform area)
        {
            RectTransform holder = Stretch("PopText", area);
            var pop = holder.gameObject.AddComponent<WorldPopText>();
            RectTransform box = Node("Box", holder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f),
                Vector2.zero, new Vector2(200f, 30f));
            TMP_Text label = AddLabel(box, "Label", "", 22f, TextAlignmentOptions.Center, Color.white);
            SingleLine(label);
            Outlined(label);
            label.raycastTarget = false;
            box.gameObject.SetActive(false);

            var cameraController = Object.FindFirstObjectByType<CameraController>();
            Camera camera = cameraController != null ? cameraController.GetComponent<Camera>() : Camera.main;
            Wire(pop, ("_camera", camera), ("_area", holder), ("_box", box), ("_label", label));
            return pop;
        }


        // Bảng cá vừa câu được: tên, hình 4×, độ hiếm, cân nặng, giá, hạng kéo + XP, nút OK.
        private static void BuildCatchResultPanel(RectTransform parent, GameBootstrap game, FishingController controller)
        {
            RectTransform holder = Stretch("CatchResult", parent);
            var panel = holder.gameObject.AddComponent<CatchResultPanel>();

            RectTransform dim = Stretch("Root", holder);
            dim.offsetMin = new Vector2(-400f, -400f);
            dim.offsetMax = new Vector2(400f, 400f);
            AddImage(dim, Dim, true);

            RectTransform box = Node("Box", dim, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(300f, 356f));
            Frame(box, Skin.Cream, Cream, true);

            TMP_Text name = AddLabel(box, "Name", "", 24f, TextAlignmentOptions.Center, TextDark);
            Inset(name.rectTransform, 12f, 306f, 12f, 14f);
            SingleLine(name);

            // Hình cá 16×16 phóng 4 lần (bội số nguyên của pixel art).
            RectTransform iconNode = Node("Icon", box, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -56f), new Vector2(64f, 64f));
            Image icon = AddImage(iconNode, Color.white, false);
            icon.preserveAspect = true;

            // Đợt 2 Phần 18: "Đột biến: <kiểu>!" (+ "Lần đầu!") giữa hình cá và độ hiếm; màu đốm của kiểu, có viền; ẩn khi cá thường.
            TMP_Text shiny = AddLabel(box, "Shiny", "", 16f, TextAlignmentOptions.Center, RecordGold);
            Inset(shiny.rectTransform, 12f, 210f, 12f, 120f);
            SingleLine(shiny);
            Outlined(shiny);
            shiny.gameObject.SetActive(false);

            TMP_Text rarity = AddLabel(box, "Rarity", "", 18f, TextAlignmentOptions.Center, TextDark);
            Inset(rarity.rectTransform, 12f, 184f, 12f, 146f);
            SingleLine(rarity);
            TMP_Text weight = AddLabel(box, "Weight", "", 18f, TextAlignmentOptions.Center, TextDark);
            Inset(weight.rectTransform, 12f, 158f, 12f, 172f);
            SingleLine(weight);
            // Đợt E: "Loài mới!" / "Kỷ lục mới! (cũ …)" ngay dưới cân nặng, ẩn khi không phá kỷ lục.
            TMP_Text record = AddLabel(box, "Record", "", 18f, TextAlignmentOptions.Center, RecordGold);
            Inset(record.rectTransform, 12f, 132f, 12f, 198f);
            SingleLine(record);
            Outlined(record);
            record.gameObject.SetActive(false);
            TMP_Text price = AddLabel(box, "Price", "", 18f, TextAlignmentOptions.Center, TextDark);
            Inset(price.rectTransform, 12f, 106f, 12f, 224f);
            SingleLine(price);
            TMP_Text detail = AddLabel(box, "Detail", "", 14f, TextAlignmentOptions.Center, TextDark);
            Inset(detail.rectTransform, 12f, 78f, 12f, 254f);
            SingleLine(detail);

            RectTransform okNode = Node("Ok", box, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 16f), new Vector2(160f, ButtonHeight));
            Button ok = MakeButton(okNode, "OK", 18f, Green, Color.white, out _, Icon.Confirm);

            dim.gameObject.SetActive(false);
            Wire(panel, ("_game", game), ("_controller", controller), ("_root", dim.gameObject), ("_name", name), ("_icon", icon),
                ("_rarity", rarity), ("_weight", weight), ("_price", price), ("_detail", detail), ("_record", record), ("_shiny", shiny), ("_ok", ok));
        }

        // Bảng kéo cá (Đợt D, 1 lượt): thanh thời gian + chuỗi mũi tên phía trên, 4 nút mũi tên chữ thập ở giữa dưới.
        private const int MaxPullArrows = 10;
        // Đợt 2 Phần 6: chữ thập ở góc dưới phải (tay trái lật sang dưới trái), nút 64 (≥ 48 pt trên khung 390);
        // tâm cao hơn nút Thu cần (góc phải dưới cùng) để không chồng nhau. Thanh thời gian, chuỗi mũi tên đẩy lên trên chữ thập.
        private const float PullButtonSize = 64f;
        private const float PullPadCenterY = 160f;
        private const float PullPadStep = 70f;
        private const float PullPadInsetX = 118f;
        private const float PullTimeBarY = 290f;

        private static void BuildPullPanel(RectTransform parent, FishingController controller)
        {
            RectTransform holder = Stretch("FishingPull", parent);
            var panel = holder.gameObject.AddComponent<FishingPullPanel>();
            holder.gameObject.AddComponent<AudioSource>().playOnAwake = false;
            var sound = holder.gameObject.AddComponent<PullSound>();
            RectTransform root = Stretch("Root", holder);

            // Vùng vuốt (thử A/B cùng chữ thập): giữa màn hình, tránh mép dưới (thanh home iOS) và HUD trên cùng.
            // Tạo trước để chữ thập, chuỗi mũi tên nằm trên; ảnh trong suốt chỉ để nhận chạm.
            // TẠM TẮT (20/11/2026, người dùng yêu cầu): bỏ comment 4 dòng dưới và "_swipe" ở Wire cuối hàm để bật lại.
            // RectTransform swipeNode = Node("SwipeArea", root, new Vector2(0f, 0.38f), new Vector2(1f, 0.8f), new Vector2(0.5f, 0.5f),
            //     Vector2.zero, Vector2.zero);
            // AddImage(swipeNode, new Color(1f, 1f, 1f, 0f), true);
            // var swipe = swipeNode.gameObject.AddComponent<SwipePad>();

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
                new Vector2(0f, PullTimeBarY), new Vector2(300f, 16f));
            AddImage(timeBar, new Color(0.15f, 0.10f, 0.06f, 0.9f), false);
            RectTransform timeTrack = Stretch("Track", timeBar);
            Inset(timeTrack, 3f, 3f, 3f, 3f);
            RectTransform timeFill = Stretch("Fill", timeTrack);
            Image timeFillImage = AddImage(timeFill, Color.white, false);

            // Chuỗi mũi tên trên nền giấy, tự căn giữa theo số mũi tên đang hiện.
            RectTransform row = Node("Arrows", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, PullTimeBarY + 24f), new Vector2(MaxPullArrows * 30f + 16f, 40f));
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

            // "Sẵn sàng..." phía trên chuỗi mũi tên, chỉ hiện lúc xem trước.
            RectTransform readyNode = Node("Ready", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, PullTimeBarY + 68f), new Vector2(240f, 30f));
            TMP_Text readyLabel = AddLabel(readyNode, "Label", "Sẵn sàng...", 22f, TextAlignmentOptions.Center, Color.white);
            SingleLine(readyLabel);
            Outlined(readyLabel);
            readyNode.gameObject.SetActive(false);

            // Chữ thập: Lên, Xuống, Trái, Phải (thứ tự theo enum Arrow).
            Vector2[] offsets =
            {
                new Vector2(0f, PullPadStep), new Vector2(0f, -PullPadStep), new Vector2(-PullPadStep, 0f), new Vector2(PullPadStep, 0f),
            };
            RectTransform pad = Node("Pad", root, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(-PullPadInsetX, PullPadCenterY), Vector2.zero);
            var buttons = new HoldButton[4];
            for (int i = 0; i < 4; i++)
            {
                RectTransform node = Node($"Button{(VeQue.Logic.Arrow)i}", pad, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), offsets[i], new Vector2(PullButtonSize, PullButtonSize));
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
            Wire(panel, ("_controller", controller), ("_root", root.gameObject), ("_timeFill", timeFill), ("_timeFillImage", timeFillImage),
                ("_readyLabel", readyNode.gameObject), ("_pad", pad), /* ("_swipe", swipe), – vuốt tạm tắt */ ("_sound", sound));
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
            FishShadow shadow = EnsureFishShadow(owner.transform);
            // Hình cá bay lên: trên phao và "!" (sortingOrder 320); sprite đặt lúc chạy.
            SpriteRenderer flyerRenderer = EnsureWorldSprite(owner.transform, "CaughtFish", null, 320);
            var flyer = flyerRenderer.GetComponent<CaughtFishFlyer>();
            if (flyer == null)
                flyer = Undo.AddComponent<CaughtFishFlyer>(flyerRenderer.gameObject);
            var character = Object.FindFirstObjectByType<VeQue.View.Character.FarmCharacterController>();
            Wire(controller, ("_game", game), ("_travel", travel), ("_character", character), ("_bobber", bobber), ("_alert", alert),
                ("_shadow", shadow), ("_flyer", flyer));
            EditorUtility.SetDirty(controller);
            return controller;
        }

        // Bóng cá dưới phao (sau phao: sortingOrder 299). Sprite từ menu Câu cá: import sprite cá.
        private static FishShadow EnsureFishShadow(Transform parent)
        {
            Sprite[] small = FishingSpriteImporter.LoadShadowSprites(FishingSpriteImporter.SmallShadowPath);
            Sprite[] medium = FishingSpriteImporter.LoadShadowSprites(FishingSpriteImporter.MediumShadowPath);
            if (small[0] == null || medium[0] == null)
                Debug.LogWarning("Tạo UI: chưa có sprite bóng cá. Chạy Về Quê → Câu cá: import sprite cá rồi Tạo UI lại.");

            SpriteRenderer renderer = EnsureWorldSprite(parent, "FishShadow", small[0], 299);
            var shadow = renderer.GetComponent<FishShadow>();
            if (shadow == null)
                shadow = Undo.AddComponent<FishShadow>(renderer.gameObject);
            var so = new SerializedObject(shadow);
            SetArray(so.FindProperty("_smallSprites"), small);
            SetArray(so.FindProperty("_mediumSprites"), medium);
            so.ApplyModifiedPropertiesWithoutUndo();
            return shadow;
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
                Debug.LogWarning($"Tạo UI: chưa có {CharacterSkinPath}; chạy Về Quê → Import art → Nhân vật Sunnyside rồi Tạo UI lại.");
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

        // ---------- Hộp thư (Đợt 2 Phần 3) ----------

        private const float MailRowHeight = 70f;
        private const int MailRewardSlots = 4;

        // Nút "Thư" cột trái HUD (dưới Đi câu) + chấm đỏ đếm thư cần chú ý.
        private static Button BuildMailButton(RectTransform parent, GameBootstrap game)
        {
            Button button = BuildRegionButton(parent, "MailButton", "Thư", 2);
            var node = (RectTransform)button.transform;
            RectTransform badge = Node("Badge", node, Vector2.one, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-4f, -4f), new Vector2(22f, 22f));
            AddImage(badge, UrgentText, false);
            TMP_Text count = AddLabel(badge, "Count", "", 12f, TextAlignmentOptions.Center, Color.white);
            Inset(count.rectTransform, 0f, 0f, 0f, 0f);
            SingleLine(count);
            badge.gameObject.SetActive(false);
            var script = button.gameObject.AddComponent<MailButton>();
            Wire(script, ("_game", game), ("_badge", badge.gameObject), ("_badgeCount", count));
            return button;
        }

        // Bảng Hộp thư: danh sách (mới nhất trên cùng) + "Nhận tất cả" trên thanh tiêu đề; lớp Chi tiết phủ lên danh sách.
        private static MailPanel BuildMailPanel(RectTransform parent, GameBootstrap game)
        {
            RectTransform root = Stretch("MailPanel", parent);
            BottomSheet sheetScript = BuildSheet(root, "Hộp thư", out RectTransform sheet);
            var panel = root.gameObject.AddComponent<MailPanel>();

            RectTransform claimAllNode = Node("ClaimAll", sheet, Vector2.one, Vector2.one, Vector2.one,
                new Vector2(-(ButtonHeight + 12f), -4f), new Vector2(140f, ButtonHeight));
            Button claimAll = MakeButton(claimAllNode, "Nhận tất cả", 15f, Green, Color.white, out _);

            BuildList(sheet, out RectTransform content);
            MailRow template = BuildMailRow(content);

            TMP_Text empty = AddLabel(sheet, "Empty", "Chưa có thư", 16f, TextAlignmentOptions.Center, TextDark);
            Inset(empty.rectTransform, 12f, 60f, 12f, 60f);
            empty.gameObject.SetActive(false);

            // Chi tiết thư: phủ vùng danh sách.
            RectTransform detail = Stretch("Detail", sheet);
            Inset(detail, 8f, 8f, 8f, 56f);
            Frame(detail, Skin.Paper, CreamDark, true);

            RectTransform backNode = Node("Back", detail, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(6f, -6f), new Vector2(110f, ButtonHeight));
            Button back = MakeButton(backNode, "Quay lại", 15f, Wood, Color.white, out _);

            TMP_Text sender = AddLabel(detail, "Sender", "", 14f, TextAlignmentOptions.MidlineRight, TextDark);
            Inset(sender.rectTransform, 124f, 0f, 12f, 0f);
            sender.rectTransform.anchorMin = new Vector2(0f, 1f);
            sender.rectTransform.anchorMax = Vector2.one;
            sender.rectTransform.pivot = new Vector2(0.5f, 1f);
            sender.rectTransform.anchoredPosition = new Vector2(56f, -16f);
            sender.rectTransform.sizeDelta = new Vector2(-136f, 24f);
            SingleLine(sender);

            TMP_Text title = AddLabel(detail, "Title", "", 18f, TextAlignmentOptions.MidlineLeft, TextDark);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -58f);
            title.rectTransform.sizeDelta = new Vector2(-24f, 28f);
            SingleLine(title);

            TMP_Text body = AddLabel(detail, "Body", "", 14f, TextAlignmentOptions.TopLeft, TextDark);
            Inset(body.rectTransform, 12f, 124f, 12f, 92f);
            body.textWrappingMode = TextWrappingModes.Normal;
            body.overflowMode = TextOverflowModes.Ellipsis;

            var rewardRoots = new GameObject[MailRewardSlots];
            var rewardIcons = new Image[MailRewardSlots];
            var rewardLabels = new TMP_Text[MailRewardSlots];
            for (int i = 0; i < MailRewardSlots; i++)
            {
                RectTransform slot = Node($"Reward{i + 1}", detail, Vector2.zero, Vector2.zero, Vector2.zero,
                    new Vector2(10f + i * 88f, 66f), new Vector2(84f, 48f));
                RectTransform iconNode = Node("Icon", slot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    Vector2.zero, new Vector2(28f, 28f));
                rewardIcons[i] = AddImage(iconNode, Color.white, false);
                rewardIcons[i].preserveAspect = true;
                rewardLabels[i] = AddLabel(slot, "Label", "", 12f, TextAlignmentOptions.Bottom, TextDark);
                Inset(rewardLabels[i].rectTransform, 0f, 0f, 0f, 28f);
                SingleLine(rewardLabels[i]);
                rewardRoots[i] = slot.gameObject;
            }

            RectTransform claimNode = Node("Claim", detail, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 10f), new Vector2(160f, ButtonHeight));
            Button claim = MakeButton(claimNode, "Nhận", 17f, Green, Color.white, out TMP_Text claimLabel, Icon.Confirm);
            detail.gameObject.SetActive(false);

            var so = new SerializedObject(panel);
            so.FindProperty("_game").objectReferenceValue = game;
            so.FindProperty("_sheet").objectReferenceValue = sheetScript;
            so.FindProperty("_content").objectReferenceValue = content;
            so.FindProperty("_rowTemplate").objectReferenceValue = template;
            so.FindProperty("_emptyLabel").objectReferenceValue = empty;
            so.FindProperty("_claimAll").objectReferenceValue = claimAll;
            so.FindProperty("_detail").objectReferenceValue = detail.gameObject;
            so.FindProperty("_back").objectReferenceValue = back;
            so.FindProperty("_detailSender").objectReferenceValue = sender;
            so.FindProperty("_detailTitle").objectReferenceValue = title;
            so.FindProperty("_detailBody").objectReferenceValue = body;
            SetArray(so.FindProperty("_rewardRoots"), rewardRoots);
            SetArray(so.FindProperty("_rewardIcons"), rewardIcons);
            SetArray(so.FindProperty("_rewardLabels"), rewardLabels);
            so.FindProperty("_claim").objectReferenceValue = claim;
            so.FindProperty("_claimLabel").objectReferenceValue = claimLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
            return panel;
        }

        // Dòng mẫu: người gửi (trái) + trạng thái (phải) ở trên, tiêu đề giữa, tóm tắt quà dưới. Cả dòng là nút.
        private static MailRow BuildMailRow(RectTransform content)
        {
            RectTransform row = Stretch("MailRowTemplate", content);
            Image background = Frame(row, Skin.Paper, CreamDark, true);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = MailRowHeight;
            Button button = row.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            var script = row.gameObject.AddComponent<MailRow>();

            TMP_Text sender = AddLabel(row, "Sender", "", 13f, TextAlignmentOptions.MidlineLeft, TextDark);
            TopRow(sender.rectTransform, 4f, 18f, 12f, 90f);
            SingleLine(sender);
            TMP_Text status = AddLabel(row, "Status", "", 13f, TextAlignmentOptions.MidlineRight, UrgentText);
            TopRow(status.rectTransform, 4f, 18f, 160f, 12f);
            SingleLine(status);
            TMP_Text title = AddLabel(row, "Title", "", 16f, TextAlignmentOptions.MidlineLeft, TextDark);
            TopRow(title.rectTransform, 22f, 24f, 12f, 12f);
            SingleLine(title);
            TMP_Text rewards = AddLabel(row, "Rewards", "", 13f, TextAlignmentOptions.MidlineLeft, TextDark);
            TopRow(rewards.rectTransform, 46f, 18f, 12f, 12f);
            SingleLine(rewards);

            Wire(script, ("_button", button), ("_background", background), ("_sender", sender), ("_title", title),
                ("_rewards", rewards), ("_status", status));
            bool framed = background.sprite != null;
            var so = new SerializedObject(script);
            so.FindProperty("_attentionColor").colorValue = framed ? Color.white : Cream;
            so.FindProperty("_doneColor").colorValue = framed ? new Color(0.82f, 0.80f, 0.76f) : CreamDark;
            so.ApplyModifiedPropertiesWithoutUndo();
            return script;
        }

        // Nhãn neo trên cùng: cách mép trên top, cao height, cách trái left, cách phải right.
        private static void TopRow(RectTransform rect, float top, float height, float left, float right)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2((left - right) * 0.5f, -top);
            rect.sizeDelta = new Vector2(-(left + right), height);
        }

        private const int OrderCards = 5;
        private const int OrderItemSlots = 3;
        // Đợt 2 Phần 11: +40 cho câu thoại 2 dòng (116 → 156).
        private const float OrderCardHeight = 156f;
        private const float OrderSpeechHeight = 38f;

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

        // Hàng trên: chân dung / chữ cái + người đặt (trái), nhãn khẩn (phải). Dưới: câu thoại 2 dòng.
        // Hàng giữa: tối đa 3 món. Hàng dưới: thưởng (trái), Bỏ + Giao (phải). Bong bóng cảm ơn đè lên đầu thẻ.
        private static OrderCard BuildOrderCard(RectTransform content, int index)
        {
            RectTransform card = Stretch($"Order{index + 1}", content);
            Image background = Frame(card, Skin.Paper, CreamDark, true);
            card.gameObject.AddComponent<LayoutElement>().preferredHeight = OrderCardHeight;
            var script = card.gameObject.AddComponent<OrderCard>();

            RectTransform body = Stretch("Content", card);
            // Chân dung 16×16 phóng ×2 (chưa có art: khung tròn màu + chữ cái đầu tên).
            RectTransform portraitNode = Node("Portrait", body, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -3f), new Vector2(32f, 32f));
            Image portrait = AddImage(portraitNode, Color.white, false);
            portrait.preserveAspect = true;
            TMP_Text initial = AddLabel(portraitNode, "Initial", "", 16f, TextAlignmentOptions.Center, TextDark);
            Inset(initial.rectTransform, 0f, 0f, 0f, 0f);
            SingleLine(initial);

            TMP_Text customer = AddLabel(body, "Customer", "", 17f, TextAlignmentOptions.MidlineLeft, TextDark);
            customer.rectTransform.anchorMin = new Vector2(0f, 1f);
            customer.rectTransform.anchorMax = Vector2.one;
            customer.rectTransform.pivot = new Vector2(0.5f, 1f);
            // Trái 46 (sau chân dung), phải 10.
            customer.rectTransform.anchoredPosition = new Vector2(18f, -6f);
            customer.rectTransform.sizeDelta = new Vector2(-56f, 26f);

            TMP_Text speech = AddLabel(body, "Speech", "", 13f, TextAlignmentOptions.TopLeft, TextDark);
            speech.rectTransform.anchorMin = new Vector2(0f, 1f);
            speech.rectTransform.anchorMax = Vector2.one;
            speech.rectTransform.pivot = new Vector2(0.5f, 1f);
            speech.rectTransform.anchoredPosition = new Vector2(0f, -38f);
            speech.rectTransform.sizeDelta = new Vector2(-20f, OrderSpeechHeight);
            speech.textWrappingMode = TextWrappingModes.Normal;
            speech.overflowMode = TextOverflowModes.Ellipsis;
            speech.gameObject.SetActive(false);

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
                RectTransform item = Node($"Item{i + 1}", body, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f + i * 116f, -38f - OrderSpeechHeight), new Vector2(112f, 30f));
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

            // Bong bóng cảm ơn: con của thẻ (không phải Content) để vẫn hiện khi thẻ chuyển sang chờ đơn mới.
            RectTransform bubble = Node("ThanksBubble", card, new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f),
                new Vector2(0f, -4f), new Vector2(-16f, 52f));
            Frame(bubble, Skin.Cream, Cream, false);
            TMP_Text bubbleText = AddLabel(bubble, "Text", "", 14f, TextAlignmentOptions.Center, TextDark);
            Inset(bubbleText.rectTransform, 10f, 4f, 10f, 4f);
            bubbleText.textWrappingMode = TextWrappingModes.Normal;
            bubbleText.overflowMode = TextOverflowModes.Ellipsis;
            bubble.gameObject.SetActive(false);

            var so = new SerializedObject(script);
            so.FindProperty("_portrait").objectReferenceValue = portrait;
            so.FindProperty("_initial").objectReferenceValue = initial;
            so.FindProperty("_speech").objectReferenceValue = speech;
            so.FindProperty("_bubble").objectReferenceValue = bubble.gameObject;
            so.FindProperty("_bubbleText").objectReferenceValue = bubbleText;
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

        // Thanh tab dưới: Đơn hàng | Kho | Cửa hàng (bỏ tab Nông trại 13/11/2026 – chỉ đóng bảng, đã có X / chạm nền).
        private static void BuildNav(RectTransform parent, out Button orders, out TMP_Text ordersLabel,
            out Button storage, out Button shop)
        {
            RectTransform nav = Node("Nav", parent, Vector2.zero, new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, NavHeight));
            Frame(nav, Skin.Sand, Wood, true);
            var layout = nav.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 6, 6);
            layout.spacing = 3f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            orders = MakeTab(nav, "Đơn hàng", out ordersLabel);
            storage = MakeTab(nav, "Kho", out _);
            shop = MakeTab(nav, "Cửa hàng", out _);
        }

        // 3 tab trên bề ngang 390: chữ 15 để "Đơn hàng (3)" vừa một dòng.
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

            RectTransform badge = BuildFlyerBadge("HarvestBadge", storageTab, 4f, out TMP_Text text, out Image badgeIcon);
            // Nhãn mồi đào được khi thu hoạch (19/11/2026): chồng ngay trên nhãn nông sản.
            RectTransform baitBadge = BuildFlyerBadge("BaitBadge", storageTab, 4f + FlyerBadgeHeight + 2f, out TMP_Text baitText, out Image baitIcon);

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
            so.FindProperty("_baitBadge").objectReferenceValue = baitBadge.gameObject;
            so.FindProperty("_baitBadgeIcon").objectReferenceValue = baitIcon;
            so.FindProperty("_baitBadgeText").objectReferenceValue = baitText;
            so.ApplyModifiedPropertiesWithoutUndo();
            return flyer;
        }

        private const float FlyerBadgeHeight = 30f;

        // Nhãn "+N [icon]" trên tab Kho, đáy cách mép trên tab bottomOffset; tắt lúc đầu.
        private static RectTransform BuildFlyerBadge(string name, RectTransform storageTab, float bottomOffset, out TMP_Text text, out Image icon)
        {
            RectTransform badge = Node(name, storageTab, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f),
                new Vector2(0f, bottomOffset), new Vector2(76f, FlyerBadgeHeight));
            AddImage(badge, new Color(0.15f, 0.10f, 0.06f, 0.85f), false);
            badge.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;
            text = AddLabel(badge, "Text", "+0", 16f, TextAlignmentOptions.MidlineRight, Color.white);
            Inset(text.rectTransform, 4f, 0f, 36f, 0f);
            RectTransform iconNode = Node("Icon", badge, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(24f, 24f));
            icon = AddImage(iconNode, Color.white, false);
            icon.preserveAspect = true;
            badge.gameObject.SetActive(false);
            return badge;
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
