import React from 'react';
import { Router } from '@reach/router';
import Header from './components/common/Header';
import HomePage from './components/home/HomePage';
import AccountsPage from './components/accounts/AccountsPage';
import PageNotFound from './PageNotFound';
import ManageAccountPage from './components/accounts/ManageAccountPage';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const App = () => (
  <div className="container-fluid">
    <Header />
    <Router>
      <HomePage exact path="/" />
      <AccountsPage path="accounts" />
      <ManageAccountPage path="account" />
      <ManageAccountPage path="account/:slug" />
      <PageNotFound default />
    </Router>
    <ToastContainer autoClose={3000} hideProgressBar />
  </div>
);

export default App;
