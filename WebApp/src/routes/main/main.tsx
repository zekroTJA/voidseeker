/** @format */

import React, { Component } from 'react';
import { withRouter, RouteComponentProps, NavLink } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import { RestAPI } from '../../api/restapi';

import './main.scss';

interface MainRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class MainRoute extends Component<MainRouteProps> {
  public state = {};

  public async componentDidMount() {
    const gstate = this.props.globalState;

    gstate.selfUser();

    try {
      gstate.images = await RestAPI.images(true);
      this.setState({});
    } catch {}
  }

  public render() {
    const imgs = this.props.globalState.images.data.map((i) => (
      <span>
        <NavLink to={`/images/${i.uid}`}>
          <img
            src={RestAPI.imageThumbnailUrl(i.uid)}
            alt={i.title || i.filename}
            title={i.title || i.filename}
          />
        </NavLink>
      </span>
    ));
    return (
      <div>
        <div className="main-image-grid">{imgs}</div>
      </div>
    );
  }
}

export default withRouter(MainRoute);
