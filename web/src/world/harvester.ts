// MakoHarvester (+ MakoAutoHarvester) — 1:1 port of Assets/Game/Mako/MakoHarvester.cs
// and MakoAutoHarvester.cs. Charge 0→9 via click / click-hold / Space; at 9 it
// discharges: publishes the laser-begin topic, waits laserDelayBeforeSpawnSec, then
// spawns the orbs (via the injected spawn fn = MakoManager.SpawnMakoOrbs) and resets.
//
// NOTE: Unity splits this across Update (charge cadence) and a HideLaser coroutine
// (the spawn delay); both are folded into the fixed-step tick() here — every duration
// is in seconds, so the behavior is identical and stays deterministic for tests.
import { HARVESTER, AUTO_HARVESTER, SCENE } from '../data/tuning';
import { TOPICS } from '../data/pubsub';
import type { EventBus } from '../core/events';

export type SpawnFn = (x: number, y: number) => void;

export class Harvester {
  chargeState = 0;
  discharging = false;
  private dischargeTimer = 0; // HideLaser countdown → spawn + reset

  // MakoHarvester input state
  private pressing = false;
  private pressTime = 0;
  private triggerTime = 0;

  // upgrade-driven flags / values
  manualClickGain = HARVESTER.manualClickGain; // ×2 per MakoManualSummonIncrease
  pressAndHoldEnabled = false;                  // MakoClickAndHold
  keyboard = false;                             // MakoKeyboard

  // each active MakoAutoHarvester is an independent cooldown timer
  private autoTimers: number[] = [];

  constructor(
    private readonly bus: EventBus,
    private readonly spawn: SpawnFn,
  ) {}

  // ---- input (driven by DOM events on the harvester sprite / keyboard) ----

  /** OnMouseDown */
  pointerDown(): void {
    if (this.discharging) return;
    this.pressing = true;
    this.pressTime = 0;
    this.triggerTime = HARVESTER.pressAndHoldTriggerTimeSec;
  }

  /** OnMouseUp: a short press (or hold disabled) charges once on release. */
  pointerUp(): void {
    if (this.pressTime <= HARVESTER.shortPressThresholdSec || !this.pressAndHoldEnabled) {
      this.chargeHarvester(this.manualClickGain);
    }
    this.pressing = false;
  }

  /** Input.GetKeyDown(Space) — only when not mouse-pressing (Unity's else branch). */
  spacePressed(): void {
    if (this.keyboard && !this.pressing) this.chargeHarvester(this.manualClickGain);
  }

  // ---- per fixed step ----
  tick(dt: number): void {
    // HideLaser coroutine: after the delay, spawn the orbs and reset to 0.
    if (this.discharging) {
      this.dischargeTimer -= dt;
      if (this.dischargeTimer <= 0) {
        this.spawn(SCENE.spawnPoint.x, SCENE.spawnPoint.y);
        this.discharging = false;
        this.chargeState = 0;
        this.bus.emit(TOPICS.harvesterLaserEnd);
      }
    }

    // Update: hold-to-charge cadence while the button is held.
    if (this.pressing) {
      this.pressTime += dt;
      this.triggerTime -= dt;
      if (this.triggerTime < 0 && this.pressAndHoldEnabled) {
        this.triggerTime = HARVESTER.pressAndHoldTriggerTimeSec;
        this.chargeHarvester(this.manualClickGain);
      }
    }

    // MakoAutoHarvester.Update: +1 charge per cooldown, while not discharging.
    for (let i = 0; i < this.autoTimers.length; i++) {
      this.autoTimers[i] -= dt;
      if (this.autoTimers[i] < 0 && !this.discharging) {
        this.autoChargeHarvester();
        this.autoTimers[i] = AUTO_HARVESTER.cooldownSec;
      }
    }
  }

  /** AutoChargeHarvester(): always +1, regardless of the summon upgrade. */
  autoChargeHarvester(): void {
    this.chargeHarvester();
  }

  private chargeHarvester(amount = 1): void {
    this.chargeState = Math.min(this.chargeState + amount, HARVESTER.maxCharge);
    if (this.chargeState === HARVESTER.maxCharge) {
      if (this.discharging) return;
      this.discharging = true;
      this.dischargeTimer = HARVESTER.laserDelayBeforeSpawnSec; // ShowLaser(hideAfter)
      this.bus.emit(TOPICS.harvesterLaserBegin);
    }
    // charge < max: only animation/SFX in Unity; render reads chargeState directly.
  }

  // ---- upgrade hooks (wired from UpgradeManager purchase handlers) ----
  enableClickAndHold(): void { this.pressAndHoldEnabled = true; }      // MakoClickAndHold
  enableKeyboard(): void { this.keyboard = true; }                     // MakoKeyboard
  doubleSummon(): void { this.manualClickGain *= 2; }                  // MakoManualSummonIncrease
  addAutoHarvester(): void { this.autoTimers.push(AUTO_HARVESTER.cooldownSec); } // MakoAutoAdditionalHarvester
  get autoHarvesterCount(): number { return this.autoTimers.length; }
}
