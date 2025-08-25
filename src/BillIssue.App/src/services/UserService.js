// import { getUserAuthToken, setUserSessionData } from "../utils/sessionUtils";
// import { userClient } from "./clients/HttpClients";

// const UserService = {
//   async loadUserSessionData() {
//     try {
//       const authToken = getUserAuthToken();
//       const response = await userClient.getSessionUserData(authToken, () => {});
//       setUserSessionData(response.data);
//     } catch (error) {
//       throw new Error(error.response?.data?.message || "Geting user data failed");
//     }
//   },
// };

// export default UserService;