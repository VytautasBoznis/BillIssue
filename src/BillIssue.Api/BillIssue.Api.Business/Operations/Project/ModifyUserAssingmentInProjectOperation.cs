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
    public class ModifyUserAssingmentInProjectOperation : BaseOperation<ModifyUserAssingmentInProjectRequest, ModifyUserAssingmentInProjectResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public ModifyUserAssingmentInProjectOperation(
            ILogger<ModifyUserAssingmentInProjectOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<ModifyUserAssingmentInProjectRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<ModifyUserAssingmentInProjectResponse> Execute(ModifyUserAssingmentInProjectRequest request, IUnitOfWork unitOfWork)
        {
            ProjectDto projectDto = await _projectRepository.GetProjectDataWithPermissionCheck(request.SessionUserData.Id, request.ProjectId, ProjectUserRoles.TeamLead, request.SessionUserData.Role == UserRole.Admin, unitOfWork);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {request.SessionUserData.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            await unitOfWork.BeginTransactionAsync();

            ProjectUserAssignmentDto newProjectAssignmentValues = new ProjectUserAssignmentDto { ProjectId = request.ProjectId, UserAssignmentId = request.ProjectUserAssignmentId, Role = request.Role };
            await _projectRepository.ModifyUserAssignmentInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, newProjectAssignmentValues, unitOfWork);

            await unitOfWork.CommitAsync();

            return new ModifyUserAssingmentInProjectResponse();
        }
    }
}
