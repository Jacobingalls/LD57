// Minimal 2D vector helpers, mirroring the subset of UnityEngine.Vector2 the Mako
// loop uses. Kept allocation-light: these back the *cold* paths (spawn/accelerate
// direction, the per-capture PD seek). The hot per-orb field force in physics.ts is
// written as scalar math to avoid per-step garbage.

export interface Vec2 {
  x: number;
  y: number;
}

// Unity Vector2.kEpsilon — used by .normalized to avoid dividing by ~0.
const EPSILON = 1e-5;

export const v2 = (x: number, y: number): Vec2 => ({ x, y });

export const len = (v: Vec2): number => Math.sqrt(v.x * v.x + v.y * v.y);

export const sub = (a: Vec2, b: Vec2): Vec2 => ({ x: a.x - b.x, y: a.y - b.y });

export const scale = (v: Vec2, s: number): Vec2 => ({ x: v.x * s, y: v.y * s });

/** C# Vector2.normalized: v/|v| when |v| > kEpsilon, else zero. */
export const normalized = (v: Vec2): Vec2 => {
  const m = len(v);
  return m > EPSILON ? { x: v.x / m, y: v.y / m } : { x: 0, y: 0 };
};

/** C# Vector2.ClampMagnitude(v, max): shrink to length `max` only if longer. */
export const clampMagnitude = (v: Vec2, max: number): Vec2 => {
  const sqr = v.x * v.x + v.y * v.y;
  if (sqr > max * max) {
    const m = Math.sqrt(sqr);
    const s = max / m;
    return { x: v.x * s, y: v.y * s };
  }
  return { x: v.x, y: v.y };
};
