import types from './types';
import initialState from '../initialState';

const create = (state, { account }) => {
    return [...state, { ...account }];
}

const loadAccountsSuccess = ({ accounts }) => accounts;

export default function accountReducer (state = initialState.accounts, { type, ...args }) {
    switch (type) {
        case types.CREATE_ACCOUNT:
            return create(state, args);
        case types.LOAD_ACCOUNT_SUCCESS:
            return loadAccountsSuccess(args);
        default:
            return state;
    }
}