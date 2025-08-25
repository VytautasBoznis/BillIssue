import { getUserSessionData } from "../utils/sessionUtils";
import { projectWorktypeClient } from "./clients/HttpClients";

const ProjectWorktypeService = {
  async getProjectById(projectWorktypeId, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await projectWorktypeClient.GetProjectWorktypeById(sessionData.authToken, projectWorktypeId, onError);
      return response.projectWorktypeDto;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to get project worktype data");
    }
  },
  async removeProjectWorktype(projectId, projectWorktypeId, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectWorktypeClient.RemoveProjectWorktype(sessionData.authToken, projectId, projectWorktypeId, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to remove worktype project");
    }
  },
  async modifyProjectWorktype(modifyProjectWorktypeRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      await projectWorktypeClient.ModifyProjectWorktype(sessionData.authToken, modifyProjectWorktypeRequest, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to modify project worktype");
    }
  },
}

export default ProjectWorktypeService;