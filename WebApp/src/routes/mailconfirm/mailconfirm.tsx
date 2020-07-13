/** @format */

import React, { Component } from 'react';
import { withRouter, RouteComponentProps } from 'react-router-dom';
import { RestAPI } from '../../api/restapi';

import './mailconfirm.scss';

enum ConfirmStatus {
  PROCESSING,
  CONFIRMED,
  ERRORED,
}

class MailConfirmRoute extends Component<RouteComponentProps> {
  public state = {
    confirmStatus: ConfirmStatus.PROCESSING,
  };

  public async componentDidMount() {
    const params = new URLSearchParams(this.props.location.search);
    const token = params.get('token') ?? '';

    try {
      await RestAPI.mailConformSet(token, true);
      this.setState({ confirmStatus: ConfirmStatus.CONFIRMED });
    } catch {
      this.setState({ confirmStatus: ConfirmStatus.ERRORED });
    }
  }

  public render() {
    return (
      <div className="mailconfirm-container">
        <span className={this.textClassName}>{this.textContent}</span>
      </div>
    );
  }

  private get textClassName(): string {
    switch (this.state.confirmStatus) {
      case ConfirmStatus.CONFIRMED:
        return 'success-text';
      case ConfirmStatus.ERRORED:
        return 'warn-text';
      default:
        return '';
    }
  }

  private get textContent(): JSX.Element {
    switch (this.state.confirmStatus) {
      case ConfirmStatus.PROCESSING:
        return <span>Checking confirmation token...</span>;
      case ConfirmStatus.ERRORED:
        return (
          <span>
            Confirmation failed.
            <br />
            Please retry setting your e-mail address.
          </span>
        );
      case ConfirmStatus.CONFIRMED:
        return <span>Successfully confirmed e-mail address.</span>;
      default:
        return <span>Undefined state.</span>;
    }
  }
}

export default withRouter(MailConfirmRoute);
