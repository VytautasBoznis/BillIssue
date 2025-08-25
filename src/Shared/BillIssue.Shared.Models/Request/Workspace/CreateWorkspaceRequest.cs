using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Workspace
{
    public class CreateWorkspaceRequest: BaseRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
