# Về Quê – Giới thiệu game (bản tổng hợp, 19/11/2026)

Tài liệu này tóm tắt game để trao đổi ý tưởng cải tiến. Game đang ở giai đoạn làm MVP.

## 1. Tổng quan
- **Tên:** Về Quê.
- **Thể loại:** nông trại thư giãn (idle / casual) kết hợp câu cá, bối cảnh làng quê Việt Nam.
- **Nền tảng:** Unity Web (WebGL), chơi bằng link trên điện thoại (Safari iOS, Chrome Android, trình duyệt nhúng Facebook / TikTok). Màn dọc. Không cần cài đặt, không có server.
- **Đối tượng:** người chơi phổ thông, ghé game vài lần mỗi ngày (mục tiêu thiết kế: ~4 lần/ngày, mỗi lần vài phút), cộng thêm phiên câu cá chơi tay lâu hơn.
- **Art:** pixel art 16×16, asset pack Sunnyside World (cây trồng tạm dùng của pack, sẽ vẽ bộ cây Việt cùng phong cách). Font pixel có dấu tiếng Việt.
- **Tham khảo:** cách sắp xếp nông trại kiểu Avatar (Teamobi); cơ chế câu cá chung của thể loại (Stardew Valley, Tiny Fishing, game nhịp điệu). Chỉ mượn cơ chế, không lấy art.

## 2. Vòng chơi chính
**Trồng → thu hoạch → giao đơn hoặc bán → xu + XP → lên cấp → mở cây mới / khu đất mới → đơn khó hơn.**
Song song: **đi câu cá** (phần chơi tay, bù cho trồng trọt vốn là chờ) → bán cá lấy xu, cộng XP chung.

Nguyên tắc: thời gian chờ cây lớn là cố ý (cây ngắn cho lúc đang chơi, cây dài cho lúc vắng mặt); chỉ thêm việc nhỏ mỗi lần vào game và lý do để quay lại, không lấp kín thời gian chờ.

## 3. Nông trại
- **Bản đồ:** 44×26 tile, trải ngang (~3,5 màn hình), hàng rào bao quanh. Có: 3 khu trồng, chuồng, nhà kho, đất mở rộng (3 cái này chỉ là cảnh, chạm hiện "sắp có"), ao nhà, cây, đống rơm, cổng trái dẫn ra vùng câu.
- **Khu trồng:** mỗi khu = 2 luống 6×2 (24 ô). Mở từng ô bằng xu, lần lượt. Khu 1 mở sẵn 6 ô; Khu 2 (cần cấp 5) và Khu 3 (cần cấp 9) khóa tới khi khu trước mở hết, khi mở khóa tặng 1 ô. Giá ô tăng dần; Khu 3 tổng ~614.000 xu.
- **Camera:** pixel perfect, chỉ zoom bậc nguyên. Kéo 1 ngón để di chuyển; chạm khu → zoom vào khu; chạm ngoài khu → về toàn cảnh. Không pinch zoom.
- **Nhân vật:** chạm để đi (tìm đường A*), không joystick. Gieo / tưới / thu hoạch được **xếp vào hàng việc**, nhân vật đi tới làm lần lượt đúng thứ tự người chơi chọn. Kéo ngón tay qua nhiều ô để gieo / tưới / thu hàng loạt. Nút Dừng để hủy hàng việc. Ẩn tab thì làm nốt việc còn lại rồi lưu.
- **Tạo nhân vật:** lần đầu chọn tên, 6 kiểu tóc, màu tóc; đổi lại qua nút "Nhân vật". Nhân vật vẽ nhiều lớp (thân, tóc, dụng cụ) để sau thêm trang phục.

## 4. Cây trồng & kinh tế
- Ô đất có các trạng thái: Khóa / Trống / Đang lớn; "chín" tính theo thời gian thực (cây vẫn lớn khi thoát game).
- **Tưới:** tùy chọn, 1 lần mỗi vụ, giảm 20% tổng thời gian lớn.
- Thu hoạch thỉnh thoảng đào được giun (10%, làm mồi câu).
- Data có 25 cây, MVP bật 4 cây. Mọi số tính theo công thức (lãi mỗi vụ = 5 × (T/2)^0,8; cây dài lãi/vụ cao hơn nhưng lãi/phút thấp hơn → mỗi cây hợp một khoảng vắng mặt):

| Cây | Thời gian | Giá hạt | Sản lượng | Giá bán | Lãi/vụ | XP | Cấp mở |
|---|---|---|---|---|---|---|---|
| Rau muống | 2 phút | 5 | 2 | 5 | 5 | 1 | 1 |
| Cải xanh | 15 phút | 15 | 2 | 20 | 25 | 4 | 2 |
| Cà chua | 1 giờ | 37 | 3 | 38 | 77 | 9 | 4 |
| Bí đỏ | 4 giờ | 92 | 1 | 322 | 230 | 22 | 6 |

- **Chống kẹt:** hết xu, hết hạt, hết nông sản, không cây nào đang lớn, không còn cá trong giỏ → tặng 1 hạt rau muống.

## 5. Tiến trình
- **Cấp độ:** tối đa 15 cấp trong MVP; XP lên cấp = 3 × cấp^3,2. XP từ thu hoạch, đơn hàng, câu cá. Lên cấp: hộp chúc mừng, thưởng xu + hạt, mở cây / cần / mồi mới.
- **Đơn hàng làng quê** (từ cấp 2): 5 đơn cùng lúc, mỗi đơn 1–3 loại nông sản; thưởng xu = giá bán × 1,6, XP ×2. Giao xong chờ 10 phút có đơn mới. 20% là đơn **Khẩn cấp** (thưởng ×2, hết hạn sau 30 phút, chờ 30 phút). Tối đa 15 đơn/ngày.
- **Nhịp mục tiêu** (ghé 4 lần/ngày): xong Khu 1 trong ngày đầu, Khu 2 ~ngày 3–4, Khu 3 ~ngày 10–14; cấp 15 ~ngày 9.

## 6. Câu cá (vùng câu riêng)
- Bấm **Đi câu** → nhân vật đi ra cổng → chuyển cảnh màn tối → vùng câu (camera đi theo nhân vật). Chạm chỗ câu để tới nơi câu. **Về nhà** để quay lại.
- **Vòng câu:**
  1. **Ném cần:** giữ nút, thanh lực chạy lên xuống, thả đúng vùng Perfect / Tốt / Thường. Perfect → cá hiếm hơn, chờ ngắn hơn, chuỗi kéo ngắn hơn.
  2. **Chờ cắn:** 2–8 giây, phao rỉa nhẹ (nhử) rồi chìm hẳn + dấu "!" → bấm **GIẬT!** trong khung phản xạ (1,6 → 0,8 giây tùy độ hiếm). Giật sớm: cá sợ, mất mồi. Trễ: cá ăn mất mồi. Có tỉ lệ trượt lưỡi.
  3. **Kéo cá:** hiện 1 chuỗi mũi tên 4 hướng (4–10 mũi tên tùy độ hiếm) + thanh thời gian; bấm 4 nút chữ thập. Bấm sai → nhập lại từ đầu (thời gian vẫn chạy). Xong nhanh = Perfect (+50% XP, cá nặng hơn). Cá huyền thoại có mũi tên đỏ = bấm hướng ngược lại. Hết giờ = cá thoát.
  4. **Được cá:** cá bay dọc dây về tay, bảng kết quả (tên, độ hiếm, cân nặng, giá, "Loài mới!", "Kỷ lục mới!").
- **Cá:** 13 loài, 5 độ hiếm (Thường / Khá / Hiếm / Quý / Huyền thoại): cá rô, diếc, tép, chép, trê, mè, lóc, lươn, tôm càng, bống tượng, chép đỏ, chép vàng, trắm đen khổng lồ. Mỗi loài có cân nặng ngẫu nhiên (giá theo cân), độ sâu, mồi ưa thích, **khung giờ xuất hiện** theo giờ thật của máy (7 khung: Bình minh, Sáng, Trưa, Chiều, Hoàng hôn, Tối, Khuya; vd. chép vàng chỉ bình minh, trắm đen chỉ khuya).
- **Cần câu:** 10 bậc (tre → nứa → trúc → mây → thủy tinh → composite → carbon → titan → vàng → rồng), mở theo cấp, giá 0 → 40.000 xu. Bậc cao: vùng Perfect rộng hơn, thêm thời gian kéo, ít trượt, nhiều cá hiếm, bền hơn. Mỗi lần ném −1 độ bền; hỏng thì sửa (mỗi lần sửa giảm 10% độ bền tối đa) hoặc mua lại.
- **Mồi:** 10 loại (giun – không bán, đào khi thu hoạch; cám, ruồi, mồi tép, dế, nhái, mồi giả, mồi thơm, nhái hơi, thìa vàng), mở theo cấp; mỗi mồi hút loài khác nhau, mồi đắt tăng tỉ lệ cá hiếm. Hết mồi = nghỉ câu (giới hạn tự nhiên, không có thanh năng lượng).
- **Giỏ cá:** mở sẵn 8 ô, mua thêm từng ô tới 25. Bán từng con hoặc Bán hết, hoặc thả.
- **Ao nhà** (chưa làm): thả cá sưu tầm, cá hiện bằng bóng bơi, chạm xem danh sách. Không tạo thu nhập.

## 7. Giao diện
- HUD: xu, cấp + thanh XP, đồng hồ (giờ + khung giờ câu cá), nút Sao lưu, nút Nhân vật.
- Tab: Đơn hàng / Kho (Nông sản | Giỏ cá) / Cửa hàng (Hạt giống | Cần câu | Mồi câu).
- Ở vùng câu HUD đổi: nút Về nhà, 3 nút Cần / Giỏ / Mồi.
- Lưu tự động (khi có thay đổi + khi ẩn tab), xuất / nhập **mã sao lưu** (vì dữ liệu trình duyệt có thể bị xóa).

## 8. Tình trạng hiện tại (19/11/2026)
**Đã có, chạy trong Unity Editor:** logic nông trại + ~450 unit test, data, camera, ô đất & cây + hoạt ảnh, UI, cấp độ, đơn hàng (Thường / Khẩn cấp / giới hạn ngày), lưu + mã sao lưu, nhân vật chạm để đi + hàng việc, tạo nhân vật, vùng câu, toàn bộ vòng câu cá, cần / mồi / giỏ / bán cá.

**Chưa làm / đang chờ:**
- Album cá + kỷ lục, đơn hàng có cá, ao nhà.
- Phần "việc làm lúc chờ cây lớn": báo cây chín trên tiêu đề tab trình duyệt; chuyện bất ngờ trên ruộng (chim, sâu → chạm đuổi; hộp quà rơi ngẫu nhiên); nông sản vàng hiếm (3%) + album; nhiệm vụ ngày + điểm danh 7 ngày; khách vãng lai hỏi mua nông sản giá cao.
- Vẽ lại map bằng Sunnyside, bộ cây Việt, art cá / mồi thật, âm thanh.
- Build web, thử trên điện thoại thật, giảm dung lượng (hiện 10,8 MB nén, mục tiêu < 10 MB).
- Cân bằng câu cá so với trồng trọt (chưa mô phỏng).

**Sau MVP:** chế biến món ăn, trang trí nông trại, cửa hàng trang phục / phụ kiện, thêm vùng câu mới (mỗi vùng bộ cá riêng).

**Ngoài phạm vi (đã quyết không làm):** nuôi cá lấy thu nhập, chuồng nuôi, đất mở rộng, joystick, trộm nông sản, hai loại tiền, thuốc trừ sâu / phân bón.

## 9. Giới hạn kỹ thuật cần nhớ khi đề xuất
- Chạy trên trình duyệt điện thoại: dung lượng build < 10 MB, bộ nhớ iOS hạn chế, tải nhanh.
- Không có server: không có tính năng online / bạn bè / bảng xếp hạng thật; chống chỉnh giờ chỉ ở mức chặn lùi giờ.
- Tab bị ẩn thì game gần như dừng; mọi thứ theo thời gian phải tính bằng timestamp.
- Art pixel 16×16 phong cách Sunnyside; mỗi tính năng mới cần art thì tốn công vẽ.
- Một người làm, nên ưu tiên ý tưởng ít art, ít code mà tăng lý do quay lại.

## 10. Muốn được góp ý
1. Vòng chơi đã đủ hấp dẫn để giữ người chơi 2 tuần đầu chưa? Chỗ nào nhàm / hụt mục tiêu?
2. Cân bằng: nhịp lên cấp, giá mở ô, đơn hàng, thu nhập câu cá so với trồng trọt.
3. Ý tưởng tăng lý do quay lại mỗi ngày mà vẫn chơi được không cần server.
4. Câu cá: cảm giác ném / giật / kéo có gì cải thiện được (đặc biệt trên màn cảm ứng)?
5. Chỗ tiêu xu sau khi mở hết đất (cuối game MVP).
6. Bản sắc "làng quê Việt": thêm chi tiết nào để game có hồn hơn (cây, cá, nhân vật, sự kiện, lễ Tết…).
