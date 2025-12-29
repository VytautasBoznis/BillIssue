using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Models.Projects;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using Npgsql;

namespace BillIssue.Api.Interfaces.Repositories.Project
{
    public interface IProjectRepository
    {
        Task<ProjectDto> GetProjectDataWithPermissionCheck(Guid userId, Guid projectId, ProjectUserRoles minimumRole, bool isUserAdmin, IUnitOfWork unitOfWork);
        Task<List<ProjectWorktypeDto>> GetProjectWorktypeDtos(Guid projectId, IUnitOfWork unitOfWork);
        Task<List<ProjectAndWorktypeModelFlattened>> GetAllProjectSelectionsForWorkspacesFlattened(List<Guid> WorkspaceIds, IUnitOfWork unitOfWork);
        Task<Guid> CreateProjectInTransaction(Guid userId, string userEmail, ProjectDto newProjectValues, IUnitOfWork unitOfWork);
        Task ModifyProjectInTransaction(Guid userId, string userEmail, ProjectDto projectNewValues, IUnitOfWork unitOfWork);
        Task CreateProjectDefaultWorktypesInTransaction(Guid newProjectId, string userEmail, IUnitOfWork unitOfWork);
        Task MarkProjectAsDeletedInTransaction(Guid userId, string userEmail, Guid projectId, IUnitOfWork unitOfWork);
        Task<List<ProjectSearchDto>> GetAllProjectsForUserInWorkspace(Guid userId, Guid workspaceId, IUnitOfWork unitOfWork);

        Task CreateUserAssignmentInTransaction(Guid userId, string userEmail, ProjectUserAssignmentDto newUserAssignment, IUnitOfWork unitOfWork);
        Task<List<ProjectUserAssignmentDto>> GetProjectUserAssignmentDtos(Guid projectId, IUnitOfWork unitOfWork);

        Task<ProjectWorktypeDto> GetProjectWorktypeWithPermissionCheck(Guid userId, Guid projectWorktypeId, ProjectUserRoles minimumRole, bool isUserAdmin, IUnitOfWork unitOfWork);
        Task CreateProjectWorktypeInTransaction(Guid userId, string userEmail, ProjectWorktypeDto newProjectWorktype, IUnitOfWork unitOfWork);
        Task ModifyProjectWorktypeInTransaction(Guid userId, string userEmail, ProjectWorktypeDto newProjectWorktypeValues, IUnitOfWork unitOfWork);
        Task MarkProjectWorktypeAsDeletedInTransaction(Guid userId, string userEmail, Guid projectId, Guid projectWorktypeId, IUnitOfWork unitOfWork);
    }
}
