/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import WorkerModel, { WorkerStatus } from '../../api/models/worker';
import { RestAPI } from '../../api/restapi';
import moment from 'moment';

import './export.scss';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';

interface ExportRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class ExportRoute extends Component<ExportRouteProps> {
  public state = {
    worker: (null as any) as WorkerModel,
  };

  private workerTimer: any;

  public async componentDidMount() {
    await this.fetchWorkerState();
    this.workerTimer = setInterval(() => this.fetchWorkerState(), 10_000);
  }

  public componentWillUnmount() {
    clearInterval(this.workerTimer);
  }

  public render() {
    const worker = this.state.worker;

    return (
      <div className="export-container">
        {worker && (
          <div>
            <div>
              Export worker status:&nbsp;&nbsp;
              <strong>{worker.finished ? 'Finished' : 'Processing'}</strong>
            </div>
            <div>Expires {this.workerExpires}.</div>
            <div className="mt-10">{this.workerStatusLine}</div>
            {worker.finished && (
              <div className="mt-10">
                <a href={RestAPI.exportDownloadLink()}>
                  Download bundled archive
                </a>
              </div>
            )}
            {this.workerErrored && (
              <div className="warn-text">
                {this.state.worker.exception.source}:&nbsp;
                {this.state.worker.exception.message}
              </div>
            )}
            <div className="export-cancel-button">
              <button onClick={() => this.cancelWorker()}>
                {worker.finished ? 'Delete archive' : 'Cancel Worker'}
              </button>
            </div>
          </div>
        )}
      </div>
    );
  }

  private get workerStatusLine(): string {
    switch (this.state.worker.status) {
      case WorkerStatus.FINISHED:
        return 'Worker has finished.';
      case WorkerStatus.COLLECTING:
        return 'Worker is currently collecting requesting images...';
      case WorkerStatus.CLEANUP:
        return 'Worker is currently cleaning up...';
      case WorkerStatus.INDEXING:
        return 'Worker is currently indexing image metadata...';
      case WorkerStatus.PACKING:
        return 'Worker is currently creating the bundle...';
      case WorkerStatus.ERRORED:
        return 'Worker errored while processing:';
      default:
        return 'unknown';
    }
  }

  private get workerExpires(): string {
    return moment(this.state.worker.expires).fromNow();
  }

  public get workerErrored(): boolean {
    return this.state.worker.status === WorkerStatus.ERRORED;
  }

  private async fetchWorkerState() {
    try {
      var worker = await RestAPI.statusExport();
      this.setState({ worker });
    } catch {}
  }

  private async cancelWorker() {
    try {
      RestAPI.cancelExport();
      const sbmsg = this.state.worker.finished
        ? 'Archive deleted.'
        : 'Export worker canceled.';
      SnackBarNotifier.show(sbmsg, SnackBarType.SUCCESS, 4000);
      this.props.history.goBack();
    } catch {}
  }
}

export default withRouter(ExportRoute);
