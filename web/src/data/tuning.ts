// ============================================================================
//  TUNING — every serialized gameplay value, lifted 1:1 from the Unity build.
//  Lifted 2026-06-19 from Assets/**/*.prefab + GameScene YAML (guid->script
//  resolved, MonoBehaviour/Rigidbody2D/Collider2D fields read).
//
//  ⚠ Several values OVERRIDE the C# field defaults — those defaults were
//    placeholders never used at runtime. Provenance noted per block. The codex
//    (docs/index.html) was built from the C# defaults; THIS file is canonical.
// ============================================================================
import type { ResourceType } from './resources';

// ---- Mako orb (MakoOrb.prefab / MakoOrb 2 / MakoOrb 3) ----------------------
export const ORB = {
  collider: { radius: 0.16 },              // CircleCollider2D
  body: { mass: 1, gravityScale: 0, linearDrag: 0 }, // Rigidbody2D (drag not serialized -> 0)
  /** Resource yield per orb by level. ⚠ L2/L3 lifted from prefabs (codex had unknown). */
  baseValue: { l1: 1, l2: 2, l3: 4 },
  autoCollectableAfterSec: 60,             // code: MakoOrb._timeToAutoBecomeCollectable
  // Spawn/accelerate impulses: magnitude scale × a normalized random direction.
  // dir = (Random.Range(dirX), Random.Range(dirY)).normalized — code: MakoOrb.ApplySpawningImpulse / ApplyAccleratingImpulse.
  spawnImpulse: { min: 1.0, max: 1.5, dirX: { min: -1.0, max: 1.0 }, dirY: { min: 0.0, max: 1.0 } }, // MakoManager.SpawnMakoOrb
  accelerateImpulse: { min: 3.0, max: 5.5, dirX: { min: -0.7, max: 0.7 }, dirY: { min: 0.0, max: 1.0 } }, // MakoAccelerator
  accelerateCollectableDelaySec: 1.0,      // code: MakoAccelerator.MakeMakoOrbCollectable(afterDelay) — flash then collectable
  float: { speed: 0.3, distance: 0.3 },    // SpriteFloat (cosmetic bob)
} as const;

// ---- Harvester (the channeling device) -------------------------------------
export const HARVESTER = {
  maxCharge: 9,                  // code: MakoHarvester._maxChargeState (private)
  manualClickGain: 1,            // code (x2 per Calibration purchase)
  shortPressThresholdSec: 0.2,   // MakoHarvester.prefab
  pressAndHoldTriggerTimeSec: 0.5,
  laserDelayBeforeSpawnSec: 0.25,
} as const;

export const AUTO_HARVESTER = {
  cooldownSec: 1.0,              // MakoAutoHarvester.prefab — ticks +1 charge each cooldown
} as const;

// ---- Collector (PD "succ" seek controller) ---------------------------------
export const COLLECTOR = {
  toVel: 2.5, maxVel: 4, maxForce: 10, gain: 3, // MakoCollector.prefab
  autoCooldownSec: 2.0,          // auto (Manifest Pylon) variant pull cadence
  captureDistance: 0.025,
  captureLaserSec: 0.25,         // code: MakoCollector.ShowLaser(hideAfter) — laser before converting
  stuckColliderDisableAfterSec: 5.0,
  // Upgrade scaling (code): let n = MakoManualCollectIncrease + ResearchProcessingMoreMakoSuck
  //   maxVelMultiplier = 2^n ;  maxForceAdditive = n
} as const;

// ---- ResourceField (makes orbs rise out of the pit) ------------------------
export const RESOURCE_FIELD = {
  upForce: 1,        // ResourceField.prefab forceVector {x:0,y:1,z:0}
  centerPull: 1,     // code: AddForce((centerAxis - pos).normalized) -> magnitude 1
  // NB: the trigger region is the prefab box (1x1) scaled by its scene transform.
} as const;

// ---- Refinement crystal: orb-spawn RNG -------------------------------------
//  From MakoManager.pickRandomOrbLevel() / pickNumberOfOrbs().
//  orbLevel = Attenuate.timesPurchased + 1 ; laserPower = Amplify.timesPurchased + 1
export const REFINEMENT = {
  crystal: { animationSpeed: 2 },          // Refinement Crystal.prefab
  // level roll (chance of L2 / L3) once crystal unlocked:
  level: { baseL2: 0.1, baseL3: 0.01, addL2AtLevel2: 0.1, addL2AtLevel3: 0.2, addL3AtLevel3: 0.1 },
  // count roll:
  extraOrbChanceCrystal: 0.1,              // +1 orb at 10% once crystal unlocked
  extraOrbChancePerPowerTier: 0.5,         // +1 orb at 50% for power tier >= 2 and >= 3
  critChancePerLuck: 0.02,                 // Luck: +2% per level to x2 the whole spawn
  // value modifiers (applied in ResourceManager):
  imbumentumAddPerLevel: 10,               // additive +10 Mako value per Imbumentum
  intellectusMultPerLevel: 4,              // multiplicative x4 Mako per Intellectus
  moduleMultOnPurchase: 2,                 // x2 Mako when an idle module is purchased
} as const;

// ---- Research distillery (Bucket Chain + Research Center) -------------------
//  ⚠ ALL lifted from Research Center.prefab — the C# defaults (10/1/-10/0) were placeholders.
export const RESEARCH = {
  gooToMakeKnowledge: 64,        // goo per 1 Science  (code default was 10)
  scienceGrantedPerKnowledge: 1,
  maxKnowledgeHeight: 9,         // knowledge particle travel before crediting Science
  chain: {
    speed: 5,                    // chainSpeed   (code default was 1)
    damping: 0.5,                // speed-ramp smoothing
    topY: 0,
    bottomY: -42,                // loop length 42 units (code default was -10)
    startingBuckets: 8,          // bucketFreeCount seeded at 8 (code default was 0)
  },
  // Upgrades: Aliquid -> chainSpeed *= 2 (max 3) ; Studeo -> gooToMakeKnowledge /= 2 (max 3) ;
  //           Situla  -> +1 bucket (max 100)
} as const;

// ---- People & housing ------------------------------------------------------
export const PEOPLE = {
  cottage: { gain: 1, genTimeSec: 2 },     // House.prefab (basePeopleGain / PeopleGenerationTime)
  flat: { gain: 2, genTimeSec: 2 },        // House Large.prefab
  maxBuildingsPerLedge: 5,                 // PeopleCityLedge.prefab  (code default was 6)
  yeetCooldownSec: 1.0,                    // code: PeopleManager (not serialized)
  // sacrifice interval = yeetCooldownSec / ritekeeperCount ; Twin Reaping Rite -> 2 souls/launch
} as const;

// ---- Sacrifice physics (Cathedral + Sacrificed Mortal Soul) ----------------
export const SACRIFICE = {
  soul: { mass: 1, gravityScale: 1, collider: { radius: 0.1, offsetY: 0.0625 } },
  initialForce: 5, forceVariance: 0.2,     // Cathedral.prefab
  initialTorque: 1, torqueVariance: 0.2,
  dirX: { min: -0.2, max: 0.2 },
  doubleYeetOffset: 0.125,                  // 2/16 world units, code
} as const;

// ---- The Gate (leaky-bucket integrator) ------------------------------------
//  ⚠ Lifted from Gate Module Variant.prefab — the C# defaults (1.0 / 0.1) were
//    placeholders, so the codex's "need >=10 souls/s" was WRONG. Real math:
//    dProgress/dt = forcePerOpenAttempt*soulsPerSec - closingForcePerSecond,
//    opens at progress >= 1.0  ->  break-even at souls/s = 0.355/0.025 = 14.2/s.
export const GATE = {
  closingForcePerSecond: 0.355,
  forcePerOpenAttempt: 0.025,
  opensAtProgress: 1.0,
  progressToAnimateToWhenOpen: 6,          // animation only
  totalSizeToOpen: 2,                      // animation only (gate-half travel)
  sucker: { toVel: 2.5, maxVel: 4, maxForce: 10, gain: 3 }, // PeopleSucker funnel
} as const;

// ---- "Go deeper" idle modules (IdleModule variants) ------------------------
//  Purchased with Mako; reveal the next stratum (and sometimes a resource).
export interface ModuleDef { key: string; cost: number; unlocks: ResourceType | null; teaser: string; }
export const MODULES: ModuleDef[] = [
  { key: 'people',     cost: 3,     unlocks: 'People',  teaser: 'Explore the Depths' }, // People Module Variant
  { key: 'refinement', cost: 100,   unlocks: null,      teaser: 'Deeper!' },            // Mako Refinement Module Variant
  { key: 'research',   cost: 750,   unlocks: 'Science', teaser: 'Search for Answers' },  // Research Module Variant
  { key: 'gate',       cost: 10000, unlocks: null,      teaser: 'Do Not Relent!' },     // Gate Module Variant
];
// base Idle Module.prefab: cost 100, teaser "Go Deeper", unlocks nothing.

// ---- Camera / depth progression --------------------------------------------
export const CAMERA = {
  startY: 7,                     // Main Camera (LD57).prefab
  // Min/Max are overwritten at runtime by IdleModuleContainer from tagged modules;
  // the prefab seeds a wide clamp:
  minHeight: -100, maxHeight: 100,
  // key-hold accel (code): moveSpeed = min(20 * 2^(10*holdTime), 100) u/s
  keyAccelBase: 20, keyAccelMax: 100,
  // wheel scroll speed branched by browser (code) — normalize WheelEvent.deltaMode instead:
  scrollSpeed: { safariMac: 10, chromeMac: 100, other: 150 },
  scrolledSufficiently: { totalAbs: 20, orDepthY: -10 }, // fires mouse.scrolled.sufficiently once
} as const;

// ---- Endgame (BatSpawner / EndGameCinimatic) -------------------------------
export const ENDGAME = {
  cinematicDurationSec: 5,
  preRollSec: 4.75,
  finishHoldSec: 5,
  batRateNormal: 50,
  batRateEnd: 200,
} as const;

// ---- Fixed-step sim + render scale -----------------------------------------
export const SIM = {
  fixedDeltaTime: 0.02, // ProjectSettings/TimeManager.asset "Fixed Timestep" (50 Hz)
  pixelsPerUnit: 16,    // MakoHarvesterBeam.pixelsPerUnit / Aseprite import PPU
} as const;

// ---- PORT-only: cloud containment (⚠ NOT a Unity value) --------------------
//  In Unity the rising cloud is confined by orb-orb collision — orbs are non-trigger
//  CircleCollider2D bodies on the ResourceOrb layer, which self-collides (layer matrix),
//  so uncollected orbs pile up instead of coasting away (no drag, no gravity). That
//  N-body collision is deferred past Phase 2's four-force integrator spec. As a faithful
//  stand-in we cap how high an orb may rise and bounce it back, so the cloud stays a
//  bounded, on-screen mass. ceilingY sits a few units above the accelerator (9.82).
export const CONTAINMENT = { ceilingY: 13, restitution: 0.5 } as const;

// ---- Scene layout (world units) --------------------------------------------
//  Lifted 2026-06-19 from MakoManager.prefab + MakoHarvester.prefab +
//  Assets/Game/ResourceField/ResourceField.prefab + GameScene (Main Camera).
//  The Mako apparatus is a vertical column at X≈-6.1; collectors flank it; the
//  ResourceField is a tall up-push box whose centerAxis sits far ABOVE the
//  playfield, so orbs are drawn "toward the surface" (this is what makes them rise).
export const SCENE = {
  // Harvester device (clickable) world pos in MakoManager.prefab. It sits at the TOP;
  // orbs are summoned from the pit BELOW it (see spawnPoint).
  harvester: { x: -6.15, y: 9.94 },
  // Orbs spawn at MakoFocus — a MakoFocus.prefab nested under the harvester at local
  // (0, -9.09) → world (-6.15, 0.85): deep in the pit, INSIDE the field. They then
  // rise up the shaft. NOTE: MakoHarvester.RepositionFocus moves MakoFocus deeper as
  // "go deeper" modules unlock; Phase 2 (mako-only) uses the initial position.
  spawnPoint: { x: -6.15, y: 0.85 },

  // Accelerator trigger — child of the harvester at local (0,-0.12) → world (-6.15,9.82),
  // i.e. at the TOP of the shaft. Rising orbs funnel through it, get the kick, and
  // become collectable. CapsuleCollider2D size (2.5,1) vertical → modeled as an AABB
  // (half-extents); a faithful-enough trigger for the 0.16-radius orbs.
  accelerator: { x: -6.15, y: 9.82, halfW: 1.25, halfH: 0.5 },

  // ResourceField up-push box. ⚠ The harvester OVERRIDES the field instance's local pos
  // to (0,-117.7) (MakoHarvester.prefab PrefabInstance), so the field is the deep pit
  // BELOW the apparatus: world center = harvester + (0,-117.7) = (-6.15,-107.76),
  // box X[-15.24,2.94] Y[-223.4,7.91]. Scale (18.177345,231.34572) is NOT overridden.
  // BoxCollider2D size (1,1) × scale → the AABB. forceVector (0,1).
  field: {
    x: -6.15,
    y: -107.76,          // harvester.y (9.94) + local override (-117.7)
    halfW: 9.0886725,    // 18.177345 / 2
    halfH: 115.67286,    // 231.34572 / 2
    // centerAxis: field child, local (0, 0.5058) × field scale 231.34572 →
    // world = (-6.15, -107.76 + 0.5058*231.34572) = (-6.15, 9.2547), just below the
    // harvester. Orbs in the pit are drawn UP toward it — that's what lifts the cloud.
    center: { x: -6.15, y: 9.2547 },
  },

  // Collectors. drawTarget (orb suck-to point) is local (0,1.075) on each.
  // Manual = AllowsManual base prefab; Auto = the Manifest Pylon variant (AllowsAuto).
  collectorDrawTargetOffset: { x: 0, y: 1.075 },
  manualCollectors: [
    { x: -19, y: 8.5 },
    { x: 5.98, y: 8.5 },
  ],
  autoCollectors: [
    { x: -16.494, y: 7.011 },
    { x: -15.494, y: 7.011 },
    { x: 2.506, y: 7.011 },
    { x: 3.506, y: 7.011 },
    { x: 4.506, y: 7.011 },
  ],

  // Main Camera (LD57): position + orthographic size (half view height, world units).
  camera: { x: 0, y: 7, orthoSize: 15.3125 },
} as const;
