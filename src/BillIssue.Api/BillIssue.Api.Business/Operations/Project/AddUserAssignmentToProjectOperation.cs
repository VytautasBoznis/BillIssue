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
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Project
{
    public class AddUserAssignmentToProjectOperation : BaseOperation<AddUserAssignmentToProjectRequest, AddUserAssignmentToProjectResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public AddUserAssignmentToProjectOperation(
            ILogger<AddUserAssignmentToProjectOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory, 
            IValidator<AddUserAssignmentToProjectRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<AddUserAssignmentToProjectResponse> Execute(AddUserAssignmentToProjectRequest request, IUnitOfWork unitOfWork)
        {
            ProjectDto projectDto = await _projectRepository.GetProjectDataWithPermissionCheck(request.SessionUserData.Id, request.ProjectId, ProjectUserRoles.TeamLead, request.SessionUserData.Role == UserRole.Admin, unitOfWork);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {request.SessionUserData.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            await unitOfWork.BeginTransactionAsync();

            ProjectUserAssignmentDto projectUserAsssignmentDto = new ProjectUserAssignmentDto { ProjectId = request.ProjectId, UserId = request.UserId, Role = request.Role };
            await _projectRepository.CreateUserAssignmentInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, projectUserAsssignmentDto, unitOfWork);

            await unitOfWork.CommitAsync();

            return new AddUserAssignmentToProjectResponse();
        }
    }
}
