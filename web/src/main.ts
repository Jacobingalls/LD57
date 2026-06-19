// Phase 2 entry: the live Mako loop. Boots the ported sim core (ResourceManager +
// UpgradeManager + EventBus) and MakoWorld, renders it with Pixi, and wires input.
// A small debug panel exercises the real upgrade *topics* (so the same handlers that
// Phase 3's UI will fire are driven here) to demo holds / autos / refinement.
//
// The whole boot lives in an async IIFE rather than a top-level await: the latter makes
// the production bundler reorder module init and trip a TDZ on tuning consts.
import './hud.css';
import { EventBus } from './core/events';
import { ResourceManager } from './sim/resources';
import { UpgradeManager } from './sim/upgrades';
import { MakoWorld } from './world/makoWorld';
import { Renderer } from './world/render';
import { SIM } from './data/tuning';
import { RESOURCE_TYPES, type ResourceType } from './data/resources';
import type { Collector } from './world/collector';

void (async () => {
  const bus = new EventBus();
  const rm = new ResourceManager();
  const um = new UpgradeManager(rm, bus);
  const world = new MakoWorld(rm, um, bus);

  // ---- DOM scaffold ----
  const app = document.getElementById('app')!;
  app.classList.add('stage-root');
  const hud = document.createElement('div');
  hud.id = 'hud';

  const renderer = await Renderer.create(world, app);
  app.appendChild(hud); // keep HUD above the canvas

  // ---- input ----
  let harvesterHeld = false;
  let heldCollector: Collector | null = null;

  renderer.harvesterHit.on('pointerdown', () => {
    harvesterHeld = true;
    world.harvester.pointerDown();
  });
  for (const [c, s] of renderer.collectorSprites) {
    s.on('pointerdown', () => {
      if (!c.allowsManual || !c.active) return;
      heldCollector = c;
      world.manualCollectBegin(c);
    });
  }
  window.addEventListener('pointerup', () => {
    if (harvesterHeld) {
      world.harvester.pointerUp();
      harvesterHeld = false;
    }
    if (heldCollector) {
      world.manualCollectEnd(heldCollector);
      heldCollector = null;
    }
  });
  window.addEventListener('keydown', (e) => {
    if (e.repeat) return;
    if (e.code === 'Space') {
      e.preventDefault();
      world.harvester.spacePressed();
    } else if (e.code === 'KeyC') {
      world.keyboardCollectBegin();
    } else if (e.code === 'KeyH') {
      hud.style.display = hud.style.display === 'none' ? '' : 'none'; // hide HUD for clean captures
    }
  });
  window.addEventListener('keyup', (e) => {
    if (e.code === 'KeyC') world.keyboardCollectEnd();
  });

  // ---- debug: drive the real upgrade topics (no cost) so the wired handlers fire ----
  function grant(key: string): void {
    const u = um.get(key);
    if (u.timesPurchased < u.maxPurchases) u.timesPurchased += 1;
    bus.emit('upgrade.purchased', key);
    bus.emit('upgrade.purchased.' + key, u);
    renderButtons();
  }

  interface Btn {
    label: string;
    hint: string;
    run: () => void;
    count?: () => string;
  }
  const flag = (k: string) => (um.get(k).timesPurchased > 0 ? '✓' : '');
  const lvl = (k: string) => {
    const u = um.get(k);
    return u.timesPurchased > 0 ? `${u.timesPurchased}/${u.maxPurchases}` : '';
  };
  const BUTTONS: Btn[] = [
    { label: 'Divine Guidance', hint: 'enable click & hold', run: () => grant('MakoClickAndHold'), count: () => flag('MakoClickAndHold') },
    { label: 'Calibration ×2', hint: 'double summon / charge', run: () => grant('MakoManualSummonIncrease'), count: () => lvl('MakoManualSummonIncrease') },
    { label: '+ Manual Collector', hint: 'activate a 2nd collector', run: () => grant('MakoManualAdditionalCollector'), count: () => lvl('MakoManualAdditionalCollector') },
    { label: '+ Auto Collector', hint: 'Manifest Pylon (idle)', run: () => grant('MakoAutoAdditionalCollector'), count: () => lvl('MakoAutoAdditionalCollector') },
    { label: '+ Auto Harvester', hint: 'Channeler (+1 / ~1s)', run: () => grant('MakoAutoAdditionalHarvester'), count: () => lvl('MakoAutoAdditionalHarvester') },
    { label: 'Keyboard', hint: 'Space = charge, C = collect', run: () => grant('MakoKeyboard'), count: () => flag('MakoKeyboard') },
    { label: 'Grow Crystal', hint: 'unlock L2/L3 + extra orbs', run: () => grant('MakoRefinementUnlockCrystal'), count: () => flag('MakoRefinementUnlockCrystal') },
    { label: 'Attenuate', hint: 'orb level → more L2/L3', run: () => grant('MakoRefinementIncreaseLaserLevel'), count: () => lvl('MakoRefinementIncreaseLaserLevel') },
    { label: 'Amplify', hint: 'laser power → more orbs', run: () => grant('MakoRefinementIncreaseLaserPower'), count: () => lvl('MakoRefinementIncreaseLaserPower') },
    { label: 'Luck', hint: '+2% crit (×2 spawn)', run: () => grant('MakoRefinementCriticalHitChance'), count: () => lvl('MakoRefinementCriticalHitChance') },
    { label: 'Intellectus ×4', hint: 'Mako ×4 modifier', run: () => grant('ResearchProcessingMoreMako'), count: () => lvl('ResearchProcessingMoreMako') },
    { label: 'Faster suck ×2', hint: 'collector vel ×2', run: () => grant('ResearchProcessingMoreMakoSuck'), count: () => lvl('ResearchProcessingMoreMakoSuck') },
    { label: 'Burst +250 orbs', hint: 'stress the cloud', run: () => world.debugSpawnBurst(250) },
    { label: 'Toggle zones', hint: 'field / accel / targets', run: () => { renderer.showZones = !renderer.showZones; } },
  ];

  hud.innerHTML = `
    <div class="readout">
      <div class="title">What Lurks Below <span class="ph">▸ phase 2 · the Mako loop</span></div>
      <div class="stats" id="stats"></div>
      <div class="tip">Click / hold / Space the <b>harvester</b> to summon · hold a <b>collector</b> (or C) to suck</div>
    </div>
    <div class="panel">
      <div class="panel-h">debug — drives the real upgrade topics</div>
      <div class="btns" id="btns"></div>
    </div>`;

  const btnWrap = hud.querySelector('#btns') as HTMLDivElement;
  function renderButtons(): void {
    btnWrap.innerHTML = '';
    for (const b of BUTTONS) {
      const el = document.createElement('button');
      const c = b.count?.() ?? '';
      el.innerHTML = `${b.label}${c ? ` <i>${c}</i>` : ''}<small>${b.hint}</small>`;
      el.addEventListener('click', b.run);
      btnWrap.appendChild(el);
    }
  }
  renderButtons();

  // ---- HUD numbers ----
  const fmt = (n: number) =>
    n >= 1e6 ? (n / 1e6).toLocaleString(undefined, { maximumFractionDigits: 2 }) + 'M'
    : n >= 1e4 ? (n / 1e3).toLocaleString(undefined, { maximumFractionDigits: 1 }) + 'k'
    : Math.floor(n).toLocaleString();
  const COLOR: Record<ResourceType, string> = { Mako: '#ff4fa3', People: '#dcb78d', Science: '#56c9d6', Artifacts: '#9a7bd0' };
  const statsEl = hud.querySelector('#stats') as HTMLDivElement;
  function updateHud(): void {
    const res = RESOURCE_TYPES.filter((t) => rm.isUnlocked(t) || rm.amount(t) > 0)
      .map((t) => `<b style="color:${COLOR[t]}">${fmt(rm.amount(t))}</b> <span class="rl">${t}</span>`)
      .join('<span class="sep">·</span>');
    statsEl.innerHTML =
      `${res}<span class="sep">·</span>` +
      `<span class="rl">orbs</span> <b>${world.orbCount}</b>` +
      `<span class="sep">·</span><span class="rl">pool</span> <b>${world.pooledCount}</b>` +
      `<span class="sep">·</span><span class="rl">fps</span> <b>${Math.round(renderer.app.ticker.FPS)}</b>` +
      `<span class="sep">·</span><span class="rl">charge</span> <b>${world.harvester.chargeState}/9</b>`;
  }

  // ---- fixed-step loop (mirrors Unity's FixedUpdate accumulator) ----
  let acc = 0;
  const MAX_STEPS = 5; // clamp to avoid a spiral of death after a long stall
  renderer.app.ticker.add((ticker) => {
    acc += ticker.deltaMS / 1000;
    let steps = 0;
    while (acc >= SIM.fixedDeltaTime && steps < MAX_STEPS) {
      world.tick(SIM.fixedDeltaTime);
      acc -= SIM.fixedDeltaTime;
      steps += 1;
    }
    if (steps >= MAX_STEPS) acc = 0;
    renderer.syncFrame();
    updateHud();
  });

  // Dev-only: step the sim deterministically for headless capture (rAF throttles when
  // the page/window is hidden). Stripped from production builds.
  if (import.meta.env.DEV) {
    (window as unknown as { __step: (n?: number) => void }).__step = (n = 1) => {
      for (let i = 0; i < n; i++) world.tick(SIM.fixedDeltaTime);
      renderer.syncFrame();
      updateHud();
    };
  }
})();
