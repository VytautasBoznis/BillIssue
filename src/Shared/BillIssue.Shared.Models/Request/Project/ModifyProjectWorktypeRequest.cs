using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class ModifyProjectWorktypeRequest: AuthenticatedRequest
    {
        public Guid ProjectId { get; set; }
        public Guid ProjectWorktypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsBillable { get; set; }
    }
}
