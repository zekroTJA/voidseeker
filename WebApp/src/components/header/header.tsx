/** @format */

import React, { Component } from 'react';
import { ReactComponent as Logo } from '../../assets/logo/logo.svg';
import { ReactComponent as LogoInvert } from '../../assets/logo/logo-invert.svg';

import './header.scss';
import LocalStorage from '../../util/localstorage';

interface HeaderPorps {
  version?: string;
  isAdmin?: boolean;
  onLogout?: () => void;
  onUpload?: () => void;
  onMyProfile?: () => void;
  onAdmin?: () => void;
  onHome?: () => void;
  onTags?: () => void;
  onSettings?: () => void;
}

export default class Header extends Component<HeaderPorps> {
  public state = {
    darkTheme: false,
  };

  componentDidMount() {
    this.setState({
      darkTheme: LocalStorage.get<boolean>('dark_theme', false),
    });
  }

  public render() {
    return (
      <div className="header-container">
        <div
          onClick={() => this.props.onHome?.call(this)}
          className="header-logo"
        >
          {(this.state.darkTheme && <Logo height="60%" />) || (
            <LogoInvert height="60%" />
          )}
        </div>
        {this.props.version && (
          <span className="header-version">v.{this.props.version}</span>
        )}
        <div className="header-right-control">
          <button onClick={() => this.props.onUpload?.call(this)}>
            Upload
          </button>
          <button onClick={() => this.props.onTags?.call(this)}>Tags</button>
          <button onClick={() => this.props.onMyProfile?.call(this)}>
            My Profile
          </button>
          <button onClick={() => this.props.onSettings?.call(this)}>
            Settings
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
