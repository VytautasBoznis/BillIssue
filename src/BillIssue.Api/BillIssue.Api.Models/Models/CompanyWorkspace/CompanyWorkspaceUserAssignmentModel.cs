using BillIssue.Api.Models.Models.Base;
using BillIssue.Shared.Models.Enums.Workspace;

namespace BillIssue.Api.Models.Models.Workspace
{
    public class WorkspaceUserAssignmentModel: BaseModel
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public WorkspaceUserRole WorkspaceRole { get; set; }
    }
}
