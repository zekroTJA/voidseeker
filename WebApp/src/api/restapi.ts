/** @format */

import EventEmitter from '../util/eventemitter';
import { InstanceStatusModel } from './models/instancestatus';
import { UserCreateModel, UserLoginModel, UserModel } from './models/user';
import PageModel from './models/page';
import ImageModel from './models/image';
import { TagModel } from './models/tag';
import { WorkerStatus } from './models/worker';
import UserSettingsModel from './models/usersettings';
import { DeadlinedToken } from './models/deadlinedtoken';

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

  private static accessToken = (null as any) as DeadlinedToken;

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

  public static async authLogin(
    username: string,
    password: string,
    remember: boolean = false
  ): Promise<UserLoginModel> {
    const login = await this.post<UserLoginModel>('auth/login', {
      username,
      password,
      remember,
    });
    this.accessToken = login.accesstoken;
    return login;
  }

  public static authLogout(): Promise<any> {
    return this.post('auth/logout');
  }

  public static getAccessToken(): Promise<DeadlinedToken> {
    return this.get('auth/accesstoken');
  }

  // ------------------------------------------------------------
  // --- USERS ---

  public static createUser(user: UserCreateModel): Promise<UserModel> {
    return this.put('users', user);
  }

  public static users(
    offset = 0,
    size = 20,
    filter = ''
  ): Promise<PageModel<UserModel>> {
    const filterQuery = !!filter ? `&filter=${filter}` : '';
    return this.get(`users?offset=${offset}&size=${size}${filterQuery}`);
  }

  public static user(ident: string): Promise<UserModel> {
    return this.get(`users/${ident}`);
  }

  public static updateUser(
    uid: string,
    user: UserCreateModel
  ): Promise<UserModel> {
    return this.post(`users/${uid}`, user);
  }

  public static deleteUser(uid: string): Promise<any> {
    return this.delete(`users/${uid}`);
  }

  public static resendUserConfirmMail(): Promise<any> {
    return this.post(`users/@me/resendconfirm`);
  }

  // ------------------------------------------------------------
  // --- USERSETTINGS ---

  public static userSettings(): Promise<UserSettingsModel> {
    return this.get(`usersettings`);
  }

  public static setUserSettings(
    settings: UserSettingsModel
  ): Promise<UserSettingsModel> {
    return this.post(`usersettings`, settings);
  }

  // ------------------------------------------------------------
  // --- IMAGES ---

  public static images(
    includePublic = false,
    includeExplicit = false,
    offset = 0,
    size = 20,
    filter = '',
    excludes: string[] = [],
    sortBy = 'created',
    ascending = false
  ): Promise<PageModel<ImageModel>> {
    const excludesQuery = excludes.map((v) => `&exclude=${v}`).join('');
    const filterQuery = !!filter ? `&filter=${filter}` : '';
    return this.get(
      `images?offset=${offset}&size=${size}${filterQuery}&includePublic=${includePublic}&includeExplicit=${includeExplicit}&sortBy=${sortBy}&ascending=${ascending}${excludesQuery}`
    );
  }

  public static imageInfo(uid: string): Promise<ImageModel> {
    return this.get(`images/${uid}/info`);
  }

  public static uploadImage(file: File): Promise<ImageModel> {
    const formData = new FormData();
    formData.append('file', file);
    return this.req('PUT', 'images', formData, 'multipart/form-data');
  }

  public static updateImageInfo(
    uid: string,
    image: ImageModel
  ): Promise<ImageModel> {
    return this.post(`images/${uid}`, image);
  }

  public static deleteImage(uid: string): Promise<any> {
    return this.delete(`images/${uid}`);
  }

  public static imageUrl(uid: string): string {
    return `${PREFIX}/images/${uid}`;
  }

  public static imageThumbnailUrl(uid: string, size = 180): string {
    return `${PREFIX}/images/${uid}/thumbnail?size=${size}`;
  }

  public static imageUploadUrl(uid: string): string {
    return `${PREFIX}/images/${uid}/upload`;
  }

  // ------------------------------------------------------------
  // --- TAGS ---

  public static tags(
    offset = 0,
    size = 20,
    filter = '',
    fuzziness = -1
  ): Promise<PageModel<TagModel>> {
    const filterQuery = !!filter ? `&filter=${filter}` : '';
    return this.get(
      `tags?offset=${offset}&size=${size}&fuzziness=${fuzziness}${filterQuery}`
    );
  }

  public static tag(ident: string): Promise<TagModel> {
    return this.get(`tags/${ident}`);
  }

  public static updateTag(tag: TagModel): Promise<TagModel> {
    return this.post(`tags/${tag.uid}`, tag);
  }

  public static deleteTag(uid: string): Promise<any> {
    return this.delete(`tags/${uid}`);
  }

  // ------------------------------------------------------------
  // --- EXPORT ---

  public static initializeExport(
    includePublic = false,
    includeExplicit = false,
    filter = '',
    excludes: string[] = []
  ): Promise<WorkerStatus> {
    const excludesQuery = excludes.map((v) => `&exclude=${v}`).join('');
    const filterQuery = !!filter ? `&filter=${filter}` : '';
    return this.post(
      `export/initialize?includePublic=${includePublic}&includeExplicit=${includeExplicit}${filterQuery}${excludesQuery}`
    );
  }

  public static statusExport(): Promise<WorkerStatus> {
    return this.get(`export/status`);
  }

  public static cancelExport(): Promise<any> {
    return this.delete(`export`);
  }

  public static exportDownloadLink(): string {
    return `${PREFIX}/export/download`;
  }

  // ------------------------------------------------------------
  // --- MAILCONFIRM ---

  public static mailConformSet(
    token: string,
    dontEmitError: boolean = false
  ): Promise<WorkerStatus> {
    return this.post(
      `mailconfirm/confirmset?token=${token}`,
      undefined,
      !dontEmitError
    );
  }

  public static mailConfirmPasswordReset(
    username: string,
    emailaddress: string
  ): Promise<any> {
    return this.post(`mailconfirm/passwordreset`, { username, emailaddress });
  }

  public static mailConfirmPasswordResetConfirm(
    token: string,
    newpassword: string
  ): Promise<any> {
    return this.post(`mailconfirm/passwordresetconfirm`, {
      token,
      newpassword,
    });
  }

  // ------------------------------------------------------------
  // --- HELPERS ---

  public static get<T>(path: string, emitError: boolean = true): Promise<T> {
    return this.req<T>('GET', path, undefined, undefined, emitError);
  }

  private static post<T>(
    path: string,
    body?: any,
    emitError: boolean = true
  ): Promise<T> {
    return this.req<T>('POST', path, body, undefined, emitError);
  }

  private static put<T>(
    path: string,
    body?: any,
    emitError: boolean = true
  ): Promise<T> {
    return this.req<T>('PUT', path, body, undefined, emitError);
  }

  private static delete<T>(
    path: string,
    emitError: boolean = true
  ): Promise<T> {
    return this.req<T>('DELETE', path, undefined, undefined, emitError);
  }

  private static async req<T>(
    method: string,
    path: string,
    body?: any,
    contentType: string | undefined = 'application/json',
    emitError: boolean = true
  ): Promise<T> {
    if (this.accessToken && new Date(this.accessToken.deadline) <= new Date()) {
      this.accessToken = await this.getAccessToken();
    }

    let reqBody = undefined;
    if (body) {
      if (typeof body !== 'string' && contentType === 'application/json') {
        reqBody = JSON.stringify(body);
      } else {
        reqBody = body;
      }
    }

    const headers: { [key: string]: string } = {};
    if (contentType !== 'multipart/form-data') {
      headers['Content-Type'] = contentType;
    }
    if (this.accessToken) {
      headers['Authorization'] = `accessToken ${this.accessToken.token}`;
    }

    const res = await window.fetch(`${PREFIX}/${path}`, {
      method,
      headers,
      body: reqBody,
      credentials: 'include',
    });

    if (res.status === 401) {
      try {
        const resBody = await res.json();
        console.log(resBody);
        if (resBody === 'invalid access token') {
          this.accessToken = await this.getAccessToken();
          return this.req(method, path, body, contentType, emitError);
        }
      } catch {}
      if (emitError) this.events.emit('authentication-error', res);
      throw new AuthenticationError();
    }

    if (!res.ok) {
      if (emitError) this.events.emit('error', res);
      throw new Error(res.statusText);
    }

    if (res.status === 204 || res.headers.get('content-length') === '0') {
      return {} as T;
    }

    return res.json();
  }
}
