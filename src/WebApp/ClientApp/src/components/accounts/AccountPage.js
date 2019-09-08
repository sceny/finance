import React from "react";
import { accountHooks } from "../../store/ducks/account";
import { institutionHooks } from "../../store/ducks/institution";
import AccountList from "./AccountList";

function AccountPage() {
  const institutions = institutionHooks.useInstitutions();
  const rawAccounts = accountHooks.useAccounts();
  const accounts = institutions.length
    ? rawAccounts.map(account => {
        return {
            ...account,
            institutionName: institutions.find(i => i.id === account.institutionId).title
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
