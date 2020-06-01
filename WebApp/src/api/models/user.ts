/** @format */

import { UniqueModel } from './unique';

export interface UserModel extends UniqueModel {
  username: string;
  displayname: string;
  description: string;
  lastlogin: Date;
  created: Date;
  isadmin: boolean;
  index: string;
}

export interface UserCreateModel extends UserModel {
  password?: string;
  oldpassword?: string;
  emailaddress?: string;
}
