using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class GetProjectsInWorkspaceForUserRequest : AuthenticatedRequest
    {
        public Guid WorkspaceId { get; set; }
    }
}
