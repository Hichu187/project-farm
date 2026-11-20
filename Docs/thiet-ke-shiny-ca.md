# Cá đột biến lấp lánh (shiny) – thiết kế và kế hoạch (20/11/2026)

Trạng thái: **thiết kế đã chốt, vị trí trong lịch đã chốt (Phần 18, sau Phần 6), kiến trúc đã duyệt 20/11/2026; Bước 1 (Logic + save + test) đã viết code, chờ Test Runner**. Đã chốt thêm: "Bán hết" bỏ qua cá shiny; random shiny riêng (sinh từ seed phiên).

Ảnh mô phỏng (Python, đúng thuật toán shader bên dưới), trong `C:\Users\PC\Downloads\VeQue-art\fish_hd\`:
- **`shiny_8kieu_full.gif`: đủ 8 kiểu, bản chốt** (script `shiny_8kieu.py`).
- `shiny_cauvong_options.gif`: các option cho kiểu Cực quang (tên cũ Xà cừ cầu vồng), chọn **D** (script `shiny_rainbow_opts.py`).
- `shiny_hac_cucquang_options.gif`: các option cho kiểu Tinh dạ (tên cũ Hắc cực quang, Hắc thiên hà), chọn **H1 – Cực quang chìm** (script `shiny_hac_cucquang.py`). Bản tinh vân C2 trước đó (`shiny_hac_thienha_tim.gif`) đã bỏ.
- Đổi tên 2 kiểu hiếm (chốt 20/11): Xà cừ cầu vồng → **Cực quang**, Hắc thiên hà → **Hắc cực quang** (enum `Opal` → `Aurora`, `DarkGalaxy` → `DarkAurora`, giữ số 2 và 8). Đổi tên 2 kiểu thường: Thiên hà → **Tinh vân** (`Galaxy` → `Nebula`, giữ số 4), Đen tuyền → **Hắc ngọc** (`Obsidian` → `Onyx`, giữ số 6). Tên file GIF/script cũ giữ nguyên. **Đổi tên lần 2 (20/11):** Hắc cực quang → **Tinh dạ**, Hắc ngọc → **Hư không** (enum giữ `DarkAurora` = 8, `Onyx` = 6).
- Bản cũ để tham khảo: `shiny_5kieu.gif`, `shiny_den_bac.gif`.

## 1. Luật chơi (đã chốt 20/11)
- Mỗi lần **câu được cá** có tỉ lệ **1/100** ra cá shiny. Áp dụng cho cả 20 loài (kể cả cá trắm đen, cá hô).
- Có **8 kiểu**, chia 2 bậc (chốt 20/11):
  - **Bậc thường** (6 kiểu): Hoàng kim, Băng tinh, Tinh vân, Hồng ngọc, Hư không, Bạc.
  - **Bậc hiếm** (2 kiểu): Cực quang, Tinh dạ.
- Cách tung khi đã ra shiny: **bước 1** tung bậc (hiếm hay thường), **bước 2** chọn đều 1 kiểu trong bậc đó (bậc hiếm: 1 trong 2).
- Tỉ lệ ra bậc hiếm giữ đúng luật ban đầu "mỗi màu hiếm bằng 1/10 một màu thường": trọng số 6 × 1 (thường) và 2 × 0,1 (hiếm) → **bậc hiếm = 0,2 / 6,2 ≈ 3,2%** số shiny. Cấu hình bằng một số `ShinyRareTierPercent` (mặc định 3,2) để chỉnh riêng.

  | Kiểu | `ShinyKind` | Bậc | Tỉ lệ trong số shiny | Tỉ lệ mỗi lần câu được cá | Hệ số giá |
  |---|---|---|---|---|---|
  | Hoàng kim | `Gold` = 1 | thường | ≈ 16,1% | ≈ 1/620 | ×3 |
  | Băng tinh | `Ice` = 3 | thường | ≈ 16,1% | ≈ 1/620 | ×3 |
  | Tinh vân | `Nebula` = 4 | thường | ≈ 16,1% | ≈ 1/620 | ×3 |
  | Hồng ngọc | `Ruby` = 5 | thường | ≈ 16,1% | ≈ 1/620 | ×3 |
  | Hư không | `Onyx` = 6 | thường | ≈ 16,1% | ≈ 1/620 | ×3 |
  | Bạc | `Silver` = 7 | thường | ≈ 16,1% | ≈ 1/620 | ×3 |
  | Cực quang | `Aurora` = 2 | **hiếm** | ≈ 1,6% | ≈ 1/6.250 | ×10 |
  | Tinh dạ | `DarkAurora` = 8 | **hiếm** | ≈ 1,6% | ≈ 1/6.250 | ×10 |

  `None` = 0. Số của enum cố định (lưu trong save), không đổi thứ tự, kiểu mới chỉ thêm số mới.
- Giá bán cá shiny = giá thường (theo cân nặng) × hệ số.
- **Album** ghi lại từng cặp (loài, kiểu shiny) đã câu được. Album (Phần 5 đợt 2) chưa làm → bây giờ chỉ ghi dữ liệu, UI album hiện sau.
- **Đơn hàng cá (Phần 11): KHÔNG lấy cá shiny** để giao đơn (chỉ đếm và trừ cá thường), tránh người chơi lỡ giao mất con hiếm.
- Mọi con số (tỉ lệ, trọng số, hệ số giá) nằm trong data (`FishingTuning` / `FishingDefinition`), chỉnh không cần sửa code.

## 2. Hiển thị (đã chốt qua ảnh mô phỏng)
Một shader dùng chung cho mọi kiểu; mỗi kiểu là một material (bảng màu). Không vẽ sprite riêng.

### Thuật toán (mỗi pixel của sprite 32×32)
1. Trong suốt → bỏ qua. Màu viền `#181425` → giữ nguyên.
2. Độ sáng `L = 0.299R + 0.587G + 0.114B`. `L < 0.16` (con ngươi, nét tối) → giữ nguyên.
3. Pixel có **alpha = 254** (vòng trắng mắt, đánh dấu sẵn trong PNG) → tô trắng `#ffffff`.
4. Chọn tông nền theo `L`: `< 0.42` → tông tối, `< 0.72` → tông giữa, còn lại → tông sáng.
   - Kiểu Cực quang (chế độ `aurora`, chốt 20/11): màu nền tính bằng HSV, chỉ chạy trong dải xanh ngọc → tím → hồng, uốn sóng mềm:
     - `w = 3 · sin(0,35x + 2πt)`, `k = (y + w) / 30 + t`
     - `hue = 0,45 + 0,5 · (0,5 + 0,5 · sin(2πk))` (dải 0,45–0,95), làm tròn về **10 bậc** để giữ chất pixel
     - Saturation / Value theo tông tối / giữa / sáng: `0,50 / 0,38 / 0,18` và `0,78 / 0,97 / 1,00`
   - Kiểu Tinh dạ (chế độ `aurora` + cờ `dark` – cực quang chìm, chốt 20/11): cùng công thức `hue` với Cực quang, nhưng màu cực quang tối và đậm hơn rồi chìm vào thân đen:
     - Màu cực quang: Saturation / Value theo tông tối / giữa / sáng `0,75 / 0,65 / 0,50` và `0,42 / 0,55 / 0,72`
     - Màu cuối = trộn **70% màu cực quang + 30% tông nền đen** (`mixk = 0,7`)
     - Cực quang là "cực quang ngày", Tinh dạ là "cực quang đêm": cùng dải màu, khác độ sáng.
5. Vệt quét: `d = (x + y) − (t × 96 − 16)` với `t` = pha 0..1 (chu kỳ ~1,5 giây). `0 ≤ d < 4` → lấy màu vệt thứ `floor(d)`; ở tông tối thì trộn 40% với màu nền.
   - Kiểu Bạc: thêm dải tối `4 ≤ d < 6` (màu `trail`) ngay sau vệt sáng → cảm giác chrome.
   - Kiểu Tinh vân và Tinh dạ: chấm sao trắng khi `(7x + 13y) mod 23 == 0` và `((x + y + frame) / 3) mod 2 == 0`, chỉ ở tông tối/giữa.
6. `x, y` là **tọa độ pixel nguyên của sprite** (không dùng UV atlas) → dải chạy đúng lưới pixel, giống nhau ở mọi con kể cả khi gom vào Sprite Atlas.

### Bảng màu
| Kiểu | Tông tối / giữa / sáng | Vệt quét (4 màu) | Đốm sáng (cánh) | Thêm |
|---|---|---|---|---|
| Hoàng kim | `b8701a` `ffbe2e` `ffe98a` | `ffd04a` `ffffff` `fffbe0` `ffd04a` | `ffd23a` | |
| Cực quang | màu nền tính theo HSV (mục 4 thuật toán), hue 0,45–0,95, 10 bậc | `ffffff` `ffffff` `e0fff8` `b8f8ff` | `a0fff0` | aurora |
| Băng tinh | `3a8ac8` `8ad8f5` `e4fbff` | `bff4ff` `ffffff` `ffffff` `bff4ff` | `7ae0ff` | |
| Tinh vân | `2a1e5a` `5a3aa8` `9a7ae0` | `ff6ad8` `ffb0f0` `a0f0ff` `6ad8ff` | `ff8ae0` | stars |
| Hồng ngọc | `8a1030` `e0283c` `ff8a6a` | `ff9a3a` `ffe45a` `fff8c0` `ff9a3a` | `ffb03a` | |
| Hư không | `1c1a24` `34303f` `56526a` | `5a5a78` `d8d8f0` `ffffff` `7a7a9a` | `c8c8e8` | |
| Bạc | `6e778c` `b8c2d2` `f0f4fa` | `ffffff` `ffffff` `dce6ff` `a0aec8` | `dce6ff` | trail `4e5670` |
| Tinh dạ | tông nền `16141e` `2a2638` `46405c` + màu cực quang HSV (mục 4 thuật toán) | `ff6ad8` `ffb0f0` `a0f0ff` `6ad8ff` | `a0fff0` | aurora dark (`mixk` 0,7) + stars |

### Đốm lấp lánh
- 3 dấu cộng mỗi con, vị trí ngẫu nhiên quanh hình (được tràn ra ngoài ô 32×32), lệch pha nhau.
- Mỗi đốm sống 6 khung (~0,55 giây): cánh dài 1 → 2 → 3 → 2 → 1 → tắt; tâm và phần trong trắng, đầu cánh màu "đốm sáng" của kiểu.
- Là object riêng đè lên icon, không vẽ vào sprite. To và nháy nhanh hơn đốm tĩnh của cá legendary để phân biệt.

## 3. Kiến trúc

### Logic (`VeQue.Logic`) 🔍
- `ShinyKind` (enum, số cố định như bảng mục 1).
- `FishingTuning`: `ShinyChancePer10000 = 100` (1/100), `ShinyRareTierPercent = 3,2`, danh sách kiểu bậc thường và bậc hiếm, hệ số giá theo bậc (thường ×3, hiếm ×10).
- `FishingSession`: tung shiny **ngay lúc chọn ra cá**, dùng random của phiên (có seed): ra shiny → tung bậc → chọn đều 1 kiểu trong bậc → `session.Shiny`. View biết trước để cá bay lên và bảng kết quả hiển thị đúng.
- `CaughtFish.Shiny` (`ShinyKind`).
- `FishingMath.FishPrice(fish, grams, tuning, shiny)`: nhân hệ số.
- `FishingState.ShinyDex`: tập cặp (fishId, kind) + thời điểm câu được lần đầu. `Farm.CompleteFishing` ghi vào khi ra shiny (không ghi trùng). Phần 5 (Album) sẽ đọc từ đây.
- Đơn cá (Phần 11): hàm đếm và trừ cá cho đơn **bỏ qua** cá có `Shiny != None`.

### Save 🔍
- `SaveFish.shiny` (int), `SaveFishing.shinyDex` (mảng `{fishId, kind, firstCaughtAt}`).
- `CurrentVersion` 9 → 10, `MinSupportedVersion` 9 → 10 (Phần 1 đã dùng 9) (theo chính sách đợt 2: trước thử kín không chuyển save, save cũ bỏ).

### View (`VeQue.View`)
- Shader `VeQue/ShinyIcon`: dựa trên `UI/Default` (giữ stencil, `_ClipRect` cho `RectMask2D`), dùng được cho cả `Image` lẫn `SpriteRenderer`. Mọi kiểu bằng tham số (màu, cờ aurora / dark / stars / trail dạng float, `mixk`), **không dùng keyword** để không sinh biến thể. Cần truyền khung sprite (rect pixel trong atlas) để tính `x, y` pixel nguyên.
- Thời gian: biến global `_ShinyTime` do một script cập nhật từ `Time.unscaledTime` → vẫn chạy khi game tạm dừng.
- `ShinyPalette` (ScriptableObject): 8 dòng `{kind, tên hiển thị, bậc, material, màu đốm}`. Tham chiếu từ `FishingDefinition` để shader chắc chắn vào build.
- `ShinySparkles`: component đốm sáng (bản UI và bản world), tự chạy bằng code, không dùng Animator.
- Gắn vào: `CaughtFishFlyer` (material + đốm), `CatchResultPanel` (dòng "Đột biến: <tên kiểu>!"), `FishCell` / `BasketView` (material + đốm + giá đã nhân hệ số).

### Editor
- Menu tạo 8 material + asset `ShinyPalette` từ bảng màu mục 2.
- `FishingDataGenerator`: đặt giá trị mặc định cho tuning shiny.

### Art
- Xuất lại 20 PNG cá (`VeQue-art\fish_hd\gen_fish_hd.py`): pixel vòng trắng mắt có **alpha 254** (nhìn như 255). Chép đè `Assets/_Game/Art/Fishing/fish/*.png`, giữ nguyên `.meta` (Compression None nên alpha giữ đúng).

## 4. Thứ tự làm và Kiểm tra
1. **Logic + save + test** (~1 ngày). Test EditMode:
   - 100.000 lần tung có seed: tỉ lệ shiny ≈ 1% (sai số cho phép ±0,15%).
   - Bậc hiếm ≈ 3,2% số shiny; trong bậc hiếm Cực quang và Tinh dạ mỗi kiểu ≈ 50%; 6 kiểu thường đều nhau.
   - Giá ×3 (bậc thường) / ×10 (bậc hiếm) đúng.
   - Save ghi → nạp lại giữ `shiny` và `shinyDex`.
   - `ShinyDex` không ghi trùng; đơn cá không trừ cá shiny.
2. **Shader + 8 material + ShinyPalette** (~0,75 ngày). Kiểm tra: icon trong giỏ cá có cuộn (RectMask2D) không tràn; dải chạy đúng pixel khi icon nằm trong Sprite Atlas; trắng mắt giữ đúng ở cả 20 con.
3. **Xuất lại PNG đánh dấu mắt** (~0,25 ngày).
4. **ShinySparkles + gắn UI** (~1 ngày). Kiểm tra: cá bay, bảng kết quả, ô giỏ cá đều đổi đúng kiểu; giá hiển thị đúng; tạm dừng game vẫn thấy dải chạy.
5. **Build WebGL**, so Build Report (mốc 10,8 MB). Thử trên iPhone (Safari + trình duyệt nhúng FB/TikTok) và Android: shader chạy, không tụt khung hình khi giỏ có nhiều cá shiny.

**Tổng công:** ~3 ngày.

## 5. Vị trí trong lịch (chốt 20/11)
- **Phần 18** trong `Docs/ke-hoach-dot-2.md`, làm **ngay sau Phần 6 – Kéo cá** (thứ tự 4, xong dự kiến 28/11). Các phần sau lùi khoảng 3 ngày; build xong ~06/01, thử kín rút còn 06–10/01.
