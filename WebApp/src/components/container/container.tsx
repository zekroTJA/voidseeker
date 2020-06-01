/** @format */

import React, { Component } from 'react';

import './container.scss';

interface ContainerProperties {
  title?: string;
}

export default class Container extends Component<ContainerProperties> {
  public render() {
    return (
      <div className="container-element">
        {this.props.title && (
          <div className="container-header">
            <h3>{this.props.title}</h3>
          </div>
        )}
        <div className="container-content">{this.props.children}</div>
      </div>
    );
  }
}
