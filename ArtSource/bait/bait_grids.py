"""Bộ icon mồi câu "Về Quê" – pixel art 16x16, palette ENDESGA 32.

Cách dùng:  python3 bait_grids.py [thu_muc_xuat]
- Mỗi mồi là 1 lưới 16x16 ký tự ('.' = trong suốt; dòng ngắn hơn 16 tự đệm '.').
- Viền #181425 tự thêm vào ô trong suốt kề 4 hướng với pixel đã vẽ.
- Ký tự OVERLAY (e = mắt, w = râu/chân mảnh, a = râu màu riêng, * + = lấp lánh) vẽ SAU bước viền, không sinh viền.
- Sửa màu: đổi trong dict 'pal' của từng mồi (chỉ dùng mã trong ENDESGA32).
"""
import json, os, sys
from PIL import Image

ENDESGA32 = """be4a2f d77643 ead4aa e4a672 b86f50 733e39 3e2731 a22633 e43b44 f77622 feae34
fee761 63c74d 3e8948 265c42 193c3e 124e89 0099db 2ce8f5 ffffff c0cbdc 8b9bb4 5a6988
3a4466 262b44 181425 ff0044 68386c b55088 f6757a e8b796 c28569""".split()
OUTLINE = "181425"
OVERLAY = set("ew*+a")
BASE = {"e": "181425", "w": "181425", "*": "ffffff", "+": "fee761"}

BAIT = []

def bait(no, slug, vi, en, rarity, pal, grid):
    assert len(grid) == 16 and all(len(r) <= 16 for r in grid), slug
    BAIT.append(dict(no=no, slug=slug, vi=vi, en=en, rarity=rarity, pal=pal,
                     grid=[r.ljust(16, ".") for r in grid]))

# ---------------- ROW 1 ----------------
# 1 Giun đất – thân dài uốn chữ "2" (đầu trên-trái), đai sinh dục cam gần đầu
bait(1, "giun_dat", "Giun đất", "earthworm", "common",
     dict(H="e8b796", L="f6757a", M="b55088", D="68386c", C="e4a672", c="c28569"), [
"",
"..LLHC",
".LHMMCcL",
".DMMMMccL",
"..DDDMMMMD",
".....DLMMD",
"......LHMD",
"......DMDD",
"......LHMD",
"......LHMD",
".......LHMD",
".......DMDD",
".......LMHHLLL",
"........DDMMMMD",
"..........DDDD",
"",
])

# 2 Cám – bát sứ xanh trắng đựng cám vàng lốm đốm
bait(2, "cam", "Cám", "rice bran", "common",
     dict(Y="feae34", O="e4a672", P="b86f50", W="ffffff", A="c0cbdc", B="8b9bb4",
          C="5a6988", U="0099db", u="124e89"), [
"",
"",
"",
"",
"......YYY",
"....YYYOYYO",
"...YYOYYOYOO",
"..YOYYOOYOOPO",
".AYOOYOPOOPOPA",
".WAAAAAAAAABBC",
".BAAAAAAAAABBC",
"..UUUUUUUUUUu",
"..BAAAAAABBBC",
"...CBBBBBBBC",
".....CCCCC",
"",
])

# 3 Bột – 3 viên bột nặn xếp tháp
bait(3, "bot", "Bột", "dough ball", "common",
     dict(W="ffffff", K="ead4aa", J="e8b796", G="c28569", N="b86f50"), [
"",
".....KKKJ",
"....KWKKJJ",
"....KKKJJG",
"....KJJJJG",
"....JJJJGN",
".....JGGN",
"",
".KKKJ...KKKJ",
"KKKKJJ.KKKKJJ",
"KKKJJG.KKKJJG",
"KJJJJG.KJJJJG",
"JJJJGN.JJJJGN",
".JGGN...JGGN",
"",
"",
])

# 4 Ruồi – nhìn từ trên, mắt đỏ, bụng xanh ánh kim, cánh xám trong
bait(4, "ruoi", "Ruồi", "fly", "uncommon",
     dict(R="e43b44", r="a22633", T="3a4466", t="262b44", X="0099db", Z="124e89", z="262b44",
          V="c0cbdc", v="8b9bb4", W="ffffff"), [
"",
"",
"......RttR",
".....RRttRr",
".....rRttrr",
"......TTtt",
"....V.TTtt.v",
"...VWVTTttVvv",
"..VVVVXZZzVVvv",
".VVVVvXZZzvVVvv",
".VVVvvXZZzvvVvv",
".VVvv.XZZz.vvVv",
"..vv..XZZz..vv",
"......XZzz",
".......zz",
"",
])

# 5 Dế – nhìn ngang, đầu trái, đùi sau to, râu dài
bait(5, "de", "Dế", "cricket", "uncommon",
     dict(E="b86f50", W="ffffff", B="733e39", M="3e2731", D="262b44", F="e4a672", f="b86f50", g="733e39"), [
"",
"",
"",
".w",
"..w.........FF",
"...w.EEEEEEFFf",
"..EWBEBBBBFFfMf",
".EeBBMBBBFFfMMf",
".EEBBMMMFFfMMMf",
"..BBMMDFFfDDDDf",
"...DD.DffDDDD.f",
"...g...g.....g",
"",
"",
"",
"",
])

# ---------------- ROW 2 ----------------
# 6 Tép – tép duỗi thẳng nằm chéo (khác icon tép cuộn bên bộ cá), đuôi quạt cam
bait(6, "tep", "Tép", "freshwater shrimp", "uncommon",
     dict(H="ead4aa", L="e8b796", M="e4a672", S="c28569", D="b86f50", T="f77622", t="be4a2f", a="c28569"), [
"",
"a",
".a.a",
"..a.a..LLLD",
"...aaLLHSHSLD",
"..LLLHHMSMSHSD",
".LLHHMMMSMSMSD",
"..DeMMMMSDDMSMD",
"...DMMMDD..DSMD",
"....DDD.a...LMD",
"....a.a.....LMD",
"..........TTtTT",
"..........Tt.Tt",
"",
"",
"",
])

# 7 Nhái – nhái con ngồi nhìn thẳng, mắt lồi, bụng kem
bait(7, "nhai", "Nhái", "frog", "rare",
     dict(H="fee761", L="63c74d", M="3e8948", D="265c42", B="ead4aa", b="e4a672"), [
"",
"",
"",
"",
"....LL....LM",
"...LeHL..LeMD",
"...LLLDLLDLMD",
"..LwwwwwwwwMD",
"..LLMMMMMMMMD",
"...MBBBBBBMD",
"..LMBBBBBbMDD",
"..LLMDbbbMDMD",
"..LL.MD.MD.DD",
"",
"",
"",
])

# 8 Mồi giả cá nhỏ (minnow) – lưng xanh, sườn bạc, lưỡi gà phía trước, lưỡi câu treo dưới bụng
bait(8, "moi_gia_ca", "Mồi giả cá nhỏ", "minnow lure", "rare",
     dict(X="0099db", Z="124e89", u="262b44", H="2ce8f5", A="c0cbdc", n="8b9bb4", Y="fee761",
          P="8b9bb4", T="124e89", t="262b44", a="5a6988"), [
"",
"",
"....XXZ",
"...XHXZZu",
"..YYHXXZZu",
".YeYYHXXZZu",
".PYYAAHXXZu",
"PP.AAAAHXZZu",
"....nAAAHXZu",
".....nAAAHZu",
"...a..nAAAZuT",
"..a.a..nnAuTTt",
"...a.....uTtt",
"....a...TTt",
"........a.a",
".........a",
])

# 9 Mồi giả nhái hơi – nhái nhựa vàng cam, chân váy silicon đỏ, 2 lưỡi câu cong lên
bait(9, "nhai_hoi", "Mồi giả nhái hơi", "frog lure", "precious",
     dict(Y="fee761", W="ffffff", O="feae34", F="f77622", d="be4a2f", K="3e8948", R="e43b44", r="a22633", a="8b9bb4"), [
".......a",
"...YYY.a.YOF",
"..YWeYYYYYeOF",
"..YYYYYYYOOFF",
"...YYYYYOOOF",
"...YYKYOOKOF",
".a.YKYYOOOKF.a",
".a.YYYOOKOOF.a",
"..a.YOOOOOF.a",
".....FdFdF",
"....R.r.R.r",
"...R..r.R..r",
"...R.r...R.r",
"",
"",
"",
])

# 10 Mồi thìa vàng – thìa kim loại vàng nằm chéo, khoen + lưỡi câu 3 chạc, lấp lánh legendary
bait(10, "thia_vang", "Mồi thìa vàng", "golden spoon lure", "legendary",
     dict(Y="feae34", H="fee761", W="ffffff", O="f77622", F="be4a2f", d="733e39", R="e43b44", a="8b9bb4"), [
".a.........+",
"a.a",
".a.Y",
"...YYO.......*",
"...YWYOF",
"...YHYYOF",
"...YYHYYOF",
"....YYHROOF",
"....OYYHYOFd",
".....OYYHOFd",
"......OOYOFFd",
".+.....FOOFdd",
"........FFdd",
"..........a",
".........a.a.a",
"..........aaa",
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
    solid = lambda x, y: 0 <= x < 16 and 0 <= y < 16 and g[y][x] not in "." and g[y][x] not in OVERLAY
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


def preview(baits, path, scale=10, gap=4):
    bg = (0x96, 0xcd, 0xcd, 255)
    n = len(baits)
    W = n * 16 * scale + (n + 1) * gap * scale // 2
    H = 16 * scale + 16 + 3 * gap * scale // 2 + 16
    im = Image.new("RGBA", (W, H), bg)
    for i, f in enumerate(baits):
        x = gap * scale // 2 + i * (16 * scale + gap * scale // 2)
        big = render(f).resize((16 * scale, 16 * scale), Image.NEAREST)
        im.alpha_composite(big, (x, gap * scale // 2))
        im.alpha_composite(render(f), (x + 72, 16 * scale + gap * scale))
    im.save(path)


def export(out):
    os.makedirs(out, exist_ok=True)
    for f in BAIT:
        render(f).save(os.path.join(out, f"bait_{f['no']:02d}_{f['slug']}.png"))
    rows = (len(BAIT) + 4) // 5
    sheet = Image.new("RGBA", (80, 16 * rows), (0, 0, 0, 0))
    for i, f in enumerate(BAIT):
        sheet.alpha_composite(render(f), ((i % 5) * 16, (i // 5) * 16))
    sheet.save(os.path.join(out, "bait_sheet.png"))
    meta = dict(cell=16, columns=5, rows=rows, bait=[
        dict(index=i, no=f["no"], id=f"bait_{f['no']:02d}_{f['slug']}", name_vi=f["vi"],
             name_en=f["en"], rarity=f["rarity"], col=i % 5, row=i // 5) for i, f in enumerate(BAIT)])
    with open(os.path.join(out, "bait_sheet.json"), "w", encoding="utf-8") as fp:
        json.dump(meta, fp, ensure_ascii=False, indent=2)
    with open(os.path.join(out, "to_aseprite.bat"), "w", encoding="utf-8", newline="\r\n") as fp:
        fp.write('@echo off\r\nset ASE=C:\\aseprite\\build\\bin\\aseprite.exe\r\ncd /d "%~dp0"\r\n'
                 'for %%f in (bait_*.png) do (\r\n  echo %%f\r\n  "%ASE%" -b "%%f" --save-as "%%~nf.aseprite"\r\n)\r\n'
                 'echo Xong.\r\npause\r\n')


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else "out"
    export(out)
    for r in range(0, len(BAIT), 5):
        preview(BAIT[r:r + 5], os.path.join(out, f"preview_row{r // 5 + 1}.png"))
