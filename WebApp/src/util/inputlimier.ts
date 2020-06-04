/** @format */

type Timer = ReturnType<typeof setTimeout>;

export default class InputLimiter {
  private timer: Timer | null = null;

  constructor(private limit: number) {}

  public input<T>(val: T, cb: (val: T) => void) {
    if (this.timer) {
      clearTimeout(this.timer);
    }
    this.timer = setTimeout(() => cb(val), this.limit);
  }
}
