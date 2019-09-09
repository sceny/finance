import React from "react";
import { accountHooks } from "../../store/ducks/account";
import { institutionHooks } from "../../store/ducks/institution";
import AccountList from "./AccountList";

function AccountPage() {
  const institutionsSource = institutionHooks.useInstitutions();
  const accountsSource = accountHooks.useAccounts();
  const accounts = institutionsSource.length
    ? accountsSource.map(account => {
        return {
            ...account,
            institutionName: institutionsSource.find(i => i.id === account.institutionId).name
        };
      })
    : [];

  return (
    <>
      <h2>Accounts</h2>
      <AccountList accounts={accounts} />
    </>
  );
}

export default AccountPage;
