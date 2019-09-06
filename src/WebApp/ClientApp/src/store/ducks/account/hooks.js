import accountApi from "../../../api/accountApi";
import actions from "./actions";
import { useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";

function useAccounts() {
  const accounts = useSelector(state => state.accounts);
  const dispatch = useDispatch();

  useEffect(() => {
    async function fetchData() {
        try {
          const accounts = await accountApi.getAccounts();
          dispatch(actions.loadAccountsSuccess(accounts));
        } catch (error) {
          throw error;
        }
    }
    fetchData();
  }, [dispatch]);

  return accounts;
}


export default {
    useAccounts
}
