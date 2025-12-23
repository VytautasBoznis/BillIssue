using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class GetWorkspaceSelectionsForUserRequest: AuthenticatedRequest
    {
        public Guid UserId { get; set; }
    }
}
