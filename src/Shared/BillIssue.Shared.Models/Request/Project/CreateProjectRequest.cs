using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class CreateProjectRequest: AuthenticatedRequest
    {
        public Guid WorkspaceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CompleteTransactions { get; set; } = true;
    }
}
