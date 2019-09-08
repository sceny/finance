import { handleResponse, handleError, baseUrl } from "./apiUtils";
const institutionsBaseUrl = `${baseUrl}/institutions/`;

function getInstitutions() {
  return fetch(institutionsBaseUrl)
    .then(handleResponse)
    .catch(handleError);
}
export default {
    getInstitutions
}
