# CLAUDE.md — What Lurks Below, web port

This is the **from-scratch HTML5 port** of *What Lurks Below* (Ludum Dare 57), a
moody Lovecraftian incremental game originally built in Unity 6. The goal is a
pure-web build that runs natively in the browser, **fully detached from Unity**,
preserving the game 1:1 (re-balancing comes later).

You are working inside `./web`. The Unity source lives at `../Assets`, and a full
interactive reference — the **codex** — is at `../docs/index.html` (build source
`../docs/template.html`). Read both freely; see "Boundaries" for what `./web` may
and may not depend on.

## The rules (non-negotiable)

1. **Port 1:1 from Unity.** Match the original's numbers and behavior exactly.
   Do not "improve" balance or mechanics — re-tuning is a later, separate phase.
   If the original has a quirk/bug, replicate it and add a `// NOTE:` comment.
2. **`./web` is self-contained.** Ship nothing that imports from outside `./web`.
   You may *read* `../Assets` and `../docs` for reference and to lift data, but
   the running app and its build must depend only on files under `./web`.
3. **Balance is data, lifted from Unity — never invented.** All tuning lives in
   `src/data/`. `src/data/tuning.ts` is **canonical** (lifted from prefab/scene
   YAML). The Unity C# field *defaults* are often placeholders — trust the
   prefab YAML, not the `.cs` default. When you need a value that isn't in
   `src/data/` yet, **lift it from the YAML** and add it there with a provenance
   comment; don't hardcode it in logic.
4. **Everything testable gets a test.** Sim/economy/physics math is covered by
   Vitest specs that assert against the Unity values. Keep `npm test` green.
5. **Keep the tracker honest.** When a phase's work lands, update the Phase
   Tracker in `../docs/template.html` (the `PHASES` array near the bottom):
   flip task flags to `1`, set the phase `status`, capture evidence screenshots,
   publish a build, then re-run `python ../docs/gen.py`. (See "Publishing a phase".)
6. **TypeScript strict, no shortcuts.** `npm run typecheck` must pass. No `any`
   to dodge types; no unused locals/params (the config forbids them).

## Stack

- **PixiJS** (WebGL) for the world / orb cloud · **HTML+CSS DOM** for UI ·
  hand-rolled **fixed-step physics** loop (mirrors Unity `FixedUpdate`) ·
  a tiny typed **EventBus** (`src/core/events.ts`, replaces JamKit PubSub) ·
  **WebAudio** · **localStorage** save (new for the sequel). Vite + Vitest.
- PixiJS 8 renders with nearest-neighbour scaling at 16 pixels per world unit
  (`SIM.pixelsPerUnit`), through the orthographic `Camera` in `src/world/render.ts`.

## Layout

```
web/
  src/
    core/    events.ts            EventBus (JamKit PubSub replacement)
    sim/     resources.ts         ResourceManager — resources + modifier stacks
             upgrades.ts          Upgrade + UpgradeManager — costs, requirement graph
    data/    resources.ts         ResourceType set
             upgrades.ts          the 29 upgrades (1:1 from UpgradeManager.cs)
             tuning.ts            ⭐ all serialized values, lifted from YAML (canonical)
             pubsub.ts            topic catalog + scene-wired listener map
    core/    rng.ts               seeded RNG
    main.ts                       boots the live Mako loop + a debug HUD (Phase 3 replaces the HUD)
    world/   makoWorld.ts         the fixed-step world: owns orbs, harvesters, collectors, zones
             render.ts            Pixi stage, Camera (world→screen), sprite layers
             orb.ts               MakoOrb + OrbPool
             physics.ts vec2.ts   the 2D integrator (mirrors Unity FixedUpdate)
             harvester.ts         charge 0→9, laser, orb spawn
             collector.ts         PD "seek" collector, manual + auto
             accelerator.ts field.ts   zone checks: accelerator kick, ResourceField pull
             spawnRng.ts          orb level + count rolls
             (Phase 2.5) backdrop: sky, cliffs, pit shaft · (Phase 4) gate
    assets/                       PNGs copied from ../Assets (orbs, harvester, collector, laser)
    ui/      (Phase 3+) DOM upgrade list, HUD, tumbler, tooltips, camera-depth scroll
    audio/   (Phase 5+) WebAudio manager
  test/                           Vitest specs (1:1 assertions)
```

## Commands

```bash
npm install          # first time
npm test             # Vitest — the 1:1 balance assertions (keep green)
npm run typecheck    # tsc --noEmit (must pass)
npm run dev          # http://localhost:5173 — live app
npm run build        # tsc + vite build -> dist/index.html (single self-contained file)
```

## How the economy maps (already ported — match this style)

- `ResourceManager.increment(t, n)` = C# `IncrementResource`: gain =
  `floor(baseGain·n·Σadditive·Πmultiplicative)`. Modifier stacks seed at `[1]`.
- `Upgrade.getCost(r)` = C# `GetCost`: `floor(costScale^timesPurchased · baseCost)`.
- `UpgradeManager.isHidden / isUnlocked` mirror the C# getters exactly.
  `purchase()` pays + increments + emits `upgrade.purchased`, `upgrade.purchased.<key>`,
  and the upgrade's `publishes` topics — it does **not** itself check unlock (1:1 with C#).

## Lifting data from Unity YAML

When you need more tuning (Phase 2 will need orb physics, field forces, collector
PD params — most already in `tuning.ts`):
- Prefab/scene blocks are `--- !u!<classId> &<id>` then a root key
  (`MonoBehaviour:`, `Rigidbody2D:`, `CircleCollider2D:`…). For `MonoBehaviour`,
  resolve `m_Script.guid` → script name via that script's `.cs.meta` (`guid:` line).
- Write a throwaway extractor in a temp folder (not in the repo): build the
  guid→name map from `../Assets/**/*.cs.meta`, then read component fields.
  Scene `PrefabInstance` `m_Modifications` override prefab values; resolve them
  and walk the `m_Father` chain for world positions (`docs/handoff-scene.md` has
  the details). **Bake results into `src/data/tuning.ts` as static values with a
  provenance comment** — do not keep a live dependency on `../Assets`.

## Publishing a phase (do this when a phase completes)

1. `npm run build` (→ `web/dist/index.html`, self-contained).
2. From repo root: `bash docs/publish-build.sh phaseN` (Git Bash) — copies the build
   to `docs/builds/phaseN.html` (the link the codex tracker exposes).
3. Capture evidence screenshots of the running phase into `docs/shots/` (see
   `docs/capture.mjs` / `docs/capture-game.mjs` for the puppeteer pattern; their
   Chrome and folder paths are the Mac's, so set them for this machine first; add
   keys to `SHOTS` in `docs/gen.py`).
4. In `docs/template.html` `PHASES`: set the phase `status:'done'`, flip its tasks
   to `1`, add `shots:[...]` and `build:'builds/phaseN.html'`; set the next phase
   `status:'active'`.
5. `python docs/gen.py` to rebuild the codex.

## Don't

- Don't import from `../Assets`, `../docs`, or anywhere outside `./web` in shipped code.
- Don't invent balance numbers. Lift them.
- Don't add gameplay the original lacks (save/localStorage is the one sanctioned addition).
- Don't leave `npm test` or `npm run typecheck` red.
