using BillIssue.Shared.Models.Response.Workspace.Dto;

namespace BillIssue.Shared.Models.Response.Workspace
{
    public class GetWorkspaceSelectionsForUserResponse
    {
        public List<WorkspaceSelectionDto> WorkspaceSelections { get; set; }
    }
}
