using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Project
{
    public class CreateProjectResponse: BaseResponse
    {
        public ProjectDto ProjectDto { get; set; }
    }
}
