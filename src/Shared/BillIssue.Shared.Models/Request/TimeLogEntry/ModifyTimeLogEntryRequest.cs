using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.TimeLogEntry
{
    public class ModifyTimeLogEntryRequest: BaseRequest
    {
        public Guid TimeLogEntryId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ProjectWorktypeId { get; set; }
        public string Title { get; set; }
        public DateTime? LogDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? HourAmount { get; set; }
        public int? MinuteAmount { get; set; }
        public int? SecondsTotalAmount { get { return HourAmount * 3600 + MinuteAmount * 60; } }
        public string WorkDescription { get; set; } = "";
    }
}
