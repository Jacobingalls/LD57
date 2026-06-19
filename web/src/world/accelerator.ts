// MakoAccelerator — 1:1 port of Assets/Game/Mako/MakoAccelerator.cs.
// On the trigger ENTER edge, an orb that isn't already collectable or flashing gets a
// randomized upward impulse (3.0–5.5) and starts a 1s "flash"; when the flash ends
// (handled in makoWorld's orb-timer pass) it becomes collectable and publishes
// "mako.harvested". Orbs spawn inside this zone, so the enter edge fires on their
// first fixed step — mirroring Unity firing OnTriggerEnter2D for instantiate-inside.
import { SCENE, ORB } from '../data/tuning';
import { inAabb, addImpulse } from './physics';
import { normalized } from './vec2';
import type { Rng } from '../core/rng';
import type { MakoOrb } from './orb';

const A = SCENE.accelerator;
const IMP = ORB.accelerateImpulse;

export function applyAccelerator(orbs: MakoOrb[], rng: Rng): void {
  for (const orb of orbs) {
    const inside = orb.colliderEnabled && inAabb(orb.x, orb.y, A.x, A.y, A.halfW, A.halfH);
    const entered = inside && !orb.inAccelerator;
    orb.inAccelerator = inside;
    if (!entered) continue;

    // OnTriggerEnter2D guards:
    if (orb.collectable) continue; // if (makoOrb.Collectable) return;
    if (orb.flashing) continue;    // if (makoOrb.Flashing) return;

    // ApplyAccleratingImpulse(Random.Range(3.0, 5.5)):
    //   dir = (Random.Range(-0.7,0.7), Random.Range(0,1)).normalized; impulse = dir * scale
    const mag = rng.range(IMP.min, IMP.max);
    const dir = normalized({ x: rng.range(IMP.dirX.min, IMP.dirX.max), y: rng.range(IMP.dirY.min, IMP.dirY.max) });
    addImpulse(orb, dir.x * mag, dir.y * mag, ORB.body.mass);

    // MakeMakoOrbCollectable(afterDelay): show accelerated visuals + flash now;
    // becomes collectable + publishes "mako.harvested" after the delay elapses.
    orb.accelerated = true;
    orb.flashing = true;
    orb.flashTimer = ORB.accelerateCollectableDelaySec;
  }
}
