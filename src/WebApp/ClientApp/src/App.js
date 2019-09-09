import React from "react";
import { Router } from "@reach/router";
import Header from "./components/common/Header";
import HomePage from "./components/home/HomePage";
import AccountPage from "./components/accounts/AccountPage";
import PageNotFound from "./PageNotFound";

import "./custom.css";
import ManageAccountPage from "./components/accounts/ManageAccountPage";

const App = () => (
  <div>
    <Header />
    <Router>
      <HomePage exact path="/" />
      <AccountPage path="accounts" />
      <ManageAccountPage path="account" />
      <ManageAccountPage path="account/:slug" />
      <PageNotFound default />
    </Router>
  </div>
);

export default App;
