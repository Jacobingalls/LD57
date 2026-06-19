// UpgradeManager + Upgrade — 1:1 port of Assets/Game/UpgradeManager.cs.
// Cost: cost(n) = floor(costScale^timesPurchased * baseCost).
// Hidden / Unlocked mirror the C# getters exactly. Purchase() pays and bumps the
// counter (it does NOT itself check Unlocked — same as C#; the UI gates visibility).
import { UPGRADES, type UpgradeDef } from '../data/upgrades';
import type { ResourceType } from '../data/resources';
import type { ResourceManager } from './resources';
import type { EventBus } from '../core/events';

export class Upgrade {
  timesPurchased = 0;
  constructor(public readonly def: UpgradeDef) {}

  get key(): string {
    return this.def.key;
  }
  get maxPurchases(): number {
    return this.def.maxPurchases;
  }
  get purchased(): boolean {
    return this.timesPurchased > 0;
  }
  get complete(): boolean {
    return this.timesPurchased >= this.def.maxPurchases;
  }

  getCost(r: ResourceType): number {
    const base = this.def.baseCosts[r];
    if (base == null) return 0;
    const scale = this.def.costScale?.[r] ?? 1;
    return Math.floor(Math.pow(scale, this.timesPurchased) * base);
  }

  costs(): Partial<Record<ResourceType, number>> {
    const out: Partial<Record<ResourceType, number>> = {};
    for (const r of Object.keys(this.def.baseCosts) as ResourceType[]) out[r] = this.getCost(r);
    return out;
  }
}

export class UpgradeManager {
  private map = new Map<string, Upgrade>();

  constructor(
    private readonly rm: ResourceManager,
    private readonly bus?: EventBus,
  ) {
    for (const def of UPGRADES) this.map.set(def.key, new Upgrade(def));
  }

  get(key: string): Upgrade {
    const u = this.map.get(key);
    if (!u) throw new Error(`unknown upgrade: ${key}`);
    return u;
  }
  all(): Upgrade[] {
    return [...this.map.values()];
  }

  /** C# Upgrade.Hidden: forcedHidden || any visible-req is itself hidden || any hidden-req not purchased. */
  isHidden(key: string): boolean {
    const u = this.get(key);
    if (u.def.requires?.some((r) => this.isHidden(r))) return true;
    if (u.def.hiddenRequires?.some((r) => !this.get(r).purchased)) return true;
    return false;
  }

  /** C# Upgrade.Unlocked: all requires purchased && all hiddenRequires purchased. */
  isUnlocked(key: string): boolean {
    const u = this.get(key);
    if (u.def.requires?.some((r) => !this.get(r).purchased)) return false;
    if (u.def.hiddenRequires?.some((r) => !this.get(r).purchased)) return false;
    return true;
  }

  canAfford(key: string): boolean {
    for (const [r, c] of Object.entries(this.get(key).costs())) {
      if (this.rm.amount(r as ResourceType) < (c as number)) return false;
    }
    return true;
  }

  purchase(key: string): boolean {
    const u = this.get(key);
    if (!this.canAfford(key) || u.complete) return false;
    for (const [r, c] of Object.entries(u.costs())) this.rm.pay(r as ResourceType, c as number);
    u.timesPurchased += 1;
    this.bus?.emit('upgrade.purchased', u.key);
    this.bus?.emit('upgrade.purchased.' + u.key, u);
    for (const topic of u.def.publishes ?? []) this.bus?.emit(topic);
    return true;
  }
}
