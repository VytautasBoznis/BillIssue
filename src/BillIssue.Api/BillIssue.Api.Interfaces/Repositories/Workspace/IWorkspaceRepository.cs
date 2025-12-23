using BillIssue.Api.Interfaces.Base;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;

namespace BillIssue.Api.Interfaces.Repositories.Workspace
{
    public interface IWorkspaceRepository
    {
        Task<WorkspaceDto> GetWorkspaceDataWithPermissionCheck(Guid userId, Guid WorkspaceId, WorkspaceUserRole minimalRole, IUnitOfWork unitOfWork, bool isAdmin = false);
        Task<List<WorkspaceUserDto>> GetAllWorkspaceUsers(Guid wokspaceId, IUnitOfWork unitOfWork);
        Task<List<WorkspaceSearchDto>> GetAllWorkspaceDataForUser(Guid userId, IUnitOfWork unitOfWork);
        Task<List<WorkspaceSelectionDto>> GetAllWorkspaceSelectionsForUser(Guid userId, IUnitOfWork unitOfWork);
        Task<Guid> CreateWorkspaceInTransaction(Guid userId, string userEmail, WorkspaceDto Workspace, IUnitOfWork unitOfWork);
        Task ModifyWorkspaceInTransaction(Guid userId, string userEmail, WorkspaceDto WorkspaceDto, IUnitOfWork unitOfWork);
        Task MarkWorkspaceAsDeleted(Guid userId, string userEmail, Guid WorkspaceId, IUnitOfWork unitOfWork);
        Task CreateWorkspaceUserAssignmentInTransaction(Guid userId, string userEmail, WorkspaceUserAssignmentDto workspaceAssignmentDto, IUnitOfWork unitOfWork);
    }
}
