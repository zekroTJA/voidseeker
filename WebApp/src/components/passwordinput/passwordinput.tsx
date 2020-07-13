/** @format */

import React, { Component } from 'react';

import './passwordinput.scss';

interface PasswordInputProperties {
  id?: string;
  placeholder?: string;
  value: string;
  onChange: (v: string) => void;
}

export default class PasswordInput extends Component<PasswordInputProperties> {
  public static defaultProps = {
    value: '',
    onChange: () => {},
  };

  public state = {
    hide: true,
  };

  public render() {
    return (
      <div className="password-input-container">
        <input
          type={this.state.hide ? 'password' : 'text'}
          id={this.props.id}
          value={this.props.value}
          placeholder={this.props.placeholder}
          onChange={(v) => this.props.onChange(v.target.value)}
        />
        <button
          onMouseDown={() => this.setState({ hide: false })}
          onMouseUp={() => this.setState({ hide: true })}
        >
          show
        </button>
      </div>
    );
  }
}
