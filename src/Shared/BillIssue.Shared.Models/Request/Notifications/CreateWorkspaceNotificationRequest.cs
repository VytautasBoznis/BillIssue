using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Notifications
{
    public class CreateWorkspaceNotificationRequest: BaseRequest
    {
        public Guid WorkspaceId { get; set; }
        public string TargetUserEmail { get; set; }
    }
}
