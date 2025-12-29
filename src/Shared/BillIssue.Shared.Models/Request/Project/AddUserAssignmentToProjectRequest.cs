using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class AddUserAssignmentToProjectRequest: AuthenticatedRequest
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public ProjectUserRoles Role { get; set; }
    }
}
