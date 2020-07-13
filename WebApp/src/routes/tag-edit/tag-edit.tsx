/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import { RestAPI } from '../../api/restapi';
import Container from '../../components/container/container';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';
import { TagModel } from '../../api/models/tag';
import TagsInput from '../../components/tagsinput/tagsinput';

import './tag-edit.scss';

interface TagEditRouteProps extends RouteComponentProps {
  globalState: GlobalState;
  tagId: string;
}

class TagEditRoute extends Component<TagEditRouteProps> {
  public state = {
    tag: (null as any) as TagModel,
  };

  public async componentDidMount() {
    try {
      const tag = await RestAPI.tag(this.props.tagId);
      this.setState({ tag, coupledWith: tag.coupledwith?.join(' ') || '' });
    } catch {}
  }

  public render() {
    const tag = this.state.tag;

    return (
      <div className="tag-edit-container">
        <h2>Edit tag {tag?.name}</h2>
        {tag && (
          <Container title="Tag Information">
            <label htmlFor="tag-edit-name">Name:</label>
            <input
              id="tag-edit-name"
              type="text"
              value={tag.name}
              onChange={(v) =>
                this.onChange(() => (tag.name = v.target.value.toLowerCase()))
              }
            />
            <label htmlFor="tag-edit-coupled">Coupled with:</label>
            <TagsInput
              tags={tag.coupledwith}
              disableTagCoupling
              onChange={(_, tagsArray) =>
                this.onChange(() => (tag.coupledwith = tagsArray))
              }
            />
            <button
              className="tag-edit-update"
              // disabled={this.actionDisabled}
              onClick={this.onSubmit.bind(this)}
            >
              Update
            </button>
          </Container>
        )}
      </div>
    );
  }

  private onChange(cb: () => void) {
    cb();
    this.setState({});
  }

  private async onSubmit() {
    try {
      await RestAPI.updateTag(this.state.tag);
      SnackBarNotifier.show(
        `Tag successfully updated.`,
        SnackBarType.SUCCESS,
        4000
      );
    } catch {}
  }
}

export default withRouter(TagEditRoute);
