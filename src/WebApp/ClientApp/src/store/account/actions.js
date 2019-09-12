import types from './types';
import accountApi from '../../api/accountApi';
function loadAccountsSuccess(accounts) {
  return { type: types.LOAD_ACCOUNT_SUCCESS, accounts };
}
function updateAccountSuccess(account) {
  return { type: types.UPDATE_ACCOUNT_SUCCESS, account };
}
function createAccountSuccess(account) {
  return { type: types.CREATE_ACCOUNT_SUCCESS, account };
}

export function loadAccounts() {
  return function(dispatch) {
    return accountApi
      .getAccounts()
      .then(accounts => {
        dispatch(loadAccountsSuccess(accounts));
      })
      .catch(error => {
        throw error;
      });
  };
}

export function saveAccount(account) {
  //eslint-disable-next-line no-unused-vars
  return function(dispatch, getState) {
    return accountApi
      .saveAccount(account)
      .then(savedAccount => {
        account.id
          ? dispatch(updateAccountSuccess(savedAccount))
          : dispatch(createAccountSuccess(savedAccount));
      })
      .catch(error => {
        throw error;
      });
  };
}
