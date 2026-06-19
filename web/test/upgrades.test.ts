import { describe, it, expect } from 'vitest';
import { ResourceManager } from '../src/sim/resources';
import { UpgradeManager, type Upgrade } from '../src/sim/upgrades';
import { EventBus } from '../src/core/events';
import { UPGRADES } from '../src/data/upgrades';

const make = () => {
  const rm = new ResourceManager();
  const bus = new EventBus();
  return { rm, bus, um: new UpgradeManager(rm, bus) };
};
const curve = (u: Upgrade, r: 'Mako' | 'People' | 'Science', upto: number) =>
  Array.from({ length: upto }, (_, n) => {
    u.timesPurchased = n;
    return u.getCost(r);
  });

describe('data integrity', () => {
  it('all 29 upgrades load', () => {
    expect(UPGRADES.length).toBe(29);
  });
  it('every hidden/visible requirement points at a real upgrade', () => {
    const keys = new Set(UPGRADES.map((u) => u.key));
    for (const u of UPGRADES) {
      for (const r of [...(u.requires ?? []), ...(u.hiddenRequires ?? [])]) {
        expect(keys.has(r), `${u.key} requires missing ${r}`).toBe(true);
      }
    }
  });
});

describe('cost formula  cost(n) = floor(scale^n * base)  (1:1 with Unity)', () => {
  it('Calibration: base 5, scale x3 → 5, 15, 45', () => {
    const { um } = make();
    expect(curve(um.get('MakoManualSummonIncrease'), 'Mako', 3)).toEqual([5, 15, 45]);
  });
  it('Cottage: base 5, scale x1.5 → floors to 5, 7, 11, 16', () => {
    const { um } = make();
    expect(curve(um.get('PeopleSmallHouse'), 'Mako', 4)).toEqual([5, 7, 11, 16]);
  });
  it('Manifest Pylon: Mako x2 doubles; People stays flat at 1', () => {
    const { um } = make();
    const u = um.get('MakoAutoAdditionalCollector');
    expect(curve(u, 'Mako', 6)).toEqual([10, 20, 40, 80, 160, 320]);
    expect(curve(u, 'People', 6)).toEqual([1, 1, 1, 1, 1, 1]);
  });
  it('Situla 100th purchase follows scale 1.13 exactly', () => {
    const { um } = make();
    const u = um.get('ResearchProcessingBuyBucket');
    u.timesPurchased = 99;
    expect(u.getCost('Mako')).toBe(Math.floor(Math.pow(1.13, 99) * 50));
  });
  it('flat upgrades (no scale) never change cost', () => {
    const { um } = make();
    expect(curve(um.get('PeopleUnlockCathedral'), 'Mako', 1)).toEqual([100000]);
  });
});

describe('requirement graph (Hidden / Unlocked mirror C#)', () => {
  it('Divine Warrior: hidden until Divine Guidance, then locked until a Cottage', () => {
    const { rm, um } = make();
    expect(um.isHidden('MakoKeyboard')).toBe(true);

    rm.increment('Mako'); // 1 Mako
    expect(um.purchase('MakoClickAndHold')).toBe(true);
    expect(um.isHidden('MakoKeyboard')).toBe(false); // hidden-req satisfied
    expect(um.isUnlocked('MakoKeyboard')).toBe(false); // visible-req (Cottage) still unmet
  });

  it('Cathedral stays hidden until the gate story is understood', () => {
    const { um } = make();
    expect(um.isHidden('PeopleUnlockCathedral')).toBe(true); // needs GateStory3
  });
});

describe('purchase: pays, increments, emits on the bus', () => {
  it('spends the right resources and completes a one-off', () => {
    const { rm, um } = make();
    rm.increment('Mako', 5);
    expect(um.canAfford('MakoManualCollectIncrease')).toBe(true);
    expect(um.purchase('MakoManualCollectIncrease')).toBe(true);
    expect(rm.amount('Mako')).toBe(0);
    expect(um.get('MakoManualCollectIncrease').complete).toBe(true);
  });

  it('cannot buy past maxPurchases or when unaffordable', () => {
    const { rm, um } = make();
    expect(um.purchase('MakoClickAndHold')).toBe(false); // 0 Mako
    rm.increment('Mako');
    expect(um.purchase('MakoClickAndHold')).toBe(true);
    expect(um.purchase('MakoClickAndHold')).toBe(false); // already complete (max 1)
  });

  it('emits upgrade.purchased and the upgrade.publishes topics', () => {
    const { rm, um, bus } = make();
    const seen: string[] = [];
    bus.on('upgrade.purchased', (k) => seen.push(`purchased:${k}`));
    bus.on('research.factory.unlocked', () => seen.push('factory'));
    rm.increment('Mako', 100);
    rm.increment('People', 2);
    expect(um.purchase('ResearchProcessingUnlockFactory')).toBe(true);
    expect(seen).toContain('purchased:ResearchProcessingUnlockFactory');
    expect(seen).toContain('factory');
  });
});
