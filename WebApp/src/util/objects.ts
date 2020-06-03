/** @format */

export default class ObjectUtils {
  public static deepCopy<T>(obj: T): T {
    const json = JSON.stringify(obj);
    return JSON.parse(json);
  }

  public static enumToArray(e: object): any[][] {
    let keys = Object.keys(e);
    const sliceStart = keys.length / 2;
    keys = keys.slice(sliceStart);
    const values = Object.values(e).slice(sliceStart);

    return keys.map((k, i) => [k, values[i]]);
  }
}
