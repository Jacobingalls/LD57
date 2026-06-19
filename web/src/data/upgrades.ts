// The 29 upgrades, lifted 1:1 from Assets/Game/UpgradeManager.cs (Reset()).
// Cost scales geometrically: cost(n) = floor(costScale^timesPurchased * baseCost).
//   requires       = visible prerequisites (the row shows, greyed, before they're met)
//   hiddenRequires = hidden prerequisites   (the row is invisible until they're met)
// `effect` is descriptive; the actual gameplay hooks are wired in the sim/world layer.
import type { ResourceType } from './resources';

export type UpgradeCategory = 'mining' | 'refine' | 'research' | 'people' | 'gate';

export interface UpgradeDef {
  key: string;
  name: string;
  category: UpgradeCategory;
  maxPurchases: number;
  baseCosts: Partial<Record<ResourceType, number>>;
  costScale?: Partial<Record<ResourceType, number>>;
  requires?: string[];
  hiddenRequires?: string[];
  hideOnComplete?: boolean;
  effect: string;
  lore: string;
  publishes?: string[];
}

export const UPGRADES: UpgradeDef[] = [
  // ---- Mako Mining ----
  { key: 'MakoClickAndHold', name: 'Divine Guidance', category: 'mining', maxPurchases: 1,
    baseCosts: { Mako: 1 }, hideOnComplete: true,
    effect: 'Unlocks click + hold on the channeling device — the true start of the game.',
    lore: 'By the grace of God, we can begin to uncover the mysteries of this strange substance.' },
  { key: 'MakoManualSummonIncrease', name: 'Calibration', category: 'mining', maxPurchases: 3,
    baseCosts: { Mako: 5 }, costScale: { Mako: 3 }, hiddenRequires: ['MakoClickAndHold'],
    effect: 'Doubles Mako summoned per harvester charge (manualClickGain x2).',
    lore: 'Calibrate the channeling device. Mako is drawn more easily from the depths.' },
  { key: 'MakoManualCollectIncrease', name: 'Alignment', category: 'mining', maxPurchases: 1,
    baseCosts: { Mako: 5 }, hiddenRequires: ['MakoClickAndHold'],
    effect: 'Collector pull speed x2 (shared counter with Divina Guidantia).',
    lore: 'Improve alignment of the collector arrays. Mako is attracted at a faster rate.' },
  { key: 'MakoManualAdditionalCollector', name: 'Collector', category: 'mining', maxPurchases: 1,
    baseCosts: { Mako: 15 }, hiddenRequires: ['MakoClickAndHold'],
    effect: 'Activates one additional manual collector tower.',
    lore: 'Construct an additional Mako collector.' },
  { key: 'MakoAutoAdditionalCollector', name: 'Manifest Pylon', category: 'mining', maxPurchases: 6,
    baseCosts: { Mako: 10, People: 1 }, costScale: { Mako: 2, People: 1 }, hiddenRequires: ['PeopleSmallHouse'],
    effect: 'Raises an auto-collector pylon — passive collection (idle).',
    lore: 'A mortal soul toils away, collecting Mako sphaera from the skies.' },
  { key: 'MakoAutoAdditionalHarvester', name: 'Channeler', category: 'mining', maxPurchases: 8,
    baseCosts: { Mako: 10, People: 1 }, costScale: { Mako: 2, People: 1 }, hiddenRequires: ['PeopleSmallHouse'],
    effect: 'Raises an auto-harvester — passive summoning (+1 charge every ~1s).',
    lore: 'A mortal soul toils away, channeling for Mako sphaera.' },
  { key: 'MakoKeyboard', name: 'Divine Warrior', category: 'mining', maxPurchases: 1,
    baseCosts: { Mako: 250, People: 3 }, hiddenRequires: ['MakoClickAndHold'], requires: ['PeopleSmallHouse'],
    effect: 'Enables keyboard control — Space = harvester, C = collector.',
    lore: 'A deep, telepathic connection with the channeling device and the collector arrays.' },

  // ---- Mako Refinement ----
  { key: 'MakoRefinementUnlockCrystal', name: 'Grow Crystal', category: 'refine', maxPurchases: 1,
    baseCosts: { Mako: 50 }, hideOnComplete: true, publishes: ['refinement.crystal.unlocked'],
    effect: 'Unlocks the crystal: enables L2 (10%) / L3 (1%) orbs + 10% double-orb.',
    lore: 'Alters the beam to attract refined Mako sphaera.' },
  { key: 'MakoRefinementIncreaseLaserLevel', name: 'Attenuate', category: 'refine', maxPurchases: 2,
    baseCosts: { Mako: 50 }, costScale: { Mako: 2 }, hiddenRequires: ['MakoRefinementUnlockCrystal'],
    publishes: ['refinement.crystal.attenuated'],
    effect: '+1 orb level -> boosts the chance of high-quality (L2/L3) orbs.',
    lore: 'Increases the chance of attracting high-quality Mako sphaera from the depths.' },
  { key: 'MakoRefinementIncreaseLaserPower', name: 'Amplify', category: 'refine', maxPurchases: 2,
    baseCosts: { Mako: 50 }, costScale: { Mako: 2 }, hiddenRequires: ['MakoRefinementUnlockCrystal'],
    publishes: ['refinement.crystal.amplified'],
    effect: '+1 laser power -> 50% chance of an extra orb per tier.',
    lore: 'Increases the chance for additional Mako sphaera from the depths.' },
  { key: 'MakoRefinementCriticalHitChance', name: 'Luck', category: 'refine', maxPurchases: 10,
    baseCosts: { Mako: 200 }, costScale: { Mako: 1.5 }, hiddenRequires: ['MakoRefinementUnlockCrystal'],
    publishes: ['refinement.crystal.criticalHitChanceImproved'],
    effect: '+2% per level to DOUBLE all orbs from a fire (crit).',
    lore: "It's not luck, it's skill. A chance of getting double Mako sphaera." },
  { key: 'MakoRefinementMoreValuableMako', name: 'Imbumentum', category: 'refine', maxPurchases: 3,
    baseCosts: { Mako: 1000, Science: 2 }, costScale: { Mako: 2, Science: 2.5 }, hiddenRequires: ['ResearchProcessingUnlockFactory'],
    effect: '+10 to Mako base value per level (additive resource modifier).',
    lore: 'Imbue the crystal with Cognitio Aqua Diluta.' },

  // ---- Research Processing ----
  { key: 'ResearchProcessingUnlockFactory', name: 'Construe', category: 'research', maxPurchases: 1,
    baseCosts: { Mako: 100, People: 2 }, hideOnComplete: true, publishes: ['research.factory.unlocked'],
    effect: 'Builds the distillery and unlocks the Science resource.',
    lore: 'A powerful liquid capable of being distilled into Science.' },
  { key: 'ResearchProcessingBuyBucket', name: 'Situla', category: 'research', maxPurchases: 100,
    baseCosts: { Mako: 50 }, costScale: { Mako: 1.13 }, hiddenRequires: ['ResearchProcessingUnlockFactory'],
    publishes: ['research.factory.bucketAdded'],
    effect: '+1 bucket on the distillery chain (more goo throughput).',
    lore: 'Add another bucket to the distillery well.' },
  { key: 'ResearchProcessingFasterChain', name: 'Aliquid', category: 'research', maxPurchases: 3,
    baseCosts: { Mako: 250 }, costScale: { Mako: 10 }, hiddenRequires: ['ResearchProcessingUnlockFactory'],
    publishes: ['research.factory.fasterChain'],
    effect: 'Chain speed x2 per level (5 -> 10 -> 20 -> 40; base is 5, see tuning).',
    lore: 'Draw the Cognitio Aqua Diluta faster from the well.' },
  { key: 'ResearchProcessingIncresedEfficiency', name: 'Studeo', category: 'research', maxPurchases: 3,
    baseCosts: { Mako: 250 }, costScale: { Mako: 10 }, hiddenRequires: ['ResearchProcessingUnlockFactory'],
    publishes: ['research.factory.increasedEfficiency'],
    effect: 'Goo-per-Science /2 per level (64 -> 32 -> 16 -> 8; base is 64, see tuning).',
    lore: 'Reduces Cognitio Aqua Diluta needed to produce Science.' },
  { key: 'ResearchProcessingMoreMako', name: 'Intellectus', category: 'research', maxPurchases: 3,
    baseCosts: { Mako: 100, Science: 1, People: 1 }, costScale: { Mako: 10, Science: 2, People: 1 },
    hiddenRequires: ['ResearchProcessingUnlockFactory'],
    effect: 'Mako x4 multiplicative modifier per level. NOTE: tooltip says 2x; code adds x4.',
    lore: 'Understand the link between Cognitio Aqua Diluta and the substance.' },
  { key: 'ResearchProcessingMoreMakoSuck', name: 'Divina Guidantia', category: 'research', maxPurchases: 3,
    baseCosts: { Mako: 200, Science: 2, People: 1 }, costScale: { Mako: 10, Science: 2, People: 1 },
    hiddenRequires: ['ResearchProcessingUnlockFactory'],
    effect: 'Collector pull speed x2 per level (shared counter with Alignment).',
    lore: 'Per Dei gratiam... Mako is attracted at a faster rate.' },

  // ---- People ----
  { key: 'PeopleSmallHouse', name: 'Cottage', category: 'people', maxPurchases: 20,
    baseCosts: { Mako: 5 }, costScale: { Mako: 1.5 },
    effect: 'Builds a cottage: +1 People on construction.',
    lore: 'A damp, cold cottage, provides housing for a mortal soul.' },
  { key: 'PeopleUnlockLargeHouse', name: 'Sturdier Supports', category: 'people', maxPurchases: 1,
    baseCosts: { Mako: 500, Science: 5 }, hiddenRequires: ['ResearchProcessingUnlockFactory'],
    effect: 'Permits construction of Flats.',
    lore: "Reinforce New Gloomhollow's support structures." },
  { key: 'PeopleLargeHouse', name: 'Flat', category: 'people', maxPurchases: 16,
    baseCosts: { Mako: 500 }, costScale: { Mako: 1.5 }, hiddenRequires: ['PeopleUnlockLargeHouse'],
    effect: 'Builds a flat: +2 People on construction.',
    lore: 'A cramped, towering flat for mortal souls.' },
  { key: 'PeopleUnlockCathedral', name: 'Cathedral', category: 'people', maxPurchases: 1,
    baseCosts: { Mako: 100000, Science: 100 }, hiddenRequires: ['GateStory3'],
    effect: 'Activates the Cathedral — the sacrifice engine.',
    lore: 'A fine place for a fine end.' },
  { key: 'PeopleConvinceResidents', name: 'Extended Flock', category: 'people', maxPurchases: 1,
    baseCosts: { Mako: 100000, Science: 100 }, hiddenRequires: ['PeopleUnlockCathedral'],
    effect: 'Each home now passively generates People (~every 2s).',
    lore: 'Persuasive letters lure new blood to New Gloomhollow.' },
  { key: 'PeopleHireRitekeeper', name: 'Ritekeeper', category: 'people', maxPurchases: 8,
    baseCosts: { Mako: 100000, People: 1 }, costScale: { Mako: 1.1, People: 1 }, hiddenRequires: ['PeopleUnlockCathedral'],
    effect: '+1 sacrifice cadence — yeet interval = 1.0 / ritekeepers.',
    lore: 'Leads willing and unwilling alike to meet their fate.' },
  { key: 'PeopleBreedingPrograms', name: 'Fecund Harvest', category: 'people', maxPurchases: 1,
    baseCosts: { Mako: 2000000, Science: 100 }, hiddenRequires: ['PeopleConvinceResidents'],
    effect: '+1 additive People modifier — doubles passive generation.',
    lore: 'It is the solemn duty of each soul to produce the next generation. For the greater good.' },
  { key: 'PeopleUpgradeRitekeeper', name: 'Twin Reaping Rite', category: 'people', maxPurchases: 1,
    baseCosts: { Mako: 1000000, Science: 200 }, hiddenRequires: ['PeopleHireRitekeeper'],
    effect: 'Each sacrifice now launches TWO souls.',
    lore: "Two hapless souls are now cast into the gate's maw." },

  // ---- Gate ----
  { key: 'GateStory1', name: 'Investigate', category: 'gate', maxPurchases: 1,
    baseCosts: { Mako: 10000, Science: 5 },
    effect: 'Story beat 1 — reveals the gate research path.',
    lore: 'Fragmented carvings whisper of riches held beyond the gate.' },
  { key: 'GateStory2', name: 'Study', category: 'gate', maxPurchases: 1,
    baseCosts: { Mako: 100000, Science: 25 }, hiddenRequires: ['GateStory1'],
    effect: 'Story beat 2 — those who remain skeptical demand further study.',
    lore: 'Worm-eaten tomes unveil a terrifying truth.' },
  { key: 'GateStory3', name: 'Understand', category: 'gate', maxPurchases: 1,
    baseCosts: { Mako: 1000000, Science: 100 }, hiddenRequires: ['GateStory2'],
    effect: 'Story beat 3 -> unlocks the Cathedral. The gate demands blood.',
    lore: 'No denial remains — its mechanisms feed upon the despair of lost souls.' },
];

export const UPGRADES_BY_KEY: Record<string, UpgradeDef> = Object.fromEntries(
  UPGRADES.map((u) => [u.key, u]),
);
