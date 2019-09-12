import types from './types';
import initialState from '../initialState';

const loadInstitutionsSuccess = ({ institutions }) => institutions;

export default function institutionReducer (state = initialState.institutions, { type, ...args }) {
    switch (type) {
        case types.LOAD_INSTITUTION_SUCCESS:
            return loadInstitutionsSuccess(args);
        default:
            return state;
    }
}