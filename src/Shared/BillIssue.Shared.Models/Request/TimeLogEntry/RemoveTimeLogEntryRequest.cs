using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.TimeLogEntry
{
    public class RemoveTimeLogEntryRequest: AuthenticatedRequest
    {
        public Guid TimeLogEntryId { get; set; }
    }
}
