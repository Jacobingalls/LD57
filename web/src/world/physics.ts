// Fixed-step 2D integrator, mirroring Unity's FixedUpdate + Physics2D for the orbs
// (mass 1, gravityScale 0, drag 0). Two force kinds, exactly as Unity exposes them:
//   - AddForce(ForceMode2D.Force)   → buffered, integrated as v += (F/m)·dt
//   - AddForce(ForceMode2D.Impulse) → instant velocity change, v += J/m
// Integration is semi-implicit (symplectic) Euler: velocity first, then position —
// the same scheme Unity's solver uses, which is what reproduces the float/swirl feel.
import type { MakoOrb } from './orb';
import { clampMagnitude, scale } from './vec2';

/** Axis-aligned box containment test (ResourceField box / accelerator trigger). */
export const inAabb = (
  px: number,
  py: number,
  cx: number,
  cy: number,
  halfW: number,
  halfH: number,
): boolean => px >= cx - halfW && px <= cx + halfW && py >= cy - halfH && py <= cy + halfH;

/** Rigidbody2D.AddForce(f, ForceMode2D.Force): accumulate for this fixed step. */
export const addForce = (orb: MakoOrb, fx: number, fy: number): void => {
  orb.fx += fx;
  orb.fy += fy;
};

/** Rigidbody2D.AddForce(j, ForceMode2D.Impulse): apply an instant velocity change. */
export const addImpulse = (orb: MakoOrb, ix: number, iy: number, mass: number): void => {
  orb.vx += ix / mass;
  orb.vy += iy / mass;
};

/** Integrate one orb by `dt`, then clear the force buffer for the next step. */
export const integrate = (orb: MakoOrb, mass: number, dt: number): void => {
  orb.vx += (orb.fx / mass) * dt;
  orb.vy += (orb.fy / mass) * dt;
  orb.x += orb.vx * dt;
  orb.y += orb.vy * dt;
  orb.fx = 0;
  orb.fy = 0;
};

/**
 * 1:1 with MakoOrb.DriftTowardsTarget — the collector's PD "seek":
 *   tgtVel = ClampMagnitude(toVel · (target - pos), maxVel)
 *   force  = ClampMagnitude(gain · (tgtVel - velocity), maxForce)   // buffered AddForce
 */
export const driftTowardsTarget = (
  orb: MakoOrb,
  tx: number,
  ty: number,
  toVel: number,
  maxVel: number,
  maxForce: number,
  gain: number,
): void => {
  const tgtVel = clampMagnitude(scale({ x: tx - orb.x, y: ty - orb.y }, toVel), maxVel);
  const force = clampMagnitude(scale({ x: tgtVel.x - orb.vx, y: tgtVel.y - orb.vy }, gain), maxForce);
  addForce(orb, force.x, force.y);
};
