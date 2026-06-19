// The PubSub topology, lifted from the Unity build. In the port these string
// topics flow through core/EventBus instead of JamKit's PubSubManager.
//
// `publisher` is the C# class that calls Publish(topic). `listener` is the
// handler it ultimately drives — much of which is wired in the SCENE via
// PubSubListener components (UnityEvent), not in C#. That wiring is the
// connective tissue to recreate; it's recorded here so the world layer can
// re-establish each subscription explicitly.

export const TOPICS = {
  upgradePurchased: 'upgrade.purchased',
  upgradePurchasedPrefix: 'upgrade.purchased.', // + UpgradeType
  modulePurchased: 'module.purchased',
  makoCollected: 'mako.collected',
  makoHarvested: 'mako.harvested',
  harvesterLaserBegin: 'mako.harvester.laser.begin',
  harvesterLaserEnd: 'mako.harvester.laser.end',
  collectorManualBegin: 'mako.collector.manual.begin',
  collectorManualEnd: 'mako.collector.manual.end',
  scrolledSufficiently: 'mouse.scrolled.sufficiently',
  yeetRequest: 'people.yeet.request',
  resourceUnlocked: 'resource.unlocked',
  showGame: 'gameManager.showGame',
  showWin: 'gameManager.showWin',
  triggerEndCinematic: 'game.triggerEndCinimatic',
  // upgrade-driven (UpgradeManager publishes these from upgrade.publishes):
  refinementUnlocked: 'refinement.crystal.unlocked',
  refinementAttenuated: 'refinement.crystal.attenuated',
  refinementAmplified: 'refinement.crystal.amplified',
  refinementCrit: 'refinement.crystal.criticalHitChanceImproved',
  factoryUnlocked: 'research.factory.unlocked',
  factoryBucketAdded: 'research.factory.bucketAdded',
  factoryFasterChain: 'research.factory.fasterChain',
  factoryIncreasedEfficiency: 'research.factory.increasedEfficiency',
} as const;

export interface Wiring {
  topic: string;
  publisher: string;
  listener: string;
  scene?: boolean; // true => the subscription is wired in the Unity scene/prefab, not C#
}

export const WIRING: Wiring[] = [
  { topic: 'upgrade.purchased', publisher: 'UpgradeManager', listener: 'UpgradeList.Redraw', scene: true },
  { topic: 'module.purchased', publisher: 'IdleModule', listener: 'MakoManager.ModulePurchased (Mako x2)' },
  { topic: 'mako.collected', publisher: 'MakoCollector', listener: 'HUD / score', scene: true },
  { topic: 'mako.harvested', publisher: 'MakoAccelerator', listener: 'orb-collectable visuals', scene: true },
  { topic: 'mako.harvester.laser.begin', publisher: 'MakoHarvester', listener: 'RefinementCrystal.LaserDidFire + laser FX', scene: true },
  { topic: 'mako.harvester.laser.end', publisher: 'MakoHarvester', listener: 'laser FX off', scene: true },
  { topic: 'mako.collector.manual.begin', publisher: 'MakoCollector', listener: 'MakoCollector.BeginSuck', scene: true },
  { topic: 'mako.collector.manual.end', publisher: 'MakoCollector', listener: 'MakoCollector.StopSuck', scene: true },
  { topic: 'mouse.scrolled.sufficiently', publisher: 'CameraController', listener: 'TutorialManager.SufficientlyScrolled', scene: true },
  { topic: 'people.yeet.request', publisher: 'PeopleManager', listener: 'Cathedral.YeetMortalSoul (spend 1 person)', scene: true },
  { topic: 'resource.unlocked', publisher: 'ResourceManager', listener: 'HUD reveals a new counter', scene: true },
  { topic: 'gameManager.showGame', publisher: 'IntroDirector', listener: 'GameManager.ShowGame', scene: true },
  { topic: 'gameManager.showWin', publisher: 'EndGameCinimatic', listener: 'GameManager.ShowWinScreen', scene: true },
  { topic: 'game.triggerEndCinimatic', publisher: 'GateController.onOpenGate', listener: 'cinematic + bats + camera dive', scene: true },
  { topic: 'refinement.crystal.unlocked', publisher: 'UpgradeManager', listener: 'RefinementCrystal (activate)', scene: true },
  { topic: 'refinement.crystal.attenuated', publisher: 'UpgradeManager', listener: 'RefinementCrystal.UpdateLaserLevel', scene: true },
  { topic: 'refinement.crystal.amplified', publisher: 'UpgradeManager', listener: 'RefinementCrystal.UpdateLaserPower', scene: true },
  { topic: 'refinement.crystal.criticalHitChanceImproved', publisher: 'UpgradeManager', listener: '(orb-spawn crit, read in MakoManager)', scene: true },
  { topic: 'research.factory.unlocked', publisher: 'UpgradeManager', listener: 'ResearchCenter (activate)', scene: true },
  { topic: 'research.factory.bucketAdded', publisher: 'UpgradeManager', listener: 'ResearchCenter.AddBucket', scene: true },
  { topic: 'research.factory.fasterChain', publisher: 'UpgradeManager', listener: 'ResearchCenter.MakeChainFaster', scene: true },
  { topic: 'research.factory.increasedEfficiency', publisher: 'UpgradeManager', listener: 'ResearchCenter.IncreaseEfficiency', scene: true },
];
