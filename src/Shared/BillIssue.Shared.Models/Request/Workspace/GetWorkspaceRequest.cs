using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class GetWorkspaceRequest: BaseRequest
    {
        public Guid WorkspaceId { get; set; }
    }
}
