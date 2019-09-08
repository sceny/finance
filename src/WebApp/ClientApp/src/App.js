import React from "react";
import { Router } from "@reach/router";
import Header from "./components/layout/Header";
import HomePage from "./components/home/HomePage";
import AccountPage from "./components/accounts/AccountPage";
import PageNotFound from "./PageNotFound";

import "./custom.css";

const App = () => (
  <div>
    <Header />
    <Router>
      <HomePage exact path="/" />
      <AccountPage path="/accounts" />
      <PageNotFound default />
    </Router>
  </div>
);

export default App;
