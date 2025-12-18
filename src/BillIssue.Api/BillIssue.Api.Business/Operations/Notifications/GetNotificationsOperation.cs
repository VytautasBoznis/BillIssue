using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Shared.Models.Enums.Notification;
using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Response.Notifications;
using BillIssue.Shared.Models.Response.Notifications.Dto;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Notifications
{
    public class GetNotificationsOperation : BaseOperation<GetNotificationsRequest, GetNotificationsResponse>
    {
        private const string WorkspaceNotificationTextTemplate = "You have been invited to a new workspace: \"{0}\", please click the notification for confirmation";

        public GetNotificationsOperation(
            ILogger<GetNotificationsOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetNotificationsRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<GetNotificationsResponse> Execute(GetNotificationsRequest request, IUnitOfWork unitOfWork)
        {
            List<NotificationDto> allNotifications = [];

            //Just do this for any other types of alerts/notifications
            List<NotificationDto> workspaceNotifications = await GetWorkspaceNotificationAsNotifications(request.SessionUserData.Email, unitOfWork);

            //Compile them to one type of notification list
            allNotifications.AddRange(workspaceNotifications);

            return new GetNotificationsResponse
            {
                NotificationDtos = allNotifications
            };
        }

        private async Task<List<NotificationDto>> GetWorkspaceNotificationAsNotifications(string userEmail, IUnitOfWork unitOfWork)
        {
            List<WorkspaceNotificationDto> workspaceNotifications = await GetAllWorkspaceNotificationsForEmail(userEmail, unitOfWork);
            return workspaceNotifications.Select(workspaceNotification => new NotificationDto
            {
                NotificationId = workspaceNotification.AlertId,
                NotificationType = NotificationType.WorkspaceNotification,
                NotificationText = string.Format(WorkspaceNotificationTextTemplate, workspaceNotification.WorkspaceName)
            }).ToList();
        }

        private async Task<List<WorkspaceNotificationDto>> GetAllWorkspaceNotificationsForEmail(string email, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@email", email } };

            IEnumerable<WorkspaceNotificationDto> workspaceNotificationDtos = await unitOfWork.Connection.QueryAsync<WorkspaceNotificationDto>(@"
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
	                email = @email", dictionary);

            return workspaceNotificationDtos.ToList();
        }
    }
}
