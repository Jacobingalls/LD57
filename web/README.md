# What Lurks Below — web port

A from-scratch HTML5 port of the Ludum Dare 57 game, **fully detached from Unity**.
Self-contained: nothing here references anything outside `./web`. Balance and
tuning are **lifted 1:1 from the Unity build** (see provenance below).

Target stack (per the codex porting blueprint): **PixiJS** for the world +
**HTML/CSS DOM** for UI + a hand-rolled fixed-step physics loop + a tiny
**EventBus** (replacing JamKit PubSub) + WebAudio + `localStorage` save.

## Status

- **Phase 0 — Recon & codex:** done — `../docs/index.html` (interactive atlas + Phase Tracker).
- **Phase 1 — Sim core (headless, tested):** done.
  - `src/core/events.ts` — `EventBus`, the JamKit-PubSub replacement.
  - `src/sim/resources.ts` — `ResourceManager`: 4 resources + additive/multiplicative modifier stacks, gain `floor(baseGain·n·Σadd·Πmult)`.
  - `src/sim/upgrades.ts` — `Upgrade` + `UpgradeManager`: `cost(n)=floor(scale^n·base)`, the requirement graph, purchase/affordability.
  - `src/data/*` — the **lifted data** (see below).
- **Phase 2 — The Mako loop:** done. PixiJS world + a hand-rolled fixed-step physics loop in `src/world/`: `OrbPool`/`MakoOrb`, the `ResourceField` + accelerator zones, the harvester (charge 0→9 via click/hold/Space + auto), and the collector PD "seek" (manual + auto) — all 1:1 with Unity, every formula commented with its C# origin. `src/main.ts` is now the **live loop + a debug HUD**. New specs cover the integrator and the orb-spawn RNG. Built snapshot: `../docs/builds/phase2.html`.
- **Next — the visible scene:** the world currently renders over a black void; the scene/backdrop (moonlit sky, cliffs, the pit shaft & walls) comes **before** Phase 3 (UI shell). Brief: `../docs/handoff-scene.md`.

`src/data/tuning.ts` `SCENE`/`SIM`/`CONTAINMENT` hold the lifted Mako-apparatus geometry, the fixed timestep (0.02) + pixels-per-unit (16), and the one ⚠ port-only value (a soft cloud ceiling standing in for Unity's deferred orb-orb collision).

## Run

```bash
cd web
npm install
npm test        # 1:1 balance + physics/RNG assertions
npm run dev     # http://localhost:5173 — the live Mako loop + debug HUD
npm run typecheck
npm run build   # single-file dist/index.html (publish via ../docs/publish-build.sh)
```

## Data provenance (the "lift")

`src/data/` is the canonical balance, extracted from the Unity project on 2026-06-19:

- **`upgrades.ts`** — the 29 upgrades, 1:1 from `Assets/Game/UpgradeManager.cs`.
- **`tuning.ts`** — every serialized gameplay value, read out of the prefab/scene YAML
  (guid→script resolved, MonoBehaviour/Rigidbody2D/Collider2D fields captured).
  Several values **override the C# field defaults**, which were placeholders never used
  at runtime — flagged with ⚠ in the file. Notable corrections vs. the codex:
  - Gate: `closingForcePerSecond = 0.355`, `forcePerOpenAttempt = 0.025` (not 1.0 / 0.1)
    → break-even is **14.2 souls/s**, not the codex's "≥10/s".
  - Research: `gooToMakeKnowledge = 64`, `chainSpeed = 5`, `bottomY = -42`, `startingBuckets = 8`.
  - Orb `baseValue = 1 / 2 / 4` by level; `maxBuildingsPerLedge = 5`.
- **`pubsub.ts`** — the event-topic catalog and the scene-wired listener map to recreate.
- **`resources.ts`** — the `ResourceType` set; Artifacts is vestigial (kept for fidelity).

Balance is ported **as-is**; re-tuning for the sequel is deferred.

## Layout

```
web/
  src/
    core/    eventBus, rng
    sim/     resources, upgrades        (headless game logic, 1:1 with Unity)
    data/    upgrades, tuning, pubsub, resources   (the lifted balance)
    world/   orb+pool, physics, field, accelerator, harvester, collector,
             spawnRng, makoWorld, render   (the Pixi Mako loop)
    assets/  the real game PNGs (orbs, harvester, collector, laser, focus)
    ui/      (Phase 3+) DOM upgrade list, HUD, tumbler, camera-depth scroll
    main.ts  boots the live loop + debug HUD
  test/      vitest specs (economy + physics + orb-spawn RNG)
```
