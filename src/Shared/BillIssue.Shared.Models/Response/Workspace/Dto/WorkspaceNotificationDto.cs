namespace BillIssue.Shared.Models.Response.Workspace.Dto
{
    public class WorkspaceNotificationDto
    {
        public Guid AlertId { get; set; }
        public Guid WorkspaceId { get; set; }
        public string WorkspaceName { get; set; }
        public string Email { get; set; }
    }
}
