/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter, NavLink } from 'react-router-dom';
import GlobalState from '../../util/globalstate';
import ImageModel, { gradeString } from '../../api/models/image';
import { RestAPI } from '../../api/restapi';
import Container from '../../components/container/container';
import { UserModel } from '../../api/models/user';
import moment from 'moment';
import { byteFormatter } from 'byte-formatter';

import './image-details.scss';
import Modal from '../../components/modal/modal';
import SnackBarNotifier, { SnackBarType } from '../../util/snackbar-notifier';

interface ImageDetailsRouteProps extends RouteComponentProps {
  globalState: GlobalState;
  imageUid: string;
}

class ImageDetailsRoute extends Component<ImageDetailsRouteProps> {
  public state = {
    image: (null as any) as ImageModel,
    owner: (null as any) as UserModel,
    deleteModal: false,
  };

  public async componentDidMount() {
    const gstate = this.props.globalState;

    let image = gstate.images.data.find((i) => i.uid === this.props.imageUid);
    if (!image) {
      try {
        image = await RestAPI.imageInfo(this.props.imageUid);
      } catch {}
    }
    this.setState({ image });

    try {
      const owner = await RestAPI.user(image!.owneruid);
      this.setState({ owner });
    } catch {}
  }

  public render() {
    const img = this.state.image;
    const owner = this.state.owner;
    return (
      <div>
        {this.state.deleteModal && this.deleteModal}
        {img && (
          <div className="image-details-container">
            <img
              className="image-details-preview"
              src={RestAPI.imageUrl(img.uid)}
              alt={img.title || img.filename}
            />
            <Container title="Image Information">
              <table className="image-details-table">
                <tbody>
                  <tr>
                    <th>Title</th>
                    <td>{img.title || img.filename}</td>
                  </tr>
                  <tr>
                    <th>Description</th>
                    <td>{img.description || <i>No description.</i>}</td>
                  </tr>
                  <tr>
                    <th>Tags</th>
                    <td>{img.tagsarray.join(', ')}</td>
                  </tr>
                  <tr>
                    <th>Grade</th>
                    <td>{gradeString(img.grade) || <i>No grade set.</i>}</td>
                  </tr>
                  <tr>
                    <th>Explicit</th>
                    <td>{img.explicit ? 'Yes' : 'No'}</td>
                  </tr>
                  <tr>
                    <th>Uploaded</th>
                    <td>
                      {moment(img.created).format('YYYY-MM-DD HH:mm:ss Z')}
                    </td>
                  </tr>
                  <tr>
                    <th>Uploaded by</th>
                    <td>
                      {owner ? (
                        <NavLink to={`/users/${owner.uid}`}>
                          {this.ownerDisplayName}
                        </NavLink>
                      ) : (
                        <i>User deleted or private.</i>
                      )}
                    </td>
                  </tr>
                  <tr>
                    <th>Size</th>
                    <td>{byteFormatter(img.size)}</td>
                  </tr>
                  <tr>
                    <th>Resolution</th>
                    <td>
                      {img.width} x {img.height}
                    </td>
                  </tr>
                  <tr>
                    <th>Media Type</th>
                    <td>{img.mimetype}</td>
                  </tr>
                  <tr>
                    <th>MD5 Hash</th>
                    <td>{img.md5hash}</td>
                  </tr>
                  <tr>
                    <th>Restriction</th>
                    <td>{img.public ? 'public' : 'private'}</td>
                  </tr>
                </tbody>
              </table>
              <div>
                <a
                  href={RestAPI.imageUrl(img.uid)}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Open in original size
                </a>
                <br />
                <a
                  href={RestAPI.imageUrl(img.uid)}
                  download={img.title || img.filename}
                >
                  Download image
                </a>
              </div>
              <div className="image-detail-control-btns">
                <NavLink to={`/images/${img.uid}/edit`}>
                  <button className="w-100">Edit</button>
                </NavLink>
                <button onClick={this.onDelete.bind(this)}>Delete</button>
              </div>
            </Container>
          </div>
        )}
      </div>
    );
  }

  private get deleteModal(): JSX.Element {
    return (
      <Modal onClose={() => this.setState({ deleteModal: false })}>
        <span>
          Do you really want to delete this image?
          <br />
          <div className="highlight-red mt-5">
            <strong>This action is permanent and can not be undone!</strong>
          </div>
          <div className="modal-control-buttons">
            <button onClick={() => this.setState({ deleteModal: false })}>
              Cancel
            </button>
            <button onClick={this.onDeleteConfirm.bind(this)}>
              <strong>Delete</strong>
            </button>
          </div>
        </span>
      </Modal>
    );
  }

  private get ownerDisplayName(): string {
    const owner = this.state.owner;
    if (owner.displayname === owner.username || !owner.displayname) {
      return owner.username;
    }
    return `${owner.displayname} (${owner.username})`;
  }

  private onDelete() {
    this.setState({ deleteModal: true });
  }

  private async onDeleteConfirm() {
    try {
      await RestAPI.deleteImage(this.state.image.uid);
      SnackBarNotifier.show(
        'Image successfully deleted.',
        SnackBarType.SUCCESS,
        4000
      );
      this.props.history.goBack();
    } catch {}
    this.setState({ deleteModal: false });
  }
}

export default withRouter(ImageDetailsRoute);
