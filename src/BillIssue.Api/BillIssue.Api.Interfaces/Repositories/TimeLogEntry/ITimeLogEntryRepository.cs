using BillIssue.Api.Interfaces.Base;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;

namespace BillIssue.Api.Interfaces.Repositories.TimeLogEntry
{
    public interface ITimeLogEntryRepository
    {
        Task<TimeLogEntryDto> GetTimeLogEntryWithPermissionCheck(
            Guid userId,
            Guid timeLogEntryId,
            IUnitOfWork unitOfWork,
            WorkspaceUserRole minimalWorkspaceRoleToAccess = WorkspaceUserRole.Manager, 
            ProjectUserRoles minimalProjectRoleToAccess = ProjectUserRoles.TeamLead, 
            bool isAdmin = false);

        Task<TimeLogEntryRoleContextDto> GetUserRoleInTimeLogEntryContext(Guid userId, Guid timeLogEntryId, IUnitOfWork unitOfWork);

        Task<Guid> CreateTimeLogEntryInTransaction(Guid userId, string userEmail, TimeLogEntryDto timeLogEntryDto, IUnitOfWork unitOfWork);

        Task ModifyTimeLogEntryInTransaction(Guid userId, string userEmail, TimeLogEntryDto timeLogEntryDto, IUnitOfWork unitOfWork);

        Task MarkTimeLogEntryAsDeleted(Guid userId, string email, TimeLogEntryDto timeLogEntryDto, IUnitOfWork unitOfWork);

        Task<TimeLogEntryRoleContextForSearchDto> CheckIfUserHasAccess(Guid userId, Guid workspaceId, Guid? projectId, IUnitOfWork unitOfWork);
        Task<List<TimeLogEntryDto>> GetSearchResultsForQuery(SearchTimeLogEntriesRequest request, IUnitOfWork unitOfWork);
        Task<List<TimeLogEntryDto>> GetTimelogEntriesInDateRangeForUser(Guid userId, Guid WorkspaceId, DateTime startTime, DateTime endTime, IUnitOfWork unitOfWork);
    }
}
