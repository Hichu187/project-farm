# Về Quê – Tiến độ

## Tóm tắt (cập nhật 13/11/2026)

| Phần | Trạng thái | Còn thiếu |
|---|---|---|
| 0 – Khung project | Xong | Chưa quyết gỡ 4 package không dùng |
| 1 – Logic + test | Xong | — |
| 2 – Data | Xong (giá cây, cấp, đơn hàng tính lại ở 6b) | — |
| 3 – Map & art | Đang làm (chuyển sang Sunnyside ở 3b) | `ComingSoonArea`, Sprite Atlas |
| 3b – Chuyển art sang Sunnyside | Đang làm | Bước 0, 1 xong; 1b tạo nhân vật đã viết code, chờ kiểm tra; 2 vẽ map (công cụ có, người dùng vẽ sau); 3 ô đất + cây tạm; 4 UI đang làm (khung 9-slice + màu, icon, thanh XP: người dùng xác nhận ổn; font FatPix dựng từ SVG + viền: chờ xem lại); 5 dọn Sprout Lands + build |
| 4 – Camera | Ổn trong Editor | Template WebGL, thử trên iPhone / Android |
| 5 – Ô đất & cây | Ổn trong Editor | Đổi sprite cây + tile đất sang Sunnyside (3b bước 3); bộ cây Việt người dùng vẽ |
| 5b – Hoạt ảnh cây | Ổn trong Editor (Đợt A + B) | Khung hình: chờ art |
| 6 – UI | Ổn trong Editor | Mục 4 hiệu ứng GUI (làm sau); mục 5 chạm/kéo chờ kiểm tra; chữ có dấu + safe-area trên iPhone |
| 6b – Cấp độ, đơn hàng, cân bằng | Ổn trong Editor | Build + Build Report; công cụ Cân bằng (làm sau) |
| 6c – Việc làm lúc chờ cây lớn | Mục 2 (đơn hàng: chờ, Khẩn cấp, 15 đơn/ngày) xong code, người dùng xác nhận đơn ổn 12/11 | Mục 1, 3–6 trong MVP, 7–8 sau MVP; Test Runner + thử Tua thời gian mục 2; mỗi mục duyệt thiết kế trước |
| 10 – Nhân vật, vùng câu, câu cá, ao nhà | Giai đoạn 1–3 xong. Giai đoạn 4: Đợt A–D xong (người dùng: câu ổn 13/11 – nhịp, bấm sai nhập lại, chữ trên đầu, cá bay, bảng cá vừa câu); HUD câu + đồng hồ chung (người dùng: ổn); giỏ 25 ô mở từng ô (tạm ổn); mua cần / mồi theo cấp, tab Cửa hàng, bán cá ở Giỏ + Kho, bỏ tab Nông trại, bóng cá xem xét / đớp thử / căng – đã viết code, chờ kiểm tra | Kiểm tra các mục chờ (xem "Việc đang chờ"); Đợt E ("Kỷ lục mới!", báo đào giun, build + Build Report); giai đoạn 5 còn album, đơn hàng có cá; giai đoạn 6 ao nhà; art cá (7 loài thừa), phao, ổ khóa, icon mồi thật |
| 7 – Lưu tiến trình | Đã viết code (save `version` 7 từ 13/11: thêm số ô giỏ `basketSlots`; save ≤ 6 quy đổi mức giỏ cũ) | Thử trên máy thật, nhập mã máy khác, trình duyệt nhúng Facebook |
| 8 – Build & phát hành | Chưa làm | Toàn bộ |
| **Đợt 2** (từ 20/11, kế hoạch `ke-hoach-dot-2.md`) | Thứ tự 1 – Phần 4 Bộ cây mới: data 25 cây, save version 8 (bỏ save cũ), art 25 cây đã gán, giai đoạn cuối chỉ khi chín – đã viết code. Kèm: thanh chọn hạt chỉ hiện hạt có trong kho + cuộn ngang (người dùng: ổn); Cửa hàng / Kho xếp theo cấp mở | Test Runner lại (dự kiến 468/468); Play xem art cây; build + Build Report. Tiếp theo: thứ tự 2 – Phần 1 Sửa cần |

Git (20/11): nhánh `setup/phan-0-camera`; `origin` có tới `3276bb3`. Commit mới nhất `dc35df7` (chưa push). **Chưa commit:** 258 mục trong `git status` (việc 13–14/11 + Đợt 2 từ 20/11). **Chưa commit (13/11):** asset cá + `ArtSource/fish/`, sửa nhịp / bóng cá / chữ trên đầu / cá bay / bảng cá vừa câu, HUD câu + đồng hồ, giỏ 25 ô, mua cần / mồi, bán cá, tab Cửa hàng / Kho, bỏ tab Nông trại (88 mục trong `git status`).
Test: Test Runner 13/11 (sau mua cần / mồi): 446 test, 445 xanh, 1 đỏ ở `FarmDefinitionTests` – data mồi chưa tạo lại (file mồi chưa có `_unlockLevel`) → chạy **Tạo data câu cá**. Sau bán cá dự kiến **449**. Ngoài Unity (13/11): NUnit Logic + `UiFormat` 345/345 xanh (không gồm `SmokeTests`, `SaveTests`, `FarmDefinitionTests`, `CharacterSkinTests`, `EaseTests`). Biên dịch Logic + View + Editor + Tests (Editor và WebGL): không lỗi.

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

## Kế hoạch: điều khiển nhân vật ở nông trại (CHƯA LÀM – 11/11/2026)

### Quyết định
- Bỏ nhân vật đi lang thang làm cảnh. Ở nông trại người chơi chạm để đi; gieo / tưới / thu hoạch (chạm hoặc kéo) xếp vào hàng việc `FarmTaskQueue` (Logic, có test), nhân vật đi tới làm **đúng thứ tự chọn** (không tự sắp theo đường ngắn).
- Zoom giữ nguyên: toàn cảnh chạm khu → zoom; chạm đất → đi tới. Đang zoom chạm ngoài khu → chỉ về toàn cảnh.
- Giữ chỗ hạt khi xếp gieo; kiểm tra lại khi tới nơi, lỗi thì bỏ việc đó. Hủy bằng chạm nhân vật / nút Dừng (trả hạt).
- Ẩn tab còn việc → làm hết ngay rồi lưu; không lưu hàng việc / vị trí nhân vật.
- Mở ô bằng xu, "sắp có", Cửa hàng, Kho vẫn làm ngay, không đi bộ.
- Đã sửa `MVP-Plan.md` (Phần 6 mục 5, Phần 10 mục 0, thêm mục 0b, mục 7 giai đoạn 1) và `CLAUDE.md`.

## Phần 10 giai đoạn 1 – Logic nhân vật (XONG – 11/11/2026)

Người dùng cho làm trước thứ tự trong plan (trước Phần 6c mục 1–2 và chạy thử trên điện thoại).

### Đã làm
- Mới trong `VeQue.Logic`:
  - `GridCell`: ô `(X, Y)`, cho phép tọa độ âm.
  - `WalkGrid`: lưới đi được (`IsWalkable`, `SetWalkable`, `SetBlocked`, `SetRect`); ô ngoài lưới coi là bị chặn.
  - `GridPath`:
    - A* 4 hướng trên trạng thái (ô, hướng): ra đường ngắn nhất, trong đó chọn đường ít rẽ nhất. Bộ đệm tái dùng.
    - `FindNearestReachable`: ô đi được, tới được, gần chỗ chạm nhất.
    - `FindStandCell`: ưu tiên trên/dưới → trái/phải → ô tới được gần nhất; trong nhóm chọn ô đi bộ ít hơn.
  - `FarmTask` + `FarmTaskKind`.
  - `FarmTaskQueue`:
    - thứ tự vào trước ra trước; 1 việc mỗi ô (tính cả việc đang làm);
    - giữ chỗ hạt khi gieo; `TryBeginNext` / `CompleteCurrent` / `Cancel`;
    - `GetAvailableSeeds`, `IsQueued`, sự kiện `Changed`.
  - `FarmResult.AlreadyQueued` (thêm cuối enum).
- Test mới: `GridPathTests` (23), `FarmTaskQueueTests` (27). Ngoài Unity: 50/50 xanh. Toàn bộ test Logic chạy được ngoài Unity: 186/187; `SmokeTests` lỗi vì ngoài Unity tên assembly khác, không phải lỗi code.

### Quyết định
- Hàng việc chỉ giữ thứ tự và kiểm tra lúc xếp. View gọi `Farm` khi nhân vật tới nơi (Farm tự kiểm tra lại), rồi luôn gọi `CompleteCurrent` để trả hạt giữ chỗ, kể cả khi việc thất bại.
- Ô đang có việc (vd. đang chờ thu hoạch) không xếp thêm việc khác cho tới khi việc đó xong.

### Còn lại
- [x] Người dùng xác nhận Test Runner ổn (11/11/2026).
- Bước View (duyệt riêng):
  - lưới đi được lấy từ Tilemap;
  - `CharacterView` nhiều lớp + kiểm tra khung trong sheet;
  - chạm để đi; bộ chạy hàng việc;
  - nối `FarmInteraction` / `FarmGesture` / `SeedPicker` vào hàng việc;
  - `Report` xử lý `AlreadyQueued`;
  - nút Dừng; làm hết khi ẩn tab.

## Phần 10 giai đoạn 1 – Đợt A: nhân vật + chạm để đi (XONG – 11/11/2026, người dùng xác nhận ổn)

### Quyết định
- Nhân vật bản đơn giản: 1 `SpriteRenderer`, art Sprout Lands có sẵn, chưa có hệ trang phục nhiều lớp.
- Bố cục sheet (đã xem): `Basic Charakter Spritesheet` 4×4 khung 48×48, hàng xuống / lên / trái / phải, cột 0–1 đứng, 2–3 đi. `Basic Charakter Actions` 2×12: hàng 0–3 cuốc, 4–7 rìu, 8–11 bình tưới (cùng thứ tự hướng). Nhân vật 16 px, chân cách đáy khung 16 px → pivot (0.5, 0.333).
- Thu hoạch dùng hoạt ảnh cuốc (chưa có khung nhặt).
- Ô đi được = `Ground` có tile, `Plots` và `Blockers` (Tilemap mới) không có, ngoài vùng `ComingSoonArea`.

### Đã làm
- Mới (`View/Character`): `CharacterSkin` (SO), `CharacterView`, `CharacterMotor`, `WalkGridSource`, `FarmCharacterController`; `View/Map/SortingOrders` (thứ tự vẽ theo Y dùng chung cây + nhân vật).
- Mới (`Editor`): `CharacterSkinAssigner` (menu **Về Quê → Gán sprite nhân vật**).
- Sửa: `CameraTouchInput` (sự kiện `TappedGround` chỉ ở toàn cảnh), `ComingSoonArea` (`SizeInCells`), `FarmView` (dùng `SortingOrders`).
- Biên dịch Logic + View + Editor ngoài Unity: không lỗi.

### Còn lại
- Người dùng: import 2 sheet, cắt 48×48, chạy Gán sprite nhân vật, tạo Tilemap `Blockers`, đặt nhân vật vào scene, chơi thử (hướng dẫn trong hội thoại 11/11/2026).
- Đợt B: hàng việc trồng trọt.

## Phần 10 giai đoạn 1 – Đợt B: hàng việc trồng trọt (XONG – 11/11/2026, người dùng xác nhận ổn)

### Đã làm
- `GameBootstrap`: giữ `Tasks` (`FarmTaskQueue`, không lưu); sự kiện `PageHiding` phát trước khi lưu lúc tab ẩn / app tạm dừng / thoát.
- `FarmInteraction`: `Enqueue` (báo lỗi, `AlreadyQueued` thì im lặng) và `Execute`. Chạm ô chín → xếp việc thu; cây chưa tưới → xếp việc tưới + thanh tiến độ hiện ngay; ô trống đã chờ gieo thì không mở lại thanh hạt. Mở ô bằng xu vẫn làm ngay.
- `FarmGesture`: kéo xếp việc theo thứ tự ngón tay đi qua; hạt kiểm tra bằng `GetAvailableSeeds`.
- `SeedPicker`: số hạt đã trừ phần giữ chỗ; chạm icon xếp việc gieo.
- `FarmView`: `TryGetPlotCell`; ô đang chờ phủ màu nhạt (`_queuedColor`, tắt được); làm xong thì sáng lên (`Flash`).
- `CameraTouchInput`: `ITapInterceptor` (chạm nhân vật xử lý trước).
- `CharacterMotor.SkipToFirstCellInside`: nhân vật ngoài màn hình thì hiện ở ô đầu tiên của đường đi nằm trong khung hình.
- `FarmCharacterController`:
  - vòng làm việc: chỗ đứng → đi → quay mặt → cuốc (gieo, thu) / bình tưới (tưới) → `Execute` → sáng ô → việc kế;
  - tốc độ +10% mỗi việc chờ, tối đa ×2;
  - hủy khi: chạm nhân vật, bấm Dừng, chạm đất ở toàn cảnh;
  - ẩn tab thì làm hết việc còn lại;
  - tự tìm `GameBootstrap` / `FarmInteraction` / `FarmView` nếu chưa gán.
- Mới: `UI/TaskStopButton` (nút **Dừng (N)**, góc dưới-phải trên thanh tab); `UiBuilder` dựng nút này.
- Biên dịch Logic + View + Editor ngoài Unity: không lỗi. Test Logic ngoài Unity 186/187 (`SmokeTests` chỉ lỗi ngoài Unity).

### Còn lại
- [x] Người dùng chạy Tạo UI, chơi thử theo **Kiểm tra** Phần 10 mục 0b: ổn (11/11/2026). Ẩn tab cần thử trên bản build web.

## Icon gợi ý tưới trên cây chưa tưới (XONG – 11/11/2026, người dùng xác nhận ổn)

### Quyết định
- Hiện ở cả toàn cảnh và khi zoom (công tắc `_showWaterHint`); giữ 16 px (thu nhỏ vỡ pixel).
- Art: bình tưới `Sprout Lands - Sprites - Basic pack/Objects/Basic_tools_and_meterials.png`, ô trên-trái.

### Đã làm
- `FarmView`:
  - hiện icon khi cây đang lớn, chưa tưới, chưa chín, chưa có việc chờ;
  - icon cách ngọn cây 2 px, đi theo giai đoạn, nhún 1 px (nhịp 1,2 giây, lệch nhịp giữa các ô);
  - vẽ trên cây và nhân vật (thứ tự 300);
  - xếp việc tưới thì ẩn, hủy thì hiện lại.
- Biên dịch ngoài Unity: không lỗi.

### Còn lại
- [x] Người dùng import sheet, gán **Water Hint Sprite**, chơi thử: ổn.

## Phần 3b – Chuyển art sang Sunnyside World (đang làm – 12/11/2026)

### Quyết định
- Chuyển hẳn sang Sunnyside World (một phong cách). Cây Sunnyside dùng tạm: rau muống ← `kale`, cải xanh ← `cabbage`, cà chua ← `radish`, bí đỏ ← `pumpkin`; người dùng sẽ vẽ bộ cây Việt cùng phong cách.
- Nhân vật: 3 lớp (thân `base`, tóc `shorthair`, dụng cụ `tools`), 2 hướng (lật trái), khung 96×64, pivot (0.5, 25/64).
- Thứ tự: 0 tài liệu + dọn → 1 nhân vật → 2 map → 3 ô đất + cây; 4 UI làm sau; 5 dọn Sprout Lands + build.

### Đã làm
- Bước 0:
  - xóa `Sunnyside_World_Gamemaker/` (66 MB) và `.DS_Store`;
  - sửa `CLAUDE.md` (Art, Map); `MVP-Plan.md` thêm Phần 3b + ghi thay đổi 12/11; viết lại `Docs/art-phan-10.md` theo Sunnyside.
- Bước 1 (XONG – người dùng chuyển sang bước 2):
  - `CharacterSkin`: 3 lớp / clip; hoạt ảnh đứng `IDLE` 8 fps, đi `WALKING` 10 fps, làm việc `DOING` 16 fps, tưới `WATERING` 10 fps; `Hoe` → `Work`.
  - `CharacterView`: 3 `SpriteRenderer` + `SortingGroup` (thứ tự theo Y), lật khi quay trái, lên / xuống giữ hướng ngang.
  - Editor: menu **Về Quê → Nhân vật Sunnyside: import + gán sprite** (tự đặt import + cắt 96×64 cho 12 strip, giữ ID sprite khi chạy lại); `VeQue.Editor.asmdef` thêm `Unity.2D.Sprite.Editor`.
  - Biên dịch ngoài Unity: không lỗi.

### Còn lại
- [x] Người dùng chạy menu nhân vật Sunnyside (bước 1).
- Bước 1b – tạo nhân vật (duyệt 12/11/2026, đã viết code, chờ kiểm tra):
  - Quyết định: chọn tên (1–16 ký tự), kiểu tóc (6 kiểu), màu tóc (Gốc + 7 màu); hiện lần đầu vào game (cả save cũ) + nút **Nhân vật** trên HUD; phủ màu bằng shader `VeQue/HairTint` (độ sáng tóc chuẩn hóa theo từng kiểu, giữ viền) vì tóc `shorthair` gần đen, nhân màu không thấy.
  - Logic: `CharacterLook` (+ chuẩn hóa tên, dạng dựng sẵn), `FarmState.Character`, `Farm.SetCharacter`, `FarmResult.InvalidName`; save `version` 3 (`SaveCharacter`; save 2 → chưa tạo nhân vật). Phần 6c mục 2 lùi sang `version` 4. Test mới `CharacterLookTests` (20; tổng trong Unity dự kiến 291); sửa `OrderTests.Save_CurrentVersionIs3`. Ngoài Unity: test Logic 206/207 (`SmokeTests` chỉ lỗi ngoài Unity).
  - View: `CharacterSkin` tách 6 bộ tóc + bảng màu + material; `CharacterView.SetLook`; `FarmCharacterController` áp ngoại hình theo `Farm.Changed`; `CharacterPreview`, `CharacterCreator`; `GameUI` đóng bảng khi mở màn nhân vật; shader `Assets/_Game/Art/Shaders/HairTint.shader`.
  - Editor: menu nhân vật import thêm 6 kiểu tóc, đo độ sáng tóc, tạo `Assets/_Game/Art/HairTint.mat`; `UiBuilder` dựng màn tạo nhân vật + nút HUD.
  - Biên dịch ngoài Unity: không lỗi.
- Bước 2 (đã viết công cụ; hướng dẫn từng bước ghi trong `MVP-Plan.md` Phần 3b bước 2; người dùng vẽ map sau):
  - Tileset Sunnyside tách lớp: đất / nước làm nền, cỏ phủ trên bằng mảnh viền bo tròn trong suốt → nền + hàng rào vẽ bằng công cụ vẽ map cũ (đổi tile), viền cỏ mềm / ao / nhà / cây vẽ tay trên `Decor`.
  - Editor: menu **Về Quê → Sunnyside: import tileset 16px** (đặt import, cắt 16×16 bỏ ô trống – 1740 tile, tên `ss_x_y` theo tọa độ, giữ ID khi chạy lại).
  - Gợi ý tile: cỏ `ss_1_1` (+ biến thể `ss_1_2`…`ss_5_2`), sân đất `ss_3_1` (biến thể `ss_9_7`, `ss_10_7`, `ss_9_8`, `ss_10_8`), nước `ss_4_1`, hàng rào gỗ quanh `ss_38_1`…`ss_42_4` (ngang `ss_39_2`, dọc `ss_40_3`, góc người dùng chọn khi thử).
- Bước 2 map, bước 3 ô đất + cây, bước 4 UI (sau), bước 5 dọn + build.
- Kiểm tra giấy phép Sunnyside.

## Khung chọn 4 góc Sunnyside (đã viết code, chờ kiểm tra – 12/11/2026)

### Quyết định
- Một kiểu khung 4 góc (`UI/selectbox_tl/tr/bl/br`, 8×9 px) cho mọi thứ "đang chọn", thay lớp phủ màu / chấm trắng.
- Chạm khu: góc thu vào ôm khu (4 px → khít, 0,15 giây), zoom xong mờ dần 0,3 giây (không giữ khi đang zoom). Ô đất (12/11 chốt sau 3 lần thử): ô còn chờ có khung khít ô (không lấn ô bên), mờ (alpha 0,4), đứng yên; ô nhân vật đang làm có 1 khung nới 2 px, co giãn 0→2 px theo từng pixel (0,6 giây; thử nhấp nháy độ đậm trước, người dùng chọn co giãn), vẽ trên khung ô chờ. Khung đứng yên tắt Update. Làm xong: vẫn lóe vàng, khung tắt. Ô khóa đang mở bảng mua: khung đứng yên (vàng nhạt). Đích đi bộ: khung thở.
- Chỉ dời góc theo bước pixel và đổi độ trong suốt, không phóng sprite; đặt góc theo `sprite.bounds` nên không cần chỉnh pivot. Khung vẽ trên cây / nhân vật (thứ tự 290), dưới icon gợi ý tưới (300).

### Đã làm
- `View/Anim/SelectBoxSkin` (SO: 4 góc + thông số), `View/Anim/SelectBox` (4 `SpriteRenderer`, kiểu Static / Breathe, thu vào, mờ dần).
- `CameraControl/ZoneSelectHighlight`: theo dõi `CameraController.Current`, không sửa luồng chạm.
- `FarmView`: khung mờ khít ô cho ô chờ, khung co giãn cho ô đang làm; `ShowPlotSelection` / `HidePlotSelection`. `UnlockDialog` bật / tắt khung (tự tìm `FarmView`). `FarmCharacterController`: dấu điểm đến dùng khung (bỏ texture tự tạo). Đổi tên trường màu / thứ tự để giá trị mặc định mới có hiệu lực.
- Editor: menu **Về Quê → Khung chọn: tạo asset + gán vào scene** (tạo `Assets/_Game/Data/SelectBoxSkin.asset`, gán vào `FarmView`, `FarmCharacterController`, thêm `ZoneSelectHighlight` lên camera; cảnh báo nếu import sai).
- Biên dịch ngoài Unity: không lỗi.

### Còn lại
- [ ] Người dùng: 4 file selectbox đặt Compression **None** (PPU 16, Point đã đúng; tắt mipmap nếu đang bật) → chạy menu Khung chọn → lưu scene → chơi thử: khung sắc nét ở toàn cảnh (5x) và zoom (~10x), phân biệt ô chờ / đang làm / đích đi; Test Runner vẫn xanh.

## Phần 6c mục 2 – Đơn hàng: thời gian chờ, hạng Khẩn cấp, giới hạn ngày (đã viết code, chờ kiểm tra – 12/11/2026)

### Quyết định (người dùng duyệt 12/11/2026, thay thiết kế 07/11)
- 5 ô đơn. Giao xong → chờ 10 phút (đơn kế khẩn cấp: 30 phút). Bỏ đơn → 5 phút (khẩn cấp vẫn 30). Hạng đơn kế tung lúc ô trống.
- Khẩn cấp: 20%, thưởng xu + XP ×2, hết hạn 30 phút kể từ lúc xuất hiện → ô trống, chờ như khi giao. Thay ô đơn gấp riêng.
- Giới hạn 15 đơn giao/ngày (giờ địa phương); bỏ đơn, hết hạn không tính; đủ thì không sinh đơn mới tới 0 giờ.
- Chưa chạy lại mô phỏng cân bằng (`sim2.py` không có trong repo) → chơi thử rồi chỉnh số trong `FarmDefinition`.

### Đã làm
- Logic: `OrderRank`; `OrderState.Rank` / `ExpiresAt`; `OrderDef` thêm thời gian chờ, tỉ lệ / % thưởng / hạn khẩn cấp, giới hạn ngày (tham số tùy chọn, mặc định = hành vi cũ); `FarmState.OrderDay` / `DeliveredToday` / `UtcOffsetSeconds`; `Farm`: `SetUtcOffset`, `CurrentDay`, `SecondsUntilNextDay`, `OrdersLeftToday`, `CanReceiveOrderToday`, `TryGetOrderTimeLeft`; `FarmResult.OrderExpired`. Lỗi cũ: giao xong sinh đơn mới ngay (không chờ) – đã sửa.
- Save `version` 4 (`rank`, `expiresAt`, `orderDay`, `deliveredToday`, `utcOffsetSeconds`); save 1–3 nạp được.
- Test: `OrderTests` thêm 11 ca (chờ sau giao, khẩn cấp ×2 + hạn, hết hạn, chờ 30 phút sau giao / bỏ, tỉ lệ ~20%, giới hạn ngày, sang ngày, bỏ đơn không tính, múi giờ, save khứ hồi, save 3 → 4); sửa `Save_CurrentVersionIs4`, `CharacterLookTests` dùng `SaveData.CurrentVersion`. NUnit ngoài Unity (Logic + test thuần): 218/218 xanh.
- View: `FarmDefinition` thêm 6 trường (mặc định 10 / 30 / 20% / 200% / 30 / 15), `_orderSlots` 3 → 5 (cả asset và `DefaultDataGenerator`); `WebBridge.GetUtcOffsetSeconds` + jslib `VeQue_GetUtcOffsetMinutes`; `GameBootstrap` gọi `Farm.SetUtcOffset` lúc mở game; `OrderCard` (màu nền + nhãn khẩn cấp), `OrderPanel` (dòng "Còn N đơn hôm nay", đếm ngược theo hạng, hết lượt, báo hết hạn); `UiBuilder` dựng 5 thẻ + dòng đầu bảng.
- Tài liệu: `MVP-Plan.md` Phần 6b mục B, 6c mục 2, thay đổi 12/11; `CLAUDE.md` (5 đơn, hạng, giới hạn ngày).
- Biên dịch ngoài Unity: không lỗi.

### Còn lại
- [ ] Người dùng: chạy **Về Quê → Tạo UI** (5 thẻ, nhãn khẩn cấp, dòng đầu bảng), Test Runner, chơi thử bằng **Tua thời gian**: giao đơn → ô đếm ngược 10 / 30 phút; thấy đơn khẩn cấp (thẻ khác màu, đếm ngược hạn), để hết hạn; giao đủ 15 đơn → hết đơn tới 0 giờ; save cũ nạp được.
- [ ] Build WebGL kiểm tra jslib múi giờ (cú pháp `.jslib` chưa được Unity xử lý thật).

## Phần 3b bước 4 – UI Sunnyside (khung + màu: người dùng xác nhận ổn 12/11/2026)

### Quyết định
- Dùng UI Sunnyside (`UI/9slice_box_white`, `label_*`), không dùng Sprout Lands UI Pack (khác phong cách, sẽ xóa).
- 1 pixel art = 2 đơn vị UI (Canvas 390 ngang → 6 px trên màn 1170).
- 12/11 (người dùng: panel hơi đậm, thêm sắc độ): tô lại khung `w` (giữ viền, đổi vệt sáng + nền) thành nhiều sắc độ: paper `f7ebd2`, cream `ead4aa`, sand `d9ae84`, tan (`lt` gốc), brown `b86f50`, dark (`dt` gốc), gray (`w` gốc), nút green / red / blue; nhãn paper / cream / tan.
- Phối: tấm bảng + hộp thoại = cream, thẻ / hàng / ô nhập = paper, thanh tab = sand, tab = nhãn paper, ô xu / cấp = nhãn cream, nút xanh = green, nút nâu = brown; chữ nâu đậm khắp nơi; đơn khẩn cấp nhuộm paper thành hồng cam nhạt.

### Đã làm
- Editor: menu **Về Quê → UI Sunnyside: ghép 9-slice** ghép 9 mảnh 3×3 thành sprite 9×9 (Border 3), tô thêm sắc độ, ghép nhãn 24×13 (Border 4) vào `Assets/_Game/Art/UI/` (`ui_box_<tên>.png`, `ui_label_<tên>.png`; xóa 4 file tên cũ nếu có) (PPU 16, Point, không nén, không mipmap); không sửa asset gốc.
- `UiBuilder`: hàm `Frame` (Image Sliced, `pixelsPerUnitMultiplier` theo `UiPixelScale`), `MakeButton` chọn khung theo màu; chưa ghép sprite thì vẫn dựng màu phẳng như cũ.
- Biên dịch ngoài Unity: không lỗi.

### Còn lại
- [x] Người dùng chạy menu ghép → **Tạo UI**: màu ổn. (Lỗi đã sửa: menu ghép từng xóa nhầm `ui_box_dark.png` vì trùng tên file cũ; `UiBuilder` thiếu 1 sprite thì cả UI về màu phẳng → nay chỉ chỗ thiếu.)
- Icon + thanh (12/11/2026, đã viết code, chờ người dùng xem):
  - Menu ghép thêm `ui_bar.png` (khung `greenbar_00`, Border 2/3/2/2) và chép `ui_icon_confirm / cancel / sandtimer / stopwatch.png`.
  - Thanh XP (HUD) và thanh tiến độ cây (popup): khung `greenbar` cao đúng 7 pixel art, phần đầy màu `#63c74d` co giãn liên tục (không nhảy theo 7 nấc của `greenbar_00–06`); chữ XP trắng.
  - Nút: dấu tích cho Giao / Mở / Tuyệt / Bắt đầu; dấu X cho Bỏ / Hủy; nút đóng bảng (X) và nút Đóng màn nhân vật chỉ còn icon (thiếu sprite thì hiện chữ). Nút Mua / Bán trong Kho / Cửa hàng chưa có icon.
  - Đồng hồ cát ở dòng chờ đơn; đồng hồ bấm giờ ở popup tiến độ cây (hộp 112×50).
- [x] Người dùng xem icon + thanh XP: ổn.
- Font pixel VT323 (12/11/2026, đã viết code, chờ người dùng xem):
  - Tìm hiểu: VT323 (OFL, Google Fonts) chắc chắn có tiếng Việt → người dùng thêm `Assets/_Game/Art/Font/VT323-Regular.ttf`. Kiểm tra file: đủ 146 chữ Việt (134 có dấu + gốc + đ/Đ). Ứng viên hợp phong cách hơn: FatPix VN (chưa rõ giấy phép thương mại).
  - VT323: em 1000, pixel thiết kế cao 80 / rộng 40 đơn vị, chữ hoa 0,56 em, rộng cố định 0,4 em.
  - Editor: menu **Về Quê → Font: tạo VT323 tiếng Việt** → `Assets/_Game/Art/Font/VT323 Pixel.asset` (TMP Static, RASTER_HINTED, lấy mẫu 25, atlas 512, lọc Point; ASCII + 146 chữ Việt + ký hiệu UI `·×→…`); tĩnh nên build không mang file .ttf.
  - `UiBuilder`: mọi nhãn dùng font này (chưa có thì font mặc định); cỡ chữ làm tròn: < 17 → 50/3, còn lại → 25 (sắc nét trên màn 1170, Canvas ×3); tab bỏ tự co cỡ.
  - Người dùng xem: VT323 bẹp, dính → đổi sang **FatPix VN** (NgnDang, itch.io: miễn phí cả thương mại, không bán lại, nên ghi công). Đã xóa file VT323.
  - FatPix `FatPix-VN-SVG.otf`: nét CFF + bảng SVG màu (1,9 MB, TMP bỏ qua); đủ 146 chữ Việt + ASCII; thiếu `·` `→` `…` → fallback font mặc định TMP. 1 pixel thiết kế = 43,7778/1000 em (đọc từ SVG).
  - Menu đổi tên **Về Quê → Font: tạo font pixel tiếng Việt** → `Assets/_Game/Art/Font/FatPix Pixel.asset` (lấy mẫu 46, atlas 1024). Cỡ chữ UI: < 15 → 15,2; 15–19 → 22,8; ≥ 20 → 30,5 (2 / 3 / 4 px màn hình mỗi pixel thiết kế khi Canvas ×3); tab dùng cỡ nhỏ.
  - Người dùng: nhiều chỗ tràn → đo độ rộng thật (hmtx FatPix) các chuỗi dài nhất so với khung: tràn ở xu HUD, Dừng, Nhân vật, tên cây, info Cửa hàng, tab "Đơn hàng (N)", toast, món trong thẻ đơn, nút Giao, dòng hết đơn, Kho trống.
  - Sửa: cỡ chữ còn 2 bậc (cỡ cũ < 20 → 15,2; ≥ 20 → 22,8); chữ trong nút một dòng, dài thì "…"; tab rộng hơn (padding 4, spacing 3, chữ tràn nhẹ lên viền nhãn); món trong thẻ đơn 2 dòng "tên / số lượng"; nút Giao / Bỏ 78; dòng thưởng một dòng.
  - Outline (duyệt cách 1, 12/11/2026, đã viết code): shader `Assets/_Game/Art/Shaders/PixelTextOutline.shader` ("VeQue/TMP Pixel Outline", dựa trên TMP Mobile/Bitmap): lấy 8 điểm quanh cách `_OutlineTexels` (2 texel = 1 pixel thiết kế), tô `_OutlineColor` (mặc định `#181425` viền Sunnyside); `_OutlineCorners` 0/1 (4 / 8 hướng); `_Padding` 2 để TMP nới khung chữ. Atlas padding 2 → 4. Menu font tạo / cập nhật `Assets/_Game/Art/Font/FatPix Outline.mat` (giữ file cũ, chỉ gắn atlas mới). `UiBuilder`: chữ trắng trên nút xanh / nâu và chữ XP dùng material viền.
  - Người dùng: chữ vẫn chưa sạch. Nguyên nhân: nét CFF của FatPix ghép theo từng chữ với tỉ lệ riêng (1 pixel = 43,2–45,1 đơn vị, lệch ~0,5 pixel) → vẽ lại ra atlas thì pixel dày mỏng không đều; Canvas không ×3 nguyên ở nhiều màn / Game view.
  - Sửa (duyệt bước 1 + 2, 12/11/2026): menu font đọc thẳng bảng SVG (ô `<rect>` đúng lưới, xử lý chữ lật `scale(s,-s)`, gzip) → atlas 512 Alpha8, 1 pixel thiết kế = 1 texel, padding 3; khoảng cách chữ đều (advance = rộng + 1; dấu cách theo hmtx); FaceInfo đo từ chữ thật, PointSize 24. Cỡ chữ UI: 16 (2 px / pixel) và 24 (3 px). Viền 1 texel (`_OutlineTexels` = `_Padding` = 1). Canvas bật `pixelPerfect`.
  - Lỗi lần chạy đầu: mất chữ có dấu / Latin-1 (â, ấ, ạ, ô, à…). Nguyên nhân: 23.141 ô vẽ bằng `<path d="M x y L … Z">` (không phải `<rect>`), và chữ ghép có nhiều nhóm `<g transform>` (chữ gốc + dấu, tỉ lệ / lệch riêng) – bản đầu chỉ đọc `<rect>` và transform đầu tiên. Sửa: đọc cả path, từng nhóm; đưa mọi nhóm về lưới nhóm đầu bằng tâm ô rồi làm tròn xuống (thử bằng Python: không chữ nào rỗng, dấu đúng chỗ). Đỉnh dòng tính theo chữ thường có dấu (chữ hoa có dấu được nhô lên) để chữ canh giữa nút.
  - Bước 3 (chưa làm, chờ người dùng chọn): ép tỉ lệ Canvas nguyên để sạch pixel trên mọi màn.
  - Chờ: người dùng chạy menu font → **Tạo UI** → xem ở Game view 1170×2532, Scale 1x; ghi công FatPix khi phát hành.
- [ ] Build lại so Build Report.

## Phần 10 giai đoạn 2 – Vùng câu + di chuyển (đã viết code, chờ kiểm tra – 12/11/2026)

### Quyết định (người dùng duyệt 12/11/2026)
- **Chuyển cảnh bằng màn tối** (không đi bộ liền mạch): đi ra cổng → tối 0,25 giây → đứng ở cổng vùng mới → sáng. Vùng câu Tilemap riêng (`FishingGround` đi được, `FishingWater` chặn), đặt ở x = −60 (camera nông trại không thấy; giới hạn camera nông trại vẫn lấy từ Ground).
- Đang có hàng việc mà đi câu: hủy, trả hạt, toast "Đã hủy N việc để đi câu". Bấm Đi câu lần nữa khi đang đi ra cổng: bỏ qua đoạn đi bộ. Chạm đất chỗ khác: hủy chuyến.
- Zoom vùng câu = toàn cảnh nông trại. Camera đi theo nhân vật (mượt), kéo để xem, thả tay 2 giây tự về.
- HUD vùng câu: ẩn thanh tab, nút Nhân vật, nút Đi câu; hiện tên vùng "Bến sông" + nút Về nhà. Giai đoạn 2 chưa câu: tới chỗ câu → quay mặt ra nước, toast "Câu cá: sắp có".
- Không lưu vùng (luôn vào game ở nông trại).

### Đã làm
- View: `PlayRegion` (khung, loại, tên, cổng, chỗ câu), `FishingSpot` (ô đứng, hướng, vùng chạm, icon cần nhấp nhô), `RegionTravel` (đi ra cổng → màn tối → đổi vùng / camera → sáng; chạm cổng, chạm chỗ câu), `CameraFollow`, `ScreenFader`. `CameraController`: `UseRegionBounds` / `UseMapBounds`, `JumpTo`, `MoveTowards`, `LastPanTime`. `WalkGridSource`: `_extraGround` / `_extraBlockers`. `FarmCharacterController`: `WalkTo(ô, khi tới)`, `PlaceAt`, `Face`, `Body`, chạm đất qua `RegionTravel` trước. `GameUI`: nút Đi câu / Về nhà, khung tên vùng, `_farmOnly`.
- Editor: menu **Về Quê → Vùng câu: tạo tạm** (tile Sunnyside cỏ / đất / nước → `Assets/_Game/Art/Tiles/Fishing/`; vùng 24×26, hồ trái, 3 bến, lối từ cổng; GameObject `Regions` + `RegionTravel`; `CameraFollow` lên camera; gán vào `WalkGridSource`, nhân vật). Cổng nông trại mặc định ô (0, 8). `UiBuilder`: nút Đi câu (dưới Nhân vật), Về nhà, nhãn vùng, màn tối (trên cùng). Menu ghép UI thêm icon `rod`.
- Biên dịch ngoài Unity: không lỗi. Logic không đổi.

### Còn lại
- [ ] Người dùng: **UI Sunnyside: ghép 9-slice** (icon cần) → **Vùng câu: tạo tạm** → **Tạo UI** → lưu scene → thử: Đi câu (có / không có hàng việc, bấm 2 lần), camera đi theo + kéo xem 2 giây tự về, chạm chỗ câu, không đi trên nước, Về nhà, chạm cổng; zoom khu / hàng việc như cũ sau khi về.
- [ ] Art vùng câu thật (người dùng thêm sau), chỉnh cổng nông trại nếu map khác.

## Phần 10 giai đoạn 3 – Câu cá: Logic + test (XONG – 12/11/2026, Test Runner 354/354)

### Quyết định (người dùng duyệt 12/11/2026)
- View chạy phần thời gian thực (thanh lực, giật câu, nhập mũi tên + chốt nhịp) rồi báo kết quả; Logic chọn cá, lịch rỉa / cắn, đề vòng kéo, thể lực / độ căng, cân nặng, giá, XP, độ bền. Ngẫu nhiên qua `SeededRandom` (seed lấy từ `FarmState.RandomState`).
- Giỏ cá 8 ô, nâng cấp được (data: 8 → 10 → 12 → 16 → 20, giá 300 / 1.000 / 3.000 / 8.000). Đào được giun khi thu hoạch (10%, 1–2 con). Mở câu cá từ cấp 1 (người dùng quy định lại sau), tặng cần tre + 10 giun 1 lần.
- Số đề xuất (chưa mô phỏng cân bằng): giá cá Thường 8–15 → Huyền thoại 600–1.200; XP 1 / 3 / 8 / 20 / 50; cần tre (tặng, 30 bền) → trúc 300 → thủy tinh 1.500 → carbon 6.000 → vàng 20.000; sửa = 40% giá; mồi giun 2 (không bán, đào được), cám 3, tép 5, mồi giả 8, mồi thơm 25.
- Mỗi lần ném trừ 1 mồi + 1 bền ngay lúc ném (bỏ dở vẫn mất); đứt dây −5 bền, 20% gãy; bền về 0 = hỏng; sửa đầy lại. (Đổi 12/11, xem mục Sửa độ bền cần bên dưới.)

### Đã làm
- Logic: `DaySlot` / `DaySlots` (7 khung, theo múi giờ); `FishingDef.cs` (`FishRarity`, `FishDepth`, `FishDef`, `RodDef`, `BaitDef`, `FishingTuning` – mọi con số, `FishingDef` + `None`); `FishingState.cs` (`RodState`, `CaughtFish`, `FishingState`); `FishingSession.cs` (máy trạng thái Waiting → Pulling → Finished; `Classify`, `Strike`, `Timeout`, `NextRound` / `SubmitRound`, `Abandon`; `PullRound` + mũi tên đỏ); `Farm.Fishing.cs` (`Farm` thành `partial`: `CanCast`, `Cast`, `CompleteFishing`, `BuyRod`, `EquipRod`, `RepairRod`, `BuyBait`, `UpgradeBasket`, quà mở câu cá, đào giun khi thu hoạch + sự kiện `BaitFound`); `FarmDef.Fishing`, `FarmState.Fishing`, 12 `FarmResult` mới.
- Chọn cá: trọng số độ hiếm × khung giờ × mồi ưa thích (×3); chỉ cá có độ sâu khớp lực ném; Hiếm trở lên nhân thưởng (Perfect +50%, Tốt +15%, cần, mồi); ném hỏng chỉ ra cá Thường.
- Save `version` 5 (`SaveFishing`, `SaveRod`, `SaveFish`); save 1–4 nạp được (được tặng cần + mồi); mục hỏng bỏ qua.
- Test `FishingTests` (51 ca: khung giờ ranh giới + múi giờ; quà mở; ném trừ mồi / bền, chặn khi hết mồi / cần hỏng / giỏ đầy; xếp hạng ném; cá hệ số 0 không ra, cá Huyền thoại ra được lúc bình minh, ném hỏng chỉ cá Thường, mồi ưa thích tăng tỉ lệ, cùng seed cùng kết quả; giật sớm / đúng / trễ / trượt; kéo Perfect bắt được, Perfect mạnh hơn Tốt, sai liên tục đứt dây; mũi tên đỏ; giỏ, XP, kỷ lục; đứt dây trừ bền + gãy; bền 0 hỏng; sửa, mua cần / mồi, nâng giỏ; đào giun; save khứ hồi, save 4 → 5, mục hỏng). `Save_CurrentVersionIs5`. NUnit ngoài Unity: 269/269 xanh.

### Còn lại
- [x] Test Runner trong Unity (12/11): 353/354 – lỗi `FarmDefinitionTests.ToDef_DefaultData_Orders` còn chờ 3 ô đơn (data đã 5 ô từ 6c mục 2) → đã sửa test, thêm kiểm các số mới (chờ, khẩn cấp, 15 đơn/ngày). Chạy lại: người dùng xác nhận tất cả ổn (354/354).
- [ ] Giai đoạn 4: ScriptableObject cá / cần / mồi + data mặc định (13 cá, 5 cần, 5 mồi) trong `DefaultDataGenerator`; giao diện câu (ném + chờ cắn + giật; kéo mũi tên); cửa hàng cần / mồi, sửa cần, nâng giỏ; nhân vật `CASTING` / `WAITING` / `REELING` / `CAUGHT`; art cá (người dùng).
- [ ] Mô phỏng cân bằng có câu cá (thu nhập / giờ so với trồng cây).
- [ ] Giai đoạn 5: chống kẹt tính cá trong giỏ (`MVP-Plan.md` Phần 6b mục Chống kẹt).
- [ ] Data giai đoạn 4: có vài cá Thường cả ngày, ở độ sâu Gần.

### Sửa độ bền cần (XONG code – 12/11/2026, người dùng duyệt)
Quyết định (sau khi kiểm tra logic câu cá):
- Chốt nhịp Trượt mà nhập đúng vẫn trừ thể lực cá (40) – giữ.
- Data luôn có vài cá Thường cả ngày (đường dự phòng chọn cá chỉ dùng khi data thiếu).
- Lý do mất độ bền giữ nguyên: ném −1, đứt dây −5 + có thể gãy; câu được cá to / hiếm không mất thêm.
- Gãy khi đứt dây: 20% (độ bền đầy) → 60% (về 0), tuyến tính theo độ bền còn / độ bền tối đa thực tế, tính sau khi trừ.
- Mỗi lần sửa độ bền tối đa giảm 10% độ bền gốc; độ bền tối đa đã dưới 15% thì không sửa được (`RodWornOut`) → mua lại.
- Mua lại cần đang có khi hỏng hoặc đã sửa: đủ giá, về như mới. Còn dùng được và chưa sửa → `AlreadyOwned`.

Đã làm:
- `FishingTuning`: `SnapBreakPercent` → `SnapBreakMinPercent` 20 / `SnapBreakMaxPercent` 60; thêm `RepairMaxLossPercent` 10, `RepairMinPercent` 15; hàm `MaxDurabilityPercent`, `MaxDurabilityAfterRepairs`, `CanRepairAgain`, `SnapBreakPercentAt`.
- `RodState.RepairCount`; `Farm`: `GetRodMaxDurability`, `CanRepairAgain`; `RepairRod` giảm độ bền tối đa, chặn `RodWornOut`; `BuyRod` mua lại được; `CompleteFishing` dùng tỉ lệ gãy theo độ bền; nạp save kẹp độ bền ≤ độ bền tối đa thực tế. `FarmResult.RodWornOut`.
- Save `version` 6 (`SaveRod.repairCount`); save 5 nạp được (chưa sửa lần nào). `Save_CurrentVersionIs5` → `Is6`.
- Test thêm 11 ca (sửa giảm dần tới dưới 15%, mua lại khi gãy / đã sửa / mòn hết, không mua lại khi còn tốt, tỉ lệ gãy 5 mốc, độ bền thấp gãy nhiều hơn, save giữ số lần sửa, save 5 → 6, kẹp độ bền khi nạp); sửa `Repair_RestoresAndCosts`. NUnit ngoài Unity: 280/280 xanh. View / Editor chưa dùng các API này (giao diện cửa hàng cần làm ở giai đoạn 4).
- [x] Người dùng: Test Runner trong Unity ổn (12/11/2026, dự kiến 365).
- Chống kẹt: ghi đầy đủ ở `MVP-Plan.md` Phần 6b.

## Phần 10 giai đoạn 4 – Câu cá: chơi (đang làm – 12/11/2026)

### Quyết định (người dùng duyệt 12/11/2026)
- Chia 5 đợt A–E (chi tiết `MVP-Plan.md` Phần 10 mục 7, giai đoạn 4).
- Cá Huyền thoại: không đổi chuỗi giữa chừng; độ khó thêm bằng mũi tên khác màu (bấm ngược) – đã có trong Logic.
- Giỏ đầy khi chưa bán được cá: thêm nút **Thả cá** trong bảng Đồ câu (tạm, tới giai đoạn 5).
- Bảng Đồ câu mở từ nút **Đồ câu** trên HUD vùng câu + nút chuyển "Hạt giống | Đồ câu" trong tab Cửa hàng.
- Data: 13 cá, 5 cần, 5 mồi theo bảng đề xuất 12/11; chỉnh số sau. Id mồi tép là `moi_tep` (khác cá `tep`).
- Art tạm (Sunnyside chưa có cá, phao): phao + icon cá theo màu độ hiếm, vẽ bằng công cụ Editor; người dùng thay sau.

### Đợt A – Data câu cá (XONG – 12/11/2026, người dùng xác nhận ổn)
- `View/Data`: `FishDefinition`, `RodDefinition`, `BaitDefinition`, `FishingDefinition` (`ToDef()`, `TryGetFish` / `TryGetRod` / `TryGetBait` để View lấy tên, icon); `FarmDefinition._fishing` → `FarmDef`. `FishingTuning` (Logic) thêm `[Serializable]` để chỉnh trong Inspector.
- Editor: `FishingDataGenerator` – menu **Về Quê → Tạo data câu cá** (cũng chạy trong **Tạo data mặc định**): `Assets/_Game/Data/Fishing/{Fish,Rods,Baits}/`, `Assets/_Game/Data/FishingDefinition.asset`, gán vào `FarmDefinition`; chạy lại chỉ ghi số, giữ icon; con số chung về mặc định `FishingTuning`. `FishingPlaceholderArt` – menu **Về Quê → Câu cá: tạo art tạm**: `Assets/_Game/Art/Fishing/bobber.png`, `fish_{common,uncommon,rare,epic,legendary}.png` (PPU 16, Point, không nén, tắt mipmap), gán icon cho cá chưa có icon / đang dùng icon tạm.
- Test `FarmDefinitionTests` thêm 6 ca: data bật câu cá (13 / 5 / 5, quà, giỏ, giun); nông trại mới có cần tre + 10 giun, ném được; khung giờ nào cũng có cá Thường gần bờ; cá Hiếm trở xuống ≥ 2 khung, mồi ưa thích tồn tại, chỉ cá Huyền thoại có mũi tên đỏ; cần sau tốt hơn / đắt hơn, sửa rẻ hơn mua; cá / cần / mồi có tên, cá có icon.
- Biên dịch Logic + View + Editor + Tests ngoài Unity: không lỗi. NUnit Logic ngoài Unity 280/280.
- [x] Người dùng: **Tạo data câu cá** → **Câu cá: tạo art tạm** → Test Runner → xem `FishingDefinition.asset`: ổn.
- Sửa kèm: thông báo (`Toast`) bị nhãn tên vùng che ở vùng câu → dời thông báo xuống dưới nhãn (`UiBuilder.BuildToast`, `HudHeight + 48`). Người dùng xác nhận ổn.

### Đợt B – Hoạt ảnh câu của nhân vật (XONG – 13/11/2026, người dùng cho sang Đợt C)
- `CharacterAnim` thêm `Cast`, `Wait`, `Reel`, `Caught`; `CharacterSkin` thêm clip `_cast` (15 khung, 15 fps, 1 lần = 1 giây), `_wait` (9 khung, 6 fps, lặp), `_reel` (13 khung, 12 fps, lặp), `_caught` (10 khung, 10 fps, 1 lần) + khung tóc tương ứng trong `HairSet`.
- `CharacterSkinAssigner` thêm 4 strip `CASTING` / `WAITING` / `REELING` / `CAUGHT` (đã đo: chân ở hàng pixel 25 như các strip cũ → dùng chung pivot).
- `CharacterView`: chuột phải component → **Thử: ném cần / chờ cá / kéo cá / được cá** (khi đang Play).
- Test mới `CharacterSkinTests` (12 ca): mỗi hoạt ảnh đủ khung ở thân, dụng cụ, 6 kiểu tóc; ném / được cá chạy 1 lần, chờ / kéo lặp.
- Biên dịch ngoài Unity: không lỗi.
- Ghi cho Đợt C: art Sunnyside `WAITING` / `REELING` vẽ sẵn dây câu thòng xuống, đầu dây cố định ở (+30 px, −22 px) so với chân (nhìn phải), không có phao; `CASTING` có phao đỏ bay. Phao / độ xa phải tính theo điểm này (đề xuất ở Đợt C).
- [x] Người dùng: **Nhân vật Sunnyside: import + gán sprite** → Test Runner → thử 4 hoạt ảnh.

### Đợt C – Ném, chờ cắn, giật (XONG tạm – 13/11/2026, người dùng: tạm thời ổn)
Quyết định (duyệt 13/11/2026): phao cố định ở đầu dây vẽ sẵn (cách A), độ sâu chỉ hiện bằng chữ; trước Đợt D móc được cá thì báo "Móc được …! (kéo cá: Đợt D)" rồi bỏ lượt (mất mồi); bố cục HUD tạm như kế hoạch.
- Logic: `FishingMath.CastPower` (thanh lực 0 → 1 → 0 theo chu kỳ cần), `FishingMath.IsNibbling`; `DaySlots.SecondsUntilEnd` (còn bao lâu hết khung, qua nửa đêm). Test thêm 16 ca trong `FishingTests`.
- View: `Fishing/FishingController` (Away → Ready → Charging → Casting → Waiting → Result; phao ở đầu dây (+30, −22 px so với chân, lật theo hướng), rỉa nhấp 1 px, cắn chìm 2 px + "!" + rung; Strike / Timeout / CompleteFishing; bỏ lượt khi Thu cần, đi qua cổng / Về nhà (`TravelStarted`), ẩn tab (`PageHiding`); đang câu khóa chạm đất). `UI/FishingHud` (khung giờ đếm ngược, cần + độ bền / độ bền tối đa, giỏ, mồi chạm để đổi, thanh lực + vùng Tốt / Perfect theo cần, nút Ném / Giật, Thu cần). `UI/HoldButton` (báo chạm xuống + nhả). `FarmCharacterController.InputLocked`, `View`, `Facing`. `GameUI` bỏ thông báo "Câu cá: sắp có". `WebBridge.Vibrate` + `VeQue_Vibrate` (jslib, `navigator.vibrate`). `UiFormat`: tên khung giờ, chất lượng ném, độ sâu, thông báo kết quả, lý do không ném được (test thêm 15 ca).
- Editor: `UiBuilder.BuildFishing` (HUD) + `EnsureFishingController` (trên `Regions`, con `Bobber` / `BiteAlert`); `FishingPlaceholderArt` thêm `alert.png`.
- Biên dịch ngoài Unity (Editor + WebGL): không lỗi. NUnit ngoài Unity (Logic + UiFormat): 328/328.
- [x] Người dùng: tạo art tạm → Tạo UI → Test Runner → chơi thử: tạm thời ổn (vị trí phao, "!", thanh lực có thể chỉnh lại sau trên máy thật).

### Đợt D – Kéo cá 1 lượt (đã viết code, chờ kiểm tra – 13/11/2026)
Quyết định (người dùng 13/11/2026, thay thiết kế cũ nhiều vòng + chốt nhịp):
- Không có nút Chốt: chỉ cần nhập đúng. Mỗi lần câu chỉ 1 lượt (1 chuỗi). Càng nhanh càng hoàn hảo: ≤ 40% thời gian Perfect (+50% XP, cân nặng u¹), ≤ 70% Tốt (u²), còn lại Thường (u³).
- Bấm sai = đứt dây (có thể gãy cần, 20% → 60% theo độ bền); hết giờ = cá thoát (chỉ mất mồi).
- Cần: "sức bền dây" đổi thành thêm thời gian kéo (0 / 10 / 20 / 30 / 50%).
- **Mọi mức trừ độ bền là 1**: chỉ trừ 1 lúc ném; đứt dây không trừ thêm (`SnapDurabilityCost` = 0, chỉnh được trong data).
- Nút: 4 mũi tên chữ thập ở giữa dưới; Thu cần góc phải dưới cùng.

Đã làm:
- Logic: `FishingSession` viết lại phần kéo: `Pull` (`PullRound`: mũi tên, đỏ, thời gian đã cộng thưởng cần), `CompletePull(giây)`, `WrongArrow()`, `PullTimeout()`, `Grade` (`PullGrade`), `GradeFor`; bỏ `NextRound` / `SubmitRound` / `BeatResult` / thể lực / độ căng / combo. `FishingOutcome.Escaped`. `FishDef`: bỏ `Rounds`, `BeatWindow`; `RoundSeconds` → `PullSeconds`. `RodDef.LineStrength` → `BonusTimePercent`. `FishingTuning`: bỏ các số thể lực / độ căng; thêm `PerfectTimePercent` 40, `GoodTimePercent` 70, `PerfectXpBonusPercent` 50, số mũ cân nặng 1 / 2 / 3; `SnapDurabilityCost` 0.
- Data: `FishDefinition` (`_pullSeconds`, bỏ `_rounds`, `_beatWindow`), `RodDefinition` (`_bonusTimePercent`), `FishingDataGenerator` theo bảng mới.
- View: `FishingController` thêm bước `Pulling` (phao giật 1 px, hoạt ảnh kéo; `PressArrow`; hết giờ) và `Landing` (hoạt ảnh được cá rồi mới áp kết quả); thông báo câu được "Câu được Cá rô 350 g! Perfect! · +2 XP", đứt dây làm gãy cần thêm "Cần gãy!". `FishingPullPanel` (chuỗi mũi tên – đỏ / đã nhập mờ, thanh thời gian đổi màu xanh → vàng → xám theo hạng còn đạt, 4 nút chữ thập, phím mũi tên). `FishingHud` ẩn nút Ném / mồi / dòng cần-giỏ khi kéo. `UiFormat`: `Weight`, `PullGradeName`, `CaughtText`, thông báo cá thoát.
- Editor: `SunnysideUiImporter` thêm icon mũi tên (`arrow_up-1` là mũi tên xuống); `UiBuilder.BuildPullPanel`; nút Thu cần xuống sát đáy.
- Test: `FishingTests` viết lại phần kéo (hạng theo thời gian, câu được, chậm = thoát, sai = đứt dây, Perfect nặng hơn + XP +50%, cần thêm thời gian, mũi tên đỏ); `UiFormatTests` thêm cân nặng, thông báo câu được, cá thoát. NUnit ngoài Unity (Logic + UiFormat): 345/345. Biên dịch Editor + WebGL: không lỗi.
- [ ] Người dùng: **UI Sunnyside: ghép 9-slice** (thêm mũi tên) → **Tạo data câu cá** (số mới) → **Tạo UI** → Ctrl+S → Test Runner (dự kiến 431) → chơi thử: kéo nhanh / chậm / bấm sai / hết giờ, mũi tên đỏ (cá Huyền thoại – có thể tạm sửa `_redArrows` của cá rô để thử), số giỏ + XP tăng khi câu được, Thu cần / Về nhà lúc đang kéo.

### Đợt D – sửa nhịp + bóng cá + icon cá thật (đã viết code, chờ kiểm tra – 13/11/2026)
Quyết định (người dùng 13/11/2026): cho sửa `.meta` icon cá qua menu; chuyển file phụ của art cá ra `ArtSource/fish/`; 7 cá thừa (rô phi, chạch, thát lát, lăng, anh vũ, hồ, trôi) để giai đoạn 5; giật sớm **vẫn phạt** (cá hoảng, mất mồi).

Đã làm:
- Asset: 20 icon 16×16 ở `Assets/_Game/Art/Fishing/fish/` (PPU 16, Point, không nén – đúng; nhưng Sprite Mode Multiple với sprite con tên `Piknik basket_0` → menu đổi sang Single). Script / json / preview chuyển ra `ArtSource/fish/` (ngoài `Assets`).
- Editor: menu **Câu cá: import sprite cá** (`FishingSpriteImporter`): icon Single + gán `FishDefinition._icon` theo id (`tom_cang_xanh` → `tom_cang`); chép `small fish` / `mediuml fish` (Ocean Pack, Sprout Lands) sang `Assets/_Game/Art/Fishing/shadow/`, cắt 4 khung theo hướng (bó sát hình). `FishingPlaceholderArt` chỉ thay icon tạm `fish_<độ hiếm>.png` (trước đây sẽ ghi đè icon thật). `UiBuilder`: tạo `FishShadow` (sorting 299, dưới phao), chữ "Sẵn sàng..." trên bảng kéo.
- View: `FishShadow` (bóng tối mờ dưới phao: lượn nhẹ, lao 4 px theo mũi tên bấm đúng, câu được → kéo về nhân vật, sai / hết giờ / thu cần → bơi đi mờ dần; cá Thường / Khá dùng bóng nhỏ, Hiếm trở lên bóng vừa). `FishingController`: phao chìm 3 px (trước 2), "!" nảy 1 px, `BiteShown`; xem trước chuỗi `PullPreviewSeconds` rồi mới tính giờ. `FishingHud`: nút "Chờ..." → "GIẬT!" phập phồng khi cá cắn. `FishingPullPanel`: "Sẵn sàng..." lúc xem trước.
- Số: chờ cắn 3–15 → 2–8 giây; khung giật 0,9/0,8/0,6/0,5/0,4 → 1,6/1,4/1,2/1,0/0,8 giây; Huyền thoại 8–10 → 7–9 mũi tên; `PullPreviewSeconds` 1,2.
- Biên dịch Logic + View + Editor + Tests (dotnet, ngoài Unity): không lỗi. Không thêm test (Logic chỉ đổi số mặc định).
- [ ] Người dùng: **Câu cá: import sprite cá** → **Tạo data câu cá** → **Tạo UI** → Ctrl+S → Test Runner (dự kiến 431) → chơi thử: để ý thấy cá cắn, giật sớm bị phạt, xem trước chuỗi, bóng cá lao theo mũi tên / bị kéo vào / bơi đi; icon cá đúng trong thông báo / giỏ.
- Sprite bóng cá lấy từ gói Sprout Lands (đã chép sang `_Game`, xóa gói vẫn còn); kiểm tra giấy phép như các asset khác.

### Đợt D – bấm sai nhập lại + sửa bóng cá (đã viết code, chờ kiểm tra – 13/11/2026)
Người dùng: câu tạm ổn; bấm sai không mất lượt mà nhập lại từ đầu, có nháy đỏ; chọn **phương án B** (bỏ hẳn đứt dây / gãy cần khi kéo, cần chỉ hỏng khi hết độ bền). Báo lỗi: chưa thấy bóng cá ở phao.

Đã làm:
- Bóng cá không thấy: bóng đặt 3 px dưới phao, bóng nhỏ chỉ 6×3 px, phao (sorting 300) vẽ đè gần hết. Sửa: bóng đặt **trong nước trước phao, phía xa nhân vật** (`_waterOffsetPixels` 9, −2; lật theo hướng nhìn), mọi cá dùng **bóng vừa** 9×5 (`_mediumFrom` = Thường), màu đậm hơn (alpha 0,6 – trường `_color` trong scene vẫn giữ 0,5, chỉnh tay nếu cần).
- Logic: `FishingSession.WrongArrow()` không kết thúc phiên, trả `None`, đếm `Mistakes`. Bỏ `FishingOutcome.LineSnapped`, nhánh gãy cần trong `Farm.CompleteFishing`, `FishingTuning.SnapDurabilityCost` / `SnapBreakMinPercent` / `SnapBreakMaxPercent` / `SnapBreakPercentAt`. `RodState.Snapped` giữ cho save cũ.
- View: `FishingController` bấm sai → về mũi tên đầu, sự kiện `PullMistake`, bóng cá `Flinch` (vùng ra xa 6 px rồi về). `FishingPullPanel` nháy đỏ cả chuỗi 2 lần trong 0,35 giây. `UiFormat` bỏ "Bấm sai, đứt dây rồi!".
- Test: thay `Pull_WrongArrow_SnapsLine` bằng `Pull_WrongArrow_KeepsPullingAndCanStillCatch`; bỏ 4 test gãy cần (`Complete_Snap…`, `SnapBreakPercent…` 5 ca, `Snap_LowDurability…`) và ca `LineSnapped` trong `UiFormatTests` → dự kiến **423** test.
- Biên dịch Logic + View + Editor + Tests (dotnet): không lỗi.
- [ ] Người dùng: **Tạo UI** (không bắt buộc – chỉ đổi code; chạy lại nếu muốn chắc) → Test Runner (dự kiến 423) → chơi thử: bấm sai thấy nháy đỏ + về đầu + bóng cá vùng ra, nhập lại vẫn câu được; bóng cá thấy rõ trước phao (cả khi nhìn trái / phải).
- Sửa thêm (người dùng: đầu bóng cá chưa quay vào phao): bóng luôn quay đầu về phía phao, mũi tên chỉ làm bóng lao theo hướng (không quay); cá thoát thì quay đầu ra xa rồi bơi đi.
- `CLAUDE.md` còn câu "gãy phải sửa / mua mới" – vẫn đúng nghĩa cần hỏng (hết độ bền) phải sửa / mua; chưa sửa.

### Đợt D – chữ trên đầu nhân vật, cá bay lên, bảng cá vừa câu (đã viết code, chờ kiểm tra – 13/11/2026)
Người dùng: ném chỉ hiện chữ hoàn hảo / tuyệt / tốt trên đầu nhân vật; hụt cá cũng là chữ trên đầu; câu được thì hình cá bắn lên từ nước theo dây về nhân vật; thêm bảng cá vừa câu (tên, hình, cân nặng, rank, giá). Người dùng chốt: rank = độ hiếm của cá (hạng kéo ở dòng nhỏ). Ném yếu hiện "Yếu" (mặc định, chưa chốt).

Đã làm:
- `UiFormat`: `CastQualityName` → Hoàn hảo! / Tuyệt! / Tốt / Yếu; `FishingOutcomeText` → Sớm quá! / Mất mồi! / Trượt lưỡi! / Cá thoát!; thêm `RarityName`, `CatchDetail`; bỏ `CastResult`, `CaughtText`. Thu cần / hết mồi / không ném được vẫn dùng thông báo.
- View: `WorldPopText` (chữ nổi trên Canvas, bám điểm trên map, nảy rồi bay lên 8 px, mờ dần 0,9 giây); `CaughtFishFlyer` (icon cá trồi khỏi nước ở phao, bay cong ~0,55 giây về tay, lật hình theo hướng bay); `CatchResultPanel` (nền tối chặn chạm, OK). `FishingController`: sự kiện `CastJudged`, `Missed`, `FishCaught`; `HeadPosition` (`_headPixels` 0, 26), tay `_handPixels` (8, 10); Landing chờ cả hoạt ảnh lẫn cá bay. `FishingHud` hiện chữ nổi (màu theo hạng ném, hụt = đỏ nhạt).
- `UiBuilder`: `PopText` trong HUD câu, `CatchResult` trên HUD câu, `CaughtFish` (SpriteRenderer, sorting 320) dưới `Regions`.
- Test: `UiFormatTests` – `CastQualityName_PopText` (4), `RarityName_AllRarities` (5), `CatchDetail_GradeAndXp`; bỏ `CastResult` (4) → dự kiến **428**.
- Biên dịch Logic + View + Editor + Tests (dotnet): không lỗi.
- [ ] Người dùng: **Tạo UI** → Ctrl+S → Test Runner (dự kiến 428) → chơi thử: chữ ném trên đầu, chữ hụt (giật sớm / không giật / để hết giờ), cá bay lên đúng hướng (nhìn trái / phải), bảng cá vừa câu đúng thông tin; chỉnh `_headPixels`, `_handPixels` nếu lệch.

### HUD câu cá: đồng hồ chung, nút icon Cần / Giỏ / Mồi, bảng giỏ cá (đã viết code, chờ kiểm tra – 13/11/2026)
Người dùng (13/11): bỏ khung giờ ở HUD câu, làm 1 đồng hồ chung cho cả game; bỏ dòng cần / giỏ, thay bằng nút icon (Cần → chọn cần, Giỏ → giỏ dạng lưới, thêm nút Mồi). Chốt: đồng hồ hiện giờ + khung giờ dưới nút Sao lưu; bảng Cần có nút Sửa; icon mồi lấy asset có sẵn → không có giun / mồi trong project, dùng tạm `rod alt` (Sunnyside, cần có mồi đỏ).

Đã làm:
- Logic: `Farm.ReleaseFish(index)` (thả 1 con, kỷ lục giữ), `Farm.GetFishPrice(cá)`; `FishingMath.WeightValuePercent` / `FishPrice` (dùng chung với lúc câu); `FarmResult.UnknownFish` (thêm cuối enum). Save không đổi.
- View: `GameClock` (2 dòng "15:42" / "Chiều", chạm = thông báo khung giờ + còn bao lâu); `FishingHud` bỏ khung giờ / dòng cần-giỏ / nút mồi chữ, thêm 3 nút icon có số nhỏ (đỏ khi cần hỏng / giỏ đầy / hết mồi), khóa khi đang câu; `RodPanel`, `BaitPanel` (dòng `FishingItemRow`), `BasketPanel` (lưới `FishCell` 5 cột, tối đa 20 ô; chi tiết + Thả). `FishingController.CycleBait` → `SelectBait(id)`. `UiFormat.Clock`.
- Editor: `SunnysideUiImporter` thêm icon `basket`, `rod alt`; `UiBuilder` dựng đồng hồ, cột nút icon, 3 bảng (BottomSheet).
- Test: `FishingTests` – `ReleaseFish_RemovesOneKeepsRecord`, `ReleaseFish_BadIndex_Unknown` (2), `GetFishPrice_MatchesPriceWhenCaught`; `UiFormatTests` – `Clock_LocalTime` (4) → dự kiến **436**.
- Biên dịch Logic + View + Editor + Tests (dotnet): không lỗi.
- [ ] Người dùng: **UI Sunnyside: ghép 9-slice** (lấy icon giỏ / mồi) → **Tạo UI** → Ctrl+S → Test Runner (dự kiến 436) → chơi thử: đồng hồ ở nông trại + vùng câu, chạm đồng hồ; 3 nút icon + số; bảng Cần (Dùng, Sửa khi hỏng), Giỏ (chọn cá, Thả, giỏ hết đầy câu tiếp được), Mồi (đổi mồi, số trên nút đổi theo); nút khóa khi đang câu.
- Sửa 19/11/2026 (người dùng): đồng hồ chỉ hiện giờ 24h, **bỏ dòng khung giờ và toast khi chạm** (dòng khung giờ cũ bị ẩn vì ô cao 16 px < dòng font pixel cỡ 16, overflow Ellipsis). `GameClock` bỏ `_slot`, `_button`, `_toast`; `UiBuilder.BuildGameClock` bỏ label `Slot` + `Button`, `Time` chiếm cả khung, nền vẫn chặn chạm. `UiFormat.SlotStatus` / `DaySlots.SecondsUntilEnd` giữ (có test). → Người dùng: **Tạo UI** → Ctrl+S → kiểm đồng hồ hiện "23:05" căn giữa.

### Giỏ cá: ô vuông, 25 ô, mở từng ô (đã viết code, chờ kiểm tra – 13/11/2026)
Người dùng: ô cá vuông; lưới tối đa 25 ô; ô chưa mở hiện khóa, bấm mở ô gần nhất. Chốt: giá ô thứ k = 100 × 1,3^(k−1) (tròn 10; 100 … 6.650); có hộp xác nhận; ô 64 px, cuộn dọc.

Đã làm:
- Logic: bỏ mức nâng cấp giỏ; `FishingDef.BasketStartSlots` (8) + `BasketSlotPrices` (17 giá) + `BasketMaxSlots`; `FishingState.BasketSlots`; `Farm.BasketCapacity` = số ô đã mở, `CanUnlockBasketSlot`, `NextBasketSlotPrice`, `UnlockBasketSlot()` (thay `UpgradeBasket`). Save **`version` 7**: `basketSlots`; save ≤ 6 quy đổi `basketLevel` → 8 / 10 / 12 / 16 / 20 ô rồi kẹp theo data.
- Data: `FishingDefinition` `_basketStartSlots`, `_basketSlotPrices`; `FishingDataGenerator.BasketSlotPrice(k)`.
- View: `FishCell` thêm trạng thái khóa (ổ khóa; ô kế tiếp hiện giá, bấm được; ô sau mờ); `BasketPanel` 25 ô, hộp xác nhận "Mở thêm 1 ô giỏ (ô N) – Giá" (nút Mở mờ khi thiếu xu). `FishingPlaceholderArt`: ổ khóa tạm 10×12 (`lock.png`, không có icon khóa trong project). `UiBuilder`: ô 64×64, 25 ô, hộp xác nhận.
- Test: `UnlockBasketSlot_OneByOneUntilMax`, `UnlockBasketSlot_NotEnoughCoins`, `Save_V6BasketLevel_ConvertsToSlots` (3); `Save_RoundTrip_KeepsFishing` kiểm số ô; `FarmDefinitionTests` kiểm 8 / 25 / bảng giá; `Save_CurrentVersionIs7` → dự kiến **440**.
- Biên dịch Logic + View + Editor + Tests (dotnet): không lỗi.
- [ ] Người dùng: **Câu cá: tạo art tạm** (ổ khóa) → **Tạo data câu cá** (giá ô giỏ) → **Tạo UI** → Ctrl+S → Test Runner (dự kiến 440) → chơi thử: lưới 25 ô vuông, cuộn; ô khóa kế tiếp có giá, bấm → xác nhận → mở; thiếu xu nút Mở mờ; save cũ (nếu có giỏ đã nâng) nạp đúng số ô.

### Mua cần / mồi theo cấp, tab Đồ câu trong Cửa hàng (đã viết code, chờ kiểm tra – 13/11/2026)
Người dùng: làm hệ thống mua mồi và cần câu, đủ cấp mới mua được. Chốt: cấp mồi cám 1 / mồi tép 3 / mồi giả 6 / mồi thơm 9 (giun không bán); làm cả tab Cửa hàng ở nông trại; hộp lên cấp báo cần / mồi mới.

Đã làm:
- Logic: `BaitDef.UnlockLevel` (tham số cuối, mặc định 1); `Farm.BuyBait` trả `LevelTooLow` khi chưa đủ cấp; `LevelUnlocks.RodIds` / `BaitIds` (cần / mồi bán ở Cửa hàng mở đúng cấp đó, câu cá đang bật). Cần đã có cấp (`RodDef.UnlockLevel`), giữ nguyên. Save không đổi.
- Data: `BaitDefinition._unlockLevel`; `FishingDataGenerator` ghi cấp mồi.
- View: `FishingItemRow` tối đa 3 nút (trái → phải), kiểu dòng Thường / Đang dùng / Khóa; `RodListView`, `BaitListView` (tự vẽ lại khi bật + khi tiến trình đổi) dùng cho bảng Cần / Mồi ở vùng câu và trang Đồ câu của Cửa hàng; `RodPanel` / `BaitPanel` chỉ còn mở bảng; `ShopTabs` ("Hạt giống | Đồ câu", mở Cửa hàng về Hạt giống); `LevelUpDialog` thêm "Mở <cần / mồi>"; `UiFormat.ShopBlocked` (thiếu xu, chưa đủ cấp…).
- Editor: `UiBuilder` – dòng mẫu 84 px 3 nút, `BuildShopFishingTab` (nút chuyển cạnh tiêu đề Cửa hàng, trang Cần câu + Mồi câu).
- Test: `BuyBait_NeedsLevel`, `GetUnlocksAtLevel_ListsRodsAndBaits`, `ShopBlocked_Messages` (4); `FarmDefinitionTests` kiểm cấp mồi → dự kiến **446**.
- Biên dịch Logic + View + Editor + Tests (dotnet): không lỗi.
- [ ] Người dùng: **Tạo data câu cá** (cấp mồi) → **Tạo UI** → Ctrl+S → Test Runner (dự kiến 446) → chơi thử: Cửa hàng ở nông trại chuyển Hạt giống / Đồ câu; mua cần (đủ / thiếu xu, chưa đủ cấp hiện "Cấp N"), Dùng, Sửa, Mua lại; mua mồi 1 / 10, Dùng; bảng Cần / Mồi ở vùng câu giống vậy; lên cấp 3 thấy "Mở Cần trúc", "Mở Mồi tép"; build lại, so Build Report.

### Bán cá, chia tab Cửa hàng / Kho (đã viết code, chờ kiểm tra – 13/11/2026)
Người dùng: tab Đồ câu chia riêng Cần câu / Mồi câu; thêm chỗ bán cá. Chốt: bán ở bảng Giỏ (vùng câu) và trong Kho (nông trại); Bán hết có xác nhận.

Đã làm:
- Logic: `Farm.SellFish(index, out xu)`, `Farm.SellAllFish(out xu)`, `Farm.GetBasketValue()`; chống kẹt: còn cá trong giỏ thì không kẹt (`IsStuck`). Save không đổi.
- View: `BasketView` (giỏ dùng chung: "Giỏ n/m" + **Bán hết (tổng)** → xác nhận; chi tiết cá + **Bán (giá)** / Thả; lưới 25 ô; 1 hộp xác nhận dùng cho mở ô và bán hết; thông báo "+X xu"); `BasketPanel` chỉ còn mở bảng; `ShopTabs` tổng quát nhiều tab (giữ tên file để không đụng `.meta`).
- Editor: `UiBuilder` – hàng tab dưới tiêu đề (`BuildTabRow`), Cửa hàng "Hạt giống | Cần câu | Mồi câu" (`BuildShopTabs`), Kho "Nông sản | Giỏ cá" (`BuildStorageTabs`, đưa Scroll + nhãn Empty vào trang Nông sản), `BuildBasketView`.
- Test: `SellFish_AddsPriceRemovesOne`, `SellAllFish_SumsAndEmpties`, `Rescue_FishInBasket_DoesNotRescue` → dự kiến **449**.
- Biên dịch Logic + View + Editor + Tests (dotnet): không lỗi.
- [ ] Người dùng: **Tạo UI** → Ctrl+S → Test Runner (dự kiến 449) → chơi thử: Cửa hàng 3 tab (mở lại về Hạt giống); Kho 2 tab, Giỏ cá bán 1 con / Bán hết (xác nhận, xu tăng đúng), Thả, mở ô; bảng Giỏ ở vùng câu giống vậy; nhãn "Kho trống" chỉ ở tab Nông sản.

### Bỏ tab Nông trại (13/11/2026)
Người dùng đề xuất bỏ; tab chỉ gọi `GameUI.CloseAll` (đóng bảng), đã có nút X / chạm nền / chuyển tab. Thanh tab còn "Đơn hàng | Kho | Cửa hàng". Sửa `GameUI` (bỏ `_farmTab`), `UiBuilder.BuildNav`. Biên dịch không lỗi. [ ] Người dùng: **Tạo UI** → Ctrl+S.

### Bóng cá: xem xét, đớp thử, căng khi kéo (đã viết code, chờ kiểm tra – 13/11/2026)
Người dùng: bóng hiện sau khi ném 0,5–1 giây, bơi quanh xem xét, lâu lâu đớp thử mồi; khi giật thì căng liên tục.

Đã làm (chỉ View, Logic / test / save không đổi):
- `FishShadow` viết lại theo bước: `Approach` (hiện mờ dần sau độ trễ, lượn qua lại theo cung phía nước xa nhân vật, đầu theo hướng bơi), `Nibble` (lao vào phao rồi lùi, khớp lần phao rỉa), `Bite` (lao vào phao và giữ, rung 1 px), `Hook` + `SetTension` (ghì ra xa, rung dọc / ngang dây liên tục, gần hết giờ mạnh gấp 2,2), `Dart` / `Flinch` như cũ, `Escape` (hụt / thoát / thu cần: quay đầu bơi đi), `Reel`. Mọi số chỉnh trong Inspector.
- `FishingController`: gọi `Approach` lúc phao chạm nước (độ trễ ngẫu nhiên `_shadowAppearSeconds` 0,5–1 giây, luôn trước lần rỉa / cắn đầu − 0,3 giây), `Nibble` ở đầu mỗi lần rỉa, `Bite` lúc cắn, `Hook` khi móc được cá, `SetTension` theo thời gian kéo, `Escape` khi lượt kết thúc không ra cá.
- Biên dịch không lỗi. [ ] Người dùng: Play (không cần Tạo UI) → thử: bóng hiện sau ném, lượn, đớp thử khớp lúc phao rỉa, cắn thì lao vào phao; giật sớm / để mất mồi → bóng bơi đi; kéo cá bóng rung liên tục, gần hết giờ mạnh hơn.

### Kéo cá tối thiểu 4 mũi tên (đã viết code, chờ kiểm tra – 14/11/2026)
Người dùng: logic câu ổn nhưng có lúc chỉ 2 mũi tên (cá Thường 3–4, ném Perfect bớt 1, chặn dưới cũ là 1). Duyệt: nâng mọi hạng +1 và chặn dưới 4.
- Logic: `FishingTuning.MinPullArrows = 4` (kiểm tra ≥ 1 trong `Validate`); `FishingSession.MakePull` bớt 1 khi Perfect rồi chặn dưới bằng `MinPullArrows`. Asset cũ thiếu trường → Unity lấy giá trị mặc định 4.
- Data (`FishingDataGenerator`): mũi tên theo hạng 4–5 / 5–6 / 6–8 / 7–9 / 8–10 (trước 3–4 / 4–5 / 5–7 / 6–8 / 7–9). Thời gian kéo không đổi.
- Test: `Strike_InWindow_NoSlip_Hooked` kiểm tra ≥ 4; thêm `Pull_PerfectCast_NeverBelowMinPullArrows` (cá rô test 3–4, ném Perfect 40 seed); `ToDef_DefaultData_FishHaveAtLeastMinPullArrows`.
- [ ] Người dùng: **Tạo data câu cá** → Ctrl+S → Test Runner (dự kiến 451) → câu thử cá Thường, ném Perfect vẫn ≥ 4 mũi tên.

### Đợt E – Kỷ lục mới, đào được giun (đã viết code, chờ kiểm tra – 14/11/2026)
Duyệt 14/11/2026; thông báo giun chọn phương án B (chữ nổi trên đầu nhân vật).
- Logic: `FishingSession.PreviousRecordGrams` / `IsNewRecord` / `IsNewSpecies`, ghi trong `Farm.CompleteFishing` lúc áp cá câu được (chỉ để hiển thị, không lưu – save không đổi).
- Bảng cá vừa câu (`CatchResultPanel`, `UiBuilder`): thêm dòng vàng có viền dưới cân nặng – "Loài mới!" (lần đầu câu loài) / "Kỷ lục mới! (cũ 1,25 kg)"; không phá kỷ lục thì ẩn. Hộp cao 330 → 356.
- Giun: `BaitFoundPopup` (mới) nghe `Farm.BaitFound`, hiện "+2 giun" trên đầu nhân vật ở nông trại bằng một `WorldPopText` riêng (`BaitFoundPopText`); đào tiếp cùng loại trong 2 giây thì cộng dồn. Thông số trong Inspector.
- `UiFormat.CatchRecord`, `UiFormat.BaitFound`.
- Sửa 19/11/2026 (người dùng duyệt): bỏ chữ nổi "+N giun" trên đầu nhân vật; giun đào được hiện bằng nhãn **"+N [icon giun]" trên tab Kho**, chồng ngay trên nhãn nông sản, hiện cùng lúc icon nông sản bay tới (cùng thời gian hiện / mờ, cộng dồn cùng loại). `HarvestFlyer` nghe `Farm.BaitFound` (gọi trước `Harvested`), gắn số mồi vào lượt bay; icon từ `BaitDefinition.Icon`. `UiBuilder`: `BuildFlyerBadge` dựng `HarvestBadge` + `BaitBadge`; bỏ `BuildBaitFoundPopup`. Xóa `BaitFoundPopup.cs` (+ .meta), `UiFormat.BaitFound`, test `BaitFound_PopText` (3 ca → tổng test giảm 3). Biên dịch View + Editor + Tests (dotnet): không lỗi. → Người dùng: **Tạo UI** → Ctrl+S → Test Runner → thu hoạch tới khi đào được giun.
- Test: `Record_FirstCatch_IsNewSpecies`, `Record_Heavier_IsNewRecordWithPrevious`, `Record_Lighter_NotNewRecord`; `UiFormatTests` thêm `CatchRecord_Lines` (5), `BaitFound_PopText` (3).
- Biên dịch ngoài Unity (Logic, View, Editor, Tests) không lỗi.
- [ ] Người dùng: **Tạo UI** → Ctrl+S → Test Runner (dự kiến 462) → câu loài mới / con nặng hơn / con nhẹ hơn; thu hoạch (tạm đặt `_wormChancePercent` = 100 trong `FishingDefinition` để thử, xong trả 10) → "+N giun" trên đầu nhân vật, thu liên tiếp cộng dồn.
- [ ] Build WebGL, so Build Report (mốc 10,8 MB đã nén).

### Mở rộng 10 cần, 10 mồi + icon (đã viết code, chờ kiểm tra – 14/11/2026)
Người dùng thêm art `Art/Fishing/rods` (10 cần) và `Art/Fishing/bait` (10 mồi), duyệt mở rộng data (bảng ở `MVP-Plan.md` Phần 10 mục 5, 6).
- Kiểm tra art: 20 icon 16×16, PPU 16, Point, không nén, tắt mipmap – ổn; nhưng meta chép theo preset là Multiple, sprite tên `Piknik basket_0` (cả icon cá, cá đã được menu import đưa về Single).
- `FishingSpriteImporter`: menu **Câu cá: import sprite cá** đưa cả icon cần / mồi về Single; `EnsureSingleIcon` dùng khi gán.
- `FishingDataGenerator`: 10 cần (thêm nứa, mây, composite, titan, rồng; cần vàng mở cấp 13 thay 12), 10 mồi (thêm ruồi, dế, nhái, nhái hơi, thìa vàng; mồi tép 5 → 6 xu, mồi giả → "Mồi giả cá" 8 → 12 xu), mồi ưa thích của cá cho mồi mới; gán icon cần / mồi; asset cũ đổi tên theo thứ tự mới bằng `MoveAsset` (giữ GUID). Id cũ giữ nguyên → save không đổi.
- Chuyển file công cụ / preview / sheet của `rods`, `bait` sang `ArtSource/rods`, `ArtSource/bait` (xóa meta).
- Test: số cần / mồi 10, bảng cấp mở mồi mới, cần / mồi có icon, `ToDef_DefaultData_EveryBaitAttractsSomeFish`. Biên dịch ngoài Unity không lỗi.
- [ ] Người dùng: **Câu cá: import sprite cá** → **Tạo data câu cá** → Ctrl+S → Test Runner (dự kiến 463) → Cửa hàng đủ 10 cần / 10 mồi, mua thử mồi mới rồi câu → build, so Build Report.
- Chưa làm (việc riêng, chưa duyệt): hiện icon cần / mồi trong các dòng danh sách.

### Nút Cần / Mồi hiện icon đồ đang dùng (đã viết code, chờ kiểm tra – 14/11/2026)
- `FishingHud`: `_rodIcon`, `_baitIcon` đặt theo cần đang dùng / mồi đang chọn mỗi lần `Refresh` (đổi cần → `Farm.Changed`, đổi mồi → `FishingController.Changed`); data chưa có icon thì giữ icon Sunnyside dựng sẵn; kích thước = sprite × `_iconPixelScale` (2).
- `UiBuilder.BuildIconButton`: luôn có ô `Icon`, trả `Image` để nối vào HUD. Biên dịch không lỗi.
- [ ] Người dùng: **Tạo UI** → Ctrl+S → ra vùng câu, đổi cần / mồi, icon trên 2 nút đổi theo.
- Đổi chỗ nút Giỏ và Mồi (người dùng yêu cầu 14/11/2026): cột nút từ trên xuống Cần, Mồi, Giỏ.

## Đợt 2 – Phần 4: Bộ cây mới (đã viết code, chờ kiểm tra – 20/11/2026)
Kế hoạch: `Docs/ke-hoach-dot-2.md` Phần 4 (thứ tự 1); bảng cây: `Docs/thiet-ke-dot-2.md` mục 1.

### Đã làm
- `DefaultDataGenerator`: bảng 25 cây thuần Việt, tên giọng Bắc – 22 cây thường bật (cấp 1–13), 3 cây sự kiện (hoa đào, hoa cúc, hướng dương) **tắt**, số tạm. `CropRow` thêm nhóm giá. Menu **Tạo data mặc định** xóa asset cây không còn trong bảng (`DeleteStaleCrops`) và gán sprite tạm của rau muống cho cây chưa có sprite nào (`FillPlaceholderArt`; cây đã có sprite giữ nguyên).
- Logic: `enum CropGroup { LeafyGreen, Fruit, RootGrain }`, `CropDef.Group` (tham số tùy chọn). View: `CropDefinition._group`.
- Save: `CurrentVersion` 8, `MinSupportedVersion` 8, `SaveLoadResult.Outdated`. Save cũ (< 8) bị bỏ → save mới + thông báo "Bản lưu cũ không còn dùng được, bắt đầu lại từ đầu" (`SaveService.OldSaveDiscarded` → `GameUI`). Nhập mã cũ: từ chối, web báo "Mã của bản cũ, không còn dùng được." (`VeQueSave.jslib`).
- `CropSpriteAssigner`: đổi tên asset theo số thứ tự mới (`03_cai_xanh`, `16_ca_chua`, `19_bi_ngo`).
- Test: `FarmDefinitionTests` (25 asset / 22 cây bật, cây sự kiện không vào game, thứ tự thời gian, cấp 1–13 đều có cây, nhóm giá, số liệu 7 cây); `SaveTests` (save cũ → save mới + cờ báo, nhập mã cũ bị từ chối); `OrderTests` version 8. **Xóa 6 test migration save cũ** (version 1–6) vì đã bỏ migration.

### Quyết định
- Trước bản thử kín (02/01) không viết migration; đổi save thì tăng cả `CurrentVersion` và `MinSupportedVersion`. Code migration cũ trong `SaveMapper` để nguyên (không còn chạy tới), dọn sau.
- Cờ `eventOnly` + mở cây theo sự kiện làm ở Phần 7–8; thời gian lớn cây sự kiện chốt sau (`note.md`).
- Biên dịch ngoài Unity (Logic, View, Editor, Tests): không lỗi. Chưa chạy Test Runner.

### Người dùng kiểm tra
- [ ] **Về Quê → Tạo data mặc định** → Ghi đè → Console báo "đã tạo 25 cây (… asset cây cũ đã xóa, … cây dùng sprite tạm)" → Ctrl+S.
- [ ] Test Runner: chạy toàn bộ, báo số test xanh / đỏ.
- [ ] Play: thấy thông báo bản lưu cũ, bắt đầu lại; cửa hàng cấp 1 chỉ có rau muống; tua lên cấp kiểm tra cây mở đúng cấp; cây không có art hiện bằng sprite rau muống.
- [ ] Build lại, so Build Report.

### Thanh chọn hạt chỉ hiện hạt có trong kho (đã viết code, chờ kiểm tra – 20/11/2026)
- `SeedPicker`: `RefreshItems` ẩn icon cây hết hạt (số hạt đã trừ hạt giữ chỗ), trả về số cây còn hạt; đang kéo gieo hàng loạt thì không bật / tắt icon (tránh mất sự kiện kéo), thả tay rồi mới ẩn. Chạm ô trống mà không còn hạt nào: không mở thanh, báo "Hết hạt giống, mua ở Cửa hàng" và mở Cửa hàng. Đang mở thanh mà hết sạch hạt thì đóng thanh. Có hạt nhưng chưa tới cấp: vẫn hiện mờ. Biên dịch không lỗi.
- [ ] Người dùng: Play – chạm ô trống chỉ thấy cây có hạt; gieo hết một loại thì loại đó biến mất; hết sạch hạt thì mở Cửa hàng; kéo gieo hàng loạt tới khi hết hạt vẫn kéo bình thường.

### Thanh chọn hạt cuộn ngang (đã viết code, chờ kiểm tra – 20/11/2026)
- `UiBuilder.BuildSeedPicker`: `Box` (nền, `CanvasGroup`, `ScrollRect` ngang, Clamped, có quán tính) → `Viewport` (`RectMask2D`, lề 4) → `Content` (layout ngang + `ContentSizeFitter`, neo trái) chứa icon.
- `SeedPicker`: `ResizeBox` đặt bề ngang = min(vừa số icon, `_maxVisibleItems` 5,5 icon, bề ngang màn hình − lề 16); mở thanh thì cuộn về đầu.
- Bị giới hạn bề ngang thì cắt cho icon cuối lộ một nửa. Người dùng xác nhận ổn 20/11.
- Sửa 20/11 (người dùng báo kéo ngang không cuộn, trong Editor, > 5 loại hạt): nghi `Content` không được nới rộng nên `ScrollRect` không có gì để cuộn → bỏ `ContentSizeFitter`, `ResizeBox` đặt thẳng bề ngang `Content` (`SetSizeWithCurrentAnchors`). Cần chạy lại **Tạo UI**.
- `SeedPickerItem`: lúc bắt đầu kéo, ngang nhiều hơn dọc → chuyển cả lượt kéo cho `ScrollRect` (cuộn); còn lại → gieo hàng loạt như cũ. Chạm vẫn gieo ô đang chọn. Biên dịch không lỗi.
- [ ] Người dùng: **Tạo UI** → Ctrl+S → Play với nhiều loại hạt: thanh không tràn màn hình, thấy nửa icon cuối; kéo ngang thì cuộn; kéo xuống ruộng thì gieo hàng loạt; chạm icon thì gieo; thử trên điện thoại.

### Cửa hàng, Kho xếp cây theo cấp mở (đã viết code, chờ kiểm tra – 20/11/2026)
- `CropListPanel.CropsByUnlockLevel`: cây đang bật xếp theo cấp mở tăng dần, cùng cấp giữ thứ tự data (sắp xếp chèn, ổn định); dùng chung cho `ShopPanel` và `StoragePanel`. Biên dịch không lỗi.
- [ ] Người dùng: Play – Cửa hàng và Kho: rau muống → hành lá, cải xanh → mồng tơi, đậu đũa → rau đay, lúa → …

### Gán art 25 cây + giai đoạn cuối chỉ khi chín (đã viết code, chờ kiểm tra – 20/11/2026)
- Người dùng thêm `Assets/_Game/Art/Crops`: 25 cây × 6 file (`seed`, `stage1`–`stage3`, `icon`, `packet`) + `crops_data.json`. Kiểm tra: import đúng (Sprite, PPU 16, Point, None, tắt mipmap; pivot Bottom cho seed/stage, Center cho icon/packet); `stage3` của ngô nếp, hướng dương, hoa đào cao 16×32; json khớp data 22 cây thường.
- `DefaultDataGenerator.AssignCropArt`: lấy art theo tên `crop_{id}_{part}.png` – ruộng `seed → stage1 → stage2 → stage3` (4 giai đoạn), hạt UI `packet`, nông sản `icon`; ghi đè sprite cũ; thiếu file thì giữ sprite cũ và báo. Bỏ sprite tạm mượn rau muống. Xóa `CropSpriteAssigner` (menu cắt sheet Sprout Lands, không dùng nữa).
- Logic `Farm.GetStage` (người dùng chọn B): giai đoạn cuối chỉ khi chín; các giai đoạn trước chia đều thời gian lớn. `FarmTests` sửa 2 test giai đoạn, thêm `GetStage_JustBeforeRipe_NotLastStage`. Biên dịch không lỗi. Test Runner 20/11: 467/468, đỏ `Water_MidGrowth_AdvancesStage` (viết theo cách chia 3 giai đoạn cũ) → sửa mốc 20 → 40 giây; chờ chạy lại.
- `crops_data.json` đã chuyển sang `ArtSource/Crops/` (người dùng duyệt 20/11, xóa `.meta`). 3 cây cao có thể bị dấu chín / thanh tiến độ đè – xem khi Play.
- [ ] Người dùng: **Tạo data mặc định** → Ghi đè → Console "… 0 cây thiếu art" → Ctrl+S → Test Runner → Play: gieo thấy hạt trên đất, lớn qua 2 giai đoạn, chỉ khi chín mới ra quả; icon gói hạt ở thanh chọn hạt / Cửa hàng; icon nông sản ở Kho / đơn hàng / khi thu hoạch; cây cao không bị đè → build, so Build Report (thêm 150 ảnh nhỏ).

### Dọn sau khi load tiến độ (20/11/2026, người dùng duyệt)
- Cây sự kiện để tạm **4 tiếng**: hoa cúc, hướng dương 120 → 240 phút (hoa đào đã 240); vẫn tắt.
- Cân lại các cặp cây trùng thời gian / lãi: để sau.
- Dọn migration save cũ trong `SaveMapper` (bỏ quy đổi `basketLevel` của save ≤ 6), làm cùng Phần 1.
- Con số câu cá chỉnh sau khi thử máy thật (giữ nguyên).
- Tên khách đơn hàng cũ (Bà Tư, Chú Ba…): đã thay ngay (người dùng chọn 20/11) bằng 5 người làng: Bà Lụa, Chú Thành, Cô Hạnh, Cu Tí, Bác Cả (`DefaultDataGenerator.CustomerNames`; tên trong `OrderTests`, `ProgressionTests` đổi theo). Thoại / món theo người: Phần 11.

### Thêm 7 loài cá đã có art (đã viết code, chờ kiểm tra – 20/11/2026)
- `FishingDataGenerator`: thêm cá rô phi, cá chạch (Thường), cá trôi, cá thát lát (Khá), cá lăng (Hiếm), cá anh vũ (Sử thi), cá hô (Huyền thoại) – bảng số người dùng duyệt 20/11; asset `14_…` → `20_…`, 13 asset cũ giữ tên. Tổng 20 loài (5 / 5 / 4 / 3 / 3).
- **Tạo data câu cá** giờ tự gán icon cá theo file `fish_NN_<tên>.png` (dùng `FishingSpriteImporter.IdFromFile`, tôm càng xanh → `tom_cang`), như cần / mồi; không cần chạy riêng menu import sprite cá.
- `FarmDefinitionTests`: số loài cá 13 → 20.
- Art câu cá HD 32×32 (cá, cần, mồi) đã import đúng (Single, PPU 32); art gói hạt / hạt cây mới gán bằng **Tạo data mặc định**.

## Đợt 2 – Phần 1: Sửa cần (đã viết code, chờ kiểm tra – 20/11/2026)
### Đã làm
- Logic: `FishingTuning` bỏ `RepairMaxLossPercent`, `RepairMinPercent`, `MaxDurabilityAfterRepairs`, `CanRepairAgain`; thêm `RepairPricePercent = 30`, `LowDurabilityPercent = 10`, `RepairPrice(giá cần, bền, bền tối đa)` (tròn lên, tối thiểu 1, đầy = 0), `IsLowDurability`. `RodDef` bỏ `RepairPrice`. `RodState` bỏ `RepairCount`, `Snapped` (hỏng = hết độ bền). `FarmResult.RodWornOut` bỏ.
- `Farm`: `RepairRod` sửa được bất cứ lúc nào chưa đầy, về đầy, độ bền tối đa giữ nguyên; `GetRodRepairPrice`, `IsRodLow`; `GetRodMaxDurability` = độ bền gốc; `BuyRod` cần đã có (kể cả hỏng) → `AlreadyOwned` (bỏ "Mua lại").
- Save `version` 9, `MinSupportedVersion` 9: bỏ `repairCount`, `snapped`, `basketLevel`; không migration.
- View: `RodListView` – nút Dùng / Đang dùng + nút "Sửa {giá}" khi chưa đầy, chữ "· Sắp hỏng" khi ≤ 10%; `FishingHud` – số độ bền đỏ khi ≤ 10%, toast "Cần sắp hỏng, nên sửa" một lần khi lần ném làm cần tụt xuống ≤ 10%. `RodDefinition`, `FishingDataGenerator` bỏ `_repairPrice`. Thông báo `RodNotDamaged`: "Cần còn đầy độ bền".
- Test: thay test sửa cần cũ bằng `Repair_RestoresFull_PriceByMissingDurability`, `Repair_FreeRod_CostsAtLeastOneCoin`, `RepairPrice_RoundsUp_MinOne` (5 ca), `Repair_ManyTimes_MaxDurabilityNeverDrops`, `Repair_NotEnoughCoins_NoChange`, `IsRodLow_AtOrBelowTenPercent`, `BuyRod_OwnedEvenBroken_AlreadyOwned`; sửa test lưu cần, test kẹp độ bền khi nạp, test version 9. Biên dịch Logic / View / Editor / Tests ngoài Unity: không lỗi.
### Quyết định
- Cần tre giá 0 nên sửa luôn 1 xu (tối thiểu).
- Bỏ nút "Mua lại" vì sửa luôn rẻ hơn mua.
### Người dùng kiểm tra
- [ ] **Tạo data câu cá** (Console không báo thiếu icon) → Ctrl+S → Test Runner Run All → Play: câu tới khi cần ≤ 10% thấy toast + số đỏ + "Sắp hỏng"; sửa nhiều lần độ bền tối đa không giảm; giá sửa giảm khi sửa sớm. Save cũ báo không dùng được (bình thường).

## Đợt 2 – Phần 6: Kéo cá (đã viết code, chờ kiểm tra – 20/11/2026)
### Đã làm
- Logic: `FishingTuning.PullSecondsPerArrow = 0,45`; thời gian kéo = (giây kéo của cá + số mũi tên × 0,45) × (1 + thưởng cần). Ví dụ cá 3 giây, 10 mũi: 7,5 giây. Test: sửa 2 test thời gian, thêm `Pull_TimeGrowsWithArrowCount`.
- `FishingPullPanel` (viết lại): mọi nguồn nhập (nút chạm xuống, vuốt, phím mũi tên) qua `Submit` – bỏ lần bấm cách lần trước < 60 ms. Bấm sai: ô sai lắc + đỏ 0,3 giây, các ô đã đúng sáng lại lần lượt từ phải sang trái (thay cho nháy đỏ cả chuỗi). Bấm đúng trong lúc hiệu ứng thì dừng hiệu ứng.
- `FishingController`: `LastMistakeIndex`; bấm sai → phao giật về phía nhân vật 0,3 giây (game không vẽ dây câu), rung 30 ms (Android); bóng cá lao theo hướng **mũi tên kế tiếp**, lúc móc được cá lao theo mũi đầu.
- `PullSound` (mới): nốt đô–rê–mi… cao dần mỗi mũi đúng, tiếng trầm khi sai; sinh bằng code (`AudioClip.Create`), không thêm file âm thanh.
- `SwipePad` (mới): vuốt ở giữa màn hình (38–80% chiều cao, tránh mép dưới), 1 lần vuốt mỗi chạm, báo ngay khi đi đủ 24 đơn vị; chỉ nhận ngón đầu. Chạy song song với chữ thập để thử A/B. **Tạm tắt (20/11, người dùng yêu cầu):** comment phần tạo `SwipeArea` trong `UiBuilder.BuildPullPanel`; code `SwipePad` giữ nguyên, bỏ comment là bật lại.
- `PullHandSetting` (mới): tay trái / phải lưu PlayerPrefs (không vào save). Nút "Tay phải / Tay trái" góc dưới trái HUD câu cá, chỉ hiện khi chưa câu.
- `UiBuilder`: chữ thập trong nút gốc `Pad` ở góc dưới phải (tâm cách mép phải 118, cao 160; nút 64 ≥ 48 pt), không chồng nút Thu cần; thanh thời gian / chuỗi mũi tên / "Sẵn sàng" đẩy lên y 290 / 314 / 358; thêm `SwipeArea`, `PullSound` + `AudioSource`, nút Tay.
- Biên dịch Logic / View / Editor / Tests ngoài Unity (bản csproj tạm có 3 file mới): không lỗi.
### Quyết định (người dùng duyệt 20/11)
- Không chặn cứng ngón thứ hai (ngón kế có thể chạm trước khi nhấc ngón trước), chỉ chặn bấm cách nhau < 60 ms.
- Thời gian kéo tính bằng cách **cộng** 0,45 giây mỗi mũi (mọi cá dễ hơn, kể cả mốc Perfect).
- Làm luôn vuốt thay nút (thiết kế mục 5.9), chạy cùng nút để so.
### Người dùng kiểm tra
- [ ] Unity mở lại sẽ tạo `.meta` cho `PullSound.cs`, `SwipePad.cs`, `PullHandSetting.cs`. **Tạo UI** → Ctrl+S → Test Runner → Play ở vùng câu: kéo cá bằng nút góc phải, bằng vuốt giữa màn hình, bấm sai (ô lắc đỏ, ô đúng sáng lại, phao giật, tiếng trầm), nốt cao dần, bóng cá lao theo mũi kế; nút Tay trái đổi chữ thập sang trái.
- [ ] Build: thử iPhone Safari + trình duyệt nhúng Facebook / TikTok – không trễ, không nhận đôi; chuỗi 10 mũi reset ở mũi 9 vẫn kịp làm lại; so nút với vuốt, chọn giữ cái nào.

## Đợt 2 – Phần 18: Cá shiny – Bước 1 Logic + save + test (XONG – 20/11/2026, Test Runner không lỗi)
### Quyết định (người dùng duyệt kiến trúc 20/11)
- "Bán hết" **bỏ qua cá shiny** (bán lẻ từng con vẫn được).
- Random của shiny **riêng**, sinh từ seed phiên qua băm phi tuyến (murmur3 fmix32) → bật / tắt shiny không xê dịch cá, lịch phao, đề kéo, cân nặng.
- Đơn cá không lấy cá shiny: làm ở Phần 11 (dùng `CaughtFish.IsShiny`).
### Đã làm
- `ShinyKind` (trong `FishingDef.cs`, số cố định: None 0, Gold 1, Aurora 2, Ice 3, Nebula 4, Ruby 5, Onyx 6, Silver 7, DarkAurora 8).
- `FishingTuning`: `ShinyChancePer10000 = 100`, `ShinyRareTierPercent = 3,2`, `ShinyCommonKinds` (6 kiểu), `ShinyRareKinds` (Aurora, DarkAurora), hệ số giá ×3 / ×10; `IsRareShiny`, `ShinyPriceMultiplier`; kiểm tra hợp lệ (mỗi bậc ≥ 1 kiểu, không None, không trùng).
- `FishingSession.Shiny` tung trong hàm tạo ngay sau khi chọn cá (`RollShiny` public static để test); `Price` đã nhân hệ số; `IsNewShiny`. XP không đổi.
- `FishingMath.FishPrice(..., shiny)`; `CaughtFish.Shiny` / `IsShiny`; `FishingState.ShinyDex` (`ShinyRecord {FishId, Kind, FirstCaughtAt}`) + `HasShiny`; `Farm.CompleteFishing` ghi dex không trùng.
- `Farm.GetSellAllCount`, `GetBasketValue` / `SellAllFish` bỏ qua cá shiny; chỉ còn cá shiny → `UnknownFish`. `BasketView`: nút Bán hết theo số cá thường, hộp xác nhận ghi "Giữ lại N con cá đột biến".
- Save `version` 9 → **10** (`SaveFish.shiny`, `SaveFishing.shinyDex`), không migration; kiểu lạ đọc ra `None`, mục dex hỏng / trùng bỏ qua.
- Test (`FishingTests`): 100.000 lần tung ≈ 1% (±0,15); bậc hiếm ≈ 3,2% (±0,3), mỗi kiểu hiếm ≈ 50%, mỗi kiểu thường ≈ 1/6; giá ×3 / ×10; shiny không xê dịch random cũ; dex không trùng; không câu được thì không ghi; Bán hết bỏ qua shiny; save giữ shiny + dex; kiểu lạ → None; tuning sai báo lỗi. Helper test tắt shiny mặc định. `OrderTests`/`SaveTests` đổi sang version 10.
- Biên dịch Logic / View / Tests ngoài Unity: không lỗi. Mô phỏng Python cùng thuật toán và seed: tỉ lệ 0,955%, bậc hiếm 3,22%, các kiểu nằm trong ngưỡng.
### Người dùng kiểm tra
- [x] Test Runner **Run All**: không lỗi (20/11).
### Bước 2 – Shader + 8 material + ShinyPalette (XONG – 20/11/2026, người dùng kiểm tra ổn; đổi tên Hắc cực quang → Tinh dạ, Hắc ngọc → Hư không)
- `Art/Shaders/ShinyIcon.shader` (`VeQue/ShinyIcon`): dựa `UI/Default` (stencil, `_ClipRect`), thêm `_Flip` / `_RendererColor` cho SpriteRenderer; thuật toán khớp `shiny_8kieu.py` (16 khung × 0,095 giây, tính trong gamma, trắng mắt = alpha 254); không keyword riêng.
- `ShinyClock` (tự tạo lúc vào game, `_ShinyTime` từ `Time.unscaledTime`); `ShinyIconMesh` (ghi khung sprite vào `uv1`, bật TexCoord1 của Canvas); `ShinyLook` (gán / gỡ material cho Image, SpriteRenderer qua `MaterialPropertyBlock`); `ShinyPalette` (SO 8 dòng).
- `FishingDefinition._shinyPalette` + `GetShinyMaterial`. Gán material sớm (duyệt): ô giỏ cá, icon chi tiết giỏ, bảng cá vừa câu, cá bay.
- Menu **Về Quê → Cá shiny: tạo material** (8 material ở `Art/Shiny/`, `Data/Fishing/ShinyPalette.asset`, gán vào FishingDefinition; Tạo data câu cá cũng tự gán). Menu **Cá shiny: xem thử (khi Play)**: bảng 20 loài × (gốc + 8 kiểu) trong khung cuộn, nút "Ẩn bảng" xem 9 con SpriteRenderer (con lẻ lật).
- Biên dịch View / Editor ngoài Unity: không lỗi. Shader chỉ kiểm được trong Unity.
#### Người dùng kiểm tra
- [ ] Mở Unity (tạo `.meta`), Console không lỗi shader → menu **Cá shiny: tạo material** → Ctrl+S.
- [ ] Play → menu **Cá shiny: xem thử**: 8 kiểu đúng màu so `shiny_8kieu_full.gif`, dải chạy theo lưới pixel, cuộn không tràn ngoài khung; "Ẩn bảng" → cá world lật vẫn đúng.
- [ ] Câu thật: tạm đặt `ShinyChancePer10000 = 10000` trong FishingDefinition → cá bay, bảng kết quả, ô giỏ, chi tiết giỏ đều hiện shiny; đặt lại 100.
- Trắng mắt chỉ đúng sau Bước 3.

### Bước 3 – Đánh dấu trắng mắt alpha 254 (XONG – 20/11/2026, người dùng kiểm tra ổn)
- Đổi cách so với thiết kế: PNG trong project khác bản `Downloads\VeQue-art\fish_hd\` và máy không có PIL → không xuất lại từ Python mà sửa thẳng PNG trong Unity.
- `Editor/ShinyEyeMarker.cs`, menu **Về Quê → Cá shiny: đánh dấu trắng mắt**: con ngươi = khối `#181425` ≥ 2×2 không ở mép; pixel độ sáng > 0,8 cách con ngươi ≤ 2 ô → alpha 254 (giống `shiny_8kieu.py`). Ghi đè PNG, giữ `.meta`; chạy lại được (pixel 254 không còn là mắt thì trả về 255). Báo số pixel từng loài ở Console, cảnh báo loài không tìm thấy mắt.
- Bản sao 20 PNG trước khi đánh dấu: scratchpad phiên 20/11 (`fish_png_backup`).
#### Người dùng kiểm tra
- [ ] Chạy menu: đủ 20 file, không loài nào "không tìm thấy mắt".
- [ ] **Cá shiny: xem thử**: vòng mắt trắng ở cả 8 kiểu; hàng Gốc không đổi.

### Bước 4 – Đốm lấp lánh + chữ "Đột biến" (XONG – 20/11/2026, người dùng kiểm tra ổn)
- `ShinySparkleSprites`: sprite dấu cộng 7×7 sinh bằng code (cánh 1/2/3, tâm trắng, đầu cánh màu đốm), cache theo màu.
- `ShinySparkles`: 3 đốm/con theo seed (x tràn 2 px mỗi bên), lệch pha 5 khung, sống 6/16 khung × 0,095 giây (`Time.unscaledTime`); UI = 3 `Image` con (bị RectMask2D cắt), world = 3 `SpriteRenderer` con (sortingOrder +1); chỉ đổi sprite khi sang khung mới.
- `ShinyLook.Apply(..., ShinyPalette.Entry, seed)` gán material + đốm; `FishingDefinition.GetShiny`. Seed = cân nặng (giỏ và bảng kết quả cùng vị trí đốm).
- Bảng cá vừa câu: dòng `Shiny` "Đột biến: <kiểu>!" (+ " Lần đầu!") giữa hình và độ hiếm, màu đốm, có viền (`UiBuilder`). Chi tiết giỏ: dòng "Đột biến: <kiểu>" đầu phần thông tin.
- `UiFormat.ShinyCatch` / `ShinyDetail` + test. Biên dịch View / Editor / Tests ngoài Unity: không lỗi.
#### Người dùng kiểm tra
- [ ] **Tạo UI** → Ctrl+S → Test Runner.
- [ ] **Cá shiny: xem thử**: đốm đúng màu, tràn nhẹ ngoài icon, bị cắt ở mép khung khi cuộn; "Ẩn bảng" → cá world có đốm.
- [ ] Câu với `ShinyChancePer10000 = 10000`: cá bay có đốm; bảng kết quả "Đột biến: …!" + "Lần đầu!" ở con đầu; ô giỏ + chi tiết có đốm và dòng đột biến; tạm dừng vẫn nháy; chữ có dấu đủ glyph. Đặt lại 100.

### Còn lại Phần 18
- Bước 5 build + thử máy thật: **bỏ qua theo yêu cầu (20/11)**, gộp vào Phần 14 (build, thử máy thật). Phần 18 coi như xong phần code.

## Đợt 2 – Phần 11: Đơn hàng có thoại + đơn cá (đang làm – 20/11/2026)
### Quyết định (người dùng duyệt kiến trúc 20/11)
- Data sửa được: `Data/Orders/npcs.json`, `order_lines.csv` (npcId, kind Request/Urgent/Fish/Thanks, text, cropTag), `dishes.json`.
- Đơn cá: tỉ lệ 10% ở cấp mở câu cá, +1%/cấp, tối đa 20%; 1–3 con theo cấp (+1 con mỗi 3 cấp); chỉ cá Thường / Khá, câu được ≥ 3 khung giờ; không đơn cá khẩn; không lấy cá shiny; giao lấy con nhẹ nhất; thưởng = số con × giá / XP gốc × hệ số đơn.
- Cô đồng nát chưa đặt đơn (để Phần 5). Agent viết nháp thoại giọng Bắc, người dùng sửa.
### Thứ tự 1 – Logic + save + test (XONG – 20/11/2026, Test Runner không lỗi)
- `OrderDialogue.cs` (mới): `NpcDef` (title, cây hay đặt, trọng số, trọng số đơn cá), `OrderLineDef`, `OrderLineKind`, `OrderDialogue` (chọn câu không lặp, món theo seed, `Format` điền {title} {qty} {crop} {dish}, đọc CSV có ngoặc kép / ghi chú / BOM).
- `OrderDef`: `CustomerNames` → `Dialogue`; thông số đơn cá (`FishOrderMin/MaxPercent`, `MaxRarity`, `MinSlots`, `MaxCount`, `LevelsPerFish`). `OrderState`: `Kind` (Crops / Fish), `NpcId`, `Line`, `Seed`; `OrderItem.CropId` → `Id`. `OrderDelivery` + event `Farm.OrderDelivered` (câu cảm ơn).
- `Farm`: đơn cá (pool, tung tỉ lệ chỉ khi đủ điều kiện – không thì không tốn số ngẫu nhiên), người đặt theo trọng số (hay đặt cây trong đơn ×3; đơn cá theo trọng số cá), câu theo loại, không lặp câu vừa dùng của người đó; seed đơn thay cho lần chọn tên khách cũ (chuỗi đơn giữ nguyên số lần tung). `GetOrderItemHave`, giao đơn cá lấy con nhẹ nhất không shiny. "Bán hết" giữ cá shiny + cá đang cần cho đơn (`IsKeptOnSellAll`).
- Save `version` 10 → **11**: `SaveOrder` bỏ `customer`, thêm `kind`, `npc`, `line`, `seed`; `orderLines`, `lastThanks`.
- View tạm: `FarmDefinition` dựng người làng từ tên cũ (thứ tự 2 thay bằng file data); `OrderPanel` hiện tên người + icon cá cho đơn cá.
- Test mới `OrderDialogueTests` (21 test): CSV, điền chỗ trống, người + câu, không lặp liền, câu có món, đơn khẩn dùng câu khẩn, ưu tiên cây hay đặt, nạp lại giữ câu, cảm ơn không lặp; đơn cá: pool, loài / số con / Cu Tí, không khẩn, chưa mở câu cá thì không có và không đổi chuỗi đơn, tỉ lệ theo cấp, thưởng, giao (shiny không tính, lấy con nhẹ), Bán hết giữ cá cho đơn, nạp lại. Sửa test cũ theo tên mới, version 11.
- Biên dịch Logic / View / Editor / Tests ngoài Unity: không lỗi.
#### Người dùng kiểm tra
- [x] Test Runner **Run All**: không lỗi (20/11).
### Thứ tự 2 – Data + đọc file + nháp thoại (XONG – 20/11/2026, Test Runner không lỗi)
- `Data/Orders/npcs.json` (5 người: Bà Lụa "bà", Chú Thành "chú", Cô Hạnh "cô", Cu Tí "em" – gọi người chơi "anh chị", trọng số cá 6; Bác Cả "bác"), `order_lines.csv` (48 câu nháp giọng Bắc: mỗi người 4 câu thường + 1 câu theo cây, câu khẩn (Bác Cả 3), câu cá (Cu Tí 4, Bà Lụa 1, Cô Hạnh 1), câu cảm ơn riêng + 4 câu chung), `dishes.json` (22 cây).
- `Format` viết hoa đầu câu và sau . ! ?; title viết thường. Câu có {dish} không kèm {crop} (thẻ đơn đã ghi món + số lượng) để câu ≤ 70 ký tự.
- `OrderDialogueLoader` (View, JsonUtility + CSV của Logic); `FarmDefinition`: bỏ `_customerNames`, thêm 3 TextAsset + thông số đơn cá; `DefaultDataGenerator` gán file + đơn cá 10–20%.
- Test data thật (`FarmDefinitionTests`): ≥ 5 người, mỗi người ≥ 4 câu đặt đơn, ≥ 3 câu cảm ơn chung; cây hay đặt / cropTag tồn tại; câu dài nhất sau khi điền (số lượng 2 chữ số, tên dài nhất) ≤ 72 ký tự. Test `Format` viết hoa.
- Biên dịch Logic / View / Editor / Tests ngoài Unity: không lỗi.
#### Người dùng kiểm tra
- [x] Tạo data mặc định + Test Runner: không lỗi (20/11).
- [ ] Đọc / sửa thoại trong `Data/Orders/order_lines.csv` (sửa xong chạy lại test để kiểm độ dài).
### Thứ tự 3 – UI thẻ đơn + bong bóng cảm ơn (đã viết code, chờ kiểm tra – 20/11/2026)
- `OrderCard`: chân dung 32×32 (art 16×16 ×2) trái tên; chưa có art → khung tròn pixel 16×16 sinh bằng code, màu theo id người + chữ cái đầu tên gọi ("Bà Lụa" → L); câu thoại 2 dòng (cỡ 13, cắt "…" nếu dài); bong bóng cảm ơn (con của thẻ, đè đầu thẻ, 2 giây thời gian thật, tắt khi đóng bảng).
- `OrderPanel`: điền câu (món đầu: số lượng, tên viết thường, món theo seed); nghe `Farm.OrderDelivered` → bong bóng "Tên: câu cảm ơn".
- `FarmDefinition._npcPortraits` (id → sprite, đang trống). `UiBuilder`: thẻ cao 116 → 156, món dời xuống dưới câu thoại.
- Biên dịch Logic / View / Editor / Tests ngoài Unity: không lỗi.
#### Người dùng kiểm tra
- [ ] **Tạo UI** → Ctrl+S → Play (cấp ≥ 2, dùng Tua thời gian nếu cần): thẻ có khung tròn + chữ cái, tên, câu thoại có dấu đủ glyph, không tràn 2 dòng; đơn khẩn câu khẩn; giao xong bong bóng cảm ơn 2 giây; đơn cá (cấp đủ mở câu cá) hiện icon cá + số cá thường trong giỏ; mở lại game câu không đổi.
- [ ] Màn 375 pt (Game view 750×1334 hoặc máy thật): câu dài nhất vẫn 2 dòng.
### Còn lại
- Chân dung 16×16 cho 5 người (người dùng vẽ, gán vào FarmDefinition → Npc Portraits).

## Đợt 2 – Phần 2: GameCalendar + âm lịch (đã viết code, chờ kiểm tra – 20/11/2026)
### Quyết định (người dùng duyệt kiến trúc 20/11)
- **Phương án A:** mọi thứ theo giờ VN UTC+7 cố định – lịch, khung giờ câu cá, mốc 0 giờ giới hạn đơn/ngày. Bỏ `Farm.SetUtcOffset`, `FarmState.UtcOffsetSeconds`.
- Bảng âm lịch sinh bằng `Tools/lunar_table.py` (thuật toán Hồ Ngọc Đức, múi giờ +7), không cần port sang C#.
### Đã làm
- `Tools/lunar_table.py` → `Data/Calendar/lunar_2026_2040.json` (186 tháng, ~7,5 KB; mỗi tháng `s` DayKey bắt đầu, `d` số ngày, `m`, `l` nhuận, `y`). Đối chiếu: Tết 17/02/2026, 06/02/2027, 26/01/2028; nhuận tháng 5/2028, 3/2031, 11/2033, 6/2036, 5/2039.
- Logic: `LunarTable.cs` (`LunarDate`, `LunarMonth`, `LunarTable` tra nhị phân, kiểm tra liền mạch); `GameCalendar.cs` (`DayKey` từ 01/01/2026 giờ VN, không lùi – lưu `FarmState.LastSeenDayKey`; `AbsoluteDay`, `SecondsUntilNextDay`, `TimeWindow`, `TryGetLunar`, `DaySeed(salt)` FNV-1a + murmur). `FarmDef.Lunar`, `Farm.Calendar`; `Farm.CurrentDay` / `SecondsUntilNextDay` / khung giờ khi ném câu dùng `Calendar`. `SeededRandom.Mix` dùng chung.
- Save `version` 11 → **12**: bỏ `utcOffsetSeconds`, thêm `lastSeenDayKey`.
- View: `LunarTableLoader`, `FarmDefinition._lunarTable` (Tạo data mặc định gán); `GameClock` giờ VN + dòng âm lịch ("Mùng 1 tháng 11", "Rằm tháng Chạp", "… nhuận"), chỉ dựng lại khi sang ngày; `UiBuilder` thêm nhãn `Lunar` dưới đồng hồ; `GameBootstrap` bỏ đọc múi giờ máy (`WebBridge.GetUtcOffsetSeconds` còn nhưng không dùng).
- Test mới `CalendarTests` (13): Tết 2026 / 2027, tháng nhuận 2028 / 2033, phạm vi bảng, bảng không liền báo lỗi, `UiFormat.LunarDay`, DayKey đổi ở 0 giờ VN (17:00 UTC), giây tới nửa đêm, khung giờ theo giờ VN, không lùi ngày + giữ qua mở lại, `DaySeed` ổn định (so với công thức viết lại độc lập), `Farm.Calendar` ra mùng 1 Tết 2027. Sửa test cũ: `FishingTests` đặt giờ theo giờ VN; `OrderTests` thay test múi giờ bằng `DayBoundary_IsVietnamMidnight`; version 12.
- Biên dịch Logic / View / Editor / Tests ngoài Unity: không lỗi.
#### Người dùng kiểm tra
- [ ] Mở Unity → **Tạo data mặc định** (gán bảng âm lịch) → **Tạo UI** → Ctrl+S → Test Runner **Run All**. Save cũ báo không dùng được (version 12).
- [x] Play: đồng hồ HUD giờ VN + dòng âm lịch hôm nay (20/11/2026 = 12 tháng 10) – người dùng xác nhận; Tua thời gian tới 17/02/2026 hoặc 06/02/2027 thấy "Mùng 1 tháng Giêng".
- **21/11/2026: bỏ dòng âm lịch trên HUD** (người dùng chốt: người chơi tự xem lịch ngoài đời). Đã gỡ nhãn `Lunar` (UiBuilder) và code âm lịch trong `GameClock`. Giữ `LunarTable`, `GameCalendar.TryGetLunar` (logic sự kiện) và `UiFormat.LunarDay` (có thể dùng cho banner sự kiện Phần 8).

## Đợt 2 – Phần 13: Ngày đêm (đã viết code, chờ kiểm tra – 20/11/2026)
### Quyết định (người dùng duyệt 20/11)
- Thay cho "shader riêng gán mọi material": **một quad phủ khung nhìn**, shader `VeQue/WorldLight` hòa màu nhân (`Blend DstColor Zero`) ở sortingOrder **280** – dưới 280 bị tint (đất, cây, nhân vật, nước), trên 280 giữ nguyên (gợi ý tưới 300, "!" 300, khung chọn 290, cá bay 320, đom đóm 285), UI không ảnh hưởng. Cùng quad làm `DarknessOverlay` (độ tối + 8 vòng sáng) cho Soi đèn.
- Cài đặt Đầy đủ / Nhẹ / Tắt: bảng Sao lưu là hộp thoại HTML nên đổi sang **chạm đồng hồ** (toast báo mức).
### Đã làm
- Logic `DayNightCurve` (mốc giờ → hệ số nhân, nội suy, vòng qua nửa đêm, giờ VN, kiểm tra mốc).
- `WorldLight.shader`: hệ số = màu trời × (1 − độ tối × (1 − ánh sáng)), ấm theo màu đèn; vòng sáng theo lưới pixel PPU 16, 4 bậc; không keyword.
- View: `DayNightProfile` (SO, mốc màu chỉnh trong Inspector), `DayNightController` (trên camera; quad tự khớp khung nhìn; tắt quad ban ngày; `Darkness` cho soi đèn; chọn 8 vòng sáng gần camera; `NightAmount`), `WorldLight` (vòng sáng, chỉ ban đêm), `Fireflies` (đom đóm 1 pixel, nhấp nháy, mỗi con 1 vòng sáng nhỏ, chỉ khi tối), `DayNightSetting` (PlayerPrefs). `GameClock` chạm → đổi mức. `UiBuilder`: Button trên đồng hồ.
- Editor menu **Về Quê → Ngày đêm: tạo trong scene**: `Art/WorldLight.mat`, `Data/DayNightProfile.asset` (màu nháp: khuya `a8b3eb` – độ sáng 0,71; bình minh `fad6cc`; sáng / trưa trắng; chiều `fffaed`; hoàng hôn `ffdbb3`; chạng vạng `d1c2e6`), gắn controller vào Main Camera, vùng đom đóm ở (22, 13).
- Test `DayNightTests` (7): nội suy, qua nửa đêm, không có mốc, mốc sai báo lỗi, giờ VN, profile thật không tối quá 30% ở mọi 15 phút.
- Biên dịch Logic / View / Editor / Tests ngoài Unity: không lỗi.
### Đổi hệ thống giờ (21/11/2026, người dùng chốt sau khi đánh giá)
- **Giờ trong game** chỉ để hiển thị: 1 ngày game = `DayNightProfile.DayLengthMinutes` phút thật (mặc định 30, chỉnh trong data, có lệch pha). Logic `GameDayClock` (tính thẳng từ giờ Unix, không lưu save, mọi người cùng giờ). Màu trời / đèn / đom đóm theo giờ game; `DayNightController.GameHour`, `ForcedHour` (ép trời tối cho giao thừa 12b).
- Đồng hồ HUD: giờ game `hh:mm` (dựng lại khi đổi phút), icon **mặt trời** (6–18h game) / **mặt trăng** – icon pixel 12×12 sinh bằng code, có ô `_sunIcon` / `_moonIcon` để thay art vẽ tay. Dòng âm lịch vẫn là ngày thật.
- Mọi logic theo giờ thật (`GameCalendar`): giờ ra cá, âm lịch, sự kiện, điểm danh, đơn/ngày. Giờ ra cá **không hiển thị** cho người chơi (album không ghi khung giờ, gợi ý không nhắc giờ).
- Rằm → **"Ngày trăng tròn"**: cả ngày Rằm tăng tỉ lệ cá hiếm. Giao thừa: ép trời game tối. Mò cua / soi đèn: **pending**. Đã sửa `thiet-ke-dot-2.md` (mục 3, 7, 8, 13) và `ke-hoach-dot-2.md` (Phần 5, 8, 13, 15, 16).
- Test thêm trong `DayNightTests`: ngày 30 phút, lệch pha / độ dài khác, định dạng hh:mm, ban ngày / đêm.
- Biên dịch Logic / View / Editor / Tests ngoài Unity: không lỗi.
- Chi tiết giỏ cá: **bỏ dòng "Bắt lúc <khung giờ>"** (người dùng chốt 21/11); khung giờ lúc câu vẫn lưu trong save (`CaughtFish.Slot`).
#### Người dùng kiểm tra
- [ ] Mở Unity → **Ngày đêm: tạo trong scene** → **Tạo UI** → Ctrl+S → Test Runner.
- [ ] Đồng hồ chạy giờ game (~1,25 giây thật / phút game), icon đổi mặt trời ↔ mặt trăng lúc 6h / 18h game, trời đổi màu theo; chỉnh `Day Length Minutes` trong `DayNightProfile` thấy nhịp đổi.
- [ ] Đặt đèn dầu: tạo GameObject con ở ô cửa sổ nhà → Add Component **World Light** (bán kính ~24 px). Kéo `Fireflies` tới chỗ có cỏ / ao, chỉnh Size (gizmo vàng).
- [ ] Tua thời gian qua 7 khung giờ, chụp màn hình, chỉnh màu trong `DayNightProfile`; 22h cây chín, icon tưới vẫn rõ; chạm đồng hồ đổi Đầy đủ / Nhẹ / Tắt.
- [ ] FPS trên iPhone cũ / trình duyệt nhúng: để Phần 14.

## Sắp xếp menu Về Quê trong Editor (21/11/2026, chờ kiểm tra)
- Chia menu thành nhóm có vạch ngăn: Tua thời gian, Xóa save ở trên cùng; menu con **Dựng scene** (Tạo map, Tạo UI, Ngày đêm, Khung chọn, Vùng câu tạm), **Data** (data mặc định, data câu cá), **Import art** (tileset, UI 9-slice, nhân vật, sprite cá, art câu cá tạm, font), **Cá shiny** (tạo material, đánh dấu trắng mắt, xem thử).
- Chỉ đổi đường dẫn và `priority`, không đổi logic. Giữ lại mọi tool (người dùng chốt). Sửa luôn các thông báo / tooltip nhắc tên menu cũ.
- "Xóa save" bị mờ khi đang Play; "Cá shiny → Xem thử" chỉ bật khi đang Play.

## Nước pixel ở vùng câu (21/11/2026, chờ kiểm tra)
- Shader `VeQue/PixelWater` (`Assets/_Game/Art/Shaders/PixelWater.shader`, Built-in): giữ tile nước gốc, thêm 2 lớp noise trôi ngược chiều (`PixelWaterNoise.png` 32×32), snap theo PPU 16, thời gian chia bậc 8 FPS, 3 bậc màu cứng (tối = nhân màu gốc, sáng = phủ màu gợn). Không GrabPass, không đổi UV tile → an toàn với Sprite Atlas; WorldLight vẫn nhân đè lên.
- Menu **Về Quê → Dựng scene → Nước pixel (vùng câu)** (`PixelWaterSetup.cs`): đặt import noise (Repeat, Point, không nén, không mipmap, không sRGB), tạo `Assets/_Game/Art/PixelWater.mat`, gán cho TilemapRenderer `FishingWater`. `FishingRegionBuilder` gọi lại nên dựng lại vùng câu vẫn giữ nước pixel.
- Kiểm tra: gợn khớp lưới pixel ở mọi bậc zoom, không đường nối giữa tile; ban đêm vẫn tối theo WorldLight; build WebGL thử trên điện thoại ~10 phút (không đứng / rung); so Build Report (thêm ~3 KB).

## Việc đang chờ người dùng (13/11/2026)
1. **Tạo data câu cá** (cấp mồi, giá ô giỏ, mũi tên tối thiểu 4) → **Câu cá: tạo art tạm** (ổ khóa) nếu chưa chạy → **Tạo UI** → Ctrl+S → Test Runner (dự kiến 451, sửa được test đỏ `FarmDefinitionTests`).
2. Chơi thử các mục "chờ kiểm tra" ở Phần 10 giai đoạn 4: mua cần / mồi theo cấp, tab Cửa hàng 3 trang, Kho 2 trang + bán cá / Bán hết, thanh tab 3 nút, bóng cá xem xét / đớp thử / căng.
3. Build lại, so Build Report (mốc 10,8 MB; thêm icon cá, bóng, ổ khóa, icon UI nhỏ).
4. Cho commit khi muốn.

## Tiếp theo (khi quay lại)
0. Phần 10 giai đoạn 4 Đợt E: đã viết code 14/11/2026 – chờ kiểm tra + build, so Build Report.
1. Phần 10 giai đoạn 5 còn lại: album cá + kỷ lục, đơn hàng có cá (7 loài cá thêm đã làm 20/11). Giai đoạn 6: ao nhà.
2. Phần 6 mục 5 – chạy **Tạo UI**, chơi thử gieo chạm/kéo, kéo thu hoạch, kéo tưới; thử cảm giác kéo trên điện thoại.
3. Phần 6c mục 1 (báo cây chín trên tab), mục 3–6.
4. Phần 4 – template WebGL (`touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`) + build, so Build Report (mốc 10,8 MB) + thử trên iPhone Safari / Android (rung, thanh lực, nút mũi tên, bàn phím ảo ô tên); cùng lúc kiểm tra Phần 7 trên máy thật.
5. Phần 3b: bước 2 vẽ lại Map D (người dùng), bước 3 ô đất + 4 cây tạm, bước 5 dọn Sprout Lands + Sprite Atlas + build (bóng cá đã chép sang `_Game`, không phụ thuộc gói Sprout Lands). Kiểm tra giấy phép Sunnyside và gói Sprout Lands (bóng cá).
6. Phần 3 – tạo `ComingSoonArea` (trừ ao) sau khi vẽ lại map Sunnyside.
7. Chưa quyết: gỡ package không dùng (spriteshape, psdimporter, aseprite, 2d.animation); giảm build < 10 MB (Phần 8); ném yếu hiện chữ "Yếu" (mặc định, chưa chốt); con số câu cá (giá cá, giá cần / mồi, giá ô giỏ) chỉnh sau khi chơi thử.
8. Làm sau: Phần 6 mục 4 (hiệu ứng GUI), công cụ Cân bằng, khung hình cây.
