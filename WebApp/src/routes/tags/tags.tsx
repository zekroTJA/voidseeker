/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter, NavLink } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import { RestAPI } from '../../api/restapi';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';
import { TagModel } from '../../api/models/tag';
import InputLimiter from '../../util/inputlimier';

import './tags.scss';

interface TagsRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class TagsRoute extends Component<TagsRouteProps> {
  public state = {
    filter: '',
    tags: [] as TagModel[],
    isAdmin: false,
  };

  private inputLimiter = new InputLimiter(250);

  public async componentDidMount() {
    const selfUser = await this.props.globalState.selfUser();
    this.setState({ isAdmin: selfUser.isadmin });
    await this.fetchTags();
  }

  public render() {
    const tagElements = this.state.tags.map((t) => (
      <div className="tags-element">
        <NavLink to={`/tags/${t.uid}`}>{t.name}</NavLink>
        {this.state.isAdmin && (
          <button
            className="tags-element-delete"
            onClick={() => this.onTagDelete(t)}
          >
            X
          </button>
        )}
      </div>
    ));

    return (
      <div>
        <div className="tags-controls">
          <input
            placeholder="Search..."
            value={this.state.filter}
            onChange={(v) => this.onFilterChange(v.target.value)}
          />
        </div>
        <div className="tags-container">{tagElements}</div>
      </div>
    );
  }

  private async onTagDelete(tag: TagModel) {
    try {
      await RestAPI.deleteTag(tag.uid);
      const i = this.state.tags.indexOf(tag);
      if (i) {
        this.state.tags.splice(i, 1);
      }
      this.setState({});
      SnackBarNotifier.show(
        `Tag ${tag.name} successfully deleted.`,
        SnackBarType.SUCCESS,
        3000
      );
    } catch {}
  }

  private async onFilterChange(filter: string) {
    this.setState({ filter });
    this.inputLimiter.input(filter, () => {
      this.fetchTags();
    });
  }

  private async fetchTags() {
    const tags = await (await RestAPI.tags(0, 10_000, this.state.filter, 5))
      ?.data;
    this.setState({ tags });
  }
}

export default withRouter(TagsRoute);
