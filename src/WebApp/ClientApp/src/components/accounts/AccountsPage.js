import React, { useEffect } from 'react';
import { connect } from 'react-redux';
import { PropTypes } from 'prop-types';
import { loadAccounts } from '../../store/ducks/account/operations';
import { loadInstitutions } from '../../store/ducks/institution/operations';
import AccountList from './AccountList';

function AccountsPage({
  accounts,
  institutions,
  loadAccounts,
  loadInstitutions
}) {
  useEffect(() => {
    if (accounts.length === 0) {
      loadAccounts().catch(error => {
        alert('Loading accounts failed' + error);
      });
    }

    if (institutions.length === 0) {
      loadInstitutions().catch(error => {
        alert('Loading institutions failed' + error);
      });
    }
  }, [accounts, institutions, loadAccounts, loadInstitutions]);

  return (
    <>
      <h2>Accounts</h2>
      <AccountList accounts={accounts} />
    </>
  );
}

AccountsPage.propTypes = {
  institutions: PropTypes.array.isRequired,
  accounts: PropTypes.array.isRequired,
  loadAccounts: PropTypes.func.isRequired,
  loadInstitutions: PropTypes.func.isRequired
};

function mapStateToProps(state) {
  return {
    accounts:
      state.institutions.length === 0
        ? []
        : state.accounts.map(account => {
            return {
              ...account,
              institutionName: state.institutions.find(
                a => a.id === account.institutionId
              ).name
            };
          }),
    institutions: state.institutions
  };
}

const mapDispatchToProps = {
  loadAccounts,
  loadInstitutions
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(AccountsPage);
