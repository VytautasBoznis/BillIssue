using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class AddUserToWorkspaceRequest: AuthenticatedRequest
    {
        public Guid WorkspaceId { get; set; }
        public string NewUserEmail { get; set; }
    }
}
