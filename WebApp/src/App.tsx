/** @format */

import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Redirect } from 'react-router-dom';
import LoginRoute from './routes/login/login';
import { RestAPI } from './api/restapi';
import GlobalState from './util/globalstate';
import InitRoute from './routes/init/init';
import MainRoute from './routes/main/main';
import ImageDetailsRoute from './routes/image-details/image-details';
import ImageEditRoute from './routes/image-edit/image-edit';
import UploadRoute from './routes/upload/upload';
import AdminRoute from './routes/admin/admin';
import ExportRoute from './routes/export/export';
import UserEditRoute from './routes/user-edit/user-edit';
import UserDetailsRoute from './routes/user-details/user-details';
import TagsRoute from './routes/tags/tags';
import TagDetailsRoute from './routes/tag-details/tag-details';
import TagEditRoute from './routes/tag-edit/tag-edit';
import SettingsRoute from './routes/settings/settings';
import MailConfirmRoute from './routes/mailconfirm/mailconfirm';
import SnackBar from './components/snackbar/snackbar';
import SnackBarNotifier, { SnackBarType } from './util/snackbar-notifier';
import Header from './components/header/header';

import './App.scss';
import ResponseErrorUtil from './util/responseerror';

export default class App extends Component {
  public state = {
    redirect: (null as any) as string,
    loggedIn: false,
    isAdmin: false,
  };

  private globalState = new GlobalState();

  public componentDidMount() {
    RestAPI.events.on('authentication-error', () => {
      this.setState({ loggedIn: false });
      this.redirect('/login');
    });

    RestAPI.events.on('error', (err: Response) => {
      if (err.headers.get('content-length') !== '0') {
        err.json().then((v) => {
          SnackBarNotifier.show(
            <span>
              API Error <code>[{err.status}]</code>:&nbsp;
              <strong>{ResponseErrorUtil.getMessage(v)}</strong>
            </span>,
            SnackBarType.ERROR
          );
        });
      } else {
        SnackBarNotifier.show(
          <span>
            API Error <code>[{err.status}]</code>:&nbsp;Unknown error.
          </span>,
          SnackBarType.ERROR
        );
      }
    });

    this.globalState.selfUser().then((u) => {
      if (u) {
        this.setState({ isAdmin: u.isadmin, loggedIn: true });
      }
    });

    RestAPI.instanceStatus().then((val) => {
      if (!val.initialized) {
        this.redirect('/init');
      } else {
        this.globalState.instance = val;
        this.setState({});
      }
    });
  }

  public render() {
    return (
      <div>
        <SnackBar />
        {this.state.loggedIn && (
          <Header
            version={this.globalState.instance?.version}
            isAdmin={this.state.isAdmin}
            onLogout={this.onLogout.bind(this)}
            onHome={this.onHome.bind(this)}
            onUpload={this.onUpload.bind(this)}
            onMyProfile={this.onMyProfile.bind(this)}
            onAdmin={this.onAdmin.bind(this)}
            onTags={this.onTags.bind(this)}
            onSettings={this.onSettings.bind(this)}
          />
        )}

        <div className="router-outlet">
          <Router>
            <Route exact path="/init" render={() => <InitRoute />}></Route>
            <Route
              exact
              path="/confirmemail"
              render={() => <MailConfirmRoute />}
            ></Route>
            <Route
              exact
              path="/login"
              render={() => (
                <LoginRoute
                  globalState={this.globalState}
                  onLoginSuccess={() => this.setState({ loggedIn: true })}
                />
              )}
            />
            <Route
              exact
              path="/images"
              render={() => <MainRoute globalState={this.globalState} />}
            />
            <Route
              exact
              path="/images/:uid"
              render={({ match }) => (
                <ImageDetailsRoute
                  globalState={this.globalState}
                  imageUid={match.params.uid}
                />
              )}
            />
            <Route
              exact
              path="/images/:uid/edit"
              render={({ match }) => (
                <ImageEditRoute
                  globalState={this.globalState}
                  imageUid={match.params.uid}
                />
              )}
            />
            <Route exact path="/upload" render={() => <UploadRoute />} />
            <Route
              exact
              path="/users/new"
              render={() => (
                <UserEditRoute globalState={this.globalState} userId="new" />
              )}
            />
            <Route
              exact
              path="/users/:uid"
              render={({ match }) => (
                <UserDetailsRoute
                  globalState={this.globalState}
                  userId={match.params.uid}
                />
              )}
            />
            <Route
              exact
              path="/users/:uid/edit"
              render={({ match }) => (
                <UserEditRoute
                  globalState={this.globalState}
                  userId={match.params.uid}
                />
              )}
            />
            <Route
              exact
              path="/tags"
              render={() => <TagsRoute globalState={this.globalState} />}
            />
            <Route
              exact
              path="/tags/:uid"
              render={({ match }) => (
                <TagDetailsRoute
                  globalState={this.globalState}
                  tagId={match.params.uid}
                />
              )}
            />
            <Route
              exact
              path="/tags/:uid/edit"
              render={({ match }) => (
                <TagEditRoute
                  globalState={this.globalState}
                  tagId={match.params.uid}
                />
              )}
            />
            <Route
              exact
              path="/settings"
              render={() => <SettingsRoute globalState={this.globalState} />}
            />
            <Route
              exact
              path="/export"
              render={() => <ExportRoute globalState={this.globalState} />}
            />
            <Route
              exact
              path="/admin"
              render={() => <AdminRoute globalState={this.globalState} />}
            />

            <Route exact path="/" render={() => <Redirect to="/images" />} />

            {this.state.redirect && <Redirect to={this.state.redirect} />}
          </Router>
        </div>
      </div>
    );
  }

  private redirect(to: string) {
    this.setState({ redirect: to });
    setTimeout(() => this.setState({ redirect: null }), 10);
  }

  private async onLogout() {
    await RestAPI.authLogout();
    this.globalState.clearSelfUser();
    this.setState({ loggedIn: false });
    this.redirect('/login');
  }

  private onHome() {
    this.redirect('/images');
  }

  private onUpload() {
    this.redirect('/upload');
  }

  private onMyProfile() {
    this.redirect('/users/me');
  }

  private onAdmin() {
    this.redirect('/admin');
  }

  private onTags() {
    this.redirect('/tags');
  }

  private onSettings() {
    this.redirect('/settings');
  }
}
