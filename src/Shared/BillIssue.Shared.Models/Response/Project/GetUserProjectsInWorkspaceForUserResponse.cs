using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Project
{
    public class GetUserProjectsInWorkspaceForUserResponse
    {
        public List<ProjectSearchDto> ProjectSearchDtos { get; set; }
    }
}
