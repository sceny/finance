import React, { Component } from 'react';

import './custom.css'
import AccountPage from './components/accounts/AccountPage';

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
        <AccountPage />
    );
  }
}
