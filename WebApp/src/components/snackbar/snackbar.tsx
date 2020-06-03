/** @format */

import React, { Component } from 'react';

import './snackbar.scss';
import SnackBarNotifier, { SnackBarEvent } from '../../util/snackbar-notifier';
import { UnregisterHandler } from '../../util/eventemitter';

export default class SnackBar extends Component {
  public state = {
    event: (null as any) as SnackBarEvent,
    display: false,
  };

  private eventUnsubscriber: UnregisterHandler[] = [];

  public componentDidMount() {
    this.eventUnsubscriber.push(
      SnackBarNotifier.events.on('show', this.show.bind(this))
    );

    this.eventUnsubscriber.push(
      SnackBarNotifier.events.on('hide', this.hide.bind(this))
    );
  }

  public componentWillUnmount() {
    this.eventUnsubscriber.forEach((u) => u());
  }

  public render() {
    return (
      <div
        className={`snackbar-outlet${
          this.state.display ? ' snackbar-show' : ''
        }`}
      >
        {this.state.event && (
          <div className={this.containerClasses}>{this.content}</div>
        )}
      </div>
    );
  }

  private show(event: SnackBarEvent) {
    this.setState({ event, display: true });
    setTimeout(() => this.hide(), event.duration);
  }

  private hide() {
    this.setState({ display: false });
    setTimeout(() => this.setState({ event: null }), 250);
  }

  private get containerClasses(): string {
    const classes = ['snackbar-container'];
    classes.push(`snackbar-type-${this.state.event.type}`);
    return classes.join(' ');
  }

  private get content(): JSX.Element {
    const cont = this.state.event.content;
    if (typeof cont === 'string') {
      return <span>{cont}</span>;
    }

    return cont;
  }
}
