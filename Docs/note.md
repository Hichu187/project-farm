# Về Quê – Note tiến độ

Bản chụp lúc lưu: 07/11/2026 (sau khi lên kế hoạch Phần 6c và đưa câu cá vào MVP). Nhánh `setup/phan-0-camera`, commit cuối `e65601e`. Thay đổi tài liệu sau commit đó (`CLAUDE.md`, `MVP-Plan.md`, `tien-do.md`, `note.md`) **chưa commit**. Không commit `.vscode/` và `LiberationSans SDF - Fallback.asset` (Unity tự ghi).

## Tóm tắt

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
| 10 – Câu cá | Trong MVP (07/11/2026), đã có thiết kế, chưa làm | Ném perfect, giật câu, kéo cá mũi tên 4 hướng, cá theo độ hiếm/cân nặng/giá/khung giờ, cần rank/độ bền/trượt/gãy, mồi; cần art cá, phao, cần, nước |
| 7 – Lưu tiến trình | Đã viết code (save `version` 2) | Thử trên máy thật, nhập mã máy khác, trình duyệt nhúng Facebook |
| 8 – Build & phát hành | Chưa làm | Toàn bộ |

Git: nhánh `setup/phan-0-camera`, commit cuối `e65601e` (gồm Phần 5, 5b, 6 mục 5, 6b, 7). Tài liệu kế hoạch Phần 6c, 10 sau commit đó **chưa commit**.
Test: lần gần nhất người dùng xác nhận trong Unity là 210 (sau Đợt 4 Phần 6b). Hiện có 221 test; ngoài Unity chạy được 164/164 xanh (phần không cần Unity).

## Việc đang chờ người dùng

1. Phần 6 mục 5 (chạm và kéo): chạy **Về Quê → Tạo UI** (Ctrl+S), **Test Runner → EditMode → Run All** (mong đợi 221 xanh), chơi thử gieo bằng chạm/kéo, kéo thu hoạch, kéo tưới, kéo vẫn di chuyển camera.
2. Cho phép bắt đầu Phần 6c mục 1–2 (đang để "chưa làm").
3. Câu cá: chốt nút mũi tên hay vuốt, XP chung hay riêng, cá giữ trong giỏ hay bán ngay, nguồn art và âm thanh.

## Việc tiếp theo

0. Phần 6 mục 5 – chạy **Tạo UI**, Test Runner (221), chơi thử gieo chạm/kéo, kéo thu hoạch, kéo tưới; thử cảm giác kéo trên điện thoại.
1. Phần 6c mục 1–2 (khi người dùng cho làm): chạy lại mô phỏng cân bằng với giới hạn đơn → duyệt → code.
2. Phần 4 – template WebGL (`touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`) + build, so Build Report (mốc 10,8 MB) + thử trên iPhone Safari / Android; cùng lúc kiểm tra Phần 7 trên máy thật.
3. Phần 10 – câu cá (trong MVP): chốt các câu hỏi còn lại, chuẩn bị art, rồi làm theo 5 đợt trong `MVP-Plan.md`.
4. Phần 3 – art (setup ô đất), vẽ phần còn lại trên `Decor`, tạo `ComingSoonArea` (trừ ao), Sprite Atlas.
5. Chưa quyết: gỡ package không dùng (spriteshape, psdimporter, aseprite, 2d.animation); giảm build < 10 MB (Phần 8).
6. Làm sau: Phần 6c mục 3–8, Phần 6 mục 4 (hiệu ứng GUI), công cụ Cân bằng, art 32×32, khung hình cây.

## Chưa quyết / ghi chú

- Gỡ package không dùng (spriteshape, psdimporter, aseprite, 2d.animation).
- Tài liệu lệch thực tế: map ghi 44×26 nhưng scene 46×26; camera có `_mapPadding` 5/5/5/2,5 (đi ra ngoài map là cố ý); hướng giảm dung lượng ghi bỏ LiberationSans trong khi UI đang dùng; Phần 5 ghi `PlotView` nhưng code là `FarmView`.
- `02_hanh_la`, `03_xa_lach` đang tắt nhưng có sprite: cần xem lại.

---

# Kế hoạch đầy đủ (bản sao `Docs/MVP-Plan.md`)

# Về Quê – Kế hoạch setup MVP trong Unity (28/10/2026)

Mỗi phần nhỏ, chạy và kiểm tra được trước khi sang phần sau. Phần nào code lớn (đánh dấu 🔍) thì đưa kiến trúc chi tiết để duyệt trước.

## Thay đổi so với instructions
- Khu = nhiều luống, lấy từ `ZoneDefinition` (ScriptableObject). Logic định danh ô bằng `(zoneId, bedIndex, col, row)`.
- Camera toàn cảnh KHÔNG thấy hết nông trại: Reference Resolution khoảng 208×370 (≈13 tile bề ngang, zoom 5x trên màn 1080–1170 px). Zoom vào khu theo bounding box (~10x).
- Bố cục map: Map D (trải ngang, 44×26), xem Phần 3.
- Số ô mở sẵn: Khu 1 là 6; Khu 2, 3 mở miễn phí 1 ô khi mở khóa. Đặt trong `ZoneDefinition.startOpenCount`.
- 07/11/2026: đưa câu cá vào MVP (Phần 10); ao không còn là "sắp có".
- 06/11/2026: thêm tiến trình (cấp độ + đơn hàng) vào MVP để người chơi có mục tiêu; cân bằng lại cây theo công thức (Phần 6b). Nhiệm vụ hằng ngày, bộ sưu tập làm sau (Phần 6c).

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
1. Định nghĩa thuần: `CropDef` (id, growSeconds, buyPrice, sellPrice), `ZoneDef` (id, danh sách luống (width, height), startOpenCount, unlockPrices). `ZoneDef` không biết tọa độ tile.
2. Trạng thái: `FarmState` (coins, seeds, inventory, zones → plots), `PlotState` (Locked / Empty / Growing; "chín" suy ra từ timestamp).
3. `IClock` (để test giả lập thời gian, chống chỉnh giờ ở bước sau).
4. Luật: gieo, thu hoạch, mua hạt, bán nông sản, mở ô kế tiếp (hết luống này mới sang luống sau), khu sau mở khi khu trước hết ô.
5. Test EditMode cho từng luật, chạy với nhiều cấu hình khu (1 luống 4×5, 2 luống 6×2, 3 luống 5×2).

**Kiểm tra:** toàn bộ test xanh; chưa cần scene.

## Phần 2 – Data (ScriptableObject)
Lưu ý: asmdef "No Engine References" không chứa được ScriptableObject → SO nằm ở `VeQue.View`, lúc khởi động chuyển sang `CropDef`/`ZoneDef` thuần.
1. `CropDefinition`: id, tên hiển thị, thời gian lớn, giá mua/bán, sprite hạt, các sprite giai đoạn (số sprite = số giai đoạn, thường là 5), icon nông sản.
2. `ZoneDefinition`: zoneId, `beds` (mỗi luống: origin `Vector3Int` + width + height), startOpenCount, giá mở từng ô. Bounding box khu tính từ các luống.
3. `FarmDefinition`: danh sách khu theo thứ tự mở khóa + danh sách cây.
4. Tạo 4 cây và 3 khu.

**Kiểm tra:** một script test trong Editor đọc `FarmDefinition` → tạo `FarmState` đúng số ô mỗi khu.

## Phần 3 – Map & art
1. Cắt tileset cỏ và bộ cây (PPU 16, Slice 16×16, sửa tay sprite cây cao 32 px, pivot Bottom). Chỉ giữ các hàng cây dùng cho MVP.
2. Sprite Atlas (padding 2, Point, không nén).
3. Grid (cell 1) với Tilemap: `Ground` (cỏ + sân đất) → `Decor` (hàng hoa, viền cỏ quanh luống, cây, rơm) → `Plots` (ô đất); vẽ theo Map D bên dưới. Map rộng hơn không làm tăng dung lượng build đáng kể; vẫn dùng chung atlas.
4. Mỗi khu có 1 GameObject `ZoneAnchor` giữ tham chiếu `ZoneDefinition` (origin từng luống nằm trong data); không cần collider vùng chạm — dùng `WorldToCell`.

**Map D** – 44×26 tile, hàng rào bao quanh. Tọa độ [x, y, rộng, cao], gốc trái-trên; khi nhập `origin` cho Unity (trục Y hướng lên, góc dưới-trái của map tại (0, 0)): `originY = 26 − y − cao`.

| Thành phần | Tọa độ |
|---|---|
| Cổng (cạnh trái) | [0, 16, 1, 2] |
| Sân đất | [1, 8, 42, 16] |
| Hàng hoa viền | [1, 7, 42, 1], [1, 23, 42, 1] |
| Chuồng (sân có rào, lối vào cạnh dưới) | [3, 9, 8, 7] |
| Khu 1 | [13, 10, 6, 2], [13, 13, 6, 2] |
| Khu 2 | [21, 10, 6, 2], [21, 13, 6, 2] |
| Khu 3 | [13, 18, 6, 2], [13, 21, 6, 2] |
| Đất mở rộng (sau MVP) | [21, 18, 6, 5] |
| Ao cá | [35, 9, 7, 11] |
| Nhà kho | [14, 1, 6, 6] |
| Cây | (4,2) (9,3) (25,2) (31,3) (38,2) |
| Đống rơm | (23,5) |

Ao, chuồng, nhà kho, đất mở rộng chỉ là art, chạm vào hiện "sắp có".
Tọa độ trên là bố cục tham chiếu; người dùng tự đặt vị trí cuối cùng trong scene. Background trời làm sau, che khoảng trống trên/dưới map ở màn hình dài.

**Kiểm tra:** Game view scale 1x nhìn sắc nét, không có đường kẻ giữa tile; Build Report không tăng bất thường.

## Phần 4 – Camera 🔍
1. Pixel Perfect Camera: Assets PPU 16, Ref ~208×370 (≈13 tile ngang, zoom 5x trên màn 1080–1170 px), Pixel Snapping bật, Crop Frame None.
2. Kéo 1 ngón để di chuyển, giới hạn trong mép map 44×26; ngưỡng 10 px để tách kéo và chạm.
3. Chạm khu → camera tới tâm bounding box của khu rồi mới đổi bậc zoom, Ref tính để bounding box (6×5 tile) chiếm ~85% bề ngang (~10x); chạm bãi cỏ → về 208×370.
4. Template WebGL tối giản: `touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`.

**Kiểm tra:** build lên iPhone (Safari) và Android qua Wi-Fi: kéo mượt, không bị trình duyệt cuộn hay tải lại trang, zoom không làm méo pixel.

## Phần 5 – Hiển thị ô đất & cây
1. `PlotView`: đổi tile ô đất theo trạng thái (khóa / ô mở được tiếp / trống / đã gieo).
2. Mỗi ô 1 `SpriteRenderer` cho cây, sắp xếp theo trục Y; dấu "chín" nhấp nháy.
3. Chạm ô trong chế độ zoom → gửi lệnh vào logic (gieo / tưới / thu hoạch / mở ô). Tưới là tùy chọn, mỗi lần trồng 1 lần, giảm 20% tổng thời gian lớn; đã tưới thì đổi tile đất sang đất ướt tới khi thu hoạch.
4. Cập nhật hiển thị theo sự kiện + mỗi giây một lần (không đếm giờ trong Update).

**Kiểm tra:** gieo rau muống, chờ 2 phút thấy đủ các giai đoạn và thu hoạch được.

## Phần 5b – Hoạt ảnh cây trồng (thêm 07/11/2026)
Hoạt ảnh tự viết trong `FarmView`, dùng sprite giai đoạn có sẵn (không cần sprite mới). Di chuyển làm tròn theo pixel; phóng/thu chỉ thoáng qua (≤ 0,3 giây).
1. **Đợt A (code):** gieo nảy lên (OutBack, 0,25 giây); lên giai đoạn nhún ép dẹt rồi vươn (0,3 giây); chín nhún 1 pixel mỗi 1,5 giây, lệch nhịp giữa các ô (thay nhấp nháy màu, vẫn bật lại được); thu hoạch bản sao bật lên 4 pixel rồi mờ (0,35 giây); tưới 6 giọt nước 1 pixel rơi xuống (0,4 giây). Mọi thông số và công tắc nằm trong Inspector của `FarmView`.
2. **Đợt B (duyệt 07/11/2026):**
   - Nông sản bay về tab Kho: 1–3 icon bay cong từ ô đất, tab Kho nảy, nhãn "+N [icon]" trên tab (cộng dồn cùng loại); bỏ thông báo "+2 Rau muống".
   - Gió bằng shader `VeQue/CropSway` (Built-in): ngọn lệch tối đa 1 pixel làm tròn theo pixel, gốc đứng yên, gió theo đợt lan theo trục X. Tắt gộp draw call (mỗi cây đang hiện 1 draw call); đo Stats, nếu nhiều thì chuyển sang mã hóa gốc vào màu đỉnh.
   - Khung hình không bắt buộc trong `CropDefinition`: chờ có art (Sprout Lands không có khung cho cây).

**Kiểm tra:** chơi trong Editor ở chế độ zoom khu: đủ 5 hoạt ảnh, cây đứng yên không nhòe, chạm nhanh nhiều ô không lỗi.

## Phần 6 – UI (uGUI) 🔍
1. Canvas Scaler (Scale With Screen Size, ref 390×844, match 0.5) + `SafeArea` cho iPhone.
2. Tạm dùng font mặc định TMP (LiberationSans, đủ dấu tiếng Việt). Sau đổi sang font pixel tiếng Việt (VT323 hoặc font khác): TMP font asset tĩnh, đủ ký tự có dấu, Atlas Render Mode Raster, Point. Font pixel của Sprout Lands thiếu toàn bộ ký tự có dấu.
3. HUD (xu, nút sao lưu), thanh điều hướng, bảng chọn hạt, xác nhận mở ô, Kho, Cửa hàng — theo bố cục đề xuất trước khi làm. Chạm ao, chuồng, nhà kho, đất mở rộng → hiện thông báo "sắp có".

4. Hiệu ứng GUI mượt hơn (người dùng sẽ làm sau, chưa chốt; camera và cây trên map tính sau):
   - **Hiệu ứng:** bảng trượt mở nhanh rồi chậm dần (~0,25 giây), nền tối mờ dần; hộp xác nhận nảy từ 85%; thông báo trượt xuống, ẩn bằng mờ dần; thanh tiến độ nảy khi hiện, chạy mượt khi tưới; thanh "Đang gieo" trượt lên; nút thu nhỏ 95% khi nhấn; số xu chạy tới giá trị mới và nảy nhẹ.
   - **Đề xuất:** DOTween Free, tắt hết module, gọi `DOTween.To`; mọi hiệu ứng qua một file `UiAnim` để dễ đổi. Phương án khác: tự viết tween nhỏ.
   - **Các bước:** build mốc GUI hiện tại → cài DOTween → viết `UiAnim`, `UiPressScale`, sửa các script GUI và `UiBuilder` → chạy lại **Về Quê → Tạo UI** → build lại, so Build Report (đo sau khi code đã dùng DOTween).
   - **Cần chốt khi làm:** DOTween hay tự viết; danh sách hiệu ứng; ngưỡng tăng dung lượng (đề xuất tối đa 300 KB đã nén, vượt thì đổi sang tự viết).

5. Thao tác chạm và kéo (duyệt 07/11/2026, thay bảng chọn hạt + thanh "Đang gieo"):
   - Chạm ô trống → thanh hạt nhỏ bám trên ô (icon + số hạt, cây gieo gần nhất đứng đầu; chưa tới cấp / hết hạt thì mờ). Chạm icon = gieo ô đó; kéo icon = gieo mọi ô trống đi qua; hết hạt thì dừng và báo "Hết hạt" (không tự mua). Chạm icon hết hạt → mở Cửa hàng.
   - Đang zoom khu: bắt đầu kéo từ ô chín = quét thu hoạch; từ cây chưa tưới = quét tưới; kéo chỗ khác vẫn di chuyển camera. Chạm 1 ô giữ như cũ.
   - Ô vừa xử lý khi kéo sáng lên thoáng qua.

**Kiểm tra:** chữ có dấu hiển thị đúng trên iPhone; nút ≥ 44 pt; không bị thanh home đè.

## Phần 6b – Tiến trình: cấp độ, đơn hàng, cân bằng kinh tế 🔍
Vòng chơi: trồng → thu hoạch → giao đơn (hoặc bán) → xu + XP → lên cấp → cây mới / mở khu → đơn khó hơn.

**Cân bằng cây** (T = phút lớn; mọi số nằm trong data, menu tạo data tính theo công thức):
- Lãi mỗi lần thu = `5 × (T/2)^0,8` → cây dài lãi/lần cao hơn nhưng lãi/phút thấp hơn (chênh 2,6 lần giữa 2 phút và 4 giờ). Mỗi cây là lựa chọn tốt nhất cho một khoảng vắng mặt.
- Tỉ lệ thu/giá hạt = `2 + 1,5 × ln(T/2) / ln(120)` (2,0 ở 2 phút → 3,5 ở 4 giờ) để bù vốn bị giữ lâu.
- Giá hạt = lãi ÷ (thu/giá hạt − 1); giá bán = (lãi + giá hạt) ÷ sản lượng; làm tròn.
- XP mỗi lần thu = `(T/2)^0,65` (số mũ thấp hơn xu → chơi tích cực lên cấp nhanh hơn).
- Sản lượng giữ theo từng cây. Công thức nằm trong `DefaultDataGenerator`, áp cho cả 25 cây.

| Cây | Phút | Giá hạt | Sản lượng | Giá bán | Lãi/lần | Lãi/phút | XP | Cấp mở |
|---|---|---|---|---|---|---|---|---|
| Rau muống | 2 | 5 | 2 | 5 | 5 | 2,50 | 1 | 1 |
| Cải xanh | 15 | 15 | 2 | 20 | 25 | 1,67 | 4 | 2 |
| Cà chua | 60 | 37 | 3 | 38 | 77 | 1,28 | 9 | 4 |
| Bí đỏ | 240 | 92 | 1 | 322 | 230 | 0,96 | 22 | 6 |

(Bảng duyệt ban đầu chọn tay tỉ lệ thu/giá hạt 2,5 / 3,0 cho cải xanh, cà chua; đổi sang công thức nên lệch ≤ 2 xu.)

**Mục tiêu nhịp** (người ghé 4 lần/ngày): Khu 1 xong trong ngày đầu, Khu 2 khoảng ngày 3–4, Khu 3 khoảng ngày 10–14.

**Đã duyệt (06/11/2026, theo mô phỏng có cấp độ + đơn hàng):**
- XP lên cấp kế tiếp = `3 × cấp^3,2`: 3, 28, 101, 253, 517, 927, 1.519, 2.328, 3.394, 4.755, 6.450, 8.521, 11.009, 13.955.
- Giá mở ô: Khu 1, Khu 2 giữ nguyên; Khu 3 ×3,5 (tổng 614.075 xu, ô cuối 63.700).
- Kết quả mô phỏng, Khu 1/2/3 xong ngày: ghé 4 lần/ngày 0,9 / 3,5 / 11,9; ghé 2 lần/ngày 1,8 / 6,8 / 23,8. Cấp 15 khoảng ngày 9 (ghé 4 lần).
- Giả định: chọn cây tốt nhất, tưới mọi cây, 40% nông sản đi giao đơn, chơi liên tục tối đa 16 ô. Chỉnh lại khi chơi thử.

**A – Cấp độ:**
1. XP từ thu hoạch (`CropDef.Xp`) và giao đơn. Bảng XP lên cấp trong data (bảng đã duyệt ở trên), MVP tối đa 15 cấp.
2. Mở khóa: cấp 1 rau muống; cấp 2 đơn hàng + cải xanh; cấp 4 cà chua; cấp 5 được mở Khu 2; cấp 6 bí đỏ; cấp 9 được mở Khu 3. Cây có `unlockLevel`, khu có `requiredLevel`.
3. Lên cấp: hộp chúc mừng, thưởng xu + vài hạt cây vừa mở.

**B – Đơn hàng làng quê:**
1. 3 đơn cùng lúc (từ cấp 2); mỗi đơn 1–3 loại cây đã mở, số lượng tăng theo cấp; tên người đặt chỉ là chữ.
2. Thưởng xu = Σ(số lượng × giá bán) × 1,6 (làm tròn 5); XP = Σ(số lượng ÷ sản lượng × XP cây) × 2 (tính theo số lần thu, để cây sản lượng cao không được XP gấp nhiều lần).
3. Giao xong → đơn mới ngay. Bỏ đơn → 5 phút sau có đơn thay thế.
4. Sinh đơn qua `IRandom` có seed lưu trong save (test lặp lại được, tải lại trang không đổi đơn).

**Sửa kèm:** chống kẹt (dưới 5 xu, hết hạt, Kho trống, không có cây → tặng 1 hạt rau muống); thông báo "+xu" khi bán, "Đã mở Khu N"; hộp xác nhận mở ô đóng cùng các bảng khác; mua hạt từ bảng chọn hạt xong thì gieo luôn vào ô đang chờ.

**Save:** lên `version` 2 (cấp, XP, đơn đang có, seed ngẫu nhiên); save `version` 1 chuyển lên với cấp 1, XP 0, đơn sinh mới.

**GUI:** HUD có cấp + thanh XP; tab Nông trại / Đơn hàng / Kho / Cửa hàng; hộp lên cấp; Cửa hàng và bảng chọn hạt hiện cây chưa mở là "🔒 Cấp N".

**Các đợt:** 0 tài liệu → 1 Logic A + chống kẹt + test → 2 Logic B + test (có test chạy trọn vòng bằng `FakeClock`) → 3 data + save v2 + test chuyển save → 4 GUI, chạy **Tạo UI**, chơi thử, build lại so Build Report.

**Làm sau (đã ghi, chưa duyệt làm):** công cụ Editor **Về Quê → Cân bằng**: nhập kiểu người chơi (giờ ghé, phiên dài, tưới, % giao đơn) và mục tiêu (khu/cấp xong ngày thứ mấy) → chạy `Farm` thật với `FakeClock` tua nhanh → dò tham số (số mũ lãi, thu/giá hạt, đường XP, hệ số giá khu, thưởng đơn) → nút Áp dụng ghi vào data. Kèm test nhịp (ví dụ ghé 4 lần/ngày hết Khu 3 trong 10–14 ngày). File dự kiến: `Logic/Balance/` (`PlayerProfile`, `BalanceSimulator`, `BalanceFitter`), `Editor/BalanceWindow.cs`, `Tests/EditMode/BalanceTests.cs`. Làm sau Đợt 1–2. Sau MVP: lấy số liệu người chơi thật (mã sao lưu, analytics) để chỉnh kiểu người chơi. Trước khi có công cụ: mô phỏng Python tạm (`sim2.py`, chưa đưa vào repo).

**Kiểm tra:** test xanh; chơi thử trong Editor từ save mới: lên cấp 2 có đơn hàng, giao đơn được, lên cấp mở cây mới, Khu 2 khóa tới khi đủ cấp; save cũ nạp được.

## Phần 6c – Việc làm trong lúc chờ cây lớn & lý do quay lại (thêm 07/11/2026)
Nguyên tắc: thời gian chờ là cố ý (cây ngắn cho lúc đang chơi, cây dài cho lúc vắng mặt); không lấp kín, chỉ thêm việc nhỏ mỗi lần vào game và lý do quay lại.
Làm theo thứ tự dưới đây (từ đơn giản tới phức tạp). Mỗi mục đưa thiết kế chi tiết (luật, con số, file, kiểm tra) để duyệt trước khi làm; xong mục nào chạy lại mô phỏng cân bằng nếu mục đó đổi thu nhập.

**"Ngày" dùng chung (mục 2, 5):** ngày theo giờ địa phương của máy. Trên WebGL, múi giờ trong C# có thể không đúng → lấy độ lệch múi giờ từ trình duyệt qua jslib (`new Date().getTimezoneOffset()`), lưu cùng save. Chống chỉnh giờ: `MonotonicClock` chặn lùi; chấp nhận gian lận chỉnh giờ tới (không có server).

1. **Báo cây chín trên tab trình duyệt** (rất thấp) – thiết kế:
   - Tab bị ẩn thì trình duyệt làm chậm hoặc dừng vòng lặp của Unity → không đếm giờ trong C# lúc ẩn. Lúc tab vừa ẩn (sự kiện `PageHidden` đã có), C# tính **thời điểm cây chín sớm nhất** và **tên cây**, gửi sang jslib; jslib đặt hẹn giờ JS (`setTimeout`, vẫn chạy khi tab ẩn, có thể trễ vài giây) để đổi tiêu đề.
   - Tiêu đề: 1 ô chín → "🌱 Rau muống đã chín · Về Quê"; đã có cây chín ngay lúc ẩn thì đổi luôn. Tab hiện lại → hủy hẹn giờ, trả tiêu đề gốc.
   - File: `VeQueSave.jslib` (`VeQue_SetRipeTitle(giây, tên)`, `VeQue_RestoreTitle`), `WebBridge`, `Farm.TryGetNextRipe` (Logic, có test). Trong Editor không làm gì.
   - Kiểm tra: build, gieo rau muống, chuyển tab, sau ~2 phút tiêu đề đổi; quay lại tab thì trả tiêu đề cũ.
2. **Đơn hàng: thời gian xuất hiện, giới hạn mỗi ngày, đơn gấp** (thấp) – số đã chốt 07/11/2026, thiết kế:
   - **Có đơn mới sau khi giao: 10 phút** (`_orderRefillMinutes`). Bỏ đơn vẫn 5 phút.
   - **Giới hạn 15 đơn giao mỗi ngày** (`_orderDailyLimit`). Chỉ sinh đơn mới khi (đã giao hôm nay + đơn đang có trên bảng) < 15, nên trên bảng không bao giờ có đơn không giao được. Bỏ đơn không tốn lượt. Hết lượt: ô trống hiện "Hết đơn hôm nay, có đơn mới sau 0 giờ"; bảng Đơn hàng hiện "Còn N đơn hôm nay".
   - **Đơn gấp** (ô riêng, nằm trên cùng bảng Đơn hàng): xuất hiện ngẫu nhiên mỗi 2–3 giờ, tối đa 1 đơn, hết hạn sau 30 phút (đếm ngược trên thẻ), thưởng xu và XP ×2 so với đơn thường cùng món; không tính vào giới hạn ngày; hết hạn thì mất và hẹn đơn gấp kế tiếp. Chỉ có từ cấp mở đơn hàng.
   - **Ngày** = theo giờ địa phương: chỉ số ngày = `floor((giờ UTC + độ lệch múi giờ) / 86400)`. Độ lệch lấy từ trình duyệt mỗi lần mở game (jslib `getTimezoneOffset`), trong Editor lấy từ máy. Sang ngày mới thì đếm lại lượt.
   - Logic: `OrderDef` thêm các số trên; `FarmState` thêm số đơn đã giao hôm nay, chỉ số ngày, đơn gấp + hạn, thời điểm đơn gấp kế tiếp; `Farm.SetUtcOffset`. Save lên `version` 3 (save 2 nạp được: lượt hôm nay = 0, chưa có đơn gấp).
   - UI: thẻ đơn gấp (màu khác, đếm ngược), dòng "Còn N đơn hôm nay", ô trống hết lượt; số trên tab Đơn hàng tính cả đơn gấp giao được.
   - Cân bằng: chạy lại mô phỏng (giới hạn 15 đơn/ngày, chờ 10 phút, đơn gấp) và so mục tiêu nhịp; lệch nhiều thì đề xuất chỉnh giới hạn hoặc % thưởng trước khi làm.
   - Kiểm tra: test Logic (chờ 10 phút, đủ 15 thì ngừng, sang ngày mới đếm lại, đơn gấp hết hạn, save 2 → 3); trong Editor dùng **Tua thời gian** thử qua ngày, đơn gấp.
3. **Chuyện bất ngờ trên ruộng** (thấp – trung bình): chim sà xuống / sâu bò trên cây đang lớn → chạm để đuổi; hộp quà, trứng, bó rơm rơi ngẫu nhiên trên map → kéo camera tìm và chạm nhặt. Thưởng ít xu / XP / hạt; giới hạn số lần mỗi giờ. Không có thuốc trừ sâu, không làm hại cây. Cần vài sprite nhỏ.
4. **Bộ sưu tập: nông sản hiếm** (thấp – trung bình): mỗi lần thu hoạch có xác suất nhỏ (đề xuất 3%) ra nông sản vàng; album theo cây, đủ bộ / mốc (vd. thu 100 rau muống) nhận thưởng. Sprite đổi màu, thêm bảng Album.
5. **Nhiệm vụ hằng ngày + điểm danh** (trung bình): 3 nhiệm vụ/ngày (thu N cây, giao N đơn, tưới N lần…), sinh theo cấp; điểm danh 7 ngày nhận quà tăng dần, lỡ ngày thì về ngày 1.
6. **Khách vãng lai** (trung bình): người dân đi qua cổng, hỏi mua một loại nông sản đang có trong Kho với giá cao hơn bán thẳng; đồng ý / từ chối. Không phải nhân vật điều khiển. Cần sprite người + đường đi.
7. **Chế biến** (cao, sau MVP): nồi / cối biến nông sản thành món (có thời gian chờ), bán giá cao hoặc giao đơn đặc biệt. Logic chuỗi sản xuất, art, cân bằng lại kinh tế.
8. **Trang trí nông trại** (cao, sau MVP): mua hàng rào, chậu hoa, bù nhìn rồi đặt lên map; chế độ đặt vật, lưu vị trí, art.

Ngoài phạm vi MVP (theo `CLAUDE.md`): nuôi cá, chuồng, đất mở rộng, trộm nông sản. Câu cá ở Phần 10 (trong MVP).

## Phần 7 – Lưu tiến trình 🔍
1. Save JSON có `version`; lưu ô theo `(zoneId, bedIndex, col, row)`.
2. Lưu khi có thay đổi + khi tab bị ẩn (`visibilitychange`), không chỉ khi thoát.
3. Cây lớn khi offline (tính theo timestamp lúc mở lại); chống chỉnh giờ cơ bản.
4. Xuất / nhập mã sao lưu.

**Kiểm tra:** đóng tab, mở lại sau vài phút thấy cây đã lớn; nhập mã trên máy khác ra đúng tiến trình; thử trong trình duyệt nhúng Facebook (dữ liệu tách riêng với Safari).

## Phần 8 – Build web & phát hành thử
1. Màn hình loading tùy chỉnh nhẹ; Brotli, tắt Decompression Fallback khi lên hosting thật.
2. Hạ Max Memory Size cho iOS; kiểm tra Build Report, mục tiêu < 10 MB đã nén.
3. Test trên Safari iOS, Chrome Android, trình duyệt nhúng Facebook/TikTok (cần tên miền riêng thay `*.workers.dev`).

**Kiểm tra:** mở link trên điện thoại là chơi được trong vài giây, chơi trọn một vòng gieo → bán → mở ô.

## Phần 10 – Câu cá (trong MVP từ 07/11/2026, chưa làm) 🔍
Đưa vào MVP 07/11/2026 (`CLAUDE.md` đã sửa phạm vi). Ao cá có sẵn trên Map D [35, 9, 7, 11]; bỏ `ComingSoonArea` của ao khi làm phần này. Thứ tự đề xuất: sau Phần 6c mục 1–2 và sau khi game chạy được trên điện thoại (Phần 4 template WebGL).
Vòng chơi: **chọn mồi → ném cần (canh perfect) → chờ cá cắn, giật đúng lúc → kéo cá bằng chuỗi mũi tên 4 hướng → được cá (loài, độ hiếm, cân nặng, giá) hoặc sổng / đứt dây / gãy cần.**
Tham khảo cơ chế chung của thể loại: thanh lực có điểm ngọt khi ném, báo hiệu cắn câu + khung phản xạ ngắn, rồi một mini game kéo cá (Stardew Valley, Fish Quest, WebFishing, Tiny Fishing). Kéo cá dùng kiểu nhập chuỗi mũi tên theo nhịp (kiểu game nhảy theo nhịp) – chỉ mượn cơ chế, art và tên riêng.

**1. Ném cần – canh perfect**
- Đứng ở bờ ao (zoom vào ao như zoom khu). Nhấn giữ nút Ném: thanh lực chạy lên xuống liên tục (0 → 100% → 0, chu kỳ ~1,2 giây); thả tay để ném.
- Vùng trên thanh: **Perfect** (hẹp, ở gần đỉnh), **Tốt**, **Thường**. Cần câu rank cao thì vùng Perfect rộng hơn, thanh chạy chậm hơn.
- Lực quyết định phao rơi gần hay xa bờ: xa = nước sâu = cá to, cá hiếm hơn.
- Perfect: tăng tỉ lệ cá hiếm, chờ cắn ngắn hơn, vòng kéo đầu dễ hơn; có hiệu ứng + chữ "Perfect!". Ném hỏng (quá yếu): phao rơi sát bờ, chỉ có cá thường.

**2. Chờ cá cắn – giật câu**
- Chờ ngẫu nhiên (vd. 3–15 giây; mồi hợp loài, ném perfect thì ngắn hơn). Phao **rỉa nhẹ** vài lần (mồi nhử) rồi mới **chìm hẳn + dấu "!"**.
- Chạm "Giật" trong khung phản xạ (vd. 0,9 giây với cá thường, 0,4 giây với cá huyền thoại). Giật lúc phao chỉ rỉa → cá sợ bỏ đi, mất mồi. Trễ → cá ăn mất mồi.
- **Tỉ lệ trượt**: giật đúng lúc vẫn có xác suất trượt lưỡi (cá sổng ngay), giảm theo rank cần, tăng theo độ hiếm cá.
- Android có thể rung máy khi cá cắn (`navigator.vibrate`); iPhone trên web không rung được → dựa vào hình + âm thanh.

**3. Kéo cá – chuỗi mũi tên 4 hướng (kiểu game nhảy theo nhịp)**
- Cá có **thể lực**, dây có **độ căng**. Mỗi **vòng**: hiện chuỗi mũi tên (↑ ↓ ← →), người chơi nhập đúng thứ tự trong thời gian giới hạn, rồi chạm **Chốt** đúng lúc con trỏ chạy qua vạch nhịp (Perfect / Tốt / Trượt).
- Nhập đúng + chốt tốt → trừ thể lực cá (Perfect trừ nhiều, có chuỗi combo). Sai mũi tên hoặc hết giờ → độ căng tăng. Độ căng đầy → **đứt dây**: cá thoát, mất mồi, cần mất độ bền nhiều hơn, có xác suất **gãy cần**.
- Cá càng hiếm: nhiều vòng hơn, chuỗi dài hơn (4 → 10 mũi tên), ít thời gian hơn, vạch nhịp hẹp hơn; cá huyền thoại có **mũi tên đỏ** (bấm hướng ngược lại) và chuỗi đổi giữa chừng.
- Nhập trên điện thoại: **4 nút mũi tên lớn** (≥ 56 pt) + nút **Chốt**, đặt ở nửa dưới màn hình (ổn định hơn vuốt khi nhập nhanh); có thể thêm tùy chọn vuốt. Cần thử cảm giác trên máy thật.

**4. Cá**
- Mỗi loài: độ hiếm (Thường / Khá / Hiếm / Quý / Huyền thoại), khoảng cân nặng (kg, lệch về nhẹ; cá nặng hiếm hơn), giá = giá gốc × hệ số cân nặng, khung giờ xuất hiện (mục 4b), độ sâu, mồi ưa thích, độ khó (số vòng, độ dài chuỗi, thời gian, khung giật, tỉ lệ trượt).
- Loài gợi ý: Thường – cá rô, cá diếc, tép; Khá – cá chép, cá trê, cá mè; Hiếm – cá lóc, lươn, tôm càng xanh; Quý – cá bống tượng, cá chép đỏ; Huyền thoại – cá chép vàng, cá trắm đen khổng lồ.
- Kỷ lục cân nặng mỗi loài vào album (nối với Phần 6c mục 4). Cá vào **giỏ cá** (giữ loài + cân nặng, có sức chứa) → bán, giao đơn, chế biến (Phần 6c mục 7).

**4b. Cá xuất hiện theo khung giờ (yêu cầu 07/11/2026)**
- Một ngày chia 7 khung theo giờ địa phương của máy (dùng chung cách tính ngày/giờ với đơn hàng, Phần 6c mục 2): Bình minh 5–7h, Sáng 7–11h, Trưa 11–14h, Chiều 14–17h, Hoàng hôn 17–19h, Tối 19–23h, Khuya 23–5h.
- Mỗi loài có hệ số theo từng khung: 0 = không có, 1 = bình thường, > 1 = ăn mạnh. Khung tính lúc ném cần (đang kéo cá mà qua khung mới thì không đổi).
- Gợi ý: cá rô, cá diếc cả ngày; cá chép mạnh buổi sáng; cá lóc mạnh lúc bình minh và hoàng hôn; cá trê, lươn chỉ tối và khuya; tôm càng tối; cá bống tượng chiều tối; **cá chép vàng chỉ lúc bình minh**; **cá trắm đen khổng lồ chỉ lúc khuya**.
- Công bằng với người chỉ chơi vài giờ cố định: cá Hiếm trở xuống có ít nhất 2 khung; chỉ cá Quý / Huyền thoại mới có 1 khung, và có mồi đặc biệt tăng tỉ lệ để bù.
- UI khi zoom vào ao: "Khung giờ: Hoàng hôn (17–19h) · còn 1:24", danh sách cá đang ăn mạnh (loài chưa bắt được hiện "?" – khám phá dần qua album); album ghi khung giờ của mỗi loài đã bắt.
- Có thể thêm sau: màu map đổi theo khung giờ (sáng, chiều vàng, tối xanh), đom đóm ban đêm – làm đẹp, không bắt buộc.
- Chỉnh giờ máy để câu cá theo khung: chấp nhận (không có server), `MonotonicClock` chỉ chặn lùi giờ. Trong Editor thử bằng **Tua thời gian**.
- Logic: `FishDef` có bảng hệ số 7 khung; hàm khung giờ thuần (giờ UTC + độ lệch múi giờ → khung), có test ranh giới (4:59 / 5:00, qua nửa đêm).

**5. Cần câu – rank, độ bền, sửa / mua mới**
- Rank gợi ý: Cần tre → Cần trúc → Cần sợi thủy tinh → Cần carbon → Cần vàng. Mua ở Cửa hàng, mở theo cấp.
- Chỉ số: độ rộng vùng Perfect, tốc độ thanh lực, sức bền dây (độ căng tối đa), tỉ lệ trượt, thưởng tỉ lệ cá hiếm, **độ bền**.
- Mỗi lần câu trừ độ bền (đứt dây trừ nhiều hơn). Độ bền về 0 hoặc gãy khi đứt dây → **cần gãy**: sửa (tốn xu, rẻ hơn mua) hoặc mua mới. Có thể giữ nhiều cần, chọn cần trước khi câu.

**6. Mồi câu**
- Mỗi lần ném tốn 1 mồi; mồi quyết định loài nào dễ cắn: giun → rô, diếc, trê; cám → chép, mè; tép → lóc, bống; mồi giả → lóc, cá quý. Mồi đặc biệt hút cá hiếm.
- Nguồn: giun có khi đào được lúc thu hoạch (nối câu cá với trồng trọt), mồi khác mua ở Cửa hàng. Hết mồi là nghỉ → giới hạn tự nhiên, không cần thanh năng lượng.

**7. Kỹ thuật & thứ tự làm (khi được đưa vào phạm vi)**
- Logic thuần (`VeQue.Logic`): `FishDef`, `RodDef`, `BaitDef`, phiên câu dạng state machine (Chờ ném → Đang ném → Chờ cắn → Cá cắn → Kéo → Kết quả), sinh cá / sinh chuỗi mũi tên bằng `SeededRandom`, thời gian qua `IClock`; test cho tỉ lệ, cân nặng, trượt, đứt dây, độ bền. Save: cần + độ bền, mồi, giỏ cá, kỷ lục.
- View: zoom vào ao, phao + dây (pixel), thanh lực, nút mũi tên + vạch nhịp, hiệu ứng Perfect / đứt dây / gãy cần, âm thanh (chỉ phát sau lần chạm đầu trên web).
- Art cần có: phao, cần theo rank, sprite từng loài cá, mồi, nước động. Âm thanh: quăng cần, cá cắn, đứt dây.
- Đợt: (1) Logic + test; (2) ném + chờ cắn; (3) kéo cá mũi tên; (4) cửa hàng cần / mồi, sửa cần; (5) giỏ cá, bán, album, đơn hàng có cá. Cân bằng: thêm câu cá vào mô phỏng kinh tế (thu nhập / giờ so với trồng cây).
- Cần chốt khi làm: nút mũi tên hay vuốt; XP câu cá chung hay riêng; cá giữ trong giỏ hay bán ngay.

---

# Tiến độ đầy đủ (bản sao `Docs/tien-do.md`)

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
| 10 – Câu cá | Trong MVP (07/11/2026), đã có thiết kế, chưa làm | Ném perfect, giật câu, kéo cá mũi tên 4 hướng, cá theo độ hiếm/cân nặng/giá/khung giờ, cần rank/độ bền/trượt/gãy, mồi; cần art cá, phao, cần, nước |
| 7 – Lưu tiến trình | Đã viết code (save `version` 2) | Thử trên máy thật, nhập mã máy khác, trình duyệt nhúng Facebook |
| 8 – Build & phát hành | Chưa làm | Toàn bộ |

Git: nhánh `setup/phan-0-camera`, commit cuối `e65601e` (gồm Phần 5, 5b, 6 mục 5, 6b, 7). Tài liệu kế hoạch Phần 6c, 10 sau commit đó **chưa commit**.
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
- Thứ tự đề xuất: Phần 6c mục 1–2 → Phần 4 template WebGL + chạy trên điện thoại → Phần 10 câu cá.

### Còn lại
- Người dùng chưa cho bắt đầu Phần 6c mục 1–2 (07/11/2026: "chưa làm").
- Câu cá cần chốt khi làm: nút mũi tên hay vuốt; XP câu cá chung hay riêng; cá giữ trong giỏ hay bán ngay; nguồn art (cá, phao, cần, nước) và âm thanh.

## Tiếp theo (khi quay lại)
0. Phần 6 mục 5 – chạy **Tạo UI**, Test Runner (221), chơi thử gieo chạm/kéo, kéo thu hoạch, kéo tưới; thử cảm giác kéo trên điện thoại.
1. Phần 6c mục 1–2 (khi người dùng cho làm): chạy lại mô phỏng cân bằng với giới hạn đơn → duyệt → code.
2. Phần 4 – template WebGL (`touch-action: none`, chặn `gesturestart`, `viewport-fit=cover`) + build, so Build Report (mốc 10,8 MB) + thử trên iPhone Safari / Android; cùng lúc kiểm tra Phần 7 trên máy thật.
3. Phần 10 – câu cá (trong MVP): chốt các câu hỏi còn lại, chuẩn bị art, rồi làm theo 5 đợt trong `MVP-Plan.md`.
4. Phần 3 – art (setup ô đất), vẽ phần còn lại trên `Decor`, tạo `ComingSoonArea` (trừ ao), Sprite Atlas.
5. Chưa quyết: gỡ package không dùng (spriteshape, psdimporter, aseprite, 2d.animation); giảm build < 10 MB (Phần 8).
6. Làm sau: Phần 6c mục 3–8, Phần 6 mục 4 (hiệu ứng GUI), công cụ Cân bằng, art 32×32, khung hình cây.
