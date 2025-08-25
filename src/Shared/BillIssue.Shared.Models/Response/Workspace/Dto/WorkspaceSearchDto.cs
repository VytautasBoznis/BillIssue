using BillIssue.Shared.Models.Enums.Workspace;

namespace BillIssue.Shared.Models.Response.Workspace.Dto
{
    public class WorkspaceSearchDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public WorkspaceUserRole UserRole { get; set; }
        public bool IsDeleted { get; set; }
    }
}
