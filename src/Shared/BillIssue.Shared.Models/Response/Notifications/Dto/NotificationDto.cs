using BillIssue.Shared.Models.Enums.Notification;

namespace BillIssue.Shared.Models.Response.Notifications.Dto
{
    public class NotificationDto
    {
        public Guid NotificationId { get; set; }
        public NotificationType NotificationType { get; set; }
        public string NotificationText { get; set; }
    }
}
