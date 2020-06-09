/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import Container from '../../components/container/container';
import LocalStorage from '../../util/localstorage';

import './settings.scss';

interface SettingsRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class SettingsRoute extends Component<SettingsRouteProps> {
  public state = {
    darktheme: false,
    darkthemeChanged: false,
  };

  public async componentDidMount() {
    const darktheme = LocalStorage.get<boolean>('dark_theme', false);
    this.setState({ darktheme });
  }

  public render() {
    return (
      <div className="settings-container">
        <Container title="User settings">
          <div className="cb-container">
            <input
              type="checkbox"
              id="settings-cb-darktheme"
              checked={this.state.darktheme}
              onChange={() => this.onDarkThemeChange()}
            />
            <label htmlFor="settings-cb-darktheme">Enable dark theme</label>
          </div>
          {this.state.darkthemeChanged && (
            <div className="settings-small-info">
              Page needs to be reloaded for the change to take effect.
            </div>
          )}
        </Container>
      </div>
    );
  }

  private onDarkThemeChange() {
    const darktheme = !this.state.darktheme;
    LocalStorage.set<boolean>('dark_theme', darktheme);
    this.setState({ darktheme, darkthemeChanged: true });
  }
}

export default withRouter(SettingsRoute);
