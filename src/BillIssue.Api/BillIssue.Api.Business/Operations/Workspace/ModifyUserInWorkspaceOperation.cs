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
    public class ModifyUserInWorkspaceOperation : BaseOperation<ModifyUserInWorkspaceRequest, ModifyUserInWorkspaceResponse>
    {
        private readonly IWorkspaceRepository _workspaceRepository;

        public ModifyUserInWorkspaceOperation(
            ILogger<ModifyUserInWorkspaceOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<ModifyUserInWorkspaceRequest> validator,
            IWorkspaceRepository workspaceRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _workspaceRepository = workspaceRepository;
        }

        protected override async Task<ModifyUserInWorkspaceResponse> Execute(ModifyUserInWorkspaceRequest request, IUnitOfWork unitOfWork)
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

            await unitOfWork.BeginTransactionAsync();

            WorkspaceUserAssignmentDto workspaceUserAssignmentDto = new WorkspaceUserAssignmentDto { WorkspaceId = request.WorkspaceId, WorkspaceRole = request.NewUserRole, UserId = request.UserId };
            await _workspaceRepository.UpdateWorkspaceUserAssignmentDataInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, workspaceUserAssignmentDto, unitOfWork);

            await unitOfWork.CommitAsync();

            return new ModifyUserInWorkspaceResponse();
        }
    }
}
