/** @format */

import React, { Component } from 'react';

import './pagedialer.scss';

interface PageDialerProps {
  offset: number;
  maxSize: number;
  currSize: number;
  onChange?: (offset: number) => void;
}

export default class PageDialer extends Component<PageDialerProps> {
  public render() {
    return (
      <div className="pagedialer-container">
        <button
          disabled={this.props.offset === 0}
          onClick={() => this.dialPage(-1)}
        >
          ◄
        </button>
        <span>
          {this.props.offset + 1} - {this.props.currSize + this.props.offset}
        </span>
        <button
          disabled={this.props.maxSize > this.props.currSize}
          onClick={() => this.dialPage(1)}
        >
          ►
        </button>
      </div>
    );
  }

  private dialPage(n: number) {
    let offset = this.props.offset + n * this.props.maxSize;
    if (offset < 0) {
      offset = 0;
    }

    this.props.onChange?.call(this, offset);
  }
}
