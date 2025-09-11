import { getUserSessionData } from "../utils/sessionUtils";
import { projectClient } from "./clients/HttpClients";

const ProjectService = {
  async getProjectById(projectId, loadUserAssignments = false, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await projectClient.GetProjectById(sessionData.authToken, projectId, loadUserAssignments, onError);
      return response.projectDto;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to get project data");
    }
  },
  async getUserProjectsInWorkspace(workspaceId, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await projectClient.GetProjectsForUserInWorkspace(sessionData.authToken, workspaceId, onError);
      return response.projectSearchDtos;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to get project data");
    }
  },
  async createProject(createProjectRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.CreateProject(sessionData.authToken, createProjectRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to create project");
    }
  },
  async modifyProject(createProjectRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.ModifyProject(sessionData.authToken, createProjectRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to modify project");
    }
  },
  async removeProject(projectId, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.RemoveProject(sessionData.authToken, projectId, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to remove project");
    }
  },
  async createUserAssignmentToProject(userAssignmentRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.CreateUserAssignmentToProject(sessionData.authToken, userAssignmentRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to create user assignment to project");
    }
  },
  async removeUserAssingnmentFromProject(userAssignmentRemoveRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.RemoveUserAssingmentFromProject(sessionData.authToken, userAssignmentRemoveRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to remove user assignment to project");
    }
  },
  async modifyUserAssingment(modifyUserAssignmentRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.ModifyUserAssingmentInProject(sessionData.authToken, modifyUserAssignmentRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to modify user assignment to project");
    }
  },
}

export default ProjectService;