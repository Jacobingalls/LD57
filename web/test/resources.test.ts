import { describe, it, expect } from 'vitest';
import { ResourceManager } from '../src/sim/resources';

describe('ResourceManager — modifier math (1:1 with Unity)', () => {
  it('a plain increment yields baseGain * 1 = 1', () => {
    const rm = new ResourceManager();
    expect(rm.increment('Mako')).toBe(1);
    expect(rm.amount('Mako')).toBe(1);
  });

  it('additive +1 (Fecund Harvest) doubles People generation', () => {
    const rm = new ResourceManager();
    rm.addAdditiveModifier('People', 1); // stack becomes [1, 1] -> sum 2
    expect(rm.increment('People')).toBe(2);
  });

  it('multiplicative x4 (Intellectus) quadruples Mako', () => {
    const rm = new ResourceManager();
    rm.addMultiplicativeModifier('Mako', 4);
    expect(rm.increment('Mako', 1)).toBe(4);
  });

  it('additive and multiplicative compose: (1+1) * 4 = 8 per unit', () => {
    const rm = new ResourceManager();
    rm.addAdditiveModifier('Mako', 1);
    rm.addMultiplicativeModifier('Mako', 4);
    expect(rm.increment('Mako', 1)).toBe(8);
  });

  it('truncates toward zero, matching the C# (int) cast', () => {
    const rm = new ResourceManager();
    rm.addMultiplicativeModifier('Mako', 0.5);
    expect(rm.increment('Mako', 1)).toBe(0); // floor(1 * 1 * 1 * 0.5) = 0
  });

  it('lifetime accumulates and never decreases when paying', () => {
    const rm = new ResourceManager();
    rm.increment('Mako', 10);
    rm.pay('Mako', 10);
    expect(rm.amount('Mako')).toBe(0);
    expect(rm.lifetime('Mako')).toBe(10);
  });

  it('only Mako starts unlocked', () => {
    const rm = new ResourceManager();
    expect(rm.isUnlocked('Mako')).toBe(true);
    expect(rm.isUnlocked('People')).toBe(false);
    expect(rm.isUnlocked('Science')).toBe(false);
    expect(rm.isUnlocked('Artifacts')).toBe(false);
  });
});
