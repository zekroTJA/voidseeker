/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import ImageEditor from '../../components/imageeditor/imageeditor';
import ImageModel from '../../api/models/image';
import Container from '../../components/container/container';
import { RestAPI } from '../../api/restapi';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';

import './image-edit.scss';
import GlobalState from '../../util/globalstate';
import { UserModel } from '../../api/models/user';

interface ImageEditRouteProps extends RouteComponentProps {
  globalState: GlobalState;
  imageUid: string;
}

class ImageEditRoute extends Component<ImageEditRouteProps> {
  public state = {
    image: (null as any) as ImageModel,
    owner: (null as any) as UserModel,
    tagSuggestions: (null as any) as string[],
  };

  public async componentDidMount() {
    const gstate = this.props.globalState;

    let image = gstate.images.data.find((i) => i.uid === this.props.imageUid);
    if (!image) {
      try {
        image = await RestAPI.imageInfo(this.props.imageUid);
      } catch {}
    }
    this.setState({ image });

    try {
      const owner = await RestAPI.user(image!.owneruid);
      this.setState({ owner });
    } catch {}
  }

  public render() {
    return (
      <div>
        <h2>Edit Image</h2>
        {this.state.image && (
          <div className="image-edit-container">
            <img
              className="image-edit-preview"
              src={RestAPI.imageUrl(this.state.image.uid)}
              alt={this.state.image.title || this.state.image.filename}
            />
            <Container title="Edit Image">
              <ImageEditor
                image={this.state.image}
                tagSuggestions={this.state.tagSuggestions}
                onChange={(image) => this.setState({ image })}
                onTagsInput={(v) => this.onTagsInput(v)}
              />
              <button
                className="upload-submit-btn"
                onClick={this.onUpload.bind(this)}
              >
                SAVE
              </button>
            </Container>
          </div>
        )}
      </div>
    );
  }

  private async onUpload() {
    console.log('state explicit: ', this.state.image.explicit);
    console.log('state public: ', this.state.image.public);
    try {
      await RestAPI.updateImageInfo(this.state.image.uid, this.state.image);
      SnackBarNotifier.show(
        'Image successfully uploaded.',
        SnackBarType.SUCCESS,
        4000
      );
      this.props.history.push('/images');
    } catch {}
  }

  private async onTagsInput(val: string) {
    const valSplit = val.split(' ');
    const lastVal = valSplit[valSplit.length - 1];
    if (lastVal.length > 0) {
      try {
        const res = await RestAPI.tags(0, 10, lastVal, 10);
        this.setState({
          tagSuggestions: res.data
            .map((t) => t.name)
            .filter((t) => !this.state.image.tagsarray.includes(t)),
        });
      } catch {}
    } else {
      this.setState({
        tagSuggestions: null,
      });
    }
  }
}

export default withRouter(ImageEditRoute);
