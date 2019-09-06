import types from './types';

export function createAccount(account) {
    return { type: types.CREATE_ACCOUNT, account };
}
export function loadAccountsSuccess(accounts) {
    return { type: types.LOAD_ACCOUNT_SUCCESS, accounts };
}

export default {
    createAccount,
    loadAccountsSuccess
}