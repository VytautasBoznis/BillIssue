using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.Workspace;
using BillIssue.Api.Interfaces.Project;
using BillIssue.Api.Interfaces.User;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using BillIssue.Shared.Models.Response.Project.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Data;

namespace BillIssue.Api.Business.Workspace
{
    public class WorkspaceFacade : IWorkspaceFacade
    {
        private readonly NpgsqlConnection _dbConnection;
        private readonly ILogger<WorkspaceFacade> _logger;

        private readonly ISessionFacade _sessionFacade;
        private readonly IProjectFacade _projectFacade;

        private const string NewUsersWorkspaceTagline = "{0}'s personal workspace";
        private const string NewProjectTagLine = "First workspace project";

        //TODO: Add fluent validators for all objects
        public WorkspaceFacade(
            ISessionFacade sessionFacade,
            IProjectFacade projectFacade,
            NpgsqlConnection dbConnection,
            ILogger<WorkspaceFacade> logger
        )
        {
            _dbConnection = dbConnection;
            _sessionFacade = sessionFacade;
            _logger = logger;

            _projectFacade = projectFacade;
        }

        #region Workspace Controll
        public async Task<List<WorkspaceSelectionDto>> GetAllWorkspaceSelectionsForUser(string sessionId, GetWorkspaceSelectionsForUserRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            Guid targetUserId = request.UserId;

            if (sessionModel.Id != targetUserId && sessionModel.Role != UserRole.Admin)
            {
                _logger.LogError($"User with user id: {sessionModel.Id} tried to access workspace selections for user with id: {request.UserId}.");
                throw new WorkspaceException("Workspaces not found", ExceptionCodes.WORKSPACES_NOT_FOUND);
            }

            List<WorkspaceSelectionDto> allUserWorkspaceSelections = GetAllWorkspaceSelectionsForUser(targetUserId);

            if (allUserWorkspaceSelections.Count == 0)
            {
                return allUserWorkspaceSelections;
            }

            List<ProjectSelectionDto> projectSelections = _projectFacade.GetProjectSelectionsForWorkspaces(allUserWorkspaceSelections.Select(aucws => aucws.Id).Distinct().ToList());

            if (projectSelections.Count == 0)
            {
                return allUserWorkspaceSelections;
            }

            foreach (WorkspaceSelectionDto companySelection in allUserWorkspaceSelections)
            {
                List<ProjectSelectionDto> companyProjectSelections = projectSelections.FindAll(pwt => pwt.WorkspaceId == companySelection.Id);
                companySelection.Projects = companyProjectSelections;
            }

            return allUserWorkspaceSelections;
        }

        public async Task<WorkspaceDto> GetWorkspace(string sessionId, GetWorkspaceRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            WorkspaceDto workspaceDto = GetWorkspaceDataWithPermissionCheck(sessionModel.Id, request.WorkspaceId, WorkspaceUserRole.Manager, sessionModel.Role == UserRole.Admin);

            if (workspaceDto == null)
            {
                _logger.LogError($"User with user id: {sessionModel.Id} tried to access an unknow workspace with id: {request.WorkspaceId}.");
                throw new WorkspaceException("Workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            if (request.LoadWorkspaceUsers)
            {
                workspaceDto.WorkspaceUsers = GetAllWorkspaceUsers(workspaceDto.Id);
            }

            return workspaceDto;
        }

        public async Task<List<WorkspaceSearchDto>> GetAllWorkspacesForUser(string sessionId, GetAllWorkspacesForUserRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            Guid targetUserId = request.UserId;

            if (sessionModel.Id != targetUserId && sessionModel.Role != UserRole.Admin)
            {
                _logger.LogError($"User with user id: {sessionModel.Id} tried to access all workspaces for user with id: {request.UserId}.");
                throw new WorkspaceException("Workspaces not found", ExceptionCodes.WORKSPACES_NOT_FOUND);
            }

            List<WorkspaceSearchDto> allUserWorkspaces = GetAllWorkspaceDataForUser(targetUserId);

            return allUserWorkspaces != null ? allUserWorkspaces: new List<WorkspaceSearchDto>();
        }

        public async Task<WorkspaceDto> CreateWorkspace(string sessionId, CreateWorkspaceRequest createWorkspaceRequest)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            WorkspaceDto WorkspaceDto = new WorkspaceDto { Name = createWorkspaceRequest.Name, Description = createWorkspaceRequest.Description };
            Guid newWorkspaceId = await CreateWorkspaceInTransaction(sessionModel.Id, sessionModel.Email, WorkspaceDto, transaction);

            WorkspaceUserAssignmentDto WorkspaceUserAssignmentDto = new WorkspaceUserAssignmentDto { WorkspaceId = newWorkspaceId, UserId = sessionModel.Id, WorkspaceRole = WorkspaceUserRole.Owner };
            await CreateWorkspaceUserAssignmentInTransaction(sessionModel.Id, sessionModel.Email, WorkspaceUserAssignmentDto, transaction);

            CreateProjectRequest createProjectRequest = new CreateProjectRequest { WorkspaceId = newWorkspaceId, Name = NewProjectTagLine, Description = NewProjectTagLine };
            await _projectFacade.CreateProject(sessionId, createProjectRequest, transaction);
            transaction.Commit();

            return await GetWorkspace(sessionId, new GetWorkspaceRequest { WorkspaceId = newWorkspaceId });
        }

        public async Task CreatePersonalWorkspaceForNewUser(Guid newUserId, string firstName, string email, NpgsqlTransaction transaction)
        {
            WorkspaceDto WorkspaceDto = new WorkspaceDto { Name = string.Format(NewUsersWorkspaceTagline, firstName), Description = string.Format(NewUsersWorkspaceTagline, firstName) };
            Guid newWorkspaceId = await CreateWorkspaceInTransaction(newUserId, firstName, WorkspaceDto, transaction);

            WorkspaceUserAssignmentDto WorkspaceUserAssignmentDto = new WorkspaceUserAssignmentDto { WorkspaceId = newWorkspaceId, UserId = newUserId, WorkspaceRole = WorkspaceUserRole.Owner };
            await CreateWorkspaceUserAssignmentInTransaction(newUserId, email, WorkspaceUserAssignmentDto, transaction);

            await _projectFacade.CreateProjectForNewUser(newUserId, newWorkspaceId, firstName, email, transaction);
        }

        public async Task<WorkspaceDto> UpdateWorkspace(string sessionId, UpdateWorkspaceRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);
            WorkspaceDto workspaceDto = GetWorkspaceDataWithPermissionCheck(sessionModel.Id, request.WorkspaceId, WorkspaceUserRole.Administrator, sessionModel.Role == UserRole.Admin);

            if (workspaceDto == null)
            {
                _logger.LogError($"User with user id: {sessionModel.Id} tried to access an unknow workspace with id: {request.WorkspaceId}.");
                throw new WorkspaceException("Workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            workspaceDto.Name = request.Name;
            workspaceDto.Description = request.Description;

            await UpdateWorkspaceInTransaction(sessionModel.Id, sessionModel.Email, workspaceDto, transaction);

            transaction.Commit();

            return await GetWorkspace(sessionId, new GetWorkspaceRequest { WorkspaceId = request.WorkspaceId });
        }

        public async Task RemoveWorkspace(string sessionId, RemoveWorkspaceRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            WorkspaceDto WorkspaceDto = GetWorkspaceDataWithPermissionCheck(sessionModel.Id, request.WorkspaceId, WorkspaceUserRole.Administrator, sessionModel.Role == UserRole.Admin);

            if (WorkspaceDto == null)
            {
                _logger.LogError($"User with user id: {sessionModel.Id} tried to access an unknow workspace with id: {request.WorkspaceId}.");
                throw new WorkspaceException("Workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            await MarkWorkspaceAsDeleted(sessionModel.Id, sessionModel.Email, request.WorkspaceId, transaction);

            transaction.Commit();
        }

        #endregion

        #region Workspace User management

        public async Task AddUserToWorkspace(string sessionId, AddUserToWorkspaceRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);
            WorkspaceDto workspaceDto = GetWorkspaceDataWithPermissionCheck(sessionModel.Id, request.WorkspaceId, WorkspaceUserRole.Manager, sessionModel.Role == UserRole.Admin);

            if (workspaceDto == null)
            {
                _logger.LogError($"User with user id: {sessionModel.Id} tried to access an unknow workspace with id: {request.WorkspaceId}.");
                throw new WorkspaceException("Workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            List<WorkspaceUserDto> workspaceUsers = GetAllWorkspaceUsers(workspaceDto.Id);

            if (workspaceUsers.Any(user => string.Equals(user.Email, request.NewUserEmail, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning($"User with id {sessionModel.Id} tried to add a new user to workspace id: {request.WorkspaceId} that is already present {request.NewUserEmail} in that workspace");
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            /*await _alertFacade.CreateWorkspaceNotificationInTransaction(
                sessionId, 
                new CreateWorkspaceNotificationRequest { 
                    WorkspaceId = request.WorkspaceId, 
                    TargetUserEmail = request.NewUserEmail 
                },
                transaction
            );*/

            //TODO: send email to user that there is an invite to a new workspace

            transaction.Commit();
        }

        public async Task<List<WorkspaceUserDto>> GetAllWorkspaceUsers(string sessionId, GetAllWorkspaceUsersRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);
            WorkspaceDto WorkspaceDto = GetWorkspaceDataWithPermissionCheck(sessionModel.Id, request.WorkspaceId, WorkspaceUserRole.Manager, sessionModel.Role == UserRole.Admin);

            if (WorkspaceDto == null)
            {
                _logger.LogError($"User with user id: {sessionModel.Id} tried to access an unknow workspace with id: {request.WorkspaceId}.");
                throw new WorkspaceException("Workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            List<WorkspaceUserDto> allWorkspaceUsers = GetAllWorkspaceUsers(WorkspaceDto.Id);

            return allWorkspaceUsers != null ? allWorkspaceUsers : new List<WorkspaceUserDto>();
        }

        //TODO: Can not remove user or modify role down if there are <2 users with owner/admin role
        public async Task RemoveUserFromWorkspace(string sessionId, RemoveUserFromWorkspaceRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);
            WorkspaceDto WorkspaceDto = GetWorkspaceDataWithPermissionCheck(sessionModel.Id, request.WorkspaceId, WorkspaceUserRole.Manager, sessionModel.Role == UserRole.Admin);

            List<WorkspaceUserDto> allWorkspaceUsers = GetAllWorkspaceUsers(WorkspaceDto.Id);

            WorkspaceUserDto requestorUserAssignment = allWorkspaceUsers.FirstOrDefault(ua => ua.UserId == sessionModel.Id);
            WorkspaceUserDto targetUserAssignment = allWorkspaceUsers.FirstOrDefault(ua => ua.UserId == request.UserId);

            if (targetUserAssignment == null)
            {
                _logger.LogError($"User with user id: {sessionModel.Id} tried to remove a user assignment for workspace {request.WorkspaceId} with user id: {request.UserId} that was not found");
                throw new WorkspaceException("User assignment not found", ExceptionCodes.WORKSPACE_USER_ASSIGNMENT_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            await RemoveWorkspaceUserAssignmentInTracsaction(sessionModel.Id, WorkspaceDto.Id, targetUserAssignment.UserId, transaction);

            transaction.Commit();
        }

        public async Task UpdateUserInWorkspace(string sessionId, UpdateUserInWorkspaceRequest request)
        {

            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            WorkspaceDto WorkspaceDto = GetWorkspaceDataWithPermissionCheck(sessionModel.Id, request.WorkspaceId, WorkspaceUserRole.Manager, sessionModel.Role == UserRole.Admin);

            if (WorkspaceDto == null)
            {
                _logger.LogError($"User with user id: {sessionModel.Id} tried to access an unknow workspace with id: {request.WorkspaceId}.");
                throw new WorkspaceException("Workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            WorkspaceUserAssignmentDto WorkspaceUserAssignmentDto = new WorkspaceUserAssignmentDto { WorkspaceId = request.WorkspaceId, WorkspaceRole = request.NewUserRole, UserId = request.UserId};
            await UpdateWorkspaceUserAssignmentDataInTransaction(sessionModel.Id, sessionModel.Email, WorkspaceUserAssignmentDto, transaction);

            transaction.Commit();
        }

        #endregion

        #region Private functions

        #region Company Workspace

        private async Task<Guid> CreateWorkspaceInTransaction(Guid userId, string userEmail, WorkspaceDto Workspace, NpgsqlTransaction transaction)
        {
            Guid newWorkspaceId = Guid.NewGuid();

            try
            {
                await using NpgsqlCommand insertWorkspace = new NpgsqlCommand("INSERT INTO workspace_workspaces (id, name, description, created_by) VALUES (@id, @name, @description, @createdBy)", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@id", newWorkspaceId),
                        new("@name", Workspace.Name),
                        new("@description", Workspace.Description),
                        new("@createdBy", userEmail),
                    }
                };

                await insertWorkspace.ExecuteNonQueryAsync();

                _logger.LogInformation($"New workspace created with name: {Workspace.Name} for userId: {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to create a company workspace for user with id: {userId} exception message: {ex.Message} 

                     stacktrace: {ex.StackTrace}
                    """);
                transaction.Rollback();

                throw new WorkspaceException("Failed to create workspace", ExceptionCodes.WORKSPACE_FAILED_TO_CREATE);
            }

            return newWorkspaceId;
        }

        private WorkspaceDto GetWorkspaceDataWithPermissionCheck(Guid userId, Guid WorkspaceId, WorkspaceUserRole minimalRole, bool isAdmin = false)
        {
            var dictionary = new Dictionary<string, object> { { "@wi", WorkspaceId } };

            WorkspaceDto WorkspaceDto = _dbConnection.Query<WorkspaceDto>("SELECT ww.id, ww.name, ww.description, ww.is_deleted as IsDeleted FROM workspace_workspaces ww WHERE id = @wi", dictionary).FirstOrDefault();

            if (WorkspaceDto == null)
            {
                _logger.LogError($"User with user id: {userId} tried to get an unknow company workspace with id: {WorkspaceId}.");
                throw new ProjectException("Company workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            if (WorkspaceDto.IsDeleted)
            {
                _logger.LogError($"User with user id: {userId} tried to get an company workspace that was deleted with id: {WorkspaceId}.");
                throw new ProjectException("Company workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            return WorkspaceDto;
        }

        private List<WorkspaceSelectionDto> GetAllWorkspaceSelectionsForUser(Guid userId)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId } };

            List<WorkspaceSelectionDto> WorkspaceSelectionsDtos = _dbConnection.Query<WorkspaceSelectionDto>(@"
                SELECT ww.id as Id, ww.name as Name 
                FROM workspace_user_assignments cwua
                    JOIN workspace_workspaces ww
                        ON cwua.workspace_id = ww.id
                WHERE
                    cwua.user_id = @ui
                    AND ww.is_deleted = false", dictionary).ToList();

            return WorkspaceSelectionsDtos;
        }

        private List<WorkspaceSearchDto> GetAllWorkspaceDataForUser(Guid userId)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId } };

            List<WorkspaceSearchDto> WorkspaceDtos = _dbConnection.Query<WorkspaceSearchDto>(@"
                SELECT cw.id, cw.name, cw.description, cwua.workspace_role as UserRole, cw.is_deleted as IsDeleted
                FROM workspace_user_assignments cwua
                    JOIN workspace_workspaces cw
                        ON cwua.workspace_id = cw.id
                WHERE
                    cwua.user_id = @ui", dictionary).ToList();

            return WorkspaceDtos;
        }

        private async Task UpdateWorkspaceInTransaction(Guid userId, string userEmail, WorkspaceDto WorkspaceDto, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand updateWorkspace = new NpgsqlCommand("UPDATE workspace_workspaces SET name = @newName, description = @newDescription, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @wi", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@wi", WorkspaceDto.Id),
                        new("@newName", WorkspaceDto.Name),
                        new("@newDescription", WorkspaceDto.Description),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await updateWorkspace.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to upadete company workspace: {WorkspaceDto.Id} for user with user id {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        private async Task MarkWorkspaceAsDeleted(Guid userId, string userEmail, Guid WorkspaceId, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand markCompnayWorkspaceAsDeleted = new NpgsqlCommand("UPDATE workspace_workspaces SET is_Deleted = @isDeleted, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @wi", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@wi", WorkspaceId),
                        new("@isDeleted", true),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await markCompnayWorkspaceAsDeleted.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to mark company workspace as deleted company workspace id: {WorkspaceId} for user with user id {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        #endregion

        #region Company Workspace Assignments

        public List<WorkspaceUserDto> GetAllWorkspaceUsers(Guid wokspaceId)
        {
            var dictionary = new Dictionary<string, object> { { "@wi", wokspaceId } };
            List<WorkspaceUserDto> WorkspaceUsers = _dbConnection.Query<WorkspaceUserDto>(@"
                SELECT WUA.id as assignmentId, UU.id as userId, UU.email, UU.first_name, UU.last_name, WUA.workspace_role as role
                FROM workspace_workspaces ww
                    JOIN workspace_user_assignments WUA
                        ON ww.id = WUA.workspace_id
                    JOIN user_users uu
                        ON WUA.user_id = UU.id
                WHERE ww.id=@wi
                    AND ww.is_deleted = false", dictionary).ToList();

            return WorkspaceUsers;
        }

        public async Task CreateWorkspaceUserAssignmentInTransaction(Guid userId, string userEmail, WorkspaceUserAssignmentDto workspaceAssignmentDto, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand insertWorkspaceUserAssignment = new NpgsqlCommand("INSERT INTO workspace_user_assignments (workspace_id, user_id, workspace_role, created_by) VALUES (@workspaceId, @userId, @workspaceRole, @createdBy)", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@workspaceId", workspaceAssignmentDto.WorkspaceId),
                        new("@userId", userId),
                        new("@workspaceRole", (int) workspaceAssignmentDto.WorkspaceRole),
                        new("@createdBy", userEmail),
                    }
                };

                await insertWorkspaceUserAssignment.ExecuteNonQueryAsync();

                _logger.LogInformation($"New workspace user assingment created with role {workspaceAssignmentDto.WorkspaceRole} for userId: {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to create a company workspace asignment for user with id: {userId} in company workspace id: {workspaceAssignmentDto.WorkspaceId} exception message: {ex.Message} 

                     stacktrace: {ex.StackTrace}
                    """);
                transaction.Rollback();

                throw new WorkspaceException("Failed to create workspace assignment", ExceptionCodes.WORKSPACE_USER_ASSIGNMENT_FAILED_TO_CREATE);
            }
        }

        private async Task UpdateWorkspaceUserAssignmentDataInTransaction(Guid userId, string userEmail, WorkspaceUserAssignmentDto WorkspaceAssignmentDto, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand updateUserAssignment = new NpgsqlCommand("UPDATE workspace_user_assignments SET workspace_role = @newRole, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE workspace_id = @cwi AND user_id = @targetUserId", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@cwi", WorkspaceAssignmentDto.WorkspaceId),
                        new("@newRole", (int) WorkspaceAssignmentDto.WorkspaceRole),
                        new("@targetUserId", WorkspaceAssignmentDto.UserId),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await updateUserAssignment.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to upadete user in company workspace assignment for workspace: {WorkspaceAssignmentDto.WorkspaceId} target user id: {WorkspaceAssignmentDto.UserId} user id: {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        private async Task RemoveWorkspaceUserAssignmentInTracsaction(Guid userId, Guid WorkspaceId, Guid targetUserId, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand deletUserAssignment = new NpgsqlCommand("DELETE FROM workspace_user_assignments WHERE workspace_id = @cwi AND user_id = @targetUserId", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@cwi", WorkspaceId),
                        new("@targetUserId", targetUserId)
                    }
                };

                await deletUserAssignment.ExecuteNonQueryAsync();

            }
            catch (Exception ex)
            {

                _logger.LogError($"""
                    Failed to remove user assignment for workspace: {WorkspaceId} user id: {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        #endregion

        #endregion
    }
}
