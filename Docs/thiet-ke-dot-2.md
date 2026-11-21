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
  - **21/11:** thêm **túi ni lông, cành cây mục, săm xe đạp** theo art người dùng → 9 món thường **cùng trọng số** + hũ tiền xu cổ hiếm.
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
- **Chốt 21/11:** nút **Album** ở cột trái HUD, **dưới nút Thư**, hiện ở **cả nông trại lẫn vùng câu** (sau này album có thể thêm cây đột biến). Cô đồng nát chỉ xuất hiện trong lời thoại khi bán rác (không có nhân vật trên map, chưa đặt đơn). Rác nằm danh sách riêng trong giỏ (chiếm ô chung với cá); "Bán hết" bán cả rác; nút "Tự bán rác" ở bảng Giỏ.
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
- **Chốt 21/11:** ngày không có sự kiện cố định có **25%** (**22/11 đổi 33%**) ra sự kiện ngẫu nhiên. Sự kiện cố định chạy chồng nhau. `OrderBonus` tính **lúc giao**. `FishSizeMult` cho cá **vượt cân tối đa** (phá kỷ lục), giá theo cân giữ trần. `minLevel` không tham gia bốc (chưa đủ cấp = ngày thường).

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
- **Chốt 22/11:** popup điểm danh hiện quà cả tuần, người chơi **tự bấm "Nhận"** (không tự nhận); nhận xong quà vào hộp thư, icon bay về nút Thư. "Để sau" thì không tự mở lại trong ngày. Nút **"Điểm danh" ở cột trái HUD** (dưới Album, cả hai vùng), có chấm đỏ khi còn quà chưa nhận.
- **Chốt 21/11:** ngày 7 tặng **xu + 1 cần câu** (quà các ngày khác người dùng tự config trong `check_in.json`). Đã có cần đó thì quà sửa đầy độ bền. Xu tăng 10% mỗi vòng, tối đa gấp đôi.
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
