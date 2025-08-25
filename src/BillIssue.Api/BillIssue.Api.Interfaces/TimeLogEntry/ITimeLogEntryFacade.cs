using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;

namespace BillIssue.Api.Interfaces.TimeLogEntry
{
    public interface ITimeLogEntryFacade
    {
        public Task<TimeLogEntryDto> CreateTimeLogEntry(string sessionId, CreateTimeLogEntryRequest request);
        public Task<TimeLogEntryDto> GetTimeLogEntry(string sessionId, GetTimeLogEntryRequest request);
        public Task RemoveTimeLogEntry(string sessionId, RemoveTimeLogEntryRequest request);
        public Task<TimeLogEntryDto> ModifyTimeLogEntry(string sessionId, ModifyTimeLogEntryRequest request);
    }
}
