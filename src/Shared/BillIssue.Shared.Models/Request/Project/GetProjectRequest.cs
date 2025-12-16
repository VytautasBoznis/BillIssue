using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class GetProjectRequest: AuthenticatedRequest
    {
        public Guid ProjectId { get; set; }
        public bool LoadUserAssignments { get; set; }
    }
}
