// MakoWorld — the headless orchestrator for the Mako loop: it owns the orb pool, the
// harvester, the collectors, and drives the fixed-step simulation. It ports the parts
// of MakoManager.cs that aren't physics (the orb-spawn RNG, ConsumeMakoOrb, and the
// upgrade-purchase handlers spread across MakoManager/MakoHarvester/MakoCollector).
//
// The render layer reads this world's state; it never drives logic. Everything here is
// deterministic given the injected Rng, so the integrator and spawn RNG are testable.
import { ORB, REFINEMENT, SCENE, SIM, CONTAINMENT } from '../data/tuning';
import { TOPICS } from '../data/pubsub';
import { Rng } from '../core/rng';
import type { EventBus } from '../core/events';
import type { ResourceManager } from '../sim/resources';
import type { UpgradeManager } from '../sim/upgrades';
import { OrbPool, type MakoOrb } from './orb';
import { integrate, addImpulse } from './physics';
import { normalized } from './vec2';
import { applyResourceField } from './field';
import { applyAccelerator } from './accelerator';
import { pickRandomOrbLevel, pickNumberOfOrbs } from './spawnRng';
import { Harvester } from './harvester';
import { Collector } from './collector';

const MASS = ORB.body.mass;

export class MakoWorld {
  readonly pool = new OrbPool();
  readonly harvester: Harvester;
  readonly manualCollectors: Collector[];
  readonly autoCollectors: Collector[];
  /** Active collectors only (the ones that tick); mirrors Unity's SetActive gating. */
  readonly activeCollectors: Collector[] = [];

  constructor(
    private readonly rm: ResourceManager,
    private readonly um: UpgradeManager,
    private readonly bus: EventBus,
    private readonly rng: Rng = new Rng(),
  ) {
    this.harvester = new Harvester(bus, (x, y) => this.spawnMakoOrbs(x, y));

    this.manualCollectors = SCENE.manualCollectors.map(
      (p) => new Collector({ x: p.x, y: p.y, allowsManual: true, allowsAuto: false }, bus, (o) => this.consumeOrb(o)),
    );
    this.autoCollectors = SCENE.autoCollectors.map(
      (p) => new Collector({ x: p.x, y: p.y, allowsManual: false, allowsAuto: true }, bus, (o) => this.consumeOrb(o)),
    );

    // Scene seeds one active manual collector; the rest activate via upgrades.
    this.activateManualCollector();
    this.applySuckUpgrades(); // MakoCollector.Start → SetMaxVelMultiplier

    this.wireUpgradeHandlers();
  }

  get orbs(): MakoOrb[] {
    return this.pool.active;
  }

  // ============================ fixed-step simulation ======================
  /** One FixedUpdate at dt = SIM.fixedDeltaTime. */
  tick(dt: number): void {
    this.harvester.tick(dt); // charge / auto-charge / discharge → spawn

    const orbs = this.pool.active;

    applyAccelerator(orbs, this.rng); // OnTriggerEnter → accelerate impulse + start flash
    applyResourceField(orbs);         // up-force + center pull (buffered AddForce)

    for (const c of this.activeCollectors) c.fixedUpdate(dt, orbs); // select / drift / capture / convert

    for (const orb of orbs) integrate(orb, MASS, dt);

    // PORT containment: bounce orbs off a ceiling so the cloud stays bounded on-screen
    // (stand-in for Unity's orb-orb collision pile — see CONTAINMENT note in tuning).
    const cap = CONTAINMENT.ceilingY;
    for (const orb of orbs) {
      if (orb.y > cap) {
        orb.y = cap;
        if (orb.vy > 0) orb.vy = -orb.vy * CONTAINMENT.restitution;
      }
    }

    this.updateOrbTimers(dt);
  }

  /** MakoOrb.Update (60s auto-collectable) + the accelerator's 1s flash completion. */
  private updateOrbTimers(dt: number): void {
    for (const orb of this.pool.active) {
      // Accelerator flash → collectable + publish "mako.harvested".
      if (orb.flashing) {
        orb.flashTimer -= dt;
        if (orb.flashTimer <= 0) {
          orb.flashing = false;
          orb.collectable = true;
          this.bus.emit(TOPICS.makoHarvested);
        }
      }
      // "Orbs can sometimes miss the collector" → become collectable after 60s.
      if (!orb.collectable) {
        orb.timeToAutoBecomeCollectable -= dt;
        if (orb.timeToAutoBecomeCollectable <= 0) {
          orb.collectable = true;
          orb.accelerated = true; // ShowAcceleratedVisuals()
        }
      }
    }
  }

  // ============================ orb spawning (MakoManager) ==================
  /** SpawnMakoOrbs: spawn pickNumberOfOrbs() orbs at the point. */
  spawnMakoOrbs(x: number, y: number): void {
    const n = pickNumberOfOrbs(this.um, this.rng);
    for (let i = 0; i < n; i++) this.spawnMakoOrb(x, y);
  }

  /** SpawnMakoOrb: pick a level, spawn, apply the spawning impulse. */
  private spawnMakoOrb(x: number, y: number): void {
    const level = pickRandomOrbLevel(this.um, this.rng);
    const baseValue = level === 3 ? ORB.baseValue.l3 : level === 2 ? ORB.baseValue.l2 : ORB.baseValue.l1;
    const orb = this.pool.acquire(x, y, level, baseValue);

    // ApplySpawningImpulse(Random.Range(1.0,1.5)):
    const s = ORB.spawnImpulse;
    const mag = this.rng.range(s.min, s.max);
    const dir = normalized({ x: this.rng.range(s.dirX.min, s.dirX.max), y: this.rng.range(s.dirY.min, s.dirY.max) });
    addImpulse(orb, dir.x * mag, dir.y * mag, MASS);
  }

  /** ConsumeMakoOrb: convert to Mako (respecting the modifier stacks) and recycle. */
  consumeOrb(orb: MakoOrb): void {
    this.rm.increment('Mako', orb.baseValue); // IncrementResource(Mako, baseValue)
    this.pool.release(orb);
  }

  // ============================ collectors / upgrades =======================
  activateManualCollector(): boolean {
    const c = this.manualCollectors.find((x) => !x.active);
    if (!c) return false;
    c.active = true;
    this.activeCollectors.push(c);
    return true;
  }

  activateAutoCollector(): boolean {
    const c = this.autoCollectors.find((x) => !x.active);
    if (!c) return false;
    c.active = true;
    this.activeCollectors.push(c);
    return true;
  }

  /** SetMaxVelMultiplier: n = MakoManualCollectIncrease + ResearchProcessingMoreMakoSuck. */
  applySuckUpgrades(): void {
    const n =
      this.um.get('MakoManualCollectIncrease').timesPurchased +
      this.um.get('ResearchProcessingMoreMakoSuck').timesPurchased;
    for (const c of [...this.manualCollectors, ...this.autoCollectors]) c.setSuckUpgrades(n);
  }

  /** Wire the per-purchase handlers from MakoManager/MakoHarvester/MakoCollector.Start. */
  private wireUpgradeHandlers(): void {
    const on = (key: string, fn: () => void) => this.bus.on(TOPICS.upgradePurchasedPrefix + key, fn);

    // MakoHarvester.Start
    on('MakoClickAndHold', () => this.harvester.enableClickAndHold());
    on('MakoManualSummonIncrease', () => this.harvester.doubleSummon());
    on('MakoKeyboard', () => this.harvester.enableKeyboard());

    // MakoManager.Start
    on('MakoAutoAdditionalHarvester', () => this.harvester.addAutoHarvester());
    on('MakoManualAdditionalCollector', () => this.activateManualCollector());
    on('MakoAutoAdditionalCollector', () => this.activateAutoCollector());
    on('ResearchProcessingMoreMako', () => this.rm.addMultiplicativeModifier('Mako', REFINEMENT.intellectusMultPerLevel));
    on('MakoRefinementMoreValuableMako', () => this.rm.addAdditiveModifier('Mako', REFINEMENT.imbumentumAddPerLevel));

    // MakoCollector.Start (both upgrades recompute the shared multiplier; + keyboard)
    on('MakoManualCollectIncrease', () => this.applySuckUpgrades());
    on('ResearchProcessingMoreMakoSuck', () => this.applySuckUpgrades());
    on('MakoKeyboard', () => {
      for (const c of [...this.manualCollectors, ...this.autoCollectors]) c.keyboard = true;
    });

    // IdleModule.module.purchased → MakoManager.ModulePurchased (Mako x2)
    this.bus.on(TOPICS.modulePurchased, () =>
      this.rm.addMultiplicativeModifier('Mako', REFINEMENT.moduleMultOnPurchase),
    );
  }

  // ---- manual-collect input (the manual.* topics → BeginSuck / StopSuck) ----
  /** All keyboard-enabled active manual collectors begin sucking (C key down). */
  keyboardCollectBegin(): void {
    for (const c of this.activeCollectors) {
      if (c.allowsManual && c.keyboard) {
        c.beginSuck();
        this.bus.emit(TOPICS.collectorManualBegin);
      }
    }
  }
  keyboardCollectEnd(): void {
    for (const c of this.activeCollectors) {
      if (c.allowsManual && c.keyboard) {
        c.stopSuck();
        this.bus.emit(TOPICS.collectorManualEnd);
      }
    }
  }
  /** A specific manual collector was pressed/released (OnMouseDown/Up). */
  manualCollectBegin(c: Collector): void {
    if (!c.allowsManual) return;
    c.beginSuck();
    this.bus.emit(TOPICS.collectorManualBegin);
  }
  manualCollectEnd(c: Collector): void {
    if (!c.allowsManual) return;
    c.stopSuck();
    this.bus.emit(TOPICS.collectorManualEnd);
  }

  // ---- debug / stress readouts ----
  get orbCount(): number {
    return this.pool.active.length;
  }
  get pooledCount(): number {
    return this.pool.createdCount;
  }
  /** Burst-spawn N orbs at the spawn point (stress test). */
  debugSpawnBurst(count: number): void {
    for (let i = 0; i < count; i++) this.spawnMakoOrb(SCENE.spawnPoint.x, SCENE.spawnPoint.y);
  }
}

export { SIM };
