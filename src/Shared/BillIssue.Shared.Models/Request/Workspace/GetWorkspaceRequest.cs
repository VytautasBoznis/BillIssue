using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class GetWorkspaceRequest: AuthenticatedRequest
    {
        public Guid WorkspaceId { get; set; }
        public bool LoadWorkspaceUsers { get; set; }
    }
}
