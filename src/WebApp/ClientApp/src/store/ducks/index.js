import { combineReducers } from "redux";
import accounts from './account';
import institutions from './institution';

const rootReducer = combineReducers({
    accounts,
    institutions
});

export default rootReducer;