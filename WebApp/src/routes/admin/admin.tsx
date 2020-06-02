/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter, NavLink } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import Container from '../../components/container/container';
import PageModel from '../../api/models/page';
import { UserModel } from '../../api/models/user';
import { RestAPI } from '../../api/restapi';
import moment from 'moment';

import './admin.scss';

interface AdminRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class AdminRoute extends Component<AdminRouteProps> {
  public state = {
    users: { size: 0, offset: 0, data: [] } as PageModel<UserModel>,
  };

  public async componentDidMount() {
    if (!(await this.props.globalState.selfUser())?.isadmin) {
      this.props.history.push('/images');
    }

    try {
      const users = await RestAPI.users();
      this.setState({ users });
    } catch {}
  }

  public render() {
    const users = this.state.users.data.map((u) => (
      <tr>
        <td>{u.username}</td>
        <td>{u.displayname || <i>Not set.</i>}</td>
        <td>
          {u.lastlogin ? (
            moment(u.lastlogin).format('YYYY-MM-DD hh:mm')
          ) : (
            <i>Never logged in</i>
          )}
        </td>
        <td>{moment(u.created).format('YYYY-MM-DD hh:mm')}</td>
        <td>{u.isadmin ? <b>Yes</b> : 'No'}</td>
        <td>
          <NavLink to={`/users/${u.uid}/edit`}>
            <button className="admin-ctrl-btn">Edit</button>
          </NavLink>
          <button className="admin-ctrl-btn">Delete</button>
        </td>
      </tr>
    ));

    return (
      <div>
        <h2>Admin Panel</h2>
        <Container
          title={
            <div className="admin-users-title">
              <h3>Users</h3>
              <NavLink to="/users/new">
                <button>Add User</button>
              </NavLink>
            </div>
          }
        >
          <table className="w-100">
            <tbody>
              <tr>
                <th>Username</th>
                <th>Displayname</th>
                <th>Last Login</th>
                <th>Created</th>
                <th>Admin</th>
                <th>Control</th>
              </tr>
              {users}
            </tbody>
          </table>
        </Container>
      </div>
    );
  }
}

export default withRouter(AdminRoute);
