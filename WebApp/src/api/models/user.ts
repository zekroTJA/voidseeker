/** @format */

import { UniqueModel } from './unique';

export interface UserModel extends UniqueModel {
  username: string;
  displayname: string;
  description: string;
  lastlogin: Date;
  isadmin?: boolean;
  imagescount?: number;
  emailaddress?: string;
}

export interface UserCreateModel extends UserModel {
  password?: string;
  oldpassword?: string;
}
