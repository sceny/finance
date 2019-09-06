import types from './types';

export function createAccount(account) {
    return { type: types.CREATE_ACCOUNT, account };
}

export default {
    createAccount
}