using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class RemoveProjectWorktypeRequest: AuthenticatedRequest
    {
        public Guid ProjectId { get; set; }
        public Guid ProjectWorktypeId { get; set; }
    }
}
