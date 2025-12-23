using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class RemoveProjectRequest: AuthenticatedRequest
    {
        public Guid ProjectId { get; set; }
    }
}
