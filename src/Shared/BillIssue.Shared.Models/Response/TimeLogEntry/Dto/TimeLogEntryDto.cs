namespace BillIssue.Shared.Models.Response.TimeLogEntry.Dto
{
    public class TimeLogEntryDto
    {
        public Guid TimeLogEntryId { get; set; }
        public Guid WorkspaceId { get; set; }
        public string WorkspaceName { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public Guid ProjectWorktypeId { get; set; }
        public string ProjectWorktypeName { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? LogDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? HourAmount { get; set; }
        public int? MinuteAmount { get; set; }
        public int? SecondsTotalAmount { get; set; }
        public string WorkDescription { get; set; } = string.Empty;
    }
}
