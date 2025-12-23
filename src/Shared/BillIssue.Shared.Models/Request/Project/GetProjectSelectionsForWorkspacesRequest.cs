using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class GetProjectSelectionsForWorkspacesRequest: AuthenticatedRequest
    {
        public List<Guid> WorkspaceIds { get; set; }
    }
}
