using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;

namespace BillIssue.Shared.Models.Response.TimeLogEntry
{
    public class GetWeekOfTimeEntriesResponse : BaseResponse
    {
        public List<TimeLogEntriesForDay> TimeLogEntriesForWeek { get; set; }
    }
}
