import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import { Provider } from 'react-redux';
import registerServiceWorker from './registerServiceWorker';
import configureStore from './store';

const rootElement = document.getElementById('root');
const store = configureStore();

ReactDOM.render(
    <Provider store={store}>
        <App />
    </Provider>,
  rootElement);

registerServiceWorker();

