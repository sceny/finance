import types from './types';

export function loadAccountsSuccess(accounts) {
  return { type: types.LOAD_ACCOUNT_SUCCESS, accounts };
}
export function updateAccountSuccess(account) {
  return { type: types.UPDATE_ACCOUNT_SUCCESS, account };
}
export function createAccountSuccess(account) {
  return { type: types.CREATE_ACCOUNT_SUCCESS, account };
}
