using BillIssue.Shared.Models.Request.Base;
using BillIssue.Shared.Models.Response.Notifications.Dto;

namespace BillIssue.Shared.Models.Request.Notifications
{
    public class DoNotificationDecisionRequest: BaseRequest
    {
        public NotificationDto Notification { get; set; }
        public bool Decision {  get; set; }
    }
}
