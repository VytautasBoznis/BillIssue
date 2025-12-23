using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Workspace;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Workspace;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Workspace
{
    public class RemoveWorkspaceOperation : BaseOperation<RemoveWorkspaceRequest, RemoveWorkspaceResponse>
    {
        private readonly IWorkspaceRepository _workspaceRepository;

        public RemoveWorkspaceOperation(
            ILogger<RemoveWorkspaceOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<RemoveWorkspaceRequest> validator,
            IWorkspaceRepository workspaceRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _workspaceRepository = workspaceRepository;
        }

        protected override async Task<RemoveWorkspaceResponse> Execute(RemoveWorkspaceRequest request, IUnitOfWork unitOfWork)
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

            await _workspaceRepository.MarkWorkspaceAsDeleted(request.SessionUserData.Id, request.SessionUserData.Email, request.WorkspaceId, unitOfWork);

            await unitOfWork.CommitAsync();

            return new RemoveWorkspaceResponse();
        }
    }
}
