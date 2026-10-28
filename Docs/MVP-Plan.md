# Về Quê – Kế hoạch setup MVP trong Unity (28/10/2026)

Mỗi phần nhỏ, chạy và kiểm tra được trước khi sang phần sau. Phần nào code lớn (đánh dấu 🔍) thì đưa kiến trúc chi tiết để duyệt trước.

## Thay đổi so với instructions
- Kích thước từng khu lấy từ `ZoneDefinition` (ScriptableObject), không viết cứng 4×5. Logic định danh ô bằng `(zoneId, col, row)`.
- Camera toàn cảnh KHÔNG thấy hết nông trại: Reference Resolution khoảng 208×370 (≈13 tile bề ngang, zoom 5x trên màn 1080–1170 px). Zoom vào khu khoảng 90×160 (≈12x).
- Bố cục map (A/B/C): [CẦN CHỐT], thử trên bản "Về Quê Chơi Thử".
- Số ô mở sẵn Khu 1: [CẦN CHỐT], đặt trong `ZoneDefinition.startOpenCount`.

---

## Phần 0 – Dọn project & khung thư mục
1. Tạo cấu trúc:
   - `Assets/_Game/Logic/` → asmdef `VeQue.Logic` (bật **No Engine References**)
   - `Assets/_Game/View/` → asmdef `VeQue.View` (tham chiếu `VeQue.Logic`)
   - `Assets/_Game/Data/` (ScriptableObject assets)
   - `Assets/_Game/Art/` (tileset, cây, UI đã cắt gọn)
   - `Assets/_Game/Tests/EditMode/` → asmdef `VeQue.Tests` (Editor only, tham chiếu `VeQue.Logic` + NUnit)
2. Tạo Preset texture importer: PPU 16, Point, Compression None, tắt mipmap → đặt làm mặc định cho thư mục Art.
3. Cài **2D Pixel Perfect**; kiểm tra có sẵn 2D Tilemap Editor, Test Framework.

**Kiểm tra:** project mở không lỗi; Test Runner thấy assembly `VeQue.Tests`; build WebGL thử, ghi lại dung lượng làm mốc.

## Phần 1 – Logic thuần + unit test 🔍
Toàn bộ trong `VeQue.Logic`, không dùng `UnityEngine`.
1. Định nghĩa thuần: `CropDef` (id, growSeconds, buyPrice, sellPrice), `ZoneDef` (id, width, height, startOpenCount, unlockPrices).
2. Trạng thái: `FarmState` (coins, seeds, inventory, zones → plots), `PlotState` (Locked / Empty / Growing; "chín" suy ra từ timestamp).
3. `IClock` (để test giả lập thời gian, chống chỉnh giờ ở bước sau).
4. Luật: gieo, thu hoạch, mua hạt, bán nông sản, mở ô kế tiếp, khu sau mở khi khu trước hết ô.
5. Test EditMode cho từng luật, chạy với vài kích thước khu khác nhau (3×4, 4×5, 5×6).

**Kiểm tra:** toàn bộ test xanh; chưa cần scene.

## Phần 2 – Data (ScriptableObject)
Lưu ý: asmdef "No Engine References" không chứa được ScriptableObject → SO nằm ở `VeQue.View`, lúc khởi động chuyển sang `CropDef`/`ZoneDef` thuần.
1. `CropDefinition`: id, tên hiển thị, thời gian lớn, giá mua/bán, sprite hạt, 3 sprite giai đoạn, icon nông sản.
2. `ZoneDefinition`: id, width, height, startOpenCount, giá mở từng ô.
3. `FarmDefinition`: danh sách khu theo thứ tự mở khóa + danh sách cây.
4. Tạo 4 cây và 3 khu.

**Kiểm tra:** một script test trong Editor đọc `FarmDefinition` → tạo `FarmState` đúng số ô mỗi khu.

## Phần 3 – Map & art
1. Cắt tileset cỏ và bộ cây (PPU 16, Slice 16×16, sửa tay sprite cây cao 32 px, pivot Bottom). Chỉ giữ các hàng cây dùng cho MVP.
2. Sprite Atlas (padding 2, Point, không nén).
3. Grid (cell 1) với Tilemap: `Ground` → `Decor` → `Plots`; vẽ map theo phương án đã chốt, hàng rào bao quanh, cổng, ao, chuồng, kho (art tĩnh).
4. Mỗi khu có 1 GameObject `ZoneAnchor` (tham chiếu `ZoneDefinition`, origin trên Tilemap) + collider vùng chạm không cần — dùng `WorldToCell`.

**Kiểm tra:** Game view scale 1x nhìn sắc nét, không có đường kẻ giữa tile; Build Report không tăng bất thường.

## Phần 4 – Camera 🔍
1. Pixel Perfect Camera: Assets PPU 16, Ref 208×370, Pixel Snapping bật, Crop Frame None.
2. Kéo 1 ngón để di chuyển, giới hạn trong mép map; ngưỡng 10 px để tách kéo và chạm.
3. Chạm khu → camera tới tâm khu rồi đổi Ref sang ~90×160; chạm bãi cỏ → về 208×370.
4. Template WebGL tối giản: `touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`.

**Kiểm tra:** build lên iPhone (Safari) và Android qua Wi-Fi: kéo mượt, không bị trình duyệt cuộn hay tải lại trang, zoom không làm méo pixel.

## Phần 5 – Hiển thị ô đất & cây
1. `PlotView`: đổi tile ô đất theo trạng thái (khóa / ô mở được tiếp / trống / đã gieo).
2. Mỗi ô 1 `SpriteRenderer` cho cây, sắp xếp theo trục Y; dấu "chín" nhấp nháy.
3. Chạm ô trong chế độ zoom → gửi lệnh vào logic (gieo / thu hoạch / mở ô).
4. Cập nhật hiển thị theo sự kiện + mỗi giây một lần (không đếm giờ trong Update).

**Kiểm tra:** gieo rau muống, chờ 2 phút thấy đủ 3 giai đoạn và thu hoạch được.

## Phần 6 – UI (uGUI) 🔍
1. Canvas Scaler (Scale With Screen Size, ref 390×844, match 0.5) + `SafeArea` cho iPhone.
2. Font pixel tiếng Việt (VT323 hoặc font khác): TMP font asset tĩnh, đủ ký tự có dấu, Atlas Render Mode Raster, Point.
3. HUD (xu, nút sao lưu), thanh điều hướng, bảng chọn hạt, xác nhận mở ô, Kho, Cửa hàng — theo canvas "Về Quê – GUI Layouts".

**Kiểm tra:** chữ có dấu hiển thị đúng trên iPhone; nút ≥ 44 pt; không bị thanh home đè.

## Phần 7 – Lưu tiến trình 🔍
1. Save JSON có `version`; lưu ô theo `(zoneId, col, row)`.
2. Lưu khi có thay đổi + khi tab bị ẩn (`visibilitychange`), không chỉ khi thoát.
3. Cây lớn khi offline (tính theo timestamp lúc mở lại); chống chỉnh giờ cơ bản.
4. Xuất / nhập mã sao lưu.

**Kiểm tra:** đóng tab, mở lại sau vài phút thấy cây đã lớn; nhập mã trên máy khác ra đúng tiến trình; thử trong trình duyệt nhúng Facebook (dữ liệu tách riêng với Safari).

## Phần 8 – Build web & phát hành thử
1. Màn hình loading tùy chỉnh nhẹ; Brotli, tắt Decompression Fallback khi lên hosting thật.
2. Hạ Max Memory Size cho iOS; kiểm tra Build Report, mục tiêu < 10 MB đã nén.
3. Test trên Safari iOS, Chrome Android, trình duyệt nhúng Facebook/TikTok (cần tên miền riêng thay `*.workers.dev`).

**Kiểm tra:** mở link trên điện thoại là chơi được trong vài giây, chơi trọn một vòng gieo → bán → mở ô.
