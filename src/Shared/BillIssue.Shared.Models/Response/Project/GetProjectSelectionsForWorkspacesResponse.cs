using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Project
{
    public class GetProjectSelectionsForWorkspacesResponse: BaseResponse
    {
        public List<ProjectSelectionDto> ProjectSelectionDtos { get; set; }
    }
}
