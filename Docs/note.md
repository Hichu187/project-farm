# Về Quê – Note tiến độ

Bản chụp lúc lưu: 21/11/2026 (tối). Nhánh `setup/phan-0-camera`, `origin` có tới `3276bb3`; commit mới nhất `dc35df7` (chưa push). **Chưa commit:** 390 mục trong `git status` (việc 13–14/11 + Đợt 2 ngày 20–21/11).

Đợt 2 (20/11/2026 → phát hành 15/01/2027): thứ tự làm và Kiểm tra ở `Docs/ke-hoach-dot-2.md`, thiết kế + quyết định ở `Docs/thiet-ke-dot-2.md`, cá shiny ở `Docs/thiet-ke-shiny-ca.md` (bản sao cả ba ở cuối file này). Các file này thắng `MVP-Plan.md` khi khác nhau.

## Tóm tắt

| Thứ tự | Phần | Trạng thái | Còn thiếu |
|---|---|---|---|
| 1 | Phần 4 – Bộ cây mới | Đã viết code (Test Runner ổn) | Build + Build Report |
| — | Thêm 7 loài cá (20 loài) | Đã viết code | — |
| 2 | Phần 1 – Sửa cần | Đã viết code (Test Runner ổn) | — |
| 3 | Phần 6 – Kéo cá | Đã viết code; vuốt tạm tắt | Thử iPhone / FB / TikTok khi build |
| 4 | Phần 18 – Cá shiny | **XONG** Bước 1–4; Bước 5 build gộp vào Phần 14 | — |
| 5 | Phần 11 – Đơn hàng có thoại + đơn cá | Thứ tự 1–2 **XONG**; thứ tự 3 UI **chờ kiểm tra** | Tạo UI + Play thẻ đơn / bong bóng; chân dung 16×16 (người dùng vẽ) |
| 6 | Phần 2 – GameCalendar + âm lịch | Đã viết code; dòng âm lịch HUD **đã bỏ** (21/11) | Test Runner |
| 7 | Phần 13 – Ngày đêm | Đã viết code + **đổi sang giờ game 30 phút/ngày** (21/11) | Menu Ngày đêm, Tạo UI, Test Runner, đèn dầu + đom đóm, chỉnh màu |
| 8 | Phần 3 – Hộp thư | **Đã viết code, chờ kiểm tra** | Tạo data mặc định, Tạo UI, Test Runner, Play nhận thư Mẹ |
| 9 → 19 | Còn lại | Chưa làm | "Lịch tổng" |

Save hiện tại: `version` **13** (11 đơn có thoại, 12 GameCalendar, 13 hộp thư). Biên dịch Logic / View / Editor / Tests ngoài Unity: không lỗi.

## Quyết định quan trọng phiên 21/11/2026
1. **Hệ thống giờ (chốt sau đánh giá):**
   - **Giờ trong game** chỉ để hiển thị: 1 ngày game = 30 phút thật (chỉnh `DayNightProfile.DayLengthMinutes`, có lệch pha). Màu trời, đèn, đom đóm, đồng hồ HUD `hh:mm` + icon mặt trời (6–18h game) / mặt trăng (icon pixel sinh bằng code, có ô thay art).
   - **Mọi logic dùng giờ thật** (`GameCalendar`, UTC+7): giờ ra cá, âm lịch, sự kiện, điểm danh, đơn/ngày.
   - Giờ ra cá **không hiển thị**: album không ghi khung giờ, gợi ý không nhắc giờ; bỏ dòng "Bắt lúc <khung giờ>" ở chi tiết giỏ cá (vẫn lưu `CaughtFish.Slot`).
   - **Không hiện âm lịch** trên HUD (người chơi tự xem lịch ngoài); âm lịch chỉ dùng cho logic sự kiện.
   - Rằm → **"Ngày trăng tròn"**: cả ngày Rằm tăng tỉ lệ cá hiếm.
   - Giao thừa (12b): ép trời game tối (`DayNightController.ForcedHour`).
   - **Mò cua / soi đèn: PENDING** – chưa chốt theo ngày–đêm game hay khung giờ thật (ghi ở Phần 15–16).
2. Ngày đêm: một quad nhân màu `VeQue/WorldLight` (sortingOrder 280) thay cho đổi shader từng material; cài đặt Đầy đủ / Nhẹ / Tắt bằng **chạm đồng hồ**.
3. Album (Phần 5): lưới 20 ô, mốc 5 / 10 / 15 / 20 loài; mốc cá shiny để sau.
4. Hộp thư: nút Thư cột trái HUD dưới Đi câu; thư chào mừng của Mẹ (50 xu + 5 hạt rau muống); bỏ `claimedIds` (dùng cờ `Claimed`, `SentIds` chặn gửi lại).

## Việc đang chờ người dùng (trong Unity)
1. **Tạo data mặc định** (gán bảng âm lịch + build_mails) → **Ngày đêm: tạo trong scene** → **Tạo UI** → Ctrl+S → Test Runner **Run All** (save cũ báo không dùng được – version 13).
2. Phần 11 thứ tự 3: thẻ đơn (khung tròn + chữ cái, câu thoại có dấu, ≤ 2 dòng ở 375 pt), bong bóng cảm ơn, đơn cá.
3. Phần 13: đặt `World Light` ở cửa sổ nhà, kéo `Fireflies`; đồng hồ chạy giờ game, icon đổi 6h / 18h; Tua thời gian chỉnh màu `Data/DayNightProfile.asset`.
4. Phần 3: nút Thư chấm đỏ → thư của Mẹ → Nhận → dừng / Play lại không nhận lại; thêm thư test vào `build_mails.json`.
5. Sửa thoại nháp `Data/Orders/order_lines.csv` nếu muốn; vẽ chân dung 16×16 cho 5 người.
6. Build lại, so Build Report (mốc 10,8 MB). Cho commit khi muốn.

## Việc tiếp theo
1. Kiểm tra xong các mục trên.
2. Thứ tự 9: **Phần 5 – Album cá + câu rác 🔍** (đưa kiến trúc để duyệt; nhớ: không hiện khung giờ, mốc 5/10/15/20, quà mốc gửi qua hộp thư, đơn cá chỉ đặt loài đã có trong album).
3. Sau đó: Phần 10 Điểm danh (quà qua hộp thư), Phần 7 Bộ máy sự kiện, Phần 9 Giá chợ, Phần 8 Mùng 1 / Rằm (Ngày trăng tròn), 12a Tết…

## Chưa quyết / ghi chú
- Mò cua / soi đèn theo ngày–đêm game hay giờ thật: pending (chốt trước Phần 15–16).
- Cây sự kiện: thời gian tạm 4 tiếng; còn chốt sản lượng, giá (trước Phần 8).
- Cân lại các cặp cây trùng thời gian / lãi: để sau.
- Vuốt thay nút kéo cá: tạm tắt.
- Tên cá "Cá lóc" là tên miền Nam (Bắc: cá quả) – chờ người dùng.
- Icon đồng xu cho quà xu chưa có (hiện chữ "50 xu").
- `WebBridge.GetUtcOffsetSeconds` + hàm jslib còn nhưng không dùng.
- Câu cá: các con số chỉnh sau khi thử máy thật. FatPix thiếu `·` `→` `…`. Ghi công: FatPix (NgnDang), Sunnyside World.
- Gỡ package không dùng; kiểm tra giấy phép Sunnyside; `ComingSoonArea` chưa tạo.

---


# Bản sao `Docs/ke-hoach-dot-2.md`

# Về Quê – Kế hoạch triển khai đợt 2 (20/11/2026 → 15/01/2027) và đợt 3 (sau Tết)

Thiết kế chi tiết và các quyết định đã chốt nằm ở `thiet-ke-dot-2.md` (cùng thư mục). Tài liệu này chỉ nói **làm theo thứ tự nào, mỗi bước kiểm tra ra sao**.

Mỗi bước nhỏ, chạy và kiểm tra được trước khi sang bước sau. Bước đánh dấu 🔍 thì đưa kiến trúc chi tiết để duyệt trước khi code.

## Nguyên tắc chung
- Logic mới đặt trong `VeQue.Logic` (C# thuần, không tham chiếu engine), kèm unit test EditMode. ScriptableObject nằm ở `VeQue.View` và được chuyển sang kiểu thuần lúc khởi động (giống cách đã làm với cây và khu).
- Mọi thứ phụ thuộc thời gian đều đọc từ `IClock` / `GameCalendar`, không gọi `DateTime.Now` trực tiếp.
- **Save (chốt 20/11):** hiện chưa có người chơi thật nên **trước bản thử kín 06/01 không viết migration**. Phần nào đổi cấu trúc save thì chỉ tăng `version`; save hoặc mã sao lưu có `version` cũ thì báo "Bản lưu cũ không còn dùng được" và tạo save mới, không lỗi (làm ở Phần 4). **Từ bản thử kín trở đi** mỗi lần đổi save phải có migration kèm test: nạp JSON bản cũ → ra state mới đúng.
- Data cần sửa thường xuyên (thoại, sự kiện, thư, bảng âm lịch) để dạng JSON/CSV trong `Assets/_Game/Data/`, không viết cứng trong code.
- **Mọi nội dung Tết phải nằm sẵn trong bản build 15/01** và tự bật theo ngày, vì game không có server. Sau đó vẫn sửa được bằng cách deploy lại bản web, nhưng phải đặt header cache đúng để người chơi tải lại là nhận bản mới.
- Sau mỗi phần: chạy toàn bộ test, build WebGL, xem Build Report để theo dõi dung lượng.

## Lịch tổng (thứ tự ưu tiên chốt 20/11/2026)

Số phần giữ nguyên làm mã định danh; **thứ tự làm** theo bảng dưới. Ngày dự kiến tính với nhịp **6 ngày làm/tuần, 20% thời gian cho art và phần MVP còn dở**.

| Thứ tự | Phần | Công | Cộng dồn | Xong dự kiến |
|---|---|---|---|---|
| 1 | Phần 4 – Bộ cây mới | 0,5 | 0,5 | 21/11 |
| 2 | Phần 1 – Sửa cần | 0,5 | 1 | 22/11 |
| 3 | Phần 6 – Kéo cá | 1 | 2 | 23/11 |
| 4 | **Phần 18 – Cá shiny** (thêm 20/11) | 3 | 5 | 28/11 |
| 5 | Phần 11 – Đơn hàng có thoại + đơn cá | 1,75 | 6,75 | 30/11 |
| 6 | Phần 2 – GameCalendar + âm lịch | 1 | 7,75 | 02/12 |
| 7 | Phần 13 – Ngày đêm | 1,75 | 9,5 | 04/12 |
| 8 | Phần 3 – Hộp thư | 1,25 | 10,75 | 06/12 |
| 9 | Phần 5 – Album cá + câu rác | 2,5 | 13,25 | 10/12 |
| 10 | Phần 10 – Điểm danh | 1 | 14,25 | 11/12 |
| 11 | Phần 7 – Bộ máy sự kiện | 2,5 | 16,75 | 15/12 |
| 12 | Phần 9 – Giá chợ theo ngày | 1,5 | 18,25 | 17/12 |
| 13 | Phần 8 – Mùng 1, Rằm, loa xã | 1 | 19,25 | 19/12 |
| 14 | **Phần 12a – Tết tối thiểu** | 0,5 | 19,75 | 19/12 |
| 15 | Phần 17 – Ô ăn quan | 2,75 | 22,5 | 23/12 |
| 16 | Phần 15 – Mò cua | 3 | 25,5 | 28/12 |
| 17 | Phần 16 – Soi đèn | 1,75 | 27,25 | **30/12** |
| 18 | Phần 12b – Tết đầy đủ (**nếu kịp**) | 1,5 | 28,75 | 01/01 |
| 19 | Phần 14 – Build, thử máy thật (cố định cuối) | 3 | 31,75 | **06/01** |
| — | **Thử kín** (rút ngắn vì thêm Phần 18) | | | 06–10/01 |
| — | Cân bằng lần cuối, sửa lỗi | | | 11–14/01 |
| — | **Phát hành public** | | | **15/01** |
| — | Sự kiện Tết tự chạy (23 tháng Chạp → mùng 10) | | | ~30/01 → 15/02 |

Kiểm tra phụ thuộc: thứ tự trên **không vi phạm phụ thuộc nào** (ví dụ Album cần Hộp thư, Giá chợ cần Sự kiện, loa xã (Phần 8) cần Giá chợ, Soi đèn cần Ngày đêm và Mò cua, Nội dung Tết cần Sự kiện, Mùng 1/Rằm, Hộp thư và Bộ cây). Phần 9 và Phần 8 đổi chỗ ngày 20/11 vì loa xã đọc cây hút hàng / rớt giá. Phần 4 (Bộ cây) lên đầu ngày 20/11 vì đơn hàng có thoại (Phần 11) cần id cây mới. Phần 11 tăng từ 1,5 lên 1,75 công vì thêm đơn cá. **Phần 18 (Cá shiny) chèn ngay sau Phần 6 ngày 20/11** vì cùng sửa `FishingSession`; Phần 11 (đơn cá không lấy cá shiny) và Phần 5 (album đọc `ShinyDex`) đều làm sau nó nên không vi phạm phụ thuộc.

### Rủi ro thời gian
- Tổng **31,75 ngày công** trước thử kín (thêm 3 công Phần 18 ngày 20/11). Với nhịp 6 ngày/tuần và 20% cho art thì xong khoảng **06/01**, thử kín rút còn 06–10/01. Với nhịp 5 ngày/tuần và 30% cho art thì phần bắt buộc xong khoảng **07/01** và build xong khoảng **17/01**, tức **trễ hạn phát hành**.
- **Mốc kiểm tra 26/12:** theo lịch mới Soi đèn (thứ tự 17) dự kiến xong 30/12, tức **gần như chắc phải cắt Soi đèn** nếu giữ mốc này. Lúc đó phải xong Soi đèn. Nếu chưa xong thì cắt từ dưới lên theo thứ tự:
  1. Soi đèn
  2. Mò cua
  3. Ô ăn quan

  Phần bị cắt chuyển sang bản cập nhật sau Tết. Vì là web nên deploy lại bất kỳ lúc nào cũng được.
- **Nội dung Tết** (chốt 20/11): tách làm hai phần.
  - **12a – Tết tối thiểu, bắt buộc**: thư lì xì mùng 1–3, loa xã chúc Tết, câu đối và cây nêu ở cổng. Phải có trong bản 15/01 dù có trễ.
  - **12b – Tết đầy đủ, nếu kịp**: thả cá chép tiễn ông Táo, chợ Tết, hoa đào, đơn hàng Tết có thoại, thư Tết, pháo hoa đêm 30.
- Thứ tự ưu tiên trên không ép phải làm tuần tự tuyệt đối. Art (cây, rác, NPC, con vật đồng ruộng) nên đặt trước khoảng 1 tuần so với phần code cần nó.

## Phần 1 – Bỏ giảm độ bền tối đa khi sửa cần (0,5 ngày)
1. Sửa luật: sửa cần đưa `durability` về `maxDurability` gốc của bậc cần; bỏ việc trừ 10% độ bền tối đa.
2. Giá sửa = giá cần × 0,3 × (1 − bền/bền tối đa), làm tròn lên, tối thiểu 1. Hệ số 0,3 để trong data.
3. Cảnh báo trên UI khi độ bền ≤ 10%.
4. Sửa các test cũ đụng tới luật sửa cần; thêm test cho giá sửa. Không migration (save cũ bỏ, xem Nguyên tắc chung).

**Kiểm tra:** test xanh; sửa cần nhiều lần, độ bền tối đa không giảm.

## Phần 2 – `GameCalendar` + lịch âm 🔍 (1 ngày)
1. Script ngoài (Python hoặc C# trong Editor) dùng thuật toán Hồ Ngọc Đức, múi giờ +7, sinh `lunar_2026_2040.json`. Mỗi tháng âm gồm: ngày dương bắt đầu, số ngày, tháng, có nhuận không, năm âm.
2. `LunarTable` (Logic): tra ngày dương → (ngày, tháng, nhuận, năm âm).
3. `GameCalendar(IClock)`:
   - `DayKey`: số ngày tính từ 01/01/2026, theo **UTC+7 cố định**.
   - `TimeWindow`: 7 khung giờ, dùng lại định nghĩa khung giờ câu cá hiện có.
   - `Lunar`.
   - `DaySeed(salt)`: hash ổn định, không dùng `string.GetHashCode` vì giá trị này có thể khác giữa các nền tảng.
4. Chống lùi ngày: lưu `lastSeenDayKey`, không cho `DayKey` nhỏ hơn giá trị này. Nối vào cơ chế chống lùi giờ đang có.
   **Chốt 20/11 (phương án A):** khung giờ câu cá và mốc 0 giờ của giới hạn đơn/ngày cũng chuyển sang `GameCalendar` (UTC+7 cố định), bỏ múi giờ máy (`UtcOffsetSeconds`). Bảng âm lịch sinh bằng `Tools/lunar_table.py` (Python, không cần thư viện ngoài).
5. ~~HUD đồng hồ thêm dòng âm lịch nhỏ~~ – **bỏ 21/11/2026**: không hiện âm lịch, người chơi tự xem lịch ngoài đời. Âm lịch chỉ dùng cho logic sự kiện (mùng 1, Rằm, Tết).

**Test:** mùng 1 Tết Bính Ngọ = 17/02/2026; mùng 1 Tết Đinh Mùi = 06/02/2027; một tháng nhuận bất kỳ trong bảng; `DayKey` đúng ở hai bên mốc 17:00 UTC (00:00 giờ VN); `DaySeed` cho cùng kết quả khi chạy lại.
**Kiểm tra:** đặt `FakeClock` vào các ngày trên, `Farm.Calendar.TryGetLunar` ra đúng ngày âm (test).

## Phần 3 – Hộp thư 🔍 (1–1,5 ngày)
1. Logic:
   - `Mail { id, sender, title, body, rewards[], createdDayKey, expiresDayKey? }`.
   - `MailboxState { mails[], sentIds (HashSet), claimedIds (HashSet) }`. `sentIds` ghi mọi id đã từng gửi, để thư bị dọn (quá 50 thư, quá hạn) không bị gửi lại.
   - `Reward`: xu / hạt / mồi / vật phẩm, dùng chung cho mọi hệ thống phát quà.
2. `MailService`:
   - `Send(mail)`: bỏ qua nếu id đã có trong `sentIds`; gửi xong thì thêm id vào `sentIds`.
   - `Claim(id)` và `ClaimAll()`: cộng thưởng, đánh dấu đã nhận, rồi **gọi lưu ngay**.
   - `Cleanup()`: giữ tối đa 50 thư; thư đã nhận xóa sau 7 ngày; thư quá hạn thì xóa.
3. **Thư theo build:** `build_mails.json` (id, ngày bắt đầu, ngày kết thúc, nội dung, quà). Lúc khởi động, gửi những thư đang trong hạn mà id chưa có trong `sentIds`.
4. UI: nút thư trên HUD có chấm đỏ báo số thư chưa đọc; danh sách thư; xem chi tiết; "Nhận" và "Nhận tất cả".
5. Save: thêm mailbox, tăng `version` (không migration).

**Test:** nhận một thư hai lần thì chỉ được thưởng một lần; giả lập tải lại trang ngay sau khi nhận (lưu → nạp lại) không nhận lại được; thư theo build chỉ gửi một lần, kể cả khi thư chưa nhận đã bị cleanup xóa; cleanup đúng hạn.
**Kiểm tra:** thêm một thư test vào `build_mails.json`, chạy game thấy thư, nhận quà, tải lại trang không thấy thư đó nữa.

## Phần 4 – Bộ cây mới (0,5 ngày data, art làm song song)
1. Thay bộ 25 cây cũ bằng **25 cây thuần Việt** ở mục 1 `thiet-ke-dot-2.md` (22 cây thường + 3 cây sự kiện, tên giọng Bắc): sửa bảng `Crops` trong `DefaultDataGenerator` (id, tên, phút, sản lượng, cấp mở), xóa asset cây cũ không dùng, chạy lại menu tạo data. Bảng là bản nháp; thời gian, sản lượng và cấp mở do bạn cân đối.
2. Chưa có art thì dùng sprite tạm (Sunnyside gần giống hoặc một bộ chung). Người dùng vẽ 25 cây sau.
3. Thêm cờ `eventOnly` cho hoa đào, hoa cúc, hướng dương. Cờ này làm cây không hiện trong cửa hàng lúc thường; sự kiện mở cây bằng hiệu ứng `UnlockCrop`. Thời gian lớn và giá của 3 cây này chốt sau (xem `note.md`).
4. Gắn nhóm giá (rau lá / quả / củ hạt) cho từng cây theo bảng, để Phần 9 dùng.
5. Cập nhật trang mô phỏng kinh tế với bộ cây mới; chỉnh tổng giá Khu 3 nếu Khu 3 xong sớm hơn ngày 10.
6. **Bỏ save cũ, không migration** (chốt 20/11): tăng `version`. Khi nạp save hoặc nhập mã sao lưu có `version` thấp hơn bản hiện tại → báo "Bản lưu cũ không còn dùng được" rồi tạo save mới, không lỗi, không treo. Cơ chế này dùng chung cho mọi phần đổi save trước bản thử kín (06/01). Thêm test: JSON `version` cũ → ra save mới; mã sao lưu cũ → bị từ chối có thông báo.
7. Sửa test đụng tới id / số cây cũ (`rescueCrop` vẫn là rau muống).

**Kiểm tra:** thời gian chín đúng thứ tự thực tế; cấp nào có cây mới thì mở đúng cấp đó; cây `eventOnly` không hiện trong cửa hàng; mở game bằng save cũ thấy thông báo rồi vào save mới, chơi từ đầu không lỗi.

## Phần 5 – Album cá + câu rác 🔍 (2,5 ngày)
**5a. Album**
1. `FishDexState { speciesId → {count, maxWeightG, firstCaughtDayKey} }`, cập nhật mỗi lần câu được cá.
2. Ô chưa câu được thì hiện bóng đen kèm gợi ý mờ. Mồi ưa thích chỉ hiện sau khi đã câu được loài đó. **Không hiện khung giờ và gợi ý không nhắc giờ** (chốt 21/11: giờ ra cá chỉ dev / GD biết).
3. Mốc thưởng (**5 / 10 / 15 / 20 loài** – chốt 20/11 vì đã có 20 loài; đủ mỗi bậc độ hiếm; mốc cá shiny: để sau) gửi qua **hộp thư**, id thư cố định để không nhận trùng.
4. Nối dữ liệu này vào "Loài mới!" và "Kỷ lục mới!" trên bảng kết quả câu.

**5b. Câu rác**
1. `JunkDef { id, name, sellPrice, flavorText, weight }`: 6 món rác thường và hũ tiền xu cổ (hiếm).
2. `BaitDef.junkChance`. Hệ số theo kiểu ném: Thường ×1, Tốt ×0,6, Perfect ×0,3.
3. Lúc cá cắn, quay xem ra rác hay ra cá. Rác thì kéo 2–3 mũi tên, **mất mồi**, chiếm ô giỏ cá.
4. Bán rác cho cô đồng nát; album có thêm tab "Đồ vớt được"; có nút bật/tắt "Tự bán rác".
5. Save: thêm dex, tăng `version` (không migration).
6. Đơn hàng cá (Phần 11): từ đây chỉ đặt loài đã có trong dex.

**Test:** mồi `junkChance = 0` không bao giờ ra rác (chạy 10.000 lần quay); tỉ lệ thực tế khớp cấu hình trong sai số; dex cập nhật đúng kỷ lục; thưởng mốc chỉ gửi một lần.

## Phần 6 – Kéo cá (giữ luật reset) (1 ngày)
1. Nhận phím ở **PointerDown**, không chờ Click. Chỉ nhận ngón chạm đầu tiên; chặn bấm đôi trong khoảng 60 ms.
2. Bấm sai: dây giật ngược 0,3 giây, mũi tên sai rung đỏ, các mũi đã đúng tắt dần → nhập lại từ đầu (giữ luật cũ).
3. Âm thanh cao dần theo từng mũi tên đúng; bóng cá lao theo hướng của mũi tên tiếp theo.
4. Nút chữ thập ở góc dưới phải, mỗi nút ≥ 48 pt; có tùy chọn đổi sang tay trái.
5. Thời gian kéo tối thiểu 0,45 giây mỗi mũi tên (tham số trong data). `navigator.vibrate` chỉ dùng khi trình duyệt hỗ trợ.

**Kiểm tra:** chơi trên iPhone (Safari) và trong trình duyệt nhúng Facebook/TikTok: không bị trễ hay nhận đôi, chuỗi 10 mũi tên bị reset ở mũi thứ 9 vẫn còn kịp làm lại.

## Phần 7 – Bộ máy sự kiện (brick) 🔍 (2–3 ngày)
1. `EventDef { id, title, loaText, priority, weight, cooldownDays, minLevel, trigger, windows, effects[] }`.
2. Trigger: `FixedSolar(dd/mm)`, `FixedLunar(dd/mm, lengthDays)`, `Random`.
3. Hiệu ứng (`IEffect`): `CropPriceMult`, `FishWeightMult` (tỉ lệ cắn), `FishSizeMult` (cân nặng), `GrowTimeMultOnPlant`, `AutoWaterOnPlant`, `YieldBonus` (sản lượng, chốt lúc gieo), `OrderBonus` (hệ số xu và hệ số XP riêng), `ExtraOrder`, `UnlockCrop` (cây `eventOnly`), `MailReward`, `Overlay`. Phần 15 thêm `FieldBonus`.
4. `EventService.GetActive(dayKey, timeWindow)`:
   - Thứ tự ưu tiên: lịch dương cố định / lịch âm cố định → mùng 1 / Rằm → ngẫu nhiên.
   - Phần ngẫu nhiên bốc theo `DaySeed`, loại sự kiện còn trong cooldown bằng cách **tính lại các ngày trước**, không lưu gì thêm.
5. Các hệ thống tự gọi vào: `GetPriceMult`, `GetFishWeightMult`, `GetGrowMultAtPlant`… Hiệu ứng tăng tốc cây **chỉ chốt lúc gieo**.
6. `MailReward` gửi một lần cho mỗi (eventId, dayKey).
7. Nhập 8 sự kiện ngẫu nhiên ban đầu vào `events.json`.
8. Công cụ Editor: chọn một ngày, xem ra sự kiện gì; liệt kê 60 ngày tới để kiểm tra phân bố.

**Test:** cùng ngày luôn ra cùng sự kiện; cooldown đúng; sự kiện cố định thắng sự kiện ngẫu nhiên; cây gieo trước khi sự kiện hết không bị đổi `readyAt`; mỗi ngày quà sự kiện chỉ gửi một lần.

## Phần 8 – Mùng 1, Rằm và loa xã (1 ngày)
1. Thêm 2 sự kiện lịch âm: "Chợ đầu tháng" (mùng 1: mở hoa cúc, hướng dương; hoa và nhóm quả giá ×1,5) và "Ngày trăng tròn" (Rằm, **cả ngày** giờ thật – đổi 21/11, trước chỉ khung Tối và Khuya).
2. Loa xã: banner lúc mở game lần đầu trong ngày (không chặn màn hình), đọc tên sự kiện hôm nay, cây hút hàng và cây rớt giá (từ `MarketService`, Phần 9 đã làm trước), ngày âm.
3. Icon sự kiện trên HUD; chạm vào xem chi tiết.
4. Thứ tự cửa sổ khi mở game: tóm tắt lúc vắng mặt → popup điểm danh (đã có từ Phần 10) → loa xã. Loa xã chỉ hiện sau khi popup điểm danh đóng.

**Kiểm tra:** `FakeClock` đặt vào mùng 1 và Rằm: thấy banner đúng, tỉ lệ cá hiếm tăng suốt ngày Rằm.

## Phần 9 – Giá chợ theo ngày, biên độ hẹp (1,5 ngày)
1. `MarketService.GetPrice(cropId, now) = basePrice × dailyMult × intradayMult × eventMult`.
   - `dailyMult`: noise theo `DaySeed`, trong khoảng 0,85–1,15. Mỗi ngày có 1 cây "hút hàng" ×1,4 và 1 cây "rớt giá" ×0,7.
   - `intradayMult`: rau lá +10% lúc Bình minh và Sáng, −10% lúc Khuya; các nhóm khác ×1. Gắn tag `group` cho cây (rau lá / quả / củ hạt). Nhóm của từng cây lấy theo bảng mục 1 thiết kế (đã gắn ở Phần 4).
2. Đơn hàng tính theo **giá gốc**, không theo giá chợ.
3. Màn bán: hiện giá hôm nay, mũi tên ↑↓ so với hôm qua, biểu đồ 7 ngày (tính lại từ seed). "Bán hết" bỏ qua hàng đang cần cho đơn; có nút "Giữ lại" cho từng loại.
4. Các hệ số và biên độ để trong data.

**Test:** trung bình `dailyMult` trên 365 ngày khoảng 1,0; mỗi ngày có đúng 1 cây hút hàng và 1 cây rớt giá; giá không phụ thuộc múi giờ của máy.

## Phần 10 – Điểm danh, quà gửi hộp thư (1 ngày)
1. `CheckInState { cycleDay (1–7), cycleCount, lastClaimDayKey, lastClaimUtc }`.
2. Xét **mỗi lần mở game và mỗi lần quay lại tab** (không chỉ lần đầu trong ngày, để người nhận lúc 23h vẫn nhận được trong ngày hôm sau khi đủ 18 giờ). Nếu đủ điều kiện (`DayKey` > `lastClaimDayKey` **và** đã qua ≥ 18 giờ thật kể từ lần nhận trước):
   - gửi thư "Điểm danh ngày N/7" vào hộp thư;
   - tăng `cycleDay`; đủ 7 thì quay về 1 và tăng `cycleCount`.
3. Lỡ ngày không mất chuỗi. Quà ngày 7 lớn hơn; quà có thể tăng theo `cycleCount`.
4. **Popup điểm danh** (cửa sổ nhỏ) hiện lịch 7 ô, ô hôm nay sáng lên, nút "Mở hộp thư" để nhận quà. Thứ tự khi mở game: tóm tắt lúc vắng mặt → popup điểm danh → loa xã. Lịch 7 ô cũng xem lại được trong màn hộp thư.

**Test:** bỏ 3 ngày, quay lại vẫn là ngày tiếp theo của chuỗi; chỉnh giờ tiến 2 ngày liền chỉ nhận được 1 lần; nhận lúc 23h, mở lại 7h hôm sau chưa được, 17h cùng ngày thì được; thư điểm danh không bị trùng id.

## Phần 11 – Đơn hàng có thoại + đơn cá (1,75 ngày + viết thoại)
1. `npcs.json` (6–8 người, tên và thoại **giọng Bắc** theo mục 2 thiết kế; bỏ danh sách tên cũ Bà Tư, Chú Ba… trong `DefaultDataGenerator`) và `order_lines.csv` (npcId, loại câu, text, tag cây), `dishes.json` (cây → tên món).
2. Mỗi đơn gắn một NPC (theo trọng số và cây người đó hay đặt). Câu thoại chọn theo seed của đơn, điền vào chỗ trống {title} {qty} {crop} {dish}.
3. Giao xong: bong bóng câu cảm ơn 2 giây. Đơn khẩn dùng câu riêng.
4. Không lặp lại câu vừa dùng của cùng một người.
5. Chân dung 16×16 cho từng NPC; chưa vẽ thì dùng khung tròn có chữ cái đầu.
6. **Đơn hàng cá** (chốt 20/11): đơn có thêm loại `Fish` (loài + số con, lấy từ giỏ cá). Thưởng tính theo giá gốc của cá với cùng hệ số như đơn nông sản. Chỉ ra khi người chơi đã mở vùng câu; chỉ đặt loài độ hiếm thấp, câu được ở ít nhất 3 khung giờ (để không kẹt đơn). Từ Phần 5 chỉ đặt loài đã có trong album. Cu Tí hay đặt đơn cá. Giao cá bỏ qua cân nặng. Không cho đơn cá thành đơn khẩn. **Không lấy cá shiny** để giao đơn (Phần 18).
7. Chống kẹt: đơn cá không tính vào điều kiện tặng hạt rau muống; "Bán hết" ở giỏ cá bỏ qua cá đang cần cho đơn.

**Test:** đơn cá chỉ chọn loài hợp lệ; giao đúng loài, đủ số thì nhận thưởng; thiếu cá thì không giao được; đơn cá không bao giờ là đơn khẩn.
**Kiểm tra:** chữ có dấu hiển thị đủ với câu dài nhất (khoảng 70 ký tự) trên màn 375 pt; mở lại game không đổi câu của đơn đang có.

## Phần 12 – Nội dung Tết: 12a bắt buộc (0,5 ngày) + 12b nếu kịp (1,5 ngày)
**12a (thứ tự 14):** mục 3 (lì xì), một phần mục 4 (câu đối, cây nêu), loa xã chúc Tết.
**12b (thứ tự 18):** các mục còn lại.

Toàn bộ dựng trên bộ máy sự kiện (Phần 7), tự bật theo lịch âm:
1. **23–25 tháng Chạp, "Tiễn ông Táo":** cá chép ×3. Bảng kết quả câu có thêm nút "Thả cá chép": không bán, gửi quà vào hộp thư, cộng điểm Tết.
2. **23–30 tháng Chạp, "Chợ Tết":** dưa hấu, hoa đào, lúa ×1,5. Hoa đào (`eventOnly`) mở trong cửa hàng. Đơn Tết có thoại riêng (mâm ngũ quả…).
3. **Mùng 1–3, "Lì xì":** mỗi ngày một thư lì xì; loa xã đọc lời chúc.
4. **Trang trí:** cây nêu và câu đối ở cổng; pháo hoa đêm 30 (particle nhẹ). Tự tắt sau mùng 10.
5. Thư Tết từ mẹ và từ sếp cũ.
6. Thư nhắc **sao lưu mã trước Tết**, kèm quà nhỏ khi người chơi bấm sao lưu.

**Kiểm tra:** dùng công cụ Editor tua `FakeClock` qua từng ngày từ 20 tháng Chạp tới mùng 11: mọi thứ bật và tắt đúng ngày, quà không bị trùng.

## Phần 13 – Ngày đêm (1,5–2 ngày, bắt buộc trước Tết, thứ tự 7) 🔍
Thiết kế ở mục 8 của `thiet-ke-dot-2.md`. Làm ngay sau GameCalendar; `DarknessOverlay` làm luôn ở đây vì Soi đèn sẽ dùng.
**Đã đổi khi làm (20–21/11, người dùng duyệt):** bước 1 thay bằng một quad nhân màu `VeQue/WorldLight` ở sortingOrder 280; bước 2 đọc **giờ trong game** (`GameDayClock`, 1 ngày ≈ 30 phút thật, chỉnh trong `DayNightProfile`), không đọc `GameCalendar.TimeWindow`. Đồng hồ HUD hiện giờ game `hh:mm` + icon mặt trời / mặt trăng; chạm đồng hồ đổi Đầy đủ / Nhẹ / Tắt.
1. Shader sprite và shader tilemap riêng, nhân với `_WorldTint` toàn cục (`Shader.SetGlobalColor`). Gán cho mọi material của thế giới game, **trừ** dấu "chín" và icon trên cây.
2. `DayNightController` (View) đọc `GameCalendar.TimeWindow` và phần trăm đã trôi trong khung giờ, rồi chuyển màu mượt giữa 7 màu khóa.
3. `DarknessOverlay`: quad phủ màn hình nằm dưới UI, độ tối nền cộng tối đa 8 vòng sáng (mảng uniform), tọa độ làm tròn về lưới pixel.
4. Đêm ở nông trại: độ tối tối đa 0,3, vòng sáng ở đèn dầu trong nhà, vài con đom đóm.
5. Cài đặt: Đầy đủ / Nhẹ / Tắt.

**Kiểm tra:** chụp màn hình ở 7 khung giờ, chỉnh màu khóa theo ảnh thật; cây chín vẫn dễ thấy lúc 22h; FPS trên iPhone đời cũ và trong trình duyệt nhúng không giảm rõ.
⏸ **Chờ 12b:** đêm 30 Tết có pháo hoa trên nền tối – **ép trời game tối** quanh giao thừa bằng `DayNightController.ForcedHour` (chốt 21/11; kiểm tra khi làm pháo hoa ở 12b).

## Phần 14 – Build, thử máy thật, phát hành
1. Build WebGL với Brotli và header hosting đúng; **cache:** file build có hash trong tên, còn `index.html` không cache, để deploy lại là người chơi nhận bản mới.
2. Dung lượng nén < 10 MB (hiện 10,8 MB). Xem Build Report, nén âm thanh AAC, bỏ module không dùng.
3. Thử trên: iPhone Safari, Android Chrome, trình duyệt nhúng Facebook và TikTok. Kiểm tra: tải lần đầu, âm thanh sau lần chạm đầu tiên, lưu và nạp, mã sao lưu, bộ nhớ trên iOS, cử chỉ trình duyệt không cướp thao tác kéo.
4. **06/01: thử kín** (5–10 người, rút còn 06–10/01), thu phản hồi về nhịp chơi ngày 1–3 và về phần kéo cá.
5. **15/01: phát hành public.**

---

---

# Phần chơi tay mới (chuyển lên trước Tết, 20/11/2026)

Thiết kế ở mục 13 và 14 của `thiet-ke-dot-2.md`. Ba phần này ban đầu dự định cho Đợt 3, nay đã **chuyển lên trước Tết** (thứ tự 15–17 trong lịch tổng). Nếu trễ mốc 26/12 thì đây là những phần cắt đầu tiên.

## Phần 15 – Mò cua 🔍 (3 ngày)
⏸ **Pending (21/11/2026):** mò cua / soi đèn mở theo ngày–đêm game (30 phút/ngày) hay khung giờ thật – chưa chốt, chốt trước khi làm (xem `thiet-ke-dot-2.md` mục 13).
1. Data: `FieldCreatureDef { id, name, mode: Day|Night, behavior: Still|Sideways|Slippery|Jumper, sellPrice, baitId?, weight }`; 9 loài; dùng lại `JunkDef` cho rác.
2. `FieldGenerator(seed = DaySeed(dayKey, khung giờ), mode, upgrades)` sinh bàn 5×5: con vật, đá, rác, và dấu hiệu (bọt khí, vệt bò).
3. `FieldRules`: mò một ô → kết quả. Cua bò ngang khi mò trượt ở ô kề bên; chạch cần chạm lần 2 trong 0,6 giây; trừ một lần mò.
4. `FieldState` trong save: lượt đang dở và số lượt đã dùng theo khung giờ. Tải lại trang thì tiếp tục đúng lượt đó, không sinh bàn mới.
5. Lối vào: một điểm chạm ở rìa vùng câu cá. Mở khóa sau lần thu hoạch lúa đầu tiên.
6. Con bắt được bỏ vào giỏ riêng của ruộng; bán được; **đổi ra mồi** (tép → mồi tép, nhái → mồi nhái, ốc → mồi ốc).
7. Album tab "Đồng ruộng"; thưởng mốc qua hộp thư. Thêm câu đơn hàng (canh cua, bún ốc) vào bộ thoại.
8. Nâng cấp: găng tay, cào, giỏ (dùng chung khung cửa hàng của cần và mồi).
9. Sự kiện: hiệu ứng `FieldBonus` (thêm lượt, thêm con) cho "Mưa rào" và "Nước lên".
10. Save: thêm `FieldState`, tăng `version` (không migration).

**Test:** cùng seed ra cùng bàn; cua không bò ra khỏi lưới hay chồng lên ô đã mở; mỗi khung giờ chỉ được 1 lượt; tải lại giữa lượt không đổi bàn.
**Kiểm tra:** chơi trên điện thoại, ô bùn đủ lớn để chạm (lưới 5×5 trên màn dọc, mỗi ô ≥ 56 pt).

## Phần 16 – Soi đèn (1,5–2 ngày)
⏸ **Pending (21/11/2026):** xem ghi chú ở Phần 15.
Cần `DarknessOverlay` (Phần 13).
1. Độ tối khoảng 0,9, một vòng sáng đèn pin đi theo ngón tay (kéo một ngón; chặn cử chỉ trình duyệt như ở màn câu cá).
2. Logic sinh danh sách con xuất hiện theo seed: vị trí, thời điểm ra, thời điểm bỏ chạy. Ếch, nhái nằm trong vòng sáng quá khoảng 1,2 giây thì nhảy đi.
3. Chỉ thấy **đôi mắt phản sáng** khi con vật nằm trong vòng sáng; chạm trúng thì bắt được.
4. Pin đèn là giới hạn thời gian (khoảng 45 giây, nâng cấp được). Đêm Rằm thì độ tối giảm và ra nhiều con hơn.
5. Chỉ mở ở khung Tối và Khuya; dùng chung giới hạn 1 lượt mỗi khung giờ và `FieldState` với Mò cua.
6. Save: lượt soi đèn đang dở (seed, thời điểm bắt đầu, con đã bắt) nằm trong `FieldState`; tải lại trang thì tiếp tục, không sinh lượt mới. Nâng cấp đèn pin lưu cùng các nâng cấp ruộng. Đổi cấu trúc save thì tăng `version` (không migration).

**Test:** cùng seed ra cùng danh sách con; con nằm trong vòng sáng quá 1,2 giây thì bỏ chạy đúng thời điểm; hết pin thì kết thúc lượt; đêm Rằm ra nhiều con hơn; mỗi khung giờ chỉ 1 lượt; tải lại giữa lượt không đổi danh sách con.

**Kiểm tra:** kéo đèn mượt trên iPhone và Android, ngón tay không che mất vòng sáng (đặt vòng sáng lệch lên trên ngón khoảng 40 pt); không bị trình duyệt cuộn trang.

## Phần 17 – Ô ăn quan 🔍 (2,5–3 ngày)
1. `OAnQuanState` và `OAnQuanRules.Apply(move)` trả về danh sách bước (rải / ăn / hết lượt / rải lại sỏi khi hết). Cờ `quanNon` mặc định tắt.
2. Unit test cho từng luật bằng các thế cờ dựng sẵn: ăn liên hoàn, gặp ô quan thì dừng, hai ô trống liền nhau thì dừng, hết sỏi phải rải lại, điều kiện kết thúc.
3. `OAnQuanAI`: Dễ (tham ăn nhất trong lượt, có 25% chọn ngẫu nhiên), Vừa (minimax độ sâu 2), Khó (minimax độ sâu 4 + alpha–beta).
4. UI: bàn phấn, chạm chọn ô rồi chọn hướng, hoạt ảnh rải từng viên theo danh sách bước, nút "Tua nhanh".
5. Phần thưởng: 3 ván thắng đầu mỗi ngày gửi quà vào hộp thư; thành tích ghi vào album.
6. Lưu ván đang chơi dở.

**Test:** AI luôn chọn nước hợp lệ; 1.000 ván AI đấu AI không treo, không lỗi; AI Khó tính một nước dưới 50 ms trong build WebGL.

## Phần 18 – Cá shiny (đột biến lấp lánh) 🔍 (3 ngày, thêm 20/11)
Thiết kế, bảng màu, thuật toán shader: `thiet-ke-shiny-ca.md` (cùng thư mục).
1. Logic: `ShinyKind` (8 kiểu, số enum cố định), tuning (tỉ lệ 1/100; 2 bậc: 6 kiểu thường và 2 kiểu hiếm Cực quang + Tinh dạ, bậc hiếm ≈ 3,2% số shiny, trong bậc chọn đều; giá ×3 / ×10). `FishingSession` tung shiny lúc chọn ra cá; `CaughtFish.Shiny`; `FishingState.ShinyDex`. Giá cá nhân hệ số.
2. Save: `SaveFish.shiny`, `SaveFishing.shinyDex`; tăng `version` 8 → 9 (chưa cần migration, theo nguyên tắc chung).
3. Shader `VeQue/ShinyIcon` (dựa trên `UI/Default`, không keyword) + 8 material + `ShinyPalette`; biến global thời gian từ `Time.unscaledTime`.
4. Xuất lại 20 PNG cá, vòng trắng mắt alpha 254 (giữ `.meta`).
5. `ShinySparkles` (3 đốm dấu cộng nhấp nháy); gắn vào cá bay lên, bảng kết quả câu ("Đột biến: …!"), ô giỏ cá.

**Test:** 100.000 lần tung có seed ra shiny ≈ 1% (±0,15%); bậc hiếm ≈ 3,2% số shiny, mỗi kiểu hiếm ≈ 50% trong bậc; giá ×3 / ×10; save ghi → nạp giữ shiny và `ShinyDex`; `ShinyDex` không ghi trùng.

**Kiểm tra:** icon trong giỏ có cuộn không tràn (RectMask2D); dải màu chạy đúng lưới pixel khi nằm trong Sprite Atlas; trắng mắt đúng ở cả 20 con; tạm dừng game dải vẫn chạy; build WebGL so Build Report; thử iPhone (Safari + trình duyệt nhúng FB/TikTok) và Android.

## Việc cần làm ngoài code
- **Art Đợt 2:** 25 cây thuần Việt (22 cây thường + hoa đào, hoa cúc, hướng dương), 7 món rác, 6–8 chân dung NPC, cây nêu, câu đối, bao lì xì, icon hộp thư và icon sự kiện.
- **Art phần chơi tay mới (trước Tết):** ô bùn và bọt khí, 9 con vật đồng ruộng kèm sprite đôi mắt phản sáng, icon 4 món nâng cấp, bàn ô ăn quan, sỏi và viên quan, đèn dầu, đom đóm.
- **Viết chữ:** thoại NPC (khoảng 4 câu yêu cầu cho mỗi người, 3 câu cảm ơn dùng chung, câu khẩn), 8 câu loa xã cho sự kiện ngẫu nhiên, thư cốt truyện, thư Tết, gợi ý album cho 20 loài cá.
- **Âm thanh:** gà gáy, ếch nhái, chuỗi nốt khi kéo cá, pháo Tết. Nhớ giữ dung lượng nhỏ.


---

# Bản sao `Docs/thiet-ke-dot-2.md`

# Về Quê – Thiết kế đợt 2 (19/11/2026, cập nhật 20/11/2026)

Trạng thái: **đã chốt phần lớn (19–20/11)**, xem mục "Đã chốt". Chưa code. Gồm mục 0–14: nền `GameCalendar`, 11 tính năng ban đầu, kế hoạch kịp Tết (12), mò cua + soi đèn (13), ô ăn quan (14). Mỗi mục có: kết luận, thiết kế, bẫy cần tránh, ước lượng công.

---

## 0. Nền chung: `GameCalendar` (làm trước tiên)

Năm tính năng (sự kiện ngày, giá chợ, lịch âm, ngày đêm, điểm danh) cùng cần biết "hôm nay là ngày gì, giờ nào". Gom về một chỗ trong `VeQue.Logic`:

```
GameCalendar(IClock clock)
  DayKey       : int   // số ngày kể từ 1/1/2026 theo giờ VN (UTC+7 cố định)
  TimeWindow   : enum  // Bình minh / Sáng / Trưa / Chiều / Hoàng hôn / Tối / Khuya
  Lunar        : (day, month, leap, year)
  DaySeed(salt): uint  // hash(DayKey, salt) – mọi máy cùng ngày cho ra cùng số
```

- **Dùng UTC+7 cố định, không dùng múi giờ máy.** Nhờ vậy mọi người chơi cùng thấy một sự kiện, một giá chợ, nên chụp màn hình chia sẻ lên TikTok vẫn khớp nhau.
- Chống chỉnh giờ: giữ cách đang có (chặn lùi giờ). Thêm `lastSeenDayKey`, không cho `DayKey` lùi.
- Unit test bằng `FakeClock`.

---

## 1. Bộ cây thuần Việt, thời gian theo thực tế (làm lại 20/11)

**Chốt 20/11:** làm lại bộ **25 cây thuần Việt** (22 cây thường + 3 cây sự kiện), **tên theo giọng Bắc**. Bỏ hẳn bộ 25 cây cũ trong data (giữ rau muống, hành lá, cải xanh, mồng tơi, đậu đũa, cà chua, ớt, khoai lang, dưa hấu; bỏ xà lách, rau dền, củ cải trắng, cà rốt, cà tím, khổ qua, khoai môn, bí đao, gừng, nghệ, dâu tây, dưa lưới; đổi tên dưa leo → dưa chuột, bắp → ngô nếp, đậu phộng → lạc, bí đỏ → bí ngô). Art người dùng vẽ sau; trong lúc chờ thì dùng sprite tạm. Save sẽ được xóa trắng khi đổi bộ cây nên **không cần migration** cho cây.

### Nguyên tắc
1. **Giữ đúng thứ tự** theo thời gian thực tế: cây nào ngoài đời lâu hơn thì trong game không được chín nhanh hơn (bằng nhau thì được).
2. Thời gian trong game lấy theo **các bậc cố định** (2p, 5p, 15p, 20p, 30p, 45p, 1h, 1h30, 2h, 2h30, 3h, 3h30, 4h, 5h, 6h, 8h, 10h, 12h). Không quy đổi tuyến tính vì 20 → 180 ngày chỉ gấp 9 lần, trong khi game cần dải từ 2 phút tới 12 giờ.
3. Giá và XP tính theo công thức trong `DefaultDataGenerator` (T = phút lớn, t = T/2): lãi = 5 × t^0,8; thu/giá hạt = 2 + 1,5 × ln t / ln 120; giá hạt = lãi / (thu/giá hạt − 1); giá bán = (lãi + giá hạt) / sản lượng; XP = t^0,65.
4. **Thứ tự mở khóa không trùng thứ tự thời gian.** Mỗi cấp 1–13 có cây mới (phần lớn 2 cây: 1 cây ngắn cho lúc chơi tay, 1 cây dài cho lúc vắng). Lúa (cây qua đêm) mở sớm ở cấp 4 để lấp khoảng trống ban đêm từ ngày 1–2. Cấp 14–15 dành cho cần và mồi.
5. Sản lượng: cây hái nhiều (rau, quả nhỏ, hạt) cho 2–5; cây một quả to (bầu, su hào, dưa hấu, bí ngô) cho 1, giá bán cao tương ứng.

### Bảng 22 cây thường (bản nháp, số tính bằng công thức)

| # | Cây | Nhóm giá | Ngoài đời (gieo/trồng → thu lứa đầu) | Trong game | Sản lượng | Cấp mở | Giá hạt | Giá bán | Lãi/vụ | XP |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Rau muống | rau lá | ~20–25 ngày | 2p | 2 | 1 | 5 | 5 | 5 | 1 |
| 2 | Hành lá | rau lá | ~25–30 ngày (trồng củ) | 5p | 2 | 2 | 8 | 9 | 10 | 2 |
| 3 | Cải xanh | rau lá | ~30–35 ngày | 15p | 2 | 2 | 15 | 20 | 25 | 4 |
| 4 | Rau đay | rau lá | ~35–40 ngày | 20p | 2 | 4 | 18 | 25 | 32 | 4 |
| 5 | Mồng tơi | rau lá | ~40–45 ngày | 30p | 2 | 3 | 24 | 34 | 44 | 6 |
| 6 | Dưa chuột | quả | ~40–45 ngày | 30p | 3 | 5 | 24 | 23 | 44 | 6 |
| 7 | Mướp | quả | ~45–50 ngày | 45p | 3 | 6 | 31 | 30 | 60 | 8 |
| 8 | Đậu đũa | quả | ~50–55 ngày | 1h | 4 | 3 | 37 | 28 | 76 | 9 |
| 9 | Bầu | quả | ~55–60 ngày | 1h30 | 1 | 7 | 48 | 153 | 105 | 12 |
| 10 | Mướp đắng | quả | ~55–65 ngày | 1h30 | 3 | 9 | 48 | 51 | 105 | 12 |
| 11 | Su hào | củ hạt | ~60–70 ngày | 2h | 1 | 8 | 58 | 190 | 132 | 14 |
| 12 | Ngô nếp | củ hạt | ~65–75 ngày (bắp tươi) | 2h30 | 2 | 6 | 67 | 113 | 158 | 17 |
| 13 | Dưa hấu | quả | ~70–80 ngày | 3h | 1 | 11 | 76 | 259 | 183 | 19 |
| 14 | Đậu xanh | củ hạt | ~70–80 ngày | 3h | 4 | 8 | 76 | 65 | 183 | 19 |
| 15 | Cà pháo | quả | ~80–90 ngày (tính cả ươm) | 3h30 | 5 | 10 | 84 | 58 | 207 | 21 |
| 16 | Cà chua | quả | ~85–95 ngày (tính cả ươm) | 4h | 3 | 5 | 92 | 107 | 230 | 22 |
| 17 | Lạc | củ hạt | ~90–100 ngày | 5h | 5 | 10 | 107 | 76 | 275 | 26 |
| 18 | Ớt | quả | ~95–110 ngày | 6h | 5 | 12 | 121 | 88 | 319 | 29 |
| 19 | Bí ngô | củ hạt | ~100–110 ngày | 6h | 1 | 7 | 121 | 440 | 319 | 29 |
| 20 | Lúa | củ hạt | ~100–115 ngày (giống ngắn ngày) | 8h | 2 | 4 | 148 | 274 | 401 | 35 |
| 21 | Khoai lang | củ hạt | ~110–120 ngày | 10h | 3 | 9 | 172 | 217 | 479 | 41 |
| 22 | Khoai sọ | củ hạt | ~150–180 ngày | 12h | 3 | 13 | 195 | 250 | 555 | 46 |

Mở theo cấp: 1 rau muống · 2 hành lá, cải xanh · 3 mồng tơi, đậu đũa · 4 rau đay, lúa · 5 dưa chuột, cà chua · 6 mướp, ngô nếp · 7 bầu, bí ngô · 8 su hào, đậu xanh · 9 mướp đắng, khoai lang · 10 cà pháo, lạc · 11 dưa hấu · 12 ớt · 13 khoai sọ.

Ghi chú:
- Số ngày ngoài đời là khoảng phổ biến, đủ để giữ thứ tự; nhóm 90–120 ngày ngoài đời khá sát nhau nên thứ tự trong nhóm này hợp lý nhưng không tuyệt đối.
- "Nhóm giá" dùng cho giá chợ trong ngày (mục 6): rau lá đổi theo chợ sáng / khuya, quả và củ hạt không đổi theo giờ.
- Hành lá thay cho "rau thơm" trong đơn hàng (cô Hạnh bán bún).
- Đậu xanh, lúa cho đơn xôi, chè ngày Rằm; cà pháo, dưa chuột cho đơn cà muối, dưa góp; rau đay, mồng tơi cho canh cua (nối với mò cua, mục 13).
- **Chạy lại mô phỏng** sau khi chốt: thu nhập đổi nên cần xem lại tổng giá Khu 3.

### 3 cây sự kiện (`eventOnly`)

| Cây | Sự kiện | Trong game | Ghi chú |
|---|---|---|---|
| Hoa đào | Chợ Tết (23–30 tháng Chạp) | chưa chốt | Bày Tết |
| Hoa cúc | Chợ đầu tháng (mùng 1) | chưa chốt | Hoa cúng đầu tháng |
| Hướng dương | Chợ đầu tháng (mùng 1) | chưa chốt | Art tạm: `sunflower` Sunnyside |

Thời gian lớn, sản lượng, giá và luật sau khi sự kiện hết: **để sau**, xem `note.md` mục "Chưa quyết / ghi chú".

### Art
- Người dùng vẽ cả 25 cây (mỗi cây: `_00` hạt, `_01`–`_04` giai đoạn, `_05` nông sản) theo phong cách Sunnyside.
- Trong lúc chờ: dùng sprite Sunnyside gần giống (kale, cabbage, radish, pumpkin, wheat, sunflower…), cây không có thì dùng tạm một bộ chung.

---

## 2. Đơn hàng có thoại

**Kết luận:** nên làm. Chỉ tốn chữ mà game có hồn hơn hẳn.

### Thiết kế
- **6–8 người làng cố định**, mỗi người có tên, vai, tính cách, cây hay đặt và câu nói riêng. Ví dụ:
  - **Bà Lụa** (hàng xóm, hay kể chuyện ngày xưa): rau đay, mồng tơi, bí ngô.
  - **Chú Thành** (trưởng thôn, nói kiểu loa xã): lúa, ngô nếp, lạc.
  - **Cô Hạnh** (bán bún đầu chợ): rau muống, hành lá, cà chua.
  - **Cu Tí** (trẻ chăn trâu): dưa hấu, ngô nếp, cá (đơn cá, xem dưới).
  - **Bác Cả** (trưởng họ, nhà có giỗ chạp, hay đặt đơn khẩn): đậu xanh, lúa, su hào (xôi, chè, canh).
  - **Cô đồng nát** (mua rác câu được, xem mục 3).
- **Giọng Bắc** (chốt 20/11) cho cả tên người, tên cây và thoại: gọi theo tên hoặc "cả" (không gọi thứ Hai, Ba, Tư… kiểu miền Nam); xưng hô cháu / bác / cô / chú / u / bầm; từ ngữ Bắc (ngô, lạc, dưa chuột, bí ngô, khoai sọ). Danh sách tên đơn hàng cũ trong `DefaultDataGenerator` (Bà Tư, Chú Ba, Cô Năm…) bỏ khi làm Phần 11.
- Art: một chân dung 16×16 cho mỗi người. Nếu chưa vẽ kịp thì tạm dùng chữ cái đầu tên trong khung tròn.

### Dữ liệu
```
NpcDef     { id, displayName, title, preferredCrops[], weight }
LineDef    { npcId, kind: Request|Urgent|Thanks|Busy, text, cropTag? }
DishMap    { cropId → ["canh cua rau muống", "rau muống xào tỏi", ...] }
```
- Câu thoại có chỗ trống: `"{title} cần {qty} {crop} về nấu {dish}, cháu mang qua giúp nhé!"`.
- Chọn câu theo seed = (orderId), để mở lại game không đổi câu.
- **Đơn hàng cá** (chốt 20/11): đơn có thể đòi cá (loài + số con, lấy từ giỏ cá), thưởng theo giá gốc của cá. Chỉ đặt loài độ hiếm thấp, câu được ở nhiều khung giờ; có album rồi thì chỉ đặt loài đã câu được. Không có đơn cá khẩn.
- **Giao xong** hiện câu cảm ơn (bong bóng 2 giây).
- Đơn khẩn dùng câu riêng: "Nhà có giỗ, cháu mang gấp 3 quả bí!".
- Giới hạn khoảng 70 ký tự mỗi câu, hai dòng trên màn dọc. Kiểm tra font pixel đủ dấu với chữ dài.

### Bẫy
- Câu lặp lại nhiều sẽ nhàm. Cần ít nhất **4 câu Request mỗi người** và 3 câu Thanks dùng chung, xoay vòng không lặp liền nhau.
- Lưu thoại trong file JSON/CSV riêng để sửa không cần build lại logic.

**Công:** khoảng 1–1,5 ngày code và 1 buổi viết thoại.

---

## 3. Album cá + câu rác

### Album
- Lưới 20 ô (20 loài, chốt 20/11) (sau này thêm vùng câu thì thêm tab). Loài chưa câu được hiện **bóng đen kèm gợi ý mờ** (21/11: gợi ý **không nhắc giờ** – giờ ra cá chỉ dev / GD biết; ví dụ gợi ý theo mồi, độ sâu, tính cách cá).
- Chi tiết mỗi loài: độ hiếm, độ sâu (21/11: **bỏ khung giờ**), mồi ưa thích (**chỉ hiện sau khi câu được**), số lần câu, **kỷ lục cân nặng**, ngày câu được lần đầu.
- **Mốc thưởng** gửi vào hộp thư (mục 9): 5 / 10 / 15 / 20 loài (chốt 20/11), và đủ mỗi bậc độ hiếm. Mốc cá shiny (dữ liệu `ShinyDex` đã có từ Phần 18): để sau.
- Lưu: `FishDex { speciesId → {count, maxWeightG, firstCaughtDayKey} }`. Tăng `version` của save (trước bản thử kín 02/01 không migration, save cũ bỏ).
- "Kỷ lục mới!" trên bảng kết quả câu đã có sẵn, chỉ cần nối dữ liệu vào.

### Câu rác
- 6 món: **dép tổ ong, nón lá rách, lon bia, chai nhựa, ủng cao su, bao tải**. Thêm 1 món "may mắn" là **hũ tiền xu cổ** (hiếm, bán được nhiều xu).
- **Tỉ lệ ra rác cấu hình theo mồi** (đã chốt): mỗi `BaitDef` có trường `junkChance`. Mồi rẻ dính rác nhiều, mồi xịn ít dần, **mồi cao cấp đặt 0** để không bao giờ dính rác. Gợi ý ban đầu (chỉnh trong data):

  | Mồi | junkChance |
  |---|---|
  | Giun, cám | 12% |
  | Ruồi, mồi tép | 9% |
  | Dế, nhái | 6% |
  | Mồi giả | 3% |
  | Mồi thơm, nhái hơi, thìa vàng | 0% |

- Kiểu ném nhân thêm hệ số: Thường ×1, Tốt ×0,6, Perfect ×0,3. Như vậy mồi 0% vẫn là 0%, còn mồi rẻ ném đẹp thì đỡ dính rác.
- Kéo rác chỉ 2–3 mũi tên, bảng kết quả có câu đùa ("Ai vứt dép ở đây vậy trời!").
- **Trúng rác thì mất mồi** (đã chốt). Mồi xịn lúc này có thêm giá trị rõ ràng: không phí lượt vào rác.
- Bán cho **cô đồng nát** (bán từng món hoặc bán hết). Album có thêm tab "Đồ vớt được".
- Rác chiếm ô giỏ cá. Có nút "Tự bán rác" bật/tắt.

**Công:** album 1,5 ngày; rác 1 ngày (cần vẽ 7 sprite nhỏ).

---

## 4. Sự kiện theo ngày: kịch bản hay ngẫu nhiên kiểu "brick"?

**Kết luận: làm cả hai, chạy chung một bộ máy.** Lịch cố định được ưu tiên, ngày trống thì bốc ngẫu nhiên.

### Thứ tự ưu tiên mỗi ngày
1. **Ngày lễ cố định** (lịch âm/dương): Tết, Rằm tháng Giêng, Trung thu…
2. **Mùng 1 / Rằm hằng tháng** (mục 7).
3. **Ngẫu nhiên theo seed ngày**, bốc từ một kho sự kiện có trọng số.
4. Không có gì thì là ngày thường (loa xã vẫn đọc giá chợ).

### Kiến trúc "brick"
Một sự kiện được ghép từ các viên gạch nhỏ. Mỗi viên là một class C# thuần, cấu hình bằng data:

```
EventDef {
  id, title, loaText, weight, cooldownDays, minLevel,
  trigger : FixedSolar(dd/mm) | FixedLunar(dd/mm) | Random
  window  : AllDay | TimeWindows[...]          // vd. chỉ Tối + Khuya
  effects : IEffect[]                          // các viên gạch
}

IEffect:
  CropPriceMult(cropId|tag, mult)      // thương lái về làng
  FishWeightMult(speciesId|rarity, m)  // tỉ lệ cắn: đêm Rằm cá hiếm cắn nhiều
  FishSizeMult(speciesId|rarity, m)    // cân nặng: nước lên cá to hơn
  GrowTimeMultOnPlant(mult)            // áp dụng LÚC GIEO
  AutoWaterOnPlant()                   // mưa rào
  YieldBonus(minGrowHours, +n)         // ngày được mùa, chốt LÚC GIEO
  OrderBonus(npcId|tag, xuMult, xpMult)// nhà bác Cả có giỗ, hội làng
  ExtraOrder(n)                        // chợ phiên
  UnlockCrop(cropId)                   // mở cây eventOnly (đào, cúc, hướng dương)
  MailReward(rewardDef)                // quà gửi hộp thư
  Overlay(id)                          // mưa, đèn lồng… (tùy chọn)
  FieldBonus(extraRounds, moreCreatures) // mò cua / soi đèn (thêm ở Phần 15)
```

- **Các hệ thống tự hỏi, không chờ đẩy.** `MarketSystem` gọi `EventService.GetPriceMult(crop, now)`; câu cá gọi `GetFishWeightMult`… Vì vậy mọi thứ vẫn đúng theo timestamp khi tab bị ẩn.
- **Bẫy lớn:** hiệu ứng làm cây lớn nhanh (và `YieldBonus`) **chỉ áp dụng lúc gieo** (chốt `readyAt` ngay khi gieo). Nếu tính lại cho cây đang lớn thì sự kiện vừa hết, cây sẽ "chín lùi".
- **Không lặp liền nhau:** vì sự kiện suy ra từ seed ngày nên có thể tính lại hôm qua, hôm kia. Khi bốc, loại các sự kiện còn trong `cooldownDays`. Không cần lưu gì thêm.
- Hiển thị: **khung loa xã** lúc mở game lần đầu trong ngày (banner, không chặn màn hình), và một icon sự kiện nhỏ trên HUD.

### Kho ngẫu nhiên khởi đầu (8 sự kiện, gần như không cần art)

| Sự kiện | Hiệu ứng |
|---|---|
| Mưa rào | Cây gieo hôm nay tự được tưới; trê và lươn ×2 |
| Nắng đẹp | Cây gieo hôm nay lớn nhanh hơn 10% |
| Thương lái về làng | 1 loại cây giá ×1,5 |
| Chợ phiên | Tất cả nông sản +10%; thêm 1 đơn |
| Nước lên | Cá to hơn (cân nặng +20%, `FishSizeMult`) |
| Gió mùa | Cá hiếm ×1,5 vào buổi sáng |
| Hội làng | Đơn hàng thưởng XP ×1,5 (`OrderBonus` xpMult) |
| Ngày được mùa | Sản lượng +1 cho cây ≥ 4h gieo trong ngày (`YieldBonus`) |

**Công:** bộ máy và 8 sự kiện khoảng 2–3 ngày. Cần unit test kỹ (seed, cooldown, ưu tiên).

---

## 5. Kéo cá: cải thiện nhưng **giữ luật sai thì nhập lại từ đầu**

Giữ luật reset. Những thứ khác thì giảm bực bội và tăng cảm giác đang kéo cá:

1. **Nhận phím khi chạm xuống** (pointer down), không đợi nhấc tay (click). Trong trình duyệt nhúng có thể trễ 100–300 ms.
2. **Chặn chạm hai ngón:** chỉ nhận một ngón đầu tiên. Thêm khoảng chờ khoảng 60 ms giữa hai lần bấm để không bị nhận đôi.
3. **Báo sai thật rõ:** dây giật ngược 0,3 giây, mũi tên sai rung đỏ, các mũi tên đã đúng lần lượt tắt từ phải sang trái. Người chơi cần hiểu ngay *vì sao* bị reset.
4. **Âm thanh mỗi lần bấm đúng cao dần** (đô–rê–mi…). Cảm giác như đang kéo dây lên, và người chơi biết mình đang ở bước nào mà không cần nhìn.
5. **Bóng cá chạy theo mũi tên tiếp theo** (cá lao về hướng đó), dây cong theo. Đây chỉ là hình ảnh, không đổi luật.
6. **Chỗ đặt nút:** chữ thập ở góc dưới phải, vừa tầm ngón cái, mỗi nút ≥ 48 pt. Có cài đặt đổi sang bên trái cho người thuận tay trái.
7. **Thời gian công bằng trên điện thoại:** tối thiểu khoảng 0,45 giây cho mỗi mũi tên, cộng thêm thời gian theo độ dài chuỗi. Với chuỗi 10 mũi tên mà bị reset ở mũi thứ 9 thì vẫn còn đủ thời gian làm lại.
8. Rung: `navigator.vibrate` chỉ có trên Android, iOS Safari không hỗ trợ. Để rung là phần thưởng thêm, không bắt buộc.
9. Thử A/B **vuốt thay cho nút** (vuốt ở giữa màn hình, tránh mép dưới để không đụng thanh home của iOS). Nếu vuốt dễ hơn hẳn thì đổi.

**Công:** khoảng 1 ngày, chủ yếu là phần hiển thị và âm thanh.

---

## 6. Giá chợ theo ngày: reset theo ngày hay theo khung giờ?

### So sánh

| | Reset theo ngày | Reset theo khung giờ (7 khung) |
|---|---|---|
| Dễ hiểu | ✅ "Hôm nay bí giá cao" | ❌ Phải nhớ giá đổi mỗi 2–4 tiếng |
| Chia sẻ, bàn tán | ✅ Cả ngày dùng chung một tin | Kém hơn |
| Lý do ghé nhiều giờ | Trung bình | ✅ Cao |
| Công bằng với người có giờ chơi cố định | ✅ | ❌ Người chỉ chơi tối luôn bị giá tối |
| Rủi ro găm hàng | Trung bình | Cao (canh khung giờ đỉnh) |

**Đề xuất: giá gốc đổi theo ngày, cộng một đường cong nhỏ trong ngày theo nhóm cây.**
- Mỗi ngày (00:00 giờ VN): mỗi cây có hệ số giá = noise(seed ngày, cropId) trong khoảng **0,85–1,15**. Thêm 1 cây **"hút hàng" ×1,4** và 1 cây **"rớt giá" ×0,7**, loa xã đọc lên.
- Trong ngày: **chợ sáng** (Bình minh, Sáng) thì rau lá +10%; Khuya thì rau lá −10%. Nhóm quả và củ hạt không đổi theo giờ (nhóm của từng cây xem bảng mục 1). Đúng với ngoài đời và cho thêm lý do ghé buổi sáng mà không phạt ai nhiều.
- Màn bán hiện giá hôm nay, mũi tên ↑↓ so với hôm qua và biểu đồ 7 ngày. Biểu đồ tính lại được từ seed nên không cần lưu.

### Bẫy và luật đi kèm
- **Găm hàng:** nếu kho không giới hạn, người chơi sẽ giữ hàng chờ ngày giá cao. Hai cách:
  - (a) **Độ tươi**: rau lá sau 24h −30% giá, quả 72h, củ/hạt không hỏng. Đúng thực tế, tạo lựa chọn hay, nhưng tốn công hơn.
  - (b) Giữ biên độ hẹp như trên, chấp nhận găm hàng nhẹ.
  - **Đã chốt: làm (b).** Sau này thêm (a) nếu thấy người chơi găm hàng nhiều.
- **Đơn hàng tính theo giá gốc**, không theo giá chợ, để phần thưởng đơn ổn định.
- **Hàng đang cần cho đơn** tự khóa khi bấm "Bán hết", có nút "Giữ lại" cho từng loại.
- Trung bình hệ số giá khoảng 1,0, nên cân bằng trong mô phỏng không đổi nhiều.

**Công:** khoảng 1,5 ngày (logic, UI bán, biểu đồ nhỏ).

---

## 7. Lịch âm: sự kiện mùng 1 và ngày Rằm

### Tính lịch âm
- **Khuyến nghị:** sinh sẵn **bảng 2026–2040** (mỗi tháng âm: ngày dương bắt đầu, có nhuận hay không) bằng script ngoài, dùng thuật toán của Hồ Ngọc Đức với múi giờ +7, rồi lưu thành file data nhỏ (khoảng 3 KB). Logic chỉ cần tra bảng, không có rủi ro tính toán thiên văn lúc chạy.
- Unit test đối chiếu vài mốc đã biết, ví dụ mùng 1 Tết Bính Ngọ = 17/02/2026, Tết Đinh Mùi = 06/02/2027.
- ~~HUD đồng hồ thêm dòng nhỏ~~ – **bỏ 21/11/2026**: không hiện âm lịch trên HUD, người chơi tự xem lịch ngoài đời.

### Nội dung
- **Mùng 1: "Chợ đầu tháng"**: mở trồng hoa cúc, hướng dương (`eventOnly`); hoa và **nhóm quả** giá ×1,5; mấy người làng đặt đơn "mua hoa, quả cúng đầu tháng"; quà nhỏ trong hộp thư.
- **Rằm: "Ngày trăng tròn"** (đổi 21/11 – trước là "Đêm trăng tròn"): **cả ngày Rằm** (giờ thật) cá Quý / Huyền thoại ×2, không giới hạn khung giờ; trăng tròn trên trời mỗi khi trời game tối (nối với mục 8); đơn xôi, chè.
- **Ngày lớn** (sự kiện cố định, ưu tiên cao): Tết Nguyên Đán (06/02/2027, nên làm kịp nếu ra MVP trước Tết), Rằm tháng Giêng, Rằm tháng 7, Trung thu.
- Lời thoại và hình ảnh nên trung tính, ví dụ "mâm cúng", "hoa đầu tháng", tránh đi sâu vào nghi lễ tôn giáo.

**Công:** bảng âm lịch và test nửa ngày; nội dung chạy trên bộ máy sự kiện ở mục 4.

---

## 8. Có nên làm ngày đêm? (xem lại 19/11, sau khi thêm soi đèn)

**Kết luận: CÓ, nên làm.** Lý do bây giờ mạnh hơn lần đánh giá trước: đã có **5 hệ thống chạy theo giờ trong ngày**:
- Cá xuất hiện theo khung giờ.
- Giá rau lá theo chợ sáng.
- Đêm Rằm cá hiếm cắn nhiều.
- Mò cua ban ngày, soi đèn ban đêm.
- Pháo hoa đêm giao thừa.

Không có ngày đêm thì "khung giờ" chỉ là một dòng chữ trên HUD, người chơi không cảm thấy nó. Có ngày đêm thì nhìn trời là biết nên làm gì.

### Có cần ngày đêm thì soi đèn mới chạy được không?
**Không bắt buộc.** Chế độ soi đèn tự làm tối màn hình của nó. Nhưng nếu làm soi đèn mà không có ngày đêm thì 23h ngoài ruộng tối om, về tới nông trại lại sáng như trưa, nhìn rất lệch. Vì vậy **hai phần dùng chung một kỹ thuật** (xem dưới) và nên làm cùng nhau.

### Kỹ thuật: một shader cho cả hai (Built-in RP, WebGL)
1. **Màu trời toàn cảnh:** shader sprite và tilemap riêng, nhân với `_WorldTint` đặt bằng `Shader.SetGlobalColor`. Không thêm lần vẽ nào, pixel vẫn sắc nét. 7 màu khóa theo 7 khung giờ, chuyển màu mượt giữa các khung.
2. **`DarknessOverlay`:** một quad phủ màn hình (nằm dưới UI), có độ tối nền cùng **tối đa 8 vòng sáng** (tâm, bán kính, độ mềm) truyền vào bằng mảng uniform.
   - Ban ngày: độ tối 0, tắt hẳn.
   - Đêm ở nông trại: độ tối khoảng 0,3, có vòng sáng ở đèn dầu trong nhà, đom đóm, đống lửa.
   - Soi đèn ngoài ruộng: độ tối khoảng 0,9, một vòng sáng đèn pin đi theo ngón tay.
3. Không cần RenderTexture, không dùng hệ ánh sáng 2D (nặng, không hợp Built-in RP). Vòng sáng tính theo khoảng cách bên trong shader nên chạy nhẹ trên điện thoại. Làm tròn tọa độ vòng sáng về lưới pixel để rìa sáng không bị mịn lệch style.

### Luật để không hại khả năng đọc
- Đêm ở nông trại **không tối quá 30%**: nhiều người chỉ chơi lúc 22h, cây chín vẫn phải dễ thấy. Dấu "chín" và icon phía trên cây **không bị tint**.
- UI không bị ảnh hưởng.
- Cài đặt có 3 mức: Đầy đủ / Nhẹ / Tắt. Mức "Tắt" vẫn giữ chế độ soi đèn, vì đó là luật chơi chứ không chỉ là hình ảnh.
- ~~Chạy theo **giờ thật**, không tua nhanh.~~ **Đổi 21/11/2026:** màu trời chạy theo **giờ trong game**: 1 ngày game ≈ **30 phút thật**, chỉnh trong data (`DayNightProfile`). Giờ game chỉ dùng cho **hiển thị** (màu trời, đèn, đom đóm, đồng hồ `hh:mm` trên HUD với icon mặt trời / mặt trăng). **Mọi logic theo thời gian dùng giờ thật** (`GameCalendar`, UTC+7): giờ ra cá, âm lịch, sự kiện, điểm danh, giới hạn đơn/ngày, cây lớn. Lý do: ai chơi giờ nào cũng thấy đủ ngày đêm.
- Giờ ra cá **không hiển thị** cho người chơi (chốt 21/11): chỉ dev và game designer biết; người chơi câu nhiều sẽ tự rút ra. Album không ghi khung giờ, gợi ý không nhắc giờ.
- Giao thừa (12b): **ép trời game tối** quanh thời điểm bắn pháo hoa (`DayNightController.ForcedHour`).

### Rủi ro
- Màu ENDESGA bị tint sẽ lệch tông: chọn 7 màu khóa bằng cách chụp màn hình thật rồi chỉnh, đừng chọn theo cảm tính.
- Thời điểm làm: **bắt buộc trước Tết**, thứ tự 6, ngay sau GameCalendar (chốt 20/11). Soi đèn dùng lại `DarknessOverlay`.

**Công:** tint toàn cảnh 0,5 ngày; `DarknessOverlay` 1 ngày; đom đóm, đèn dầu 0,5 ngày.

---

## 9. Hộp thư nhận thưởng

### Công dụng
Gom mọi phần thưởng về một chỗ: mốc album, sự kiện, điểm danh (nếu chọn), **thư cốt truyện** (mẹ ở quê, sếp cũ trên thành phố…), và **thư từ nhà phát triển kèm theo mỗi bản build**.

Thư theo build là cách làm "live ops" không cần server: mỗi bản cập nhật kèm danh sách thư (id, ngày bắt đầu, ngày kết thúc, quà), máy người chơi chỉ phát một lần theo id. Dùng để đền bù khi có lỗi, tặng quà dịp lễ, hoặc báo tính năng mới.

### Dữ liệu
```
Mail { id, sender, title, body, rewards[], createdDayKey, expiresDayKey?, claimed }
Save: mailbox[] + sentMailIds (HashSet, chống gửi lại thư đã bị dọn) + claimedMailIds (HashSet, chống nhận trùng)
```
- **Nhận quà là giao dịch nguyên vẹn:** cộng thưởng, đánh dấu đã nhận, **lưu ngay**. Nếu không, người chơi tải lại trang giữa chừng có thể nhận được hai lần.
- Giữ tối đa 50 thư; thư đã nhận tự xóa sau 7 ngày. Có nút "Nhận tất cả".
- HUD có chấm đỏ báo số thư chưa đọc.

**Công:** khoảng 1–1,5 ngày.

---

## 10. Điểm danh hằng ngày

- **Chu kỳ 7 ngày, cộng dồn**, lỡ ngày không mất chuỗi: "Ngày 4/7" vẫn là ngày 4 dù hôm qua bỏ chơi. Ngày 7 thưởng lớn (mồi hiếm, hạt cây cao cấp, xu), sau đó lặp lại chu kỳ. Có thể nâng quà theo từng vòng.
- Điều kiện nhận: `DayKey` hôm nay > `lastClaimDayKey`. Xét lại **mỗi lần mở game và mỗi lần quay lại tab**, không chỉ lần đầu trong ngày.
- **Chống chỉnh giờ tiến:** thêm điều kiện đã qua ít nhất khoảng 18 giờ thật kể từ lần nhận trước (tính theo timestamp đã lưu). Kèm việc chặn lùi giờ sẵn có, chỉnh giờ để nhận nhiều ngày một lúc sẽ không còn lợi.
- **Popup điểm danh** (chốt 20/11): cửa sổ nhỏ hiện lịch 7 ô và nút mở hộp thư; quà vẫn nằm trong hộp thư.
- **Thứ tự cửa sổ khi mở game** (tránh hiện ồ ạt): tóm tắt lúc vắng mặt → popup điểm danh → loa xã (banner, không chặn màn hình).

**Công:** khoảng 1 ngày.

---

## 11. Bỏ việc sửa cần làm giảm độ bền tối đa

- Sửa cần **đưa độ bền về đầy**, độ bền tối đa giữ nguyên.
- Giá sửa = giá cần × 0,3 × (1 − độ bền hiện tại / độ bền tối đa), tối thiểu 1 xu. Sửa sớm thì rẻ, nên không phạt ai.
- Cảnh báo khi độ bền còn 10%.
- Save: không migration (chưa có người chơi thật, save cũ bỏ – chốt 20/11).
- Sửa unit test liên quan (khoảng 450 test hiện có, chắc chỉ vài test đụng tới).

**Công:** nửa ngày.

---

## Thứ tự làm

Bảng thứ tự cũ (19/11) đã bỏ. Thứ tự làm, công và ngày dự kiến: xem bảng "Lịch tổng" trong `ke-hoach-dot-2.md` (tổng 28,75 ngày công trước thử kín).

## Đã chốt (19/11/2026)
1. **Bộ cây và thứ tự mở khóa:** bạn tự cân đối sau. Bảng ở mục 1 chỉ là bản nháp. (20/11: làm lại 25 cây thuần Việt, tên giọng Bắc, có cột sản lượng; save xóa trắng, không migration cây.)
2. **Câu rác:** trúng rác thì **mất mồi**. Tỉ lệ ra rác cấu hình theo từng mồi (`junkChance`), mồi xịn giảm dần tới 0 (xem mục 3).
3. **Nhắm ra MVP kịp Tết Đinh Mùi (mùng 1 = 06/02/2027)**, có sự kiện Tết (xem mục 12).

4. **Giá chợ:** làm (b) **biên độ hẹp**, chưa làm độ tươi.
5. **Điểm danh:** quà **gửi vào hộp thư**.

6. **Ngày đêm:** làm, **bắt buộc trước Tết** (thứ tự 6, chốt 20/11), dùng chung kỹ thuật với soi đèn (mục 8).
7. **Thứ tự ưu tiên (20/11):** mọi phần, kể cả Ngày đêm, Mò cua, Soi đèn và Ô ăn quan, đều vào nhóm **bắt buộc trước Tết**. Nội dung Tết tách làm hai: **Tết tối thiểu bắt buộc** (lì xì, loa xã chúc Tết, câu đối, cây nêu) và **Tết đầy đủ nếu kịp**. Lịch chi tiết và thứ tự cắt khi trễ: xem `ke-hoach-dot-2.md`.
8. **Chốt 20/11:** Giá chợ (Phần 9) làm trước Mùng 1/Rằm/loa xã (Phần 8) vì loa xã đọc giá chợ. Điểm danh **có popup**. Cúc, hướng dương là cây `eventOnly` cho Chợ đầu tháng.
9. **Chốt 20/11 (sau khi rà kế hoạch):** Bộ cây (Phần 4) làm **đầu tiên**. Chưa có người chơi thật nên **không viết migration trước bản thử kín 02/01**: đổi save thì tăng `version`, save / mã sao lưu cũ báo không dùng được và tạo save mới. Chợ đầu tháng tăng giá **nhóm quả**. Đơn hàng có thêm **đơn cá**. Các cặp cây trùng thời gian / lãi: cân lại khi chạy mô phỏng.

Kế hoạch triển khai: `ke-hoach-dot-2.md` (cùng thư mục).

---

## 12. Kế hoạch kịp Tết 2027

### Mốc thời gian (tính từ 19/11/2026)
- Mùng 1 Tết: **06/02/2027**. Ông Công ông Táo (23 tháng Chạp): khoảng **30/01/2027** (ngày chính xác lấy từ bảng âm lịch ở mục 7).
- Sự kiện Tết nên mở **từ 23 tháng Chạp tới mùng 10**. Người chơi cần vào game trước đó vài tuần mới có nông trại để chơi Tết.
- **Phát hành bản public: chậm nhất khoảng 15/01/2027.** Từ giờ tới đó còn khoảng **8 tuần**, mà phải gánh: khoảng 28,75 ngày công đợt 2 (gồm cả Mò cua, Soi đèn, Ô ăn quan), phần MVP còn dở (vẽ lại map, build web, thử trên điện thoại thật, giảm dung lượng xuống dưới 10 MB), và art.
- **Bản thử kín: khoảng 02/01/2027** (02–08/01), cho vài người chơi thật trên iPhone và trong trình duyệt nhúng Facebook/TikTok.

### Phạm vi kịp Tết

Cập nhật theo chốt 20/11.

| Bắt buộc | Bắt buộc, cắt đầu tiên nếu trễ 26/12 | Nếu kịp | Để sau Tết |
|---|---|---|---|
| `GameCalendar` + âm lịch (0, 7) | Ô ăn quan (14) | Tết đầy đủ (12b) | Độ tươi nông sản |
| Hộp thư (9) | Mò cua (13) | Kéo cá: thử vuốt thay nút | Vườn cây ăn trái |
| Bộ máy sự kiện + Tết tối thiểu (4, 12a) | Soi đèn (13) | Thêm sự kiện ngẫu nhiên ngoài 8 cái | Ao nhà |
| Bộ cây mới (1) | | | |
| Album cá + câu rác (3) | | | |
| Đơn hàng có thoại (2) | | | |
| Điểm danh (10) | | | |
| Sửa cần (11) + kéo cá (5) | | | |
| Giá chợ theo ngày (6) | | | |
| Ngày đêm (8) | | | |

### Nội dung sự kiện Tết (dựng trên bộ máy brick, ít art)
- **23 tháng Chạp, "Tiễn ông Táo":** trong 3 ngày, tỉ lệ cá chép ×3. Câu được cá chép thì có thêm nút **"Thả cá chép"**: không bán, đổi lấy quà trong hộp thư và điểm Tết. Rất đúng phong tục, và chỉ cần một nút cùng một dòng thoại.
- **Từ 23 tới 30 Tết, "Chợ Tết":** dưa hấu, hoa đào, lúa giá ×1,5. Người làng đặt đơn Tết, có thoại riêng (Bà Lụa cần dưa hấu bày mâm ngũ quả…). Hoa đào (art vẽ sau) mở như cây sự kiện chỉ trồng được trong dịp này.
- **Mùng 1–3, "Lì xì":** mỗi ngày một bao lì xì trong hộp thư (xu, mồi, hạt hiếm). Loa xã đọc lời chúc Tết.
- **Trang trí:** cây nêu và câu đối ở cổng (2 sprite), pháo hoa nhỏ đêm 30 (particle). Tắt hết sau mùng 10.
- **Thư Tết** từ mẹ và từ sếp cũ, nối vào cốt truyện bỏ phố về quê.

### Rủi ro
- Lượng việc sát nút với một người làm. Nếu tới **26/12** chưa xong Soi đèn thì cắt theo thứ tự Soi đèn → Mò cua → Ô ăn quan (xem `ke-hoach-dot-2.md`), đừng để lỡ hạn ra mắt.
- Dịp Tết nhiều người chơi lần đầu qua link Facebook/TikTok. Cần ưu tiên **thời gian tải và chạy ổn trong trình duyệt nhúng** hơn là thêm tính năng.
- Dữ liệu trong trình duyệt nhúng tách riêng với Safari/Chrome. Nhắc người chơi **sao lưu mã** trước Tết, ví dụ bằng một thư trong hộp thư kèm quà nhỏ khi bấm sao lưu.


---

## 13. Mò cua (ban ngày) + Soi đèn (ban đêm) – trước Tết (thứ tự 15–16)

> ⏸ **Pending (21/11/2026):** sau khi tách giờ game (30 phút/ngày, chỉ hiển thị) khỏi giờ thật, chưa chốt mò cua / soi đèn mở theo ngày–đêm game hay khung giờ thật. Chốt lại trước khi làm Phần 15–16.

Phần chơi tay thứ hai, song song với câu cá. **Một thửa ruộng, hai chế độ** theo khung giờ.

### Vị trí và mở khóa
- Thửa ruộng nằm ở rìa vùng câu cá có sẵn (đi qua cổng như khi đi câu), không cần vẽ map mới.
- Mở khóa sau **lần đầu thu hoạch lúa**, để nối với phần trồng trọt.
- Giới hạn: **1 lượt mỗi khung giờ**. Bình minh tới Hoàng hôn là mò cua; Tối và Khuya là soi đèn. Không có thanh năng lượng. Sự kiện "Mưa rào" và "Nước lên" cho thêm 1 lượt và nhiều con hơn.

### Chế độ mò cua (khoảng 1 phút)
- Lưới bùn 5×5, mỗi lượt có **8 lần mò**. Dưới bùn giấu 3–6 con vật, 2–3 cục đá và có thể có rác (dùng chung bảng `JunkDef` với câu cá).
- **Dấu hiệu:** ô có con hoặc ô cạnh nó có thể nổi bọt khí (theo tỉ lệ, không phải lúc nào cũng có). Ốc để lại vệt bò chỉ hướng. Người chơi suy luận rồi mới mò.
- **Mỗi loài có nét riêng:**
  - Cua đồng: mò trượt ở ô kề bên thì **cua bò ngang** sang ô khác.
  - Chạch: trơn, phải chạm lần 2 trong 0,6 giây.
  - Ốc bươu, ốc nhồi, hến: nằm yên.
  - Tép đồng: mỗi ô có vài con.
- **Sinh bàn bằng seed** (dayKey, khung giờ). Lưu trạng thái giữa lượt, nên tải lại trang không đổi được bàn (chống "chơi lại cho tới khi đẹp").

### Chế độ soi đèn (khoảng 45 giây)
- Màn hình tối (`DarknessOverlay`, mục 8). **Kéo ngón tay** để di chuyển vòng đèn pin.
- Con vật chỉ thấy được trong vòng sáng: **hai đốm mắt phản sáng**. Chạm trúng trong lúc chúng còn đứng là bắt được.
- Ếch, nhái nằm trong vòng sáng quá khoảng 1,2 giây thì nhảy đi. Người chơi phải soi lướt qua rồi chạm nhanh.
- Lươn ra khỏi hang ngắn hơn; cua ban đêm ra nhiều hơn ban ngày.
- Giới hạn bằng **pin đèn** (thanh thời gian), nâng cấp được.
- Đêm Rằm: trăng sáng, độ tối giảm, ra nhiều con hơn.

### Ăn khớp với các hệ thống khác
- **Mồi câu:** ốc → mồi ốc; tép đồng → **mồi tép** (đã có); nhái → **mồi nhái** (đã có). Đây là nguồn mồi thứ hai ngoài giun và cửa hàng.
- **Đơn hàng:** canh cua rau muống, bún ốc, bánh đa cua, ếch om chuối đậu. Thoại người làng có sẵn chỗ để thêm câu.
- **Album:** thêm tab **"Đồng ruộng"**: cua đồng, ốc bươu, ốc nhồi, hến, tép đồng, chạch, ếch, nhái, lươn đồng. Thưởng mốc qua hộp thư.
- **Chỗ tiêu xu:**
  - Găng tay: +1 lần mò mỗi lượt.
  - Cào: lộ thêm dấu hiệu.
  - Giỏ: chứa được nhiều hơn.
  - Đèn pin: vòng sáng to hơn, pin lâu hơn.
- **Sự kiện:** dùng lại các hiệu ứng `FishWeightMult` và `ExtraOrder`; thêm hiệu ứng `FieldBonus`.

### Kiến trúc
- `FieldRoundState`: lưới, nội dung từng ô, số lần mò còn lại, các ô đã mở, con đã bắt.
- `FieldGenerator(seed, mode, upgrades)` và `FieldRules`: mò ô, cua di chuyển, xử lý chạch. Viết C# thuần, unit test bằng seed cố định.
- Soi đèn: logic chỉ giữ danh sách con xuất hiện (vị trí, thời điểm ra, thời điểm bỏ chạy) sinh theo seed; tầng hiển thị lo vòng sáng và nhận chạm.
- Save: tăng `version` (không migration trước bản thử kín 02/01); thêm trạng thái lượt đang dở, số lượt đã dùng theo khung giờ, và cấp nâng cấp.

### Art
- 2–3 kiểu ô bùn, bọt khí 2 frame, vệt bò.
- 9 con vật 16×16, kèm sprite "đôi mắt phản sáng".
- Icon cho 4 món nâng cấp.

**Công:** mò cua 3 ngày, soi đèn 1,5–2 ngày (không tính `DarknessOverlay`), album, mồi và đơn 0,5 ngày.

---

## 14. Ô ăn quan với lũ trẻ trong làng – trước Tết (thứ tự 14)

Phần chơi thư giãn, gần như **đứng ngoài kinh tế**, ít art, rất "hồn Việt". Dễ thành clip TikTok kiểu "ai còn nhớ trò này?".

### Luật (bản phổ biến, chốt trong data)
- Bàn có 10 ô dân (mỗi bên 5 ô, mỗi ô 5 sỏi) và 2 ô quan ở hai đầu (mỗi ô 1 quan, tính bằng 10 điểm).
- Tới lượt: chọn một ô dân bên mình còn sỏi, chọn hướng trái hoặc phải, rải từng viên.
  - Ô kế tiếp còn sỏi: bốc lên rải tiếp.
  - Ô kế tiếp trống và ô sau nó có sỏi: **ăn** ô sau đó. Nếu tiếp tục cách một ô trống mà ô sau còn sỏi thì ăn tiếp (ăn liên hoàn).
  - Gặp hai ô trống liền nhau, hoặc ô kế tiếp là ô quan: hết lượt.
- Hết sỏi bên mình thì phải lấy 5 sỏi đã ăn ra rải lại; không đủ thì thua.
- Hết ván khi cả hai ô quan đã bị ăn hết ("hết quan, tàn dân"). Ai nhiều điểm hơn thì thắng.
- Luật "quan non" (không được ăn ô quan khi quan còn ít sỏi) là **tùy chọn**, mặc định tắt cho dễ.

### Đối thủ
| Người chơi | Độ khó | AI |
|---|---|---|
| Cu Tí | Dễ | Chọn nước ăn được nhiều nhất ngay lượt đó, có 25% chọn ngẫu nhiên |
| Bé Na | Vừa | Minimax độ sâu 2 |
| Cụ Đồ | Khó (mở từ cấp 10) | Minimax độ sâu 4, cắt tỉa alpha–beta |

Mỗi nước có tối đa 10 lựa chọn, nên độ sâu 4 chỉ khoảng 10.000 nút: chạy tức thì trên WebGL, không cần thread.

### Phần thưởng
- **3 ván thắng đầu tiên mỗi ngày** có quà gửi hộp thư: mồi, dế, hạt hiếm. Sau đó chơi bao nhiêu cũng được nhưng không có quà, để không thành cách cày xu.
- Album thêm mục "Thành tích ô ăn quan": lần đầu thắng từng người, ăn liên hoàn 3 ô…
- Sự kiện Tết sau này có thể thêm "giải ô ăn quan đầu xuân".

### Kiến trúc
- `OAnQuanState` (12 ô, điểm, lượt) và `OAnQuanRules.Apply(move)` trả về **danh sách bước** (rải, ăn, hết lượt). Tầng hiển thị chỉ phát hoạt ảnh theo danh sách này, nên logic và animation tách hẳn nhau.
- `OAnQuanAI` dùng chung luật. Unit test mỗi luật bằng các thế cờ dựng sẵn.
- Lưu ván đang chơi dở.

### UI và art
- Bàn kẻ bằng phấn trên nền đất (một sprite), sỏi 2–3 kiểu, viên quan to.
- Chạm chọn ô rồi chạm mũi tên trái hoặc phải; rải từng viên có âm thanh "lộc cộc".
- Chân dung lũ trẻ dùng lại của bộ NPC.
- Nút ≥ 48 pt; bàn đặt nằm ngang vừa màn dọc (2 hàng 5 ô cộng 2 ô quan).

**Công:** luật và test 1 ngày, AI 0,5 ngày, UI và hoạt ảnh 1–1,5 ngày.


---

# Bản sao `Docs/thiet-ke-shiny-ca.md`

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


---

# Bản sao `Docs/MVP-Plan.md`

# Về Quê – Kế hoạch setup MVP trong Unity (28/10/2026)

Mỗi phần nhỏ, chạy và kiểm tra được trước khi sang phần sau. Phần nào code lớn (đánh dấu 🔍) thì đưa kiến trúc chi tiết để duyệt trước.

## Thay đổi so với instructions
- Khu = nhiều luống, lấy từ `ZoneDefinition` (ScriptableObject). Logic định danh ô bằng `(zoneId, bedIndex, col, row)`.
- Camera toàn cảnh KHÔNG thấy hết nông trại: Reference Resolution khoảng 208×370 (≈13 tile bề ngang, zoom 5x trên màn 1080–1170 px). Zoom vào khu theo bounding box (~10x).
- Bố cục map: Map D (trải ngang, 44×26), xem Phần 3.
- Số ô mở sẵn: Khu 1 là 6; Khu 2, 3 mở miễn phí 1 ô khi mở khóa. Đặt trong `ZoneDefinition.startOpenCount`.
- 07/11/2026: đưa câu cá vào MVP (Phần 10) ở vùng câu riêng (cùng scene, ngoài cổng trái); thêm nhân vật (đi lang thang ở nông trại, chạm để đi ở vùng câu, hiển thị nhiều lớp để sau thêm trang phục); ao nhà chỉ để thả cá sưu tầm.
- 11/11/2026: bỏ nhân vật đi lang thang; ở nông trại cũng chạm để đi, gieo / tưới / thu hoạch xếp vào hàng việc, nhân vật đi tới làm lần lượt đúng thứ tự chọn (Phần 10 mục 0b); zoom giữ nguyên.
- 12/11/2026: chuyển toàn bộ art từ Sprout Lands sang Sunnyside World (Phần 3b); cây Sunnyside dùng tạm, người dùng vẽ bộ cây Việt cùng phong cách.
- 12/11/2026: đơn hàng 5 ô, giao xong chờ 10 phút; hạng Thường / Khẩn cấp (20%, chờ 30 phút, thưởng ×2, hết hạn 30 phút) thay cho ô đơn gấp riêng; giới hạn 15 đơn/ngày (Phần 6c mục 2).
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

## Phần 3b – Chuyển art sang Sunnyside World (duyệt 12/11/2026)
Lý do: một phong cách thống nhất; Sunnyside có sẵn nhân vật nhiều lớp (thân / tóc / dụng cụ), đủ bộ câu cá, UI (thanh, icon, khung 9-slice), nước, bến, thuyền thúng. Cây Sunnyside chỉ dùng tạm, người dùng tự vẽ bộ cây Việt cùng phong cách. Làm trước Phần 10 giai đoạn 2.
0. **Tài liệu + dọn:** `CLAUDE.md` (Art), plan, `Docs/art-phan-10.md`; xóa thư mục GameMaker + `.DS_Store` của gói. Người dùng kiểm tra giấy phép dùng thương mại.
1. **Nhân vật:** `CharacterView` 3 lớp đồng bộ khung (thân, tóc `shorthair`, dụng cụ), lật khi quay trái, lên/xuống giữ hướng ngang; hoạt ảnh: đứng `IDLE`, đi `WALKING`, gieo + thu `DOING`, tưới `WATERING` (fps chỉnh được, mỗi việc ≤ ~0,5 giây); menu gán sprite mới (strip 96×64, pivot (0.5, 0.39)).
1b. **Tạo nhân vật (duyệt 12/11/2026):** lần đầu vào game (chưa tạo nhân vật, kể cả save cũ) hiện lớp phủ chọn nhân vật trước khi chơi; đổi lại được qua nút **Nhân vật** trên HUD.
   - Chọn: tên (1–16 ký tự, có dấu), kiểu tóc (6 kiểu Sunnyside: tóc ngắn, dài, xoăn, bát úp, bù xù, dựng), màu tóc (Gốc + 7 màu có sẵn). Xem trước nhân vật phóng 4× chạy hoạt ảnh đứng.
   - Màu tóc: shader `VeQue/HairTint` (lấy độ sáng tóc chuẩn hóa theo từng kiểu → tô theo màu chọn, giữ viền tối); "Gốc" giữ nguyên.
   - Logic: `CharacterLook` (tên, tóc, màu, đã tạo), `Farm.SetCharacter`, chuẩn hóa tên; save `version` 3 (save 2 → chưa tạo nhân vật, tóc mặc định); mã sao lưu mang theo. Có test.
   - View: `CharacterSkin` tách bộ tóc (6 kiểu) + bảng màu; `CharacterView` đổi tóc / màu; `CharacterPreview`, `CharacterCreator` (UI), nút HUD; `UiBuilder` dựng; menu import thêm 6 kiểu tóc.
   - Bẫy web mobile: ô nhập tên trên WebGL mobile cần thử bàn phím ảo trên máy thật.
   - Kiểm tra: test Logic; xóa save → vào game hiện màn tạo nhân vật, chưa nhập tên không bắt đầu được; đổi tóc / màu thấy ngay ở xem trước và nhân vật; mở lại game giữ nguyên; nút Nhân vật trên HUD sửa được.
2. **Map D vẽ lại** (công cụ đã có 12/11/2026; người dùng làm sau). Tileset Sunnyside tách lớp: đất / nước làm nền, cỏ phủ trên bằng mảnh viền bo tròn trong suốt → nền + hàng rào vẽ bằng công cụ vẽ map (đổi tile), viền cỏ mềm / ao / nhà / cây vẽ tay trên `Decor`. Các bước:
   1. Chạy **Về Quê → Sunnyside: import tileset 16px** (cắt 1740 tile 16×16, tên `ss_x_y`: x đếm từ trái, y đếm từ trên, từ 0).
   2. **Window → 2D → Tile Palette → Create New Palette** `Sunnyside`, lưu vào `Assets/Art/Tile Pallette/Sunnyside`; kéo `spr_tileset_sunnysideworld_16px` vào palette (tile lưu cùng thư mục).
   3. Gán vào `Assets/_Game/Data/MapPaintSettings.asset`:
      - Grass Tiles: `ss_1_1` (Weight 20) + `ss_1_2`, `ss_2_2`, `ss_3_2`, `ss_4_2`, `ss_5_2` (Weight 1).
      - Yard Tiles (cả 9 ô): `ss_3_1` (đất trơn); viền làm mềm ở bước 5.
      - Fence Tiles: ngang (Top / Bottom) `ss_39_2`, dọc (Left / Right) `ss_40_3`, 4 góc thử `ss_40_2`; góc thừa thanh gỗ thì thử cột khác trong vùng `ss_38_1`…`ss_42_4`.
      - Plot Tile: giữ nguyên (đổi ở bước 3 dưới).
   4. Cửa sổ vẽ map: vẽ **Ground** và **Hàng rào**; không vẽ lại **Plots**.
   5. Vẽ tay trên `Decor` (xóa tile Sprout Lands cũ): viền cỏ quanh sân (mảnh bo trong vùng `ss_1_7`…`ss_17_8`, vd. `ss_2_7`, `ss_3_7`, `ss_13_7`, `ss_14_7`, `ss_1_8`, `ss_6_7`); đất có vết `ss_9_7`, `ss_10_7`, `ss_9_8`, `ss_10_8`; ao [35, 9, 7, 11] nước `ss_4_1` + mảnh viền xanh dương ở hàng trên tileset; nhà kho / chuồng = nhà gỗ Sunnyside (giữa tileset, 5 màu mái); cây / đá / bụi / hoa / rơm ở góc phải tileset (cây động: `Elements/Plants`); để trống hàng rào ở cổng trái.
   6. Tô lại `Blockers` (rào, nhà, cây to, ao, đá); chọn `WalkGrid` xem ô đỏ.
   - Kiểm tra: Game view 1x sắc nét, không đường kẻ giữa tile (nếu có → bước 5 Sprite Atlas padding); nhân vật không đi xuyên rào / nhà / ao. Vẽ xong chụp màn hình để xem cùng.
3. **Ô đất + 4 cây (tạm):** rau muống ← `kale`, cải xanh ← `cabbage`, cà chua ← `radish` (tạm), bí đỏ ← `pumpkin`; `_00` hạt, `_01`–`_04` giai đoạn, `_05` nông sản; menu gán sprite mới; ô đất `soil_*` (thường / ướt). Thời gian lớn, cân bằng không đổi.
4. **UI (làm sau, cùng Phần 6 mục 4):** icon hạt / nông sản / giỏ, khung 9-slice, thanh. Font tiếng Việt giữ TMP.
5. **Dọn + build:** Sprite Atlas chỉ sprite Sunnyside đang dùng; hết tham chiếu thì xóa Sprout Lands (xin phép); build, so Build Report (mốc 10,8 MB).

**Kiểm tra:** chơi trọn vòng gieo → tưới → thu → bán trên map Sunnyside, nhân vật 3 lớp đi / làm việc đúng hướng, không còn sprite Sprout Lands trên màn hình; Build Report không tăng quá mốc.

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
1. 5 đơn cùng lúc (từ cấp 2; trước 12/11 là 3); mỗi đơn 1–3 loại cây đã mở, số lượng tăng theo cấp; tên người đặt chỉ là chữ.
2. Thưởng xu = Σ(số lượng × giá bán) × 1,6 (làm tròn 5); XP = Σ(số lượng ÷ sản lượng × XP cây) × 2 (tính theo số lần thu, để cây sản lượng cao không được XP gấp nhiều lần).
3. Giao xong → chờ 10 phút (đơn khẩn cấp 30 phút) mới có đơn mới; bỏ đơn → 5 phút. Chi tiết, hạng Khẩn cấp, giới hạn 15 đơn/ngày: Phần 6c mục 2.
4. Sinh đơn qua `IRandom` có seed lưu trong save (test lặp lại được, tải lại trang không đổi đơn).

**Chống kẹt (ghi đầy đủ 12/11/2026, theo code `Farm.TryRescue` / `IsStuck`):**
- Kẹt khi đủ cả 4 điều: (1) Kho không còn nông sản nào (kể cả nông sản cây chưa mở); (2) không còn hạt của cây đã mở (hạt cây chưa tới cấp không tính); (3) xu ít hơn giá hạt rẻ nhất trong các cây đã mở; (4) không ô nào đang có cây.
- Kẹt → tặng 1 hạt cây cứu (`FarmDefinition._rescueCrop` → `FarmDef.RescueCropId`, hiện là rau muống; cây cứu phải `enabled`, không thì tắt chống kẹt), phát sự kiện `Rescued` → HUD hiện "Được tặng 1 hạt …".
- Kiểm tra lúc nạp save (khởi tạo `Farm`) và sau mỗi lệnh làm đổi tiến trình (`NotifyChanged`). Tặng xong là có hạt nên không tặng lặp.
- Không tính: đơn hàng (giao đơn cần nông sản, đã có ở điều kiện 1), mồi, cần. **Từ 13/11/2026: còn cá trong giỏ thì không kẹt** (bán được lấy xu).
- ~~Phải sửa ở Phần 10 giai đoạn 5~~ – đã sửa 13/11/2026 (`Farm.IsStuck` xét giỏ cá; test `Rescue_FishInBasket_DoesNotRescue`). Mồi / cần không tính (câu ra cá mới có tiền, nhưng giỏ đầy hoặc cần hỏng vẫn có thể kẹt – xem lại lúc đó).
- Test: `ProgressionTests` – kẹt sau lệnh, kẹt khi nạp, chỉ còn hạt cây khóa vẫn tặng, có cây đang lớn thì không tặng; `FarmDefinitionTests` – data mặc định cây cứu là rau muống.

**Sửa kèm:** chống kẹt (chi tiết ở trên); thông báo "+xu" khi bán, "Đã mở Khu N"; hộp xác nhận mở ô đóng cùng các bảng khác; mua hạt từ bảng chọn hạt xong thì gieo luôn vào ô đang chờ.

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
2. **Đơn hàng: thời gian chờ, hạng Khẩn cấp, giới hạn mỗi ngày** (thấp) – XONG phần code 12/11/2026 (thay thiết kế 07/11 theo yêu cầu người dùng):
   - **5 ô đơn** (trước 3). Giao xong thì ô trống, đếm ngược tới đơn kế tiếp. Hạng của đơn kế tiếp tung ngay lúc ô trống: **Thường** chờ **10 phút**, **Khẩn cấp** (xác suất **20%**) chờ **30 phút**. Bỏ đơn: đơn kế thường chờ 5 phút, khẩn cấp vẫn chờ 30 phút. Ô chưa từng hẹn đơn (mới mở đơn hàng, save cũ) có đơn ngay, hạng ngẫu nhiên.
   - **Khẩn cấp:** thưởng xu và XP **×2** so với đơn thường cùng món; **hết hạn sau 30 phút** kể từ lúc xuất hiện (lúc vào game thấy đơn, không tính thời gian vắng mặt) → ô trống, chờ đơn kế như khi giao. Thẻ màu khác, nhãn "KHẨN CẤP · còn mm:ss". Thay cho ô đơn gấp riêng của thiết kế 07/11.
   - **Giới hạn 15 đơn giao mỗi ngày.** Chỉ sinh đơn mới khi (đã giao hôm nay + đơn đang có trên bảng) < 15, nên trên bảng không có đơn không giao được. Bỏ đơn và đơn hết hạn không tính. Hết lượt: ô trống hiện "Hết đơn hôm nay, có đơn mới sau 0 giờ"; đầu bảng hiện "Còn N đơn hôm nay".
   - **Ngày** = theo giờ địa phương: chỉ số ngày = `floor((giờ UTC + độ lệch múi giờ) / 86400)`. Độ lệch lấy từ trình duyệt mỗi lần mở game (jslib `VeQue_GetUtcOffsetMinutes`), trong Editor lấy từ máy; lưu trong save. Sang ngày mới đếm lại lượt; ngày lùi (đổi múi giờ) giữ nguyên.
   - Logic: `OrderRank`, `OrderState.Rank` / `ExpiresAt`, `OrderDef` thêm thời gian chờ / tỉ lệ / % thưởng / hạn / giới hạn ngày, `FarmState.OrderDay` / `DeliveredToday` / `UtcOffsetSeconds`, `Farm.SetUtcOffset`, `FarmResult.OrderExpired`. Save `version` 4 (save 1–3 nạp được: đơn thường, lượt hôm nay = 0).
   - Cân bằng: chưa chạy lại mô phỏng (`sim2.py` không có trong repo) → chơi thử rồi chỉnh số trong `FarmDefinition`.
   - Kiểm tra: test Logic (chờ 10 / 30 phút, khẩn cấp ×2 và hết hạn, đủ 15 thì ngừng, sang ngày mới đếm lại, bỏ đơn không tính, múi giờ, save 3 → 4); trong Editor dùng **Tua thời gian** thử chờ đơn, hết hạn, qua ngày.
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
- **Sang vùng câu:** bấm **Đi câu** (hoặc chạm cổng) → nhân vật **tự đi ra cổng** (cổng trái Map D) → **chuyển cảnh bằng màn tối** (chốt 12/11/2026) sang vùng câu (cùng scene, cùng Grid, Tilemap riêng, đặt xa nông trại) → nhân vật đứng ở cổng vùng câu. Chạm thêm lần nữa thì bỏ qua đoạn đi ra cổng. Đang có hàng việc thì hủy + báo "Đã hủy N việc để đi câu". **Về nhà** thì ngược lại. Zoom ở vùng câu bằng toàn cảnh nông trại.
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

- **Art (đổi 12/11/2026 sang Sunnyside World, xem Phần 3b và `Docs/art-phan-10.md`):** nhân vật 3 lớp + câu cá `CASTING` / `WAITING` / `REELING` / `CAUGHT`; nước, bến, thuyền thúng, icon cần / giỏ / "!" / thanh / mũi tên có sẵn. Còn thiếu: 13 loài cá, icon mồi, cần 5 bậc.
Vòng chơi: **chọn mồi → ném cần (canh perfect) → chờ cá cắn, giật đúng lúc → kéo cá bằng chuỗi mũi tên 4 hướng → được cá (loài, độ hiếm, cân nặng, giá) hoặc sổng / cá thoát.** (13/11/2026: bỏ đứt dây / gãy cần khi kéo.)
Tham khảo cơ chế chung của thể loại: thanh lực có điểm ngọt khi ném, báo hiệu cắn câu + khung phản xạ ngắn, rồi một mini game kéo cá (Stardew Valley, Fish Quest, WebFishing, Tiny Fishing). Kéo cá dùng kiểu nhập chuỗi mũi tên theo nhịp (kiểu game nhảy theo nhịp) – chỉ mượn cơ chế, art và tên riêng.

**1. Ném cần – canh perfect**
- Nhân vật đã đứng ở chỗ câu trong vùng câu. Nhấn giữ nút Ném: thanh lực chạy lên xuống liên tục (0 → 100% → 0, chu kỳ ~1,2 giây); thả tay để ném.
- Vùng trên thanh: **Perfect** (hẹp, ở gần đỉnh), **Tốt**, **Thường**. Cần câu rank cao thì vùng Perfect rộng hơn, thanh chạy chậm hơn.
- Lực quyết định phao rơi gần hay xa bờ: xa = nước sâu = cá to, cá hiếm hơn.
- Perfect: tăng tỉ lệ cá hiếm, chờ cắn ngắn hơn, vòng kéo đầu dễ hơn; có hiệu ứng + chữ "Perfect!". Ném hỏng (quá yếu): phao rơi sát bờ, chỉ có cá thường.

**2. Chờ cá cắn – giật câu**
- Chờ ngẫu nhiên (2–8 giây, sửa 13/11/2026 từ 3–15; mồi hợp loài, ném perfect thì ngắn hơn). Phao **rỉa nhẹ** (1 px) vài lần (mồi nhử) rồi mới **chìm hẳn (3 px) + dấu "!" nảy**; nút ghi "Chờ..." lúc chờ, đổi thành **"GIẬT!"** phập phồng khi cá cắn.
- Chạm "Giật" trong khung phản xạ (1,6 / 1,4 / 1,2 / 1,0 / 0,8 giây từ Thường tới Huyền thoại, sửa 13/11/2026 từ 0,9 … 0,4). Giật trước khi cá cắn → cá sợ bỏ đi, mất mồi (giữ phạt, chốt 13/11/2026). Trễ → cá ăn mất mồi.
- **Tỉ lệ trượt**: giật đúng lúc vẫn có xác suất trượt lưỡi (cá sổng ngay), giảm theo rank cần, tăng theo độ hiếm cá.
- Android có thể rung máy khi cá cắn (`navigator.vibrate`); iPhone trên web không rung được → dựa vào hình + âm thanh.

**3. Kéo cá – chuỗi mũi tên 4 hướng, 1 lượt (chốt lại 13/11/2026)**
- Móc được cá → hiện **1 chuỗi** mũi tên (↑ ↓ ← →) + thanh thời gian; **xem trước 1,2 giây** ("Sẵn sàng...", chưa tính giờ; bấm đúng mũi tên đầu thì tính luôn – `FishingTuning.PullPreviewSeconds`). **Bóng cá** trong nước ngay trước phao (phía xa nhân vật) lao theo mũi tên vừa bấm, bấm sai thì vùng ra rồi về; câu được thì bị kéo vào, thoát thì bơi đi. Nhập đúng hết chuỗi trước khi hết giờ là **câu được**. Không có nhiều vòng, thể lực cá, độ căng dây, combo hay nút Chốt.
- **Càng nhanh càng hoàn hảo**: dùng ≤ 40% thời gian = **Perfect** (+50% XP, cân nặng lệch về nặng hơn), ≤ 70% = **Tốt** (bình thường), còn lại = **Thường** (cân nặng lệch về nhẹ). Các mốc ở `FishingTuning`.
- **Bấm sai** 1 mũi tên → cả chuỗi **nháy đỏ**, về mũi tên đầu, phải **nhập lại từ đầu**; thời gian vẫn chạy (phạt bằng thời gian: khó Perfect, dễ hết giờ). Không đứt dây, không gãy cần (chốt 13/11/2026, phương án B). **Hết giờ** → cá thoát (chỉ mất mồi).
- Cá càng hiếm: chuỗi dài hơn (4–5 → 8–10 mũi tên, nâng 14/11/2026), ít thời gian hơn (4 → 3 giây); cá huyền thoại có **mũi tên khác màu (đỏ)** – bấm hướng ngược lại (không đổi chuỗi giữa chừng). Ném Perfect: chuỗi bớt 1 mũi tên. Chuỗi luôn **ít nhất 4 mũi tên** (`FishingTuning.MinPullArrows`, 14/11/2026). Cần rank cao **thêm thời gian** (tre 0%, trúc +10%, thủy tinh +20%, carbon +30%, vàng +50%; thay cho "sức bền dây").
- Nhập trên điện thoại: **4 nút mũi tên** 64 pt xếp **chữ thập ở giữa** nửa dưới màn hình (bàn phím: phím mũi tên). Cần thử cảm giác trên máy thật.

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
- Rank (mở rộng 10 bậc 14/11/2026, có icon `Art/Fishing/rods`): tre (cấp 1, tặng) → nứa (2, 120 xu) → trúc (3, 300) → mây (4, 700) → sợi thủy tinh (5, 1.500) → composite (7, 3.000) → carbon (9, 6.000) → titan (11, 11.000) → vàng (13, 20.000) → rồng (15, 40.000). Mỗi bậc tăng vùng Perfect (±0,030 → 0,090), chu kỳ thanh (1,2 → 1,9 giây), thêm thời gian kéo (0 → 60%), giảm trượt (0 → 18), thưởng cá hiếm (0 → 70%), độ bền (30 → 250); giá sửa ≈ 40% giá mua. Số trong `FishingDataGenerator`. Mua ở Cửa hàng, mở theo cấp.
- Chỉ số: độ rộng vùng Perfect, tốc độ thanh lực, sức bền dây (độ căng tối đa), tỉ lệ trượt, thưởng tỉ lệ cá hiếm, **độ bền**.
- Mỗi lần câu trừ độ bền. Độ bền về 0 → **cần hỏng**: sửa (tốn xu, rẻ hơn mua) hoặc mua mới. Có thể giữ nhiều cần, chọn cần trước khi câu.
- **Lý do mất độ bền (chốt lại 13/11/2026: mọi mức trừ là 1):** mỗi lượt câu −1 lúc ném (trừ ngay, mọi kết quả kể cả bỏ dở); độ bền về 0 = hỏng (không còn gãy khi đứt dây – bỏ 13/11/2026). Câu được cá (to / hiếm), cá sợ, cá ăn mồi, trượt lưỡi, cá thoát không mất thêm.
- ~~Gãy khi đứt dây~~ (bỏ 13/11/2026). `RodState.Snapped` giữ trong save `version` 6 để save cũ nạp được; sửa / mua lại thì xóa.
- **Sửa làm giảm độ bền tối đa:** mỗi lần sửa, độ bền tối đa giảm 10% độ bền gốc (100 → 90 → … → 10%). Khi độ bền tối đa đã **dưới 15%** thì không sửa được nữa (sau 9 lần sửa) → mua lại.
- **Mua lại cần đang có:** được, khi cần hỏng hoặc đã từng sửa; trả đủ giá, cần về như mới (độ bền gốc, số lần sửa 0). Cần còn dùng được và chưa sửa thì không mua lại.
- Các con số ở `FishingTuning` (data), không viết cứng.

**6. Mồi câu**
- Mỗi lần ném tốn 1 mồi; mồi quyết định loài nào dễ cắn: giun → rô, diếc, trê; cám → chép, mè; tép → lóc, bống; mồi giả → lóc, cá quý. Mồi đặc biệt hút cá hiếm.
- Nguồn: giun có khi đào được lúc thu hoạch (nối câu cá với trồng trọt), mồi khác mua ở Cửa hàng. Hết mồi là nghỉ → giới hạn tự nhiên, không cần thanh năng lượng.
- **10 mồi (mở rộng 14/11/2026, icon `Art/Fishing/bait`)** – cấp mở, giá, thưởng cá hiếm, cá ưa: giun (không bán; rô, diếc, trê, lươn) · cám (1, 3 xu; diếc, tép, chép, mè) · ruồi (2, 4; mè, rô) · mồi tép (3, 6; lóc, tôm càng, bống tượng) · dế (4, 6, +5%; rô, trê, lóc) · nhái (5, 10, +10%; lóc, bống tượng) · mồi giả cá (6, 12, +20%; lóc, trắm đen) · mồi thơm (9, 25, +100%, icon bột; chép đỏ, chép vàng, trắm đen) · nhái hơi (11, 30, +60%; lóc, bống tượng, trắm đen) · thìa vàng (13, 50, +150%; chép vàng, chép đỏ, trắm đen). Id cũ giữ nguyên cho save.

**7. Kỹ thuật & thứ tự làm**
- Logic thuần (`VeQue.Logic`): `FishDef`, `RodDef`, `BaitDef`, phiên câu dạng state machine (Chờ ném → Đang ném → Chờ cắn → Cá cắn → Kéo → Kết quả), sinh cá / sinh chuỗi mũi tên bằng `SeededRandom`, thời gian qua `IClock`; test cho tỉ lệ, cân nặng, trượt, bấm sai nhập lại, độ bền. Save: cần + độ bền, mồi, giỏ cá, kỷ lục.
- View: khung camera câu cá, phao + dây (pixel), thanh lực, nút mũi tên + vạch nhịp, hiệu ứng Perfect / bấm sai (nháy đỏ) / cá thoát, âm thanh (chỉ phát sau lần chạm đầu trên web).
- Nhân vật / vùng câu: `GridPath` (A*, C# thuần, có test) trên lưới ô đi được lấy từ Tilemap; `CharacterView` (nhiều lớp đồng bộ khung, 4 hướng, đi / đứng / câu, di chuyển theo pixel, vẽ theo trục Y); bộ điều khiển chạm để đi (nông trại + vùng câu), `FarmTaskQueue` + bộ chạy hàng việc ở nông trại; `CameraController` giới hạn + zoom theo vùng, chế độ đi theo nhân vật + kéo xem rồi tự quay về; `CameraTouchInput` thêm chế độ vùng câu. Save: không lưu vị trí nhân vật (luôn vào game ở nông trại); lưu cá trong ao nhà.
- Art cần có: phao, cần theo rank, sprite từng loài cá, mồi, nước động, tile vùng câu (xem mục 0). Âm thanh: bước chân, quăng cần, cá cắn, bấm sai, cá thoát.
- Giai đoạn (mỗi giai đoạn duyệt thiết kế chi tiết trước khi làm):
  1. **Nhân vật ở nông trại: chạm để đi + hàng việc trồng trọt** (mục 0b): `GridPath`, lưới ô đi được, `CharacterView` nhiều lớp (1 trang phục), chạm để đi, `FarmTaskQueue`, nối `FarmInteraction` / `FarmGesture` / `SeedPicker` vào hàng việc, nút Dừng, làm hết khi ẩn tab.
  2. **Vùng câu + di chuyển** (đã viết code 12/11/2026, vùng tạm bằng menu **Vùng câu: tạo tạm**): vẽ vùng (Tilemap riêng, chuyển cảnh), giới hạn camera theo vùng, **Đi câu** (đi ra cổng) / **Về nhà**, chạm để đi, chạm chỗ câu, camera đi theo + kéo xem rồi tự quay về, HUD theo chế độ.
  3. **Câu cá – Logic + test** (đã viết code 12/11/2026): `FishDef` (gồm khung giờ), `RodDef`, `BaitDef`, phiên câu, sinh cá / chuỗi mũi tên, độ bền. Chốt 12/11: giỏ cá 8 ô, nâng cấp được (8 → 10 → 12 → 16 → 20, giá 300 / 1.000 / 3.000 / 8.000 – data) – **đổi 13/11/2026:** mở sẵn 8 ô, mua lần lượt từng ô tới 25, giá ô thứ k (k = 1…17) = 100 × 1,3^(k−1) tròn 10 xu (100, 130, 170 … 6.650; tới 20 ô ≈ 7.400 xu, đủ 25 ô ≈ 28.500 xu), save `version` 7 (`basketSlots`, save ≤ 6 quy đổi theo sức chứa cũ); đào được giun khi thu hoạch (10%, 1–2 con); câu cá mở từ cấp 1 (người dùng quy định lại sau), tặng cần tre + 10 giun. View chạy phần thời gian thực (thanh lực, giật, mũi tên), Logic nhận kết quả. ScriptableObject + data mặc định làm đầu giai đoạn 4.
  4. **Câu cá – chơi** (duyệt 12/11/2026, chia 5 đợt):
     - Đợt A – data: SO `FishDefinition` / `RodDefinition` / `BaitDefinition` / `FishingDefinition` (+ `FarmDefinition._fishing`); menu **Tạo data câu cá** (13 cá, 5 cần, 5 mồi theo bảng duyệt 12/11, cũng chạy trong **Tạo data mặc định**); menu **Câu cá: tạo art tạm** (phao 8×8, icon cá 16×16 tô màu theo độ hiếm → `Assets/_Game/Art/Fishing/`); test data.
     - Đợt B – hoạt ảnh nhân vật `CASTING` / `WAITING` / `REELING` / `CAUGHT` (thêm vào `CharacterSkin`, `CharacterSkinAssigner`).
     - Đợt C – ném (giữ nút, thanh lực, vùng Perfect / Tốt) + chờ cắn (phao rỉa, chìm, "!", rung Android) + giật; phao ở **đầu dây vẽ sẵn** của art Sunnyside (cố định, chốt 13/11/2026 – cách A), độ sâu Gần / Vừa / Xa vẫn theo lực nhưng chỉ hiện bằng chữ ("Perfect! · Phao: Xa"); trước Đợt D móc được cá thì báo rồi bỏ lượt; HUD vùng câu (cần + độ bền, mồi – chạm đổi, giỏ, khung giờ + còn bao lâu); đang câu chạm map không đi, nút **Thu cần** (bỏ lượt, mất mồi), ẩn tab = bỏ lượt.
     - Đợt D – kéo cá 1 lượt (chốt lại 13/11/2026, xem mục 3): chuỗi mũi tên + thanh thời gian (màu theo hạng còn đạt được); 4 nút mũi tên chữ thập ở giữa; nhanh = Perfect; sai = nháy đỏ + nhập lại từ đầu (sửa 13/11/2026), hết giờ = cá thoát; câu được → hoạt ảnh được cá → thông báo tạm (Đợt E thay bằng hộp kết quả); Editor dùng phím mũi tên. Logic bỏ vòng / thể lực / độ căng / combo / chốt nhịp; cần: sức bền dây → thêm thời gian.
     - Đợt D bổ sung (13/11/2026): ném xong hiện chữ trên đầu nhân vật (Hoàn hảo! / Tuyệt! / Tốt / Yếu, bỏ độ sâu phao); hụt cá cũng là chữ trên đầu (Sớm quá! / Mất mồi! / Trượt lưỡi! / Cá thoát!); câu được → hình cá bật khỏi nước ở phao, bay dọc dây về tay → **bảng cá vừa câu** (tên, hình, cân nặng, độ hiếm có màu, giá; dòng nhỏ hạng kéo + XP; OK).
     - HUD câu + giỏ (duyệt 13/11/2026, làm trước Đợt E): bỏ khung giờ và dòng cần / giỏ; **đồng hồ chung** trên HUD (giờ + khung giờ, góc trên phải dưới Sao lưu, chạm = khung giờ + còn bao lâu, cả nông trại và vùng câu); cột **3 nút icon** Cần / Giỏ / Mồi (số nhỏ: độ bền, số cá, số mồi; khóa khi đang câu) mở 3 bảng: **Cần** (tên, rank, độ bền, Đang dùng / Hỏng; Dùng, Sửa, Hết sửa), **Giỏ** (lưới 5 cột ô vuông tới 25 ô, cuộn dọc; ô chưa mở có ổ khóa, ô khóa kế tiếp hiện giá, bấm → xác nhận mở; nền theo độ hiếm; chạm → chi tiết tên / độ hiếm / cân nặng / giá / khung giờ bắt + **Thả**), **Mồi** (tên, số lượng, tác dụng; Dùng). Icon: Sunnyside `rod`, `basket`; mồi tạm `rod alt` (không có icon giun trong project).
     - Mua cần / mồi (duyệt 13/11/2026): mồi có cấp mở mua (cám 1, mồi tép 3, mồi giả 6, mồi thơm 9; giun không bán), cần giữ cấp cũ (tre 1, trúc 3, thủy tinh 5, carbon 9, vàng 12 – 14/11/2026 mở rộng 10 cần / 10 mồi, cấp mới ở mục 5, 6); bảng Cần / Mồi ở vùng câu và tab Cửa hàng → **"Hạt giống | Đồ câu"** dùng chung danh sách: cần (Dùng / Sửa / Hết sửa / Mua lại / Mua / "Cấp N"), mồi (Dùng / Mua 1 / Mua 10 / "Cấp N"); hộp lên cấp báo "Mở <cần / mồi>".
     - Bán cá + tab (duyệt 13/11/2026, phần đầu giai đoạn 5): Cửa hàng 3 tab "Hạt giống | Cần câu | Mồi câu"; Kho 2 tab "Nông sản | Giỏ cá"; giỏ cá (bảng Giỏ ở vùng câu và tab Giỏ cá trong Kho dùng chung) bán từng con (Bán + giá) và **Bán hết** (hộp xác nhận, tổng xu), vẫn Thả / mở ô; chống kẹt tính cá trong giỏ.
     - Đợt E – "Kỷ lục mới!" trong bảng cá vừa câu ("Loài mới!" khi lần đầu câu loài; "Kỷ lục mới! (cũ …)" khi nặng hơn); thông báo đào được giun bằng chữ nổi "+N giun" trên đầu nhân vật ở nông trại (duyệt 14/11/2026, cộng dồn trong 2 giây); build lại, so Build Report. (Chọn / sửa / mua cần, chọn / mua mồi, Thả cá, mở ô giỏ đã làm ở các mục trên.)
     - Con số data (bảng duyệt 12/11): chỉnh lại sau khi chơi thử / mô phỏng cân bằng.
  5. **Giỏ cá, bán, album, đơn hàng có cá**.
  6. **Ao nhà** (sau cùng): thả cá, bóng cá bơi, bảng danh sách cá.
  - Sau MVP: cửa hàng trang phục / phụ kiện, thêm vùng chơi.
  - Cân bằng: thêm câu cá vào mô phỏng kinh tế (thu nhập / giờ so với trồng cây). Mỗi giai đoạn thêm art: build lại, so Build Report.
- Đã chốt 07/11/2026:
  - Nhập khi kéo cá: **4 nút mũi tên** + nút Chốt (không dùng vuốt).
  - **XP câu cá cộng vào XP nhân vật** (chung cấp với trồng trọt). XP mỗi con theo độ hiếm + cân nặng; đưa vào mô phỏng cân bằng để nhịp lên cấp không lệch mục tiêu.
  - **Cá giữ trong giỏ** (loài + cân nặng, có sức chứa); từ giỏ: bán, giao đơn, thả ao nhà. Giỏ đầy thì phải bán / thả bớt mới câu tiếp.
  - Asset vùng câu (tile nước, bờ, bến), biểu tượng chỗ câu, âm thanh: **người dùng thêm sau**; làm code trước với hình tạm, thay khi có asset.
- Đã chốt 12/11/2026 (sau kiểm tra logic câu cá):
  - Kéo cá: nhập đúng chuỗi nhưng chốt nhịp **Trượt** vẫn trừ một ít thể lực cá (40, so với Tốt 70, Perfect 100) và vẫn giảm độ căng – giữ như vậy.
  - Data cá: luôn có vài loài cá Thường xuất hiện **cả ngày** (mọi khung giờ) và ở độ sâu Gần, để ném hỏng / khung giờ nào cũng có cá (code dự phòng lấy cá Thường bất kỳ, bỏ qua khung giờ, chỉ dùng khi data thiếu).
  - Độ bền cần: xem mục 5 (lý do mất độ bền, sửa giảm độ bền tối đa tới dưới 15%, mua lại cần đang có). Save lên `version` 6 (`repairCount`).
  - Chống kẹt chưa tính cá trong giỏ → sửa ở giai đoạn 5 (Phần 6b mục Chống kẹt).


---

# Bản sao `Docs/tien-do.md`

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


---
