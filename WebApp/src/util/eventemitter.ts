/** @format */

export type EventHandler = (args: any) => void;
export type UnregisterHandler = () => void;

export default class EventEmitter {
  private handlers: { [key: string]: EventHandler[] } = {};

  public on(event: string, handler: EventHandler): UnregisterHandler {
    if (!this.handlers[event]) {
      this.handlers[event] = [];
    }
    this.handlers[event].push(handler);

    return () => this.unregister(event, handler);
  }

  public unregister(event: string, handler: EventHandler) {
    const h = this.handlers[event];
    if (!h) return;

    const i = h.indexOf(handler);
    if (i < 0) return;

    h.splice(i, 1);
  }

  public emit(event: string, args: any) {
    const h = this.handlers[event];
    if (!h) return;

    h.filter((handler) => !!handler).forEach((handler) => handler(args));
  }
}
