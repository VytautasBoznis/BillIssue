using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class GetProjectWorktypeRequest : AuthenticatedRequest
    {
        public Guid ProjectWorktypeId { get; set; }
    }
}
