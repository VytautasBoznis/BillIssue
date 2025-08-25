using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;

namespace BillIssue.Shared.Models.Response.TimeLogEntry
{
    public class ModifyTimeLogEntryResponse: BaseResponse
    {
        public TimeLogEntryDto TimeLogEntryDto { get; set; }
    }
}
