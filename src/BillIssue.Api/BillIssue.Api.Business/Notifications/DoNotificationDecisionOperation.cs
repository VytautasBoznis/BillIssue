using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Notification;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Errors;
using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Response.Notifications;
using BillIssue.Shared.Models.Response.Notifications.Dto;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Notifications
{
    public class DoNotificationDecisionOperation : BaseOperation<DoNotificationDecisionRequest, DoNotificationDecisionResponse>
    {
        public DoNotificationDecisionOperation(
            ILogger<DoNotificationDecisionOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<DoNotificationDecisionRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<DoNotificationDecisionResponse> Execute(DoNotificationDecisionRequest request, IUnitOfWork unitOfWork)
        {
            switch (request.Notification.NotificationType)
            {
                case NotificationType.WorkspaceNotification:
                    {
                        await HandleWorkspaceNotificationDecision(request.Notification, request.Decision, request.SessionUserData, unitOfWork);
                        break;
                    }
                default:
                    {
                        throw new UnknowNotificationTypeException("Unknow notification type", ExceptionCodes.NOTIFICATION_UNKNOWN_TYPE);
                    }
            }

            return new DoNotificationDecisionResponse();
        }

        private async Task HandleWorkspaceNotificationDecision(NotificationDto notification, bool decision, SessionUserData sessionModel, IUnitOfWork unitOfWork)
        {
            WorkspaceNotificationDto alert = await GetWorkspaceNotification(notification.NotificationId, unitOfWork);

            if (alert == null)
            {
                _logger.LogError($"User tried to make a decision for a notification that does not exist. Notification id: ${notification.NotificationId}, User id: ${sessionModel.Id}");
                throw new AlertException("Alert not found", ExceptionCodes.NOTIFICATION_NOT_FOUND);
            }

            if (alert.Email != sessionModel.Email)
            {
                _logger.LogError($"User tried to make a decision for another users notification. Notification id: ${notification.NotificationId}, User id: ${sessionModel.Id}");
                throw new AlertException("Decision not allowed", ExceptionCodes.NOTIFICATION_NOT_ALLOWED_DECISION);
            }

            await unitOfWork.BeginTransactionAsync();

            if (decision)
            {
                WorkspaceUserAssignmentDto newUserAssignment = new WorkspaceUserAssignmentDto
                {
                    WorkspaceId = alert.WorkspaceId,
                    UserId = sessionModel.Id,
                    WorkspaceRole = WorkspaceUserRole.Contributor,
                };

                await CreateWorkspaceUserAssignmentInTransaction(sessionModel.Id, sessionModel.Email, newUserAssignment, unitOfWork);
            }

            await DeleteWorkspaceNotificationInTransaction(notification.NotificationId, sessionModel.Email, unitOfWork);

            await unitOfWork.CommitAsync();
        }

        private async Task CreateWorkspaceUserAssignmentInTransaction(Guid userId, string userEmail, WorkspaceUserAssignmentDto workspaceAssignmentDto, IUnitOfWork unitOfWork)
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

        private async Task<WorkspaceNotificationDto> GetWorkspaceNotification(Guid workspaceNotificationId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@id", workspaceNotificationId } };

            IEnumerable<WorkspaceNotificationDto> workspaceNotifications = await unitOfWork.Connection.QueryAsync<WorkspaceNotificationDto>(@"
                SELECT
	                id as AlertId,
	                workspace_id as WorkspaceId,
                    email
                FROM
	                workspace_notifications
                WHERE
	                id = @id", dictionary);

            return workspaceNotifications.FirstOrDefault();
        }

        private async Task DeleteWorkspaceNotificationInTransaction(Guid workspaceNotificationId, string userEmail, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand deleteWorkspaceNotification = new NpgsqlCommand("DELETE FROM workspace_notifications WHERE id = @id", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@id", workspaceNotificationId),
                    }
                };

                await deleteWorkspaceNotification.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to Delete workspace notifications with id: {workspaceNotificationId} for user with email {userEmail} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);

                await unitOfWork.RollbackAsync();
            }
        }
    }
}
