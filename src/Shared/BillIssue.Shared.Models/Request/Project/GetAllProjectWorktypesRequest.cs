using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class GetAllProjectWorktypesRequest: AuthenticatedRequest
    {
        public Guid ProjectId { get; set; }
    }
}
