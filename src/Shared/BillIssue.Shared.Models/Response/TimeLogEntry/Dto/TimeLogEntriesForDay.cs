namespace BillIssue.Shared.Models.Response.TimeLogEntry.Dto
{
    public class TimeLogEntriesForDay
    {
        public DateTime Day {  get; set; }
        public int SecondsLogged { get; set; }
        public List<TimeLogEntryDto> TimeLogEntries { get; set; }
    }
}
