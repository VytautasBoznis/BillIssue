import { getUserSessionData } from "../utils/sessionUtils";
import { projectClient } from "./clients/HttpClients";

const ProjectService = {
  async getProjectById(projectId, loadUserAssignments = false, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await projectClient.GetProjectById(sessionData.jwtToken, projectId, loadUserAssignments, onError);
      return response.projectDto;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to get project data");
    }
  },
  async getUserProjectsInWorkspace(workspaceId, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await projectClient.GetProjectsForUserInWorkspace(sessionData.jwtToken, workspaceId, onError);
      return response.projectSearchDtos;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to get project data");
    }
  },
  async createProject(createProjectRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.CreateProject(sessionData.jwtToken, createProjectRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to create project");
    }
  },
  async modifyProject(createProjectRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.ModifyProject(sessionData.jwtToken, createProjectRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to modify project");
    }
  },
  async removeProject(projectId, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.RemoveProject(sessionData.jwtToken, projectId, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to remove project");
    }
  },
  async createUserAssignmentToProject(userAssignmentRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.CreateUserAssignmentToProject(sessionData.jwtToken, userAssignmentRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to create user assignment to project");
    }
  },
  async removeUserAssingnmentFromProject(userAssignmentRemoveRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.RemoveUserAssingmentFromProject(sessionData.jwtToken, userAssignmentRemoveRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to remove user assignment to project");
    }
  },
  async modifyUserAssingment(modifyUserAssignmentRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectClient.ModifyUserAssingmentInProject(sessionData.jwtToken, modifyUserAssignmentRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to modify user assignment to project");
    }
  },
}

export default ProjectService;