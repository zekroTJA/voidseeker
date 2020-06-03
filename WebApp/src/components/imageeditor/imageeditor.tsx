/** @format */

import React, { Component } from 'react';
import ImageModel, { Grade } from '../../api/models/image';

import './imageeditor.scss';
import { withRouter, RouteComponentProps } from 'react-router-dom';
import ObjectUtils from '../../util/objects';

interface ImageEditorProperties extends RouteComponentProps {
  image: ImageModel;
  onChange?: (user: ImageModel) => void;
}

class ImageEditor extends Component<ImageEditorProperties> {
  public render() {
    const image = this.props.image;

    const gradeSelectOptions = [
      <option key="-1" value={undefined}>
        unset
      </option>,
    ].concat(
      ObjectUtils.enumToArray(Grade).map((v) => (
        <option key={v[0]} value={v[1]}>
          {v[0]}
        </option>
      ))
    );

    return (
      <div className="image-editor-container">
        <label htmlFor="image-editor-title">Title:</label>
        <input
          id="image-editor-title"
          value={image.title}
          onChange={(v) => this.onChange(() => (image.title = v.target.value))}
        />
        <label htmlFor="image-editor-tags">Tags:</label>
        <input
          id="image-editor-tags"
          value={this.getImageTags(image)}
          onChange={(v) => this.onImageTagsChange(image, v.target.value)}
          onBlur={() => this.onImageTagsBlur(image)}
        />
        <label htmlFor="image-editor-description">Description:</label>
        <textarea
          id="image-editor-description"
          value={image.description}
          onChange={(v) =>
            this.onChange(() => (image.description = v.target.value))
          }
        />
        <label htmlFor="image-editor-grade">Grade:</label>
        <select
          id="image-editor-grade"
          value={image.grade}
          onChange={(v) => this.onGradeChange(image, v.target.value)}
        >
          {gradeSelectOptions}
        </select>
        <div className="image-editor-checkboxes">
          <input
            id="image-editor-cb-public"
            type="checkbox"
            checked={image.public}
            onChange={() => this.onChange(() => (image.public = !image.public))}
          />
          <label htmlFor="image-editor-cb-public">
            Public for logged in users
          </label>
          <input
            id="image-editor-cb-explicit"
            type="checkbox"
            checked={image.explicit}
            onChange={() =>
              this.onChange(() => (image.explicit = !image.explicit))
            }
          />
          <label htmlFor="image-editor-cb-explicit">
            Contains explicit content
          </label>
        </div>
      </div>
    );
  }

  private onChange(cb: () => void) {
    cb();
    if (this.props.onChange) {
      this.props.onChange(this.props.image);
    }
  }

  private onImageTagsChange(image: ImageModel, input: string) {
    this.onChange(() => {
      image.tagsarray = input.toLowerCase().split(' ');
    });
  }

  private onImageTagsBlur(image: ImageModel) {
    this.onChange(() => {
      image.tagsarray = image.tagsarray.filter((t) => t.length > 0);
      image.tagscombined = image.tagsarray.join(' ');
    });
  }

  private getImageTags(image: ImageModel): string {
    return image.tagsarray ? image.tagsarray.join(' ') : '';
  }

  private onGradeChange(image: ImageModel, value: string) {
    this.onChange(() => {
      let val;
      if (value === 'unset') {
        val = undefined;
      } else {
        val = parseInt(value, 10);
      }
      image.grade = val;
    });
  }
}

export default withRouter(ImageEditor);
