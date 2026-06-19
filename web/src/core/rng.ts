// Rng — a thin stand-in for UnityEngine.Random, with an injectable [0,1) source so
// the orb-spawn RNG and physics jitter are deterministic under test.
//
// Unity's Random.Range(float min, float max) returns a uniform float in [min, max].
// We mirror that with `min + source()*(max-min)`. The orb-spawn rolls in
// MakoManager.cs use Range(0,1) and Range(0,100); spawn/accelerate directions use
// Range(-1,1)/Range(-0.7,0.7)/Range(0,1).

/** Returns a float in [0, 1). Defaults to Math.random; override for tests. */
export type RandomSource = () => number;

export class Rng {
  constructor(private readonly source: RandomSource = Math.random) {}

  /** C# Random.Range(float, float): uniform in [min, max]. */
  range(min: number, max: number): number {
    return min + this.source() * (max - min);
  }

  /** Convenience for Random.Range(0f, 1f). */
  value(): number {
    return this.source();
  }
}

/** Shared default instance for production code paths. */
export const rng = new Rng();
