using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Enums.Project;

namespace BillIssue.Shared.Models.Response.TimeLogEntry.Dto
{
    public class TimeLogEntryRoleContextDto
    {
        public Guid TimeLogEntryId { get; set; }
        public string ProjectName { get; set; }
        public ProjectUserRoles ProjectUserRole { get; set; }
        public string WorkspaceName { get; set; }
        public WorkspaceUserRole WorkspaceUserRole { get; set; }
    }
}
