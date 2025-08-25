using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.TimeLogEntry
{
    public class RemoveTimeLogEntryRequest: BaseRequest
    {
        public Guid TimeLogEntryId { get; set; }
    }
}
