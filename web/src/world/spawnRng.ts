// The orb-spawn RNG — pure functions lifted 1:1 from MakoManager.cs
// (pickRandomOrbLevel / pickNumberOfOrbs). Kept separate from MakoWorld so the exact
// probabilities can be unit-tested against the C# with a scripted Rng.
//
//   orbLevel  = MakoRefinementIncreaseLaserLevel.timesPurchased + 1
//   laserPower = MakoRefinementIncreaseLaserPower.timesPurchased + 1
import { REFINEMENT } from '../data/tuning';
import type { Rng } from '../core/rng';
import type { OrbLevel } from './orb';

/** Minimal view of UpgradeManager: just the per-key purchase counts these rolls read. */
export interface UpgradeLevels {
  get(key: string): { timesPurchased: number };
}

/** pickRandomOrbLevel(): chances of L2/L3 grow with the crystal + laser level. */
export function pickRandomOrbLevel(um: UpgradeLevels, rng: Rng): OrbLevel {
  const hasCrystal = um.get('MakoRefinementUnlockCrystal').timesPurchased > 0;
  const orbLevel = um.get('MakoRefinementIncreaseLaserLevel').timesPurchased + 1;
  const L = REFINEMENT.level;

  let chance2 = 0;
  let chance3 = 0;
  if (hasCrystal) {
    chance2 = L.baseL2; // 0.1
    chance3 = L.baseL3; // 0.01
  }
  if (orbLevel >= 2) chance2 += L.addL2AtLevel2; // +0.1
  if (orbLevel >= 3) {
    chance2 += L.addL2AtLevel3; // +0.2
    chance3 += L.addL3AtLevel3; // +0.1
  }

  const r = rng.value(); // Random.Range(0f, 1f), evaluated once for both thresholds
  if (r < chance3) return 3;
  if (r < chance2) return 2;
  return 1;
}

/** pickNumberOfOrbs(): laser power and the crystal add extra orbs; Luck can double all. */
export function pickNumberOfOrbs(um: UpgradeLevels, rng: Rng): number {
  let n = 1;
  const laserPower = um.get('MakoRefinementIncreaseLaserPower').timesPurchased + 1;
  const hasCrystal = um.get('MakoRefinementUnlockCrystal').timesPurchased > 0;
  const luck = um.get('MakoRefinementCriticalHitChance').timesPurchased;

  if (hasCrystal && rng.value() < REFINEMENT.extraOrbChanceCrystal) n += 1; // 10%
  if (laserPower >= 2 && rng.value() < REFINEMENT.extraOrbChancePerPowerTier) n += 1; // 50%
  if (laserPower >= 3 && rng.value() < REFINEMENT.extraOrbChancePerPowerTier) n += 1; // 50%

  // crit: critChance = luck*2 (out of 100), compared to Random.Range(0,100) → ×2 the spawn.
  const critPercent = luck * REFINEMENT.critChancePerLuck * 100;
  if (critPercent > rng.range(0, 100)) n *= 2;

  return n;
}
