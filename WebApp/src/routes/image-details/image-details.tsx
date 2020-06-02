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

interface ImageDetailsRouteProps extends RouteComponentProps {
  globalState: GlobalState;
  imageUid: string;
}

class ImageDetailsRoute extends Component<ImageDetailsRouteProps> {
  public state = {
    image: (null as any) as ImageModel,
    owner: (null as any) as UserModel,
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
                    <th>Media Type</th>
                    <td>{img.mimetype}</td>
                  </tr>
                  <tr>
                    <th>Restriction</th>
                    <td>{img.public ? 'public' : 'private'}</td>
                  </tr>
                </tbody>
              </table>
            </Container>
          </div>
        )}
      </div>
    );
  }

  private get ownerDisplayName(): string {
    const owner = this.state.owner;
    if (owner.displayname === owner.username || !owner.displayname) {
      return owner.username;
    }
    return `${owner.displayname} (${owner.username})`;
  }
}

export default withRouter(ImageDetailsRoute);
