import React, { useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import { accountActions } from "../store/ducks/account";

function AccountPage() {
  const [account, setAccount] = useState({ title: "" });
  const accounts = useSelector(state => state.accounts);
  const dispatch = useDispatch();

  function handleChange(event) {
    setAccount({
        ...account,
        title: event.target.value
    });
  }

  function handleSubmit(event) {
    event.preventDefault();
    dispatch(accountActions.createAccount(account));
    setAccount({
        ...account,
        title: ""
    });
  }

  return (
    <div>
      <form onSubmit={handleSubmit}>
        <h2>Accounts</h2>
        <h3>Add Account</h3>
        <input type="text" onChange={handleChange} value={account.title} />
        <input type="submit" value="Save" />
      </form>

      <h3>Existing accounts</h3>
      <ul>
        {accounts.map(account => (
          <li key={account.title}>{account.title}</li>
        ))}
      </ul>
    </div>
  );
}

export default AccountPage;
