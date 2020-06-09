/** @format */

import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import LocalStorage from './util/localstorage';

import './index.scss';
import './vars.css';

if (LocalStorage.get<boolean>('dark_theme')) {
  import('./apply-darktheme');
}

ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById('root')
);
