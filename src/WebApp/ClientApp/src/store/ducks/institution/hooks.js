import { useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";
import institutionApi from "../../../api/institutionApi";
import actions from "./actions";

function useInstitutions() {
  const institutions = useSelector(state => state.institutions);
  const dispatch = useDispatch();

  useEffect(() => {
    if (institutions.length) {
        return;
    }
    async function fetchData() {
        try {
          const fetchedInstitutions = await institutionApi.getInstitutions();
          dispatch(actions.loadInstitutionsSuccess(fetchedInstitutions));
        } catch (error) {
          throw error;
        }
    }
    fetchData();
  }, [dispatch, institutions]);

  return institutions;
}


export default {
    useInstitutions
}
