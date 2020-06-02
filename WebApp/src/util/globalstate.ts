/** @format */

import { UserModel } from '../api/models/user';
import PageModel from '../api/models/page';
import ImageModel from '../api/models/image';
import { RestAPI } from '../api/restapi';

export default class GlobalState {
  public images = { offset: 0, size: 0, data: [] } as PageModel<ImageModel>;

  private _selfUser = (null as any) as UserModel;

  public async selfUser(): Promise<UserModel> {
    if (!this._selfUser) {
      try {
        this._selfUser = await RestAPI.user('@me');
      } catch {}
    }

    return this._selfUser;
  }

  public setSelfUser(v: UserModel) {
    this._selfUser = v;
  }
}
