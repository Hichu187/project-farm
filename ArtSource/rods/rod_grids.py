"""Bộ icon cần câu "Về Quê" – pixel art 16x16, palette ENDESGA 32.

Cách dùng:  python3 rod_grids.py [thu_muc_xuat]
- Mỗi cần là 1 lưới 16x16 ký tự ('.' = trong suốt; dòng ngắn hơn 16 tự đệm '.').
- Dáng chung: cần nằm chéo, cán dưới-trái -> ngọn trên-phải, dây câu cong nhẹ thả từ ngọn.
- Viền #181425 tự thêm vào ô trong suốt kề 4 hướng với pixel đã vẽ.
- Ký tự OVERLAY vẽ SAU bước viền, không sinh viền:
    w = dây câu (#181425), a = chi tiết mảnh màu riêng, * + = lấp lánh trắng/vàng.
- Sửa màu: đổi trong dict 'pal' của từng cần (chỉ dùng mã trong ENDESGA32).
- Chỉ số game (stats) nằm trong dict 'stats' -> ghi vào rod_sheet.json, không ảnh hưởng hình.
"""
import json, os, sys
from PIL import Image

ENDESGA32 = """be4a2f d77643 ead4aa e4a672 b86f50 733e39 3e2731 a22633 e43b44 f77622 feae34
fee761 63c74d 3e8948 265c42 193c3e 124e89 0099db 2ce8f5 ffffff c0cbdc 8b9bb4 5a6988
3a4466 262b44 181425 ff0044 68386c b55088 f6757a e8b796 c28569""".split()
OUTLINE = "181425"
OVERLAY = set("ew*+a")
BASE = {"e": "181425", "w": "181425", "*": "ffffff", "+": "fee761"}

# Chỉ số game – điền theo bảng thiết kế (None = chưa có).
STAT_KEYS = ["perfect_zone", "power_cycle", "reel_time", "slip_reduction",
             "rare_fish_bonus", "durability", "price", "unlock_level", "repair_price"]

RODS = []


def rod(no, slug, vi, en, pal, grid, stats=None):
    assert len(grid) == 16 and all(len(r) <= 16 for r in grid), slug
    RODS.append(dict(no=no, slug=slug, vi=vi, en=en, pal=pal,
                     stats={k: (stats or {}).get(k) for k in STAT_KEYS},
                     grid=[r.ljust(16, ".") for r in grid]))


# ---------------- KHUNG CHUNG (theo ảnh mẫu ui_icon_rod.png) ----------------
# Thân chéo 2–3 px: L sáng (trên-trái) / M giữa / D tối (dưới-phải), ngọn ở (13,2).
# Dây câu F/f (#c0cbdc/#8b9bb4) CÓ viền: từ ngọn chạy xuống cột 12, vòng về máy (x5–7, y10–12)
#   hoặc, với cần không máy, vòng về buộc vào cán ở hàng 11.
# 'w' ở (14,1) (14,3) (13,4) = viền bo mũi ngọn như ảnh mẫu.
LINE = dict(F="c0cbdc", f="8b9bb4")

# ---------------- ROW 1 ----------------
# 1 Cần tre – tre kem vàng xỉn, đốt rõ (n sáng + N tối), cán buộc dây gai, không máy (dây buộc vào cán)
rod(1, "can_tre", "Cần tre", "bamboo rod",
    dict(LINE, L="ead4aa", M="e4a672", D="b86f50", n="b86f50", N="733e39", T="b86f50", t="733e39"), [
"",
"..............w",
".............L",
"...........LM.w",
".........nNN.w",
"........LMM.f",
".......LMD..F",
"......nNN...F",
".....LMD....F",
"....LMD....F",
"...nNN....F",
"..TTtFFFFf",
".LMD",
".TTt",
"",
"",
])

# 2 Cần nứa – nứa xám ánh xanh rêu, thân mảnh (ngọn 1 px), đốt thưa, 1 vòng dây buộc
rod(2, "can_nua", "Cần nứa", "thin bamboo rod",
    dict(LINE, L="8b9bb4", M="5a6988", G="3e8948", N="3a4466", T="e4a672", t="b86f50"), [
"",
"..............w",
".............L",
"............L",
"...........L",
"..........L.f",
".........LG.F",
"........NN..F",
".......LM...F",
"......LG...F",
".....LM...F",
"....NNFFFf",
"..TTt",
"..LG",
".NN",
"",
])

# 3 Cần trúc – trúc nâu vàng, đốt dày (2 hàng 1 đốt), 2 khoen bạc (K, dây xỏ qua khoen ngọn), cán quấn vải
rod(3, "can_truc", "Cần trúc", "golden bamboo rod",
    dict(LINE, L="e4a672", M="b86f50", D="733e39", n="c28569", N="3e2731", K="c0cbdc",
         C="3e2731", c="262b44"), [
"",
"..............w",
".............L",
"...........LM.w",
".........nNNKw",
"........LMM.f",
".......nNN..F",
"......LMDK..F",
".....nNN....F",
"....LMD....F",
"...CCc....F",
"..CCcFFFFf",
".CCc",
".MD",
"",
"",
])

# 4 Cần mây – mây nâu đỏ, mắt mây gồ sáng (n), cán quấn sợi mây sọc vàng/nâu, 2 khoen
rod(4, "can_may", "Cần mây", "rattan rod",
    dict(LINE, L="be4a2f", M="733e39", D="3e2731", n="d77643", K="c0cbdc",
         W="e4a672", V="b86f50", v="733e39"), [
"",
"..............w",
".............L",
"...........LM.w",
".........LLMKw",
"........nMM.f",
".......LMDK.F",
"......LMD...F",
".....nMD....F",
"....WWV....F",
"...VVv....F",
"..WWVFFFFf",
".VVv",
".WV",
"",
"",
])

# 5 Cần sợi thủy tinh – trắng sữa ánh xanh ngọc, khoen xanh, cán xốp xám, máy câu nhỏ 2x2
rod(5, "can_thuy_tinh", "Cần sợi thủy tinh", "fiberglass rod",
    dict(LINE, L="c0cbdc", M="0099db", D="124e89", T="2ce8f5", P="5a6988", p="3a4466",
         R="0099db", r="124e89", g="262b44"), [
"",
"..............w",
".............L",
"...........LM.w",
".........LLMTw",
"........LMM.f",
".......LMD..F",
"......LMDT..F",
".....LMD....F",
"....L......F",
"...P.RrfffF",
"..PP.rg",
".PPp",
".TT",
"",
"",
])

# ---------------- ROW 2 ----------------
# 6 Cần composite – xanh dương đậm, cán cao su đen, máy kim loại 3x3 (như ảnh mẫu), 2 khoen bạc
rod(6, "can_composite", "Cần composite", "composite rod",
    dict(LINE, L="0099db", M="124e89", D="262b44", K="c0cbdc", G="3a4466", g="262b44",
         R="c0cbdc", r="8b9bb4", s="5a6988"), [
"",
"..............w",
".............L",
"...........LM.w",
".........LLMKw",
"........LMM.f",
".......LMD..F",
"......LMDK..F",
".....LMD....F",
"....G...w..F",
"...G.RRR.fF",
"..Gg.Rsr",
".GGg.Rrr",
".gg",
"",
"",
])

# 7 Cần carbon – đen than vân carbon (ô cờ 2 tông), khoen bạc, vòng đỏ ở cổ cán, máy to 4x3 có tay quay
rod(7, "can_carbon", "Cần carbon", "carbon rod",
    dict(LINE, L="5a6988", M="3a4466", D="262b44", K="c0cbdc", X="e43b44", G="3a4466", g="262b44",
         R="c0cbdc", r="8b9bb4", s="5a6988", a="c0cbdc"), [
"",
"..............w",
".............L",
"...........LD.w",
".........LMDKw",
"........LDM.f",
".......LMDK.F",
"......LDM...F",
".....LMDK...F",
"....X......F",
"...G.RRRRfF",
"..Gg.RssrF",
".GGg.Rrrr.",
".gg....aa",
"",
"",
])

# 8 Cần titan – xám bạc ánh xanh, khoen xanh ngọc sáng, máy thép tối viền ngọc, 1 pixel trắng ở thân
rod(8, "can_titan", "Cần titan", "titanium rod",
    dict(LINE, L="c0cbdc", M="8b9bb4", D="3a4466", C="2ce8f5", W="ffffff", G="124e89", g="262b44",
         R="2ce8f5", r="5a6988", s="262b44", a="2ce8f5"), [
"",
"..............w",
".............C",
"...........LM.w",
".........WLMCw",
"........LMD.f",
".......LCM..F",
"......LMDC..F",
".....LMD....F",
"....C......F",
"...G.RRRRfF",
"..Gg.RssrF",
".GGg.rrrr.",
".CC....aa",
"",
"",
])

# 9 Cần vàng – vàng óng (fee761→feae34→f77622), cán đỏ, máy vàng đỏ, 1 trắng + 3 lấp lánh
rod(9, "can_vang", "Cần vàng", "golden rod",
    dict(LINE, H="fee761", L="feae34", M="f77622", D="be4a2f", W="ffffff", K="fee761",
         G="e43b44", g="a22633", R="fee761", r="feae34", s="be4a2f", a="fee761"), [
"",
"..+...........w",
".............H",
"...........WL.w",
".........HLMKw",
"........HLM.f",
".......HLMD.F",
"......HLMDK.F",
".....HLMD...F.*",
"....H......F",
"...G.RRRRfF",
"..Gg.RssrF",
".GGg.rrrr.",
".HH....aa.....+",
"",
"",
])

# 10 Cần rồng – thân đỏ vảy ngọc lục bảo, cán chạm đầu rồng ngọc (sừng vàng, mắt), máy ngọc, 1 trắng + 3 lấp lánh
rod(10, "can_rong", "Cần rồng", "dragon rod",
    dict(LINE, L="e43b44", M="a22633", D="68386c", J="63c74d", j="3e8948", W="ffffff", K="fee761",
         G="3e8948", H="63c74d", g="265c42", Y="fee761", T="e43b44",
         R="e43b44", r="a22633", s="68386c", a="fee761"), [
"",
"..*...........w",
".............L",
"...........WM.w",
".........LJMKw",
"........LMj.f",
".......JMD..F",
"......LjD...F",
".....JMD....F.+",
"...GL......F",
"..YH.RRRRfF",
".YHG.RssrF",
".GeG.rrrr.",
"..GT...aa....*",
"...T",
"",
])

def hexrgb(h):
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4)) + (255,)


def render(f):
    g, pal = f["grid"], {**BASE, **f["pal"]}
    assert len(g) == 16 and all(len(r) == 16 for r in g), f["slug"]
    for c in set("".join(g)) - {"."}:
        assert c in pal, (f["slug"], c)
        assert pal[c].lower() in ENDESGA32, (f["slug"], c, pal[c])
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    solid = lambda x, y: 0 <= x < 16 and 0 <= y < 16 and g[y][x] != "." and g[y][x] not in OVERLAY
    for y in range(16):
        for x in range(16):
            c = g[y][x]
            if c != "." and c not in OVERLAY:
                img.putpixel((x, y), hexrgb(pal[c]))
            elif any(solid(x + dx, y + dy) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                img.putpixel((x, y), hexrgb(OUTLINE))
    for y in range(16):
        for x in range(16):
            if g[y][x] in OVERLAY:
                img.putpixel((x, y), hexrgb(pal[g[y][x]]))
    return img


def preview(items, path, scale=10, gap=4):
    bg = (0x96, 0xcd, 0xcd, 255)
    n = len(items)
    W = n * 16 * scale + (n + 1) * gap * scale // 2
    H = 16 * scale + 16 + 3 * gap * scale // 2 + 16
    im = Image.new("RGBA", (W, H), bg)
    for i, f in enumerate(items):
        x = gap * scale // 2 + i * (16 * scale + gap * scale // 2)
        big = render(f).resize((16 * scale, 16 * scale), Image.NEAREST)
        im.alpha_composite(big, (x, gap * scale // 2))
        im.alpha_composite(render(f), (x + 72, 16 * scale + gap * scale))
    im.save(path)


def export(out):
    os.makedirs(out, exist_ok=True)
    for f in RODS:
        render(f).save(os.path.join(out, f"rod_{f['no']:02d}_{f['slug']}.png"))
    rows = (len(RODS) + 4) // 5
    sheet = Image.new("RGBA", (80, 16 * rows), (0, 0, 0, 0))
    for i, f in enumerate(RODS):
        sheet.alpha_composite(render(f), ((i % 5) * 16, (i // 5) * 16))
    sheet.save(os.path.join(out, "rod_sheet.png"))
    meta = dict(cell=16, columns=5, rows=rows, rods=[
        dict(index=i, rank=f["no"], id=f["slug"], file=f"rod_{f['no']:02d}_{f['slug']}.png",
             name_vi=f["vi"], name_en=f["en"], col=i % 5, row=i // 5, **f["stats"])
        for i, f in enumerate(RODS)])
    with open(os.path.join(out, "rod_sheet.json"), "w", encoding="utf-8") as fp:
        json.dump(meta, fp, ensure_ascii=False, indent=2)
    with open(os.path.join(out, "to_aseprite.bat"), "w", encoding="utf-8", newline="\r\n") as fp:
        fp.write('@echo off\nset ASE=C:\\aseprite\\build\\bin\\aseprite.exe\ncd /d "%~dp0"\n'
                 'for %%f in (rod_*.png) do (\n  echo %%f\n  "%ASE%" -b "%%f" --save-as "%%~nf.aseprite"\n)\n'
                 'echo Xong.\npause\n')


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else "out"
    export(out)
    for r in range(0, len(RODS), 5):
        preview(RODS[r:r + 5], os.path.join(out, f"preview_row{r // 5 + 1}.png"))
