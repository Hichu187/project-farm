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
- Kích thước từng khu lấy từ `ZoneDefinition`, không viết cứng 4×5. Ô định danh bằng `(zoneId, col, row)`.
- Trạng thái ô đất: state machine (Khóa / Trống / Đang lớn); "chín" suy ra từ timestamp qua `IClock`, không đếm giờ trong `Update()`.
- Save JSON có trường `version`; chống chỉnh giờ máy ở mức cơ bản.

## Gameplay MVP (chỉ đúng phạm vi này)
- 3 khu trồng (mặc định 4×5, chỉnh được qua data). Mở từng ô bằng tiền, lần lượt; khu sau khóa toàn bộ cho tới khi khu trước mở hết.
- Gieo → lớn qua 3 giai đoạn theo thời gian thực → thu hoạch → Kho → bán. Cửa hàng bán hạt.
- 4 cây: rau muống 2 phút, cải xanh 15 phút, cà chua 1 giờ, bí đỏ 4 giờ. Data có 25 cây với cờ `enabled`; chỉ 4 cây này bật trong MVP.
- Cây vẫn lớn khi thoát game. Lưu tiến trình + xuất/nhập mã sao lưu.
- Ngoài phạm vi: nuôi cá, chuồng trại, nhân vật/joystick.

## Map & camera
- Tilemap, Grid cell 1, các lớp Ground / Decor / Plots. Không dùng `TilemapCollider2D`; xác định ô bằng `Grid.WorldToCell`.
- Hàng rào bao quanh, cổng, 3 khu trồng, ao cá, chuồng, nhà kho (ao/chuồng/kho chỉ là art).
- Camera: Pixel Perfect Camera (package 2D Pixel Perfect cho Built-in), PPU 16, chỉ zoom bậc nguyên. Toàn cảnh Ref ~208×370 (không thấy hết map, phải kéo); zoom vào khu Ref ~90×160.
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
