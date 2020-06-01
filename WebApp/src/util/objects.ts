/** @format */

export default class ObjectUtils {
  public static deepCopy<T>(obj: T): T {
    const json = JSON.stringify(obj);
    return JSON.parse(json);
  }
}
