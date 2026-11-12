# Về Quê – Art cần cho Phần 10 (cập nhật 12/11/2026: chuyển sang Sunnyside World)

Ký hiệu: ✅ đã dùng · 🟡 có trong gói, chưa dùng · ❌ thiếu, cần vẽ / tìm thêm (cùng phong cách Sunnyside).
Gốc gói: `Assets/Art/Sunnyside_World_ASSET_PACK_V2.1/Sunnyside_World_Assets/` (viết tắt `SS/`).
Mỗi lần thêm art: build lại, so Build Report (mốc 10,8 MB đã nén, mục tiêu < 10 MB). Tileset 1024×1024 không nén ~4 MB → chỉ đưa vào Sprite Atlas các tile dùng tới.
Giấy phép: gói không kèm file license → người dùng xem trang mua (Daniel Diggle, itch.io).

## Nhân vật (Phần 3b bước 1, Phần 10 giai đoạn 1)
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| Đứng / đi | 🟡 | `SS/Characters/Human/IDLE` (9 khung), `WALKING` (8) – strip 96×64, 2 hướng (lật trái) |
| 3 lớp: thân, tóc, dụng cụ | 🟡 | `base_*`, `shorthair_*` (mặc định; còn bowl/curly/long/mop/spikey), `tools_*` |
| Gieo / thu hoạch | 🟡 | `DOING` (8); có thêm `DIG` (13), `CARRY` (8) |
| Tưới | 🟡 | `WATERING` (5) |
| Hướng lên / xuống | ❌ | Gói chỉ có nhìn ngang – chấp nhận (giữ hướng ngang gần nhất) |

## Vùng câu (Phần 10 giai đoạn 2)
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| Nước, bờ, vách, bến / cầu gỗ | 🟡 | `SS/Tileset/spr_tileset_sunnysideworld_16px.png` |
| Thuyền thúng | 🟡 | `SS/Elements/Other/spr_deco_coracle_strip4.png`, `spr_deco_coracle_land.png` |
| Vịt, chim, cây, nấm | 🟡 | `SS/Elements/Animals`, `SS/Elements/Plants` |
| Biểu tượng chỗ câu | 🟡 | `SS/UI/rod.png` hoặc `SS/UI/indicator.png` |
| Biển tên vùng | ❌ | Không bắt buộc (chữ trên HUD) |

## Câu cá (Phần 10 giai đoạn 4)
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| Ném / chờ / kéo / được cá | 🟡 | `SS/Characters/Human/CASTING` (15), `WAITING` (9, có dây + phao), `REELING` (13), `CAUGHT` (10) |
| Dấu "!" khi cá cắn | 🟡 | `SS/UI/expression_alerted.png` |
| Mũi tên kéo cá + Chốt | 🟡 | `SS/UI/arrow_left/right/up.png` (xuống = lật lên), `confirm.png` |
| Thanh lực / độ căng / thể lực | 🟡 | `SS/UI/greenbar_*`, `redbar_*`, `bluebar_*`; khung `SS/UI/9slice_box_white` |
| Icon cần câu 5 bậc | 🟡 2 / ❌ 3 | `SS/UI/rod.png`, `rod alt.png`; thiếu 3 (đổi màu / vẽ thêm) |
| Icon cần gãy | ❌ | |
| Icon mồi: giun, cám, tép, mồi giả, mồi đặc biệt | ❌ | |

## Giỏ cá, album, đơn (Phần 10 giai đoạn 5)
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| **13 loài cá nước ngọt** | ❌ | rô, diếc, tép, chép, trê, mè, lóc, lươn, tôm càng xanh, bống tượng, chép đỏ, chép vàng, trắm đen. Gói chỉ có 1 cá `SS/Elements/Crops/fish.png` (11×11) – dùng tạm |
| Icon giỏ cá | 🟡 | `SS/UI/basket.png` |
| Đồng hồ / thời gian | 🟡 | `SS/UI/sandtimer.png`, `stopwatch.png` |
| Icon 7 khung giờ | ❌ | Không có mặt trời / trăng |
| Độ hiếm, "?" | ❌ / 🟡 | Tô màu bằng code hoặc vẽ thêm; "?" xem icon nhỏ trong tileset |

## Ao nhà (Phần 10 giai đoạn 6)
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| Mặt ao | 🟡 | Nước trong tileset |
| Bóng cá bơi | ❌ | Vẽ bằng code (bóng tối hình cá) hoặc vẽ thêm |

## Nông trại (Phần 3b bước 2–3)
| Thứ | Trạng thái | Nguồn / ghi chú |
|---|---|---|
| Cỏ, sân đất, hàng rào, cây, đá, nhà | 🟡 | Tileset 16 px |
| Ô đất thường / ướt / có lỗ | 🟡 | `SS/Elements/Crops/soil_00/01/03/04.png` (16×12) |
| 4 cây (tạm) | 🟡 | rau muống ← `kale`, cải xanh ← `cabbage`, cà chua ← `radish`, bí đỏ ← `pumpkin` (`_00` hạt, `_01`–`_04` giai đoạn, `_05` nông sản) |
| **Bộ cây Việt cùng phong cách** | ❌ | Người dùng tự vẽ (25 cây trong data; 4 cây MVP trước) |
| Gà, bò, lợn, cừu (chuồng "sắp có") | 🟡 | `SS/Elements/Animals` |
| Cối xay gió, khói ống khói | 🟡 | `SS/Elements/Other`, `SS/Elements/VFX` |

## Âm thanh (người dùng thêm sau)
❌ bước chân, gieo, tưới, quăng cần, cá cắn, giật, đứt dây, được cá. AudioClip: Decompress on Load + AAC; chỉ phát sau lần chạm đầu trên web.

## Cần vẽ / tìm thêm (theo thứ tự dùng)
1. Bộ cây Việt (4 cây MVP trước).
2. 5 icon mồi, 3 icon cần còn thiếu, icon cần gãy.
3. 13 sprite cá nước ngọt.
4. Không bắt buộc: icon khung giờ, bóng cá bơi, biển tên vùng.
