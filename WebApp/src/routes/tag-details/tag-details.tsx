/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter, NavLink } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import { RestAPI } from '../../api/restapi';
import Container from '../../components/container/container';
import moment from 'moment';
import Consts from '../../consts';
import { TagModel } from '../../api/models/tag';

import './tag-details.scss';
import { UserModel } from '../../api/models/user';

interface TagDetailsRouteProps extends RouteComponentProps {
  globalState: GlobalState;
  tagId: string;
}

class TagDetailsRoute extends Component<TagDetailsRouteProps> {
  public state = {
    tag: (null as any) as TagModel,
    owner: (null as any) as UserModel,
    canEdit: false,
  };

  public async componentDidMount() {
    try {
      const tag = await RestAPI.tag(this.props.tagId);
      this.setState({ tag });

      const me = await this.props.globalState.selfUser();
      const canEdit = me.isadmin || me.uid === tag.creatoruid;
      this.setState({ canEdit });

      const owner = await RestAPI.user(tag.creatoruid);
      this.setState({ owner });
    } catch {}
  }

  public render() {
    const tag = this.state.tag;
    const owner = this.state.owner;

    return (
      <div className="user-details-container">
        {tag && (
          <Container title={`User details of ${tag.name}`}>
            <table className="user-details-table">
              <tbody>
                <tr>
                  <th>Name</th>
                  <td>{tag.name}</td>
                </tr>
                <tr>
                  <th>UID</th>
                  <td>{tag.uid}</td>
                </tr>
                <tr>
                  <th>Created</th>
                  <td>{moment(tag.created).format(Consts.TIME_FORMAT)}</td>
                </tr>
                <tr>
                  <th>Creator</th>
                  <td>
                    {owner ? (
                      <NavLink to={`/users/${owner.uid}`}>
                        {owner.username}
                      </NavLink>
                    ) : (
                      <i>unset</i>
                    )}
                  </td>
                </tr>
                <tr>
                  <th>Coupled With</th>
                  <td>{tag.coupledwith?.join(', ') || <i>no couples</i>}</td>
                </tr>
              </tbody>
            </table>
            {this.state.canEdit && (
              <NavLink to={`/tags/${tag.uid}/edit`}>
                <button className="mt-10 w-100">Edit tag</button>
              </NavLink>
            )}
          </Container>
        )}
      </div>
    );
  }
}

export default withRouter(TagDetailsRoute);
