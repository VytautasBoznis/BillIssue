using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Workspace.Dto;

namespace BillIssue.Shared.Models.Response.Workspace
{
    public class GetAllWorkspaceUsersResponse: BaseResponse
    {
        public List<WorkspaceUserDto> WorkspaceUserDtos {  get; set; }
    }
}
