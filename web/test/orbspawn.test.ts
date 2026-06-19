import { describe, it, expect } from 'vitest';
import { pickRandomOrbLevel, pickNumberOfOrbs, type UpgradeLevels } from '../src/world/spawnRng';
import { Rng } from '../src/core/rng';

// Stub the UpgradeManager view: just the per-key purchase counts the rolls read.
const um = (levels: Record<string, number>): UpgradeLevels => ({
  get: (k) => ({ timesPurchased: levels[k] ?? 0 }),
});

// Rng whose [0,1) source plays back a fixed sequence; range(0,100) = s·100.
const seqRng = (...seq: number[]): Rng => {
  let i = 0;
  return new Rng(() => seq[i++]);
};
const constRng = (v: number): Rng => new Rng(() => v);

const CRYSTAL = 'MakoRefinementUnlockCrystal';
const LEVEL = 'MakoRefinementIncreaseLaserLevel';
const POWER = 'MakoRefinementIncreaseLaserPower';
const LUCK = 'MakoRefinementCriticalHitChance';

describe('pickRandomOrbLevel — 1:1 with MakoManager.cs', () => {
  it('with no crystal, every roll is level 1', () => {
    expect(pickRandomOrbLevel(um({}), constRng(0))).toBe(1);
    expect(pickRandomOrbLevel(um({}), constRng(0.999))).toBe(1);
  });

  it('crystal only → L2 @10%, L3 @1%', () => {
    expect(pickRandomOrbLevel(um({ [CRYSTAL]: 1 }), seqRng(0.005))).toBe(3); // < 0.01
    expect(pickRandomOrbLevel(um({ [CRYSTAL]: 1 }), seqRng(0.05))).toBe(2); // < 0.1
    expect(pickRandomOrbLevel(um({ [CRYSTAL]: 1 }), seqRng(0.5))).toBe(1);
  });

  it('crystal + Attenuate×1 (orbLevel 2) → L2 chance 0.2, L3 0.01', () => {
    const lv = um({ [CRYSTAL]: 1, [LEVEL]: 1 });
    expect(pickRandomOrbLevel(lv, seqRng(0.005))).toBe(3);
    expect(pickRandomOrbLevel(lv, seqRng(0.15))).toBe(2); // 0.1 < 0.15 < 0.2
    expect(pickRandomOrbLevel(lv, seqRng(0.5))).toBe(1);
  });

  it('crystal + Attenuate×2 (orbLevel 3) → L2 chance 0.4, L3 0.11', () => {
    const lv = um({ [CRYSTAL]: 1, [LEVEL]: 2 });
    expect(pickRandomOrbLevel(lv, seqRng(0.05))).toBe(3); // < 0.11
    expect(pickRandomOrbLevel(lv, seqRng(0.3))).toBe(2); // 0.11 < 0.3 < 0.4
    expect(pickRandomOrbLevel(lv, seqRng(0.5))).toBe(1);
  });

  it('Attenuate without the crystal still adds the +0.1 L2 chance (no L3)', () => {
    const lv = um({ [LEVEL]: 1 }); // orbLevel 2, no crystal → chance2 0.1, chance3 0
    expect(pickRandomOrbLevel(lv, seqRng(0.0))).toBe(2); // 0 < chance3(0) is false; 0 < 0.1 → 2
    expect(pickRandomOrbLevel(lv, seqRng(0.05))).toBe(2);
    expect(pickRandomOrbLevel(lv, seqRng(0.2))).toBe(1);
  });
});

describe('pickNumberOfOrbs — 1:1 with MakoManager.cs', () => {
  it('base case is always 1 orb', () => {
    expect(pickNumberOfOrbs(um({}), seqRng(0.5))).toBe(1); // single crit roll consumed
  });

  it('crystal adds +1 orb at 10%', () => {
    expect(pickNumberOfOrbs(um({ [CRYSTAL]: 1 }), seqRng(0.05, 0.5))).toBe(2); // 0.05 < 0.1
    expect(pickNumberOfOrbs(um({ [CRYSTAL]: 1 }), seqRng(0.5, 0.5))).toBe(1); // miss
  });

  it('laser power ≥2 and ≥3 each add +1 orb at 50%', () => {
    expect(pickNumberOfOrbs(um({ [POWER]: 1 }), seqRng(0.4, 0.5))).toBe(2); // power2 hit
    expect(pickNumberOfOrbs(um({ [POWER]: 1 }), seqRng(0.6, 0.5))).toBe(1); // power2 miss
    expect(pickNumberOfOrbs(um({ [POWER]: 2 }), seqRng(0.4, 0.4, 0.5))).toBe(3); // power2 + power3 hit
  });

  it('Luck (critChance = level×2 vs Range(0,100)) doubles the whole spawn', () => {
    // luck 10 → 20% crit. range(0,100) = s·100.
    expect(pickNumberOfOrbs(um({ [LUCK]: 10 }), seqRng(0.1))).toBe(2); // 20 > 10 → ×2
    expect(pickNumberOfOrbs(um({ [LUCK]: 10 }), seqRng(0.5))).toBe(1); // 20 > 50 false
  });

  it('everything stacks: crystal + power3 + crit → (1+1+1+1)×2 = 8', () => {
    const lv = um({ [CRYSTAL]: 1, [POWER]: 2, [LUCK]: 10 });
    // crystal hit, power2 hit, power3 hit, crit hit (range 10 < 20)
    expect(pickNumberOfOrbs(lv, seqRng(0.05, 0.4, 0.4, 0.1))).toBe(8);
  });
});
