using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Project
{
    public  class GetProjectWorktypeResponse : BaseResponse
    {
        public ProjectWorktypeDto ProjectWorktypeDto { get; set; }
    }
}
