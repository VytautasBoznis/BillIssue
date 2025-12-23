using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Workspace;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Transactions;

namespace BillIssue.Api.Business.Repositories.Workspace
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly ILogger<WorkspaceRepository> _logger;

        public WorkspaceRepository(ILogger<WorkspaceRepository> logger)
        {
            _logger = logger;
        }

        public async Task<List<WorkspaceUserDto>> GetAllWorkspaceUsers(Guid wokspaceId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@wi", wokspaceId } };
            IEnumerable<WorkspaceUserDto> workspaceUsers = await unitOfWork.Connection.QueryAsync<WorkspaceUserDto>(@"
                SELECT WUA.id as assignmentId, UU.id as userId, UU.email, UU.first_name, UU.last_name, WUA.workspace_role as role
                FROM workspace_workspaces ww
                    JOIN workspace_user_assignments WUA
                        ON ww.id = WUA.workspace_id
                    JOIN user_users uu
                        ON WUA.user_id = UU.id
                WHERE ww.id=@wi
                    AND ww.is_deleted = false", dictionary);

            return workspaceUsers.ToList();
        }

        public async Task<List<WorkspaceSearchDto>> GetAllWorkspaceDataForUser(Guid userId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId } };

            IEnumerable<WorkspaceSearchDto> workspaceDtos = await unitOfWork.Connection.QueryAsync<WorkspaceSearchDto>(@"
                SELECT cw.id, cw.name, cw.description, cwua.workspace_role as UserRole, cw.is_deleted as IsDeleted
                FROM workspace_user_assignments cwua
                    JOIN workspace_workspaces cw
                        ON cwua.workspace_id = cw.id
                WHERE
                    cwua.user_id = @ui", dictionary);

            return workspaceDtos.ToList();
        }

        public async Task<WorkspaceDto> GetWorkspaceDataWithPermissionCheck(Guid userId, Guid WorkspaceId, WorkspaceUserRole minimalRole, IUnitOfWork unitOfWork, bool isAdmin = false)
        {
            var dictionary = new Dictionary<string, object> { { "@wi", WorkspaceId } };

            IEnumerable<WorkspaceDto> workspaceDtos = await unitOfWork.Connection.QueryAsync<WorkspaceDto>("SELECT ww.id, ww.name, ww.description, ww.is_deleted as IsDeleted FROM workspace_workspaces ww WHERE id = @wi", dictionary);
            WorkspaceDto? workspaceDto = workspaceDtos.FirstOrDefault();

            if (workspaceDto == null)
            {
                _logger.LogError($"User with user id: {userId} tried to get an unknow company workspace with id: {WorkspaceId}.");
                throw new ProjectException("Company workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            if (workspaceDto.IsDeleted)
            {
                _logger.LogError($"User with user id: {userId} tried to get an company workspace that was deleted with id: {WorkspaceId}.");
                throw new ProjectException("Company workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            return workspaceDto;
        }

        public async Task<List<WorkspaceSelectionDto>> GetAllWorkspaceSelectionsForUser(Guid userId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId } };

            IEnumerable<WorkspaceSelectionDto> workspaceSelectionsDtos = await unitOfWork.Connection.QueryAsync<WorkspaceSelectionDto>(@"
                SELECT ww.id as Id, ww.name as Name 
                FROM workspace_user_assignments cwua
                    JOIN workspace_workspaces ww
                        ON cwua.workspace_id = ww.id
                WHERE
                    cwua.user_id = @ui
                    AND ww.is_deleted = false", dictionary);

            return workspaceSelectionsDtos.ToList();
        }

        public async Task<Guid> CreateWorkspaceInTransaction(Guid userId, string userEmail, WorkspaceDto Workspace, IUnitOfWork unitOfWork)
        {
            Guid newWorkspaceId = Guid.NewGuid();

            try
            {
                await using NpgsqlCommand insertWorkspace = new NpgsqlCommand("INSERT INTO workspace_workspaces (id, name, description, created_by) VALUES (@id, @name, @description, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
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

                await unitOfWork.RollbackAsync();

                throw new WorkspaceException("Failed to create workspace", ExceptionCodes.WORKSPACE_FAILED_TO_CREATE);
            }

            return newWorkspaceId;
        }
        
        public async Task ModifyWorkspaceInTransaction(Guid userId, string userEmail, WorkspaceDto WorkspaceDto, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand updateWorkspace = new NpgsqlCommand("UPDATE workspace_workspaces SET name = @newName, description = @newDescription, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @wi", unitOfWork.Connection, unitOfWork.Transaction)
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
                await unitOfWork.RollbackAsync();
            }
        }

        public async Task MarkWorkspaceAsDeleted(Guid userId, string userEmail, Guid WorkspaceId, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand markCompnayWorkspaceAsDeleted = new NpgsqlCommand("UPDATE workspace_workspaces SET is_Deleted = @isDeleted, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @wi", unitOfWork.Connection, unitOfWork.Transaction)
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
                await unitOfWork.RollbackAsync();
            }
        }

        public async Task CreateWorkspaceUserAssignmentInTransaction(Guid userId, string userEmail, WorkspaceUserAssignmentDto workspaceAssignmentDto, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand insertWorkspaceUserAssignment = new NpgsqlCommand("INSERT INTO workspace_user_assignments (workspace_id, user_id, workspace_role, created_by) VALUES (@workspaceId, @userId, @workspaceRole, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
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

                await unitOfWork.RollbackAsync();

                throw new WorkspaceException("Failed to create workspace assignment", ExceptionCodes.WORKSPACE_USER_ASSIGNMENT_FAILED_TO_CREATE);
            }
        }
    }
}
