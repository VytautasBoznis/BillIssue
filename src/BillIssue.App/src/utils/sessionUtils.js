import Cookies from "js-cookie";
import { 
  USER_SESSION_DATA,
  USER_COMPANY_WORKSPACE_DATA,
  HOURS_TO_STORE_SESSION_DATA
} from "../services/constants/LoginConstants";

export const setUserAuthData = (userSessionData) => {
  try {
    if (!userSessionData) {
      throw new Error('User session data not found');
    }

    if (!userSessionData.authToken) {
      throw new Error('No auth token found in user session data');
    }

    const serializedSessionData = JSON.stringify(userSessionData);
    Cookies.set(USER_SESSION_DATA, serializedSessionData, { expires: (HOURS_TO_STORE_SESSION_DATA / 24) });
  }
  catch(ex) {
    throw new Error('User session data failed to serialize');
  }
};

export const getUserAuthToken = () => {
  const serializedSessionData = Cookies.get(USER_SESSION_DATA);

  try {
    const sessionData = JSON.parse(serializedSessionData);

    if (!sessionData.authToken) {
      throw new Error('No auth token found in user session data');
    }

    return sessionData.authToken;
  }
  catch(ex) {
    logout(true);
    throw new Error('User session data failed to deserialize');
  }
};

export const getUserSessionData = () => {
  const serializedSessionData = Cookies.get(USER_SESSION_DATA);

  if(!serializedSessionData) {
    logout();
    throw new Error('User session data not found');
  }

  try {
    const sessionData = JSON.parse(serializedSessionData);
    return sessionData;
  }
  catch(ex) {
    logout(true);
    throw new Error('User session data failed to deserialize');
  }
};

export const setAvailableWorkspaces = (userWorkspaces) => {
  try {
    if (!userWorkspaces) {
      throw new Error('No user wokspaces found');
    }

    const serializedCWData = JSON.stringify(userWorkspaces);
    Cookies.set(USER_COMPANY_WORKSPACE_DATA, serializedCWData, { expires: (HOURS_TO_STORE_SESSION_DATA / 24) });
  }
  catch(ex) {
    throw new Error('Company workspace data failed to serialize');
  }
}

export const getAvailableWorkspaces = () => {
  const serializedCWData = Cookies.get(USER_COMPANY_WORKSPACE_DATA);

  if(!serializedCWData) {
    return null;
  }

  try {
    const cwData = JSON.parse(serializedCWData);
    return cwData;
  }
  catch(ex) {
    return null;
  }
}

export const getNotifications = () => {
  const sessionData = getUserSessionData();
  return sessionData?.notifications;
}

export const logout = (redirect) => {
  Cookies.remove(USER_SESSION_DATA); 
  Cookies.remove(USER_COMPANY_WORKSPACE_DATA);

  if (redirect) {
    window.location.href = '/login';
  }
};