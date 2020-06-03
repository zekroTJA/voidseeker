/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import ImageEditor from '../../components/imageeditor/imageeditor';
import ImageModel from '../../api/models/image';
import Container from '../../components/container/container';
import { RestAPI } from '../../api/restapi';

import './upload.scss';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';

interface UploadRouteProps extends RouteComponentProps {}

class UploadRoute extends Component<UploadRouteProps> {
  public state = {
    image: {} as ImageModel,
  };

  public render() {
    return (
      <div>
        <iframe
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
            onChange={(image) => this.setState({ image })}
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
}

export default withRouter(UploadRoute);
