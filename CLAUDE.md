# Về Quê – Luật làm việc cho agent

Kế hoạch đầy đủ: `Docs/MVP-Plan.md` (Phần 0 → 8). Tiến độ: `Docs/tien-do.md`.

## Cách làm việc
- Trả lời bằng tiếng Việt, ngắn gọn.
- Mỗi việc: đề xuất kế hoạch (file sẽ tạo/sửa, các bước) → CHỜ duyệt → mới làm.
- Làm đúng phần được giao trong `Docs/MVP-Plan.md`. Không tự mở rộng phạm vi, không làm trước phần sau.
- Một phần chỉ được báo "xong" khi đạt mục **Kiểm tra** của phần đó. Sau mỗi phần, cập nhật `Docs/tien-do.md` (đã làm gì, quyết định gì, việc còn lại).
- Không tự thêm/xóa package, không sửa `ProjectSettings/` hay file `.meta` nếu chưa được duyệt. Không động vào `Library/`, `Temp/`, `Logs/`.
- Việc cần thao tác trong Unity Editor (Preset, Sprite Editor, Tile Palette, build) → viết hướng dẫn từng bước để người dùng làm, không đoán kết quả.
- Khi thêm package/asset: nhắc build lại và so Build Report (mốc hiện tại 10,8 MB đã nén, Gzip, mục tiêu < 10 MB).

## Kiến trúc
- `VeQue.Logic` (`Assets/_Game/Logic`): C# thuần, asmdef bật **No Engine References**. Không `using UnityEngine`.
- `VeQue.View` (`Assets/_Game/View`): MonoBehaviour, ScriptableObject, tham chiếu `VeQue.Logic`. ScriptableObject KHÔNG đặt được trong Logic → lúc khởi động chuyển SO sang class thuần (`CropDef`, `ZoneDef`).
- `VeQue.Tests` (`Assets/_Game/Tests/EditMode`): test EditMode (Unity Test Framework) cho Logic.
- Khu = danh sách luống. `ZoneDefinition` (SO): `zoneId`, `beds` (mỗi luống: origin `Vector3Int` + width + height), `startOpenCount`, giá mở ô. Bounding box khu tính từ các luống; không viết cứng kích thước khu/map.
- `ZoneDef` (class thuần trong Logic): chỉ biết số luống và kích thước từng luống, không biết tọa độ tile.
- Ô định danh bằng `(zoneId, bedIndex, col, row)`; có thể quy ra `plotIndex` để tiện, nhưng save JSON lưu theo định danh ổn định đó.
- View lo chuyển `(zoneId, bedIndex, col, row)` ↔ tọa độ tile qua origin của luống, và `Grid.WorldToCell` khi chạm.
- Trạng thái ô đất: state machine (Khóa / Trống / Đang lớn); "chín" suy ra từ timestamp qua `IClock`, không đếm giờ trong `Update()`.
- Save JSON có trường `version`; chống chỉnh giờ máy ở mức cơ bản.

## Gameplay MVP (chỉ đúng phạm vi này)
- 3 khu trồng; mỗi khu = 2 luống 6×2 xếp chồng, cách nhau 1 tile → 24 ô/khu; luống có viền cỏ. Mở từng ô bằng tiền, lần lượt (hết luống này mới sang luống sau); khu sau khóa toàn bộ cho tới khi khu trước mở hết. Khu 1 mở sẵn 6 ô; Khu 2, 3 mở miễn phí 1 ô khi mở khóa.
- Gieo → lớn qua các giai đoạn theo thời gian thực (mỗi cây có số giai đoạn theo số sprite, mặc định dùng 5) → thu hoạch → Kho → bán. Cửa hàng bán hạt.
- Tưới: tùy chọn, mỗi lần trồng tưới được 1 lần, giảm 20% tổng thời gian lớn (lùi `PlantedAt`); không tưới cây vẫn lớn.
- 4 cây: rau muống 2 phút, cải xanh 15 phút, cà chua 1 giờ, bí đỏ 4 giờ. Data có 25 cây với cờ `enabled`; chỉ 4 cây này bật trong MVP.
- Giá hạt, sản lượng, giá bán, XP theo công thức cân bằng ở `Docs/MVP-Plan.md` Phần 6b; không chỉnh tay từng cây.
- Tiến trình (Phần 6b): cấp độ (XP từ thu hoạch + đơn hàng; lên cấp mở cây mới, khu sau cần đủ cấp) và đơn hàng làng quê (3 đơn cùng lúc, thưởng cao hơn bán thẳng). Chống kẹt: hết xu, hạt, nông sản, cây thì tặng 1 hạt rau muống.
- Làm sau A + B (Phần 6c): nhiệm vụ hằng ngày + điểm danh, bộ sưu tập/mốc thành tích.
- Cây vẫn lớn khi thoát game. Lưu tiến trình + xuất/nhập mã sao lưu.
- Ao, chuồng, nhà kho, đất mở rộng: chỉ là art, chạm vào hiện "sắp có".
- Ngoài phạm vi: nuôi cá/câu cá, chuồng, đất mở rộng, nhân vật/joystick, trộm nông sản, hai ví xu, thuốc trừ sâu/phân bón.

## Map & camera
- Tilemap, Grid cell 1. Ground: cỏ + sân đất. Decor: hàng hoa, viền cỏ quanh luống, cây, rơm. Plots: ô đất. Không dùng `TilemapCollider2D`; xác định ô bằng `Grid.WorldToCell`.
- Map D: 44×26 tile, trải ngang, hàng rào bao quanh. Kéo ngang là chính (~3,5 màn hình), dọc chỉ dịch nhẹ. Tọa độ chi tiết ở `Docs/MVP-Plan.md` Phần 3. Chỉ tham khảo cách sắp xếp nông trại Avatar/Teamobi, không lấy art.
- Camera: Pixel Perfect Camera (package 2D Pixel Perfect cho Built-in), PPU 16, chỉ zoom bậc nguyên. Toàn cảnh Ref ~208×370 (≈13 tile ngang, zoom 5x trên màn 1080–1170 px). Zoom vào khu: tính Ref để bounding box khu (6×5 tile) chiếm ~85% bề ngang (~10x); camera tới tâm bounding box rồi mới đổi bậc zoom. Giới hạn trong mép map 44×26.
- Kéo 1 ngón để di chuyển (giới hạn trong mép map); chạm khu → zoom vào; chạm bãi cỏ → về toàn cảnh. Không pinch zoom. Ngưỡng tách chạm/kéo ~10 px.

## Art
- Asset pack Sprout Lands. Mọi sprite: PPU 16, Filter Point, Compression None, tắt mipmap. 1 ô đất = 1 tile 16×16.
- Cây: mỗi ô 1 `SpriteRenderer`, pivot Bottom, sắp thứ tự theo trục Y. Gom vào Sprite Atlas (padding ≥ 2).

## Bẫy Unity Web trên mobile (luôn nhắc khi liên quan)
- Bộ nhớ WebAssembly trên iOS (Max Memory Size), thời gian tải, dung lượng build.
- Trình duyệt nhúng FB/TikTok: chặn `*.workers.dev`, lưu trữ tách riêng với Safari.
- Cử chỉ trình duyệt cướp chạm/kéo → template có `touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`, xử lý safe-area.
- Âm thanh chỉ phát sau lần chạm đầu (AudioClip: Decompress on Load + AAC).
- Nén Brotli/Gzip cần header đúng ở hosting.
- PlayerPrefs/IndexedDB có thể bị xóa → có mã sao lưu.
- Font tiếng Việt có dấu: TMP font asset tĩnh đủ glyph, Raster, Point.
