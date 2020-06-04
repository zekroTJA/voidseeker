/** @format */

import React, { Component } from 'react';
import { RestAPI } from '../../api/restapi';
import { withRouter, RouteComponentProps } from 'react-router-dom';
import Container from '../../components/container/container';

import './login.scss';
import GlobalState from '../../util/globalstate';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';

interface LoginRouteProps extends RouteComponentProps {
  globalState: GlobalState;
  onLoginSuccess?: () => void;
}

class LoginRoute extends Component<LoginRouteProps> {
  public state = {
    username: '',
    password: '',
    remember: false,
  };

  public async componentDidMount() {
    try {
      const user = await this.props.globalState.selfUser();
      if (user) {
        this.props.history.push('/images');
      }
    } catch {}
  }

  public render() {
    return (
      <div className="login-container">
        <Container title="Login">
          <input
            placeholder="Username"
            type="text"
            value={this.state.username}
            onChange={(e) => this.setState({ username: e.target.value })}
          />
          <input
            placeholder="Password"
            type="password"
            value={this.state.password}
            onChange={(e) => this.setState({ password: e.target.value })}
          />
          <div className="cb-container mb-10">
            <input
              id="input-remember"
              type="checkbox"
              checked={this.state.remember}
              onChange={() => this.setState({ remember: !this.state.remember })}
            />
            <label htmlFor="input-remember">Remember for 30 Days</label>
          </div>
          <button
            disabled={!this.state.username || !this.state.password}
            onClick={this.onLogin.bind(this)}
          >
            LOGIN
          </button>
        </Container>
      </div>
    );
  }

  private async onLogin() {
    try {
      const user = await RestAPI.authLogin(
        this.state.username,
        this.state.password
      );
      this.props.globalState.setSelfUser(user);
      this.props.onLoginSuccess?.call(this);
      this.props.history.push('/images');
    } catch (e) {
      SnackBarNotifier.show(
        'Invalid login credentials.',
        SnackBarType.ERROR,
        4000
      );
    }
  }
}

export default withRouter(LoginRoute);
