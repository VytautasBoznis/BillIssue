using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Project
{
    public class CreateProjectOperation : BaseOperation<CreateProjectRequest, CreateProjectResponse>
    {
        private readonly List<ProjectWorktypeDto> _defaultProjectWorktypes = [
            new() { Name = "Billable time", Description = "Billable time", IsBillable = true },
            new() { Name = "Non-billable time", Description = "Non-billable time", IsBillable = false },
        ];

        public CreateProjectOperation(
            ILogger<CreateProjectOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<CreateProjectRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<CreateProjectResponse> Execute(CreateProjectRequest request, IUnitOfWork unitOfWork)
        {
            await unitOfWork.BeginTransactionAsync();

            ProjectDto projectDto = new ProjectDto { WorkspaceId = request.WorkspaceId, Name = request.Name, Description = request.Description };

            Guid newProjectId = await CreateProjectInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, projectDto, unitOfWork);
            ProjectUserAssignmentDto projectUserAssignmentDto = new ProjectUserAssignmentDto { ProjectId = newProjectId, UserId = request.SessionUserData.Id, Role = ProjectUserRoles.Owner };

            await CreateUserAssignmentInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, projectUserAssignmentDto, unitOfWork);
            await CreateProjectDefaultWorktypesInTransaction(newProjectId, request.SessionUserData.Email, unitOfWork);

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

        private async Task<Guid> CreateProjectInTransaction(Guid userId, string userEmail, ProjectDto newProjectValues, IUnitOfWork unitOfWork)
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

        private async Task CreateUserAssignmentInTransaction(Guid userId, string userEmail, ProjectUserAssignmentDto newUserAssignment, IUnitOfWork unitOfWork)
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

        private async Task CreateProjectDefaultWorktypesInTransaction(Guid newProjectId, string userEmail, IUnitOfWork unitOfWork)
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
    }
}
