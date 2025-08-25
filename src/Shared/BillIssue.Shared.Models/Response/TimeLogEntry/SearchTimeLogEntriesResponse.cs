using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;

namespace BillIssue.Shared.Models.Response.TimeLogEntry
{
    public class SearchTimeLogEntriesResponse: BaseResponse
    {
        public List<TimeLogEntryDto> TimeLogEntryDtos { get; set; }
    }
}
