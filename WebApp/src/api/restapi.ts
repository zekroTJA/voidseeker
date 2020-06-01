/** @format */

import EventEmitter from '../util/eventemitter';
import { InstanceStatusModel } from './models/instancestatus';
import { UserCreateModel, UserModel } from './models/user';

const PREFIX =
  process.env.NODE_ENV === 'development'
    ? 'https://localhost:5001/api'
    : '/api';

export class AuthenticationError extends Error {
  constructor() {
    super('authentication error');
  }
}

export class RestAPI {
  public static readonly events = new EventEmitter();

  // ------------------------------------------------------------
  // --- INSTANCE ---

  public static instanceStatus(): Promise<InstanceStatusModel> {
    return this.get('instance/status');
  }

  public static instanceInitialize(user: UserCreateModel): Promise<UserModel> {
    return this.post('instance/initialize', user);
  }

  // ------------------------------------------------------------
  // --- AUTH ---

  public static authLogin(
    username: string,
    password: string,
    remember: boolean = false
  ): Promise<any> {
    return this.post('auth/login', {
      username,
      password,
      remember,
    });
  }

  public static authLogout(): Promise<any> {
    return this.post('auth/logout');
  }

  // ------------------------------------------------------------
  // --- HELPERS ---

  private static get<T>(path: string): Promise<T> {
    return this.req<T>('GET', path, undefined);
  }

  private static post<T>(path: string, body?: any): Promise<T> {
    return this.req<T>('POST', path, body);
  }

  private static put<T>(path: string, body?: any): Promise<T> {
    return this.req<T>('PUT', path, body);
  }

  private static async req<T>(
    method: string,
    path: string,
    body?: any
  ): Promise<T> {
    const res = await window.fetch(`${PREFIX}/${path}`, {
      method,
      headers: {
        'content-type': 'application/json',
      },
      body: !!body ? JSON.stringify(body) : undefined,
      credentials: 'include',
    });

    if (res.status === 401) {
      this.events.emit('authentication-error', res);
      throw new AuthenticationError();
    }

    if (!res.ok) {
      this.events.emit('error', res);
      throw new Error(res.statusText);
    }

    return res.json();
  }
}
