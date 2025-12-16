using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Project;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Project;
using BillIssue.Shared.Models.Response.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Workspace
{
    public class CreateWorkspaceOperation : BaseOperation<CreateWorkspaceRequest, CreateWorkspaceResponse>
    {
        public CreateWorkspaceOperation(
            ILogger<CreateWorkspaceOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<CreateWorkspaceRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<CreateWorkspaceResponse> Execute(CreateWorkspaceRequest request, IUnitOfWork unitOfWork)
        {
            await unitOfWork.BeginTransactionAsync();

            WorkspaceDto workspaceDto = new WorkspaceDto { Name = request.Name, Description = request.Description };

            Guid newWorkspaceId = await CreateWorkspaceInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, workspaceDto, unitOfWork);

            WorkspaceUserAssignmentDto WorkspaceUserAssignmentDto = new()
            { 
                WorkspaceId = newWorkspaceId,
                UserId = request.SessionUserData.Id,
                WorkspaceRole = WorkspaceUserRole.Owner 
            };

            await CreateWorkspaceUserAssignmentInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, WorkspaceUserAssignmentDto, unitOfWork);

            CreateProjectRequest createProjectRequest = new()
            {
                SessionUserData = request.SessionUserData,
                WorkspaceId = newWorkspaceId,
                Name = request.Name,
                Description = request.Description,
                CompleteTransactions = false
            };
            
            await _operationFactory
                    .Get<CreateProjectOperation>(typeof(CreateProjectOperation))
                    .Run(createProjectRequest, unitOfWork);

            GetWorkspaceRequest getWorkspaceRequest = new()
            {
                SessionUserData = request.SessionUserData,
                WorkspaceId = newWorkspaceId
            };

            GetWorkspaceResponse getWorkspaceResponse = await _operationFactory
                                                                .Get<GetWorkspaceOperation>(typeof(GetWorkspaceOperation))
                                                                .Run(getWorkspaceRequest, unitOfWork);

            return new CreateWorkspaceResponse
            {
                WorkspaceDto = getWorkspaceResponse.WorkspaceDto
            };
        }

        private async Task<Guid> CreateWorkspaceInTransaction(Guid userId, string userEmail, WorkspaceDto Workspace, IUnitOfWork unitOfWork)
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
