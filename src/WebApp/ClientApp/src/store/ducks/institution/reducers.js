import types from './types';

const INITIAL_STATE = [];

const loadInstitutionsSuccess = ({ institutions }) => institutions;

export default function institutionReducer (state = INITIAL_STATE, { type, ...args }) {
    switch (type) {
        case types.LOAD_INSTITUTION_SUCCESS:
            return loadInstitutionsSuccess(args);
        default:
            return state;
    }
}