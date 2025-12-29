using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Project
{
    public class GetProjectsForUserResponse : BaseResponse
    {
        public List<ProjectDto> ProjectDtos { get; set; }
    }
}
