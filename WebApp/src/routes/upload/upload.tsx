/** @format */

import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import ImageEditor from '../../components/imageeditor/imageeditor';
import ImageModel from '../../api/models/image';
import Container from '../../components/container/container';

interface UploadRouteProps extends RouteComponentProps {}

class UploadRoute extends Component<UploadRouteProps> {
  public state = {
    image: {} as ImageModel,
  };

  public render() {
    return (
      <div>
        <h2>Image Upload</h2>
        <Container title="Image Information">
          <ImageEditor
            image={this.state.image}
            onChange={(image) => this.setState({ image })}
          />
        </Container>
      </div>
    );
  }
}

export default withRouter(UploadRoute);
