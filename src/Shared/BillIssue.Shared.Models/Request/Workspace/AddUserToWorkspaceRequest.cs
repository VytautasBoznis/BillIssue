using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class AddUserToWorkspaceRequest: BaseRequest
    {
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public WorkspaceUserRole Role { get; set; }
    }
}
