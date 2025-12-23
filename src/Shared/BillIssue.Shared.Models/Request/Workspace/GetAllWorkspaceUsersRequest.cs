using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class GetAllWorkspaceUsersRequest: AuthenticatedRequest
    {
        public Guid WorkspaceId { get; set; }
    }
}
