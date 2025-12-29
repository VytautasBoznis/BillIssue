using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.TimeLogEntry;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BillIssue.Api.Business.Operations.TimeLogEntry
{
    public class SearchTimeLogEntriesOperation : BaseOperation<SearchTimeLogEntriesRequest, SearchTimeLogEntriesResponse>
    {
        private readonly ITimeLogEntryRepository _timeLogEntryRepository;

        public SearchTimeLogEntriesOperation(
            ILogger<SearchTimeLogEntriesOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory,
            IValidator<SearchTimeLogEntriesRequest> validator,
            ITimeLogEntryRepository timeLogEntryRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _timeLogEntryRepository = timeLogEntryRepository;
        }

        protected override async Task<SearchTimeLogEntriesResponse> Execute(SearchTimeLogEntriesRequest request, IUnitOfWork unitOfWork)
        {
            TimeLogEntryRoleContextForSearchDto timeLogEntryRoleContextForSearchDto = await _timeLogEntryRepository.CheckIfUserHasAccess(request.SessionUserData.Id, request.WorkspaceId, request.ProjectId, unitOfWork);

            if (CheckIfUserEligibleForSearch(timeLogEntryRoleContextForSearchDto, request.SessionUserData.Id, request.SessionUserData.Role, request.UserId, request.ProjectId != null))
            {
                _logger.LogWarning($"Search failed for user because the user did not have enaugh permission to search for those time log entries user id: {request.SessionUserData.Id}, searchRequest: {JsonConvert.SerializeObject(request)}");
                return new SearchTimeLogEntriesResponse() { TimeLogEntryDtos = [] };
            }

            List<TimeLogEntryDto> searchResults = await _timeLogEntryRepository.GetSearchResultsForQuery(request, unitOfWork);

            return new SearchTimeLogEntriesResponse
            {
                TimeLogEntryDtos = searchResults
            };
        }

        private bool CheckIfUserEligibleForSearch(TimeLogEntryRoleContextForSearchDto timeLogEntryRoleContextForSearchDto, Guid userId, UserRole userRole, Guid? targetUserId, bool searchingInProject)
        {
            if (targetUserId != null && targetUserId == userId) // if searching for your entries allow any search
            {
                return true;
            }

            if (userRole == UserRole.Admin) // if admin is searching allow any search
            {
                return true;
            }

            if (searchingInProject && timeLogEntryRoleContextForSearchDto.ProjectUserRole >= Shared.Models.Enums.Project.ProjectUserRoles.TeamLead)
            {
                return true;
            }

            if (!searchingInProject && timeLogEntryRoleContextForSearchDto.WorkspaceUserRole >= Shared.Models.Enums.Workspace.WorkspaceUserRole.Manager)
            {
                return true;
            }

            return false;
        }
    }
}
