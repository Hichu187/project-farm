# Về Quê – Kế hoạch setup MVP trong Unity (28/10/2026)

Mỗi phần nhỏ, chạy và kiểm tra được trước khi sang phần sau. Phần nào code lớn (đánh dấu 🔍) thì đưa kiến trúc chi tiết để duyệt trước.

## Thay đổi so với instructions
- Khu = nhiều luống, lấy từ `ZoneDefinition` (ScriptableObject). Logic định danh ô bằng `(zoneId, bedIndex, col, row)`.
- Camera toàn cảnh KHÔNG thấy hết nông trại: Reference Resolution khoảng 208×370 (≈13 tile bề ngang, zoom 5x trên màn 1080–1170 px). Zoom vào khu theo bounding box (~10x).
- Bố cục map: Map D (trải ngang, 44×26), xem Phần 3.
- Số ô mở sẵn: Khu 1 là 6; Khu 2, 3 mở miễn phí 1 ô khi mở khóa. Đặt trong `ZoneDefinition.startOpenCount`.
- 07/11/2026: đưa câu cá vào MVP (Phần 10) ở vùng câu riêng (cùng scene, ngoài cổng trái); thêm nhân vật (đi lang thang ở nông trại, chạm để đi ở vùng câu, hiển thị nhiều lớp để sau thêm trang phục); ao nhà chỉ để thả cá sưu tầm.
- 11/11/2026: bỏ nhân vật đi lang thang; ở nông trại cũng chạm để đi, gieo / tưới / thu hoạch xếp vào hàng việc, nhân vật đi tới làm lần lượt đúng thứ tự chọn (Phần 10 mục 0b); zoom giữ nguyên.
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
| Ao nhà (thả cá sưu tầm, Phần 10) | [35, 9, 7, 11] |
| Nhà kho | [14, 1, 6, 6] |
| Cây | (4,2) (9,3) (25,2) (31,3) (38,2) |
| Đống rơm | (23,5) |

Chuồng, nhà kho, đất mở rộng chỉ là art, chạm vào hiện "sắp có". Ao nhà dùng cho Phần 10. Vùng câu (Phần 10) đặt ngoài cổng trái, cùng Grid; tọa độ chốt khi làm.
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
   - **Từ Phần 10 giai đoạn 1 (duyệt 11/11/2026):** thao tác chạm / kéo giữ nguyên, nhưng gieo / tưới / thu hoạch không làm ngay mà xếp vào **hàng việc** của nhân vật; nhân vật đi tới làm lần lượt (xem Phần 10 mục 0b). Mở ô bằng xu vẫn làm ngay khi xác nhận.
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

## Phần 10 – Nhân vật, vùng câu, câu cá, ao nhà (trong MVP từ 07/11/2026, chưa làm) 🔍
Đưa vào MVP 07/11/2026 (`CLAUDE.md` đã sửa phạm vi). Thứ tự đề xuất: sau Phần 6c mục 1–2 và sau khi game chạy được trên điện thoại (Phần 4 template WebGL).

**0. Nhân vật, vùng câu, ao nhà (chốt 07/11/2026)**
- **Đánh giá:** nhân vật tự nó không làm vòng chơi hấp dẫn hơn (câu cá mới là phần chơi tay bù cho trồng trọt là chờ), nhưng mở ra: nhân vật của riêng người chơi; **chỗ tiêu xu sau khi mở hết đất** (Khu 3 hết khoảng ngày 12, cấp 15 khoảng ngày 9) bằng trang phục / phụ kiện / cần rank cao mà không lệch cân bằng; dễ thêm vùng chơi mới. Điều kiện: đoạn đi bộ ngắn, bỏ qua được; chế độ đang ở luôn rõ ràng.
- **Nông trại (nhìn từ trên xuống, như hiện tại):** người chơi điều khiển nhân vật bằng chạm để đi; nhân vật đi tới ô đất để gieo / tưới / thu hoạch (chi tiết mục 0b). Logic zoom / kéo camera giữ nguyên; camera không đi theo nhân vật ở nông trại. Không có đi lang thang làm cảnh: rảnh thì đứng yên (idle). Vẽ theo trục Y cùng cây.
- **Sang vùng câu:** bấm **Đi câu** (hoặc chạm cổng) → nhân vật **tự đi ra cổng** (cổng trái Map D) → chuyển sang vùng câu đặt ngoài cổng trái (cùng scene, cùng Grid). Chạm thêm lần nữa thì đi nhanh / bỏ qua đoạn đi ra cổng. **Về nhà** thì ngược lại.
- **Ở vùng câu (điều khiển nhân vật):** chạm đất trống → tìm đường và đi tới (có dấu điểm đến); **chạm chỗ câu** (bến / mép nước có biểu tượng) → đi tới điểm đứng, quay mặt ra nước, **tới nơi mới bắt đầu câu**; đang đi chạm chỗ khác thì đổi điểm đến. Câu nhiều lần tại chỗ không phải đi lại; nhớ chỗ câu lần trước. Không joystick.
- **Camera ở vùng câu:** đi theo nhân vật (mượt, trong mép vùng); **kéo để xem xung quanh**, thả tay một lúc (vd. 2 giây) thì tự quay về nhân vật. Đang câu thì cố định khung quanh nhân vật + phao.
- **Chế độ rõ ràng:** HUD đổi khi ở vùng câu (tên vùng, nút **Về nhà**, ẩn thanh hạt và các tab chỉ dùng ở nông trại).
- **Vùng câu MVP:** 1 vùng nhỏ, vài chỗ câu gần lối vào. Thêm vùng (rừng, chợ, sông khác – mỗi vùng bộ cá riêng) sau MVP.
- **Ao nhà** (Map D [35, 9, 7, 11]): chỉ thả cá sưu tầm – chọn giữ cá thay vì bán; cá không lớn, không đẻ, không cho ăn, không tạo thu nhập. Cá hiện bằng **bóng màu theo độ hiếm** bơi lang thang; sức chứa ban đầu 6 (nâng cấp sau); **chạm ao → bảng danh sách cá trong ao** (loài, cân nặng, ngày bắt, khung giờ), thả / bắt lại. Làm **sau cùng**. Bỏ `ComingSoonArea` của ao khi làm.
- **Trang phục (chuẩn bị từ đầu, cửa hàng sau MVP):** nhân vật vẽ bằng nhiều lớp đồng bộ khung (thân, mũ, phụ kiện). Thân đổi cả bộ + biến thể màu bằng thay bảng màu (gần như không tốn art); mũ / phụ kiện là lớp riêng bám điểm neo đầu theo từng khung (chỉ vẽ 4 hướng mỗi món). Sau có thể chuyển sang phối đồ đầy đủ (mỗi món vẽ đủ khung) mà không viết lại hệ hiển thị. MVP chỉ có 1 trang phục mặc định.

**0b. Nhân vật làm việc ở nông trại (duyệt 11/11/2026)**
- **Chạm (zoom giữ nguyên):** toàn cảnh: chạm khu → zoom vào như cũ; chạm đất đi được → nhân vật đi tới (có dấu điểm đến); chạm cổng → đi câu. Đang zoom khu: chạm ngoài khu → về toàn cảnh như cũ (nhân vật không đi). Chạm ô trong khu → như Phần 6 mục 5, nhưng việc vào hàng việc.
- **Hàng việc** (`FarmTaskQueue`, C# thuần trong `VeQue.Logic`, có test): mỗi việc = `(PlotId, Gieo / Tưới / Thu, cropId)`.
  - Làm **đúng thứ tự được chọn** (thứ tự chạm, hoặc thứ tự ngón tay kéo qua); không tự sắp lại theo đường ngắn nhất.
  - Mỗi ô chỉ có 1 việc đang chờ (trùng thì bỏ qua). Gieo **giữ chỗ hạt**: kéo qua nhiều ô hơn số hạt thì chỉ xếp đủ số hạt, báo "Hết hạt"; thanh hạt hiện số hạt trừ phần giữ chỗ.
  - Kiểm tra lúc xếp việc và **kiểm tra lại lúc tới nơi** (gọi `Farm.Plant / Water / Harvest`); không làm được (cây chưa chín, đã thu, hết hạt…) thì bỏ việc đó, báo 1 lần.
- **Đi tới chỗ làm:** A* (`GridPath`); điểm đứng là tile đi được sát ô, ưu tiên ngay trên / dưới (luống 6×2: hàng trên sát lối trên, hàng dưới sát lối dưới, giữa 2 luống có 1 tile trống), không có thì tile đi được gần nhất. Tới nơi quay mặt về ô, hoạt ảnh làm việc ~0,3–0,4 giây, rồi mới đổi trạng thái ô (hoạt ảnh cây Phần 5b chạy lúc đó).
- **Hiển thị:** ô đang chờ có dấu nhỏ; làm xong sáng lên (`FarmView.Flash`). Chạm cây chưa tưới: thanh tiến độ hiện ngay, nhân vật tới nơi mới tưới.
- **Nhân vật ở xa / hàng việc dài:** nhân vật ngoài khung hình khu đang zoom → hiện ở mép khu gần nhất rồi đi vào. Tốc độ đi tăng dần theo độ dài hàng việc (có trần).
- **Hủy:** chạm nhân vật hoặc nút **Dừng** (hiện khi hàng việc không rỗng) → xóa phần còn lại, trả hạt đã giữ chỗ. Đang có hàng việc mà chạm đất để đi → cũng hủy hàng việc.
- **Tab bị ẩn khi còn việc:** làm ngay toàn bộ phần còn lại (theo thứ tự, vẫn kiểm tra lại từng việc) rồi lưu. Hàng việc và vị trí nhân vật không lưu.
- **Không cần đi bộ:** mở ô bằng xu, chạm chuồng / nhà kho / đất mở rộng ("sắp có"), Cửa hàng, Kho, Đơn hàng.
- **Kiểm tra:** test Logic (thứ tự, trùng ô, giữ chỗ hạt, kiểm tra lại khi tới nơi, hủy trả hạt, làm hết khi ẩn tab); trong Editor: chạm 1 ô, kéo gieo / thu / tưới 12 ô thấy nhân vật làm lần lượt đúng thứ tự, kéo quá số hạt chỉ xếp đủ hạt, hủy giữa chừng, zoom / kéo camera như cũ, ẩn tab rồi mở lại thấy việc đã làm xong.

- **Art có sẵn:** nhân vật `Basic Charakter Spritesheet.png`, `Basic Charakter Actions.png` (Sprout Lands Basic); câu cá `fishing animation front/back/side.png`, `fishing_rod*.png`, `fishing water splash frames and rod.png`, `Fish Sprites.png`, `small fish.png`, `mediuml fish.png`, `big fish 2 swimming in cirkels.png` (Ocean Pack). Cần kiểm tra đủ khung khi làm. Cần thêm: tile vùng câu (nước, bờ, bến), biểu tượng chỗ câu.
Vòng chơi: **chọn mồi → ném cần (canh perfect) → chờ cá cắn, giật đúng lúc → kéo cá bằng chuỗi mũi tên 4 hướng → được cá (loài, độ hiếm, cân nặng, giá) hoặc sổng / đứt dây / gãy cần.**
Tham khảo cơ chế chung của thể loại: thanh lực có điểm ngọt khi ném, báo hiệu cắn câu + khung phản xạ ngắn, rồi một mini game kéo cá (Stardew Valley, Fish Quest, WebFishing, Tiny Fishing). Kéo cá dùng kiểu nhập chuỗi mũi tên theo nhịp (kiểu game nhảy theo nhịp) – chỉ mượn cơ chế, art và tên riêng.

**1. Ném cần – canh perfect**
- Nhân vật đã đứng ở chỗ câu trong vùng câu. Nhấn giữ nút Ném: thanh lực chạy lên xuống liên tục (0 → 100% → 0, chu kỳ ~1,2 giây); thả tay để ném.
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
- Nhập trên điện thoại (chốt): **4 nút mũi tên lớn** (≥ 56 pt) + nút **Chốt**, đặt ở nửa dưới màn hình. Cần thử cảm giác trên máy thật.

**4. Cá**
- Mỗi loài: độ hiếm (Thường / Khá / Hiếm / Quý / Huyền thoại), khoảng cân nặng (kg, lệch về nhẹ; cá nặng hiếm hơn), giá = giá gốc × hệ số cân nặng, khung giờ xuất hiện (mục 4b), độ sâu, mồi ưa thích, độ khó (số vòng, độ dài chuỗi, thời gian, khung giật, tỉ lệ trượt).
- Loài gợi ý: Thường – cá rô, cá diếc, tép; Khá – cá chép, cá trê, cá mè; Hiếm – cá lóc, lươn, tôm càng xanh; Quý – cá bống tượng, cá chép đỏ; Huyền thoại – cá chép vàng, cá trắm đen khổng lồ.
- Kỷ lục cân nặng mỗi loài vào album (nối với Phần 6c mục 4). Cá vào **giỏ cá** (giữ loài + cân nặng, có sức chứa) → bán, giao đơn, chế biến (Phần 6c mục 7).

**4b. Cá xuất hiện theo khung giờ (yêu cầu 07/11/2026)**
- Một ngày chia 7 khung theo giờ địa phương của máy (dùng chung cách tính ngày/giờ với đơn hàng, Phần 6c mục 2): Bình minh 5–7h, Sáng 7–11h, Trưa 11–14h, Chiều 14–17h, Hoàng hôn 17–19h, Tối 19–23h, Khuya 23–5h.
- Mỗi loài có hệ số theo từng khung: 0 = không có, 1 = bình thường, > 1 = ăn mạnh. Khung tính lúc ném cần (đang kéo cá mà qua khung mới thì không đổi).
- Gợi ý: cá rô, cá diếc cả ngày; cá chép mạnh buổi sáng; cá lóc mạnh lúc bình minh và hoàng hôn; cá trê, lươn chỉ tối và khuya; tôm càng tối; cá bống tượng chiều tối; **cá chép vàng chỉ lúc bình minh**; **cá trắm đen khổng lồ chỉ lúc khuya**.
- Công bằng với người chỉ chơi vài giờ cố định: cá Hiếm trở xuống có ít nhất 2 khung; chỉ cá Quý / Huyền thoại mới có 1 khung, và có mồi đặc biệt tăng tỉ lệ để bù.
- UI khi ở vùng câu: "Khung giờ: Hoàng hôn (17–19h) · còn 1:24", danh sách cá đang ăn mạnh (loài chưa bắt được hiện "?" – khám phá dần qua album); album ghi khung giờ của mỗi loài đã bắt.
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

**7. Kỹ thuật & thứ tự làm**
- Logic thuần (`VeQue.Logic`): `FishDef`, `RodDef`, `BaitDef`, phiên câu dạng state machine (Chờ ném → Đang ném → Chờ cắn → Cá cắn → Kéo → Kết quả), sinh cá / sinh chuỗi mũi tên bằng `SeededRandom`, thời gian qua `IClock`; test cho tỉ lệ, cân nặng, trượt, đứt dây, độ bền. Save: cần + độ bền, mồi, giỏ cá, kỷ lục.
- View: khung camera câu cá, phao + dây (pixel), thanh lực, nút mũi tên + vạch nhịp, hiệu ứng Perfect / đứt dây / gãy cần, âm thanh (chỉ phát sau lần chạm đầu trên web).
- Nhân vật / vùng câu: `GridPath` (A*, C# thuần, có test) trên lưới ô đi được lấy từ Tilemap; `CharacterView` (nhiều lớp đồng bộ khung, 4 hướng, đi / đứng / câu, di chuyển theo pixel, vẽ theo trục Y); bộ điều khiển chạm để đi (nông trại + vùng câu), `FarmTaskQueue` + bộ chạy hàng việc ở nông trại; `CameraController` giới hạn + zoom theo vùng, chế độ đi theo nhân vật + kéo xem rồi tự quay về; `CameraTouchInput` thêm chế độ vùng câu. Save: không lưu vị trí nhân vật (luôn vào game ở nông trại); lưu cá trong ao nhà.
- Art cần có: phao, cần theo rank, sprite từng loài cá, mồi, nước động, tile vùng câu (xem mục 0). Âm thanh: bước chân, quăng cần, cá cắn, đứt dây.
- Giai đoạn (mỗi giai đoạn duyệt thiết kế chi tiết trước khi làm):
  1. **Nhân vật ở nông trại: chạm để đi + hàng việc trồng trọt** (mục 0b): `GridPath`, lưới ô đi được, `CharacterView` nhiều lớp (1 trang phục), chạm để đi, `FarmTaskQueue`, nối `FarmInteraction` / `FarmGesture` / `SeedPicker` vào hàng việc, nút Dừng, làm hết khi ẩn tab.
  2. **Vùng câu + di chuyển**: vẽ vùng ngoài cổng trái, giới hạn camera theo vùng, **Đi câu** (đi ra cổng) / **Về nhà**, chạm để đi, chạm chỗ câu, camera đi theo + kéo xem rồi tự quay về, HUD theo chế độ.
  3. **Câu cá – Logic + test**: `FishDef` (gồm khung giờ), `RodDef`, `BaitDef`, phiên câu, sinh cá / chuỗi mũi tên, độ bền.
  4. **Câu cá – chơi**: ném + chờ cắn + giật; kéo cá mũi tên; cửa hàng cần / mồi, sửa cần.
  5. **Giỏ cá, bán, album, đơn hàng có cá**.
  6. **Ao nhà** (sau cùng): thả cá, bóng cá bơi, bảng danh sách cá.
  - Sau MVP: cửa hàng trang phục / phụ kiện, thêm vùng chơi.
  - Cân bằng: thêm câu cá vào mô phỏng kinh tế (thu nhập / giờ so với trồng cây). Mỗi giai đoạn thêm art: build lại, so Build Report.
- Đã chốt 07/11/2026:
  - Nhập khi kéo cá: **4 nút mũi tên** + nút Chốt (không dùng vuốt).
  - **XP câu cá cộng vào XP nhân vật** (chung cấp với trồng trọt). XP mỗi con theo độ hiếm + cân nặng; đưa vào mô phỏng cân bằng để nhịp lên cấp không lệch mục tiêu.
  - **Cá giữ trong giỏ** (loài + cân nặng, có sức chứa); từ giỏ: bán, giao đơn, thả ao nhà. Giỏ đầy thì phải bán / thả bớt mới câu tiếp.
  - Asset vùng câu (tile nước, bờ, bến), biểu tượng chỗ câu, âm thanh: **người dùng thêm sau**; làm code trước với hình tạm, thay khi có asset.
