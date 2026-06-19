// The four game resources. Mako starts unlocked; People/Science unlock via the
// "go deeper" idle modules. Artifacts is vestigial in the Unity build (defined and
// shown on the win screen, but nothing ever increments it) — kept for fidelity.
// Source: Assets/Game/ResourceManager.cs (enum ResourceType).

export type ResourceType = 'Mako' | 'People' | 'Science' | 'Artifacts';

export const RESOURCE_TYPES: readonly ResourceType[] = ['Mako', 'People', 'Science', 'Artifacts'];

export const STARTS_UNLOCKED: Record<ResourceType, boolean> = {
  Mako: true,
  People: false,
  Science: false,
  Artifacts: false,
};
