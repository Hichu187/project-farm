"""Bộ icon cá "Về Quê" – pixel art 16x16, palette ENDESGA 32.

Cách dùng:  python3 fish_grids.py [thu_muc_xuat]
- Mỗi con là 1 lưới 16x16 ký tự. '.' = trong suốt.
- Viền #181425 tự thêm vào ô trong suốt kề 4 hướng với pixel đã vẽ.
- Ký tự OVERLAY (e = mắt, w = râu, a = râu tôm, * + = lấp lánh) vẽ SAU bước viền, không sinh viền.
- Sửa màu: đổi trong dict 'pal' của từng con (chỉ dùng mã trong ENDESGA32).
"""
import json, os, sys
from PIL import Image

ENDESGA32 = """be4a2f d77643 ead4aa e4a672 b86f50 733e39 3e2731 a22633 e43b44 f77622 feae34
fee761 63c74d 3e8948 265c42 193c3e 124e89 0099db 2ce8f5 ffffff c0cbdc 8b9bb4 5a6988
3a4466 262b44 181425 ff0044 68386c b55088 f6757a e8b796 c28569""".split()
OUTLINE = "181425"
OVERLAY = set("ew*+a")   # a = râu tôm (màu theo từng con)
BASE = {"e": "181425", "w": "181425", "*": "ffffff", "+": "fee761"}

FISH = []

def fish(no, slug, vi, en, rarity, pal, grid):
    FISH.append(dict(no=no, slug=slug, vi=vi, en=en, rarity=rarity, pal=pal, grid=grid))

# ---------------- ROW 1: COMMON (màu xỉn, trầm) ----------------
# 1 Cá rô – lưới chuẩn cá rô v2, đổi màu sang xám-oliu xỉn
fish(1, "ca_ro", "Cá rô", "climbing perch", "common",
     dict(H="c28569", L="5a6988", M="265c42", D="3a4466", S="193c3e",
          B="c28569", b="b86f50", s="733e39", F="b86f50", f="733e39", g="3e2731"), [
"................",
"................",
"...MMD..g.......",
"..LHMMDfg.......",
"..LeHMMDgf......",
"..BLLHMSDDfg....",
"...BLLHSMDDg....",
"...BbLLHSMDD....",
"....sbLLLSMD....",
"...f.sbLLLSD....",
"...Ff.sbLLMD....",
".......sbLMD.FF.",
"........sbDDFffg",
".........sDFFfg.",
"..........FffFg.",
"...........ggg..",
])

# 2 Cá diếc – thân tròn cao, bạc ánh oliu, nghiêng nhẹ
fish(2, "ca_diec", "Cá diếc", "crucian carp", "common",
     dict(H="c0cbdc", L="8b9bb4", M="5a6988", D="265c42",
          B="c0cbdc", s="5a6988", F="c28569", f="b86f50", g="733e39"), [
"................",
"................",
"......gfg.......",
".....DDgfg......",
"...DDHHDDgf.....",
"..DHHMMMDDg.....",
".LHeLMMMMDD.....",
".LHLLLMMMMDD....",
".LLLLLLMMMMD....",
"..BLLLLLMMMD..g.",
"..sBBLLLLMMD.Ffg",
"...sBBBLLLMDFFfg",
"....ssBBBLDDFfg.",
".....fFsssDFFfg.",
"......f.....Fg..",
"................",
])

# 3 Tép – 3 con tép hồng nhạt trong veo, mỗi con một hướng
fish(3, "tep", "Tép", "freshwater shrimp", "common",
     dict(H="ead4aa", P="e8b796", p="f6757a", s="c28569", T="f6757a", t="b55088"), [
"................",
"................",
"w...............",
".w..HHHH........",
"..wHPPPPHH......",
"..ePPPpPPPHp....",
".PPPPPpPPPpPp...",
"..PPPPpPPpPpps..",
"...s.s.spPPpps..",
".......s.sPppp..",
"..........ppps..",
"........tTpps...",
".......TTTTs....",
"........Tt......",
"................",
"................",
])

# 4 Cá rô phi – xám, sọc dọc mờ, vây lưng dài nhiều gai
fish(4, "ca_ro_phi", "Cá rô phi", "tilapia", "common",
     dict(H="c0cbdc", L="8b9bb4", M="5a6988", D="3a4466",
          B="c0cbdc", s="5a6988", F="c28569", f="68386c", g="3e2731"), [
"................",
"................",
".....gFgFgF.....",
"....DDfDfDfg....",
"...DHHHHDDDDg...",
"..DHLMLMLMDDD...",
".LHeLMLMLMMDD...",
".LLLLMLMLMMMDD..",
"..BLLMLMLMMMDD..",
"..sBBMLMLMMMD...",
"...sBBBMLMMDDF..",
"....ssBBBMDDFfg.",
".....f.sssDFFffg",
".....Ff...FFffg.",
"...........Fgg..",
"................",
])

# 5 Cá chạch – mảnh, nâu, uốn chữ S, có râu
fish(5, "ca_chach", "Cá chạch", "pond loach", "common",
     dict(H="e4a672", L="b86f50", M="733e39", D="3e2731",
          B="c28569", s="733e39", F="8b9bb4", f="5a6988", g="3a4466"), [
"................",
"................",
"...MMMMD........",
".LHHHHHHD.......",
".LeLLMLHMD......",
"w.BBBBLLHD......",
".w....BLHD......",
"......BMHM......",
"......BLHD......",
"......BLHD......",
"......BBLHMD....",
".......BBLHHMD..",
".........BBLHMF.",
"...........sFfg.",
"...........FfFg.",
"............g.g.",
])


# ---------------- ROW 2: UNCOMMON (màu đậm hơn chút) ----------------
# 6 Cá chép – vàng đồng, vảy, râu; dáng nhảy cong chữ C
fish(6, "ca_chep", "Cá chép", "common carp", "uncommon",
     dict(H="feae34", L="e4a672", S="b86f50", M="b86f50", D="733e39",
          B="ead4aa", s="c28569", F="d77643", f="be4a2f", g="733e39"), [
"................",
".......gg.......",
"....MMMDfg......",
"..MHHHHHMDg.....",
".LHLSLSLHMDD....",
".LeLLSLSLHMD....",
"w.BBLLSLSLHMD...",
".w..BBLLSLHMD...",
"......BBLSLHD...",
".......BLLSHMD..",
"........BLLHMD..",
".......FBBLHD...",
"......FfsBLMDF..",
"......FffsMDFf..",
".......gf..Ffg..",
"................",
])

# 7 Cá trê – nâu sẫm trơn như lươn, râu dài, vây viền lưng/bụng; dáng cong chữ ")"
fish(7, "ca_tre", "Cá trê", "walking catfish", "uncommon",
     dict(H="b86f50", L="733e39", M="3e2731", D="262b44",
          B="c28569", s="733e39", F="68386c", f="68386c", g="3e2731"), [
"................",
"w...............",
".w.MMMD.........",
".LHHHHMDf.......",
"wLeLLHHMDf......",
".wBBLLLHMDf.....",
"..w.BBLLHMDf....",
".....FBLLHMDf...",
"......FBLHMDf...",
"......FBLHMDf...",
".....FBLHMDf....",
"....FsBLHMDf....",
"...FsBLHMDf.....",
"..FgsMMDf.......",
"..Ffg...........",
"................",
])

# 8 Cá mè (theo ref) – thân dài, đầu to có nắp mang sẫm, mắt thấp sát miệng, lưng xám sẫm, đuôi chẻ sẫm màu
fish(8, "ca_me", "Cá mè", "silver carp", "uncommon",
     dict(H="c0cbdc", L="8b9bb4", M="5a6988", D="3a4466", G="5a6988",
          B="c0cbdc", s="8b9bb4", F="3a4466", f="262b44", g="262b44"), [
"................",
"................",
"................",
"................",
"........gf......",
"..DDDDDDDDD...g.",
".DHHHHHHHHDD.Fg.",
".LGGLLLLLLHHDFf.",
".LeGLLLLLLLLHMF.",
".BGGBLLLLLLLLM..",
"..BBBBBBBLLLMFf.",
"...ssssBBBMMDFg.",
".....Ff..ss..Fg.",
"..............g.",
"................",
"................",
])

# 9 Cá trôi – xám bạc thon, vây đỏ; dáng cong chữ ")"
fish(9, "ca_troi", "Cá trôi", "mud carp", "uncommon",
     dict(H="c0cbdc", L="8b9bb4", M="5a6988", D="3a4466",
          B="c0cbdc", s="5a6988", F="e43b44", f="a22633", g="3e2731"), [
"................",
"................",
"...DDMM.........",
".LHHHHMDD.......",
".LeLLLHHMD......",
"..BBLLLLHMDF....",
"...sBBLLLHMDF...",
"....FsBBLHMDFf..",
"....FfsBLHMD....",
"......sBLHMD....",
".....sBLHMDD....",
"....sBLHMD......",
"..FFBLLMD.......",
".FffgsDD........",
"..Ffg...........",
"................",
])

# 10 Cá thát lát – đầu nhỏ, lưng gù, thân dẹt thuôn như lưỡi dao, vây hậu môn dài chạy dọc bụng tới đuôi
fish(10, "ca_that_lat", "Cá thát lát", "bronze featherback", "uncommon",
     dict(H="ead4aa", L="c0cbdc", M="8b9bb4", D="5a6988", k="3a4466",
          B="ead4aa", s="8b9bb4", F="c28569", f="733e39", g="733e39"), [
"................",
"................",
"................",
".....DD.........",
"...DDHHD........",
"..DHHLLHDf......",
".LHLLLLLHD......",
".LLeLLMLLHD.....",
"..BLLLLLLLHD....",
"..FBkLLMLLHD....",
"...FFsBkLLLHD...",
"....FFFskBLHD...",
"......FFFskHD...",
"........FFfsHD..",
"..........ffgHD.",
"................",
])


# ---------------- ROW 3: RARE (tương phản rõ) + PRECIOUS đầu tiên ----------------
# 11 Cá lóc – thân dài xanh đen, đốm loang, vây lưng/hậu môn dài; dáng chéo dốc
fish(11, "ca_loc", "Cá lóc", "snakehead", "rare",
     dict(H="fee761", L="3e8948", X="193c3e", M="265c42", D="193c3e",
          B="ead4aa", s="c28569", F="5a6988", f="3a4466", g="262b44"), [
"................",
"................",
"..MMMD..........",
".LHHHMDf........",
".LeLLHMDf.......",
".BLLXLHMDf......",
"..BLLLLHMDf.....",
"..FBLXLLHMDf....",
"...FBLLLXHMDf...",
"....FBLXLLHMD...",
".....FBLLLXHDF..",
"......FBXLLHDFf.",
".......FBBLMDFf.",
"........FsBDFfg.",
"..........gFfg..",
"................",
])

# 12 Lươn – vàng nâu, thân trơn uốn vòm, không vây, đuôi nhọn
fish(12, "luon", "Lươn", "swamp eel", "rare",
     dict(H="fee761", L="feae34", k="733e39", M="b86f50", D="733e39",
          B="ead4aa"), [
"................",
"................",
"................",
".....MMMMMD.....",
"....MHHHHHHD....",
"...MHLkLLkLHD...",
"..MHLBB...BLHD..",
"..MHLB.....BHD..",
".MHkB......BHM..",
".MHLB.......BM..",
".MHLB.......BM..",
".MHkB.......HD..",
".LeLB.......D...",
".LLLB...........",
"................",
"................",
])

# 13 Tôm càng xanh – dáng tôm cuộn chữ C quay phải (theo ref), râu dài vắt qua đầu; đầu xanh ngọc bích, nhạt dần sang xám về đuôi
fish(13, "tom_cang_xanh", "Tôm càng xanh", "giant river prawn", "rare",
     dict(H="2ce8f5", J="0099db", j="124e89", D="262b44", k="5a6988", N="8b9bb4", G="c0cbdc",
          x="3a4466", a="5a6988"), [
"................",
"................",
".aa.............",
"a..aaaaaaaaaa...",
"............a...",
"......JJJJJJJa..",
"....JJHHHHHHeJH.",
"...HJJJJJJJJJJ..",
"..HJjJjjjjj.....",
".HJjD.......kG..",
".JJjD......NkG..",
".jJjjD...NkNG...",
".NjkjxNGNxNG....",
"..kNNxNGNkx.....",
"...xxxxxx.......",
"................",
])

# 14 Cá lăng – thân xám, bụng trắng xám, đuôi đỏ cam chẻ, râu dài
fish(14, "ca_lang", "Cá lăng", "Asian redtail catfish", "rare",
     dict(H="c0cbdc", L="8b9bb4", M="5a6988", D="3a4466",
          B="c0cbdc", s="8b9bb4", F="f77622", t="feae34", f="be4a2f", g="733e39"), [
"................",
"................",
"...........tF...",
"..........FFf...",
"...DDDDg.FFfg...",
"..DHHHHDDMFf....",
".LHLLLLHHMDf....",
"wLeLLLLLLHMD....",
".wBBBLLLLHMD....",
"w.sBBBBLLMDF....",
".w.ss.sBMDFfF...",
"..w..F..sFffg...",
"......f...Ffg...",
"...........g....",
"................",
"................",
])

# 15 Cá bống tượng – nâu kem vân đá, miệng rộng, đầu to [PRECIOUS: màu rực + 1 px trắng]
fish(15, "ca_bong_tuong", "Cá bống tượng", "marble goby", "precious",
     dict(H="fee761", L="feae34", X="b86f50", M="d77643", D="733e39", m="3e2731",
          B="ead4aa", s="e4a672", F="ead4aa", f="b86f50", g="733e39"), [
"................",
"................",
"......gFg.......",
"...MMMDgFg.gFg..",
"..MH*HMMDDgFfg..",
".LHLXLHMXDDDf...",
".LeLLXLLHXMDD...",
".mmLXLLXLHMXD...",
".BBBLLXLLHMDD...",
"..sBBBLXLLHMDF..",
"...sssBBLXHMFFf.",
"...Ff..ssBMDFfg.",
"...f.....sDFfFg.",
"..........ggg...",
"................",
"................",
])


# ---------------- ROW 4: PRECIOUS (màu rực + 1 px trắng) + LEGENDARY (thêm 2–3 pixel lấp lánh * +) ----------------
# 16 Cá chép đỏ (koi) – đỏ cam rực, mảng trắng kem; dùng dáng nhảy cong của cá chép #6
fish(16, "ca_chep_do", "Cá chép đỏ", "red koi carp", "precious",
     dict(H="feae34", L="f77622", W="ead4aa", M="e43b44", D="a22633", B="ead4aa", s="d77643",
          F="f6757a", f="b55088", g="68386c"), [
"................",
".......gg.......",
"....MMMDfg......",
"..MH*HHHMDg.....",
".LHLWWLLHMDD....",
".LeLWWWLLHMD....",
"w.BBLLLLWWHMD...",
".w..BBLLLWHMD...",
"......BBLWWHD...",
".......BLLLHMD..",
"........BLLHMD..",
".......FBBLHD...",
"......FfsBLMDF..",
"......FffsMDFf..",
".......gf..Ffg..",
"................",
])

# 17 Cá anh vũ – lưng oliu, thân hồng, môi dày; dáng cong võng, đuôi vểnh lên
fish(17, "ca_anh_vu", "Cá anh vũ", "anh vu carp", "precious",
     dict(H="f6757a", L="c28569", M="3e8948", D="265c42", B="e8b796", s="c28569",
          P="f6757a", p="b55088", F="b55088", f="68386c"), [
"................",
"................",
"............Fp..",
"...........FfF..",
"..........DMFf..",
".....FfDDDHMD...",
"...DDH*HHHLMD...",
"..DHLLLLLLLMD...",
".PLeLLLLLLMD....",
".PpLLLLLLMMD....",
"..BBBLLLMMD.....",
"...sBBBBsD......",
"....Ff.Ff.......",
"................",
"................",
"................",
])

# 18 Cá chép vàng – vàng óng, vây đuôi xòe mềm, 3 pixel lấp lánh [LEGENDARY]
fish(18, "ca_chep_vang", "Cá chép vàng", "golden carp", "legendary",
     dict(H="fee761", L="feae34", M="f77622", D="be4a2f", B="fee761", s="feae34",
          F="ead4aa", f="e4a672"), [
"................",
"......+.........",
".....FfF........",
"...MMMfFf....*..",
"..MHHHMMFf......",
".LHLLLHMMDF.FFf.",
".LeLLLLHMDFFffF.",
".LLLLLLLHMDFfFF.",
".BLLLLLLHMDFFfF.",
"..BBLLLLHMDfFFf.",
"...BBBLLMDFFfFf.",
"....sFsBDFfFFf..",
"...FFf.ssFFf.Ff.",
"..Ff......Ff..f.",
"..+.............",
"................",
])

# 19 Cá trắm đen khổng lồ – xanh đen, thân dày chiếm gần hết ô, lấp lánh [LEGENDARY]
fish(19, "ca_tram_den", "Cá trắm đen khổng lồ", "giant black carp", "legendary",
     dict(H="0099db", L="3a4466", M="262b44", D="262b44", B="5a6988", s="3a4466",
          F="124e89", f="262b44", g="262b44"), [
"................",
".............*..",
"....DDDDg.......",
"..DDHHHHDgf.....",
".DHHLLLLHDDg....",
".BHBLLLLLHDDf...",
".BeBLLLLLLHDDf..",
".BBLLLLLLLLHDD..",
"..BBLLLLLLLHMD..",
"...sBBLLLLLHMD..",
"...FsBBBLLHMDF..",
"..Ff.ssBBLHMDFf.",
"........sBMDFff.",
"+........sDFfFg.",
"..........gFfg..",
"...+............",
])

# 20 Cá hô khổng lồ – bạc xám đồ sộ, đầu rất to (vạch mang), vây hồng, lấp lánh [LEGENDARY]
fish(20, "ca_ho", "Cá hô khổng lồ", "giant Mekong barb", "legendary",
     dict(H="c0cbdc", L="8b9bb4", M="5a6988", D="3a4466", G="5a6988", B="c0cbdc", s="8b9bb4",
          F="f6757a", f="b55088", g="68386c"), [
"................",
"..+.............",
"....DDDD.gf.....",
"..DDHHHHDDgf....",
".DHHLLLLHHDDf...",
".LHLLGLLLLHHDD..",
".LLeLGLLLLLLHDD*",
".LLLLGLLLLLLLHD.",
".BBLLGLLLLLLLHD.",
"..BBBBBLLLLLHMF.",
"...ssBBBBBLHMFf.",
"....F.sssBBMDFf.",
"...Ff....sDDFff.",
"..........FffFg.",
"...........gg...",
"..........*.....",
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


def preview(fishes, path, scale=10, gap=4):
    bg = (0x96, 0xcd, 0xcd, 255)
    n = len(fishes)
    W = n * 16 * scale + (n + 1) * gap * scale // 2
    H = 16 * scale + 16 + 3 * gap * scale // 2 + 16
    im = Image.new("RGBA", (W, H), bg)
    for i, f in enumerate(fishes):
        x = gap * scale // 2 + i * (16 * scale + gap * scale // 2)
        big = render(f).resize((16 * scale, 16 * scale), Image.NEAREST)
        im.alpha_composite(big, (x, gap * scale // 2))
        im.alpha_composite(render(f), (x + 72, 16 * scale + gap * scale))
    im.save(path)


def export(out):
    os.makedirs(out, exist_ok=True)
    for f in FISH:
        render(f).save(os.path.join(out, f"fish_{f['no']:02d}_{f['slug']}.png"))
    rows = (len(FISH) + 4) // 5
    sheet = Image.new("RGBA", (80, 16 * rows), (0, 0, 0, 0))
    for i, f in enumerate(FISH):
        sheet.alpha_composite(render(f), ((i % 5) * 16, (i // 5) * 16))
    sheet.save(os.path.join(out, "fish_sheet.png"))
    meta = dict(cell=16, columns=5, rows=rows, fish=[
        dict(index=i, no=f["no"], id=f"fish_{f['no']:02d}_{f['slug']}", name_vi=f["vi"],
             name_en=f["en"], rarity=f["rarity"], col=i % 5, row=i // 5) for i, f in enumerate(FISH)])
    with open(os.path.join(out, "fish_sheet.json"), "w", encoding="utf-8") as fp:
        json.dump(meta, fp, ensure_ascii=False, indent=2)
    with open(os.path.join(out, "to_aseprite.bat"), "w", encoding="utf-8", newline="\r\n") as fp:
        fp.write('@echo off\r\nset ASE=C:\\aseprite\\build\\bin\\aseprite.exe\r\ncd /d "%~dp0"\r\n'
                 'for %%f in (fish_*.png) do (\r\n  echo %%f\r\n  "%ASE%" -b "%%f" --save-as "%%~nf.aseprite"\r\n)\r\n'
                 'echo Xong.\r\npause\r\n')


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else "out"
    export(out)
    for r in range(0, len(FISH), 5):
        preview(FISH[r:r + 5], os.path.join(out, f"preview_row{r // 5 + 1}.png"))
