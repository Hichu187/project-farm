# Về Quê – Tiến độ

## Phần 0 – Dọn project & khung thư mục (XONG – 29/10/2026)

### Đã làm
- Tạo `CLAUDE.md` (luật làm việc cho agent).
- Bỏ theo dõi `UserSettings/` trong git (`git rm -r --cached`), file local giữ nguyên.
- Tạo `Assets/_Game/{Logic,View,Data,Art,Tests/EditMode}`.
- Asmdef: `VeQue.Logic` (No Engine References), `VeQue.View` (→ Logic), `VeQue.Tests` (Editor, NUnit, → Logic).
- Test khói `SmokeTests.LogicAssembly_IsReferenced` + class đánh dấu `LogicAssembly`.
- Preset `Assets/_Game/Data/Presets/SpritePreset.preset` (Sprite Single, PPU 16, Point, Compression None, tắt mipmap, tắt Physics Shape), đặt mặc định TextureImporter cho `glob:"Assets/_Game/Art/**"`.
- Xóa `Assets/Scripts/` (Test.cs rỗng).

### Quyết định
- Không thêm package: đã có `2d.pixel-perfect` 5.1.1, `test-framework` 1.6.0, `2d.tilemap` 1.0.0, `ugui` 2.0.0 (TMP).
- `VeQue.View` chưa tham chiếu `Unity.2D.PixelPerfect` / `Unity.TextMeshPro`; thêm ở Phần 4 / Phần 6.
- Preset chỉ tự áp cho ảnh MỚI import vào `_Game/Art`; không áp tay lên sheet đã cắt (sẽ mất slice).
- Asset gốc Sprout Lands giữ ở `Assets/Art`; bản đã cắt gọn đưa vào `Assets/_Game/Art` ở Phần 3.
- Mốc Build Report WebGL (28/10/2026, sau Phần 0): 10,8 MB đã nén, Gzip, Release (mục tiêu < 10 MB).
  - `wasm.gz` 8,45 MB · `data.gz` 2,24 MB · `framework.js.gz` 85 KB · `loader.js` 26 KB.
  - Tạm giữ nguyên. Hướng giảm (Phần 6/8): Brotli, Managed Stripping High, Input chỉ dùng Input System mới, tắt splash, bỏ LiberationSans/EmojiOne của TMP.

### Kiểm tra
- [x] Project compile không lỗi.
- [x] Test Runner thấy `VeQue.Tests.dll`, `SmokeTests` xanh (1/1).
- [x] Mốc build WebGL 10,8 MB.
- [x] Preset mặc định đã lưu trong `ProjectSettings/PresetManager.asset`.

### Còn lại
- Chưa quyết: gỡ package không dùng (spriteshape, psdimporter, aseprite, 2d.animation).
- ~~Xóa `LogicAssembly` khi Phần 1 có class thật.~~ (đã xóa ở Phần 1)

## Phần 1 – Logic thuần + unit test (XONG – 29/10/2026; sửa theo mô hình luống 02/11/2026, xem mục Map D)

### Đã làm
- `Assets/_Game/Logic/`: `CropDef`, `ZoneDef`, `FarmDef`, `PlotId`, `PlotState` (+ `PlotStatus`), `ZoneState`, `FarmState`, `IClock` (+ `SystemClock`), `FarmResult`, `Farm`.
- `Assets/_Game/Tests/EditMode/`: `FakeClock`, `FarmTests` (kích thước 3×4, 4×5, 5×6). Test khói trỏ sang `Farm`; xóa `LogicAssembly`.

### Quyết định
- Thứ tự mở ô: theo hàng, từ góc trên-trái (row 0 = hàng trên cùng), chỉ số = `row * Width + col`.
- `StartOpenCount` = số ô mở miễn phí khi khu được mở khóa; Khu 2, 3 dùng 1 (đặt ở data Phần 2).
- `CropDef.HarvestYield` (≥ 1) = số nông sản mỗi lần thu hoạch.
- Giá `int`, xu `long`.
- Đồng hồ lùi → coi như 0 giây trôi qua (không âm). Chống chỉnh giờ đầy đủ để Phần 7.
- `UnlockNextPlot` luôn mở ô kế tiếp → bỏ mã `NotNextPlot`, thêm `ZoneFull`, `UnknownPlot`, `UnknownZone`, `NothingToHarvest`.
- Khi tạo `Farm` từ state đã lưu: kiểm tra khớp data (số khu, id, kích thước) và cấp lại ô miễn phí còn thiếu.

### Kiểm tra
- [x] Compile không lỗi.
- [x] Test Runner EditMode: 45/45 xanh (`FarmTests` 44, `SmokeTests` 1).

### Còn lại
- TODO(Phần 7): `FarmState` dùng `Dictionary` — `JsonUtility` không serialize được, cần chuyển đổi khi lưu.

## Phần 4 – Camera (TẠM ỔN trong Editor – làm trước Phần 1–3 theo yêu cầu; còn template WebGL + test máy thật)

### Đã làm
- `Assets/_Game/View/CameraControl/`: `CameraController`, `CameraTouchInput`, `FocusArea` (namespace `VeQue.View.CameraControl` — không dùng `...Camera` vì trùng tên `UnityEngine.Camera`).
- `VeQue.View.asmdef` thêm reference `Unity.2D.PixelPerfect`, `Unity.InputSystem`, `UnityEngine.UI` (cho `EventSystem`).
- 03/11/2026: lúc start, camera về tâm map (`CameraController.GetMapCenter()`); map 46×26 → (23, 13). Người dùng đã thử trong Editor, xác nhận ổn.
- 03/11/2026: chạm khu thì camera di chuyển trước (`_moveDuration` 0,25 giây) rồi mới zoom; `_focusRef` đổi sang 113×200. Người dùng xác nhận ổn trong Editor.

### Quyết định
- Tâm map tính từ `cellBounds` của Tilemap, không tính `_mapPadding` (padding có thể lệch giữa các cạnh). Chưa gán Tilemap thì giữ vị trí camera trong scene.
- Zoom có thời gian chuyển (`_transitionDuration`), 2 chế độ chọn bằng `_smoothZoom`:
  - Bật (mặc định): tạm tắt PixelPerfectCamera, nội suy `orthographicSize` theo 1/size; tới nơi bật lại ở bậc zoom nguyên. Mượt, pixel hơi lệch nét trong lúc chuyển.
  - Tắt: đi qua từng bậc zoom nguyên (Ref = màn hình / zoom). Luôn sắc nét, nhìn theo nấc.
- Đang zoom vào khu: chỉ kéo trong khu đó + padding từng cạnh (`FocusArea._padding`: Left/Right/Top/Bottom, mặc định 0) và vẫn trong mép map.
- Giới hạn camera theo `cellBounds` của Tilemap được gán + `_mapPadding` từng cạnh (dương = nới, âm = thu; struct dùng chung `EdgePadding`); vùng nhìn tự tính theo công thức của PixelPerfectCamera (Crop None).
- Input: `Pointer.current` (Input System mới), ngưỡng kéo 10 px, bỏ qua chạm bắt đầu trên UI.
- `FocusArea` là vùng tạm cho camera; Phần 3 gắn `ZoneAnchor` lên cùng GameObject.

### Còn lại
- Người dùng setup scene + thử trong Editor.
- Template WebGL (`touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`) — chưa làm.
- Kiểm tra trên iPhone Safari + Android; cân nhắc ngưỡng kéo theo `Screen.dpi`.
- Chốt 03/11/2026: tâm khu ngoài giới hạn ở toàn cảnh (Khu 3) thì đi hết mức, phần còn lại đi trong lúc zoom, để không lộ khoảng trống.
- Chốt 03/11/2026: giữ `_mapPadding` = 5/5/5/2,5, camera đi ra ngoài map là hiệu ứng người dùng muốn; không đổi về 0.

## Phần 2 – Data (ScriptableObject) (XONG – 29/10/2026; sửa theo mô hình luống 02/11/2026, xem mục Map D)

### Đã làm
- `Assets/_Game/View/Data/`: `CropDefinition`, `ZoneDefinition`, `FarmDefinition` (namespace `VeQue.View.Data`), mỗi cái có `ToDef()` sang class thuần.
- `Assets/_Game/Editor/`: asmdef `VeQue.Editor` (Editor only, namespace `VeQue.EditorTools` — tránh trùng `UnityEditor.Editor`) + `DefaultDataGenerator` (menu **Về Quê → Tạo data mặc định**). Chạy lại được: ghi số liệu, giữ sprite đã gán.
- `VeQue.Tests` thêm reference `VeQue.View`; `FarmDefinitionTests` đọc `Assets/_Game/Data/FarmDefinition.asset`.

### Quyết định
- 25 cây trong data, có cờ `enabled`; MVP bật 4: rau muống, cải xanh, cà chua, bí đỏ. Bật thêm khi có art, không cần sửa code.
- Thời gian lớn nhập theo phút (`growMinutes`), chuyển sang giây khi `ToDef()`.
- 3 khu 4×5. Khu 1 mở sẵn 4 ô (16 giá). Khu 2, 3 mở miễn phí 1 ô khi mở khóa (19 giá — bỏ giá cuối 2100 và 20400 của bảng gốc).
- Giá mở ô ≈ 40 × 1.12^(n−1), làm tròn; tổng Khu 1 / 2 / 3 ≈ 1.715 / 15.585 / 150.050 xu.
- Vốn ban đầu 60 xu + 5 hạt rau muống.

### Còn lại
- [x] Đã tạo data (25 cây, 3 khu, FarmDefinition); Test Runner 49/49 xanh.
- Sprite cây gán ở Phần 3.

02/11/2026 – Chốt Map D (trải ngang), khu = nhiều luống

## Thay đổi thiết kế Map D (02/11/2026)

### Quyết định
- Map 44×26 tile, trải ngang. Tọa độ trong `Docs/MVP-Plan.md` Phần 3 chỉ là tham chiếu; người dùng tự đặt vị trí cuối cùng trong scene.
- Mỗi khu = 2 luống 6×2, cách nhau 1 tile → 24 ô. Ô định danh bằng `(zoneId, bedIndex, col, row)`; hết luống này mới mở sang luống sau.
- Khu 1 mở sẵn 6 ô; Khu 2, 3 mở miễn phí 1 ô khi mở khóa → bảng giá mới có 18 / 23 / 23 giá. Thay cho số liệu 4 ô và 16 / 19 / 19 giá ghi ở Phần 2.
- Background trời làm sau, che khoảng trống trên/dưới map ở màn hình dài.
- Chạm ao, chuồng, nhà kho, đất mở rộng → hiện "sắp có" (làm ở Phần 6).

- Giá mở ô ≈ 40 × 1.102^(n−1), giữ giá cuối 18.200; tổng Khu 1 / 2 / 3 = 1.860 / 18.765 / 175.450 xu.

### Đã làm
- Code Logic, Data, test đã sửa theo mô hình luống (`BedDef`, `BedDefinition`, `PlotId.BedIndex`); menu tạo data giữ origin luống đã có.
- Đã tạo lại data; Test Runner EditMode 60/60 xanh (`FarmTests` 53, `FarmDefinitionTests` 6, `SmokeTests` 1).

### Còn lại
- Viết `ZoneAnchor`, sửa camera theo bounding box.
- TODO (sau MVP): nâng cấp ô đất.

## Phần 3 – Map & art (ĐANG LÀM)

### Đã làm
- Scene có Grid `Map` và 3 Tilemap `Ground`, `Decor`, `Plots`; `Ground` đã có tile.
- `Docs/huong-dan-setup-khu-trong.md` đã viết lại theo Map D.
- Tool Editor **Về Quê → Tạo map** (`MapPaintSettings`, `MapPainter`, `MapPaintWindow`): nhập kích thước rồi vẽ từng lớp: Ground (cỏ random theo Weight + seed, sân), Decor (hàng rào có **Fence Offset**, giữ phần vẽ tay), Plots (ô đất theo `Beds`). Có nút thêm nhiều tile cỏ cùng lúc.
- Người dùng đã vẽ Ground và hàng rào bằng tool, xác nhận tạm ổn (03/11/2026): map 46×26, bên trong rào 42×23 = 966 ô.
- 03/11/2026: `ZoneAnchor` (`Assets/_Game/View/Map/`, gắn cùng GameObject với `FocusArea`), người dùng xác nhận ổn trong Editor.
- 03/11/2026: đã vẽ `Plots` bằng tool: 72 ô, khớp `Beds` trong data; `Plots` về `x = 0`. Plot Tile tạm là `ground_dirt_orange_dark_34`.
- 03/11/2026: đã tạo 3 khu (`Zone`, `Zone (1)`, `Zone (2)`) + `FocusArea` 6×5 tại (13, 11), (21, 11), (13, 3); đã gán **Areas**, **Focus Ref** = (113, 200). Scene khớp; người dùng xác nhận ổn trong Editor.

### Còn lại
- Ô đất: người dùng sẽ setup sau. Sheet `ground_dirt_orange_dark.png` đang Bilinear + Normal Quality, cần đổi Point + None nếu giữ tile này.
- Vẽ tay phần còn lại trên `Decor`: ao, chuồng, kho, viền cỏ, cây, rơm.
- Mục kiểm tra còn lại của tool: Ctrl+Z.
- Kiểm tra Sprout Lands có sprite cho 4 cây MVP; cắt tileset/cây, Sprite Atlas, gán sprite vào `CropDefinition`.

## Phần 5 + 6 – Trồng cây, tưới, GUI (ĐANG LÀM, chia 3 đợt theo yêu cầu 03/11/2026)

### Quyết định
- Tưới: tùy chọn, mỗi lần trồng 1 lần, giảm 20% tổng thời gian lớn (`FarmDefinition._waterReducePercent`), chỉnh được trong data. Ô được tưới thu nhập cao hơn 25%, có thể cần chỉnh giá mở ô khi chơi thử.
- Chạm ô: khóa kế tiếp → xác nhận mở; trống → chọn hạt; chưa tưới → tưới; chín → thu hoạch.
- Font: tạm dùng LiberationSans mặc định của TMP (đủ 134 ký tự có dấu). Font pixel Sprout Lands thiếu toàn bộ ký tự có dấu.
- Layout GUI đã duyệt tạm: HUD (xu, sao lưu) trên cùng; tab Nông trại / Kho / Cửa hàng dưới cùng; Kho, Cửa hàng, chọn hạt là bảng trượt từ dưới lên; xác nhận mở ô ở giữa màn hình. Canvas "Về Quê – GUI Layouts" không có trong repo.
- Gieo nhiều ô: nhớ hạt vừa chọn, chạm ô trống khác gieo luôn; thanh mỏng trên tab hiện `Đang gieo: <cây> x<số> [Đổi]`.
- Sprite: tool gán cải xanh hàng 7, cà chua hàng 4, bí đỏ hàng 9 của `Farming Plants v2.png`, giai đoạn dùng cột 1–5. Rau muống người dùng tự gán, tool giữ nguyên (đang 3 sprite từ sheet đất ướt, hàng 1).
- Số giai đoạn theo từng cây = số phần tử `_stageSprites`. Người dùng được giữ cách cắt của mình; tool nhận cả cắt lưới và cắt tự động.
- Gốc cây nâng 5 pixel khỏi cạnh dưới ô (`FarmView._cropOffsetPixels`) để hạt nằm giữa ô; cây cao nhô lên ô trên.
- Ô đất khóa hiện bằng màu tối (`FarmView`), chưa cần tile riêng; có sẵn ô gán tile khi setup ô đất.

### Đợt 1 – Logic (XONG)
- `Farm.Water()`, `PlotState.Watered`, sự kiện `Farm.Changed`, mã `NothingToWater`, `AlreadyWatered`, `AlreadyRipe`; thêm 13 test. Người dùng đã chạy Test Runner, xác nhận ổn.

### Đợt 2 – Trồng cây (đã viết code, chờ setup và kiểm tra)
- `GameBootstrap`, `FarmView`, `FarmInteraction` (`Assets/_Game/View/Map/`); `ZoneDefinition.GetCell` / `TryGetPlotId`; `CropDefinition._wateredStageSprites`; menu **Về Quê → Gán sprite 4 cây**; `CropDef.StageCount` theo từng cây; thêm 5 test.

Người dùng xác nhận Đợt 2 ổn.

### Đợt 3 – GUI (đã viết code, chờ dựng và kiểm tra)
- `Assets/_Game/View/UI/`: `GameUI`, `SafeArea`, `Toast`, `BottomSheet`, `CropRow`, `CropListPanel` + `SeedPanel`, `StoragePanel`, `ShopPanel`, `UnlockDialog`, `SeedingBar`, `UiFormat`.
- `Farm.GetRemainingSeconds`, `CameraTouchInput.TappedOutsideAreas`, `ComingSoonArea`; `FarmInteraction` hỏi qua sự kiện, bỏ `_defaultSeed`. Asmdef `VeQue.View`, `VeQue.Editor` thêm `Unity.TextMeshPro`.
- Menu **Về Quê → Tạo UI** dựng toàn bộ GUI và nối tham chiếu; thêm 15 test (tổng 93).
- `CropProgressPopup`: chạm cây chưa chín (chưa tưới thì tưới trước) hiện thanh tiến độ + thời gian ngay trên cây; tự ẩn sau 3 giây, chạm cây khác thì chuyển sang cây đó và ẩn cây trước. Không hiện thời gian trên HUD.
- Thu hoạch vẫn báo `+2 Rau muống` ở thông báo đầu màn hình. Chưa làm hiệu ứng giọt nước khi tưới.

### Còn lại
- Để ngỏ (04/11/2026): tưới chưa thấy khác biệt trên hình. Rau muống đang dùng sprite đất ướt cho mọi giai đoạn; 3 cây còn lại chưa có sprite. Người dùng cân nhắc không dùng sprite đất ướt. Chưa sửa; khi quay lại cần chốt cách báo đã tưới và chỉnh `CropSpriteAssigner`.
- Hiệu ứng GUI mượt hơn: đã đưa vào `MVP-Plan.md` Phần 6 mục 4, người dùng sẽ làm sau, chưa cài DOTween.
- Chạy lại kiểm tra Đợt 3; thử chữ có dấu, safe-area trên iPhone; build lại và so Build Report (font, sprite mới; mốc 10,8 MB).
- Sau khi vẽ `Decor`: tạo `ComingSoonArea` cho ao, chuồng, nhà kho, đất mở rộng, gán vào `GameUI`.
- TODO: thay màu phẳng bằng khung, nút UI Pack (cần đặt Border 9-slice trong Sprite Editor).
- TODO: xem lại bố cục GUI sau khi chơi thử, người dùng có thể đổi.
- TODO: có canvas GUI thật hoặc font pixel thì thay vào.

## Tiếp theo (khi quay lại)
1. Phần 3 – art (gồm setup ô đất), vẽ tay phần còn lại trên `Decor`.
2. Phần 4 – sửa camera theo Map D, template WebGL (`touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`) + thử trên iPhone Safari / Android.
4. Chưa quyết: gỡ package không dùng (spriteshape, psdimporter, aseprite, 2d.animation); giảm build < 10 MB (Phần 6/8).
