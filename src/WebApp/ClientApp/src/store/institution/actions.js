import types from './types';
import institutionApi from '../../api/institutionApi';

function loadInstitutionsSuccess(institutions) {
  return { type: types.LOAD_INSTITUTION_SUCCESS, institutions };
}

export function loadInstitutions() {
  return function(dispatch) {
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
