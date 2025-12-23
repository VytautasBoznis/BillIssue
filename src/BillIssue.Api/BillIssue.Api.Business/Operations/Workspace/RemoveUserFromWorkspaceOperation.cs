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
    public class RemoveUserFromWorkspaceOperation : BaseOperation<RemoveUserFromWorkspaceRequest, RemoveUserFromWorkspaceResponse>
    {
        private readonly IWorkspaceRepository _workspaceRepository;

        public RemoveUserFromWorkspaceOperation(
            ILogger<RemoveUserFromWorkspaceOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<RemoveUserFromWorkspaceRequest> validator,
            IWorkspaceRepository workspaceRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _workspaceRepository = workspaceRepository;
        }

        protected override async Task<RemoveUserFromWorkspaceResponse> Execute(RemoveUserFromWorkspaceRequest request, IUnitOfWork unitOfWork)
        {
            GetWorkspaceResponse workspaceResponse = await _operationFactory
                                                                .Get<GetWorkspaceOperation>(typeof(GetWorkspaceOperation))
                                                                .Run(new GetWorkspaceRequest
                                                                {
                                                                    SessionUserData = request.SessionUserData,
                                                                    WorkspaceId = request.WorkspaceId
                                                                }, unitOfWork);

            List<WorkspaceUserDto> allWorkspaceUsers = await _workspaceRepository.GetAllWorkspaceUsers(workspaceResponse.WorkspaceDto.Id, unitOfWork);

            WorkspaceUserDto requestorUserAssignment = allWorkspaceUsers.FirstOrDefault(ua => ua.UserId == request.SessionUserData.Id);
            WorkspaceUserDto targetUserAssignment = allWorkspaceUsers.FirstOrDefault(ua => ua.UserId == request.UserId);

            if (targetUserAssignment == null)
            {
                _logger.LogError($"User with user id: {request.SessionUserData.Id} tried to remove a user assignment for workspace {request.WorkspaceId} with user id: {request.UserId} that was not found");
                throw new WorkspaceException("User assignment not found", ExceptionCodes.WORKSPACE_USER_ASSIGNMENT_NOT_FOUND);
            }

            await unitOfWork.BeginTransactionAsync();

            await _workspaceRepository.RemoveWorkspaceUserAssignmentInTracsaction(request.SessionUserData.Id, workspaceResponse.WorkspaceDto.Id, targetUserAssignment.UserId, unitOfWork);

            await unitOfWork.CommitAsync();

            return new RemoveUserFromWorkspaceResponse();
        }
    }
}
