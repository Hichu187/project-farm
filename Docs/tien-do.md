# Về Quê – Tiến độ

## Tóm tắt (cập nhật 07/11/2026)

| Phần | Trạng thái | Còn thiếu |
|---|---|---|
| 0 – Khung project | Xong | Chưa quyết gỡ 4 package không dùng |
| 1 – Logic + test | Xong | — |
| 2 – Data | Xong (giá cây, cấp, đơn hàng tính lại ở 6b) | — |
| 3 – Map & art | Đang làm | Art ô đất, `Decor` còn lại, `ComingSoonArea`, Sprite Atlas |
| 4 – Camera | Ổn trong Editor | Template WebGL, thử trên iPhone / Android |
| 5 – Ô đất & cây | Ổn trong Editor | Gán sprite đủ 4 cây, tile đất ướt (xác nhận lại) |
| 5b – Hoạt ảnh cây | Ổn trong Editor (Đợt A + B) | Khung hình: chờ art |
| 6 – UI | Ổn trong Editor | Mục 4 hiệu ứng GUI (làm sau); mục 5 chạm/kéo chờ kiểm tra; chữ có dấu + safe-area trên iPhone |
| 6b – Cấp độ, đơn hàng, cân bằng | Ổn trong Editor | Build + Build Report; công cụ Cân bằng (làm sau) |
| 6c – Việc làm lúc chờ cây lớn | Chưa làm (kế hoạch 8 mục, 07/11/2026) | Mục 1–6 trong MVP, 7–8 sau MVP; mỗi mục duyệt thiết kế trước |
| 10 – Nhân vật, vùng câu, câu cá, ao nhà | Trong MVP (07/11/2026), đã có thiết kế, chưa làm | Ném perfect, giật câu, kéo cá mũi tên 4 hướng, cá theo độ hiếm/cân nặng/giá/khung giờ, cần rank/độ bền/trượt/gãy, mồi; cần art cá, phao, cần, nước |
| 7 – Lưu tiến trình | Đã viết code (save `version` 2) | Thử trên máy thật, nhập mã máy khác, trình duyệt nhúng Facebook |
| 8 – Build & phát hành | Chưa làm | Toàn bộ |

Git: nhánh `setup/phan-0-camera`, đã push lên `origin`. Code mới nhất ở `e65601e` (Phần 5, 5b, 6 mục 5, 6b, 7); các commit sau chỉ là tài liệu kế hoạch Phần 6c, 10.
Test: lần gần nhất người dùng xác nhận trong Unity là 210 (sau Đợt 4 Phần 6b). Hiện có 221 test; ngoài Unity chạy được 164/164 xanh (phần không cần Unity).

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
- ~~Gieo nhiều ô: nhớ hạt vừa chọn, chạm ô trống khác gieo luôn; thanh `Đang gieo`.~~ Thay bằng chạm và kéo (Phần 6 mục 5, 07/11/2026).
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
- ~~Thu hoạch báo `+2 Rau muống`; chưa có giọt nước khi tưới.~~ Đã thay ở Phần 5b (icon bay về Kho, giọt nước). `SeedPanel`, `SeedingBar` đã xóa ở Phần 6 mục 5.

### Còn lại
- Chốt 04/11/2026: đã tưới thì đổi tile đất (`FarmView._wateredTile`), thu hoạch xong trở về tile cũ; bỏ `_wateredStageSprites`. Code đã sửa, chờ người dùng: gán Watered Tile, gán lại sprite rau muống sang sheet thường (đang lấy từ sheet đất ướt), chạy **Gán sprite 4 cây** cho 3 cây còn lại, rồi kiểm tra.
- Hiệu ứng GUI mượt hơn: đã đưa vào `MVP-Plan.md` Phần 6 mục 4, người dùng sẽ làm sau, chưa cài DOTween.
- Chạy lại kiểm tra Đợt 3; thử chữ có dấu, safe-area trên iPhone; build lại và so Build Report (font, sprite mới; mốc 10,8 MB).
- Sau khi vẽ `Decor`: tạo `ComingSoonArea` cho ao, chuồng, nhà kho, đất mở rộng, gán vào `GameUI`.
- TODO: thay màu phẳng bằng khung, nút UI Pack (cần đặt Border 9-slice trong Sprite Editor).
- TODO: xem lại bố cục GUI sau khi chơi thử, người dùng có thể đổi.
- TODO: có canvas GUI thật hoặc font pixel thì thay vào.

## Phần 7 – Lưu tiến trình (ĐÃ VIẾT CODE, chờ kiểm tra trên máy thật; save lên `version` 2 ở Phần 6b)

### Đã làm
- `Assets/_Game/Logic/Save/`: `SaveData` (`version` = 1), `SaveMapper`, `BackupCode`; `Logic/MonotonicClock`.
- `Assets/_Game/View/Save/`: `SaveService` (+ `ISaveStore`, `PlayerPrefsStore`), `WebBridge`; `View/Plugins/WebGL/VeQueSave.jslib` (sự kiện ẩn tab, màn sao lưu HTML).
- `GameBootstrap` nạp save khi khởi động; `GameUI` nối nút "Sao lưu". Không cần chạy lại **Tạo UI**, không cần sửa scene.
- Menu **Về Quê → Xóa save**; trong Editor, menu ba chấm của `GameBootstrap` có **Nhập mã từ clipboard** (khi đang Play).
- `Tests/EditMode/SaveTests.cs`.

### Quyết định
- Chỉ lưu ô đang có cây, theo `(zoneId, bed, col, row)`; khóa/trống suy ra từ `openCount`. Ô không còn trong data thì bỏ qua, cây không còn thì ô về trống. Hạt, nông sản của cây chưa bật vẫn giữ.
- Lưu sau `Farm.Changed` khoảng 1 giây và khi ẩn tab (`visibilitychange`, `pagehide`). Giữ thêm bản hợp lệ liền trước (`veque.save.prev`) để phòng bản chính hỏng.
- Save từ bản game mới hơn: không ghi đè, báo người chơi tải lại trang.
- Nhập mã: kiểm tra, ghi save, ngừng ghi rồi tải lại scene.
- Chống chỉnh giờ: `MonotonicClock` chỉ chặn lùi giờ. Không có server nên không phát hiện chỉnh giờ tới; máy từng đặt sai giờ về tương lai sẽ đứng thời gian tới khi giờ thật đuổi kịp.
- Mã sao lưu `VQ1-<base64>-<checksum>`: checksum chỉ phát hiện lỗi khi chép, không chống sửa mã.
- Màn sao lưu bằng HTML phủ lên game: `GUIUtility.systemCopyBuffer` không chạm tới clipboard trình duyệt trên WebGL. `WebGLInput.captureAllKeyboardInput` tắt từ lần mở đầu tiên để ô HTML nhận phím.

### Kiểm tra
- [x] Biên dịch ngoài Unity (chế độ Editor và WebGL) không lỗi; cú pháp `.jslib` chưa được Unity xử lý thật.
- [ ] Test Runner EditMode xanh.
- [ ] Trong Editor: gieo, tưới, thoát Play, vào lại sau vài phút thấy cây đã lớn, đất vẫn ướt.
- [ ] Build WebGL, so Build Report (mốc 10,8 MB).
- [ ] Đóng tab, mở lại sau vài phút thấy cây đã lớn (iPhone Safari, Android).
- [ ] Nhập mã trên máy khác ra đúng tiến trình.
- [ ] Thử trong trình duyệt nhúng Facebook (dữ liệu tách riêng với Safari).

## Phần 6b – Tiến trình: cấp độ, đơn hàng, cân bằng (XONG trong Editor – 06/11/2026; chưa build, chưa so Build Report)

### Quyết định
- Duyệt lại vòng chơi: thiếu mục tiêu giữ chân người chơi → thêm cấp độ (A) + đơn hàng (B) vào MVP; nhiệm vụ hằng ngày (C) + bộ sưu tập (D) để Phần 6c.
- Phát hiện khi duyệt: có thể kẹt (hết xu, hạt, nông sản, cây) → tặng 1 hạt rau muống. Bán và mở khu chưa có thông báo; `UnlockDialog` không đóng cùng `CloseAll`; mua hạt từ bảng chọn hạt làm mất ô đang chờ.
- Cân bằng cây theo công thức (chi tiết ở `MVP-Plan.md` Phần 6b): lãi/lần = `5 × (T/2)^0,8`, thu/giá hạt 2,0 → 3,5, XP = `(T/2)^0,65`. Tham khảo: phân tích kinh tế FarmVille (Adam Nash), "The Math of Idle Games" (Kongregate), bảng Hay Day / Stardew.
- Mô phỏng (bảng giá mở ô hiện tại, chưa tính cấp/đơn): người ghé 4 lần/ngày hết Khu 3 khoảng ngày 9,3 (bảng cũ 18,8); ghé 2 lần/ngày khoảng ngày 18 (bảng cũ 37).
- Mục tiêu nhịp (ghé 4 lần/ngày): Khu 1 trong ngày đầu, Khu 2 ngày 3–4, Khu 3 ngày 10–14.

- Duyệt bảng XP `3 × cấp^3,2` và giá Khu 3 ×3,5 (Khu 1, 2 giữ nguyên). XP đơn hàng tính theo số lần thu: Σ(số lượng ÷ sản lượng × XP cây) × 2.
- Lưu ý đã chấp nhận: mấy ô cuối Khu 3 gần như không hoàn vốn (chỗ tiêu xu cuối game); cấp 15 đạt khoảng ngày 9, sớm hơn khi hết Khu 3.

### Đã làm
- Đợt 0: cập nhật `CLAUDE.md`, `MVP-Plan.md` (Phần 6b, 6c), `tien-do.md`.
- Mô phỏng có cấp độ + đơn hàng (Python, ngoài repo), dò 270 tổ hợp, chốt bảng XP + giá khu.
- Đợt 1 – Logic cấp độ + chống kẹt (đã viết code, chờ Test Runner):
  - Mới: `Logic/LevelDef.cs` (bảng XP, cấp tối đa, `CoinsPerLevel`, `UnlockSeedCount`; `LevelDef.None` = không có cấp), `Tests/EditMode/ProgressionTests.cs` (32 test).
  - `CropDef` + `Xp`, `UnlockLevel`; `ZoneDef` + `RequiredLevel`; `FarmDef` + `Levels`, `RescueCropId` (đều không bắt buộc, test cũ giữ nguyên); `FarmState` + `Level`, `Xp`; `FarmResult` + `CropLocked`, `LevelTooLow`.
  - `Farm`: thu hoạch cộng XP, lên liền nhiều cấp, thưởng `CoinsPerLevel × cấp` + hạt cây vừa mở, sự kiện `LeveledUp`; gieo/mua cây chưa tới cấp → `CropLocked`; khu sau cần khu trước hết ô + đủ cấp (`LevelTooLow`), lên cấp thì mở ô miễn phí; chống kẹt sau mỗi lệnh và khi nạp save (sự kiện `Rescued`); `IsCropUnlocked`, `GetXpToNext`.
  - Save: thêm `level`, `xp` (save cũ → cấp 1); cấp ngoài bảng kẹp về cấp tối đa, XP không vượt ngưỡng. `version` vẫn 1, lên 2 ở Đợt 3.
  - Biên dịch + chạy NUnit ngoài Unity (scratchpad): Logic + `FarmTests` + `ProgressionTests` 101/101 xanh. `SaveTests` cần View nên chưa chạy được ngoài Unity.

- Tua thời gian + log + data cấp độ (làm trước theo yêu cầu 06/11/2026):
  - `Logic/OffsetClock.cs` (giờ thật + khoảng lệch, chỉ tua tới) + `Tests/EditMode/ClockTests.cs` (6 test).
  - `GameBootstrap` dùng `OffsetClock`; trong Editor khoảng lệch lưu ở PlayerPrefs `veque.debug.timeOffset` (bản build luôn 0); log khi lên cấp và khi được cứu kẹt.
  - Menu **Về Quê → Tua thời gian** (`Editor/TimeTravelWindow.cs`): +1 phút / +10 phút / +1 giờ / +4 giờ / +1 ngày, hiện đã tua, cấp, XP, xu. **Xóa save** xóa luôn khoảng đã tua.
  - Data: `CropDefinition` + `_xp`, `_unlockLevel`; `ZoneDefinition` + `_requiredLevel`; `FarmDefinition` + `_xpToNext`, `_coinsPerLevel` (20), `_unlockSeedCount` (3), `_rescueCrop`. `DefaultDataGenerator` tính giá/XP 25 cây theo công thức, ghi bảng XP, cấp mở cây (tạm cho cây đang tắt), khu cần cấp 5 / 9, giá Khu 3 ×3,5.
  - Biên dịch toàn bộ Logic + View + Editor + Tests ngoài Unity với thư viện Unity: không lỗi. NUnit ngoài Unity: 107/107 xanh.

- Đợt 2 – Logic đơn hàng + save v2 (đã viết code, chờ Test Runner):
  - Mới: `Logic/OrderDef.cs` (số ô, cấp mở, % xu, % XP, thời gian chờ khi bỏ, tên người đặt, trần số lần thu), `Logic/OrderState.cs` (+ `OrderItem`), `Logic/SeededRandom.cs` (xorshift32), `Tests/EditMode/OrderTests.cs` (26 test, có test chạy trọn vòng gieo → giao đơn → lên cấp → mở Khu 2).
  - `FarmDef` + `Orders` (mặc định `OrderDef.None`); `FarmState` + `Orders`, `RandomState`; `FarmResult` + `OrdersLocked`, `UnknownOrder`, `OrderNotReady`.
  - `Farm`: `DeliverOrder`, `DiscardOrder`, `RefreshOrders` (View gọi định kỳ), `TryGetOrder`, `CanDeliver`, `OrdersUnlocked`, `OrderSlotCount`. Sau mỗi lệnh tự điền đơn cho ô đã hết thời gian chờ; lên cấp mở đơn thì có đủ 3 đơn. Nạp save: chỉnh số ô theo data, đơn có cây không còn thì sinh lại; `RandomState` 0 thì gieo từ đồng hồ.
  - Đơn: 1 loại (cấp < 4), tối đa 2 (cấp 4–6), tối đa 3 (cấp ≥ 7); mỗi loại 1..min(6, 2 + cấp/2) lần thu × sản lượng; không lặp loại; xu làm tròn 5 (≥ 5), XP ≥ 1.
  - Save `version` 2: `orders`, `randomState`. Save 1 nạp được (đơn sinh mới).
  - Biên dịch toàn bộ ngoài Unity: không lỗi. NUnit ngoài Unity: 133/133 xanh.

- Đợt 3 – Data đơn hàng + thử trong Editor (đã viết code, chờ kiểm tra):
  - `FarmDefinition` nhóm **Đơn hàng**: `_orderSlots` 3, `_orderUnlockLevel` 2, `_orderCoinPercent` 160, `_orderXpPercent` 200, `_orderDiscardMinutes` 5, `_orderHarvestCap` 6, `_customerNames`. `DefaultDataGenerator` ghi các số này + 8 tên (Bà Tư, Chú Ba, Cô Năm, Bác Hai, Dì Sáu, Ông Bảy, Chị Út, Anh Tám).
  - `GameBootstrap`: `RefreshOrders()` mỗi giây (cả bản build); `DeliverOrder(slot)` có log; `NowSeconds` (giờ game).
  - Cửa sổ **Tua thời gian** có phần **Đơn hàng**: món cần / đang có, thưởng, nút Giao (mờ khi thiếu hàng), Bỏ, đếm ngược ô trống.
  - `FarmDefinitionTests` + 8 test kiểm tra data mặc định (bảng XP, cân bằng 4 cây, cấp khu, tổng Khu 3, cây cứu kẹt, cấu hình đơn).
  - Biên dịch toàn bộ ngoài Unity: không lỗi; NUnit Logic 133/133 xanh (`FarmDefinitionTests` cần Unity).

- Người dùng xác nhận Đợt 3 ổn (06/11/2026).
- Đợt 4 – GUI (đã viết code, chờ dựng và kiểm tra):
  - Logic: `LevelUnlocks` + `Farm.GetUnlocksAtLevel(cấp)` (xu thưởng, cây mở, số hạt, khu đủ cấp, mở đơn hàng); 3 test trong `ProgressionTests`.
  - Mới trong `View/UI/`: `LevelHud` (Cấp N + thanh XP, "tối đa"), `OrderCard`, `OrderPanel` (3 thẻ: người đặt, tối đa 3 món icon + có/cần, xanh khi đủ, thưởng, Giao/Bỏ, đếm ngược ô trống), `LevelUpDialog` (lần lượt từng cấp: xu, cây mở + hạt, mở đơn hàng, đủ cấp mở khu).
  - `GameUI`: tab thứ 4 **Đơn hàng** (số đơn giao được trên tab; chưa tới cấp 2 → thông báo), thông báo mở khu mới, được tặng hạt; `CloseAll` đóng cả hộp mở ô và bảng Đơn hàng.
  - `ShopPanel`, `SeedPanel`: cây chưa tới cấp hiện "Mở ở cấp N", nút mờ. `SeedPanel` hết hạt: nút "Mua / N xu" mua 1 rồi gieo luôn, nút phụ "Cửa hàng". `StoragePanel`: bán xong báo "+N xu".
  - `FarmInteraction`: khu khóa báo "Mở hết Khu N trước" hoặc "Khu N mở ở cấp M"; câu báo cho `CropLocked`, `LevelTooLow`.
  - `UiFormat.Reward`, `UiFormat.ZoneName` + 4 test. `UiBuilder`: HUD (xu 130, cấp 130, sao lưu 90), 4 tab (chữ tự co 11–15), bảng Đơn hàng, hộp lên cấp; tách `BuildSheet` dùng chung.
  - Biên dịch toàn bộ ngoài Unity: không lỗi. NUnit ngoài Unity (Logic + UiFormat): 153/153 xanh.

- Người dùng đã dựng UI, chạy test, chơi thử từ save mới: xác nhận Đợt 4 ổn (06/11/2026).

### Còn lại
- Build WebGL lại, so Build Report (mốc 10,8 MB).
- Icon nông sản trong thẻ đơn lấy `ProduceIcon`; cây chưa gán icon thì chỉ hiện chữ.
- TODO (sau MVP, 06/11/2026): cân nhắc nâng art lên 32×32. Code ít việc (`FarmView.SpritePixelsPerUnit`, `_cropOffsetPixels`, `CropSpriteAssigner` CellSize/PPU, Preset PPU, `assetsPPU`, `_overviewRef`/`_focusRef`); logic, data, save, map không đổi. Cái giá: cần bộ art 32×32 đầy đủ (Sprout Lands chỉ có 16×16, không trộn 2 cỡ), dung lượng build và bộ nhớ texture ×4, bậc zoom nguyên thô hơn (màn 720–1170 px chỉ còn zoom 1–3). MVP giữ 16×16; làm đẹp trước bằng UI 9-slice, hiệu ứng GUI, hiệu ứng trên map, background trời.
- Đợt 1–4 đã xong và được xác nhận trong Editor (khóa cây theo cấp, báo khu thiếu cấp đã làm ở Đợt 4).
- Làm sau (ghi lại, chưa duyệt): công cụ Editor **Về Quê → Cân bằng** chạy `Farm` thật để dò tham số theo số ngày mục tiêu (chi tiết ở `MVP-Plan.md` Phần 6b).

## Phần 5b – Hoạt ảnh cây trồng (Đợt A + B XONG trong Editor – 07/11/2026; khung hình chờ art)

### Quyết định
- Không cần danh sách sprite theo trạng thái: hoạt ảnh bằng code trên sprite giai đoạn có sẵn. Khung hình (art) để Đợt B, dạng không bắt buộc.
- Di chuyển làm tròn theo pixel (`Ease.SnapToPixel`); phóng/thu chỉ trong hoạt ảnh ngắn.

### Đã làm
- Mới: `View/Anim/Ease.cs` (OutQuad, InQuad, OutBack, DampedWave, SnapToPixel; không dùng UnityEngine) + `Tests/EditMode/EaseTests.cs` (11 test).
- `FarmView`: nhớ trạng thái trước của từng ô (có cây, cây gì, giai đoạn, đã tưới) để bắt sự kiện; không chạy hoạt ảnh lúc mới vào game. Gieo nảy, lên giai đoạn nhún, chín nhún 1 pixel lệch nhịp (`_ripeHop`, tắt thì nhấp nháy màu như cũ), thu hoạch bản sao bật lên rồi mờ (pool 8), tưới giọt nước (pool 36, sprite 1×1 tạo lúc chạy). Mỗi hiệu ứng có công tắc + thông số trong Inspector. `Update` chỉ ghi transform cho ô đang chuyển động. `TryGetCropTop` tính từ vị trí gốc nên thanh tiến độ không nhún theo cây.
- Biên dịch toàn bộ ngoài Unity: không lỗi. NUnit ngoài Unity: 164/164 xanh.

### Còn lại
- Đợt A: người dùng chơi thử, ổn (07/11/2026). Chưa ghi nhận kết quả Test Runner 221.
- Đợt B (đã viết code, chờ kiểm tra – 07/11/2026):
  - Nông sản bay về Kho: `FarmInteraction.Harvested(ô, cây, số lượng)` thay thông báo "+2 Rau muống"; `FarmView.TryGetPlotCenter`; mới `View/UI/HarvestFlyer.cs` (pool 9 icon, Bezier bậc 2, nhanh dần về cuối, thu nhỏ còn 60%; tab Kho nảy; nhãn "+N [icon]" trên tab, cộng dồn cùng loại, hiện 1,2 giây rồi mờ). `UiBuilder` dựng lớp `HarvestFly` và nhãn `HarvestBadge` trong tab Kho → cần chạy lại **Tạo UI**.
  - Gió: mới `Art/Shaders/CropSway.shader` (dựa trên Sprites/Default, `DisableBatching`), `FarmView._cropMaterial` (để trống = không gió). Người dùng tạo material + gán. Shader chưa được Unity biên dịch.
  - Biên dịch C# ngoài Unity: không lỗi; NUnit 164/164 xanh.
  - Khung hình không bắt buộc: chờ có art.
  - 07/11/2026: người dùng xác nhận ổn (bay về Kho, gió); Batches tăng không đáng kể → giữ `DisableBatching`.

## Phần 6 mục 5 – Gieo / thu hoạch / tưới bằng chạm và kéo (đã viết code, chờ kiểm tra – 07/11/2026)

### Quyết định
- Nghiên cứu: luồng cũ khó chịu vì bảng chọn hạt che nửa màn hình, mỗi ô 1 lần chạm, chạm ô trống gieo luôn hạt cũ (dễ nhầm), hết hạt thì bảng bật ra giữa chừng, kéo chỉ di chuyển camera. Kiểu chạm rồi kéo của thể loại farm (Hay Day: menu cạnh ô, kéo hạt/liềm qua nhiều ô) giải quyết các điểm này.
- Hết hạt khi kéo: dừng, báo "Hết hạt", không tự mua (cách a).
- Làm luôn kéo để thu hoạch, tưới.

### Đã làm
- Mới: `View/Map/FarmGesture.cs` (gieo/thu hoạch/tưới khi kéo, lấy mẫu đường kéo mỗi 0,25 ô, mỗi ô 1 lần), `View/UI/SeedPicker.cs` + `SeedPickerItem.cs` (thanh hạt bám trên ô, chạm/kéo icon; khi kéo thanh trong suốt, icon hạt theo ngón tay nâng 48 pt).
- `CameraTouchInput`: interface `IWorldDragHandler`; vượt ngưỡng kéo khi đang zoom thì hỏi `FarmGesture` trước khi di chuyển camera.
- `FarmInteraction`: bỏ hạt đang chọn (`SelectedCrop`, `SelectionChanged`); thêm `Plant` / `Water` / `Harvest` / `Report` dùng chung, sự kiện `Tapped`, `LastCropId`.
- `FarmView.Flash(ô)`: ô sáng thoáng qua (pool 16, thứ tự vẽ 50: trên đất, dưới cây).
- `GameUI`, `UiBuilder`: thanh hạt thay bảng chọn hạt; tự thêm `FarmGesture` vào GameObject của `FarmInteraction`. Xóa `SeedingBar.cs`, `SeedPanel.cs` (+ `.meta`).
- Biên dịch toàn bộ ngoài Unity: không lỗi; NUnit 164/164 xanh.

### Còn lại
- Người dùng: **Tạo UI** (Ctrl+S), Test Runner (221 xanh), chơi thử gieo chạm/kéo, kéo thu hoạch, kéo tưới, kéo di chuyển camera vẫn được; thử trên điện thoại.

## Kế hoạch Phần 6c và Phần 10 (CHƯA LÀM – 07/11/2026)

### Quyết định
- Thời gian chờ cây lớn là cố ý; thêm việc nhỏ mỗi lần vào game và lý do quay lại. 8 mục Phần 6c sắp theo độ phức tạp trong `MVP-Plan.md`: 1 báo cây chín trên tab, 2 đơn hàng (chờ / giới hạn ngày / đơn gấp), 3 chuyện bất ngờ trên ruộng, 4 nông sản hiếm + album, 5 nhiệm vụ ngày + điểm danh, 6 khách vãng lai, 7 chế biến (sau MVP), 8 trang trí (sau MVP).
- Đơn hàng (chốt): chờ 10 phút sau khi giao, giới hạn 15 đơn giao mỗi ngày (chỉ sinh đơn khi đã giao + đang có < 15, bỏ đơn không tốn lượt), đơn gấp 2–3 giờ một lần / hết hạn 30 phút / thưởng ×2 / không tính lượt. Ngày theo giờ địa phương (độ lệch múi giờ lấy từ trình duyệt). Save lên `version` 3. Trước khi code: chạy lại mô phỏng cân bằng.
- Báo cây chín trên tab: tính thời điểm chín sớm nhất lúc tab ẩn, hẹn giờ bằng JS (Unity dừng khi tab ẩn).
- Câu cá đưa vào MVP (Phần 10; `CLAUDE.md` đã sửa phạm vi, còn "nuôi cá" ngoài phạm vi): ném cần canh perfect → chờ cắn, giật trong khung phản xạ (có tỉ lệ trượt) → kéo cá bằng chuỗi mũi tên 4 hướng + chốt theo nhịp (độ căng dây, đứt dây, gãy cần) → cá có độ hiếm (5 mức), cân nặng, giá theo cân nặng, khung giờ xuất hiện (7 khung theo giờ địa phương), mồi ưa thích; càng hiếm càng khó. Cần câu 5 rank, độ bền, sửa / mua mới. Mồi hút loài khác nhau, giun có thể đào được khi thu hoạch.
- Thứ tự đề xuất: Phần 6c mục 1–2 → Phần 4 template WebGL + chạy trên điện thoại → Phần 10.
- Phần 10 chốt thêm (07/11/2026): câu cá ở **vùng câu riêng** cùng scene, đặt ngoài cổng trái; ao nhà chỉ thả cá sưu tầm (bóng màu, chạm xem danh sách – làm sau cùng). Nhân vật: ở nông trại đi lang thang làm cảnh (thao tác kiểu nhìn từ trên xuống giữ nguyên); bấm **Đi câu** thì nhân vật **tự đi ra cổng** rồi sang vùng câu; ở vùng câu chạm để đi, chạm chỗ câu thì đi tới rồi mới câu; camera đi theo, **kéo để xem xung quanh rồi tự quay về**. Mục đích chính của nhân vật: nhân vật của người chơi, chỗ tiêu xu cuối game (trang phục / phụ kiện), dễ thêm vùng. Trang phục: thân đổi cả bộ + biến thể màu, mũ / phụ kiện là lớp riêng bám điểm neo; hệ hiển thị nhiều lớp dựng sẵn, cửa hàng trang phục sau MVP. 6 giai đoạn trong `MVP-Plan.md` Phần 10 mục 7.

### Còn lại
- Người dùng chưa cho bắt đầu Phần 6c mục 1–2 (07/11/2026: "chưa làm").
- Câu cá đã chốt (07/11/2026): 4 nút mũi tên + Chốt; XP câu cá cộng vào XP nhân vật (đưa vào mô phỏng cân bằng); cá giữ trong giỏ (bán / giao đơn / thả ao, giỏ đầy phải bớt mới câu tiếp); asset vùng câu, biểu tượng chỗ câu, âm thanh người dùng thêm sau – code dùng hình tạm trước. Nhân vật và Ocean Pack có sẵn, cần kiểm tra đủ khung khi làm.

## Tiếp theo (khi quay lại)
0. Phần 6 mục 5 – chạy **Tạo UI**, Test Runner (221), chơi thử gieo chạm/kéo, kéo thu hoạch, kéo tưới; thử cảm giác kéo trên điện thoại.
1. Phần 6c mục 1–2 (khi người dùng cho làm): chạy lại mô phỏng cân bằng với giới hạn đơn → duyệt → code.
2. Phần 4 – template WebGL (`touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`) + build, so Build Report (mốc 10,8 MB) + thử trên iPhone Safari / Android; cùng lúc kiểm tra Phần 7 trên máy thật.
3. Phần 10 – nhân vật, vùng câu, câu cá, ao nhà (trong MVP): chốt các câu hỏi còn lại, kiểm tra art, làm theo 6 giai đoạn trong `MVP-Plan.md`.
4. Phần 3 – art (setup ô đất), vẽ phần còn lại trên `Decor`, tạo `ComingSoonArea` (trừ ao), Sprite Atlas.
5. Chưa quyết: gỡ package không dùng (spriteshape, psdimporter, aseprite, 2d.animation); giảm build < 10 MB (Phần 8).
6. Làm sau: Phần 6c mục 3–8, Phần 6 mục 4 (hiệu ứng GUI), công cụ Cân bằng, art 32×32, khung hình cây.
