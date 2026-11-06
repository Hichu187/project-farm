# Về Quê – Kế hoạch setup MVP trong Unity (28/10/2026)

Mỗi phần nhỏ, chạy và kiểm tra được trước khi sang phần sau. Phần nào code lớn (đánh dấu 🔍) thì đưa kiến trúc chi tiết để duyệt trước.

## Thay đổi so với instructions
- Khu = nhiều luống, lấy từ `ZoneDefinition` (ScriptableObject). Logic định danh ô bằng `(zoneId, bedIndex, col, row)`.
- Camera toàn cảnh KHÔNG thấy hết nông trại: Reference Resolution khoảng 208×370 (≈13 tile bề ngang, zoom 5x trên màn 1080–1170 px). Zoom vào khu theo bounding box (~10x).
- Bố cục map: Map D (trải ngang, 44×26), xem Phần 3.
- Số ô mở sẵn: Khu 1 là 6; Khu 2, 3 mở miễn phí 1 ô khi mở khóa. Đặt trong `ZoneDefinition.startOpenCount`.
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

## Phần 6c – Nhiệm vụ hằng ngày, bộ sưu tập (làm sau 6b)
1. Nhiệm vụ hằng ngày (3 nhiệm vụ/ngày theo ngày địa phương) + thưởng điểm danh. Lưu ý chỉnh giờ máy: dùng `MonotonicClock`, chấp nhận gian lận chỉnh giờ tới.
2. Bộ sưu tập / mốc thành tích (ví dụ thu 100 rau muống) có thưởng.

Thiết kế chi tiết đưa duyệt khi tới phần này.

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
