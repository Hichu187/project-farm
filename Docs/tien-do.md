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

## Phần 1 – Logic thuần + unit test (XONG – 29/10/2026, 45/45 test xanh)

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
- TODO(Phần 5): sự kiện `Changed` trên `Farm`.
- TODO(Phần 7): `FarmState` dùng `Dictionary` — `JsonUtility` không serialize được, cần chuyển đổi khi lưu.

## Phần 4 – Camera (TẠM ỔN trong Editor – làm trước Phần 1–3 theo yêu cầu; còn template WebGL + test máy thật)

### Đã làm
- `Assets/_Game/View/CameraControl/`: `CameraController`, `CameraTouchInput`, `FocusArea` (namespace `VeQue.View.CameraControl` — không dùng `...Camera` vì trùng tên `UnityEngine.Camera`).
- `VeQue.View.asmdef` thêm reference `Unity.2D.PixelPerfect`, `Unity.InputSystem`, `UnityEngine.UI` (cho `EventSystem`).

### Quyết định
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

## Phần 2 – Data (ScriptableObject) (XONG – 29/10/2026, 49/49 test xanh)

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

## Tiếp theo (khi quay lại)
1. Phần 3 – Map & art: chốt bố cục map (A/B/C, thử trên bản "Về Quê Chơi Thử"); kiểm tra Sprout Lands có sprite cho 4 cây MVP; cắt tileset/cây, Sprite Atlas, Tilemap Ground/Decor/Plots, `ZoneAnchor` (gắn cùng GameObject với `FocusArea`), gán sprite vào `CropDefinition`.
2. Phần 4 – còn template WebGL (`touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`) + thử trên iPhone Safari / Android.
3. Phần 5 – thêm sự kiện `Changed` trên `Farm` (TODO trong `Farm.cs`).
4. Chưa quyết: gỡ package không dùng (spriteshape, psdimporter, aseprite, 2d.animation); giảm build < 10 MB (Phần 6/8).
