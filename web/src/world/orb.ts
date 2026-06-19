// MakoOrb — 1:1 port of Assets/Game/Mako/MakoOrb.cs (+ its Rigidbody2D state),
// plus an OrbPool. The pink "substance cloud" is hundreds–thousands of these live
// at once, so orbs are plain mutable structs (scalar x/y/vx/vy, no per-step alloc)
// and recycled through the pool rather than GC'd.
import { ORB } from '../data/tuning';

export type OrbLevel = 1 | 2 | 3;

export class MakoOrb {
  // --- pose + Rigidbody2D state (mass 1, gravityScale 0, drag 0; see ORB.body) ---
  x = 0;
  y = 0;
  vx = 0; // Rigidbody2D.linearVelocity.x
  vy = 0;
  fx = 0; // buffered AddForce(Force) for the current fixed step
  fy = 0;

  // --- identity ---
  level: OrbLevel = 1;
  baseValue = 1; // ORB.baseValue by level

  // --- MakoOrb.cs flags ---
  private _collectable = false;
  beingSucked = false;       // BeingSucked
  captured = false;          // Captured
  flashing = false;          // Flashing (true during the accelerator's 1s flash)
  accelerated = false;       // ShowAcceleratedVisuals() was called (base→accel sprite)
  colliderEnabled = true;    // CircleCollider2D.enabled — collector disables a stuck orb after 5s
  timeToAutoBecomeCollectable = ORB.autoCollectableAfterSec; // _timeToAutoBecomeCollectable = 60
  flashTimer = 0;            // remaining accelerator flash time before it becomes collectable

  // --- zone membership: replaces Unity's OnTriggerEnter/Exit edges ---
  inField = false;
  inAccelerator = false;

  // --- pool bookkeeping ---
  active = false;
  poolIndex = -1;

  /** C# MakoOrb.Collectable getter: `_collectable && !BeingSucked`. */
  get collectable(): boolean {
    return this._collectable && !this.beingSucked;
  }
  set collectable(v: boolean) {
    this._collectable = v;
  }
  /** Underlying `_collectable`, ignoring BeingSucked — for the render/visual swap. */
  get rawCollectable(): boolean {
    return this._collectable;
  }

  reset(x: number, y: number, level: OrbLevel, baseValue: number): void {
    this.x = x;
    this.y = y;
    this.vx = 0;
    this.vy = 0;
    this.fx = 0;
    this.fy = 0;
    this.level = level;
    this.baseValue = baseValue;
    this._collectable = false;
    this.beingSucked = false;
    this.captured = false;
    this.flashing = false;
    this.accelerated = false;
    this.colliderEnabled = true;
    this.timeToAutoBecomeCollectable = ORB.autoCollectableAfterSec;
    this.flashTimer = 0;
    this.inField = false;
    this.inAccelerator = false;
    this.active = true;
  }
}

/** Object pool for orbs. `active` mirrors MakoManager.makoOrbs (the live list). */
export class OrbPool {
  readonly active: MakoOrb[] = [];
  private readonly free: MakoOrb[] = [];
  private created = 0;

  /** Total MakoOrb objects ever allocated (pool high-water mark). */
  get createdCount(): number {
    return this.created;
  }

  acquire(x: number, y: number, level: OrbLevel, baseValue: number): MakoOrb {
    let o = this.free.pop();
    if (!o) {
      o = new MakoOrb();
      this.created++;
    }
    o.reset(x, y, level, baseValue);
    o.poolIndex = this.active.length;
    this.active.push(o);
    return o;
  }

  /** O(1) swap-remove from the active list; the object is recycled, not freed. */
  release(orb: MakoOrb): void {
    if (!orb.active) return;
    const i = orb.poolIndex;
    const last = this.active.length - 1;
    const moved = this.active[last];
    this.active[i] = moved;
    moved.poolIndex = i;
    this.active.pop();
    orb.active = false;
    orb.poolIndex = -1;
    this.free.push(orb);
  }
}
