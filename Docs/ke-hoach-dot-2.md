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
