/** @format */

import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Redirect } from 'react-router-dom';
import LoginRoute from './routes/login/login';
import { RestAPI } from './api/restapi';
import InitRoute from './routes/init/init';

import './App.scss';
import GlobalState from './util/globalstate';

export default class App extends Component {
  public state = {
    redirect: '',
  };

  private globalState = new GlobalState();

  public componentDidMount() {
    RestAPI.events.on('authentication-error', () =>
      this.setState({ redirect: '/login' })
    );

    RestAPI.instanceStatus().then((val) => {
      if (!val.initialized) {
        this.setState({ redirect: '/init' });
      }
    });
  }

  public render() {
    return (
      <div className="router-outlet">
        <Router>
          <Route
            exact
            path="/login"
            render={() => <LoginRoute globalState={this.globalState} />}
          ></Route>
          <Route exact path="/init" render={() => <InitRoute />}></Route>
          {this.state.redirect && <Redirect to={this.state.redirect} />}
        </Router>
      </div>
    );
  }
}
