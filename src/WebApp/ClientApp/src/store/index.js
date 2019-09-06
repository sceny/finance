import { createStore, applyMiddleware, compose } from 'redux';
import rootReducer from './ducks';
import reduxImmutableStateInvariant from 'redux-immutable-state-invariant';
import thunk from 'redux-thunk';


export default function configureStore(initialState) {
    const composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose; // add support for Redux dev tools;
    // const middleware = process.env.NODE_ENV !== 'production' ?
    //   [require('redux-immutable-state-invariant').default(), thunk] :
    //   [thunk];
    return createStore(
        rootReducer,
        initialState,
        composeEnhancers(applyMiddleware(thunk, reduxImmutableStateInvariant()))
    );
}