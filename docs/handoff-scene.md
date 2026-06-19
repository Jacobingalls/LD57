# Next-session prompt — The visible scene (paste into a fresh Claude Code Opus session)

---

You're picking up an in-progress project: a from-scratch **HTML5 port of *What Lurks Below***, a Unity 6 incremental game (Ludum Dare 57, theme "deeper"). The port lives in the **`web/`** folder of this repo and work is on the **`web-port`** branch. Phase 0 (reconnaissance + an interactive "codex"), Phase 1 (the headless sim core), and Phase 2 (the Mako gameplay loop) are **done**.

**Your job: make the game world visible — build the scene / backdrop — *then* start Phase 3 (UI shell).**

Right now `npm run dev` renders the Mako loop (a pink orb cloud rising from a pit, the harvester, collectors, beams, a climbing Mako counter) over a **flat black void** — there is no environment, so it's hard to read what you're looking at. Before the DOM UI shell (Phase 3), the game needs its *setting*: the moonlit surface, the cliffs around the pit's mouth, and the dark walled shaft descending into the abyss, composited **behind** the Mako apparatus. The art already exists in the Unity project — this is mostly lifting the scene layout and compositing it as Pixi background layers, registered to the world coordinates Phase 2 already established.

## Orient yourself first (read these before writing code)

1. **`web/CLAUDE.md`** — the project's rules and conventions. Follow them strictly; they govern everything below.
2. **`web/src/data/tuning.ts`** — the canonical, YAML-lifted values. The **`SCENE`** block has the exact world geometry of the Mako apparatus (harvester ≈Y9.94, spawn pit ≈Y0.85, the `ResourceField` pit, collectors ≈Y7–8.5, the camera). Your backdrop must register to these coordinates so the scene lines up with the orbs. The **`SIM`** block has `pixelsPerUnit: 16` (the art's native import PPU) and the fixed timestep (0.02).
3. **`web/src/world/`** — the Phase 2 world. Read **`render.ts`** especially: it has the `Camera` (world→screen, orthographic, pixel-perfect *nearest* scaling) and the Pixi layer/sprite setup you'll extend, and a `FRAME` constant for the default framing. `main.ts` boots the world + a debug HUD (a placeholder; Phase 3 replaces it).
4. **The codex** — open `docs/index.html` (build it from `docs/template.html` → `python3 docs/gen.py`; never hand-edit `index.html`). The Phase 0 "architecture/art" sections inventory the scene art, and **`docs/gen.py`'s `ART` map** already lists the curated backdrop sprites — `moon`, `mountains`, `cliffsL/R/TL/TR`, `holeBg` (`Hole Background`), `holeGradient` (`Hole End Gradient`), `wall`, `wallEdgeTop`, `wallEdgeInner` — with their `Assets/...` paths. That map is your shopping list.
5. **Unity reference (read-only):** the backdrop layout lives in **`Assets/Scenes/GameScene.unity`** and in prefabs nested there. The Mako apparatus itself is in **`Assets/Game/IdleModules/Collections Module.prefab`** (which nests `MakoManager.prefab`); the sky/cliffs/hole/walls are placed as scene objects or in an environment prefab with specific transforms and **sorting orders** (`m_SortingOrder` / sorting layers → your Pixi layer/Z order). The art PNGs are under `Assets/Art/Aseprite/` and `Assets/Art/Walls/`. The Main Camera is at world `(0, 7)`, orthographic size `15.3125`. **Ship nothing that imports from outside `web/`** — copy art into `web/src/assets/`, and lift any positions/scales/sort-orders into `tuning.ts`.

## How to lift scene geometry (the Phase 2 pattern — important)

Unity transforms are **nested and sometimes overridden**, so a naïve read of a single prefab gives wrong world coordinates. Resolve the full parent chain. Phase 2 was bitten by exactly this: the `ResourceField` is nested under the harvester with a local-position **override** `(0, -117.7)`, which is why it's the deep pit below the apparatus (not at the prefab default), and why the orbs spawn at `MakoFocus ≈Y0.85` and rise. Lift backdrop transforms the same careful way:
- A `MonoBehaviour`/`SpriteRenderer` block references its `m_GameObject`; the `Transform` gives `m_LocalPosition` / `m_LocalScale` / `m_Father`; scene **`PrefabInstance` `m_Modifications`** (e.g. `propertyPath: m_LocalPosition.y`) override the prefab defaults and **win at runtime**. World pos = walk the father chain applying `parentPos + parentScale ⊙ localPos` (no rotation in this game).
- Resolve a sprite's PNG by its `m_Sprite` guid → the matching `.png.meta`'s `guid:`.
- Record `m_SortingOrder` (and sorting layer) so your Pixi layers stack in the same order.

## The task — the visible scene

Composite the environment as Pixi layers **behind** the orb world (which currently draws on a transparent/black stage), registered to the existing `SCENE` coordinates and `Camera`, so the Mako loop reads as happening at the mouth of a deep pit under a moonlit sky.

- **Lift the backdrop layout** from `GameScene.unity` + the relevant prefab(s): world positions, scales, and draw order of the sky (`moon`, `mountains`), the cliffs around the pit mouth (`ClifsLeft/Right/TopLeft/TopRight`), the hole background + end-gradient, and the pit walls (`Assets/Art/Walls/*`). Add a **`BACKDROP`** block (or extend `SCENE`) in `tuning.ts` with provenance comments — **do not hardcode positions in render code.**
- **Copy the needed PNGs** into `web/src/assets/` (use the `ART` map paths). Keep the set minimal — only what the Mako scene needs now (cathedral etc. is later).
- **Build the backdrop** in a new `web/src/world/backdrop.ts` (or extend `render.ts`): static Pixi sprites and a vertically-tiling `TilingSprite` for the shaft walls, layered by the lifted draw order, all positioned through the `Camera` so they stay registered to the orb world at any zoom. Nearest-neighbour scaling, like the orbs (`TextureSource.defaultOptions.scaleMode = 'nearest'` is already set).
- **Vertical composition:** sky/surface at the top, cliffs framing the pit mouth, the shaft + walls descending into darkness (a depth gradient toward black). The Mako apparatus (harvester at the top of the shaft, orbs rising from `≈Y0.85` to the accelerator `≈Y9.82`) should sit naturally inside it. Re-frame the default camera if needed so the surface and the upper shaft are both visible.
- **Leave room for the depth camera (Phase 3):** the scene is a tall vertical strip the camera will scroll down as the player goes deeper. Build the walls to tile/extend downward; don't assume Phase 2's fixed `FRAME` is final.

## Then: Phase 3 (UI shell)

Once the scene is visible, proceed to **Phase 3** as listed in the codex Phase Tracker: the DOM upgrade list (re-render on `upgrade.purchased`), the HUD + `NumberTumbler` odometer, tooltips, camera-depth scroll (normalize `WheelEvent.deltaMode` per the `CAMERA` tuning), and the "go deeper" idle modules (resource reveal). Phase 3's real DOM UI replaces the Phase-2 debug HUD in `main.ts`.

## Rules (from `web/CLAUDE.md` — do not violate)

- **1:1 with Unity.** Match the original's layout, scale, and look. Replicate quirks with a `// NOTE:`. No re-design; re-styling for the sequel is a later, separate concern.
- **Numbers come from `src/data/tuning.ts`, never invented.** Lift backdrop positions / scales / sort-orders from the scene/prefab YAML and add them there with provenance (the Phase-2 `SCENE` block shows the pattern, including how nested-prefab overrides resolve to world coordinates). The one sanctioned exception is a clearly-flagged **PORT-only** presentation value, like the existing `CONTAINMENT` (⚠ marked "NOT a Unity value") — use that pattern sparingly and only when the original mechanism is genuinely deferred.
- **`web/` is self-contained** — no imports from `../Assets` / `../docs` in shipped code; copy art into `web/src/assets/`.
- **Tests + types stay green.** `npm test` and `npm run typecheck` must pass. Add Vitest specs for anything testable you add (e.g., camera world↔screen math, layout helpers).

## Definition of done

- `npm run dev`: the Mako loop now plays inside a recognizable scene — moonlit sky, cliffs at the pit mouth, a dark walled shaft descending — **not a black void** — and it matches the Unity game's framing and feel. Pixel-perfect; still 60 fps with the cloud at high counts.
- New/updated Vitest specs pass; typecheck clean.
- **Publish + update the tracker:** capture screenshots into `docs/shots/` via **`node docs/capture-game.mjs`** (headless puppeteer; see gotchas), run `bash docs/publish-build.sh phase2-scene` (pick a clear name), then in `docs/template.html`'s `PHASES` flip the **`n:'2.5'` "The visible scene"** phase to `status:'done'` (+ tasks → `1`, add `shots:[...]` and `build:`), set Phase 3 `status:'active'`, and re-run `python3 docs/gen.py`.

## Phase-2 facts & workflow gotchas you'll rely on

- **Mako geometry is already in `tuning.ts` `SCENE`** (corrected): a vertical shaft at **X≈−6.1**; orbs spawn at the **MakoFocus pit point ≈(−6.15, 0.85)** and rise to the **accelerator ≈Y9.82** at the top (harvester device ≈Y9.94); collectors flank at Y7–8.5; the `ResourceField` is the deep pit below; camera was framed `Y[−1, 16.5]`, `X[−21, 8]`. Your backdrop registers to these.
- **`pixelsPerUnit = 16`** (`SIM.pixelsPerUnit`): art imported at 16 PPU → an `N`-px sprite spans `N/16` world units. Use this to place/scale backdrop art (and read each PNG's pixel size).
- **Containment is deferred, by design.** Unity confines the cloud via orb-orb collision (non-trigger colliders, `ResourceOrb` layer self-collides); that N-body physics is out of Phase 2's scope, so a PORT-only soft ceiling (`CONTAINMENT` in `tuning.ts`) stands in. Don't be surprised the orbs don't physically pile.
- **Captures:** macOS throttles `requestAnimationFrame` for windows/tabs on an **inactive Space** → the preview tool and headed puppeteer can show a *frozen* sim (orbs spawn but nothing moves/collects). The dev build exposes **`window.__step(n)`** (DEV-only, stripped from prod) to advance the sim deterministically for screenshots. Game shots come from **`docs/capture-game.mjs`** (headless; `puppeteer-core` is installed in `docs/`). Headless uses **software GL**, so its FPS drops past ~700 sprites — judge "smooth at high counts" by the CPU step cost (Phase 2 measured **~0.245 ms for 2300 orbs**), not the headless FPS.
- **Build:** `main.ts` boots via an async IIFE, *not* top-level await — TLA trips a TDZ in the production bundle (module init reorders and a `tuning` const is read before init). Vite `build.target = 'esnext'`.

**Start by reading `web/CLAUDE.md` and `web/src/data/tuning.ts` (the `SCENE`/`SIM` blocks), skim `web/src/world/render.ts`, then lift the `GameScene` backdrop layout and propose a short plan before building.**

---
