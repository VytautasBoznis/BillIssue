using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Schedule.Dto;

namespace BillIssue.Shared.Models.Response.Schedule
{
    public class GetUserScheduleResponse: BaseResponse
    {
        public UserScheduleDto UserScheduleDto { get; set; }
    }
}
