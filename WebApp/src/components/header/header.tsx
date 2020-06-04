/** @format */

import React, { Component } from 'react';

import './header.scss';

interface HeaderPorps {
  version?: string;
  isAdmin?: boolean;
  onLogout?: () => void;
  onUpload?: () => void;
  onUpdateProfile?: () => void;
  onAdmin?: () => void;
  onHome?: () => void;
}

export default class Header extends Component<HeaderPorps> {
  public render() {
    return (
      <div className="header-container">
        <div
          onClick={() => this.props.onHome?.call(this)}
          className="header-logo"
        >
          voidseeker
        </div>
        {this.props.version && (
          <span className="header-version">v.{this.props.version}</span>
        )}
        <div className="header-right-control">
          <button onClick={() => this.props.onUpload?.call(this)}>
            Upload image
          </button>
          <button onClick={() => this.props.onUpdateProfile?.call(this)}>
            Update Profile
          </button>
          {this.props.isAdmin && (
            <button onClick={() => this.props.onAdmin?.call(this)}>
              Admin Interface
            </button>
          )}
          <div className="header-spacer"></div>
          <button onClick={() => this.props.onLogout?.call(this)}>
            Logout
          </button>
        </div>
      </div>
    );
  }
}
