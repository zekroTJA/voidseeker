/** @format */

import React, { Component } from 'react';

import './container.scss';

interface ContainerProperties {
  title?: string | JSX.Element;
}

export default class Container extends Component<ContainerProperties> {
  public render() {
    return (
      <div className="container-element">
        {this.props.title && (
          <div className="container-header">{this.title}</div>
        )}
        <div className="container-content">{this.props.children}</div>
      </div>
    );
  }

  private get title(): JSX.Element | null {
    if (!this.props.title) return null;
    if (typeof this.props.title === 'string') {
      return <h3>{this.props.title}</h3>;
    }
    return this.props.title;
  }
}
