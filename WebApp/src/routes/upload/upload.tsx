/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import ImageModel from '../../api/models/image';
import { RestAPI } from '../../api/restapi';

import './upload.scss';

interface UploadRouteProps extends RouteComponentProps {}

class UploadRoute extends Component<UploadRouteProps> {
  public state = {
    image: {} as ImageModel,
    tagSuggestions: (null as any) as string[],
    dragging: false,
    dropText: 'Drop your image here',
  };

  public render() {
    return (
      <div>
        <h2>Image Upload</h2>
        <div
          className={`upload-drop-zone${
            this.state.dragging ? ' upload-drop-zone-over' : ''
          }`}
          onDragOver={this.onDragOver.bind(this)}
          onDragLeave={this.onDragLeave.bind(this)}
          onDrop={this.onDrop.bind(this)}
        >
          <p>{this.state.dropText}</p>
        </div>
      </div>
    );
  }

  private onDragOver(e: React.DragEvent<HTMLDivElement>) {
    e.stopPropagation();
    e.preventDefault();
    e.dataTransfer.dropEffect = 'copy';
    this.setState({ dragging: true });
  }

  private onDragLeave() {
    this.setState({ dragging: false });
  }

  private async onDrop(e: React.DragEvent<HTMLDivElement>) {
    e.stopPropagation();
    e.preventDefault();
    this.setState({ dragging: false, dropText: 'Uploading image...' });

    const files = e.dataTransfer.files;
    if (files.length <= 0) return;

    const file = files[0];
    try {
      const res = await RestAPI.uploadImage(file);
      this.props.history.replace(`/images/${res.uid}/edit`);
    } catch {}
  }
}

export default withRouter(UploadRoute);
