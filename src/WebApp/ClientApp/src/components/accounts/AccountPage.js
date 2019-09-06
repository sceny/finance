import React from "react";
import { accountHooks } from "../../store/ducks/account";
import AccountList from "./AccountList";

function AccountPage() {
  const accounts = accountHooks.useAccounts();

  return (
    <>
      <h2>Accounts</h2>
      <AccountList accounts={accounts} />
    </>
  );
}

export default AccountPage;
