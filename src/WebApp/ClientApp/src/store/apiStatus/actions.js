import types from './types';

export function beginApiCall() {
  return { type: types.BEGIN_API_CALL };
}
