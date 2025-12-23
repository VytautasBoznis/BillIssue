using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Workspace;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Workspace
{
    public class GetAllWorkspaceUsersOperation : BaseOperation<GetAllWorkspaceUsersRequest, GetAllWorkspaceUsersResponse>
    {
        public readonly IWorkspaceRepository _workspaceRepository;

        public GetAllWorkspaceUsersOperation(
            ILogger<GetAllWorkspacesForUserOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<GetAllWorkspaceUsersRequest> validator,
            IWorkspaceRepository workspaceRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _workspaceRepository = workspaceRepository;
        }

        protected override async Task<GetAllWorkspaceUsersResponse> Execute(GetAllWorkspaceUsersRequest request, IUnitOfWork unitOfWork)
        {
            GetWorkspaceResponse workspaceResponse = await _operationFactory
                                                        .Get<GetWorkspaceOperation>(typeof(GetWorkspaceOperation))
                                                        .Run(new GetWorkspaceRequest
                                                        {
                                                            SessionUserData = request.SessionUserData,
                                                            WorkspaceId = request.WorkspaceId
                                                        }, unitOfWork);

            if (workspaceResponse.WorkspaceDto == null)
            {
                _logger.LogError($"User with user id: {request.SessionUserData.Id} tried to access an unknow workspace with id: {request.WorkspaceId}.");
                throw new WorkspaceException("Workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            List<WorkspaceUserDto> allWorkspaceUsers = await _workspaceRepository.GetAllWorkspaceUsers(request.WorkspaceId, unitOfWork);

            return new GetAllWorkspaceUsersResponse
            {
                WorkspaceUserDtos = allWorkspaceUsers ?? new List<WorkspaceUserDto>()
            };
        }
    }
}
