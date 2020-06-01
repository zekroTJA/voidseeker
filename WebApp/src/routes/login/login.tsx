/** @format */

import React, { Component } from 'react';
import { RestAPI } from '../../api/restapi';
import { withRouter, RouteComponentProps } from 'react-router-dom';
import Container from '../../components/container/container';

import './login.scss';
import GlobalState from '../../util/globalstate';

interface LoginRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class LoginRoute extends Component<LoginRouteProps> {
  public state = {
    username: '',
    password: '',
    remember: false,
  };

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
          <div className="login-remember-container">
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

  private onLogin() {
    RestAPI.authLogin(this.state.username, this.state.password)
      .then((user) => {
        this.props.globalState.selfUser = user;
        this.props.history.push('/');
      })
      .catch();
  }
}

export default withRouter(LoginRoute);
