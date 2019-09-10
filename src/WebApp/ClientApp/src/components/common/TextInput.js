import React from 'react';
import classnames from 'classnames';
import PropTypes from 'prop-types';

const TextInput = ({
  name,
  label,
  onChange,
  placeholder,
  defaultValue,
  error
}) => {
  const className = classnames('form-group', {
    'has-error': error && error.length
  });

  return (
    <div className={className}>
      <label htmlFor={name}>{label}</label>
      <div className='field'>
        <input
          type='text'
          name={name}
          className='form-control'
          placeholder={placeholder}
          defaultValue={defaultValue}
          onChange={onChange}
        />
        {error && <div className='alert alert-danger'>{error}</div>}
      </div>
    </div>
  );
};

TextInput.propTypes = {
  name: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  placeholder: PropTypes.string,
  defaultValue: PropTypes.string,
  error: PropTypes.string
};

export default TextInput;
