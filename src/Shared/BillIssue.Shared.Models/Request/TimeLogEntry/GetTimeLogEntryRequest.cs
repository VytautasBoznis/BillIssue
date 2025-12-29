using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.TimeLogEntry
{
    public class GetTimeLogEntryRequest: AuthenticatedRequest
    {
        public Guid TimeLogEntryId { get; set; }
    }
}
