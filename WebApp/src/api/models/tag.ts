/** @format */

import { UniqueModel } from './unique';

export interface TagModel extends UniqueModel {
  name: string;
  creatoruid: string;
}
