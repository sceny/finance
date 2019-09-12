import React from 'react';
import PropTypes from 'prop-types';
import TextInput from '../common/TextInput';
import SelectInput from '../common/SelectInput';

const AccountForm = ({
  account,
  institutions,
  onSave,
  onChange,
  saving = false,
  errors = {}
}) => {
  return (
    <form onSubmit={onSave}>
      <h2>{account.id ? 'Edit' : 'Add'} Account</h2>
      {errors.onSave && errors.onSave.errors && (
        <div className="alert alert-danger" role="alert">
          {errors.onSave.title}
          <ul>
            {Object.keys(errors.onSave.errors).map(key => (
              <li key={key}>{errors.onSave.errors[key][0]}</li>
            ))}
          </ul>
        </div>
      )}
      <TextInput
        name="name"
        label="Name"
        defaultValue={account.name}
        onChange={onChange}
        error={errors.name}
      />

      <SelectInput
        name="institutionId"
        label="Institution"
        value={account.institutionId || ''}
        defaultOption="Select Institution"
        options={institutions.map(institution => ({
          value: institution.id,
          text: institution.name
        }))}
        onChange={onChange}
        error={errors.institution}
      />

      <TextInput
        name="type"
        label="Type"
        defaultValue={account.type}
        onChange={onChange}
        error={errors.type}
      />

      <button type="submit" disabled={saving} className="btn btn-primary">
        {saving ? 'Saving...' : 'Save'}
      </button>
    </form>
  );
};

AccountForm.propTypes = {
  institutions: PropTypes.array.isRequired,
  account: PropTypes.object.isRequired,
  errors: PropTypes.object,
  onSave: PropTypes.func.isRequired,
  onChange: PropTypes.func.isRequired,
  saving: PropTypes.bool
};

export default AccountForm;