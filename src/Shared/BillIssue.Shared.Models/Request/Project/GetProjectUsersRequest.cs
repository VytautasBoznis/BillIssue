using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class GetProjectUsersRequest: BaseRequest
    {
        public Guid ProjectId { get; set; }
    }
}
