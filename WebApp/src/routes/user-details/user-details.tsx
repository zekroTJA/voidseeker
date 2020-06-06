/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter, NavLink } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import { RestAPI } from '../../api/restapi';
import Container from '../../components/container/container';
import { UserModel } from '../../api/models/user';
import moment from 'moment';

import './user-details.scss';
import Consts from '../../consts';

interface UserDetailsRouteProps extends RouteComponentProps {
  globalState: GlobalState;
  userId: string;
}

class UserDetailsRoute extends Component<UserDetailsRouteProps> {
  public state = {
    user: (null as any) as UserModel,
    isMe: false,
  };

  public async componentDidMount() {
    let user;
    let isMe = false;

    if (this.props.userId === 'me') {
      user = await this.props.globalState.selfUser();
      isMe = true;
    } else {
      try {
        user = await RestAPI.user(this.props.userId);
      } catch {}
    }
    this.setState({ user, isMe });
  }

  public render() {
    const user = this.state.user;

    return (
      <div className="user-details-container">
        {user && (
          <Container title={`User details of ${user.username}`}>
            <table className="user-details-table">
              <tbody>
                <tr>
                  <th>Username</th>
                  <td>{user.username}</td>
                </tr>
                <tr>
                  <th>Displayname</th>
                  <td>{user.displayname}</td>
                </tr>
                <tr>
                  <th>UID</th>
                  <td>{user.uid}</td>
                </tr>
                <tr>
                  <th>Created</th>
                  <td>{moment(user.created).format(Consts.TIME_FORMAT)}</td>
                </tr>
                <tr>
                  <th>Last Login</th>
                  <td>
                    {user.lastlogin ? (
                      moment(user.lastlogin).format(Consts.TIME_FORMAT)
                    ) : (
                      <i>Never logged in.</i>
                    )}
                  </td>
                </tr>
                <tr>
                  <th>Images</th>
                  <td>{user.imagescount}</td>
                </tr>
              </tbody>
            </table>
            {this.state.isMe && (
              <NavLink to="/users/me/edit">
                <button className="mt-10 w-100">Edit my profile</button>
              </NavLink>
            )}
          </Container>
        )}
      </div>
    );
  }
}

export default withRouter(UserDetailsRoute);
