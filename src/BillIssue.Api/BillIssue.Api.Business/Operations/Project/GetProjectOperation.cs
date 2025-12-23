using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Project;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Project
{
    public class GetProjectOperation : BaseOperation<GetProjectRequest, GetProjectResponse>
    {
        private IProjectRepository _projectRepository;

        public GetProjectOperation(
            ILogger<GetProjectOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetProjectRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<GetProjectResponse> Execute(GetProjectRequest request, IUnitOfWork unitOfWork)
        {
            ProjectDto projectDto = await _projectRepository.GetProjectDataWithPermissionCheck(request.SessionUserData.Id, request.ProjectId, ProjectUserRoles.Contributor, request.SessionUserData.Role == UserRole.Admin, unitOfWork);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {request.SessionUserData.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            projectDto.ProjectWorktypes = await _projectRepository.GetProjectWorktypeDtos(request.ProjectId, unitOfWork);

            if (request.LoadUserAssignments)
            {
                projectDto.ProjectUserAssignments = await _projectRepository.GetProjectUserAssignmentDtos(request.ProjectId, unitOfWork);
            }

            return new GetProjectResponse
            {
                ProjectDto = projectDto
            };
        }
    }
}
