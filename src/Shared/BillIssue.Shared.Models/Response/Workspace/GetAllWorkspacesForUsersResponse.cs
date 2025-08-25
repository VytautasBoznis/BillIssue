using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Workspace.Dto;

namespace BillIssue.Shared.Models.Response.Workspace
{
    public class GetAllWorkspacesForUsersResponse: BaseResponse
    {
        public List<WorkspaceSearchDto> WorkspaceDtos { get; set; }
    }
}
