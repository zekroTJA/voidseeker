/** @format */

import React, { Component } from 'react';
import { withRouter, RouteComponentProps } from 'react-router-dom';
import Container from '../../components/container/container';
import GlobalState from '../../util/globalstate';
import { RestAPI } from '../../api/restapi';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';
import PasswordInput from '../../components/passwordinput/passwordinput';

import './password-reset.scss';

interface PasswordResetRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class PasswordResetRoute extends Component<PasswordResetRouteProps> {
  public state = {
    username: '',
    email: '',
    password: '',
    isReset: false,
  };

  private token = '';

  public async componentDidMount() {
    const params = new URLSearchParams(this.props.location.search);
    this.token = params.get('token') ?? '';

    if (this.token) {
      this.setState({ isReset: true });
    }
  }

  public render() {
    return this.state.isReset
      ? this.finalizeResetComponent
      : this.initResetComponent;
  }

  private get initResetComponent() {
    return (
      <div className="reset-container">
        <Container title="Password Reset">
          <input
            className="mb-5"
            placeholder="Username"
            type="text"
            value={this.state.username}
            onChange={(e) => this.setState({ username: e.target.value })}
          />
          <input
            placeholder="E-Mail Address"
            type="text"
            value={this.state.email}
            onChange={(e) => this.setState({ email: e.target.value })}
          />
          <button
            className="w-100 mt-10"
            disabled={!this.state.email && !this.state.username}
            onClick={this.onReset.bind(this)}
          >
            Send Reset Information
          </button>
        </Container>
      </div>
    );
  }

  private get finalizeResetComponent() {
    return (
      <div className="reset-container">
        <Container title="Password Reset Finalize">
          <PasswordInput
            placeholder="New Password"
            value={this.state.password}
            onChange={(v) => this.setState({ password: v })}
          />
          <button
            className="w-100 mt-10"
            disabled={!this.state.password}
            onClick={this.onResetConfirm.bind(this)}
          >
            Reset Password
          </button>
        </Container>
      </div>
    );
  }

  private async onReset() {
    try {
      await RestAPI.mailConfirmPasswordReset(
        this.state.username,
        this.state.email
      );
      SnackBarNotifier.show(
        'Password Reset Information sent to Account E-Mail Address.',
        SnackBarType.SUCCESS
      );
      this.props.history.push('/login');
    } catch {
      SnackBarNotifier.show(
        'Invalid username or mail address.',
        SnackBarType.ERROR
      );
    }
  }

  private async onResetConfirm() {
    try {
      await RestAPI.mailConfirmPasswordResetConfirm(
        this.token,
        this.state.password
      );
      SnackBarNotifier.show(
        'Password successfully reset.',
        SnackBarType.SUCCESS
      );
      this.props.history.push('/login');
    } catch {}
  }
}

export default withRouter(PasswordResetRoute);
