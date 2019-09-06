import types from './types';

const INITIAL_STATE = [];

const create = (state = INITIAL_STATE, { account }) => {
    return [...state, { ...account }];
}

const loadAccountsSuccess = ({ accounts }) => accounts;

export default function accountReducer (state = INITIAL_STATE, { type, ...args }) {
    switch (type) {
        case types.CREATE_ACCOUNT:
            return create(state, args);
        case types.LOAD_ACCOUNT_SUCCESS:
            return loadAccountsSuccess(args);
        default:
            return state;
    }
}