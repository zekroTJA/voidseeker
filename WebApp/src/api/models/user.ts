/** @format */

import { UniqueModel } from './unique';

export enum EmailConfirmStatus {
  UNSET,
  UNCONFIRMED,
  CONFIRMED,
}

export interface UserModel extends UniqueModel {
  username: string;
  displayname: string;
  description: string;
  lastlogin: Date;
  isadmin?: boolean;
  imagescount?: number;
  emailaddress?: string;
  emailconfirmstatus?: EmailConfirmStatus;
}

export interface UserCreateModel extends UserModel {
  password?: string;
  oldpassword?: string;
}
