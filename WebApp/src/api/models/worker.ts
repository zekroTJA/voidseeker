/** @format */

export enum WorkerStatus {
  FINISHED,
  INDEXING,
  COLLECTING,
  PACKING,
  CLEANUP,
}

export default interface WorkerModel {
  expires: Date;
  finished: boolean;
  status: WorkerStatus;
}
