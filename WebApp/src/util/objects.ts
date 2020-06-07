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

  public static objectForEach<TVal>(
    obj: { [key: string]: TVal },
    cb: (key: string, val: TVal) => void
  ) {
    Object.keys(obj).forEach((k) => {
      cb(k, obj[k]);
    });
  }

  public static objectMap<TVal, TRes>(
    obj: { [key: string]: TVal },
    cb: (key: string, val: TVal) => TRes
  ): TRes[] {
    const res: TRes[] = [];
    this.objectForEach(obj, (k, v) => res.push(cb(k, v)));
    return res;
  }
}
