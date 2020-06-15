/** @format */

import EventEmitter from '../util/eventemitter';
import { InstanceStatusModel } from './models/instancestatus';
import { UserCreateModel, UserModel } from './models/user';
import PageModel from './models/page';
import ImageModel from './models/image';
import { TagModel } from './models/tag';
import { WorkerStatus } from './models/worker';

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

  // public static initCreateImage(image: ImageModel): Promise<ImageModel> {
  //   return this.put('images', image);
  // }

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
  // --- HELPERS ---

  public static get<T>(path: string): Promise<T> {
    return this.req<T>('GET', path, undefined);
  }

  private static post<T>(path: string, body?: any): Promise<T> {
    return this.req<T>('POST', path, body);
  }

  private static put<T>(path: string, body?: any): Promise<T> {
    return this.req<T>('PUT', path, body);
  }

  private static delete<T>(path: string): Promise<T> {
    return this.req<T>('DELETE', path);
  }

  private static async req<T>(
    method: string,
    path: string,
    body?: any,
    contentType: string | undefined = 'application/json'
  ): Promise<T> {
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
      headers['content-type'] = contentType;
    }

    const res = await window.fetch(`${PREFIX}/${path}`, {
      method,
      headers,
      body: reqBody,
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

    if (res.headers.get('content-length') === '0') {
      return {} as T;
    }

    return res.json();
  }
}
