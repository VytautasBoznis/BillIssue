using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.Project;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Api.Models.Models.Projects;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Data;

namespace BillIssue.Api.Business.Project
{
    public class ProjectFacade : IProjectFacade
    {
        private readonly NpgsqlConnection _dbConnection;
        private readonly ILogger<ProjectFacade> _logger;

        private readonly ISessionFacade _sessionFacade;

        private readonly List<ProjectWorktypeDto> _defaultProjectWorktypes;

        private const string NewUsersProjectTagline = "{0}'s personal project";

        //TODO: Add fluent validators for all objects
        public ProjectFacade(
            ISessionFacade sessionFacade,
            NpgsqlConnection dbConnection,
            ILogger<ProjectFacade> logger)
        {
            _sessionFacade = sessionFacade;
            _dbConnection = dbConnection;
            _logger = logger;

            _defaultProjectWorktypes = [
                new() { Name = "Billable time", Description = "Billable time", IsBillable = true },
                new() { Name = "Non-billable time", Description = "Non-billable time", IsBillable = false },
            ];

            //_defaultProjectWorktypes = [
            //    new() { Name = "Software development", Description = "Time spent on software development", IsBillable = true },
            //    new() { Name = "Planning/Management", Description = "Time spent on project planning/management", IsBillable = true },
            //    new() { Name = "Internal meetings", Description = "Time spent on internal meetings", IsBillable = true },
            //    new() { Name = "External meetings", Description = "Time spent on external meetings", IsBillable = true },
            //    new() { Name = "DevOps", Description = "Time spent on DevOps", IsBillable = true },
            //    new() { Name = "Production recovery", Description = "Time spent resolving issues in production", IsBillable = true },
            //    new() { Name = "Training", Description = "Time spent on training", IsBillable = true },
            //    new() { Name = "Internal activities", Description = "Time spent on internal activities (not related to prject)", IsBillable = false },
            //    new() { Name = "Hardware/Software issue resolution", Description = "Time spent resolving issues with software or hardware", IsBillable = false },
            //];
        }

        #region Project management
        public List<ProjectSelectionDto> GetProjectSelectionsForWorkspaces(List<Guid> WorkspaceIds)
        {
            List<ProjectAndWorktypeModelFlattened> projectAndWorktypeModelsFlattened = GetAllProjectSelectionsForWorkspacesFlattened(WorkspaceIds);
            List<Guid> uniqueProjectIds = projectAndWorktypeModelsFlattened.Select(paw => paw.ProjectId).Distinct().ToList();
            List<ProjectSelectionDto> finalSelectionList = [];

            foreach (Guid projectId in uniqueProjectIds)
            {
                List<ProjectAndWorktypeModelFlattened> projectItems = projectAndWorktypeModelsFlattened.FindAll(pwt => pwt.ProjectId == projectId);

                if (projectItems.Count == 0) 
                {
                    continue;
                }

                ProjectSelectionDto projectSelectionDto = new()
                {
                    Id = projectId,
                    Name = projectItems[0].ProjectName,
                    WorkspaceId = projectItems[0].WorkspaceId,
                };

                List<ProjectWorktypeSelectionDto> projectWorktypes = [];

                projectWorktypes.AddRange(projectItems.Select(projectItem => new ProjectWorktypeSelectionDto
                {
                    Id = projectItem.WorktypeId,
                    Name = projectItem.WorktypeName
                }));

                projectSelectionDto.Worktypes = projectWorktypes;

                finalSelectionList.Add(projectSelectionDto);
            }

            return finalSelectionList;
        }

        public async Task<ProjectDto> CreateProject(string sessionId, CreateProjectRequest request, NpgsqlTransaction transaction = null)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            bool newTransaction = false;

            if (transaction == null)
            { 
                newTransaction = true;
                transaction = _dbConnection.BeginTransaction();
            }

            ProjectDto projectDto = new ProjectDto { WorkspaceId = request.WorkspaceId, Name = request.Name, Description = request.Description };

            Guid newProjectId = await CreateProjectInTransaction(sessionModel.Id, sessionModel.Email, projectDto, transaction);
            ProjectUserAssignmentDto projectUserAssignmentDto = new ProjectUserAssignmentDto { ProjectId = newProjectId, Role = ProjectUserRoles.Owner };
            await CreateUserAssignmentInTransaction(sessionModel.Id, sessionModel.Email, projectUserAssignmentDto, transaction);
            await CreateProjectDefaultWorktypesInTransaction(newProjectId, sessionModel.Email, transaction);

            if (newTransaction)
            {
                transaction.Commit();
            }

            return await GetProject(sessionId, new GetProjectRequest { ProjectId = newProjectId });
        }

        public async Task CreateProjectForNewUser(Guid newUserId, Guid newWorkspaceId, string firstName, string email, NpgsqlTransaction transaction)
        {
            ProjectDto projectDto = new ProjectDto { WorkspaceId = newWorkspaceId, Name = string.Format(NewUsersProjectTagline, firstName), Description = string.Format(NewUsersProjectTagline, firstName) };

            Guid newProjectId = await CreateProjectInTransaction(newUserId, email, projectDto, transaction);
            ProjectUserAssignmentDto projectUserAssignmentDto = new ProjectUserAssignmentDto { ProjectId = newProjectId, UserId = newUserId, Role = ProjectUserRoles.Owner };
            await CreateUserAssignmentInTransaction(newUserId, email, projectUserAssignmentDto, transaction);
            await CreateProjectDefaultWorktypesInTransaction(newProjectId, email, transaction);
        }

        public async Task<ProjectDto> GetProject(string sessionId, GetProjectRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.Contributor, sessionModel.Role == UserRole.Admin);

            if (projectDto == null) {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            projectDto.ProjectWorktypes = GetProjectWorktypeDtos(request.ProjectId);

            if (request.LoadUserAssignments)
            {
                projectDto.ProjectUserAssignments = GetProjectUserAssignmentDtos(request.ProjectId);
            }

            return projectDto;
        }

        public async Task ModifyProject(string sessionId, ModifyProjectRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto =  GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.Owner, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            projectDto.Name = request.Name;
            projectDto.Description = request.Description;

            await ModifyProjectInTransaction(sessionModel.Id, sessionModel.Email, projectDto, transaction);

            transaction.Commit();
        }

        public async Task RemoveProject(string sessionId, RemoveProjectRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.Owner, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            await MarkProjectAsDeletedInTransaction(sessionModel.Id, sessionModel.Email, request.ProjectId, transaction);

            transaction.Commit();
        }

        public async Task<List<ProjectSearchDto>> GetUserProjectsInWorkspace(string sessionId, GetUserProjectsInWorkspaceForUserRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            List<ProjectSearchDto> projectSearchDtos = GetAllProjectsForUserInWorkspace(sessionModel.Id, request.WorkspaceId);

            if (projectSearchDtos == null)
            {
                _logger.LogWarning($"There were no projects found in workspace id: {request.WorkspaceId} for user with id: {sessionModel.Id} .");
                return new List<ProjectSearchDto>();
            }

            return projectSearchDtos;
        }

        #endregion

        #region Project worktype management

        public async Task CreateProjectWorktype(string sessionId, CreateProjectWorktypeRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.Owner, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            ProjectWorktypeDto newProjectWorktype = new() { ProjectId = request.ProjectId, Name = request.Name, Description = request.Description, IsBillable = request.IsBillable };
            await CreateProjectWorktypeInTransaction(sessionModel.Id, sessionModel.Email, newProjectWorktype, transaction);
            transaction.Commit();
        }

        public async Task<ProjectWorktypeDto> GetProjectWorktype(string sessionId, GetProjectWorktypeRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectWorktypeDto projectWorktypeDto = GetProjectWorktypeWithPermissionCheck(sessionModel.Id, request.ProjectWorktypeId, ProjectUserRoles.TeamLead, sessionModel.Role == UserRole.Admin);

            if (projectWorktypeDto == null)
            {
                _logger.LogError($"There was an issue loading the project worktype with id: {request.ProjectWorktypeId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project worktype not found", ExceptionCodes.PROJECT_WORKTYPE_NOT_FOUND);
            }

            return projectWorktypeDto;
        }

        public async Task<List<ProjectWorktypeDto>> GetAllProjectWorktypes(string sessionId, GetAllProjectWorktypesRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.Contributor, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            return GetProjectWorktypeDtos(request.ProjectId);
        }

        public async Task ModifyProjectWorktype(string sessionId, ModifyProjectWorktypeRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.Owner, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();
            ProjectWorktypeDto newProjectWorktypeValues = new ProjectWorktypeDto { ProjectId = request.ProjectId, ProjectWorktypeId = request.ProjectWorktypeId, Name = request.Name, Description = request.Description, IsBillable = request.IsBillable };
            await ModifyProjectWorktypeInTransaction(sessionModel.Id, sessionModel.Email, newProjectWorktypeValues, transaction);

            transaction.Commit();
        }

        public async Task RemoveProjectWorktype(string sessionId, RemoveProjectWorktypeRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.Owner, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            await MarkProjectWorktypeAsDeletedInTransaction(sessionModel.Id, sessionModel.Email, request.ProjectId, request.ProjectWorktypeId, transaction);

            transaction.Commit();
        }

        #endregion

        #region Project User management

        public async Task<List<ProjectDto>> GetProjectsForUser(string sessionId)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);
            return GetAllProjectsWhereUserIsAssigned(sessionModel.Id);
        }

        public async Task<List<ProjectUserAssignmentDto>> GetProjectUsers(string sessionId, GetProjectUsersRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.TeamLead, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            return GetProjectUserAssignmentDtos(request.ProjectId);
        }

        public async Task AddUserAssignmentToProject(string sessionId, AddUserAssignmentToProjectRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.TeamLead, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();
            ProjectUserAssignmentDto projectUserAsssignmentDto = new ProjectUserAssignmentDto { ProjectId = request.ProjectId, UserId = request.UserId, Role = request.Role };
            await CreateUserAssignmentInTransaction(sessionModel.Id, sessionModel.Email, projectUserAsssignmentDto, transaction);

            transaction.Commit();
        }

        //TODO: Can not remove user or modify role down if there are <2 users with owner/admin role
        public async Task ModifyUserAssingmentInProject(string sessionId, ModifyUserAssingmentInProjectRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.TeamLead, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            ProjectUserAssignmentDto newProjectAssignmentValues = new ProjectUserAssignmentDto { ProjectId = request.ProjectId, UserAssignmentId = request.ProjectUserAssignmentId, Role = request.Role };
            await ModifyUserAssignmentInTransaction(sessionModel.Id, sessionModel.Email, newProjectAssignmentValues, transaction);
            transaction.Commit();
        }

        //TODO: Can not remove user or modify role down if there are <2 users with owner/admin role
        public async Task RemoveUserAssingmentFromProject(string sessionId, RemoveUserAssingmentFromProjectRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            ProjectDto projectDto = GetProjectDataWithPermissionCheck(sessionModel.Id, request.ProjectId, ProjectUserRoles.TeamLead, sessionModel.Role == UserRole.Admin);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {sessionModel.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();
            ProjectUserAssignmentDto newProjectAssignmentValues = new ProjectUserAssignmentDto { ProjectId = request.ProjectId, UserAssignmentId = request.ProjectUserAssignmentId };
            await DeleteUserAssignmentInTransaction(sessionModel.Id, sessionModel.Email, newProjectAssignmentValues, transaction);
            transaction.Commit();
        }

        #endregion

        #region Private Functions

        #region Project

        private ProjectDto GetProjectDataWithPermissionCheck(Guid userId, Guid projectId, ProjectUserRoles minimumRole, bool isUserAdmin)
        {
            var dictionary = new Dictionary<string, object> { { "@id", projectId } };
            ProjectDto projectDto = _dbConnection.Query<ProjectDto>("SELECT id as ProjectId, workspace_id as WorkspaceId, name, is_deleted as IsDeleted, description FROM project_projects WHERE id = @id", dictionary).FirstOrDefault();

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
            dictionary.Add("@pur", (int) minimumRole);
            ProjectUserAssignmentDto projectUserAssignmentDto = _dbConnection.Query<ProjectUserAssignmentDto>(@"
                SELECT pua.id as UserAssignmentId, pua.project_id as ProjectId, pua.user_id as UserId, pua.project_role as Role
                FROM project_user_assignments pua
                WHERE pua.user_id = @pua
                    AND pua.project_id = @id
                    AND pua.project_role >= @pur", dictionary).FirstOrDefault();

            if (projectUserAssignmentDto == null && !isUserAdmin)
            {
                _logger.LogError($"User with user id: {userId} tried to access project with id: {projectId} that he did not have access to.");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            return projectDto;
        }

        private List<ProjectDto> GetAllProjectsWhereUserIsAssigned(Guid userId)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId } };
            List<ProjectDto> projectDtos = _dbConnection.Query<ProjectDto>(@"SELECT pp.id as ProjectId, pp.name, pp.description
            FROM project_projects pp
	            JOIN project_user_assigments pua
		            ON pp.id = pua.project_id
            WHERE
	            pua.user_id = @ui
                AND pp.is_deleted = false", dictionary).ToList();

            if (projectDtos == null || projectDtos.Count == 0)
            {
                return [];
            }

            dictionary.Add("@pids", string.Join(",", projectDtos.Select(p => p.ProjectId)));
            List<ProjectUserAssignmentDto> projectUserAssignmentDtos = _dbConnection.Query<ProjectUserAssignmentDto>(@"
            SELECT pua.id as UserAssignmentId, pua.ProjectId as ProjectId, pua.user_id, pua.project_role 
            FROM project_user_assignments pua 
            WHERE pua.user_id = @ui AND pua.project_id in (@pids)", dictionary).ToList();

            foreach (ProjectDto project in projectDtos)
            {
                ProjectUserAssignmentDto userAssignment = projectUserAssignmentDtos.FirstOrDefault(puad => puad.UserId == userId && puad.ProjectId == project.ProjectId);
                project.ProjectUserAssignments = new List<ProjectUserAssignmentDto>() { userAssignment };
            }

            return projectDtos;
        }

        private async Task<Guid> CreateProjectInTransaction(Guid userId, string userEmail, ProjectDto newProjectValues, NpgsqlTransaction transaction)
        {
            Guid newProjectId = Guid.NewGuid();

            try
            {
                await using NpgsqlCommand insertProject = new NpgsqlCommand("INSERT INTO project_projects (id, workspace_id, name, description, created_by) VALUES (@id, @cwi, @name, @description, @createdBy)", _dbConnection, transaction)
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
                transaction.Rollback();

                throw new ProjectException("Failed to create Project", ExceptionCodes.PROJECT_FAILED_TO_CREATE);
            }

            return newProjectId;
        }

        private async Task ModifyProjectInTransaction(Guid userId, string userEmail, ProjectDto projectNewValues, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand updateProject = new NpgsqlCommand("UPDATE project_projects SET name = @newName, description = @newDescription, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @pi", _dbConnection, transaction)
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
                transaction.Rollback();
            }
        }

        private async Task MarkProjectAsDeletedInTransaction(Guid userId, string userEmail, Guid projectId, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand markProjectDeleted = new NpgsqlCommand("UPDATE project_projects SET is_deleted = @isDeleted, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @pi", _dbConnection, transaction)
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

                await using NpgsqlCommand markAllProjectWorktypesAsDeleted = new NpgsqlCommand("UPDATE project_worktypes SET is_deleted = @isDeleted, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE project_id = @pi", _dbConnection, transaction)
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
                transaction.Rollback();
            }
        }

        private List<ProjectAndWorktypeModelFlattened> GetAllProjectSelectionsForWorkspacesFlattened(List<Guid> WorkspaceIds)
        {
            var dictionary = new Dictionary<string, object> { { "@cwi", WorkspaceIds } };

            List<ProjectAndWorktypeModelFlattened> projectAndWorktypeModelsFlattened = _dbConnection.Query<ProjectAndWorktypeModelFlattened>(@"
                SELECT
                    pp.workspace_id as WorkspaceId,
	                pp.id as ProjectId,
	                pp.name as ProjectName,
	                pw.id as WorktypeId,
	                pw.name as WorktypeName
                FROM project_projects pp
	                JOIN project_worktypes pw
		                ON pp.id = pw.project_id
                WHERE pp.workspace_id = ANY (@cwi)", dictionary).ToList();

            return projectAndWorktypeModelsFlattened;
        }

        private List<ProjectSearchDto> GetAllProjectsForUserInWorkspace(Guid userId, Guid workspaceId)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId }, { "@wi", workspaceId} };

            List<ProjectSearchDto> projectSearchDtos = _dbConnection.Query<ProjectSearchDto>(@"
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
	                AND pua.user_id = @ui", dictionary).ToList();

            return projectSearchDtos;
        }

        #endregion

        #region Project Worktypes
        private async Task CreateProjectDefaultWorktypesInTransaction(Guid newProjectId, string userEmail, NpgsqlTransaction transaction)
        {
            try
            {
                foreach(ProjectWorktypeDto defaultWorktype in _defaultProjectWorktypes)
                {
                    await using NpgsqlCommand insertProjectWorktype = new NpgsqlCommand("INSERT INTO project_worktypes (project_id, name, description, is_billable, created_by) VALUES (@projectId, @name, @description, @isBillable, @createdBy)", _dbConnection, transaction)
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
                transaction.Rollback();

                throw new ProjectException("Failed to create projects default worktype", ExceptionCodes.PROJECT_WORKTYPE_FAILED_TO_CREATE);
            }
        }

        private ProjectWorktypeDto GetProjectWorktypeWithPermissionCheck(Guid userId, Guid projectWorktypeId, ProjectUserRoles minimumRole, bool isUserAdmin)
        {
            var dictionary = new Dictionary<string, object> { { "@pwi", projectWorktypeId} };

            ProjectWorktypeDto projectWorktypeDto = _dbConnection.Query<ProjectWorktypeDto>(@"SELECT 
	                pw.id as ProjectWorktypeId,
	                pw.project_id as ProjectId,
	                pp.name as ProjectName,
	                pw.name as Name,
	                pw.description as Description,
	                pw.is_billable as IsBillable,
	                pw.is_deleted as IsDeleted	
                FROM
	                project_worktypes pw
	                JOIN project_projects pp
		                ON pw.project_id = pp.id
                WHERE
	                pw.id = @pwi", dictionary).FirstOrDefault();

            if (projectWorktypeDto == null)
            {
                _logger.LogError($"User with user id: {userId} tried to get an unknow project worktype with id: {projectWorktypeId}.");
                throw new ProjectException("Project worktype not found", ExceptionCodes.PROJECT_WORKTYPE_NOT_FOUND);
            }

            if (projectWorktypeDto.IsDeleted)
            {
                _logger.LogError($"User with user id: {userId} tried to get an a project worktype that is deleted project worktype with id: {projectWorktypeId}.");
                throw new ProjectException("Project worktype not found", ExceptionCodes.PROJECT_WORKTYPE_NOT_FOUND);
            }

            dictionary.Add("@ui", userId);
            dictionary.Add("@ur", (int)minimumRole);

            ProjectUserAssignmentDto projectUserAssignmentDto = _dbConnection.Query<ProjectUserAssignmentDto>(@"
                SELECT pua.id as UserAssignmentId, pua.project_id as ProjectId, pua.user_id as UserId, pua.project_role as Role
                FROM project_user_assignments pua
	                JOIN project_projects pp
		                ON pp.id = pua.project_id
	                JOIN project_worktypes pw
		                ON pw.project_id = pp.id
                WHERE pua.user_id = @ui
                    AND pw.id = @pwi
                    AND pua.project_role >= @ur", dictionary).FirstOrDefault();

            if (projectUserAssignmentDto == null && !isUserAdmin)
            {
                _logger.LogError($"User with user id: {userId} tried to access project worktype with id: {projectWorktypeId} that he did not have access to.");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_WORKTYPE_NOT_FOUND);
            }

            return projectWorktypeDto;
        }

        private async Task CreateProjectWorktypeInTransaction(Guid userId, string userEmail, ProjectWorktypeDto newProjectWorktype, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand insertProjectWorktype = new NpgsqlCommand("INSERT INTO project_worktypes (project_id, name, description, is_billable, created_by) VALUES (@projectId, @name, @description, @isBillable, @createdBy)", _dbConnection, transaction)
                {
                    Parameters =
                        {
                            new("@projectId", newProjectWorktype.ProjectId),
                            new("@name", newProjectWorktype.Name),
                            new("@description", newProjectWorktype.Description),
                            new("@isBillable", newProjectWorktype.IsBillable),
                            new("@createdBy", userEmail),
                        }
                };

                await insertProjectWorktype.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to create project worktype for project id: {newProjectWorktype.ProjectId}, user id: {userId} exception message: {ex.Message} 

                     stacktrace: {ex.StackTrace}
                    """);
                transaction.Rollback();

                throw new ProjectException("Failed to create projects worktype", ExceptionCodes.PROJECT_WORKTYPE_FAILED_TO_CREATE);
            }
        }

        private async Task ModifyProjectWorktypeInTransaction(Guid userId, string userEmail, ProjectWorktypeDto newProjectWorktypeValues, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand updateWorktype = new NpgsqlCommand("UPDATE project_worktypes SET name = @newName, description = @newDescription, is_billable = @billable, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE project_id = @pi AND id = @pwi", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@pi", newProjectWorktypeValues.ProjectId),
                        new("@pwi", newProjectWorktypeValues.ProjectWorktypeId),
                        new("@newName", newProjectWorktypeValues.Name),
                        new("@newDescription", newProjectWorktypeValues.Description),
                        new("@billable", newProjectWorktypeValues.IsBillable),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await updateWorktype.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to upadete project worktype id: {newProjectWorktypeValues.ProjectWorktypeId} in project id: {newProjectWorktypeValues.ProjectId} for user with user id {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        private List<ProjectWorktypeDto> GetProjectWorktypeDtos(Guid projectId)
        {
            var dictionary = new Dictionary<string, object> { { "@pid", projectId } };
            List<ProjectWorktypeDto> projectWorktypes = _dbConnection.Query<ProjectWorktypeDto>(@"
                SELECT pp.id as ProjectId, pw.id as ProjectWorktypeId, pw.name, pw.description, pw.is_billable as IsBillable, pw.is_deleted as IsDeleted
                FROM project_projects pp
	                JOIN project_worktypes pw
		                ON pp.id = pw.project_id
                WHERE pp.id = @pid", dictionary).ToList();

            return projectWorktypes;
        }

        private async Task MarkProjectWorktypeAsDeletedInTransaction(Guid userId, string userEmail, Guid projectId, Guid projectWorktypeId, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand markProjectWorktypeAsDeleted = new NpgsqlCommand("UPDATE project_worktypes SET is_deleted = @isDeleted, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE project_id = @pi AND id = @pwi", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@pi", projectId),
                        new("@pwi", projectWorktypeId),
                        new("@isDeleted", true),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await markProjectWorktypeAsDeleted.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed mark project worktype as deleted project worktype id: {projectWorktypeId} in project id: {projectId} for user with user id {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        #endregion

        #region Project User Assignments

        private List<ProjectUserAssignmentDto> GetProjectUserAssignmentDtos(Guid projectId)
        {
            var dictionary = new Dictionary<string, object> { { "@pid", projectId } };
            List<ProjectUserAssignmentDto> projectUserAssignments = _dbConnection.Query<ProjectUserAssignmentDto>(@"
                SELECT pua.id as UserAssignmentId, pua.project_id as ProjectId, uu.id as UserId, uu.email, uu.first_name, uu.last_name, pua.project_role as Role
                FROM Project_projects pp
                    JOIN Project_user_assignments pua
                        ON pp.id = pua.project_id
                    JOIN User_users uu
                        ON pua.user_id = uu.id
                WHERE pp.id = @pid", dictionary).ToList();

            return projectUserAssignments;
        }

        private async Task CreateUserAssignmentInTransaction(Guid userId, string userEmail, ProjectUserAssignmentDto newUserAssignment, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand insertProjectUserAssignment = new NpgsqlCommand("INSERT INTO project_user_assignments (project_id, user_id, project_role, created_by) VALUES (@projectId, @userId, @projectRole, @createdBy)", _dbConnection, transaction)
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
                transaction.Rollback();
            }
        }

        private async Task ModifyUserAssignmentInTransaction(Guid userId, string userEmail, ProjectUserAssignmentDto newUserAssignment, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand modifyUserAssginment = new NpgsqlCommand("UPDATE project_user_assignments SET project_role = @projectRole, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @id", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@id", newUserAssignment.UserAssignmentId),
                        new("@projectRole", (int) newUserAssignment.Role),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await modifyUserAssginment.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to modify user assignment with id: {newUserAssignment.UserAssignmentId} in project id: {newUserAssignment.ProjectId} for user with user id {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        private async Task DeleteUserAssignmentInTransaction(Guid userId, string userEmail, ProjectUserAssignmentDto targetUserAssignment, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand deleteUserAssignment = new NpgsqlCommand("DELETE FROM project_user_assignments WHERE id = @id", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@id", targetUserAssignment.UserAssignmentId),
                    }
                };

                await deleteUserAssignment.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to Delete user assignment with id: {targetUserAssignment.UserAssignmentId} in project id: {targetUserAssignment.ProjectId} for user with user id {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        #endregion

        #endregion
    }
}
