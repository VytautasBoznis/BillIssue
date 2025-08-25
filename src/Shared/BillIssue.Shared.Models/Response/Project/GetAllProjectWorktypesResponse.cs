using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Project
{
    public class GetAllProjectWorktypesResponse: BaseResponse
    {
        public List<ProjectWorktypeDto> ProjectWorktypeDtos { get; set; }
    }
}
