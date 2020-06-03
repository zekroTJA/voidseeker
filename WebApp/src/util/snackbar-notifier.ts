/** @format */

import EventEmitter from './eventemitter';

export enum SnackBarType {
  INFO = 'info',
  ERROR = 'error',
  SUCCESS = 'success',
  WARN = 'warn',
}

export interface SnackBarEvent {
  content: string | JSX.Element;
  type: SnackBarType;
  duration: number;
}

export default class SnackBarNotifier {
  public static readonly events = new EventEmitter();

  public static show(
    content: string | JSX.Element,
    type: SnackBarType = SnackBarType.INFO,
    duration: number = 5000
  ) {
    this.events.emit('show', { content, type, duration } as SnackBarEvent);
  }

  public static hide() {
    this.events.emit('hide', null);
  }
}
