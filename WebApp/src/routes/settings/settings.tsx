/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import Container from '../../components/container/container';
import LocalStorage from '../../util/localstorage';

import './settings.scss';
import UserSettingsModel from '../../api/models/usersettings';
import { RestAPI } from '../../api/restapi';
import TagsInput from '../../components/tagsinput/tagsinput';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';

interface SettingsRouteProps extends RouteComponentProps {
  globalState: GlobalState;
}

class SettingsRoute extends Component<SettingsRouteProps> {
  public state = {
    darktheme: false,
    darkthemeChanged: false,
    userSettings: (null as any) as UserSettingsModel,
  };

  public async componentDidMount() {
    const darktheme = LocalStorage.get<boolean>('dark_theme', false);
    this.setState({ darktheme });

    const userSettings = await RestAPI.userSettings();
    this.setState({ userSettings });
  }

  public render() {
    const settings = this.state.userSettings;

    return (
      <div className="settings-container">
        <Container title="Theme Settings">
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
            <div className="settings-small-info warn-text">
              Page needs to be reloaded for the change to take effect.
            </div>
          )}
        </Container>
        {settings && (
          <Container
            title="User Settings"
            className="settings-usersettings-container"
          >
            <div className="settings-blacklist">
              <label>Blacklisted tags:</label>
              <TagsInput
                tags={settings.tagblacklist}
                disableTagCoupling
                onChange={(_, tagsArray) =>
                  this.onUpdate(() => (settings.tagblacklist = tagsArray))
                }
              />
            </div>
            <button onClick={() => this.onUserSettingsUpdate()}>UPDATE</button>
          </Container>
        )}
      </div>
    );
  }

  private onUpdate(cb: () => void) {
    cb();
    this.setState({});
  }

  private onDarkThemeChange() {
    const darktheme = !this.state.darktheme;
    LocalStorage.set<boolean>('dark_theme', darktheme);
    this.setState({ darktheme, darkthemeChanged: true });
  }

  private async onUserSettingsUpdate() {
    setTimeout(async () => {
      try {
        await RestAPI.setUserSettings(this.state.userSettings);
        SnackBarNotifier.show(
          'User settings successfully updated.',
          SnackBarType.SUCCESS,
          4000
        );
      } catch {}
    }, 100 /* <- Small delay required for tag input to be set */);
  }
}

export default withRouter(SettingsRoute);
