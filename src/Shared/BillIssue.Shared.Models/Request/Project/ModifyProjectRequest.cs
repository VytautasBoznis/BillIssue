using BillIssue.Shared.Models.Request.Base;
using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Request.Project
{
    public class ModifyProjectRequest: BaseRequest
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
