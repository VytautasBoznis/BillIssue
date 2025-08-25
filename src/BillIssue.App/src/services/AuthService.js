import { setUserAuthData } from "../utils/sessionUtils";
import { authClient } from "./clients/HttpClients";

const AuthService = {
  async login(email, password, onError) {
    try {
      const response = await authClient.login({ email, password }, onError);
      setUserAuthData(response.session);
      return response;
    } catch (error) {
      throw new Error(error.response?.data?.message || "Login failed");
    }
  },
  async register(email, firstName, lastName, password, onError) {
    try {
      const response = await authClient.register({ email, firstName, lastName, password }, onError);
      setUserAuthData(response.session);
      return response;
    } catch (error) {
      throw new Error(error.response?.data?.message || "Registration failed");
    }
  },
  async remindPassword(email, onError) {
    try {
      await authClient.remindPassword({ email }, onError);
      return true;
    } catch (error) {
      throw new Error(error.response?.data?.message || "Remind password email send failed");
    }
  },
};

export default AuthService;
