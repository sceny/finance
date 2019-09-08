import { useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";
import accountApi from "../../../api/accountApi";
import actions from "./actions";

function useAccounts() {
  const accounts = useSelector(state => state.accounts);
  const dispatch = useDispatch();

  useEffect(() => {
    if (accounts.length) {
        return;
    }
    async function fetchData() {
        try {
          const fetchedAccounts = await accountApi.getAccounts();
          dispatch(actions.loadAccountsSuccess(fetchedAccounts));
        } catch (error) {
          throw error;
        }
    }
    fetchData();
  }, [dispatch, accounts]);

  return accounts;
}


export default {
    useAccounts
}
