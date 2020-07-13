/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter, NavLink } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import Container from '../../components/container/container';
import PageModel from '../../api/models/page';
import { UserModel } from '../../api/models/user';
import { RestAPI } from '../../api/restapi';
import moment from 'moment';
import Modal from '../../components/modal/modal';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';
import Consts from '../../consts';

import './admin.scss';

interface AdminRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class AdminRoute extends Component<AdminRouteProps> {
  public state = {
    users: { size: 0, offset: 0, data: [] } as PageModel<UserModel>,
    deleteUser: (null as any) as UserModel,
  };

  public async componentDidMount() {
    if (!(await this.props.globalState.selfUser())?.isadmin) {
      this.props.history.push('/images');
    }

    await this.fetchUsers();
  }

  public render() {
    const users = this.state.users.data.map((u) => (
      <tr key={u.uid}>
        <td>{u.username}</td>
        <td>{u.displayname || <i>Not set.</i>}</td>
        <td>
          {u.lastlogin ? (
            moment(u.lastlogin).format(Consts.TIME_FORMAT)
          ) : (
            <i>Never logged in</i>
          )}
        </td>
        <td>{moment(u.created).format(Consts.TIME_FORMAT)}</td>
        <td>{u.isadmin ? <b>Yes</b> : 'No'}</td>
        <td>
          <NavLink to={`/users/${u.uid}/edit`}>
            <button className="admin-ctrl-btn">Edit</button>
          </NavLink>
          <button
            className="admin-ctrl-btn"
            onClick={() => this.onDeleteUser(u)}
          >
            Delete
          </button>
        </td>
      </tr>
    ));

    return (
      <div>
        {this.state.deleteUser && this.deleteModal}
        <h2>Admin Panel</h2>
        <Container
          title={
            <div className="admin-users-title">
              <h3>Users</h3>
              <NavLink to="/users/new/edit">
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

  private async fetchUsers() {
    try {
      const users = await RestAPI.users();
      this.setState({ users });
    } catch {}
  }

  private get deleteModal(): JSX.Element {
    return (
      <Modal onClose={() => this.setState({ deleteUser: null })}>
        <span>
          Do you really want to delete the user&nbsp;
          <strong>
            {this.state.deleteUser.username} (
            {this.state.deleteUser.displayname})
          </strong>
          ?<br />
          Deleting a user will not delete the content uploaded by this user!
          <br />
          <div className="highlight-red mt-5">
            <strong>This action is permanent and can not be undone!</strong>
          </div>
          <div className="modal-control-buttons">
            <button onClick={() => this.setState({ deleteUser: null })}>
              Cancel
            </button>
            <button onClick={this.onDeleteUserConfirm.bind(this)}>
              <strong>Delete</strong>
            </button>
          </div>
        </span>
      </Modal>
    );
  }

  private onDeleteUser(deleteUser: UserModel) {
    this.setState({ deleteUser });
  }

  private async onDeleteUserConfirm() {
    try {
      await RestAPI.deleteUser(this.state.deleteUser.uid);
      this.setState({ deleteUser: null });
      SnackBarNotifier.show(
        'User successfully deleted.',
        SnackBarType.SUCCESS,
        4000
      );
      await this.fetchUsers();
    } catch {}
  }
}

export default withRouter(AdminRoute);
