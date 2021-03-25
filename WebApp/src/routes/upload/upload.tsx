/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import ImageModel from '../../api/models/image';
import { RestAPI } from '../../api/restapi';
import ImageEditor from '../../components/imageeditor/imageeditor';
import ProgressBar from '../../components/progressbar/progressbar';

import './upload.scss';

interface UploadRouteProps extends RouteComponentProps {}

class UploadRoute extends Component<UploadRouteProps> {
  public state = {
    image: {} as ImageModel,
    tagSuggestions: (null as any) as string[],
    dragging: false,
    status: 0,
    nToProcess: 0,
    nProcessed: 0,
    nErrors: 0,
    processing: false,
    stack: [] as ImageModel[],
  };

  public render() {
    return (
      <div>
        <h2>Image Upload</h2>
        <input
          type="file"
          id="upload-fileinput"
          style={{ display: 'none' }}
          onChange={this.onFileInputChange.bind(this)}
        />
        {this.state.status === 0 && (
          <div
            className={`upload-body upload-drop-zone${
              this.state.dragging ? ' upload-drop-zone-over' : ''
            }`}
            onDragOver={this.onDragOver.bind(this)}
            onDragLeave={this.onDragLeave.bind(this)}
            onDrop={this.onDrop.bind(this)}
            onClick={this.onClick.bind(this)}
          >
            <p>Drop your image here or click to select a file.</p>
          </div>
        )}
        {this.state.status === 1 && (
          <div className="upload-body upload-uploading">
            <div>
              <p>
                Uploaded {this.state.nProcessed} of {this.state.nToProcess} ...
              </p>
              <ProgressBar
                progress={this.state.nProcessed / this.state.nToProcess}
              />
              {this.imageStack}
            </div>
          </div>
        )}
        {this.state.status === 2 && (
          <div className="upload-afteredit">
            {this.imageStack}
            <ImageEditor
              image={this.state.image}
              onChange={(image) => this.setState({ image })}
              onTagsInput={(v) => this.onTagsInput(v)}
            />
            <button onClick={() => this.onSave()}>
              Save for all uploaded images
            </button>
            {this.state.processing && (
              <div>
                <p>
                  Updated{this.state.nProcessed} of {this.state.nToProcess} ...
                </p>
                <ProgressBar
                  progress={this.state.nProcessed / this.state.nToProcess}
                />
              </div>
            )}
          </div>
        )}
      </div>
    );
  }

  private get imageStack(): JSX.Element {
    const stack = this.state.stack.map((i) => (
      <span>
        <img
          src={RestAPI.imageThumbnailUrl(i.uid)}
          alt={i.title || i.filename}
          title={i.title || i.filename}
        />
      </span>
    ));
    return <div className="upload-image-grid">{stack}</div>;
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

    const files = e.dataTransfer.files;
    if (files.length <= 0) return;

    var lastRes;
    this.setState({
      status: 1,
      nToProcess: files.length,
      nProcessed: 0,
      nErrors: 0,
    });
    for (let i = 0; i < files.length; ++i) {
      const file = files[i];
      try {
        lastRes = await this.uploadFile(file);
        this.setState({
          nProcessed: i + 1,
          stack: this.state.stack.concat(lastRes),
        });
      } catch (err) {
        this.setState({
          nProcessed: i + 1,
          nErrors: this.state.nErrors + 1,
        });
      }
    }

    if (files.length === 1) {
      this.props.history.replace(`/images/${lastRes?.uid}/edit`);
      return;
    }

    this.setState({ status: 2 });
  }

  private onClick() {
    const fileInput = document.getElementById('upload-fileinput');
    if (!fileInput) return;
    fileInput.click();
  }

  private async onFileInputChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files ? e.target.files[0] : null;
    if (!file) return;
    await this.uploadFile(file);
  }

  private uploadFile(file: File): Promise<ImageModel> {
    return RestAPI.uploadImage(file);
  }

  private async onTagsInput(val: string) {
    const valSplit = val.split(' ');
    const lastVal = valSplit[valSplit.length - 1];
    if (lastVal.length > 0) {
      try {
        const res = await RestAPI.tags(0, 10, lastVal, 10);
        this.setState({
          tagSuggestions: res.data.filter(
            (t) =>
              !this.state.image.tagsarray
                .slice(0, valSplit.length - 1)
                .includes(t.name)
          ),
        });
      } catch {}
    } else {
      this.setState({
        tagSuggestions: null,
      });
    }
  }

  private async onSave() {
    this.setState({
      processing: true,
      nProcessed: 0,
      nErrors: 0,
    });

    for await (const image of this.state.stack) {
      image.tagsarray = this.state.image.tagsarray;
      image.tagscombined = this.state.image.tagscombined;
      image.title = this.state.image.title;
      image.grade = this.state.image.grade;
      image.explicit = this.state.image.explicit;
      image.public = this.state.image.public;

      try {
        await RestAPI.updateImageInfo(image.uid, image);
        this.setState({
          nProcessed: this.state.nProcessed + 1,
        });
      } catch {
        this.setState({
          nErrors: this.state.nErrors + 1,
        });
      }
    }

    this.setState({
      processing: false,
    });
  }
}

export default withRouter(UploadRoute);
