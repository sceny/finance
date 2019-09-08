import React from "react";
import { Link } from "react-router-dom";

const AccountList = ({ accounts }) => (
  <table className="table">
    <thead>
      <tr>
        <th />
        <th>Name</th>
        <th>Institution</th>
      </tr>
    </thead>
    <tbody>
      {accounts.map(account => {
        return (
          <tr key={account.id}>
            <td>
              <a
                className="btn btn-light"
                href={`http://pluralsight.com/accounts/${account.slug}`}
              >
                Watch
              </a>
            </td>
            <td>
              <Link to={"/account/" + account.slug}>{account.title}</Link>
            </td>
            <td>{account.institutionName}</td>
          </tr>
        );
      })}
    </tbody>
  </table>
);

export default AccountList;
