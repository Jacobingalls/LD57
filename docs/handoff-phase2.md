# Next-session prompt — Phase 2 (paste into a fresh Claude Code Opus session)

---

You're picking up an in-progress project: a from-scratch **HTML5 port of *What Lurks Below***, a Unity 6 incremental game (Ludum Dare 57, "deeper"). The port lives in the **`web/`** folder of this repo and work is on the **`web-port`** branch. Phase 0 (reconnaissance + an interactive "codex") and Phase 1 (the headless sim core) are **done**. **Your job is Phase 2 — the Mako gameplay loop.**

## Orient yourself first (read these before writing code)

1. **`web/CLAUDE.md`** — the project's rules and conventions. Follow them strictly; they govern everything below.
2. **`web/src/data/tuning.ts`** — the canonical, YAML-lifted tuning values. The orb / harvester / collector / field / refinement numbers you need for Phase 2 are already in here. This is the source of truth — **not** the Unity C# field defaults (those were placeholders).
3. **The codex** — read `docs/template.html` (or open `docs/index.html` in a browser). Study the **"Mako Loop"** section: it traces one orb's full lifecycle — summon → spawn → rise → accelerate/refine → collect → convert — with exact mechanics, and the **"Porting"** section. Its **Phase Tracker** lists your Phase 2 tasks and links the Phase 1 build.
4. **`web/src/sim/*` and `web/src/core/events.ts`** — the already-ported code. Match its style: small, strict-typed, 1:1 with Unity, every formula commented with its C# origin.
5. **Unity reference (read-only):** `Assets/Game/Mako/*.cs` (`MakoManager`, `MakoHarvester`, `MakoCollector`, `MakoOrb`, `MakoAccelerator`) and `Assets/Game/ResourceField/ResourceField.cs`. These define the loop you're reproducing. You may read `../Assets` freely, but **ship nothing that imports from outside `web/`** — lift any new values into `src/data/tuning.ts`.

## The task — Phase 2: the Mako loop

Build the core gameplay loop using **PixiJS** for rendering (add it: `npm i pixi.js`) and a **hand-rolled fixed-step physics loop** that mirrors Unity's `FixedUpdate` (fixed dt accumulator). Deliver under `web/src/world/`:

- **Pixi stage + vertical "depth" camera**, pixel-perfect (nearest-neighbor) scaling. Use the real sprites — PNGs in `Assets/Game/Mako/` and `Assets/Art/`; copy the handful you need into `web/src/assets/` so the app stays self-contained.
- **`MakoOrb` entities backed by an object pool.** The pink "substance cloud" is hundreds–thousands of *live* orbs (there is NO particle system) — this is the #1 perf risk. Batch-render via Pixi and pool aggressively.
- **Fixed-step 2D integrator** for: spawn impulse, the `ResourceField` up-force + center pull, the accelerator kick, and the collector's PD "seek" controller. Use the `tuning.ts` values (`ORB`, `RESOURCE_FIELD`, `COLLECTOR`, `HARVESTER`, `REFINEMENT`).
- **Harvester:** 0→9 charge via click / click-hold / keyboard (Space); fires after the laser delay, then spawns 1–N orbs using the exact orb-level + orb-count RNG (in `REFINEMENT` / documented in the codex).
- **Collector:** nearest-collectable seek → capture → convert to Mako via the ported `ResourceManager.increment('Mako', orb.baseValue)`. Manual + auto (Manifest Pylon) variants.
- **Accelerator + ResourceField** as zone checks in the orb update loop (enter/exit edge detection), plus the anti-stuck safeties (5s collider-disable, 60s auto-collectable).
- Wire everything through the existing **EventBus** topics (`mako.harvester.laser.*`, `mako.harvested`, `mako.collected`, …; see `src/data/pubsub.ts`).

## Rules (from `web/CLAUDE.md` — do not violate)

- **1:1 with Unity.** Match behavior and numbers exactly; replicate quirks with a `// NOTE:` comment. No re-balancing — that's a later, separate phase.
- **Numbers come from `src/data/tuning.ts`, never invented.** If you need a value that isn't there, lift it from the prefab/scene YAML (the `/tmp/lift.py` extractor pattern is described in CLAUDE.md) and add it with a provenance comment.
- **`web/` is self-contained** — no imports from `../Assets` / `../docs` in shipped code.
- **Tests + types stay green.** Add Vitest specs for the physics integrator and the orb-spawn RNG (assert against `tuning.ts` + the codex). `npm test` and `npm run typecheck` must pass.

## Definition of done

- `npm run dev`: clicking/holding the harvester summons orbs that visibly rise, get accelerated/refined, and are pulled in by the collector, incrementing Mako. The cloud stays smooth at high orb counts. Auto harvester/collector run on their cadences. Feel matches the original.
- New Vitest specs pass; typecheck clean.
- **Publish + update the tracker:** capture evidence screenshots into `docs/shots/` (puppeteer pattern in `docs/capture.mjs`), run `bash docs/publish-build.sh phase2`, then in `docs/template.html`'s `PHASES` set Phase 2 `status:'done'` (+ flip tasks to `1`, add `shots:[...]` and `build:'builds/phase2.html'`) and Phase 3 `status:'active'`; finally `python3 docs/gen.py`.

**Start by reading `web/CLAUDE.md` and `web/src/data/tuning.ts`, then propose a short plan before building.**

---
