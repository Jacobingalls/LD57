// ResourceManager — 1:1 port of Assets/Game/ResourceManager.cs.
// Holds the four resources and the additive / multiplicative modifier stacks.
// Gain formula (the gated path): gain = baseGain * n * Σ(additive) * Π(multiplicative),
// truncated toward zero to match C#'s (int) cast.
import { RESOURCE_TYPES, STARTS_UNLOCKED, type ResourceType } from '../data/resources';

export interface Resource {
  type: ResourceType;
  amount: number;
  lifetimeAmount: number;
  baseGain: number;
  unlocked: boolean;
}

export class ResourceManager {
  private resources = new Map<ResourceType, Resource>();
  private additive = new Map<ResourceType, number[]>();
  private multiplicative = new Map<ResourceType, number[]>();

  constructor() {
    this.reset();
  }

  reset(): void {
    for (const t of RESOURCE_TYPES) {
      this.resources.set(t, {
        type: t,
        amount: 0,
        lifetimeAmount: 0,
        baseGain: 1,
        unlocked: STARTS_UNLOCKED[t],
      });
      // C# seeds both stacks with a single 1.0 (additive folds from 0, multiplicative from 1).
      this.additive.set(t, [1]);
      this.multiplicative.set(t, [1]);
    }
  }

  amount(t: ResourceType): number {
    return this.resources.get(t)!.amount;
  }
  lifetime(t: ResourceType): number {
    return this.resources.get(t)!.lifetimeAmount;
  }
  isUnlocked(t: ResourceType): boolean {
    return this.resources.get(t)!.unlocked;
  }
  unlock(t: ResourceType): void {
    this.resources.get(t)!.unlocked = true;
  }

  /** Spend resources directly (C# PayResource) — bypasses modifiers. */
  pay(t: ResourceType, n: number): void {
    this.resources.get(t)!.amount -= n;
  }

  addAdditiveModifier(t: ResourceType, m: number): void {
    this.additive.get(t)!.push(m);
  }
  addMultiplicativeModifier(t: ResourceType, m: number): void {
    this.multiplicative.get(t)!.push(m);
  }

  /** C# IncrementResource(type, gain=n): respects the modifier stacks. Returns the gain applied. */
  increment(t: ResourceType, n = 1): number {
    const r = this.resources.get(t)!;
    const add = this.additive.get(t)!.reduce((a, b) => a + b, 0);
    const mul = this.multiplicative.get(t)!.reduce((a, b) => a * b, 1);
    const gain = Math.trunc(r.baseGain * n * add * mul);
    r.amount += gain;
    r.lifetimeAmount += gain;
    return gain;
  }
}
