using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Notifications
{
    public class CreateWorkspaceNotificationRequest: AuthenticatedRequest
    {
        public Guid WorkspaceId { get; set; }
        public string TargetUserEmail { get; set; }
    }
}
