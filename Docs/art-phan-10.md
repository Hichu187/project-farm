# Về Quê – Art cần cho Phần 10 (lập 11/11/2026)

Ký hiệu: ✅ đã dùng · 🟡 có trong pack, chưa dùng (cần kiểm tra khung khi làm) · ❌ thiếu, cần thêm.
Đường dẫn tính từ `Assets/Art/`. Mỗi lần thêm art: build lại, so Build Report (mốc 10,8 MB đã nén, mục tiêu < 10 MB).

## Giai đoạn 1 – Nhân vật ở nông trại (đã làm)
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| Nhân vật đứng / đi 4 hướng | ✅ | `Sprout Lands - Sprites - Basic pack/Characters/Basic Charakter Spritesheet.png` |
| Cuốc, tưới 4 hướng | ✅ | `.../Characters/Basic Charakter Actions.png` (hàng 0–3 cuốc, 8–11 tưới) |
| Icon gợi ý tưới (bình tưới) | ✅ | `.../Objects/Basic_tools_and_meterials.png` ô trên-trái |
| Hoạt ảnh thu hoạch (cúi nhặt) | ❌ | Tạm dùng cuốc; không bắt buộc |
| Hoạt ảnh gieo hạt (rắc hạt) | ❌ | Tạm dùng cuốc; không bắt buộc |
| Dấu điểm đến, phủ ô đang chờ | – | Vẽ bằng code; có sprite đẹp thì thay |

## Giai đoạn 2 – Vùng câu + di chuyển
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| Nước động | 🟡 | `Sprout Lands - Sprites - Basic pack/Tilesets/Water.png` (4 khung 16 px) |
| Bờ nước / mép cỏ | 🟡 | `.../Tilesets/Grass.png`, `Hills.png` – xem đủ góc viền không |
| Bến câu | 🟡 | `.../Objects/Wood_Bridge.png` |
| Sen, lau sậy, cây ven nước | 🟡 | `Sprout Sorry pack/Early Access/Plant update 2/* water plants.png` |
| Cây, bụi, đá trang trí | 🟡 | `.../Plant update 2/Trees, stumps and bushes v2.png`, `Basic_Grass_Biom_things.png` |
| Cổng / lối ra nông trại | 🟡 | `.../Tilesets/Fences.png` |
| **Biểu tượng chỗ câu** | ❌ | Tạm: `Ocean Pack/fishing_rod.png` hoặc "!" trong `All Icons.png` |
| Biển tên vùng | ❌ | Không bắt buộc (có thể chỉ hiện chữ trên HUD) |

## Giai đoạn 4 – Chơi câu cá
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| Hoạt ảnh câu 3 hướng (trái = lật phải) | 🟡 | `Sprout Sorry pack/Early Access/Ocean Pack/fishing animation front/back/side.png` – khung 48×48, 6 hàng: ném ×2, chờ, giật/kéo, thu cần, giơ cá. Kiểm tra khớp nhân vật Basic |
| Phao + nước bắn | 🟡 | `.../Ocean Pack/fishing water splash frames and rod.png` |
| Dây câu | – | Vẽ bằng code |
| **Icon cần câu 5 bậc** | 🟡 2 / ❌ 3 | Có `fishing_rod.png`, `fishing_rod_2.png`; thiếu 3 (có thể đổi bảng màu) |
| Icon cần gãy | ❌ | |
| **Icon mồi: giun, cám, tép, mồi giả, mồi đặc biệt** | ❌ | Thiếu cả 5 |
| Dấu "!" khi cá cắn | 🟡 | `Sprout Lands - UI Pack - Basic pack/Sprite sheets/Icons/All Icons.png` |
| 4 nút mũi tên + nút Chốt | 🟡 | Mũi tên trong `All Icons.png`, nền `Sprite sheets/buttons/Square Buttons *.png`; mũi tên đỏ tô màu bằng code |
| Thanh lực, vạch nhịp, độ căng dây, thể lực cá | – / ❌ | Vẽ bằng UI; muốn đẹp thì cần khung thanh |
| "Perfect!", đứt dây | – | Chữ + hiệu ứng bằng code |

## Giai đoạn 5 – Giỏ cá, bán, album, đơn hàng
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| **Sprite 13 loài cá nước ngọt** | ❌ | rô, diếc, tép, chép, trê, mè, lóc, lươn, tôm càng xanh, bống tượng, chép đỏ, chép vàng, trắm đen. `Ocean Pack/Fish Sprites.png` chủ yếu sinh vật biển, chỉ tạm được vài con |
| Icon giỏ cá | ❌ | |
| Icon độ hiếm (5 mức) | 🟡 | Sao nhiều màu `Sprite sheets/Icons/special icons/Special Icons.png` |
| "?" loài chưa bắt | 🟡 | `All Icons.png` |
| Icon 7 khung giờ (bình minh → khuya) | 🟡 / ❌ | `emojis-free/emoji style ui/weather/Weather_Icons_smal_freel.png` – xem đủ mặt trời / trăng không |

## Giai đoạn 6 – Ao nhà
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| Bóng cá bơi | 🟡 | `Ocean Pack/small fish.png`, `mediuml fish.png`, `big fish 2 swimming in cirkels.png` (15 khung 16 px); tô màu theo độ hiếm bằng code |
| Mặt ao | 🟡 | Dùng chung tile nước |

## Âm thanh (người dùng thêm sau)
❌ bước chân, cuốc, tưới, quăng cần, cá cắn, giật, đứt dây, được cá. AudioClip: Decompress on Load + AAC; chỉ phát sau lần chạm đầu trên web.

## Cần thêm chắc chắn (theo thứ tự dùng)
1. Biểu tượng chỗ câu (giai đoạn 2).
2. 5 icon mồi, 3 icon cần còn thiếu, icon cần gãy (giai đoạn 4).
3. 13 sprite cá nước ngọt + icon giỏ cá (giai đoạn 5) – phần lớn nhất.
4. Không bắt buộc: thu hoạch / gieo hạt, khung thanh UI, biển tên vùng.

Ghi chú dung lượng: 3 sheet câu cá 1536×288 khá lớn – chỉ giữ các hàng dùng tới (cắt lại sheet hoặc gom vào Sprite Atlas chỉ phần cần).
