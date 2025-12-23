using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Workspace;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Workspace
{
    public class GetWorkspaceOperation : BaseOperation<GetWorkspaceRequest, GetWorkspaceResponse>
    {
        private readonly IWorkspaceRepository _workspaceRepository;

        public GetWorkspaceOperation(
            ILogger<GetWorkspaceOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetWorkspaceRequest> validator,
            IWorkspaceRepository workspaceRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _workspaceRepository = workspaceRepository;
        }

        protected override async Task<GetWorkspaceResponse> Execute(GetWorkspaceRequest request, IUnitOfWork unitOfWork)
        {
            WorkspaceDto workspaceDto = await _workspaceRepository.GetWorkspaceDataWithPermissionCheck(request.SessionUserData.Id, request.WorkspaceId, WorkspaceUserRole.Manager, unitOfWork, request.SessionUserData.Role == UserRole.Admin);

            if (workspaceDto == null)
            {
                _logger.LogError($"User with user id: {request.SessionUserData.Id} tried to access an unknow workspace with id: {request.WorkspaceId}.");
                throw new WorkspaceException("Workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            if (request.LoadWorkspaceUsers)
            {
                workspaceDto.WorkspaceUsers = await _workspaceRepository.GetAllWorkspaceUsers(workspaceDto.Id, unitOfWork);
            }

            return new GetWorkspaceResponse
            {
                WorkspaceDto = workspaceDto
            };
        }
    }
}
