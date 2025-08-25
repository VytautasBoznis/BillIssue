import { PROJECT_USER_ROLES } from "../services/constants/ProjectConstants";
import { WORKSPACE_USER_ROLES } from "../services/constants/WorkspaceConstants";

export const resolveUserWorkspaceRoleName = (role) => {
  const targetRole = WORKSPACE_USER_ROLES.find(r => r.id === role);

  if (targetRole) {
    return targetRole.name;
  }
  
  return '';
}

export const resolveUserProjectRoleName = (role) => {
  const targetRole = PROJECT_USER_ROLES.find(r => r.id === role);

  if (targetRole) {
    return targetRole.name;
  }
  
  return '';
}