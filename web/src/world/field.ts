// ResourceField — 1:1 port of Assets/Game/ResourceField/ResourceField.cs.
// FixedUpdate applies, to every orb inside the box trigger:
//   rb.AddForce(forceVector)                         // (0,1) — pushes orbs up
//   rb.AddForce((centerAxis - pos).normalized)       // magnitude-1 pull to the axis
// The centerAxis sits far ABOVE the playfield (SCENE.field.center.y ≈ 114.7), so the
// pull is essentially "up + toward X≈-6.1": that's what lifts orbs out of the pit and
// keeps the cloud as a coherent column.
import { SCENE, RESOURCE_FIELD } from '../data/tuning';
import { inAabb, addForce } from './physics';
import { normalized } from './vec2';
import type { MakoOrb } from './orb';

const F = SCENE.field;

export function applyResourceField(orbs: MakoOrb[]): void {
  for (const orb of orbs) {
    // A disabled collider is removed from the trigger's overlap set in Unity, so it
    // stops receiving the field force — mirror that with the colliderEnabled gate.
    const inside = orb.colliderEnabled && inAabb(orb.x, orb.y, F.x, F.y, F.halfW, F.halfH);
    orb.inField = inside;
    if (!inside) continue;

    addForce(orb, 0, RESOURCE_FIELD.upForce); // forceVector (0,1)

    const dir = normalized({ x: F.center.x - orb.x, y: F.center.y - orb.y });
    addForce(orb, dir.x * RESOURCE_FIELD.centerPull, dir.y * RESOURCE_FIELD.centerPull);
  }
}
