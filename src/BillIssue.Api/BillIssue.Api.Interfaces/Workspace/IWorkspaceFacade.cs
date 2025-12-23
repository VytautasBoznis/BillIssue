using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using Npgsql;
using System.Data;

namespace BillIssue.Api.Interfaces.Workspace
{
    public interface IWorkspaceFacade
    {
        #region Company Workspace management
        public Task<List<WorkspaceSelectionDto>> GetAllWorkspaceSelectionsForUser(string sessionId, GetWorkspaceSelectionsForUserRequest request);
        public Task<List<WorkspaceSearchDto>> GetAllWorkspacesForUser(string sessionId, GetAllWorkspacesForUserRequest request);
        public Task<WorkspaceDto> GetWorkspace(string sessionId, GetWorkspaceRequest request);
        public Task<WorkspaceDto> CreateWorkspace(string sessionId, CreateWorkspaceRequest request);
        public Task<WorkspaceDto> UpdateWorkspace(string sessionId, ModifyWorkspaceRequest request);
        public Task CreatePersonalWorkspaceForNewUser(Guid newUserId, string firstName, string email, NpgsqlTransaction transaction);
        public Task RemoveWorkspace(string sessionId, RemoveWorkspaceRequest request);
        #endregion

        #region Company Workspace user management
        public Task<List<WorkspaceUserDto>> GetAllWorkspaceUsers(string sessionId, GetAllWorkspaceUsersRequest request);
        public Task AddUserToWorkspace(string sessionId, AddUserToWorkspaceRequest request);
        public Task UpdateUserInWorkspace(string sessionId, UpdateUserInWorkspaceRequest request);
        public Task RemoveUserFromWorkspace(string sessionId, RemoveUserFromWorkspaceRequest request);
        #endregion
    }
}
