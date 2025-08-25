using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Project
{
    public class GetProjectUsersResponse: BaseResponse
    {
        public List<ProjectUserAssignmentDto> ProjectUserAssignmentDtos { get; set; }
    }
}
