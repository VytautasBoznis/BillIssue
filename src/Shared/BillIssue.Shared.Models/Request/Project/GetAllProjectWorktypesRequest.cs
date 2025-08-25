using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class GetAllProjectWorktypesRequest: BaseRequest
    {
        public Guid ProjectId { get; set; }
    }
}
