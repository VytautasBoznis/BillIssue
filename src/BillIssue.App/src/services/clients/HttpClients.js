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

  async request(endpoint, method, data, onError, jwtToken) {
    try {
      const request = {
        url: endpoint,
        method,
      }

      if(data) {
        request.data = data;
      }

      if (jwtToken) {
        request.headers = {
          'Authorization': `Bearer ${jwtToken}`
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
  async getSessionUserData(jwtToken, onError) {
    return this.request(`/User/GetCurrentSessionUserData`, "GET", null, onError, jwtToken);
  }
}

class WorkspaceHttpClient extends HttpClient {
  async GetWorkspaceById(jwtToken, workspaceId, loadUserData, onError) {
    return this.request(`/Workspace/GetWorkspace/${workspaceId}?loadUserAssignments=${loadUserData}`, "GET", null, onError, jwtToken);
  }
  async GetAllWorkspaceUsers(jwtToken, workspaceId, onError) {
    return this.request(`/Workspace/GetAllWorkspaceUsers/${workspaceId}`, "GET", null, onError, jwtToken);
  }
  async UpdateWorkspace(jwtToken, workspaceData, onError) {
    return this.request(`/Workspace/UpdateWorkspace`, "PATCH", workspaceData, onError, jwtToken);
  }
  async CreateWorkspace(jwtToken, workspaceData, onError) {
    return this.request(`/Workspace/CreateWorkspace`, "POST", workspaceData, onError, jwtToken);
  }
  async RemoveWorkspace(jwtToken, workspaceId, onError) {
    return this.request(`/Workspace/RemoveWorkspace/${workspaceId}`, "DELETE", null, onError, jwtToken);
  }
  async GetAllWorkspacesForUser(jwtToken, userId, onError) {
    return this.request(`/Workspace/GetAllWorkspacesForUser/${userId}`, "GET", null, onError, jwtToken);
  }
  async GetAllWorkspaceSelections(jwtToken, userId, onError) {
    return this.request(`/Workspace/GetAllWorkspaceSelectionsForUser/${userId}`, "GET", null, onError, jwtToken);
  }
  async AddUserToWorkspace(jwtToken, addUserRequest, onError) {
    return this.request(`/Workspace/AddUserToWorkspace`, "POST", addUserRequest, onError, jwtToken);
  }
  async UpdateUserInWorkspace(jwtToken, modifyUserRequest, onError) {
    return this.request(`/Workspace/UpdateUserInWorkspace`, "PATCH", modifyUserRequest, onError, jwtToken);
  }  
  async RemoveUserAssingmentFromProject(jwtToken, request, onError) {
    return this.request(`/Workspace/RemoveUserFromWorkspace`, "DELETE", request, onError, jwtToken);
  }
}

class TimeLoggingHttpClient extends HttpClient {
  async DeleteTimeEntry(jwtToken, timelogEntryId, onError) {
    return this.request(`/TimeLogging/RemoveTimeLogEntry/${timelogEntryId}`, "DELETE", null, onError, jwtToken);
  }
  async EditTimeEntry(jwtToken, timelogEntry, onError) {
    return this.request(`/TimeLogging/ModifyTimeLogEntry`, "PATCH", timelogEntry, onError, jwtToken);
  }
  async LogTimeEntry(jwtToken, timelogEntry, onError) {
    return this.request(`/TimeLogging/CreateTimeLogEntry`, "POST", timelogEntry, onError, jwtToken);
  }
  async GetWeekOfTimeEntries(jwtToken, timeEntryLookupFilter, onError) {
    return this.request(`/TimeLogging/GetWeekOfTimeEntries`, "POST", timeEntryLookupFilter, onError, jwtToken);
  }
  async SearchTimeLogEntries(jwtToken, searchTimeLoggingEntriesRequest, onError) {
    return this.request(`/TimeLogging/SearchTimeLogEntries`, "POST", searchTimeLoggingEntriesRequest, onError, jwtToken);
  }
}

class ProjectHttpClient extends HttpClient {
  async GetProjectById(jwtToken, projectId, loadUserAssignments, onError) {
    return this.request(`/Project/GetProject/${projectId}?loadUserAssignments=${loadUserAssignments ? 'true' : 'false'}`, "GET", null, onError, jwtToken);
  }
  async GetProjectsForUserInWorkspace(jwtToken, workspaceId, onError) {
    return this.request(`/Project/GetProjectsForUserInWorkspace/${workspaceId}`, "GET", null, onError, jwtToken);
  }
  async CreateProject(jwtToken, request, onError) {
    return this.request(`/Project/CreateProject`, "POST", request, onError, jwtToken);
  }
  async ModifyProject(jwtToken, request, onError) {
    return this.request(`/Project/ModifyProject`, "PATCH", request, onError, jwtToken);
  }
  async RemoveProject(jwtToken, projectId, onError) {
    return this.request(`/Project/RemoveProject/${projectId}`, "DELETE", null, onError, jwtToken);
  }
  async CreateUserAssignmentToProject(jwtToken, request, onError) {
    return this.request(`/Project/AddUserAssignmentToProject`, "POST", request, onError, jwtToken);
  }
  async RemoveUserAssingmentFromProject(jwtToken, request, onError) {
    return this.request(`/Project/RemoveUserAssingmentFromProject/${request.projectId}/${request.projectUserAssignmentId}`, "DELETE", null, onError, jwtToken);
  }
  async ModifyUserAssingmentInProject(jwtToken, request, onError) {
    return this.request(`/Project/ModifyUserAssingmentInProject`, "PATCH", request, onError, jwtToken);
  }
}

class ProjectWorktypeHttpClient extends HttpClient {
  async GetProjectWorktypeById(jwtToken, projectWorktypeId, onError) {
    return this.request(`/Project/GetProjectWorktype/${projectWorktypeId}`, "GET", null, onError, jwtToken);
  }
  async RemoveProjectWorktype(jwtToken, projectId, projectWorktypeId, onError) {
    return this.request(`/Project/RemoveProjectWorktype/${projectId}/${projectWorktypeId}`, "DELETE", null, onError, jwtToken);
  }
  async ModifyProjectWorktype(jwtToken, request, onError) {
    return this.request(`/Project/ModifyProjectWorktype`, "PATCH", request, onError, jwtToken);
  }
}

class NotificationHttpClient extends HttpClient {
  async GetNotifications(jwtToken, onError) {
    return this.request(`/Notification/GetNotifications/`, "GET", null, onError, jwtToken);
  }  
  async DoNotificationDecision(jwtToken, request, onError) {
    return this.request(`/Notification/DoNotificationDecision`, "POST", request, onError, jwtToken);
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