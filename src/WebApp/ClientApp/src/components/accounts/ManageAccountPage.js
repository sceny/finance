import React, { useState } from "react";
import { accountHooks } from "../../store/ducks/account";
import { institutionHooks } from "../../store/ducks/institution";
import AccountForm from "./AccountForm";
import PropTypes from "prop-types";

function ManageAccountPage({ ...props }) {
  const [account, setAccount] = useState({ ...props.account });
  const [errors, setErrors] = useState({ ...props.errors });
  const institutions = institutionHooks.useInstitutions();
  const accountsSource = accountHooks.useAccounts();
  const accounts = institutions.length
    ? accountsSource.map(account => {
        return {
          ...account,
          institutionName: institutions.find(
            i => i.id === account.institutionId
          ).name
        };
      })
    : [];

  function handleChange(event) {
    const { name, value } = event.target;
    setAccount(prevAccount => ({
      ...prevAccount,
      [name]: name === "institutionId" ? parseInt(value, 10) : value
    }));
  }

  return (
    <AccountForm
      account={account}
      errors={errors}
      institutions={institutions}
      onChange={handleChange}
    />
  );
}

ManageAccountPage.propTypes = {
    account: PropTypes.object.isRequired,
    errors: PropTypes.array.isRequired
}

export default ManageAccountPage;
