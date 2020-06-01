/** @format */

import React, { Component } from 'react';
import { UserCreateModel } from '../../api/models/user';

import './usereditor.scss';
import PasswordInput from '../passwordinput/passwordinput';

interface UserEditorProperties {
  user: UserCreateModel;
  onChange?: (user: UserCreateModel) => void;
}

export default class UserEditor extends Component<UserEditorProperties> {
  public render() {
    const user = this.props.user;

    return (
      <div className="user-editor-container">
        <label htmlFor="user-editor-username">Username:</label>
        <input
          id="user-editor-username"
          value={user.username}
          onChange={(v) =>
            this.onChange(() => (user.username = v.target.value))
          }
        />
        <label htmlFor="user-editor-displayname">Displayname:</label>
        <input
          id="user-editor-displayname"
          placeholder={user.username}
          value={user.displayname}
          onChange={(v) =>
            this.onChange(() => (user.displayname = v.target.value))
          }
        />
        <label htmlFor="user-editor-email">Email Address:</label>
        <input
          id="user-editor-email"
          value={user.emailaddress}
          onChange={(v) =>
            this.onChange(() => (user.emailaddress = v.target.value))
          }
        />
        <label htmlFor="user-editor-password">Password:</label>
        <PasswordInput
          id="user-editor-password"
          value={user.password}
          onChange={(v) => this.onChange(() => (user.password = v))}
        />

        <label htmlFor="user-editor-description">Description:</label>
        <textarea
          id="user-editor-passwdescriptionord"
          value={user.description}
          onChange={(v) =>
            this.onChange(() => (user.description = v.target.value))
          }
        />
      </div>
    );
  }

  private onChange(cb: () => void) {
    cb();
    if (this.props.onChange) {
      this.props.onChange(this.props.user);
    }
  }
}
