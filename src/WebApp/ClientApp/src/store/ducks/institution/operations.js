import institutionApi from '../../../api/institutionApi';
import { loadInstitutionsSuccess } from './actions';

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

export default {
  loadInstitutions
};
