using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Workspace.Dto;

namespace BillIssue.Shared.Models.Response.Workspace
{
    public class GetWorkspaceResponse: BaseResponse
    {
        public WorkspaceDto WorkspaceDto { get; set; }
    }
}
