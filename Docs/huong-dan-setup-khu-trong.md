# Hướng dẫn setup map & khu trồng (Phần 3 – Map D)

Bố cục: **trải ngang**, kéo ngang là chính. Người dùng tự vẽ và tự đặt vị trí cuối cùng; tọa độ ở `Docs/MVP-Plan.md` Phần 3 chỉ là bố cục tham chiếu.
Camera tự giới hạn theo `cellBounds` của Tilemap được gán → đổi kích thước map không cần sửa code camera.

## Quy tắc cần giữ khi tự đặt vị trí
- Mọi vị trí là tọa độ nguyên, đúng theo ô của Grid.
- Mỗi khu mặc định 2 luống 6×2, xếp chồng, cách nhau 1 tile → bounding box 6×5, 24 ô.
- Luống trên là luống 0, luống dưới là luống 1. Ô mở lần lượt: hết luống 0 mới sang luống 1.
- Khu trên map phải khớp thứ tự mở khóa: Khu 1 → 2 → 3.
- Không thêm `TilemapCollider2D`.

## A. Grid và các lớp (`Assets/Scenes/Gameplay Scene.unity`)
Scene đã có Grid `Map` và 3 Tilemap `Ground`, `Decor`, `Plots`. Chỉ cần kiểm tra lại:
1. `Map` → Grid: Cell Size = (1, 1, 0), Position = (0, 0, 0).
2. **Tilemap Renderer → Order in Layer**: `Ground` = 0, `Decor` = 10, `Plots` = 20 (cây dùng số cao hơn, Phần 5).
3. `CameraController` → **Map Tilemap** = `Ground`.

## B. Vẽ map
Dùng tool để vẽ cỏ, sân đất, hàng rào và ô đất; phần còn lại vẽ tay.

### Vẽ bằng tool
1. Mở **Về Quê → Tạo map**. Tool tự tìm `Ground`, `Decor`, `Plots` theo tên trong scene đang mở.
2. Nhập **Map Size** và vị trí sân, cổng. Tọa độ là cell Unity: góc dưới-trái map tại (0, 0), trục Y hướng lên.
3. Gán tile (chỉ cần làm 1 lần, tool tự lưu):
   - **Grass Tiles**: danh sách tile cỏ, **Weight** là tỉ lệ. Chọn nhiều tile hoặc cả thư mục trong Project, nhập **Weight khi thêm**, bấm **Thêm tile đang chọn**. Bấm lại với tile đã có để đổi Weight.
   - **Yard Tiles**: 4 góc và 4 cạnh. Để trống **Center** thì phần giữa dùng cỏ random từ **Grass Tiles**; gán **Center** thì phần giữa dùng đúng tile đó.
   - **Fence Tiles**: 4 góc và 4 cạnh.
   - **Plot Tile**: tile ô đất.
4. Bấm nút của từng lớp, xác nhận rồi lưu scene. Ctrl+Z để hoàn tác. Đổi **Grass Seed** để có kiểu rải cỏ khác.

| Nút | Tác dụng |
|---|---|
| **Vẽ Ground** | Xóa sạch `Ground`, vẽ lại cỏ và sân đất (bỏ tick **Paint Yard** nếu chưa cần sân) |
| **Vẽ hàng rào** | Xóa các ô đang dùng **Fence Tiles** trên `Decor`, rồi vẽ lại theo **Fence Offset** (khoảng cách tới mép map ở từng cạnh); phần vẽ tay bằng tile khác được giữ, trừ ô trùng vị trí hàng rào mới |
| **Vẽ Plots** | Xóa sạch `Plots`, vẽ lại ô đất theo `Beds` |
| **Xóa ...** | Xóa toàn bộ tile trên lớp đó, gồm cả phần vẽ tay |

**Quan trọng:**
- **Gate Start** tính từ góc dưới hoặc góc trái của hàng rào, không phải của map.
- Đổi **Fence Tiles** rồi mới vẽ lại thì hàng rào dùng tile cũ không tự xóa, phải tẩy tay hoặc vẽ lại trước khi đổi tile.
- Nếu vẽ tay trên `Decor` bằng chính tile trong **Fence Tiles**, các ô đó cũng bị xóa khi vẽ lại hàng rào.

Vị trí ô đất lấy từ `Beds` trong `Assets/_Game/Data/Zones/*.asset`, nên muốn dời luống thì sửa `Origin` rồi vẽ lại `Plots`.

### Vẽ tay phần còn lại
Mở **Window → 2D → Tile Palette**, dùng palette trong `Assets/Art/Tile Pallette/Ground Layer`.

| Lớp | Nội dung |
|---|---|
| `Ground` | Cỏ + sân đất. Phải phủ kín map vì camera lấy mép map từ lớp này |
| `Decor` | Hàng rào, cổng, hàng hoa, viền cỏ quanh luống, ao, chuồng, nhà kho, cây, rơm |
| `Plots` | Ô đất của các luống |

Kiểm tra `CameraController` → **Map Padding**: đặt 0 nếu muốn camera dừng đúng mép map. Scene đang để 5/5/5/2,5 nên camera đi ra ngoài map; chỉ nới cạnh nào cần lộ background trời (làm sau).

## C. 3 khu trồng
Làm cho từng khu:
1. Vẽ 2 luống trên lớp `Plots`, vẽ viền cỏ trên lớp `Decor`.
2. Tạo GameObject rỗng con của `Map`: `Zone1`, `Zone2`, `Zone3`.
3. **Position = góc dưới-trái bounding box của khu**, là số nguyên. Xem tọa độ cell ở góc dưới Tile Palette khi rê chuột.
4. Gắn `FocusArea`, đặt **Size In Cells** = (6, 5). Khung gizmo vàng phải bao đúng 2 luống. (Khung xanh = vùng padding khi đang zoom.)
5. `CameraTouchInput` → kéo 3 khu vào mảng **Areas**, xóa phần tử rỗng đang có.
6. `CameraController` → **Focus Ref** = (113, 200) để nhìn đủ khu rộng 6 tile.
7. Ghi lại Position góc dưới-trái của từng luống để nhập vào data ở mục E.

Giá trị tham chiếu nếu đặt đúng Map D và góc dưới-trái map tại (0, 0):

| Khu | Position khu | Origin luống 0 (trên) | Origin luống 1 (dưới) |
|---|---|---|---|
| Khu 1 | (13, 11) | (13, 14) | (13, 11) |
| Khu 2 | (21, 11) | (21, 14) | (21, 11) |
| Khu 3 | (13, 3) | (13, 6) | (13, 3) |

## D. Kiểm tra (Play)
- [ ] Kéo ngang được hết map, dừng ở mép.
- [ ] Chạm khu → zoom vào khu, thấy đủ bề ngang 6 tile; chạm bãi cỏ → về toàn cảnh.
- [ ] Khe 1 tile giữa 2 luống vẫn tính là trong khu (không về toàn cảnh).
- [ ] Game view scale 1x sắc nét, không có đường kẻ giữa tile.

Lưu ý: code camera hiện vẫn vừa di chuyển vừa zoom; "tới tâm rồi mới đổi bậc zoom" sẽ sửa sau.

## E. Làm sau khi code xong (chưa làm được)
- **Nhập origin luống:** sau khi `ZoneDefinition` có danh sách `Beds`, nhập origin đã ghi ở bước C7 vào `Assets/_Game/Data/Zones/*.asset`.
- **Gắn `ZoneAnchor`:** code chưa viết. Gắn cùng GameObject với `FocusArea`, không phải làm lại phần A–C.
