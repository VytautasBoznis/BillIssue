import axios from 'axios';
import { logout } from '../../utils/sessionUtils';

class HttpClient {
  constructor(baseURL) {
    this.client = axios.create({
      baseURL,
      headers: {
        "Content-Type": "application/json",
      },
    });
  }

  async request(endpoint, method, data, onError, authToken) {
    try {
      const request = {
        url: endpoint,
        method,
      }

      if(data) {
        request.data = data;
      }

      if (authToken) {
        request.headers = {
          'AuthToken': authToken
        };
      }

      const response = await this.client(request);
      return response.data;
    } catch (error) {

      console.log(error);
      if (error.response && error.response.status === 401) {
        logout(true);
      }

      const errorMessage = error.response?.data?.message || "An error occurred";
      onError(errorMessage);
      throw new Error(errorMessage);
    }
  }
}

class AuthHttpClient extends HttpClient {
  async login(credentials, onError) {
    return this.request(`/Auth/login`, "POST", credentials, onError);
  }
  async register(registerData, onError) {
    return this.request(`/auth/register`, "POST", registerData, onError);
  }
  async remindPassword(emailData, onError) {
    return this.request(`/auth/remindPassword`, "POST", emailData, onError);
  }
}

class UserHttpClient extends HttpClient {
  async getSessionUserData(authToken, onError) {
    return this.request(`/User/GetCurrentSessionUserData`, "GET", null, onError, authToken);
  }
}

class WorkspaceHttpClient extends HttpClient {
  async GetWorkspaceById(authToken, workspaceId, loadUserData, onError) {
    return this.request(`/Workspace/GetWorkspace/${workspaceId}?loadUserAssignments=${loadUserData}`, "GET", null, onError, authToken);
  }
  async GetAllWorkspaceUsers(authToken, workspaceId, onError) {
    return this.request(`/Workspace/GetAllWorkspaceUsers/${workspaceId}`, "GET", null, onError, authToken);
  }
  async UpdateWorkspace(authToken, workspaceData, onError) {
    return this.request(`/Workspace/UpdateWorkspace`, "PATCH", workspaceData, onError, authToken);
  }
  async CreateWorkspace(authToken, workspaceData, onError) {
    return this.request(`/Workspace/CreateWorkspace`, "POST", workspaceData, onError, authToken);
  }
  async RemoveWorkspace(authToken, workspaceId, onError) {
    return this.request(`/Workspace/RemoveWorkspace/${workspaceId}`, "DELETE", null, onError, authToken);
  }
  async GetAllWorkspacesForUser(authToken, userId, onError) {
    return this.request(`/Workspace/GetAllWorkspacesForUser/${userId}`, "GET", null, onError, authToken);
  }
  async GetAllWorkspaceSelections(authToken, userId, onError) {
    return this.request(`/Workspace/GetAllWorkspaceSelectionsForUser/${userId}`, "GET", null, onError, authToken);
  }
  async AddUserToWorkspace(authToken, addUserRequest, onError) {
    return this.request(`/Workspace/AddUserToWorkspace`, "POST", addUserRequest, onError, authToken);
  }
  async UpdateUserInWorkspace(authToken, modifyUserRequest, onError) {
    return this.request(`/Workspace/UpdateUserInWorkspace`, "PATCH", modifyUserRequest, onError, authToken);
  }  
  async RemoveUserAssingmentFromProject(authToken, request, onError) {
    return this.request(`/Workspace/RemoveUserFromWorkspace`, "DELETE", request, onError, authToken);
  }
}

class TimeLoggingHttpClient extends HttpClient {
  async DeleteTimeEntry(authToken, timelogEntryId, onError) {
    return this.request(`/TimeLogging/RemoveTimeLogEntry/${timelogEntryId}`, "DELETE", null, onError, authToken);
  }
  async EditTimeEntry(authToken, timelogEntry, onError) {
    return this.request(`/TimeLogging/ModifyTimeLogEntry`, "PATCH", timelogEntry, onError, authToken);
  }
  async LogTimeEntry(authToken, timelogEntry, onError) {
    return this.request(`/TimeLogging/CreateTimeLogEntry`, "POST", timelogEntry, onError, authToken);
  }
  async GetWeekOfTimeEntries(authToken, timeEntryLookupFilter, onError) {
    return this.request(`/TimeLogging/GetWeekOfTimeEntries`, "POST", timeEntryLookupFilter, onError, authToken);
  }
}

class ProjectHttpClient extends HttpClient {
  async GetProjectById(authToken, projectId, loadUserAssignments, onError) {
    return this.request(`/Project/GetProject/${projectId}?loadUserAssignments=${loadUserAssignments ? 'true' : 'false'}`, "GET", null, onError, authToken);
  }
  async GetProjectsForUserInWorkspace(authToken, workspaceId, onError) {
    return this.request(`/Project/GetProjectsForUserInWorkspace/${workspaceId}`, "GET", null, onError, authToken);
  }
  async CreateProject(authToken, request, onError) {
    return this.request(`/Project/CreateProject`, "POST", request, onError, authToken);
  }
  async ModifyProject(authToken, request, onError) {
    return this.request(`/Project/ModifyProject`, "PATCH", request, onError, authToken);
  }
  async RemoveProject(authToken, projectId, onError) {
    return this.request(`/Project/RemoveProject/${projectId}`, "DELETE", null, onError, authToken);
  }
  async CreateUserAssignmentToProject(authToken, request, onError) {
    return this.request(`/Project/AddUserAssignmentToProject`, "POST", request, onError, authToken);
  }
  async RemoveUserAssingmentFromProject(authToken, request, onError) {
    return this.request(`/Project/RemoveUserAssingmentFromProject/${request.projectId}/${request.projectUserAssignmentId}`, "DELETE", null, onError, authToken);
  }
  async ModifyUserAssingmentInProject(authToken, request, onError) {
    return this.request(`/Project/ModifyUserAssingmentInProject`, "PATCH", request, onError, authToken);
  }
}

class ProjectWorktypeHttpClient extends HttpClient {
  async GetProjectWorktypeById(authToken, projectWorktypeId, onError) {
    return this.request(`/Project/GetProjectWorktype/${projectWorktypeId}`, "GET", null, onError, authToken);
  }
  async RemoveProjectWorktype(authToken, projectId, projectWorktypeId, onError) {
    return this.request(`/Project/RemoveProjectWorktype/${projectId}/${projectWorktypeId}`, "DELETE", null, onError, authToken);
  }
  async ModifyProjectWorktype(authToken, request, onError) {
    return this.request(`/Project/ModifyProjectWorktype`, "PATCH", request, onError, authToken);
  }
}

class NotificationHttpClient extends HttpClient {
  async GetNotifications(authToken, onError) {
    return this.request(`/Notification/GetNotifications/`, "GET", null, onError, authToken);
  }  
  async DoNotificationDecision(authToken, request, onError) {
    return this.request(`/Notification/DoNotificationDecision`, "POST", request, onError, authToken);
  }
}

const baseURL = process.env.REACT_APP_API_URL;
const authClient = new AuthHttpClient(baseURL);
const userClient = new UserHttpClient(baseURL);
const workspaceClient = new WorkspaceHttpClient(baseURL);
const timeloggingClient = new TimeLoggingHttpClient(baseURL);
const projectClient = new ProjectHttpClient(baseURL);
const projectWorktypeClient = new ProjectWorktypeHttpClient(baseURL);
const notificationsClient = new NotificationHttpClient(baseURL);

export { 
  authClient,
  userClient,
  workspaceClient,
  timeloggingClient,
  projectClient,
  projectWorktypeClient,
  notificationsClient
};