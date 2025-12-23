using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Workspace;
using BillIssue.Api.Interfaces.Workspace;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Workspace
{
    public class GetAllWorkspacesForUserOperation : BaseOperation<GetAllWorkspacesForUserRequest, GetAllWorkspacesForUsersResponse>
    {
        private readonly IWorkspaceRepository _workspaceRepository;

        public GetAllWorkspacesForUserOperation(
            ILogger<GetAllWorkspacesForUserOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetAllWorkspacesForUserRequest> validator,
            IWorkspaceRepository workspaceRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _workspaceRepository = workspaceRepository;
        }

        protected override async Task<GetAllWorkspacesForUsersResponse> Execute(GetAllWorkspacesForUserRequest request, IUnitOfWork unitOfWork)
        {
            Guid targetUserId = request.UserId;

            if (request.SessionUserData.Id != targetUserId && request.SessionUserData.Role != UserRole.Admin)
            {
                _logger.LogError($"User with user id: {request.SessionUserData.Id} tried to access all workspaces for user with id: {request.UserId}.");
                throw new WorkspaceException("Workspaces not found", ExceptionCodes.WORKSPACES_NOT_FOUND);
            }

            List<WorkspaceSearchDto> allUserWorkspaces = await _workspaceRepository.GetAllWorkspaceDataForUser(targetUserId, unitOfWork);;

            GetAllWorkspacesForUsersResponse response = new GetAllWorkspacesForUsersResponse
            {
                WorkspaceDtos = allUserWorkspaces ?? new List<WorkspaceSearchDto>()
            };

            return response;
        }
    }
}
