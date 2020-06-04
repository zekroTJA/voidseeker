/** @format */

import React, { Component } from 'react';
import { withRouter, RouteComponentProps, NavLink } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import { RestAPI } from '../../api/restapi';

import './main.scss';
import InputLimiter from '../../util/inputlimier';

interface MainRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class MainRoute extends Component<MainRouteProps> {
  public state = {
    filter: '',
    excludes: [],
    includeExplicit: false,
    includePublic: true,
  };

  private searchLimiter = new InputLimiter(300);

  public async componentDidMount() {
    this.props.globalState.selfUser();

    await this.fetchImages();
  }

  public render() {
    const imgs = this.props.globalState.images.data.map((i) => (
      <span key={i.uid}>
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
        <div className="main-controls">
          <input
            placeholder="search"
            onChange={(v) => this.onSearch(v.target.value)}
          />
          <div>
            <input
              id="main-control-explicit"
              type="checkbox"
              checked={this.state.includeExplicit}
              onChange={() => this.onIncludeExplicitChange()}
            />
            <label htmlFor="main-control-explicit">Display explicit</label>
          </div>
          <div>
            <input
              id="main-control-public"
              type="checkbox"
              checked={this.state.includePublic}
              onChange={() => this.onIncludePublicChange()}
            />
            <label htmlFor="main-control-public">Display public</label>
          </div>
        </div>
        <div className="main-image-grid">{imgs}</div>
      </div>
    );
  }

  private onSearch(val: string) {
    this.searchLimiter.input(val, (v) => {
      const split = val.split(' ');
      const excludes = split
        .filter((s) => s.startsWith('-') && s.length > 1)
        .map((s) => s.substr(1));
      const filter = split
        .filter((s) => !s.startsWith('-') && s.length > 0)
        .join(' ');

      this.setState({ excludes, filter }, () => {
        this.fetchImages();
      });
    });
  }

  private async onIncludeExplicitChange() {
    this.setState({ includeExplicit: !this.state.includeExplicit }, () => {
      this.fetchImages();
    });
  }

  private async onIncludePublicChange() {
    this.setState({ includePublic: !this.state.includePublic }, () => {
      this.fetchImages();
    });
  }

  private async fetchImages() {
    try {
      this.props.globalState.images = await RestAPI.images(
        this.state.includePublic, // include public
        this.state.includeExplicit, // include explicit
        0, // oofset
        100, // limit
        this.state.filter, // filter
        this.state.excludes, // excludes
        'created', // sort by
        false // ascending
      );
      this.setState({});
    } catch {}
  }
}

export default withRouter(MainRoute);
