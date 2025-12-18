using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
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
        public GetProjectOperation(
            ILogger<GetProjectOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetProjectRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<GetProjectResponse> Execute(GetProjectRequest request, IUnitOfWork unitOfWork)
        {
            ProjectDto projectDto = await GetProjectDataWithPermissionCheck(request.SessionUserData.Id, request.ProjectId, ProjectUserRoles.Contributor, request.SessionUserData.Role == UserRole.Admin, unitOfWork);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {request.SessionUserData.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            projectDto.ProjectWorktypes = await GetProjectWorktypeDtos(request.ProjectId, unitOfWork);

            if (request.LoadUserAssignments)
            {
                projectDto.ProjectUserAssignments = await GetProjectUserAssignmentDtos(request.ProjectId, unitOfWork);
            }

            return new GetProjectResponse
            {
                ProjectDto = projectDto
            };
        }

        private async Task<ProjectDto> GetProjectDataWithPermissionCheck(Guid userId, Guid projectId, ProjectUserRoles minimumRole, bool isUserAdmin, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@id", projectId } };

            IEnumerable<ProjectDto> projectDtos = await unitOfWork.Connection.QueryAsync<ProjectDto>("SELECT id as ProjectId, workspace_id as WorkspaceId, name, is_deleted as IsDeleted, description FROM project_projects WHERE id = @id", dictionary);
            ProjectDto? projectDto = projectDtos.FirstOrDefault();

            if (projectDto == null)
            {
                _logger.LogError($"User with user id: {userId} tried to get an unknow project with id: {projectId}.");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            if (projectDto.IsDeleted)
            {
                _logger.LogError($"User with user id: {userId} tried to get an a project that is deleted project with id: {projectId}.");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            dictionary.Add("@pua", userId);
            dictionary.Add("@pur", (int)minimumRole);

            IEnumerable<ProjectUserAssignmentDto> projectUserAssignmentDtos = await unitOfWork.Connection.QueryAsync<ProjectUserAssignmentDto>(@"
                SELECT pua.id as UserAssignmentId, pua.project_id as ProjectId, pua.user_id as UserId, pua.project_role as Role
                FROM project_user_assignments pua
                WHERE pua.user_id = @pua
                    AND pua.project_id = @id
                    AND pua.project_role >= @pur", dictionary);

            ProjectUserAssignmentDto? projectUserAssignment = projectUserAssignmentDtos.FirstOrDefault();

            if (projectUserAssignment == null && !isUserAdmin)
            {
                _logger.LogError($"User with user id: {userId} tried to access project with id: {projectId} that he did not have access to.");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            return projectDto;
        }

        private async Task<List<ProjectWorktypeDto>> GetProjectWorktypeDtos(Guid projectId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@pid", projectId } };
            IEnumerable<ProjectWorktypeDto> projectWorktypes = await unitOfWork.Connection.QueryAsync<ProjectWorktypeDto>(@"
                SELECT pp.id as ProjectId, pw.id as ProjectWorktypeId, pw.name, pw.description, pw.is_billable as IsBillable, pw.is_deleted as IsDeleted
                FROM project_projects pp
	                JOIN project_worktypes pw
		                ON pp.id = pw.project_id
                WHERE pp.id = @pid", dictionary);

            return projectWorktypes.ToList();
        }

        private async Task<List<ProjectUserAssignmentDto>> GetProjectUserAssignmentDtos(Guid projectId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@pid", projectId } };
            IEnumerable<ProjectUserAssignmentDto> projectUserAssignments = await unitOfWork.Connection.QueryAsync<ProjectUserAssignmentDto>(@"
                SELECT pua.id as UserAssignmentId, pua.project_id as ProjectId, uu.id as UserId, uu.email, uu.first_name, uu.last_name, pua.project_role as Role
                FROM Project_projects pp
                    JOIN Project_user_assignments pua
                        ON pp.id = pua.project_id
                    JOIN User_users uu
                        ON pua.user_id = uu.id
                WHERE pp.id = @pid", dictionary);

            return projectUserAssignments.ToList();
        }
    }
}
