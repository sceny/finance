import accountApi from '../../../api/accountApi';
import {
  loadAccountsSuccess,
  updateAccountSuccess,
  createAccountSuccess
} from './actions';

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

export default {
  loadAccounts,
  saveAccount
};
