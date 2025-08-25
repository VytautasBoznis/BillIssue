using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;

namespace BillIssue.Api.Interfaces.TimeLogEntry
{
    public interface ITimeLogEntrySearchFacade
    {
        public Task<List<TimeLogEntryDto>> SearchTimeLogEntries(string sessionId, SearchTimeLogEntriesRequest request);
        public Task<List<TimeLogEntriesForDay>> GetWeekOfTimeEntries(string sessionId, GetWeekOfTimeEntriesRequest request);
    }
}
