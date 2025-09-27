using BillIssue.Api.Interfaces.Alerts;
using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Notification;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Errors;
using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Response.Notifications.Dto;
using BillIssue.Shared.Models.Response.Project.Dto;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Alerts
{
    public class NotificationFacade : INotificationFacade
    {
        private readonly NpgsqlConnection _dbConnection;
        private readonly ILogger<NotificationFacade> _logger;

        private readonly ISessionFacade _sessionFacade;

        private readonly string WorkspaceNotificationTextTemplate = "You have been invited to a new workspace: \"{0}\", please click the notification for confirmation";

        public NotificationFacade(
            ISessionFacade sessionFacade,
            NpgsqlConnection dbConnection,
            ILogger<NotificationFacade> logger
        ) {
            _sessionFacade = sessionFacade;
            _dbConnection = dbConnection;
            _logger = logger;
        }

        #region Interface implementation

        public async Task CreateWorkspaceNotificationInTransaction(string sessionId, CreateWorkspaceNotificationRequest request, NpgsqlTransaction transaction)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);
            
            await CreateWorkspaceNotificationInTransaction(request.WorkspaceId, request.TargetUserEmail, sessionModel.Email, transaction);
        }

        public async Task DoNotificationDecision(string sessionId, DoNotificationDecisionRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            switch (request.Notification.NotificationType)
            {
                case NotificationType.WorkspaceNotification :
                { 
                    await HandleWorkspaceNotificationDecision(request.Notification, request.Decision, sessionModel);
                    break;
                }
                default: {
                    throw new UnknowNotificationTypeException("Unknow notification type", ExceptionCodes.NOTIFICATION_UNKNOWN_TYPE);
                }
            }
        }

        public async Task<List<NotificationDto>> GetUserNotifications(string sessionId, GetNotificationRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            List<NotificationDto> allNotifications = new List<NotificationDto>();

            //Just do this for any other types of alerts/notifications
            List<NotificationDto> workspaceNotifications = GetWorkspaceNotificationAsNotifications(sessionModel.Email);

            //Compile them to one type of notification list
            allNotifications.AddRange(workspaceNotifications);

            return allNotifications;
        }

        public List<NotificationDto> GetWorkspaceNotificationAsNotifications(string userEmail)
        {
            List<WorkspaceNotificationDto> workspaceNotifications = GetAllWorkspaceNotificationsForEmail(userEmail);
            return workspaceNotifications.Select(workspaceNotification => new NotificationDto
            {
                NotificationId = workspaceNotification.AlertId,
                NotificationType = NotificationType.WorkspaceNotification,
                NotificationText = string.Format(WorkspaceNotificationTextTemplate, workspaceNotification.WorkspaceName)
            }).ToList();
        }

        #endregion

        #region Private methods

        private async Task HandleWorkspaceNotificationDecision(NotificationDto notification, bool decision, SessionModel sessionModel)
        {
            WorkspaceNotificationDto alert = GetWorkspaceNotification(notification.NotificationId);

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

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            if (decision)
            {
                WorkspaceUserAssignmentDto newUserAssignment = new WorkspaceUserAssignmentDto
                {
                    WorkspaceId = alert.WorkspaceId,
                    UserId = sessionModel.Id,
                    WorkspaceRole = WorkspaceUserRole.Contributor,
                };

                await CreateWorkspaceUserAssignmentInTransaction(sessionModel.Id, sessionModel.Email, newUserAssignment, transaction);
            }

            await DeleteWorkspaceNotificationInTransaction(notification.NotificationId, sessionModel.Email, transaction);

            transaction.Commit();
        }

        private async Task CreateWorkspaceNotificationInTransaction(Guid workspaceId, string targetUserEmail, string userEmail, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand insertWorkspaceNotification = new NpgsqlCommand("INSERT INTO workspace_notifications (workspace_id, email, created_by) VALUES (@workspaceId, @email, @createdBy)", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@workspaceId", workspaceId),
                        new("@email", targetUserEmail),
                        new("@createdBy", userEmail),
                    }
                };

                await insertWorkspaceNotification.ExecuteNonQueryAsync();

                _logger.LogInformation($"New workspace invite created for workspace id: {workspaceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    New workspace alert for user invitation failed to create in workspace id {workspaceId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
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

        private WorkspaceNotificationDto GetWorkspaceNotification(Guid workspaceNotificationId)
        {
            var dictionary = new Dictionary<string, object> { { "@id", workspaceNotificationId } };

            WorkspaceNotificationDto workspaceNotification = _dbConnection.Query<WorkspaceNotificationDto>(@"
                SELECT
	                id as AlertId,
	                workspace_id as WorkspaceId,
                    email
                FROM
	                workspace_notifications
                WHERE
	                id = @id", dictionary).FirstOrDefault();

            return workspaceNotification;
        }

        private async Task DeleteWorkspaceNotificationInTransaction(Guid workspaceNotificationId, string userEmail, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand deleteWorkspaceNotification = new NpgsqlCommand("DELETE FROM workspace_notifications WHERE id = @id", _dbConnection, transaction)
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
                transaction.Rollback();
            }
        }

        private List<WorkspaceNotificationDto> GetAllWorkspaceNotificationsForEmail(string email)
        {
            var dictionary = new Dictionary<string, object> { { "@email", email } };

            List<WorkspaceNotificationDto> workspaceNotificationDtos = _dbConnection.Query<WorkspaceNotificationDto>(@"
                SELECT
	                wa.id as AlertId,
	                wa.workspace_id as WorkspaceId,
	                ww.name as WorkspaceName,
	                wa.email as Email
                FROM
	                workspace_notifications wa
	                JOIN workspace_workspaces ww
		                ON ww.id = wa.workspace_id 
                WHERE
	                email = @email", dictionary).ToList();

            return workspaceNotificationDtos;
        }

        #endregion
    }
}
