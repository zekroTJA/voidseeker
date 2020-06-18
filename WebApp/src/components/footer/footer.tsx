/** @format */

import React, { Component } from 'react';

import './footer.scss';

export enum FooterType {
  ERROR = 'footer-error',
}

interface FooterProps {
  type: FooterType;
}

export default class Footer extends Component<FooterProps> {
  public static defaultProps = {
    type: FooterType.ERROR,
  };

  public render() {
    return (
      <div className={`footer-container ${this.props.type}`}>
        {this.props.children}
      </div>
    );
  }
}
