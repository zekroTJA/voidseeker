/** @format */

export default class ResponseErrorUtil {
  public static getMessage(data: any): string {
    if (data.message) {
      return data.message;
    }

    if (data.errors) {
      var errors = Object.values(data.errors) as string[];
      if (errors.length > 0) {
        return errors[0];
      }
    }

    return 'unknown';
  }
}
