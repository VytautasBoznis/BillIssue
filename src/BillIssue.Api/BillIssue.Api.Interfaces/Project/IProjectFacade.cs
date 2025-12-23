using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using Npgsql;

namespace BillIssue.Api.Interfaces.Project
{
    public interface IProjectFacade
    {
        #region Project management
        public List<ProjectSelectionDto> GetProjectSelectionsForWorkspaces(List<Guid> WorkspaceIds);
        public Task<ProjectDto> CreateProject(string sessionId, CreateProjectRequest request, NpgsqlTransaction transaction = null);
        public Task CreateProjectForNewUser(Guid newUserId, Guid newWorkspaceId, string firstName, string email, NpgsqlTransaction transaction);
        public Task<ProjectDto> GetProject(string sessionId, GetProjectRequest request);
        public Task RemoveProject(string sessionId, RemoveProjectRequest request);
        public Task ModifyProject(string sessionId, ModifyProjectRequest request);
        public Task<List<ProjectSearchDto>> GetUserProjectsInWorkspace(string sessionId, GetProjectsInWorkspaceForUserRequest request);
        #endregion

        #region Project worktype management
        public Task CreateProjectWorktype(string sessionId, CreateProjectWorktypeRequest request);
        public Task<ProjectWorktypeDto> GetProjectWorktype(string sessionId, GetProjectWorktypeRequest request);
        public Task<List<ProjectWorktypeDto>> GetAllProjectWorktypes(string sessionId, GetAllProjectWorktypesRequest request);
        public Task RemoveProjectWorktype(string sessionId, RemoveProjectWorktypeRequest request);
        public Task ModifyProjectWorktype(string sessionId, ModifyProjectWorktypeRequest request);
        #endregion

        #region Project User management
        public Task<List<ProjectUserAssignmentDto>> GetProjectUsers(string sessionId, GetProjectUsersRequest request);
        public Task<List<ProjectDto>> GetProjectsForUser(string sessionId);
        public Task AddUserAssignmentToProject(string sessionId, AddUserAssignmentToProjectRequest request);
        public Task ModifyUserAssingmentInProject(string sessionId, ModifyUserAssingmentInProjectRequest request);
        public Task RemoveUserAssingmentFromProject(string sessionId, RemoveUserAssingmentFromProjectRequest request);
        #endregion 
    }
}
