using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Project
{
    public class GetProjectsForUserResponse
    {
        public List<ProjectDto> ProjectDtos { get; set; }
    }
}
