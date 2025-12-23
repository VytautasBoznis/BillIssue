using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Workspace.Dto;

namespace BillIssue.Shared.Models.Response.Workspace
{
    public class GetWorkspaceSelectionsForUserResponse : BaseResponse
    {
        public List<WorkspaceSelectionDto> WorkspaceSelections { get; set; }
    }
}
