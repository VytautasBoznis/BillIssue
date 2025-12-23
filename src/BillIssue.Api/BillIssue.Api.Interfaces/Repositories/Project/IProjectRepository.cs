using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Models.Projects;

namespace BillIssue.Api.Interfaces.Repositories.Project
{
    public interface IProjectRepository
    {
        Task<List<ProjectAndWorktypeModelFlattened>> GetAllProjectSelectionsForWorkspacesFlattened(List<Guid> WorkspaceIds, IUnitOfWork unitOfWork);
    }
}
