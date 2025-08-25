using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class RemoveUserFromWorkspaceRequest: BaseRequest
    {
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
    }
}
