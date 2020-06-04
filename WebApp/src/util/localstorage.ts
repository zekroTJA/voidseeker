/** @format */

export default class LocalStorage {
  public static get<T>(key: string, def?: T): T | undefined {
    const valStr = window.localStorage.getItem(`vs_${key}`);
    if (!valStr) {
      return def;
    }
    return JSON.parse(valStr) as T;
  }

  public static set<T>(key: string, val: T) {
    const valStr = JSON.stringify(val);
    window.localStorage.setItem(`vs_${key}`, valStr);
  }
}
