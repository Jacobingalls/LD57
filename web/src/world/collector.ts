// MakoCollector — 1:1 port of Assets/Game/Mako/MakoCollector.cs. Each collector seeks
// the nearest collectable orb, PD-"sucks" it to its drawTarget, captures it within
// captureDistance, fires a 0.25s laser, then converts it to Mako and (if auto) starts
// its cooldown. Two variants share this class: manual (AllowsManual, driven by
// begin/stop suck) and auto (AllowsAuto, the Manifest Pylon — cooldown-gated).
//
// NOTE: Unity runs the suck in FixedUpdate and the capture laser in a coroutine; both
// are folded into fixedUpdate() here (durations are in seconds → identical behavior).
import { COLLECTOR, SCENE } from '../data/tuning';
import { TOPICS } from '../data/pubsub';
import { driftTowardsTarget } from './physics';
import type { EventBus } from '../core/events';
import type { MakoOrb } from './orb';

export interface CollectorOpts {
  x: number;
  y: number;
  allowsManual: boolean;
  allowsAuto: boolean;
}

/** Converts a captured orb to resources and recycles it (= MakoManager.ConsumeMakoOrb). */
export type ConsumeFn = (orb: MakoOrb) => void;

export class Collector {
  readonly x: number;
  readonly y: number;
  readonly drawTargetX: number;
  readonly drawTargetY: number;
  readonly allowsManual: boolean;
  readonly allowsAuto: boolean;
  keyboard = false; // MakoKeyboard
  active = false;   // gameObject.activeSelf — scene seeds some on, upgrades activate the rest

  // PD scaling from upgrades: n = MakoManualCollectIncrease + ResearchProcessingMoreMakoSuck
  private maxVelMultiplier = 1; // 2^n
  private maxForceAdditive = 0; // n

  // runtime state (MakoCollector private fields)
  private isSucking = false;
  private isLasering = false;
  private laserTimer = 0;
  private currentCooldown = 0;
  private target: MakoOrb | null = null;
  private timeHoldingOrb = 0;

  constructor(
    opts: CollectorOpts,
    private readonly bus: EventBus,
    private readonly consume: ConsumeFn,
  ) {
    this.x = opts.x;
    this.y = opts.y;
    this.drawTargetX = opts.x + SCENE.collectorDrawTargetOffset.x;
    this.drawTargetY = opts.y + SCENE.collectorDrawTargetOffset.y;
    this.allowsManual = opts.allowsManual;
    this.allowsAuto = opts.allowsAuto;
  }

  // ---- manual control (MakoCollector.BeginSuck / StopSuck via the manual.* topics) ----
  beginSuck(): void {
    this.isSucking = true;
  }
  stopSuck(): void {
    this.isSucking = false;
    if (!this.isLasering) this.cleanUp();
  }

  // ---- per fixed step ----
  fixedUpdate(dt: number, orbs: MakoOrb[]): void {
    // HideLaser coroutine: convert the captured orb when the laser finishes.
    if (this.isLasering) {
      this.laserTimer -= dt;
      if (this.laserTimer <= 0) {
        const captured = this.target!;
        this.consume(captured); // ConsumeMakoOrb → ResourceManager.IncrementResource(Mako, baseValue)
        this.target = null;
        this.bus.emit(TOPICS.makoCollected);
        this.cleanUp();
        this.isLasering = false;
        this.currentCooldown = COLLECTOR.autoCooldownSec; // AutoCooldown (only used when AllowsAuto)
      }
    }

    if (this.allowsAuto) {
      if (this.currentCooldown > 0) {
        this.currentCooldown -= dt;
        this.isSucking = false;
      } else {
        this.isSucking = true;
      }
    }

    if (this.isSucking) {
      this.selectTarget(orbs);
      this.pullInTarget(dt);
    }
  }

  private selectTarget(orbs: MakoOrb[]): void {
    if (this.target !== null) return;

    // nearest collectable orb to drawTarget (sqr distance); earliest in list wins ties.
    let best: MakoOrb | null = null;
    let bestD = Infinity;
    for (const orb of orbs) {
      if (!orb.collectable) continue;
      const dx = this.drawTargetX - orb.x;
      const dy = this.drawTargetY - orb.y;
      const d = dx * dx + dy * dy;
      if (d < bestD) {
        bestD = d;
        best = orb;
      }
    }
    if (best === null) return;

    this.target = best;
    this.target.beingSucked = true;
  }

  private pullInTarget(dt: number): void {
    const t = this.target;
    if (t === null) return;
    if (t.captured) return;

    // NOTE: 1:1 quirk — _timeHoldingOrb is cumulative over the collector's lifetime and
    // never reset, so once it passes 5s of total pulling, every held orb gets its
    // collider disabled immediately (drops it from the field while it's reeled in).
    this.timeHoldingOrb += dt;
    if (this.timeHoldingOrb > COLLECTOR.stuckColliderDisableAfterSec) {
      t.colliderEnabled = false;
    }

    driftTowardsTarget(
      t,
      this.drawTargetX,
      this.drawTargetY,
      COLLECTOR.toVel,
      COLLECTOR.maxVel * this.maxVelMultiplier,
      COLLECTOR.maxForce + this.maxForceAdditive,
      COLLECTOR.gain,
    );

    const dx = this.drawTargetX - t.x;
    const dy = this.drawTargetY - t.y;
    const dist = Math.sqrt(dx * dx + dy * dy);
    if (dist < COLLECTOR.captureDistance && !this.isLasering) {
      t.captured = true;
      t.vx = 0; // StopDrifting()
      t.vy = 0;
      t.x = this.drawTargetX; // snap to drawTarget
      t.y = this.drawTargetY;
      this.showLaser();
    }
  }

  private showLaser(): void {
    this.isLasering = true;
    this.laserTimer = COLLECTOR.captureLaserSec;
  }

  private cleanUp(): void {
    if (this.target !== null) {
      this.target.colliderEnabled = true;
      this.target.beingSucked = false;
    }
    this.target = null;
  }

  // ---- upgrade hook (SetMaxVelMultiplier) ----
  /** n = MakoManualCollectIncrease + ResearchProcessingMoreMakoSuck purchases. */
  setSuckUpgrades(n: number): void {
    this.maxVelMultiplier = Math.pow(2, n);
    this.maxForceAdditive = n;
  }

  // ---- render/debug readouts ----
  get currentTarget(): MakoOrb | null {
    return this.target;
  }
  get lasering(): boolean {
    return this.isLasering;
  }
  get sucking(): boolean {
    return this.isSucking;
  }
}
