/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import { UserCreateModel } from '../../api/models/user';
import { RestAPI } from '../../api/restapi';

import './user-edit.scss';
import Container from '../../components/container/container';
import UserEditor from '../../components/usereditor/usereditor';

interface UserEditRouteProps extends RouteComponentProps {
  globalState: GlobalState;
  userId: string;
}

class UserEditRoute extends Component<UserEditRouteProps> {
  public state = {
    user: (null as any) as UserCreateModel,
    isNew: false,
    isAdmin: false,
  };

  public async componentDidMount() {
    const selfUser = await this.props.globalState.selfUser();

    if (!selfUser.isadmin && this.props.userId !== selfUser.uid) {
      this.props.history.goBack();
    }

    this.setState({ isAdmin: selfUser.isadmin });

    if (this.props.userId === 'new') {
      this.setState({
        isNew: true,
        user: {} as UserCreateModel,
      });
    } else {
      try {
        const user = await RestAPI.user(this.props.userId);
        this.setState({ user });
      } catch {
        this.props.history.goBack();
      }
    }
  }

  public render() {
    const user = this.state.user;

    return (
      <div>
        <h2>
          {this.state.isNew ? 'Create new user' : `Edit user ${user?.username}`}
        </h2>
        <Container title="User Information">
          {user && (
            <UserEditor
              user={user}
              onChange={(u) => this.setState({ user: u })}
            />
          )}
          {this.state.isAdmin && (
            <div className="cb-container user-edit-cb-admin">
              <input
                id="user-edit-admin-cb"
                type="checkbox"
                checked={user?.isadmin}
                onChange={() => {
                  user.isadmin = !user.isadmin;
                  this.setState({});
                }}
              />
              <label htmlFor="user-edit-admin-cb">Has admin previleges</label>
            </div>
          )}
          {user?.password && !this.state.isNew && (
            <div className="user-edit-password-confirm">
              <label htmlFor="input-password-confirm">
                Confirm current password:
              </label>
              <input
                id="input-password-confirm"
                type="password"
                value={user.oldpassword}
                onChange={(v) => {
                  user.oldpassword = v.target.value;
                  this.setState({});
                }}
              />
            </div>
          )}
          <button
            className="user-edit-update"
            disabled={this.actionDisabled}
            onClick={this.onSubmit.bind(this)}
          >
            {this.state.isNew ? 'Create' : 'Update'}
          </button>
        </Container>
      </div>
    );
  }

  private get actionDisabled(): boolean {
    const user = this.state.user;

    if (this.state.isNew) {
      return !user || !user.username || !user.password;
    }

    return !user || !user.username || (!!user.password && !user.oldpassword);
  }

  private async onSubmit() {
    try {
      if (this.state.isNew) {
        await RestAPI.createUser(this.state.user);
        this.props.history.push('/admin');
      } else {
        await RestAPI.updateUser(this.state.user.uid, this.state.user);
      }
    } catch {}
  }
}

export default withRouter(UserEditRoute);
