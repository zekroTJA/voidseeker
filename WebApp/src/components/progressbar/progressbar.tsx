/** @format */

import { Component } from 'react';
import './progressbar.scss';

export interface ProgressBarProps {
  progress: number;
  height: string | number;
}

export default class ProgressBar extends Component<ProgressBarProps> {
  static defaultProps = {
    height: '20px',
  };

  render() {
    return (
      <div className="progress-bar" style={{ height: this.props.height }}>
        <div style={{ width: `${this.props.progress * 100}%` }}></div>
      </div>
    );
  }
}
