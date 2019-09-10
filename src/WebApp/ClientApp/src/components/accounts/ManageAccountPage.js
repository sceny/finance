import React, { useState, useEffect } from 'react';
import { connect } from 'react-redux';
import {
  loadAccounts,
  saveAccount
} from '../../store/ducks/account/operations';
import { loadInstitutions } from '../../store/ducks/institution/operations';
import AccountForm from './AccountForm';
import PropTypes from 'prop-types';

function ManageAccountPage({
  accounts,
  institutions,
  loadInstitutions,
  loadAccounts,
  saveAccount,
  navigate,
  ...props
}) {
  const [account, setAccount] = useState({ ...props.account });
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (accounts.length === 0) {
      loadAccounts().catch(error => {
        alert('Loading accounts failed' + error);
      });
    } else {
      setAccount({ ...props.account });
    }

    if (institutions.length === 0) {
      loadInstitutions().catch(error => {
        alert('Loading institutions failed' + error);
      });
    }
  }, [props.account, accounts, institutions, loadAccounts, loadInstitutions]);

  function handleChange(event) {
    const { name, value } = event.target;
    setAccount(prevAccount => ({
      ...prevAccount,
      [name]: name === 'institutionId' ? parseInt(value, 10) : value
    }));
  }

  function handleSave(event) {
    event.preventDefault();
    saveAccount(account).then(() => {
      navigate('/accounts');
    });
  }

  return (
    <AccountForm
      account={account}
      errors={errors}
      institutions={institutions}
      onChange={handleChange}
      onSave={handleSave}
    />
  );
}

ManageAccountPage.propTypes = {
  account: PropTypes.object.isRequired,
  accounts: PropTypes.array.isRequired,
  institutions: PropTypes.array.isRequired,
  loadAccounts: PropTypes.func.isRequired,
  loadInstitutions: PropTypes.func.isRequired,
  saveAccount: PropTypes.func.isRequired,
  navigate: PropTypes.func.isRequired
};

export function getAccountBySlug(accounts, slug) {
  return accounts.find(account => account.slug === slug) || null;
}

function mapStateToProps(state, ownProps) {
  const slug = ownProps.slug;
  const account =
    slug && state.accounts.length
      ? getAccountBySlug(state.accounts, slug)
      : { name: '', slug: '', institutionId: 0, category: '' };
  return {
    account,
    accounts: state.accounts,
    institutions: state.institutions
  };
}

const mapDispatchToProps = {
  loadAccounts,
  loadInstitutions,
  saveAccount
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(ManageAccountPage);
