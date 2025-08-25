using BillIssue.Shared.Models.Enums.TimeLog;
using BillIssue.Shared.Models.Request.Base;
using System.Data;

namespace BillIssue.Shared.Models.Request.TimeLogEntry
{
    public class SearchTimeLogEntriesRequest: BaseRequest
    {
        public Guid WorkspaceId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? ProjectWorktypeId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CustomTimeInterval { get; set; } = false;
        public TimeLogSearchInterval TimeLogSearchInterval { get; set; } = TimeLogSearchInterval.Day;
        public string SearchContent { get; set; }
    }
}
