using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Project;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Workspace;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using BillIssue.Shared.Models.Response.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Workspace
{
    public class GetWorkspaceSelectionsForUserOperation : BaseOperation<GetWorkspaceSelectionsForUserRequest, GetWorkspaceSelectionsForUserResponse>
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        public GetWorkspaceSelectionsForUserOperation(
            ILogger<GetWorkspaceSelectionsForUserOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetWorkspaceSelectionsForUserRequest> validator,
            IWorkspaceRepository workspaceReposity) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _workspaceRepository = workspaceReposity;
        }

        protected override async Task<GetWorkspaceSelectionsForUserResponse> Execute(GetWorkspaceSelectionsForUserRequest request, IUnitOfWork unitOfWork)
        {
            Guid targetUserId = request.UserId;

            if (request.SessionUserData.Id != targetUserId && request.SessionUserData.Role != UserRole.Admin)
            {
                _logger.LogError($"User with user id: {request.SessionUserData.Id} tried to access workspace selections for user with id: {request.UserId}.");
                throw new WorkspaceException("Workspaces not found", ExceptionCodes.WORKSPACES_NOT_FOUND);
            }

            List<WorkspaceSelectionDto> allUserWorkspaceSelections = await _workspaceRepository.GetAllWorkspaceSelectionsForUser(targetUserId, unitOfWork);

            GetWorkspaceSelectionsForUserResponse response = new GetWorkspaceSelectionsForUserResponse
            {
                WorkspaceSelections = allUserWorkspaceSelections
            };

            if (allUserWorkspaceSelections.Count == 0)
            {
                return response;
            }

            GetProjectSelectionsForWorkspacesRequest getProjectSelectionsForWorkspacesRequest = new GetProjectSelectionsForWorkspacesRequest
            {
                SessionUserData = request.SessionUserData,
                CreatedFromController = false,
                WorkspaceIds = allUserWorkspaceSelections.Select(aucws => aucws.Id).Distinct().ToList()
            };

            GetProjectSelectionsForWorkspacesResponse projectSelectionsForWorkspacesResponse =  await _operationFactory
                                                                                                        .Get<GetProjectSelectionsForWorkspacesOperation>()
                                                                                                        .Run(getProjectSelectionsForWorkspacesRequest, unitOfWork);

            if (projectSelectionsForWorkspacesResponse.ProjectSelectionDtos.Count == 0)
            {
                return response;
            }

            foreach (WorkspaceSelectionDto companySelection in allUserWorkspaceSelections)
            {
                List<ProjectSelectionDto> companyProjectSelections = projectSelectionsForWorkspacesResponse.ProjectSelectionDtos.FindAll(pwt => pwt.WorkspaceId == companySelection.Id);
                companySelection.Projects = companyProjectSelections;
            }

            response.WorkspaceSelections = allUserWorkspaceSelections;

            return response;
        }
    }
}
