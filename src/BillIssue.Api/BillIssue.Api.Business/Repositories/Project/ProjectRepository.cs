using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Project;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Projects;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Transactions;

namespace BillIssue.Api.Business.Repositories.Project
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ILogger _logger;

        private readonly List<ProjectWorktypeDto> _defaultProjectWorktypes = [
            new() { Name = "Billable time", Description = "Billable time", IsBillable = true },
            new() { Name = "Non-billable time", Description = "Non-billable time", IsBillable = false },
        ];

        public ProjectRepository(ILogger<ProjectRepository> logger)
        {
            _logger = logger;
        }

        public async Task<ProjectDto> GetProjectDataWithPermissionCheck(Guid userId, Guid projectId, ProjectUserRoles minimumRole, bool isUserAdmin, IUnitOfWork unitOfWork)
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

        public async Task<List<ProjectWorktypeDto>> GetProjectWorktypeDtos(Guid projectId, IUnitOfWork unitOfWork)
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

        public async Task<List<ProjectAndWorktypeModelFlattened>> GetAllProjectSelectionsForWorkspacesFlattened(List<Guid> WorkspaceIds, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@cwi", WorkspaceIds } };

            IEnumerable<ProjectAndWorktypeModelFlattened> projectAndWorktypeModelsFlattened = await unitOfWork.Connection.QueryAsync<ProjectAndWorktypeModelFlattened>(@"
                SELECT
                    pp.workspace_id as WorkspaceId,
	                pp.id as ProjectId,
	                pp.name as ProjectName,
	                pw.id as WorktypeId,
	                pw.name as WorktypeName
                FROM project_projects pp
	                JOIN project_worktypes pw
		                ON pp.id = pw.project_id
                WHERE pp.workspace_id = ANY (@cwi)", dictionary);

            return projectAndWorktypeModelsFlattened.ToList();
        }

        public async Task<Guid> CreateProjectInTransaction(Guid userId, string userEmail, ProjectDto newProjectValues, IUnitOfWork unitOfWork)
        {
            Guid newProjectId = Guid.NewGuid();

            try
            {
                await using NpgsqlCommand insertProject = new NpgsqlCommand("INSERT INTO project_projects (id, workspace_id, name, description, created_by) VALUES (@id, @cwi, @name, @description, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@id", newProjectId),
                        new("@cwi", newProjectValues.WorkspaceId),
                        new("@name", newProjectValues.Name),
                        new("@description", newProjectValues.Description),
                        new("@createdBy", userEmail),
                    }
                };

                await insertProject.ExecuteNonQueryAsync();

                _logger.LogInformation($"New project created with name: {newProjectValues.Name} in company workspace id {newProjectValues.WorkspaceId} for userId: {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to create a project for user with id: {userId} exception message: {ex.Message} 

                     stacktrace: {ex.StackTrace}
                    """);

                await unitOfWork.RollbackAsync();

                throw new ProjectException("Failed to create Project", ExceptionCodes.PROJECT_FAILED_TO_CREATE);
            }

            return newProjectId;
        }

        public async Task ModifyProjectInTransaction(Guid userId, string userEmail, ProjectDto projectNewValues, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand updateProject = new NpgsqlCommand("UPDATE project_projects SET name = @newName, description = @newDescription, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @pi", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@pi", projectNewValues.ProjectId),
                        new("@newName", projectNewValues.Name),
                        new("@newDescription", projectNewValues.Description),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await updateProject.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to upadete project id: {projectNewValues.ProjectId} for user with user id {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);

                await unitOfWork.RollbackAsync();
            }
        }

        public async Task CreateProjectDefaultWorktypesInTransaction(Guid newProjectId, string userEmail, IUnitOfWork unitOfWork)
        {
            try
            {
                foreach (ProjectWorktypeDto defaultWorktype in _defaultProjectWorktypes)
                {
                    await using NpgsqlCommand insertProjectWorktype = new NpgsqlCommand("INSERT INTO project_worktypes (project_id, name, description, is_billable, created_by) VALUES (@projectId, @name, @description, @isBillable, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                    {
                        Parameters =
                        {
                            new("@projectId", newProjectId),
                            new("@name", defaultWorktype.Name),
                            new("@description", defaultWorktype.Description),
                            new("@isBillable", defaultWorktype.IsBillable),
                            new("@createdBy", userEmail),
                        }
                    };

                    await insertProjectWorktype.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to create default project worktypes for new project, user with email: {userEmail} exception message: {ex.Message} 

                     stacktrace: {ex.StackTrace}
                    """);

                await unitOfWork.RollbackAsync();

                throw new ProjectException("Failed to create projects default worktype", ExceptionCodes.PROJECT_WORKTYPE_FAILED_TO_CREATE);
            }
        }

        public async Task MarkProjectAsDeletedInTransaction(Guid userId, string userEmail, Guid projectId, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand markProjectDeleted = new NpgsqlCommand("UPDATE project_projects SET is_deleted = @isDeleted, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @pi", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@pi", projectId),
                        new("@isDeleted", true),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await markProjectDeleted.ExecuteNonQueryAsync();

                await using NpgsqlCommand markAllProjectWorktypesAsDeleted = new NpgsqlCommand("UPDATE project_worktypes SET is_deleted = @isDeleted, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE project_id = @pi", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@pi", projectId),
                        new("@isDeleted", true),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await markAllProjectWorktypesAsDeleted.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to mark project as deleted project id: {projectId} for user with user id {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                await unitOfWork.RollbackAsync();
            }
        }

        public async Task<List<ProjectSearchDto>> GetAllProjectsForUserInWorkspace(Guid userId, Guid workspaceId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId }, { "@wi", workspaceId } };

            IEnumerable<ProjectSearchDto> projectSearchDtos = await unitOfWork.Connection.QueryAsync<ProjectSearchDto>(@"
                SELECT DISTINCT
	                ww.id as WorkspaceId, 
	                pp.id as ProjectId,
	                pp.name as Name,
	                pp.description as Description,
	                pua.project_role as UserRole,
	                pp.is_deleted as IsDeleted
                FROM Project_projects pp
	                JOIN Workspace_workspaces ww
		                ON pp.workspace_id = ww.id
	                JOIN Workspace_user_assignments wua
		                ON wua.workspace_id = ww.id
	                JOIN project_user_assignments pua
		                ON pua.project_id = pp.id
                WHERE
	                wua.workspace_id = @wi
	                AND pua.user_id = @ui", dictionary);

            return projectSearchDtos.ToList();
        }

        public async Task CreateUserAssignmentInTransaction(Guid userId, string userEmail, ProjectUserAssignmentDto newUserAssignment, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand insertProjectUserAssignment = new NpgsqlCommand("INSERT INTO project_user_assignments (project_id, user_id, project_role, created_by) VALUES (@projectId, @userId, @projectRole, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@projectId", newUserAssignment.ProjectId),
                        new("@userId", newUserAssignment.UserId),
                        new("@projectRole", (int) newUserAssignment.Role),
                        new("@createdBy", userEmail),
                    }
                };

                await insertProjectUserAssignment.ExecuteNonQueryAsync();

                _logger.LogInformation($"New project user assingment created with role {newUserAssignment.Role} for userId: {userId} in project id {newUserAssignment.ProjectId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to create user assignment to project id: {newUserAssignment.ProjectId} for user with user id {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);

                await unitOfWork.RollbackAsync();
            }
        }

        public async Task<List<ProjectUserAssignmentDto>> GetProjectUserAssignmentDtos(Guid projectId, IUnitOfWork unitOfWork)
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
