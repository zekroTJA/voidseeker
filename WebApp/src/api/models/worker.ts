/** @format */

export enum WorkerStatus {
  FINISHED,
  INDEXING,
  COLLECTING,
  PACKING,
  CLEANUP,
  ERRORED,
}

export interface WorkerExceptionModel {
  message: string;
  source: string;
}

export default interface WorkerModel {
  expires: Date;
  finished: boolean;
  status: WorkerStatus;
  exception: WorkerExceptionModel;
}
