import { getUserSessionData} from "../utils/sessionUtils";
import { workspaceClient } from "./clients/HttpClients";
import { getAvailableWorkspaces, setAvailableWorkspaces } from "../utils/sessionUtils";

const WorkspaceService = {
  async getWorkspaceById(workspaceId, loadUserData = false, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await workspaceClient.GetWorkspaceById(sessionData.authToken, workspaceId, loadUserData, onError);
      return response.workspaceDto;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to get workspace data");
    }
  },
  async getAllWorkspaceUsers(workspaceId, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await workspaceClient.GetAllWorkspaceUsers(sessionData.authToken, workspaceId, onError);
      return response.workspaceUserDtos;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to get all workspace users");
    }
  },
  async updateWorkspace(workspaceData, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await workspaceClient.UpdateWorkspace(sessionData.authToken, workspaceData, onError);
      return response.workspaceDto;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to update workspace data");
    }
  },
  async createWorkspace(workspaceData, onError) {
    try {
      const sessionData = getUserSessionData();
      await workspaceClient.CreateWorkspace(sessionData.authToken, workspaceData, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to create workspace");
    }
  },  
  async removeWorkspace(workspaceId, onError) {
    try {
      const sessionData = getUserSessionData();
      await workspaceClient.RemoveWorkspace(sessionData.authToken, workspaceId, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to remove workspace");
    }
  },
  async loadWorkspacesForUser(onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await workspaceClient.GetAllWorkspacesForUser(sessionData.authToken, sessionData.userId, onError);
      return response.workspaceDtos || [];
    } catch (error) {
      onError(error.response?.data?.message || "Workspace data loading failed");
    }
  },
  async getWorkspaceData(onError) {
    let workspaceData = getAvailableWorkspaces();
  
    if (workspaceData){
       return workspaceData;
    }

    const userWorkspaces = await this.loadUserWorkspaceSelections(onError);

    if (!userWorkspaces || userWorkspaces.length <= 0){
      onError('Failed to load workspace data');
    }

    userWorkspaces[0].selected = true;

    setAvailableWorkspaces(userWorkspaces);
    return userWorkspaces;
  },
  async loadUserWorkspaceSelections(onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await workspaceClient.GetAllWorkspaceSelections(sessionData.authToken, sessionData.userId, onError);
      return response.workspaceSelections || [];
    } catch (error) {
      onError(error.response?.data?.message || "Workspace data loading failed");
    }
  },
  async addWorkspaceUser(addUserRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await workspaceClient.AddUserToWorkspace(sessionData.authToken, addUserRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to add user to user to workspace");
    }
  },
  async modifyWorkspaceUser(modifyUserRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await workspaceClient.UpdateUserInWorkspace(sessionData.authToken, modifyUserRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to modify user in workspace");
    }
  },  
  async removeWorkspaceUser(removeUserRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await workspaceClient.RemoveUserAssingmentFromProject(sessionData.authToken, removeUserRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to remove user in workspace");
    }
  },
}

export default WorkspaceService;