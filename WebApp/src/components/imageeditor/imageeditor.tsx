/** @format */

import React, { Component } from 'react';
import ImageModel, { Grade } from '../../api/models/image';
import { withRouter, RouteComponentProps } from 'react-router-dom';
import ObjectUtils from '../../util/objects';
import InputLimiter from '../../util/inputlimier';
import { TagModel } from '../../api/models/tag';
import TagsInput from '../tagsinput/tagsinput';

import './imageeditor.scss';

interface ImageEditorProperties extends RouteComponentProps {
  image: ImageModel;
  tagSuggestions?: TagModel[];
  onChange?: (image: ImageModel) => void;
  onTagsInput?: (v: string) => void;
}

class ImageEditor extends Component<ImageEditorProperties> {
  // TODO: Limiter value might not be the optimal value
  private limiter = new InputLimiter(250);

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
        <TagsInput
          tags={image.tagsarray}
          tagsCompiled={image.tagscombined}
          onChange={(tagsCombined, tagsArray) =>
            this.onTagsChange(image, tagsCombined, tagsArray)
          }
        />
        <label htmlFor="image-editor-description">Description:</label>
        <textarea
          id="image-editor-description"
          value={image.description || ''}
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

  private onTagsChange(
    image: ImageModel,
    tagsCombined: string,
    tagsArray: string[]
  ) {
    this.onChange(() => {
      image.tagsarray = tagsArray;
      image.tagscombined = tagsCombined;
    });
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
