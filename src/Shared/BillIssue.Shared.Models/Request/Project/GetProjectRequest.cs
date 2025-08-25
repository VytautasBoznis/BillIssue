using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Project
{
    public class GetProjectRequest: BaseRequest
    {
        public Guid ProjectId { get; set; }
        public bool LoadUserAssignments { get; set; }
    }
}
