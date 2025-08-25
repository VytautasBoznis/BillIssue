using BillIssue.Shared.Models.Enums.Project;

namespace BillIssue.Shared.Models.Response.Project.Dto
{
    public class ProjectUserAssignmentDto
    {
        public Guid UserAssignmentId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ProjectUserRoles Role { get; set; }
    }
}
