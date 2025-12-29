using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.TimeLogEntry
{
    public class GetWeekOfTimeEntriesRequest : AuthenticatedRequest
    {
        public Guid WorkspaceId { get; set; }
        public DateTime TargetDay { get; set; }
    }
}
