// EventBus — the web replacement for JamKit's PubSubManager.
// Mirrors its semantics: a topic maps to a set of listeners; emit() notifies all.
// (Reference: Library/PackageCache/info.jacobingalls.jamkit/Runtime/PubSub/PubSubManager.cs)

export type Handler<T = unknown> = (payload: T, topic: string) => void;

export class EventBus {
  private listeners = new Map<string, Set<Handler>>();

  /** Subscribe to a topic. Returns an unsubscribe function. */
  on<T = unknown>(topic: string, fn: Handler<T>): () => void {
    let set = this.listeners.get(topic);
    if (!set) {
      set = new Set();
      this.listeners.set(topic, set);
    }
    set.add(fn as Handler);
    return () => this.off(topic, fn as Handler);
  }

  off(topic: string, fn: Handler): void {
    this.listeners.get(topic)?.delete(fn);
  }

  emit<T = unknown>(topic: string, payload?: T): void {
    const set = this.listeners.get(topic);
    if (!set) return;
    // Copy so handlers may unsubscribe during dispatch without mutating mid-iteration.
    for (const fn of [...set]) fn(payload as unknown, topic);
  }
}
