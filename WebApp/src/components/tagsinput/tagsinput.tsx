/** @format */

import React, { Component } from 'react';
import { TagModel } from '../../api/models/tag';
import InputLimiter from '../../util/inputlimier';
import { RestAPI } from '../../api/restapi';

import './tagsinput.scss';

enum KEY_CODE {
  ARROW_DOWN = 40,
  ARROW_UP = 38,
  ENTER = 13,
  ESCAPE = 27,
}

interface TagsInputProps {
  tags: string[];
  tagsCompiled?: string;
  onChange?: (tagsCompiled: string, tags: string[]) => void;
}

export default class TagsInput extends Component<TagsInputProps> {
  public state = {
    tagsCompiled: this.props.tagsCompiled || this.props.tags.join(' '),
    suggestions: [] as TagModel[],
    selected: -1,
  };

  private limiter = new InputLimiter(250);

  public render() {
    const tagSuggestions = (this.state.suggestions || []).map((s, i) => (
      <p
        key={s.uid}
        className={this.state.selected === i ? 'tagsinput-selected' : ''}
        onClick={() => this.onSuggestionClick(s)}
      >
        {s.name}{' '}
        {s.coupledwith?.length > 0 && (
          <span className="tagsinput-coupled-num">+{s.coupledwith.length}</span>
        )}
      </p>
    ));

    return (
      <div className="tagsinput-tags-container">
        <input
          id="tagsinput-tags"
          value={this.state.tagsCompiled}
          onChange={(v) => this.onImageTagsChange(v.target.value)}
          onKeyDown={(e) => this.onKeyPress(e)}
          onBlur={() => this.onImageTagsBlur()}
          autoComplete="off"
        />
        {this.state.suggestions && this.state.suggestions.length > 0 && (
          <div className="tagsinput-tags-suggestions">{tagSuggestions}</div>
        )}
      </div>
    );
  }

  private onImageTagsChange(v: string) {
    this.setState({ tagsCompiled: v.toLowerCase().replace(',', ' ') });
    this.limiter.input(v, async (val) => {
      const valSplit = val.split(' ');
      const filter = valSplit[valSplit.length - 1];

      let suggestions: TagModel[] = [];
      if (filter) {
        try {
          const res = await RestAPI.tags(0, 5, filter, 5);
          suggestions = res.data;
        } catch {}
      }
      this.setState({ suggestions });
    });
  }

  private onSuggestionClick(s: TagModel) {
    const inputElement = document.getElementById('tagsinput-tags');

    let tagsCompiledSplit = this.state.tagsCompiled.split(' ');
    tagsCompiledSplit[tagsCompiledSplit.length - 1] = s.name;

    if (s.coupledwith) {
      tagsCompiledSplit = tagsCompiledSplit.concat(
        s.coupledwith.filter((t) => !tagsCompiledSplit.includes(t))
      );
    }

    const tagsCompiled = tagsCompiledSplit.join(' ');
    this.setState({ tagsCompiled, suggestions: [], selected: -1 }, () =>
      inputElement?.focus()
    );
  }

  private onKeyPress(e: React.KeyboardEvent<HTMLInputElement>) {
    if (this.state.suggestions?.length > 0) {
      switch (e.keyCode) {
        case KEY_CODE.ARROW_DOWN:
          this.changeSelected(1);
          break;
        case KEY_CODE.ARROW_UP:
          this.changeSelected(-1);
          break;
        case KEY_CODE.ENTER:
          this.onSuggestionClick(this.state.suggestions[this.state.selected]);
          break;
        case KEY_CODE.ESCAPE:
          this.setState({ suggestions: [] });
          break;
      }
    }
  }

  private changeSelected(diff: number) {
    const ln = this.state.suggestions.length;
    let selected = this.state.selected;
    selected += diff;

    if (selected < 0) selected = 0;
    if (selected >= ln) selected = ln - 1;

    this.setState({ selected });
  }

  private onImageTagsBlur() {
    const tags = this.state.tagsCompiled
      .toLowerCase()
      .split(' ')
      .filter((t) => !!t);
    const tagsCompiled = tags.join(' ');

    this.setState({ tagsCompiled });
    this.props.onChange?.call(this, tagsCompiled, tags);
  }
}
