#!/usr/bin/env python3
"""
Inlines the curated game art + the game's own pixel fonts into the codex as
base64 data URIs, producing a single self-contained docs/index.html.

Usage:  python3 docs/gen.py
Edit    docs/template.html  (the source) — never edit index.html by hand.
"""
import base64, os, sys, pathlib

ROOT = pathlib.Path(__file__).resolve().parent.parent          # repo root
DOCS = ROOT / "docs"

# ---- curated, embeddable game art (real runtime PNGs) ---------------------
ART = {
    "orb":            "Assets/Game/Mako/mako_orb.png",
    "collector":      "Assets/Game/Mako/mako_collector.png",
    "laser":          "Assets/Game/Mako/mako_laser.png",
    "focus":          "Assets/Game/Mako/mako_focus.png",
    "harvesterBg":    "Assets/Game/Mako/mako_harvester_background.png",
    "harvesterFg":    "Assets/Game/Mako/mako_harvester_foreground.png",
    "harvesterSheet": "Assets/Game/Mako/mako_harvester_foreground_spritesheet.png",
    "icons":          "Assets/Art/Aseprite/font_spritesheet.png",
    "cathedralBg":    "Assets/Art/Aseprite/cathedral_background.png",
    "cathedralFg":    "Assets/Art/Aseprite/cathedral_foreground.png",
    "moon":           "Assets/Art/Aseprite/moon.png",
    "mountains":      "Assets/Art/Aseprite/Mountains.png",
    "cliffsL":        "Assets/Art/Aseprite/ClifsLeft.png",
    "cliffsR":        "Assets/Art/Aseprite/ClifsRight.png",
    "cliffsTL":       "Assets/Art/Aseprite/ClifsTopLeft.png",
    "cliffsTR":       "Assets/Art/Aseprite/ClifsTopRight.png",
    "holeBg":         "Assets/Art/Aseprite/Hole Background.png",
    "holeGradient":   "Assets/Art/Aseprite/Hole End Gradient.png",
    "wall":           "Assets/Art/Walls/Black.png",
    "wallEdgeTop":    "Assets/Art/Walls/Black Edge Top.png",
    "wallEdgeInner":  "Assets/Art/Walls/Black Edge Inner Top Left.png",
    "bucketEmpty":    "Assets/Art/Aseprite/GoopBucketEmpty.png",
    "bucketTrack":    "Assets/Art/Aseprite/GoopBucketTrackHolder.png",
    "lock":           "Assets/Art/Aseprite/lock.png",
    "check":          "Assets/Art/Aseprite/checkmark.png",
    "ladder":         "Assets/Art/Aseprite/ladder.png",
}

# ---- embedded pixel fonts (the game's own) --------------------------------
FONTS = {
    "ThaleahFat":     "Assets/Thaleah_PixelFont/Materials/ThaleahFat_TTF.ttf",
    "PixelBotoSerif": "Assets/Pixelart Fonts Pack/Fonts/PixelBotoSerif.ttf",
}

# ---- phase-tracker "field evidence" screenshots ---------------------------
# Codex shots (p0*) by docs/capture.mjs; running-game shots (p2*) by
# docs/capture-game.mjs. Add new phases' shots here as they ship; missing files
# are skipped so the build never breaks before capture.
SHOTS = {
    "p0_hero":     "docs/shots/p0_hero.jpg",
    "p0_map":      "docs/shots/p0_map.jpg",
    "p0_explorer": "docs/shots/p0_explorer.jpg",
    "p0_porting":  "docs/shots/p0_porting.jpg",
    "p1_simcore":  "docs/shots/p1_simcore.jpg",
    "p2_loop":     "docs/shots/p2_loop.jpg",
    "p2_cloud":    "docs/shots/p2_cloud.jpg",
}

def b64(path):
    with open(path, "rb") as f:
        return base64.b64encode(f.read()).decode("ascii")

def main():
    # --- art -> JS object ---
    art_entries, missing = [], []
    for name, rel in ART.items():
        p = ROOT / rel
        if not p.exists():
            missing.append(rel); continue
        art_entries.append(f'  {name}: "data:image/png;base64,{b64(p)}"')
    art_js = "window.WLB_ART = {\n" + ",\n".join(art_entries) + "\n};"

    # --- fonts -> @font-face ---
    faces = []
    for fam, rel in FONTS.items():
        p = ROOT / rel
        if not p.exists():
            missing.append(rel); continue
        data = b64(p)
        faces.append(
            f"@font-face{{font-family:'{fam}';"
            f"src:url(data:font/ttf;base64,{data}) format('truetype');"
            f"font-display:swap;}}"
        )
    font_css = "\n".join(faces)

    # --- screenshots -> JS object ---
    shot_entries = []
    for name, rel in SHOTS.items():
        p = ROOT / rel
        if not p.exists():
            continue
        shot_entries.append(f'  {name}: "data:image/jpeg;base64,{b64(p)}"')
    shots_js = "window.WLB_SHOTS = {" + ("\n" + ",\n".join(shot_entries) + "\n" if shot_entries else "") + "};"

    # --- inject into template ---
    tpl = (DOCS / "template.html").read_text(encoding="utf-8")
    tpl = tpl.replace("/*__FONTFACE__*/", font_css)
    tpl = tpl.replace("/*__ART__*/", art_js)
    tpl = tpl.replace("/*__SHOTS__*/", shots_js)
    out = DOCS / "index.html"
    out.write_text(tpl, encoding="utf-8")

    size_kb = out.stat().st_size / 1024
    print(f"wrote {out}  ({size_kb:.0f} KB)")
    print(f"embedded {len(art_entries)} sprites, {len(faces)} fonts, {len(shot_entries)} screenshots")
    if missing:
        print("WARNING missing files:")
        for m in missing: print("  -", m)

if __name__ == "__main__":
    main()
