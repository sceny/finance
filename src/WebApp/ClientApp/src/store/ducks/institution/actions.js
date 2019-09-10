import types from './types';

export function loadInstitutionsSuccess(institutions) {
  return { type: types.LOAD_INSTITUTION_SUCCESS, institutions };
}
