using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class ModifyUserInWorkspaceRequest: AuthenticatedRequest
    {
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public WorkspaceUserRole NewUserRole { get; set; }
    }
}
