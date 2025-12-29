using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class RemoveUserAssingmentFromProjectRequest: AuthenticatedRequest
    {
        public Guid ProjectId { get; set; }
        public Guid ProjectUserAssignmentId { get; set; }
    }
}
