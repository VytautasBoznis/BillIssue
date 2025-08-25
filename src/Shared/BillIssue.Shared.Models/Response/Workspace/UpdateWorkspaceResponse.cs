using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Workspace.Dto;

namespace BillIssue.Shared.Models.Response.Workspace
{
    public class UpdateWorkspaceResponse: BaseResponse
    {
        public WorkspaceDto WorkspaceDto { get; set; }
    }
}
