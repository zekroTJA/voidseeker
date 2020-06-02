/** @format */

import React, { Component } from 'react';

import './modal.scss';

interface ModalProps {
  onClose?: () => void;
  open?: boolean;
}

export default class Modal extends Component<ModalProps> {
  public render() {
    return (
      <div id="modal-container-bg" onClick={this.onBgClick.bind(this)}>
        <div className="modal-conatiner">{this.props.children}</div>
      </div>
    );
  }

  private onBgClick(e: React.MouseEvent<HTMLDivElement, MouseEvent>) {
    if (
      this.props.onClose &&
      (e.target as HTMLDivElement).id === 'modal-container-bg'
    ) {
      this.props.onClose();
    }
  }
}
