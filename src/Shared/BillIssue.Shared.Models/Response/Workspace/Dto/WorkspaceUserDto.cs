using BillIssue.Shared.Models.Enums.Workspace;

namespace BillIssue.Shared.Models.Response.Workspace.Dto
{
    public class WorkspaceUserDto
    {
        public Guid AssignmentId { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public WorkspaceUserRole Role { get; set; }
    }
}
