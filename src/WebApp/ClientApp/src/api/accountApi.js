import { handleResponse, handleError } from "./apiUtils";
const baseUrl = process.env.API_URL + "/accounts/";

export function getAccounts() {
  return fetch(baseUrl)
    .then(handleResponse)
    .catch(handleError);
}

export function saveAccount(account) {
  return fetch(baseUrl + (account.id || ""), {
    method: account.id ? "PUT" : "POST", // POST for create, PUT to update when id already exists.
    headers: { "content-type": "application/json" },
    body: JSON.stringify(account)
  })
    .then(handleResponse)
    .catch(handleError);
}

export function deleteAccount(accountId) {
  return fetch(baseUrl + accountId, { method: "DELETE" })
    .then(handleResponse)
    .catch(handleError);
}
