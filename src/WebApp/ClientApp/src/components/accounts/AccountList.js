import React from "react";
import { Link } from "@reach/router";

const AccountList = ({ accounts }) => (
  <table className="table">
    <thead>
      <tr>
        <th>Name</th>
        <th>Institution</th>
      </tr>
    </thead>
    <tbody>
      {accounts.map(account => {
        return (
          <tr key={account.id}>
            <td>
              <Link to={`/account/${account.slug}`}>{account.title}</Link>
            </td>
            <td>{account.institutionName}</td>
          </tr>
        );
      })}
    </tbody>
  </table>
);

export default AccountList;
