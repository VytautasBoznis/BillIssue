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
    public class ModifyProjectOperation : BaseOperation<ModifyProjectRequest, ModifyProjectResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public ModifyProjectOperation(
            ILogger<ModifyProjectOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<ModifyProjectRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<ModifyProjectResponse> Execute(ModifyProjectRequest request, IUnitOfWork unitOfWork)
        {
            GetProjectResponse getProjectResponse = await _operationFactory
                                                                .Get<GetProjectOperation>(typeof(GetProjectOperation))
                                                                .Run(new GetProjectRequest
                                                                {
                                                                    SessionUserData = request.SessionUserData,
                                                                    ProjectId = request.ProjectId,
                                                                }, unitOfWork);

            if (getProjectResponse.ProjectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {request.SessionUserData.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            await unitOfWork.BeginTransactionAsync();

            getProjectResponse.ProjectDto.Name = request.Name;
            getProjectResponse.ProjectDto.Description = request.Description;

            await _projectRepository.ModifyProjectInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, getProjectResponse.ProjectDto, unitOfWork);

            await unitOfWork.CommitAsync();

            return new ModifyProjectResponse();
        }
    }
}
