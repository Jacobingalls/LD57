# CLAUDE.md — What Lurks Below, web port

*What Lurks Below* is a Ludum Dare 57 incremental game (theme "Depths") built
in Unity 6: harvest glowing Mako from a pit, build a town, research, and push
your people through a gate at the bottom. This folder rebuilds it as a plain
web game (TypeScript, PixiJS, Vite) with no Unity in it. The port copies the
Unity game 1:1 first; changes for the sequel come after the port is done.

## Where things are

| Path | What it is |
|---|---|
| `web/` | The web game. All new work happens here. Its rules are in `web/CLAUDE.md`; read them before writing code. |
| `web/src/data/tuning.ts` | Every number the game uses, copied from the Unity scene and prefab files, with notes saying where each came from. |
| `docs/template.html` | Source of the "codex": a map of the Unity game, the porting plan and the phase tracker (`PHASES` near the bottom). |
| `docs/index.html` | The codex page built from `template.html` by `docs/gen.py`. Never edit it by hand. |
| `docs/builds/phaseN.html` | A single-file build saved at the end of each phase. |
| `docs/handoff-*.md` | The prompt each phase started from. `handoff-scene.md` is the current one. |
| `Assets/`, `ProjectSettings/`, `Packages/` | The original Unity project. Read it to copy numbers and art; never change it, and never import from it in `web/`. |

## Where it stands

- Done: phase 0 (the codex), phase 1 (economy and upgrades, tested) and
  phase 2 (the Mako loop: orbs, harvester, collectors, debug HUD).
- Next: phase 2.5, the visible scene. The loop draws over a black void; the
  moonlit sky, cliffs and pit shaft go behind it, placed from
  `Assets/Scenes/GameScene.unity`. The full brief, with how to read Unity
  positions correctly, is `docs/handoff-scene.md`.
- After that: phase 3 (the real UI), phase 4 (every layer down to the gate),
  phase 5 (audio, save, polish, deploy). The tracker in the codex lists the
  tasks for each.

## Commands

Run from `web/`:

```bash
npm install          # first time only
npm test             # Vitest, 44 tests; keep green
npm run typecheck    # tsc --noEmit; keep clean
npm run dev          # http://localhost:5173
npm run build        # one self-contained file at web/dist/index.html
```

From the repo root:

```bash
bash docs/publish-build.sh phase2-scene   # build and copy to docs/builds/
python docs/gen.py                        # rebuild docs/index.html from template.html
```

In the browser pane, `.claude/launch.json` has `web` (the dev server) and
`codex` (serves `docs/` on port 8765). Both were written on the Mac and are
untested here. If `web` won't start, run Vite through Node directly:
`"C:/Program Files/nodejs/node.exe"` with arguments
`web/node_modules/vite/bin/vite.js web --port 5173`; that form is proven on
this PC. `codex` needs `python` in place of `python3`.

## This PC (Windows)

The port was started on a Mac; these are the differences here.

- **Node** 26 LTS lives in `C:\Program Files\nodejs`. If `node` or `npm` is
  not found, the shell started before the install: prepend that folder to
  PATH for the command.
- **Python** is `python`, not `python3`. Docs and the `codex` launch entry
  say `python3`, which fails here.
- **Bash scripts** (`docs/publish-build.sh`) need Git Bash. From PowerShell,
  `bash` is WSL's; use the Bash tool instead.
- **Screenshot scripts** `docs/capture.mjs` and `docs/capture-game.mjs` have
  the Mac's Chrome path and `/Users/jameskelly/...` folders written in. Point
  `CHROME`, `URL` and `OUT` at this machine before running them (Chrome is
  usually `C:/Program Files/Google/Chrome/Application/chrome.exe`).
- The dev build exposes `window.__step(n)` to advance the sim by hand for
  screenshots.

## Git

- This folder is a git worktree of `D:\Development\LD57`, on branch
  `web-port`, which tracks `origin/web-port` on GitHub (`Jacobingalls/LD57`).
  Work and commit here on `web-port`; don't switch branches.
- `D:\Development\LD57` itself is on `djk/vibe` with uncommitted work. Leave
  that checkout alone.
- The stash is shared with that checkout, so don't use `git stash`.
- `web/node_modules` and `web/dist` are ignored. Commit `package-lock.json`
  changes along with `package.json`.
