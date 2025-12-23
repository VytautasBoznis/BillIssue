using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Project;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Project
{
    public class CreateProjectOperation : BaseOperation<CreateProjectRequest, CreateProjectResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public CreateProjectOperation(
            ILogger<CreateProjectOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<CreateProjectRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<CreateProjectResponse> Execute(CreateProjectRequest request, IUnitOfWork unitOfWork)
        {
            await unitOfWork.BeginTransactionAsync();

            ProjectDto projectDto = new ProjectDto { WorkspaceId = request.WorkspaceId, Name = request.Name, Description = request.Description };

            Guid newProjectId = await _projectRepository.CreateProjectInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, projectDto, unitOfWork);
            ProjectUserAssignmentDto projectUserAssignmentDto = new ProjectUserAssignmentDto { ProjectId = newProjectId, UserId = request.SessionUserData.Id, Role = ProjectUserRoles.Owner };

            await _projectRepository.CreateUserAssignmentInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, projectUserAssignmentDto, unitOfWork);
            await _projectRepository.CreateProjectDefaultWorktypesInTransaction(newProjectId, request.SessionUserData.Email, unitOfWork);

            if (request.CompleteTransactions)
            {
                await unitOfWork.CommitAsync();
            }

            GetProjectResponse getProjectResponse = await _operationFactory
                                                                .Get<GetProjectOperation>(typeof(GetProjectOperation))
                                                                .Run(new GetProjectRequest
                                                                {
                                                                    SessionUserData = request.SessionUserData,
                                                                    ProjectId = newProjectId,
                                                                }, unitOfWork);

            return new CreateProjectResponse
            {
                ProjectDto = getProjectResponse.ProjectDto
            };
        }
    }
}
