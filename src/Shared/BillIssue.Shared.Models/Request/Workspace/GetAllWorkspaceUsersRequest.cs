using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class GetAllWorkspaceUsersRequest: BaseRequest
    {
        public Guid WorkspaceId { get; set; }
    }
}
