import types from './types';
import initialState from '../initialState';

export default function accountReducer(
  state = initialState.accounts,
  { type, ...args }
) {
  switch (type) {
    case types.LOAD_ACCOUNT_SUCCESS:
      return args.accounts;
    case types.UPDATE_ACCOUNT_SUCCESS:
      return state.map(account =>
        account.id === args.account.id ? args.account : account
      );
    case types.CREATE_ACCOUNT_SUCCESS:
      return [...state, { ...args.account }];
    default:
      return state;
  }
}
