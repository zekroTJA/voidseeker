/** @format */

import React, { Component } from 'react';
import { UserCreateModel } from '../../api/models/user';
import UserEditor from '../../components/usereditor/usereditor';
import Container from '../../components/container/container';
import { RestAPI } from '../../api/restapi';
import { withRouter, RouteComponentProps } from 'react-router-dom';

import './init.scss';

class InitRoute extends Component<RouteComponentProps> {
  public state = {
    user: {} as UserCreateModel,
  };

  public componentDidMount() {
    RestAPI.instanceStatus().then((val) => {
      if (val.initialized) {
        this.props.history.push('/login');
      }
    });
  }

  public render() {
    return (
      <div className="init-body">
        <h2>Instance Initialization</h2>
        <p>
          This voidseeker instance is not initialized yet. Please enter the
          cedentials and details of the first admin user which is then used to
          create other users and edit general instance preferences.
        </p>
        <Container title="Initial Admin Account">
          <UserEditor
            user={this.state.user}
            onChange={(user) => this.setState({ user })}
          />
          <button
            className="init-init-button"
            disabled={this.initDisabled}
            onClick={this.onInitialize.bind(this)}
            title={
              this.initDisabled
                ? 'Username and password is required!'
                : undefined
            }
          >
            initialize
          </button>
        </Container>
      </div>
    );
  }

  private get initDisabled() {
    return !this.state.user.username || !this.state.user.password;
  }

  private onInitialize() {
    RestAPI.instanceInitialize(this.state.user).then(() =>
      this.props.history.push('/login')
    );
  }
}

export default withRouter(InitRoute);
