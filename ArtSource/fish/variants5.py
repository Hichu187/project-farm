import fish_grids as fg
pal = dict(fg.FISH[4]["pal"], F="8b9bb4", f="5a6988", g="3a4466")
V = {
"5A_vom": [
"................",
"................",
"................",
".....MMMMMD.....",
"....MHHHHHHD....",
"...MHLMLLMLHD...",
"..MHLBB...BLHD..",
"..MHLB.....BHD..",
".MHLB......BHM..",
".MHLB.......BM..",
".MHLB......FBMF.",
".MHLB......FfFg.",
".LeLB.......gFg.",
".LLLB......g..g.",
"w....w..........",
"................"],
"5B_dau_hoi": [
"................",
"................",
"..MMMMMMMMD.....",
".LHHHHHHHHHD....",
".LeLLMLLMLHMD...",
"w.BBBBBBBLLHD...",
".w.......BLHD...",
".........BLHD...",
".........BMHM...",
"........BLHMD...",
".......BLHMD....",
".....BBLHMD.....",
".FF.BLHMMD......",
".FffsMMD........",
"..gFg...........",
"..g.............",],
"5C_chu_S": [
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
"............g.g.",],
}
fl = [dict(fg.FISH[4], slug=k, grid=g, pal=pal) for k, g in V.items()]
fl = [fg.FISH[4]] + fl
fg.preview(fl, "out/preview_ca_chach_variants.png")
import json; json.dump(V, open("out/ca_chach_variants.json","w"), indent=1)
