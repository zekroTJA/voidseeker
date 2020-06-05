/** @format */

import { UniqueModel } from './unique';

export enum Grade {
  S,
  A,
  B,
  C,
  D,
  E,
  F,
}

export function gradeString(grade: Grade | undefined | null): string | null {
  if (grade === null || grade === undefined) return null;
  if (grade === Grade.S) return 'S';
  return String.fromCharCode(64 + grade);
}

export default interface ImageModel extends UniqueModel {
  owneruid: string;
  mimetype: string;
  filename: string;
  blobname: string;
  bucket: string;
  title: string;
  description: string;
  size: number;
  explicit: boolean;
  public: boolean;
  grade?: Grade;
  tagscombined: string;
  tagsarray: string[];
  md5hash: string;
  width: number;
  height: number;
}
