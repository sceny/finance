import React from "react";
import { Route } from "react-router";
import { Layout } from "./components/Layout";
import { Home } from "./components/Home";
import { Counter } from "./components/Counter";

import "semantic-ui-css/semantic.min.css";

export default function App() {
  return (
    <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/counter' component={Counter} />
    </Layout>
  );
}
