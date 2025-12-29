using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class ModifyUserAssingmentInProjectRequest: AuthenticatedRequest
    {
        public Guid ProjectId { get; set; }
        public Guid ProjectUserAssignmentId { get; set; }
        public ProjectUserRoles Role { get; set; }
    }
}
