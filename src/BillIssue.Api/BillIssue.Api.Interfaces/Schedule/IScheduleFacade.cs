using BillIssue.Shared.Models.Request.Schedule;
using BillIssue.Shared.Models.Response.Schedule.Dto;

namespace BillIssue.Api.Interfaces.Schedule
{
    public interface IScheduleFacade
    {
        UserScheduleDto GetUserSchedule(string sessionid, GetUserScheduleRequest getUserSchedule);
    }
}
