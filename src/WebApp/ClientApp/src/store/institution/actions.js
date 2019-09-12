import types from './types';
import institutionApi from '../../api/institutionApi';
import { beginApiCall } from '../apiStatus';

function loadInstitutionsSuccess(institutions) {
  return { type: types.LOAD_INSTITUTION_SUCCESS, institutions };
}

export function loadInstitutions() {
  return function(dispatch) {
    dispatch(beginApiCall());
    return institutionApi
      .getInstitutions()
      .then(institutions => {
        dispatch(loadInstitutionsSuccess(institutions));
      })
      .catch(error => {
        throw error;
      });
  };
}
