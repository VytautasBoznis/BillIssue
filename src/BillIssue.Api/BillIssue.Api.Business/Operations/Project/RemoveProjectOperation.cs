using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Project;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Project
{
    public class RemoveProjectOperation : BaseOperation<RemoveProjectRequest, RemoveProjectResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public RemoveProjectOperation(
            ILogger<RemoveProjectOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<RemoveProjectRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<RemoveProjectResponse> Execute(RemoveProjectRequest request, IUnitOfWork unitOfWork)
        {
            GetProjectResponse getProjectResponse = await _operationFactory
                                                                .Get<GetProjectOperation>()
                                                                .Run(new GetProjectRequest
                                                                {
                                                                    SessionUserData = request.SessionUserData,
                                                                    CreatedFromController = false,
                                                                    ProjectId = request.ProjectId,
                                                                }, unitOfWork);

            if (getProjectResponse.ProjectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {request.SessionUserData.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            await unitOfWork.BeginTransactionAsync();

            await _projectRepository.MarkProjectAsDeletedInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, request.ProjectId, unitOfWork);

            await unitOfWork.CommitAsync();

            return new RemoveProjectResponse();
        }
    }
}
