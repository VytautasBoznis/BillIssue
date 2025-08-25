using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class RemoveProjectRequest: BaseRequest
    {
        public Guid ProjectId { get; set; }
    }
}
