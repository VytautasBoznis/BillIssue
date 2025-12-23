using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Project
{
    public class GetProjectsInWorkspaceForUserResponse : BaseResponse
    {
        public List<ProjectSearchDto> ProjectSearchDtos { get; set; }
    }
}
