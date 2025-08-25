using BillIssue.Shared.Models.Enums.Workspace;

namespace BillIssue.Shared.Models.Response.Workspace.Dto
{
    public class WorkspaceUserAssignmentDto
    {
        public Guid WorkspaceUserAssignmentId { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public WorkspaceUserRole WorkspaceRole { get; set; }
    }
}
