import { createStore, applyMiddleware, compose, combineReducers } from 'redux';
import reduxImmutableStateInvariant from 'redux-immutable-state-invariant';
import thunk from 'redux-thunk';
import accounts from './account';
import institutions from './institution';

export const rootReducer = combineReducers({
  accounts,
  institutions
});

export default function configureStore(initialState) {
  const composeEnhancers =
    window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose; // add support for Redux dev tools;
  // const middleware = process.env.NODE_ENV !== 'production' ?
  //   [require('redux-immutable-state-invariant').default(), thunk] :
  //   [thunk];
  return createStore(
    rootReducer,
    initialState,
    composeEnhancers(applyMiddleware(thunk, reduxImmutableStateInvariant()))
  );
}
