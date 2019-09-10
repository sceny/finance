import React from 'react';
import { Router } from '@reach/router';
import Header from './components/common/Header';
import HomePage from './components/home/HomePage';
import AccountsPage from './components/accounts/AccountsPage';
import PageNotFound from './PageNotFound';

import ManageAccountPage from './components/accounts/ManageAccountPage';

const App = () => (
  <div className='container-fluid'>
    <Header />
    <Router>
      <HomePage exact path='/' />
      <AccountsPage path='accounts' />
      <ManageAccountPage path='account' />
      <ManageAccountPage path='account/:slug' />
      <PageNotFound default />
    </Router>
  </div>
);

export default App;
