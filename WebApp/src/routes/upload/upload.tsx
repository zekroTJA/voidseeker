/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import ImageEditor from '../../components/imageeditor/imageeditor';
import ImageModel from '../../api/models/image';
import Container from '../../components/container/container';
import { RestAPI } from '../../api/restapi';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';

import './upload.scss';

interface UploadRouteProps extends RouteComponentProps {}

class UploadRoute extends Component<UploadRouteProps> {
  public state = {
    image: {} as ImageModel,
    tagSuggestions: (null as any) as string[],
  };

  public render() {
    return (
      <div>
        <iframe
          title="summyframe"
          width="0"
          height="0"
          frameBorder="0"
          name="upload-dummyframe"
          id="upload-dummyframe"
        ></iframe>
        <h2>Image Upload</h2>
        <Container title="Image Information">
          <ImageEditor
            image={this.state.image}
            tagSuggestions={this.state.tagSuggestions}
            onChange={(image) => this.setState({ image })}
            onTagsInput={(v) => this.onTagsInput(v)}
          />
          <form
            method="post"
            encType="multipart/form-data"
            id="upload-form"
            target="upload-dummyframe"
          >
            <input type="file" name="file" className="upload-file-input" />
          </form>
          <button
            className="upload-submit-btn"
            onClick={this.onUpload.bind(this)}
          >
            UPLOAD
          </button>
        </Container>
      </div>
    );
  }

  private async onUpload() {
    const uploadForm = document.getElementById(
      'upload-form'
    ) as HTMLFormElement;
    try {
      const image = await RestAPI.initCreateImage(this.state.image);
      SnackBarNotifier.show(
        'Image metadata successfully initialized. Image will now be uploaded...',
        SnackBarType.INFO,
        4000
      );
      uploadForm.action = RestAPI.imageUploadUrl(image.uid);
      uploadForm.submit();
      SnackBarNotifier.show(
        'Image successfully uploaded.',
        SnackBarType.SUCCESS,
        4000
      );
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

export default withRouter(UploadRoute);
