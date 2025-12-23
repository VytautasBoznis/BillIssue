using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Project;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Workspace;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Workspace
{
    public class CreateWorkspaceOperation : BaseOperation<CreateWorkspaceRequest, CreateWorkspaceResponse>
    {
        private readonly IWorkspaceRepository _workspaceRepository;

        public CreateWorkspaceOperation(
            ILogger<CreateWorkspaceOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<CreateWorkspaceRequest> validator,
            IWorkspaceRepository workspaceRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _workspaceRepository = workspaceRepository;
        }

        protected override async Task<CreateWorkspaceResponse> Execute(CreateWorkspaceRequest request, IUnitOfWork unitOfWork)
        {
            await unitOfWork.BeginTransactionAsync();

            WorkspaceDto workspaceDto = new WorkspaceDto { Name = request.Name, Description = request.Description };

            Guid newWorkspaceId = await _workspaceRepository.CreateWorkspaceInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, workspaceDto, unitOfWork);

            WorkspaceUserAssignmentDto WorkspaceUserAssignmentDto = new()
            { 
                WorkspaceId = newWorkspaceId,
                UserId = request.SessionUserData.Id,
                WorkspaceRole = WorkspaceUserRole.Owner 
            };

            await _workspaceRepository.CreateWorkspaceUserAssignmentInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, WorkspaceUserAssignmentDto, unitOfWork);

            CreateProjectRequest createProjectRequest = new()
            {
                SessionUserData = request.SessionUserData,
                WorkspaceId = newWorkspaceId,
                Name = request.Name,
                Description = request.Description,
                CompleteTransactions = false
            };
            
            await _operationFactory
                    .Get<CreateProjectOperation>(typeof(CreateProjectOperation))
                    .Run(createProjectRequest, unitOfWork);

            GetWorkspaceRequest getWorkspaceRequest = new()
            {
                SessionUserData = request.SessionUserData,
                WorkspaceId = newWorkspaceId
            };

            GetWorkspaceResponse getWorkspaceResponse = await _operationFactory
                                                                .Get<GetWorkspaceOperation>(typeof(GetWorkspaceOperation))
                                                                .Run(getWorkspaceRequest, unitOfWork);

            return new CreateWorkspaceResponse
            {
                WorkspaceDto = getWorkspaceResponse.WorkspaceDto
            };
        }
    }
}
