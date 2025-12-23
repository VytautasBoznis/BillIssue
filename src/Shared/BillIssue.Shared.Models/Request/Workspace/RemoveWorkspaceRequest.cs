using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class RemoveWorkspaceRequest : AuthenticatedRequest
    {
        public Guid WorkspaceId { get; set; }
    }
}