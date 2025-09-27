using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Response.Notifications.Dto;
using Npgsql;

namespace BillIssue.Api.Interfaces.Alerts
{
    public interface INotificationFacade
    {
        Task CreateWorkspaceNotificationInTransaction(string sessionId, CreateWorkspaceNotificationRequest request, NpgsqlTransaction transaction);
        Task DoNotificationDecision(string sessionId, DoNotificationDecisionRequest request);
        Task<List<NotificationDto>> GetUserNotifications(string sessionId, GetNotificationRequest request);
        List<NotificationDto> GetWorkspaceNotificationAsNotifications(string userEmail);
    }
}
