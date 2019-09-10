import React from 'react';
import { Link } from '@reach/router';
import PropTypes from 'prop-types';

const AccountList = ({ accounts }) => (
  <table className='table'>
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
              <Link to={`/account/${account.slug}`}>{account.name}</Link>
            </td>
            <td>{account.institutionName}</td>
          </tr>
        );
      })}
    </tbody>
  </table>
);

AccountList.propTypes = {
  accounts: PropTypes.array.isRequired
};

export default AccountList;
