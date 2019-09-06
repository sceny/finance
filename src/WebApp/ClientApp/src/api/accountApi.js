import { handleResponse, handleError, baseUrl } from "./apiUtils";
const accountsBaseUrl = `${baseUrl}/accounts/`;

function getAccounts() {
  return fetch(accountsBaseUrl)
    .then(handleResponse)
    .catch(handleError);
}

function saveAccount(account) {
  return fetch(accountsBaseUrl + (account.id || ""), {
    method: account.id ? "PUT" : "POST", // POST for create, PUT to update when id already exists.
    headers: { "content-type": "application/json" },
    body: JSON.stringify(account)
  })
    .then(handleResponse)
    .catch(handleError);
}

function deleteAccount(accountId) {
  return fetch(accountsBaseUrl + accountId, { method: "DELETE" })
    .then(handleResponse)
    .catch(handleError);
}

export default {
    getAccounts,
    saveAccount,
    deleteAccount
}
