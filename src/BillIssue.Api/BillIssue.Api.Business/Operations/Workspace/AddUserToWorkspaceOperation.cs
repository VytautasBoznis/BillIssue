using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Notifications;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Workspace;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Notifications;
using BillIssue.Shared.Models.Response.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Operations.Workspace
{
    public class AddUserToWorkspaceOperation : BaseOperation<AddUserToWorkspaceRequest, AddUserToWorkspaceResponse>
    {
        private readonly IWorkspaceRepository _workspaceRepository;

        public AddUserToWorkspaceOperation(
            ILogger<AddUserToWorkspaceOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<AddUserToWorkspaceRequest> validator,
            IWorkspaceRepository workspaceRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _workspaceRepository = workspaceRepository;
        }

        protected override async Task<AddUserToWorkspaceResponse> Execute(AddUserToWorkspaceRequest request, IUnitOfWork unitOfWork)
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

            GetAllWorkspaceUsersResponse workspaceUsersResponse = await _operationFactory
                                                                        .Get<GetAllWorkspaceUsersOperation>(typeof(GetAllWorkspaceUsersOperation))
                                                                        .Run(new GetAllWorkspaceUsersRequest
                                                                        {
                                                                            SessionUserData = request.SessionUserData,
                                                                            WorkspaceId = request.WorkspaceId
                                                                        }, unitOfWork);

            if (workspaceUsersResponse.WorkspaceUserDtos.Any(user => string.Equals(user.Email, request.NewUserEmail, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning($"User with id {request.SessionUserData.Id} tried to add a new user to workspace id: {request.WorkspaceId} that is already present {request.NewUserEmail} in that workspace");
            }

            await unitOfWork.BeginTransactionAsync();

            CreateWorkspaceNotificationResponse createWorkspaceNotificationResponse = await _operationFactory
                                                                        .Get<CreateWorkspaceNotificationOperation>(typeof(CreateWorkspaceNotificationOperation))
                                                                        .Run(new CreateWorkspaceNotificationRequest
                                                                        {
                                                                            SessionUserData = request.SessionUserData,
                                                                            WorkspaceId = request.WorkspaceId,
                                                                            TargetUserEmail = request.NewUserEmail
                                                                        }, unitOfWork);

            //TODO: send email to user that there is an invite to a new workspace

            await unitOfWork.CommitAsync();

            return new AddUserToWorkspaceResponse();
        }
    }
}
