import { describe, it, expect } from 'vitest';
import { len, sub, scale, normalized, clampMagnitude } from '../src/world/vec2';
import { addForce, addImpulse, integrate, driftTowardsTarget, inAabb } from '../src/world/physics';
import { MakoOrb } from '../src/world/orb';
import { applyResourceField } from '../src/world/field';
import { applyAccelerator } from '../src/world/accelerator';
import { Rng } from '../src/core/rng';
import { SCENE, RESOURCE_FIELD, ORB, SIM } from '../src/data/tuning';
import { ResourceManager } from '../src/sim/resources';
import { UpgradeManager } from '../src/sim/upgrades';
import { EventBus } from '../src/core/events';
import { MakoWorld } from '../src/world/makoWorld';

const mkOrb = (x: number, y: number): MakoOrb => {
  const o = new MakoOrb();
  o.reset(x, y, 1, 1);
  return o;
};

describe('vec2 — mirrors UnityEngine.Vector2', () => {
  it('len / sub / scale', () => {
    expect(len({ x: 3, y: 4 })).toBe(5);
    expect(sub({ x: 5, y: 7 }, { x: 1, y: 2 })).toEqual({ x: 4, y: 5 });
    expect(scale({ x: 2, y: -3 }, 2)).toEqual({ x: 4, y: -6 });
  });

  it('normalized returns the unit vector, or zero below kEpsilon', () => {
    expect(normalized({ x: 3, y: 4 })).toEqual({ x: 0.6, y: 0.8 });
    expect(normalized({ x: 0, y: 0 })).toEqual({ x: 0, y: 0 });
  });

  it('ClampMagnitude shrinks only when longer than max', () => {
    expect(clampMagnitude({ x: 3, y: 4 }, 5)).toEqual({ x: 3, y: 4 }); // exactly at max
    expect(clampMagnitude({ x: 3, y: 4 }, 2.5)).toEqual({ x: 1.5, y: 2 });
    expect(clampMagnitude({ x: 1, y: 0 }, 5)).toEqual({ x: 1, y: 0 });
  });
});

describe('integrator — Unity AddForce / Impulse semantics @ dt=0.02, mass=1', () => {
  it('Impulse is an instant velocity change v += J/m', () => {
    const o = mkOrb(0, 0);
    addImpulse(o, 0, 3, ORB.body.mass);
    expect(o.vy).toBe(3);
    expect(o.vx).toBe(0);
  });

  it('Force is buffered, then integrated as v += (F/m)·dt and x += v·dt', () => {
    const o = mkOrb(0, 0);
    addForce(o, 0, 1); // ResourceField up-force
    integrate(o, ORB.body.mass, 0.02);
    expect(o.vy).toBeCloseTo(0.02, 12); // 1/1 * 0.02
    expect(o.y).toBeCloseTo(0.0004, 12); // 0.02 * 0.02
    expect(o.fy).toBe(0); // buffer cleared after the step
  });

  it('two steps accumulate velocity (constant force → linear ramp)', () => {
    const o = mkOrb(0, 0);
    addForce(o, 0, 1);
    integrate(o, ORB.body.mass, 0.02);
    addForce(o, 0, 1);
    integrate(o, ORB.body.mass, 0.02);
    expect(o.vy).toBeCloseTo(0.04, 12);
  });
});

describe('driftTowardsTarget — 1:1 with MakoOrb.DriftTowardsTarget (PD seek)', () => {
  it('far target clamps target-velocity to maxVel then force to maxForce', () => {
    // dist=(10,0) → toVel·dist=(25,0) clamp→(4,0); error=(4,0); gain·error=(12,0) clamp→(10,0)
    const o = mkOrb(0, 0);
    driftTowardsTarget(o, 10, 0, 2.5, 4, 10, 3);
    expect(o.fx).toBeCloseTo(10, 12);
    expect(o.fy).toBeCloseTo(0, 12);
  });

  it('near target: nothing clamps', () => {
    // dist=(1,0) → toVel·dist=(2.5,0); error=(2.5,0); gain·error=(7.5,0) (<maxForce)
    const o = mkOrb(0, 0);
    driftTowardsTarget(o, 1, 0, 2.5, 4, 10, 3);
    expect(o.fx).toBeCloseTo(7.5, 12);
  });
});

describe('inAabb — zone containment', () => {
  it('matches the ResourceField box bounds', () => {
    expect(inAabb(SCENE.field.x, SCENE.field.y, SCENE.field.x, SCENE.field.y, SCENE.field.halfW, SCENE.field.halfH)).toBe(true);
    expect(inAabb(100, 0, SCENE.field.x, SCENE.field.y, SCENE.field.halfW, SCENE.field.halfH)).toBe(false);
  });
});

describe('ResourceField.applyResourceField — up-force + center pull', () => {
  it('on the center axis the pull is straight up: total force = (0, upForce + 1)', () => {
    const o = mkOrb(SCENE.field.center.x, 0); // same X as centerAxis, inside the box
    applyResourceField([o]);
    expect(o.inField).toBe(true);
    expect(o.fx).toBeCloseTo(0, 9);
    expect(o.fy).toBeCloseTo(RESOURCE_FIELD.upForce + 1, 9); // up-force + magnitude-1 pull
  });

  it('orbs outside the box get no force', () => {
    const o = mkOrb(100, 0);
    applyResourceField([o]);
    expect(o.inField).toBe(false);
    expect(o.fx).toBe(0);
    expect(o.fy).toBe(0);
  });

  it('a disabled collider is dropped from the field', () => {
    const o = mkOrb(SCENE.field.center.x, 0);
    o.colliderEnabled = false;
    applyResourceField([o]);
    expect(o.inField).toBe(false);
    expect(o.fy).toBe(0);
  });
});

describe('MakoAccelerator.applyAccelerator — enter-edge kick', () => {
  it('an orb entering the zone gets the upward impulse and starts flashing', () => {
    const o = mkOrb(SCENE.accelerator.x, SCENE.accelerator.y); // inside the zone, inAccelerator=false
    // scripted: mag=3.0 (s=0), dirX=0 (s=0.5), dirY=0.5 → dir (0,0.5)→(0,1); impulse (0,3)
    applyAccelerator([o], new Rng((() => { const seq = [0, 0.5, 0.5]; let i = 0; return () => seq[i++]; })()));
    expect(o.vy).toBeCloseTo(3.0, 9);
    expect(o.vx).toBeCloseTo(0, 9);
    expect(o.inAccelerator).toBe(true);
    expect(o.flashing).toBe(true);
    expect(o.accelerated).toBe(true);
    expect(o.flashTimer).toBeCloseTo(ORB.accelerateCollectableDelaySec, 9);
  });

  it('does nothing for an already-collectable orb', () => {
    const o = mkOrb(SCENE.accelerator.x, SCENE.accelerator.y);
    o.collectable = true;
    applyAccelerator([o], new Rng(() => 0.5));
    expect(o.vy).toBe(0);
    expect(o.flashing).toBe(false);
  });
});

describe('end-to-end Mako loop — spawn → rise → accelerate → collect → convert', () => {
  it('one orb is pulled in by an auto-collector and increments Mako', () => {
    const bus = new EventBus();
    const rm = new ResourceManager();
    const um = new UpgradeManager(rm, bus);
    const world = new MakoWorld(rm, um, bus, new Rng(() => 0.5)); // deterministic

    world.activateAutoCollector(); // the Manifest Pylon variant runs on its own cadence
    world.spawnMakoOrbs(SCENE.spawnPoint.x, SCENE.spawnPoint.y);
    expect(world.orbCount).toBe(1);

    let collected = 0;
    bus.on('mako.collected', () => collected++);
    for (let i = 0; i < 3000; i++) world.tick(SIM.fixedDeltaTime); // 60s

    expect(collected).toBe(1);
    expect(world.orbCount).toBe(0);
    expect(rm.amount('Mako')).toBe(1); // baseValue 1, no modifiers
  });
});
