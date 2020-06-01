/** @format */

import React, { Component } from 'react';
import { RestAPI } from '../../api/restapi';

export default class LoginRoute extends Component {
  public state = {
    username: '',
    password: '',
    remember: false,
  };

  public render() {
    return (
      <div>
        <h3>Login</h3>
        <div>
          <input
            placeholder="Username"
            type="text"
            value={this.state.username}
            onChange={(e) => this.setState({ username: e.target.value })}
          />
        </div>
        <div>
          <input
            placeholder="Password"
            type="password"
            value={this.state.password}
            onChange={(e) => this.setState({ password: e.target.value })}
          />
        </div>
        <div>
          <input
            id="input-remember"
            type="checkbox"
            checked={this.state.remember}
            onChange={() => this.setState({ remember: !this.state.remember })}
          />
          <label htmlFor="input-remember">Remember for 30 Days</label>
        </div>
        <div>
          <button
            disabled={!this.state.username || !this.state.password}
            onClick={this.onLogin.bind(this)}
          >
            LOGIN
          </button>
        </div>
      </div>
    );
  }

  private async onLogin() {
    try {
      await RestAPI.authLogin(this.state.username, this.state.password);
    } catch (err) {}
  }
}
