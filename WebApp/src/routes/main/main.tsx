/** @format */

import React, { Component } from 'react';
import { withRouter, RouteComponentProps, NavLink } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import { RestAPI } from '../../api/restapi';
import InputLimiter from '../../util/inputlimier';
import LocalStorage from '../../util/localstorage';

import './main.scss';
import ObjectUtils from '../../util/objects';

const SORT_OPTIONS = {
  // 'File Name': 'filename',
  // 'Title': 'title',
  // 'Description': 'description',
  Size: 'size',
  Grade: 'grade',
  Created: 'created',
};

interface MainRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class MainRoute extends Component<MainRouteProps> {
  public state = {
    filter: '',
    excludes: [],
    includeExplicit: false,
    includePublic: false,
    offset: 0,
    size: 100,
    sortBy: 'created',
    ascending: false,
  };

  private searchLimiter = new InputLimiter(300);

  public componentDidMount() {
    this.props.globalState.selfUser();

    this.setState(
      {
        includeExplicit: LocalStorage.get<boolean>('include_explicit', false),
        includePublic: LocalStorage.get<boolean>('include_public', false),
        sortBy: LocalStorage.get<string>('sort_by', 'created'),
        ascending: LocalStorage.get<boolean>('sort_ascending', false),
      },
      () => {
        this.fetchImages();
      }
    );

    this.fromQueryParams();
  }

  public render() {
    const gs = this.props.globalState;

    const imgs = gs.images.data.map((i) => (
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

    const sortOptions = ObjectUtils.objectMap(SORT_OPTIONS, (k, v) => (
      <option key={k} value={v}>
        {k}
      </option>
    ));

    return (
      <div>
        <div className="main-controls">
          <input
            id="main-control-searchbar"
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
          <div>
            <label htmlFor="main-sort-by">Sort by: </label>
            <select
              id="main-sort-by"
              value={this.state.sortBy}
              onChange={(v) => this.onSortByChange(v.target.value)}
            >
              {sortOptions}
            </select>
            <button className="ml-10" onClick={() => this.onAscendingChange()}>
              {this.state.ascending ? '▲' : '▼'}
            </button>
          </div>
          <div className="main-page-dialer">
            <button
              disabled={this.state.offset === 0}
              onClick={() => this.dialPage(-1)}
            >
              ◄
            </button>
            <span>
              {this.state.offset + 1} - {gs.images.size + this.state.offset}
            </span>
            <button
              disabled={this.state.size > gs.images.size}
              onClick={() => this.dialPage(1)}
            >
              ►
            </button>
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
      LocalStorage.set<boolean>('include_explicit', this.state.includeExplicit);
      this.fetchImages();
    });
  }

  private async onIncludePublicChange() {
    this.setState({ includePublic: !this.state.includePublic }, () => {
      LocalStorage.set<boolean>('include_public', this.state.includePublic);
      this.fetchImages();
    });
  }

  private async onSortByChange(sortBy: string) {
    this.setState({ sortBy }, () => {
      LocalStorage.set<string>('sort_by', this.state.sortBy);
      this.fetchImages();
    });
  }

  private async onAscendingChange() {
    this.setState({ ascending: !this.state.ascending }, () => {
      LocalStorage.set<boolean>('sort_ascending', this.state.ascending);
      this.fetchImages();
    });
  }

  private async fetchImages() {
    try {
      this.props.globalState.images = await RestAPI.images(
        this.state.includePublic, // include public
        this.state.includeExplicit, // include explicit
        this.state.offset, // oofset
        this.state.size, // limit
        this.state.filter, // filter
        this.state.excludes, // excludes
        this.state.sortBy, // sort by
        this.state.ascending // ascending
      );
      this.setState({});
      this.setQueryParams();
    } catch {}
  }

  private dialPage(n: number) {
    let offset = this.state.offset + n * this.state.size;
    if (offset < 0) {
      offset = 0;
    }

    this.setState({ offset }, () => {
      this.fetchImages();
    });
  }

  private setQueryParams() {
    const params = new URLSearchParams();

    params.append('explicit', JSON.stringify(this.state.includeExplicit));
    params.append('public', JSON.stringify(this.state.includePublic));

    if (this.state.filter) {
      params.append('filter', this.state.filter);
    }

    this.state.excludes.forEach((ex) => params.append('exclude', ex));

    params.append('sortBy', this.state.sortBy);
    params.append('ascending', JSON.stringify(this.state.ascending));

    this.props.history.replace({
      search: params.toString(),
    });
  }

  private fromQueryParams(cb: () => void = () => {}) {
    const params = new URLSearchParams(this.props.location.search);
    const searchInput = (document.getElementById(
      'main-control-searchbar'
    ) as HTMLInputElement) || { value: null };

    const includeExplicit = JSON.parse(params.get('explicit') || 'null');
    const includePublic = JSON.parse(params.get('public') || 'null');
    const ascending = JSON.parse(params.get('ascending') || 'null');
    const filter = params.get('filter');
    const sortBy = params.get('sortBy');
    const excludes = params.getAll('exclude');

    const state: { [key: string]: any } = {};
    if (includeExplicit !== null) state.includeExplicit = includeExplicit;
    if (includePublic !== null) state.includePublic = includePublic;
    if (sortBy !== null) state.sortBy = sortBy;
    if (ascending !== null) state.ascending = ascending;

    if (filter !== null) {
      state.filter = filter;
      searchInput.value = filter;
    }

    if (excludes.length > 0) {
      state.excludes = excludes;
      searchInput.value += ` ${excludes.map((e) => `-${e}`).join(' ')}`;
    }

    this.setState(state, cb);
  }
}

export default withRouter(MainRoute);
